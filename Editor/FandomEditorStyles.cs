using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FandomEditorStyles
{
    public static GUIStyle ListViewHeader { get; } = new GUIStyle(EditorStyles.largeLabel)
    {
        alignment = TextAnchor.MiddleCenter,
    };

    public static GUIStyle ListViewItem { get; } = new GUIStyle("button")
    {
        alignment = TextAnchor.MiddleLeft,
        //normal =
        //{
        //    background = EditorGUIUtility.FindTexture("d_forward")
        //},
    };
}
