using System.Collections.Generic;
using UnityEngine;

public class AgentLineDrawer : MonoBehaviour {
    public List<TextAsset> files;
    public float yLevel;
    [Range(0.1f, 5f)]
    public float lineThickness;

    public Color lineColor;
}
