using FandomApi;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FandomSettingsWindow : EditorWindow
{
    FandomApiClient _client;
    ApiSettingsModel _model;

    public static FandomSettingsWindow Open(FandomApiClient client)
    {
        FandomSettingsWindow window = EditorWindow.GetWindow<FandomSettingsWindow>();
        window.maxSize = new Vector2(512, 128);
        window.titleContent = new GUIContent("Login");

        window._client = client;
        window._model = window._client.ApiSettings.GetApiSettings();

        window.ShowModal();
        return window;
    }

    private void OnGUI()
    {
        var defaultWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 64;

        _model.ApiPath = EditorGUILayout.TextField("Api Path", _model.ApiPath);

        EditorGUIUtility.labelWidth = defaultWidth;

        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
        {
            this.Close();
        }
        if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
        {
            _client.ApiSettings.SetApiSettings(_model);
            this.Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}
