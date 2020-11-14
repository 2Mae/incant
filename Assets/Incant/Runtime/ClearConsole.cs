using System.Reflection;
using UnityEditor;

public static class ClearConsole
{
    [MenuItem("Incant/Console clear")]
    public static void CleanConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}
