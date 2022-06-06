using UnityEditor;
using UnityEngine;

public class WorldGridContainer: MonoBehaviour {
    public WorldGrid worldGrid;

    private void OnDrawGizmos() {
        /*Gizmos.color = Color.yellow;
        foreach (var worldGridCell in worldGrid.cells) {
            if (worldGridCell != null) {
                //Gizmos.DrawSphere(worldGridCell.position, worldGridCell.GetComponent<HeatmapCell>().numVisFreq);
                //Handles.Label(worldGridCell.position, worldGridCell.GetComponent<HeatmapCell>().numVis.ToString());
            }
        }*/
    }
}
