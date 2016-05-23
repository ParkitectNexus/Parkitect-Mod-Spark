using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System;
using System.Linq;
using MiniJSON;
[Serializable]
public class Objects
{
    [SerializeField]
    public List<string> GONames = new List<string>();
}
public static class Exporter {

    static string ModName;
    public static void ExportMod(ParkitectModManager ModManager)
    {
        string error = "";
        //Check for unique GO names
        List<string> GONames = new List<string>();
        for (int i = 0; i < ModManager.ParkitectObjects.Count; i++) // Loop with for.
        {
            if (!GONames.Contains(ModManager.ParkitectObjects[i].gameObject.name))
                GONames.Add(ModManager.ParkitectObjects[i].gameObject.name);
            else
                error = "You have two or more GameObjects that are using the name: '" + ModManager.ParkitectObjects[i].gameObject.name +  "'. Make sure to change the GameObjects name and not the in game name! This causes problems when loading from the Assetbundle and from savegames";

        }


        //Check all GO if they are in the asset bundle
        List<GameObject> AllNoneGO = new List<GameObject>();
        foreach(ParkitectObject PO in ModManager.ParkitectObjects)
        {
            if (PO.gameObject != null && PO.type == ParkitectObject.ObjType.none)
                AllNoneGO.Add(PO.gameObject);
        }

        foreach (ParkitectObject PO in ModManager.ParkitectObjects)
        {


            //Car Type
            if (PO.frontCar != null && PO.type == ParkitectObject.ObjType.CoasterCar && !AllNoneGO.Contains(PO.frontCar))
                error = "Coaster Car Type '" + PO.inGameName + "' does have a front car that isn't in the mod as none object. Add it to the mod and make sure it is set to none. Otherwise the Front Car won't be in the bundle with all the objects.";
            else if (PO.gameObject != null && PO.type == ParkitectObject.ObjType.Shop && PO.shop != null)
                foreach (Product P in PO.shop.products)
                    if (P.GO == null)
                        error = "The Product '" + P.Name + "' from shop '" + PO.inGameName + "' doesn't have a GameObject as a model.";
                    else if (!AllNoneGO.Contains(P.GO))
                        error = "The Product '" + P.Name + "' from shop '" + PO.inGameName + "' does have a GameObject that isn't in the mod with the type None. Add it to the mod and make sure the Type is set to None. Otherwise the model of the product won't be in the bundle with all the objects.";
            else if (PO.type == ParkitectObject.ObjType.PathStyle && (!PO.gameObject.GetComponent< MeshRenderer >() || !PO.gameObject.GetComponent<MeshRenderer>().material))
                error = "The Path Style '" + PO.inGameName + "' has not been setup correctly, make sure that you are creating the path by clicking the Create button after you asigned a texture. ";
        }

        //Error logging
        if (error != "")
        {
            EditorUtility.DisplayDialog("Export failed", error, "Ok");
            return;
        }
        //Setup project path
        string path = "Assets/Mods/" + ModManager.mod.name;
        if (AssetDatabase.IsValidFolder(path))
        {
           
            AssetDatabase.DeleteAsset(path);
        }
        else
        {
            if (!AssetDatabase.IsValidFolder("Assets/Mods"))
            {
                AssetDatabase.CreateFolder("Assets", "Mods");
            }
        }
        AssetDatabase.CreateFolder("Assets/Mods", ModManager.mod.name);

        //Get modname for identifier
        ModName = ModManager.mod.name + "@";

        //Save XML
        SaveToXML(path + "/" + ModManager.mod.name + ".xml", ModManager);

        //Save json files
        SaveObjectsJson(ModManager);
        SaveModJson(ModManager);

        //Setup prefabs, create asset bundle, delete prefabs
        List<UnityEngine.Object> selection = new List<UnityEngine.Object>();
        AssetBundleBuild AB = new AssetBundleBuild();
        AB.assetBundleName = "mod";
        for (int i = 0; i < ModManager.ParkitectObjects.Count; i++) // Loop with for.
        {
            EditorUtility.DisplayProgressBar("Exporiting Prefabs", "Creating Prefabs", ((float)i + 1f) / (float)ModManager.ParkitectObjects.Count);
            selection.Add(PrefabUtility.CreatePrefab(path + "/"+ ModManager.mod.name + "@"+ ModManager.ParkitectObjects[i].gameObject.name + ".prefab", ModManager.ParkitectObjects[i].gameObject));
        }
        EditorUtility.ClearProgressBar();
        List<string> Paths = new List<string>();
        for (int i = 0; i < selection.Count; i++) // Loop with for.
        {
            EditorUtility.DisplayProgressBar("Saving paths", "converting objects to paths", ((float)i + 1f) / (float)selection.Count);
            Paths.Add(AssetDatabase.GetAssetPath(selection[i]));
        }
        EditorUtility.ClearProgressBar();
        AB.assetNames = Paths.ToArray();
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        builds.Add(AB);
        AssetDatabase.CreateFolder(path, "assetbundle");
        BuildPipeline.BuildAssetBundles(path + "/assetbundle", builds.ToArray());
        AssetDatabase.LoadMainAssetAtPath(path);
        Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        EditorGUIUtility.PingObject(Selection.activeObject);
        for (int i = 0; i < Paths.Count; i++) // Loop with for.
        {

            EditorUtility.DisplayProgressBar("Deleting Prefabs", "Deleting created prefabs", 1 - (((float)i + 1f) / (float)Paths.Count));
            AssetDatabase.DeleteAsset(Paths[i]);
        }
        AssetDatabase.DeleteAsset(path + "/assetbundle/assetbundle");
        AssetDatabase.DeleteAsset(path + "/assetbundle/assetbundle.manifest");

        //Check for copying to documents
        if (EditorUtility.DisplayDialog("Copy to documents? ",
            "Do you want the mod also in Documents/Parkitect/pnmods", "Yes", "No thanks"))
        {
            string targetDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/Parkitect/pnmods/" + ModManager.mod.name;

            ZipUtil.Unzip(Application.dataPath + "/Parkitect_ModSpark/ModTemplate.zip", targetDir);
            string sourceDir = Application.dataPath + "/Mods/" + ModManager.mod.name;

            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*",
    SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourceDir, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourceDir, targetDir), true);
            foreach (var file in System.IO.Directory.GetFiles(targetDir, "*.meta"))
                File.Delete(file);
            foreach (string dirPath in Directory.GetDirectories(targetDir, "*", SearchOption.AllDirectories))
                foreach (var file in System.IO.Directory.GetFiles(dirPath, "*.meta"))
                    File.Delete(file);
        }


        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
    private static void SaveModJson(ParkitectModManager ModManager)
    {
        string Content = "{\n\"BaseDir\":\"\",\n\"CompilerVersion\":\"v3.5\",\n\"IsDevelopment\":true,\n\"IsEnabled\":true,\n\"Name\":\"" + ModManager.mod.name + "\",\n\"Project\":\"ModSparkLoader.csproj\",\n\"Dependencies\":[\"ParkitectNexus/Depender\"]\n}";
        File.WriteAllText(Application.dataPath + "/Mods/" + ModManager.mod.name + "/mod.json", Content);
    }
    private static void SaveObjectsJson(ParkitectModManager ModManager)
    {
        Objects OBJS = new Objects();
        for (int i = 0; i < ModManager.ParkitectObjects.Count; i++) // Loop with for.
        {
            OBJS.GONames.Add(ModManager.mod.name + "@" + ModManager.ParkitectObjects[i].gameObject.name);
        }
        File.WriteAllText(Application.dataPath + "/Mods/" + ModManager.mod.name + "/objects.json", "{ " + Environment.NewLine + "objects : " +  Json.Serialize(OBJS.GONames) + Environment.NewLine + " }");
    }
    // =========[ SaveToXML ]=========================================================
    public static void SaveToXML(string path, ParkitectModManager ModManager)
    {

        try
        {

            var xEle =

                new XElement("Mod",
                        new XElement("ModName", ModManager.mod.name),
                        new XElement("ModDiscription", ModManager.mod.discription),
                        new XElement("Objects",
                        from obj in ModManager.ParkitectObjects
                        select new XElement("Object", Basic(obj))
                               )
                    );
            //write string to file

            if (File.Exists(path))
                File.Delete(path);

            xEle.Save(path);
            UnityEngine.Debug.Log("Converted to XML in : " + path);
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(ex.Message);
            EditorUtility.DisplayDialog("Couldn't create XML File", "The XML file Couldn't be created because " + ex.Message, "Ok");
        }

        ModManager.asset = new ParkitectObject();
    }
    static object[] Basic(ParkitectObject PO)
    {
        List<object> E = new List<object>();
        E.Add(new XElement("OBJName", ModName + PO.gameObject.name));
        E.Add(new XElement("Type", PO.type));
        E.Add(new XElement("inGameName", PO.inGameName));
        E.Add(new XElement("price", PO.price));
        switch (PO.type)
        {
            case ParkitectObject.ObjType.none:
                break;
            case ParkitectObject.ObjType._:
                break;
            case ParkitectObject.ObjType.deco:
                E.Add(new XElement("grid", PO.grid));
                E.Add(new XElement("snapCenter", PO.snapCenter));
                E.Add(new XElement("category", PO.category));
                E.Add(new XElement("heightDelta", PO.heightDelta));
                E.Add(new XElement("gridSubdivision", PO.gridSubdivision));
                break;
            case ParkitectObject.ObjType.trashbin:
                break;
            case ParkitectObject.ObjType.seating:
                break;
            case ParkitectObject.ObjType.lamp:
                break;
            case ParkitectObject.ObjType.fence:
                break;
            case ParkitectObject.ObjType.FlatRide:
                E.Add(new XElement("X", PO.XSize));
                E.Add(new XElement("Z", PO.ZSize));
                E.Add(new XElement("RestraintAngle", PO.closedAngleRetraints));
                E.Add(new XElement("Excitement", PO.Excitement / 100f));
                E.Add(new XElement("Intensity", PO.Intensity / 100f));
                E.Add(new XElement("Nausea", PO.Nausea / 100f));
                E.Add(AnimationToXML(PO));
                E.Add(new XElement("Waypoints",
                            from point in PO.waypoints
                            select new XElement("Waypoint",

                                           new XElement("isOuter", point.isOuter.ToString()),
                                           new XElement("isRabbitHoleGoal", point.isRabbitHoleGoal.ToString()),
                                           new XElement("connectedTo", string.Join(",", point.connectedTo.Select(x => x.ToString()).ToArray())),
                                           new XElement("localPosition", point.localPosition)
                                      )));
                break;
            case ParkitectObject.ObjType.Shop:
                E.Add(ShopToXML(PO));
                break;
            case ParkitectObject.ObjType.CoasterCar:
                E.Add(new XElement("CoasterName", PO.CoasterName));
                E.Add(new XElement("RestraintAngle", PO.closedAngleRetraints));

                if (PO.frontCar)
                    E.Add(new XElement("FrontCarGO", ModName + PO.frontCar.name));
                break;
            case ParkitectObject.ObjType.PathStyle:
                E.Add(new XElement("PathStyle", PO.pathType));
                break;
            default:
                break;
        }
        E.Add(new XElement("recolorable", PO.recolorable));
        E.Add(new XElement("shader", PO.Shader));
        if (PO.recolorable)
        {
            E.Add( new XElement("Color1", ColorToHex(PO.color1)));
            E.Add( new XElement("Color2", ColorToHex(PO.color2)));
            E.Add( new XElement("Color3", ColorToHex(PO.color3)));
            E.Add( new XElement("Color4", ColorToHex(PO.color4)));
        }
        E.Add( BoudingBoxToXML(PO));
        return E.ToArray();
    }

    public static void SaveToXML(ParkitectModManager ModManager)
    {
        SaveToXML(EditorUtility.SaveFilePanel(
                  "Save mod as XML",
                  Application.dataPath,
                  ModManager.mod.name + ".xml",
                  "xml"), ModManager);

    }
    private static object BoudingBoxToXML(ParkitectObject obj)
    {
        if (obj.BoundingBoxes.Count > 0)
        {
            return
               new XElement("BoudingBoxes",
               from BB in obj.BoundingBoxes
               select new XElement("BoudingBox",
               new XElement("min",BB.bounds.min),
               new XElement("max", BB.bounds.max)
               ));
        }
        else
        {
            return null;
        }
    }
    private static object ShopToXML(ParkitectObject obj)
    {
        if (obj.type == ParkitectObject.ObjType.Shop)
        {
            List<object> productsXML = new List<object>();
            foreach (ongoing P in obj.shop.products.OfType<ongoing>())
            {
                List<object> ingredientsXML = new List<object>();
                foreach (ingredient I in P.ingredients)
                {
                    List<object> effectsXML = new List<object>();
                    foreach (effect E in I.effects)
                    {
                        effectsXML.Add(new XElement("effect", new XElement("Type", E.Type), new XElement("Amount", E.amount)));
                    }
                    ingredientsXML.Add(new XElement("Ingredient",
                   new XElement("Name", I.Name),
                   new XElement("Price", I.price),
                   new XElement("Amount", I.amount),
                   new XElement("tweakable", I.tweakable),
                   new XElement("Effects", effectsXML)
                   ));

                }

                productsXML.Add(new XElement("Product",
                new XElement("Model", ModName + P.GO.name),
                new XElement("Price", P.price),
                new XElement("Type", P.GetType().Name),
                new XElement("Name", P.Name),
                new XElement("Hand", P.Hand),
                new XElement("Duration", P.duration),
                new XElement("Ingredients", ingredientsXML)));
            }
            foreach (consumable P in obj.shop.products.OfType<consumable>())
            {
                List<object> ingredientsXML = new List<object>();
                foreach (ingredient I in P.ingredients)
                {
                    List<object> effectsXML = new List<object>();
                    foreach (effect E in I.effects)
                    {
                        effectsXML.Add(new XElement("effect", new XElement("Type", E.Type), new XElement("Amount", E.amount)));
                    }
                    ingredientsXML.Add(new XElement("Ingredient",
                   new XElement("Name", I.Name),
                   new XElement("Price", I.price),
                   new XElement("Amount", I.amount),
                   new XElement("tweakable", I.tweakable),
                   new XElement("Effects", effectsXML)
                   ));

                }

                productsXML.Add(new XElement("Product",
                new XElement("Model", ModName + P.GO.name),
                new XElement("Price", P.price),
                new XElement("Type", P.GetType().Name),
                new XElement("Name", P.Name),
                new XElement("Hand", P.Hand),
                new XElement("ConsumeAnimation", P.ConsumeAnimation),
                new XElement("Temprature", P.Temprature),
                new XElement("Portions", P.portions),
                new XElement("Ingredients", ingredientsXML)));
            }
            foreach (wearable P in obj.shop.products.OfType<wearable>())
            {
                List<object> ingredientsXML = new List<object>();
                foreach (ingredient I in P.ingredients)
                {
                    List<object> effectsXML = new List<object>();
                    foreach (effect E in I.effects)
                    {
                        effectsXML.Add(new XElement("effect", new XElement("Type", E.Type), new XElement("Amount", E.amount)));
                    }
                    ingredientsXML.Add(new XElement("Ingredient",
                   new XElement("Name", I.Name),
                   new XElement("Price", I.price),
                   new XElement("Amount", I.amount),
                   new XElement("tweakable", I.tweakable),
                   new XElement("Effects", effectsXML)
                   ));

                }

                productsXML.Add(new XElement("Product",
                new XElement("Model", ModName + P.GO.name),
                new XElement("Price", P.price),
                new XElement("Type", P.GetType().Name),
                new XElement("Name", P.Name),
                new XElement("Hand", P.Hand),
                new XElement("BodyLocation", P.BodyLocation),
                new XElement("Ingredients", ingredientsXML)));
            }

            return new XElement("Shop", productsXML);
        }
        else
        {
            return null;
        }
    }
    private static object AnimationToXML(ParkitectObject obj)
    {
        return
            new XElement("Animation", 
            Motors(obj),
            PhasesToXML(obj)
            
            );
    }
    private static object PhasesToXML(ParkitectObject obj)
    {
        List<object> Phases = new List<object>();
        foreach (Phase R in obj.Animation.phases.OfType<Phase>().ToList())
        {
            Phases.Add( new XElement("phase",EventsToXml(R)));
                
                    
        }
        return new XElement("phases", Phases.ToArray());
    }
    private static object EventsToXml(Phase phase)
    {
        List<object> Events = new List<object>();

        foreach (Wait R in phase.Events.OfType<Wait>().ToList())
        {
            Events.Add(new XElement("Wait", 
                new XElement("Seconds", R.seconds)
                )
            );
        }
        foreach (StartRotator R in phase.Events.OfType<StartRotator>().ToList())
        {
            Events.Add(new XElement("StartRotator",
                new XElement("Identifier", R.rotator.Identifier)
                )
            );
        }
        foreach (SpinRotater R in phase.Events.OfType<SpinRotater>().ToList())
        {
            Events.Add(new XElement("SpinRotator",
                new XElement("Identifier", R.rotator.Identifier),
                new XElement("spin", R.spin),
                new XElement("spins", R.spins)
                )
            );
        }
        foreach (StopRotator R in phase.Events.OfType<StopRotator>().ToList())
        {
            Events.Add(new XElement("StopRotator",
                new XElement("Identifier", R.rotator.Identifier)
                )
            );
        }
        foreach (FromToRot R in phase.Events.OfType<FromToRot>().ToList())
        {
            Events.Add(new XElement("FromToRot",
                new XElement("Identifier", R.rotator.Identifier)
                )
            );
        }
        foreach (ToFromRot R in phase.Events.OfType<ToFromRot>().ToList())
        {
            Events.Add(new XElement("ToFromRot",
                new XElement("Identifier", R.rotator.Identifier)
                )
            );
        }
        foreach (FromToMove R in phase.Events.OfType<FromToMove>().ToList())
        {
            Events.Add(new XElement("FromToMove",
                new XElement("Identifier", R.rotator.Identifier)
                )
            );
        }
        foreach (ToFromMove R in phase.Events.OfType<ToFromMove>().ToList())
        {
            Events.Add(new XElement("ToFromMove",
                new XElement("Identifier", R.rotator.Identifier)
                )
            );
        }
        foreach (ApplyRotation R in phase.Events.OfType<ApplyRotation>().ToList())
        {
            Events.Add(new XElement("ApplyRotation",
                new XElement("Identifier", R.rotator.Identifier)
                )
            );
        }
        foreach (ChangePendulum R in phase.Events.OfType<ChangePendulum>().ToList())
        {
            Events.Add(new XElement("ChangePendulum",
                new XElement("Identifier", R.rotator.Identifier),
                new XElement("Friction", R.Friction),
                new XElement("Pendulum", R.Pendulum)
                )
            );
        }
        return new XElement("events", Events.ToArray());
    }
    private static object Motors(ParkitectObject obj)
    {
        List<object> motors = new List<object>();
        List<Rotator> PendulumRotators = new List<Rotator>();
        foreach (PendulumRotator R in obj.Animation.motors.OfType<PendulumRotator>().ToList())
        {
            PendulumRotators.Add(R);
            motors.Add(new XElement("PendulumRotator",
                new XElement("Identifier", R.Identifier),
                new XElement("axis", GetGameObjectPath(R.axis.gameObject).Substring(GetGameObjectPath(obj.gameObject).Length + 1)),
                new XElement("maxSpeed", R.maxSpeed),
                new XElement("accelerationSpeed", R.accelerationSpeed),
                new XElement("rotationAxis", R.rotationAxis),
                new XElement("gravity", R.gravity),
                new XElement("armLength", R.armLength),
                new XElement("angularFriction", R.angularFriction),
                new XElement("pendulum", R.pendulum)

                )
                );
        }
        foreach (Rotator R in obj.Animation.motors.OfType<Rotator>().ToList())
        {
            if (!PendulumRotators.Contains(R))
            {
                motors.Add(new XElement("Rotator",
                    new XElement("Identifier", R.Identifier),
                    new XElement("axis", GetGameObjectPath(R.axis.gameObject).Substring(GetGameObjectPath(obj.gameObject).Length + 1)),
                    new XElement("maxSpeed", R.maxSpeed),
                    new XElement("accelerationSpeed", R.accelerationSpeed),
                    new XElement("rotationAxis", R.rotationAxis)
                    )
                    );
            }
        }
        foreach (RotateBetween R in obj.Animation.motors.OfType<RotateBetween>().ToList())
        {
             motors.Add(new XElement("RotateBetween",
                 new XElement("Identifier", R.Identifier),
                new XElement("axis", GetGameObjectPath(R.axis.gameObject).Substring(GetGameObjectPath(obj.gameObject).Length + 1)),
                new XElement("rotationAxis", R.rotationAxis),
                new XElement("duration", R.duration)
                )
                );
        }
        foreach (Mover R in obj.Animation.motors.OfType<Mover>().ToList())
        {

            motors.Add(new XElement("Mover",
                new XElement("Identifier", R.Identifier),
                new XElement("axis", GetGameObjectPath(R.axis.gameObject).Substring(GetGameObjectPath(obj.gameObject).Length + 1)),
                new XElement("toPosition", R.toPosition),
                new XElement("duration", R.duration)
                )
                );
        }
        foreach (MultipleRotations R in obj.Animation.motors.OfType<MultipleRotations>().ToList())
        {

            motors.Add(new XElement("MultipleRotations",
                new XElement("Identifier", R.Identifier),
                new XElement("MainAxis", GetGameObjectPath(R.mainAxis.gameObject).Substring(GetGameObjectPath(obj.gameObject).Length + 1)),
                
                    from axis in R.Axiss
                    select new XElement("axis", GetGameObjectPath(axis.gameObject).Substring(GetGameObjectPath(obj.gameObject).Length + 1))
                         )
                     
                );
        }
        return new XElement("motors", motors.ToArray());
    }
    public static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }
}
