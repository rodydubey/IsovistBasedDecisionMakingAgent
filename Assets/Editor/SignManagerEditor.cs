using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(SignManager))]
[CanEditMultipleObjects]
public class SignManagerEditor : Editor {
    private SignManager obj;
    
    private void OnEnable() {
        obj = (SignManager)target;
    }
    
    public override void OnInspectorGUI() {
        serializedObject.Update();
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Signs")) {
            obj.GenerateSigns();
        }
        if (GUILayout.Button("Generate Directions")) {
            obj.GenerateDirections();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
