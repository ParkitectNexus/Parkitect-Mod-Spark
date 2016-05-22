using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

[Serializable]
public class Shop : ObjectType
{
    public List<Product> products = new List<Product>();
    public Vector2 scrollPos2;
    public Product selected;

    public override void DrawGUI()
    {

        foreach (Product p in products)
        {
            try
            {

                EditorUtility.SetDirty(p);
            }
            catch 
            {
            }
        }
        Event e = Event.current;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Products:", EditorStyles.boldLabel);
        scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, "GroupBox", GUILayout.Height(100));
        for (int i = 0; i < products.Count; i++)
        {
            Color gui = GUI.color;
            if (products[i] == selected)
            { GUI.color = Color.red; }

            if (GUILayout.Button(products[i].Name + "    $" + products[i].price + ".00"))
            {

                GUI.FocusControl("");
                if (e.button == 1)
                {
                    GameObject.DestroyImmediate(products[i]);
                    products.RemoveAt(i);
                    return;
                }

                if (selected == products[i])
                {
                    selected = null;
                    return;
                }
                selected = products[i];
            }
            GUI.color = gui;
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Wearable Product"))
        {
            products.Add(GameObject.Find("ParkitectModManager").AddComponent<wearable>());
        }
        if (GUILayout.Button("Add Consumable Product"))
        {
            products.Add(GameObject.Find("ParkitectModManager").AddComponent<consumable>());
        }
        if (GUILayout.Button("Add OnGoing Product"))
        {
            products.Add(GameObject.Find("ParkitectModManager").AddComponent<ongoing>());
        }
        EditorGUILayout.EndHorizontal();
        if(selected != null)
        {
            if(!products.Contains(selected))
            {
                selected = null;
                return;
            }
            GUILayout.Space(10);
            selected.DrawGUI();

        }
    }

    public override List<object> Export()
    {
        
        return base.Export();
    }
}
[Serializable]
public class Product : MonoBehaviour
{
    [SerializeField]
    public string Name = "New Product";
    [SerializeField]
    public GameObject GO;
    [SerializeField]
    public float price = 10;
    public enum hand { Left, Right }
    [SerializeField]
    public hand Hand = hand.Left;
    [SerializeField]
    public List<ingredient> ingredients = new List<ingredient>();
    ingredient selected;
    private Vector2 scrollPos;

    public virtual void DrawGUI()
    {
        Name = EditorGUILayout.TextField("Product Name", Name);
        GO = (GameObject)EditorGUILayout.ObjectField("Game Object ", GO, typeof(GameObject), true);
        price = EditorGUILayout.FloatField("Price ", price);
        Hand = (hand)EditorGUILayout.EnumPopup("Hand ", Hand);
    }

    public void DrawIngredientsGUI()
    {
        Event e = Event.current;
        EditorGUILayout.LabelField("Ingredients:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal(GUILayout.Height(300));
        EditorGUILayout.BeginVertical("ShurikenEffectBg", GUILayout.Width(150));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

        for (int i = 0; i < ingredients.Count; i++)
        {
            Color gui = GUI.color;
            if (ingredients[i] == selected)
            { GUI.color = Color.red; }

            if (GUILayout.Button(ingredients[i].Name + "    $" + ingredients[i].price + ".00", "ShurikenModuleTitle"))
            {
                
                    GUI.FocusControl("");
                if (e.button == 1)
                {
                    ingredients.RemoveAt(i);
                    return;
                }

                if (selected == ingredients[i])
                {
                    selected = null;
                    return;
                }
                selected = ingredients[i];
            }
            GUI.color = gui;
        }
        EditorGUILayout.EndScrollView();
        
        if (GUILayout.Button("Add Ingredients"))
        {
            ingredients.Add(new ingredient());
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if(selected != null)
        {
            if (!ingredients.Contains(selected))
            {
                selected = null;
                return;
            }
            selected.Name = EditorGUILayout.TextField("Ingridient Name ", selected.Name);
            selected.price = EditorGUILayout.FloatField("Price ", selected.price);
            selected.amount = EditorGUILayout.FloatField("Amount ", selected.amount);
            selected.tweakable = EditorGUILayout.Toggle("Tweakable ", selected.tweakable);

            for (int i = 0; i < selected.effects.Count; i++)
            {
                Color gui = GUI.color;

                if (GUILayout.Button( "Effector " + selected.effects[i].Type, "ShurikenModuleTitle"))
                {

                    GUI.FocusControl("");
                    if (e.button == 1)
                    {
                        selected.effects.RemoveAt(i);
                        return;
                    }
                }

                selected.effects[i].Type = (effect.Types)EditorGUILayout.EnumPopup("Type ", selected.effects[i].Type);
                selected.effects[i].amount = EditorGUILayout.Slider("Amount", selected.effects[i].amount, 1f, -1f);
                GUI.color = gui;
            }
            if (GUILayout.Button("Add Effect"))
            {
                selected.effects.Add(new effect());
            }

        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }
}



[Serializable]
public class effect
{
    [SerializeField]
    public enum Types { hunger, thirst, happiness, tiredness, sugarboost }
    [SerializeField]
    public Types Type = Types.hunger;
    [SerializeField]
    public float amount;
}
[Serializable]
public class ingredient 
{
    
    [SerializeField]
    public string Name = "New Ingredient";
    [SerializeField]
    public float price = 1;
    [SerializeField]
    public float amount = 1;
    [SerializeField]
    public bool tweakable = true;
    [SerializeField]
    public List<effect> effects = new List<effect>();
}