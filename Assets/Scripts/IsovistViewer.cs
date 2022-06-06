using System;
using System.Linq;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class IsovistViewer : MonoBehaviour {
    [SerializeField] private float perimeter;
    [SerializeField] private float maxRadialLength;
    [SerializeField] private float minRadialLength;
    [SerializeField] private float signedPolygonArea;
    [SerializeField] private float centroidX;
    [SerializeField] private float centroidY;
    [SerializeField] private float area;
    [SerializeField] private float realSurfaceLength;
    [SerializeField] private float drift;
    [SerializeField] private float openness;
    [SerializeField] private float jaggedness;
    [SerializeField] private float occlusivity;
    //
    [SerializeField] private float wallSumX;
    [SerializeField] private float wallSumZ;
    //

    [SerializeField] private Isovist isovist;
    [SerializeField] public bool shouldDraw;
    private LineRenderer lr;
    private GameObject centroidMarker;

    public float Area => area;
    //
    private bool wallDistanceNavModel;
    //

    private void Start() {
        lr = GetComponent<LineRenderer>();
        centroidMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        centroidMarker.transform.localScale = new Vector3(0.2f, float.Epsilon, 0.2f);
        centroidMarker.GetComponent<Renderer>().material = Resources.Load<Material>("CentroidMarker");
        centroidMarker.name = "Centroid View Marker";
        Destroy(centroidMarker.GetComponent<Collider>());
        try {
            GetComponent<SmartAgentController>().SetCentroid(centroidMarker);
        } catch (Exception e) {
            //lol
        }
    }

    public void SetIsovist(Isovist isovist) {
        this.isovist = isovist;
    }
    
    //
    public void setWallDistanceModel(bool state)
    {
        wallDistanceNavModel = state;
    }
    //

    private void LateUpdate() {
        var dict = isovist.CalculateIsovistMeasures();
        perimeter = dict[Isovist.IsovistMeasures.Perimeter];
        maxRadialLength = dict[Isovist.IsovistMeasures.MaxRadialLength];
        minRadialLength = dict[Isovist.IsovistMeasures.MinRadialLength];
        signedPolygonArea = dict[Isovist.IsovistMeasures.SignedPolygonArea];
        centroidX = dict[Isovist.IsovistMeasures.CentroidX];
        centroidY = dict[Isovist.IsovistMeasures.CentroidY];
        area = dict[Isovist.IsovistMeasures.Area];
        realSurfaceLength = dict[Isovist.IsovistMeasures.RealSurfaceLength];
        drift = dict[Isovist.IsovistMeasures.Drift];
        openness = dict[Isovist.IsovistMeasures.Openness];
        jaggedness = dict[Isovist.IsovistMeasures.Jaggedness];
        occlusivity = dict[Isovist.IsovistMeasures.Occlusivity];
        //
        wallSumX = dict[Isovist.IsovistMeasures.WallSumX];
        wallSumZ = dict[Isovist.IsovistMeasures.WallSumZ];
        //
        if (shouldDraw) {
            DrawIsovist();
        } 
        centroidMarker.GetComponent<Renderer>().enabled = shouldDraw;
        DrawCentroid();
    }

    private void DrawIsovist() {
        var pts = isovist.GetPoints();
        var offset = transform.position;
        lr.positionCount = pts.Count + 1;
        lr.SetPositions(pts.Select(x => x + offset).ToArray());
        lr.SetPosition(pts.Count, offset);
    }

    private void DrawCentroid() {
        var offset = transform.position;
        //
        if (wallDistanceNavModel)
            centroidMarker.transform.position = 4 * new Vector3(wallSumX, 0f, wallSumZ) + offset;
        else
        //
            centroidMarker.transform.position = new Vector3(centroidX, 0f, centroidY) + offset;
    }
}
