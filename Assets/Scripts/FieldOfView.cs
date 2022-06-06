using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DefaultNamespace;
using UnityEditor;

public class FieldOfView : MonoBehaviour {
    
    [Range(0,100)]
    [SerializeField] private float viewRadius;

    [Range(0,360)]
    [SerializeField] private float viewAngle;
    
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask crowdMask;
    [SerializeField] private LayerMask obstacleMask;
    
    [Space]
    [SerializeField] private float meshResolution;
    [SerializeField] private int edgeResolveIterations;
    [SerializeField] private float edgeDstThreshold;
    [Space]
    [SerializeField] private MeshFilter viewMeshFilter;
    private Mesh viewMesh;
    [Space]
    [SerializeField] public Isovist isovist;
    [SerializeField] private CrowdManager crowdManager;
    private bool shouldDraw => gameObject.GetComponent<IsovistViewer>().shouldDraw;
    
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();
    

    private void Start() {
        viewMesh = new Mesh {name = "View Mesh"};
        viewMeshFilter.mesh = viewMesh;
        
        StartCoroutine (nameof(FindTargetsWithDelay), 1/60f);
    }

    private void LateUpdate() {
        DrawFieldOfView();
        viewMeshFilter.gameObject.GetComponent<MeshRenderer>().enabled = shouldDraw;
    }

    public void SetIsovist(Isovist isovist) {
        this.isovist = isovist;
        try {
            GetComponent<IsovistViewer>().SetIsovist(isovist);
        } catch (Exception e) {
            // lol
        }
    }

    private IEnumerator FindTargetsWithDelay(float delay) {
        while (true) {
            yield return new WaitForSeconds (delay);
            FindVisibleTargets ();
        }
    }
    
    private void FindVisibleTargets() {
        visibleTargets.Clear ();
        var results = new Collider[100];
        Physics.OverlapSphereNonAlloc(transform.position, viewRadius, results, targetMask);
        foreach(var col in results) {
            if (col == null) break;
            var target = col.transform;
            if (col.GetComponent<Detectable>().IsVisible(transform, viewAngle, obstacleMask)) {
                visibleTargets.Add (target);
            }
        }
        
        var results2 = new Collider[100];
        crowdManager.crowd.Clear();
        Physics.OverlapSphereNonAlloc(transform.position, viewRadius, results2, crowdMask);
        foreach(var col in results2) {
            if (col == null) break;
            var target = col.transform;
            var dirToTarget = (target.position - transform.position).normalized;
            var dstToTarget = Vector3.Distance (transform.position, target.position);
            if (!(Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)) continue;
            if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask)) {
                crowdManager.crowd.Add (target);
            }
        }
    }

    private void DrawFieldOfView() {
        var stepCount = Mathf.RoundToInt(viewAngle * meshResolution); // Ray
        var stepAngleSize = viewAngle / stepCount; // Deg / Ray
        var viewPoints = new List<Vector3> ();
        var oldViewCast = new ViewCastInfo(); 
        
        for (var i = 0; i <= stepCount; i++) { 
            var angle = transform.eulerAngles.y - (viewAngle / 2) + (stepAngleSize * i); 
            var newViewCast = ViewCast(angle);

            if (i > 0) {
                var edgeDstThresholdExceeded = Mathf.Abs (oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit ||
                   (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
                    var edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) {
                        viewPoints.Add (edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero) {
                        viewPoints.Add (edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        var vertexCount = viewPoints.Count + 1;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[(vertexCount-2) * 3];

        vertices[0] = Vector3.zero;
        for(var i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i >= vertexCount - 2) continue;
            triangles [i * 3] = 0;
            triangles [i * 3 + 1] = i + 1;
            triangles [i * 3 + 2] = i + 2;
        }
        
        isovist.ClearPoints();
        isovist.AddPointFromWorld(transform.position, transform.position);
        foreach (var viewPoint in viewPoints) {
            isovist.AddPointFromWorld(viewPoint, transform.position);
        }
        
        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
        var minAngle = minViewCast.angle;
        var maxAngle = maxViewCast.angle;
        var minPoint = Vector3.zero;
        var maxPoint = Vector3.zero;

        for (var i = 0; i < edgeResolveIterations; i++) {
            var angle = (minAngle + maxAngle) / 2;
            var midViewCast = ViewCast(angle);

            var edgeDstThresholdExceeded = Mathf.Abs (minViewCast.dst - midViewCast.dst) > edgeDstThreshold;
            if (midViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
                minAngle = angle;
                minPoint = midViewCast.point;
            } else {
                maxAngle = angle;
                maxPoint = midViewCast.point;
            }
        }

        return new EdgeInfo (minPoint, maxPoint);
    }
    
    private ViewCastInfo ViewCast(float globalAngle) {
        var dir = DirFromAngle(globalAngle, true);
        return Physics.Raycast(transform.position, dir, out var hit, viewRadius, obstacleMask) 
            ? new ViewCastInfo(true, hit.point, hit.distance, globalAngle) 
            : new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if (!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private readonly struct ViewCastInfo {
        public readonly bool hit;
        public readonly Vector3 point;
        public readonly float dst;
        public readonly float angle;

        public ViewCastInfo(bool hit, Vector3 point, float dst, float angle) {
            this.hit = hit;
            this.point = point;
            this.dst = dst;
            this.angle = angle;
        }
    }

    private readonly struct EdgeInfo {
        public readonly Vector3 pointA;
        public readonly Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB) {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos() {
        var t = transform;
        var pos = t.position;
        var forward = t.forward;
        Handles.color = Color.white;
        Handles.DrawWireDisc(pos, Vector3.up, viewRadius);
        Handles.color = Color.magenta;
        Handles.DrawWireArc(
            pos, 
            Vector3.up, 
            Quaternion.Euler(0, -viewAngle/2f, 0) * forward, 
            viewAngle, 
            viewRadius
        );
        Handles.DrawLine(pos, Quaternion.Euler(0, -viewAngle/2f, 0) * forward * viewRadius + pos);
        Handles.DrawLine(pos, Quaternion.Euler(0, +viewAngle/2f, 0) * forward * viewRadius + pos);
    }
#endif
}