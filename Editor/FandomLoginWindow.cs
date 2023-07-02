using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FandomLoginWindow : EditorWindow
{
    public bool ModalComplete { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }

    bool saveUsername;

    public static FandomLoginWindow Open()
    {
        FandomLoginWindow window = EditorWindow.GetWindow<FandomLoginWindow>();
        window.maxSize = new Vector2(512, 128);
        window.titleContent = new GUIContent("Login");

        if (EditorPrefs.HasKey("Fandom_Api_Login_Username"))
        {
            window.Username = EditorPrefs.GetString("Fandom_Api_Login_Username");
            window.saveUsername = true;
        }

        window.ShowModal();
        return window;
    }

    private void OnGUI()
    {
        var defaultWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 72;

        Username = EditorGUILayout.TextField("Username", Username);
        Password = EditorGUILayout.PasswordField("Password", Password);

        EditorGUIUtility.labelWidth = 104;
        saveUsername = EditorGUILayout.Toggle("Save Username?", saveUsername);

        EditorGUIUtility.labelWidth = defaultWidth;

        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
        {
            ModalComplete = false;
            this.Close();
        }
        if (GUILayout.Button("Login", GUILayout.ExpandWidth(false)))
        {
            ModalComplete = true;

            if (saveUsername && !string.IsNullOrEmpty(Username))
            {
                EditorPrefs.SetString("Fandom_Api_Login_Username", Username);
            }

            this.Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}
