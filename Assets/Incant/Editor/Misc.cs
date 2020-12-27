using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class Misc
{
    // [Shortcut("Create Folder", KeyCode.Q, ShortcutModifiers.Action)]
    // public static void Test()
    // {
    //     EditorApplication.ExecuteMenuItem("Assets/Create/Folder");
    // }

    [MenuItem("Incant/Console clear")]
    public static void CleanConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    [MenuItem("Incant/Create prefab")]
    public static void TestTestTest()
    {
        if (Selection.activeGameObject == null) { return; }
        string path = EditorUtility.SaveFilePanel("Save prefab", Application.dataPath, $"{Selection.activeGameObject.name}", "prefab");
        if (path == "") { return; }
        path = $"Assets/{path.Remove(0, Application.dataPath.Length + 1)}";
        var prefab = PrefabUtility.SaveAsPrefabAsset(Selection.activeGameObject, path, out bool success);
    }
}