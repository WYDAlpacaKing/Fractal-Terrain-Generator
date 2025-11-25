using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class GeneratorGUI: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MapGenerator generator = (MapGenerator)target;
        if (GUILayout.Button("Generate Map"))
        {
            generator.Generator();
        }
    }
}
