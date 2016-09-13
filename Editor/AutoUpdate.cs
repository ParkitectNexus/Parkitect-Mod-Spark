using UnityEngine;
using UnityEditor;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

public static class UpdateInfo
{
    static public int CurVerion = 201;
    static public int CurNewVersion;
    static public string NewSite;

    [MenuItem("Parkitect/Check Update", false, 54)]
    public static void Check(bool showDialog)
    {
        WebClient client = new WebClient();
        try
        {
            Stream stream;
            using (stream = client.OpenRead("http://modspark.parkitectnexus.com/ParkitectToolVersion.txt"))
            {
                StreamReader reader = new StreamReader(stream);
                string content = reader.ReadToEnd();
                List<string> Contents = content.Split(',').ToList<string>();
                Contents.Reverse();
                int NewVersion = int.Parse(Contents[1]);
                NewSite = Contents[0];

                if (NewVersion > CurVerion)
                {
                    try
                    {
                        PlayerPrefs.SetInt("LastUpdated", DateTime.Now.Day);
                        CurNewVersion = NewVersion;
                        Debug.Log("There is a new Parkitect Mod Tool version aviable for download! :" + NewVersion + "  Download: " + NewSite);
                        if (!showDialog)
                            return;
                        AutoUpdate window = (AutoUpdate)EditorWindow.GetWindow(typeof(AutoUpdate));
                        window.position = new Rect(Screen.width / 2 + 250, Screen.height / 2 + 150, 250, 150);
                        window.ShowUtility();
                    }
                    catch { }
                }
                else
                {

                    Debug.Log("Your'e parkitect mod setup is up to date!  CurVer: " + CurVerion + "   NewVer: " + NewVersion);
                    PlayerPrefs.SetInt("LastUpdated", DateTime.Now.Day);
                }

            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            //Handle Error
        }
    }
}


[InitializeOnLoad]
class AutoUpdate : EditorWindow
{

    static AutoUpdate()
    {
        CheckIfUpdate();
    }


    static private void CheckIfUpdate()
    {
        if (PlayerPrefs.GetInt("LastUpdated", 99) != 99)
        {
            if (DateTime.Now.Day != PlayerPrefs.GetInt("LastUpdated"))
            {
                UpdateInfo.Check(true);
            }
        }
        else
        {
            UpdateInfo.Check(true);
        }
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("There is a new Parkitect Mod Tool version aviable for download!", EditorStyles.wordWrappedLabel);
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Your Verion: " + UpdateInfo.CurVerion, EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("New Version: " + UpdateInfo.CurNewVersion, EditorStyles.boldLabel);

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Download")) { Application.OpenURL(UpdateInfo.NewSite); this.Close(); }
        GUI.color = Color.white;
        if (GUILayout.Button("Ok")) this.Close();
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
    }
}
