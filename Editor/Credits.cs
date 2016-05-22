using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Net;

public class Credits : EditorWindow
{

    Texture2D logo = new Texture2D(200, 100);
    private Vector2 scrollPosition;
    XmlElement xelRoot;
    [MenuItem("Parkitect/Credits", false, 105)]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        Credits window = (Credits)EditorWindow.GetWindow(typeof(Credits));
        window.Show();
    }
    void OnEnable()
    {
        WebClient client = new WebClient();//
        // Start a download of the given URL
        var www = new WWW("https://parkitectnexus.com/img/logo.png");
        // assign the downloaded image to the main texture of the object
        www.LoadImageIntoTexture(logo);

        Stream stream;
        using (stream = client.OpenRead("http://modspark.parkitectnexus.com/Credits.xml"))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            xelRoot = doc.DocumentElement;

        }

    }
    void OnGUI()
    {
        if(xelRoot == null)
        {
            GUILayout.Label("Credits not found", "CN EntryError");
            return;
        }
        GUILayout.BeginHorizontal("flow background");
        GUILayout.BeginVertical();
        GUILayout.Space(7);
        GUILayout.Label("Credits", "LODLevelNotifyText");
        GUILayout.Space(7);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        XmlNodeList ModNodes = xelRoot.ChildNodes;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (XmlNode Node in ModNodes)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(Node.Name, "PreToolbar");
            foreach (XmlNode Credit in Node.ChildNodes)
            {
                try
                {
                    GUILayout.BeginHorizontal("ShurikenModuleTitle");
                    GUILayout.Label(Credit["Name"].InnerText, GUILayout.MaxWidth(Screen.width / 3 - 40 ));
                    GUILayout.Label(Credit["Job"].InnerText, GUILayout.MaxWidth(Screen.width / 3 * 2 - 40));
                    if (GUILayout.Button("Nexus", "minibutton", GUILayout.MaxWidth(50)))
                    {
                        if(Credit["URL"] != null)
                        Application.OpenURL(Credit["URL"].InnerText);
                    }

                    GUILayout.EndHorizontal();
                }
                catch {}
                
                
            }
            GUILayout.Space(10);
            GUILayout.EndVertical();

        }
        GUILayout.EndScrollView();
        GUILayout.FlexibleSpace();
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(logo, centeredStyle, GUILayout.MaxWidth(Screen.width), GUILayout.ExpandWidth(false));
    }
}