using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor (typeof(CrowdAgentSpawner))]
[CanEditMultipleObjects]
public class CrowdAgentSpawnerEditor: Editor {

    private CrowdAgentSpawner obj;
    
    private void OnEnable() {
        obj = (CrowdAgentSpawner)target;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        DrawDefaultInspector();
        if (GUILayout.Button("Fill Destination List With Random Weights")) {
            
            var gos = GameObject.Find("DestinationTargets").GetComponentsInChildren<Transform>().Skip(1);
            obj.locationWeightList = gos.Select(x => new CrowdAgentSpawner.LocationWeightPair( x.transform, Random.Range(1, 20))).ToList();
        }

        if (GUILayout.Button("Fill Destination List With Uniform Weight")) {
            var gos = GameObject.Find("DestinationTargets").GetComponentsInChildren<Transform>().Skip(1);
            obj.locationWeightList = gos.Select(x => new CrowdAgentSpawner.LocationWeightPair( x.transform, 1)).ToList();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
