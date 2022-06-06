using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(WorldGridContainer))]
[CanEditMultipleObjects]
public class WorldGridEditor: Editor {

    private WorldGridContainer obj;
    
    private void OnEnable() {
        obj = (WorldGridContainer)target;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Heatmap")) {
            for (var i = obj.transform.childCount - 1; i >= 0; i--) {
                DestroyImmediate(obj.transform.GetChild(i).gameObject);
            }
            obj.worldGrid.Generate(obj.transform);
        }
        serializedObject.ApplyModifiedProperties();
    }
}