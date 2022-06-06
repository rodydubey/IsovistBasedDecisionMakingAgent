using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SmartAgentController : MonoBehaviour {
    [SerializeField] protected float speed = 10.0f;
    [SerializeField] protected float maxDegreesDelta = 1f; // rad/s
    [SerializeField] private float trackingTimeResolution = 0.5f; // Time between coord tracking
    public uint id;
    public string rootPath;
    
    private List<TimeStampedCoords> coords;
    private Coroutine tracker;

    private GameObject midPt;
    
    protected List<Transform> lastSeenSigns = new List<Transform>();
    private bool visitedSign = true;

    public String dest;

    public FieldOfView fov;
    public IsovistViewer isov;

    private int framesStuck;
    public void Start() {
        coords = new List<TimeStampedCoords>();
        tracker = StartCoroutine(TrackAgent());
        fov = GetComponent<FieldOfView>();
    }

    public virtual void MakeRandomDecision(Vector3[] options) {
        Vector3 newDir;
        do {
            newDir = options[Random.Range(0, options.Length)];
        } while (Vector3.Dot(newDir, transform.forward) < -0.8 && options.Length > 1);
        transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);
    }

    public void SetCentroid(GameObject centroid) {
        midPt = centroid;
    }

    protected virtual void Update() {
        if (GetComponent<MoveTo>().Pathfinding) return;
        if (midPt == null) return;

        if (visitedSign) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation((midPt.transform.position - transform.position).normalized), maxDegreesDelta);
            transform.Translate(speed * Time.deltaTime * Vector3.forward);

            if (fov.visibleTargets.Count > 0) {
                var closest = fov.visibleTargets.OrderBy(x => Vector3.Distance(transform.position, x.position)).FirstOrDefault(x => !lastSeenSigns.Contains(x));
                if (closest != null) {
                    lastSeenSigns.Insert(0, closest);
                    visitedSign = false;
                }
            }
        } else {
            var dif = lastSeenSigns[0].position - transform.position;
            var nlook = new Vector3(dif.x, 0, dif.z);
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation((nlook).normalized), maxDegreesDelta);
            transform.Translate(speed * Time.deltaTime * Vector3.forward);

            if (Vector3.Distance(transform.position, lastSeenSigns[0].position) <= 1.0f) {
                visitedSign = true;
                ProcessSign(lastSeenSigns[0].GetComponent<Sign>());
            }
        }

        if (isov.Area < 0.5f) {
            framesStuck++;
            if (framesStuck > 120) {
                transform.rotation = transform.rotation * Quaternion.AngleAxis(180, Vector3.up);
                framesStuck = 0;
            }
        } else {
            framesStuck = 0;
        }
    }

    private void ProcessSign(Sign sign) {
        Debug.Log("Reading sign");
        //Debug.Log(sign.directions.Count);
        var dirs = ProcessSignDirs(sign);
        var dir = dirs.Where(x => x.Item1.Equals(dest));
        if (dir.Count() != 0) {
            var d = dir.First();
            //var dirs = sign.directions[sign.destinations.IndexOf(dest)];
            if (d.Item2.Equals("reached")) {
                Debug.Log("Reached Destination");
                return;
            }

            var navDest = sign.transform.GetComponentInChildren<Transform>().GetComponentsInChildren<Transform>();
            foreach (var dest in navDest) {
                if (dest.name == d.Item2) {
                    GetComponent<MoveTo>().SetTarget(dest);
                    while (!lastSeenSigns[0].Equals(sign.transform)) {
                        lastSeenSigns.RemoveAt(0);
                    }

                    break;
                }
                
            }
        } else {
            Debug.Log("Sign wasn't helpful");
            MakeRandomDecision(new []{transform.right, -transform.right});
        }
        visitedSign = true;
    }

    private List<(string, string)> ProcessSignDirs(Sign sign) {
        var dirs = new List<(string, string)>();
        foreach (var dest in sign.destinations) {
            if (!dest.Contains("-")) {
                dirs.Add((dest, sign.directions[sign.destinations.IndexOf(dest)]));
            } else {
                int r1, r2;
                try {
                    r1 = int.Parse(dest.Substring(0, dest.IndexOf("-")));
                    r2 = int.Parse(dest.Substring(dest.IndexOf("-") + 1));
                } catch (Exception e) {
                    r1 = int.Parse(dest.Substring(1, dest.IndexOf("-")));
                    r2 = int.Parse(dest.Substring(dest.IndexOf("-") + 2));
                }

                for (var i = r1; i <= r2; i++) {
                    dirs.Add((i.ToString(), sign.directions[sign.destinations.IndexOf(dest)]));
                }
            }
        }

        return dirs;
    }

    public void OnDestroy() {
        if(id == 0) return; // Player
        StopCoroutine(tracker);
        var fileName = rootPath + id + ".csv";
        File.Create(fileName).Dispose();
        var writer = new StreamWriter(fileName);
        writer.WriteLine("Time,XPos,ZPos");
        foreach (var frame in coords) {
            writer.WriteLine(frame.Time + "," + frame.X + "," + frame.Z);
        }
        writer.Close();
    }

    private IEnumerator TrackAgent() {
        while (true) {
            yield return new WaitForSeconds(trackingTimeResolution);
            coords.Add(new TimeStampedCoords(Time.time, transform.position.x, transform.position.z));
        }
    }

    private readonly struct TimeStampedCoords {
        public TimeStampedCoords(float time, float x, float z) {
            Time = time;
            X = x;
            Z = z;
        }
        
        public float Time { get; }
        public float X { get; }
        public float Z { get; }
    }
}
