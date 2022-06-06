using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Run simulations length of walking from one end to the other
// Kill all agents after that length and then spawn the next wave
// Remove agent/agent collision


// Add a sign somwehere the agent can see
// Sign has information on destinations and direction to go
// IF agent sees their destination on the sign, they will take that direction
// Sign read from a JSON file


[CustomEditor (typeof(AgentLineDrawer))]
[CanEditMultipleObjects]
public class AgentLineDrawerEditor : Editor {
    private AgentLineDrawer obj;

    private List<(Vector3, Vector3)> segments;

    private void OnEnable() {
        obj = (AgentLineDrawer)target;
        segments = new List<(Vector3, Vector3)>();
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        DrawDefaultInspector();
        if (GUILayout.Button("Visualize Paths")) {
            segments.Clear();
            foreach(var csv in obj.files) {
                Debug.Log(csv.text);
                var lines = csv.text.Split('\n').Skip(1).Select(x => x.Split(',')).Select(x => x.Skip(1).Select(float.Parse).ToList()).ToList();
                
                for (var i = 1; i < lines.Count - 1; i++) {
                    segments.Add((new Vector3(lines[i - 1][0], obj.yLevel, lines[i - 1][1]), new Vector3(lines[i][0], obj.yLevel, lines[i][1])));
                    Debug.Log(i);
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI() {
        Handles.color = obj.lineColor;
        foreach (var segment in segments) {
            Handles.DrawLine(
                segment.Item1,
                segment.Item2,
                obj.lineThickness
            );
        }
    }
}