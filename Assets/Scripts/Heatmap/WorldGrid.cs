using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "World Grid", menuName = "World Grid", order = 0)]
public class WorldGrid: ScriptableObject {
    [SerializeField] private int height;
    [SerializeField] private int width;
    [SerializeField] private float cellSize;
    [SerializeField] private Vector3 worldZeroCoord;
    [SerializeField] private LayerMask validMask;
    [SerializeField] private LayerMask invalidMask;
    [SerializeField] private float testSize;
    [SerializeField] private Gradient heatmapGradient;
    [SerializeField] private GameObject heatmapObject;
    [SerializeField] private int searchRadius;
    [SerializeField] private int extremaRadius;
    [SerializeField] private Transform worldZero;
    public Transform[,] cells;

    public void SetZero(Vector3 zero) {
        worldZeroCoord = zero;
    }
    
    public void Generate(Transform zero) {
        cells = new Transform[width, height];
        worldZero = zero;
        worldZeroCoord = worldZero.position;
        ApproveValidCells();
        GenerateHeatMap();
        CalculateLocalExtrema();
        var dirPath = Application.dataPath + "/Data/";
        var fileName = dirPath + "extrema.csv";
        File.Create(fileName).Dispose();
        var writer = new StreamWriter(fileName);
        writer.WriteLine("XPos,ZPos");
        foreach (var cell in cells) {
            if(cell && cell.GetComponent<HeatmapCell>() && cell.GetComponent<HeatmapCell>().isExtrema)
                writer.WriteLine(cell.position.x + "," + cell.position.z);
        }
        writer.Close();
    }
    // reduce clumps to centroid
    // Add in decision point obj
    // Use crowd spawner to spawn agents
    
    private void ApproveValidCells() {
        Debug.Log(worldZeroCoord);
        for (var i = 0; i < cells.GetLength(0); i++) {
            for (var j = 0; j < cells.GetLength(1); j++) {
                var castOrigin = worldZeroCoord + Vector3.forward * ((i + 0.5f) * cellSize) + Vector3.left * ((j + 0.5f) * cellSize);
                
                if (Physics.CheckSphere(castOrigin, cellSize/testSize, validMask)) {
                    //Debug.Log("valid");
                    if(!Physics.CheckCapsule(castOrigin + Vector3.down * cellSize, castOrigin + Vector3.up * cellSize, cellSize/testSize, invalidMask)) {
                        //var test = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        var test = Instantiate(heatmapObject, worldZero);
                        cells[i, j] = test.transform;
                        //test.GetComponent<Collider>().enabled = false;
                        test.transform.position = castOrigin;
                        test.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                    }
                    
                } else {
                    cells[i, j] = null;
                }
            }
        }
    }

    private void GenerateHeatMap() {
        var numVisFrom = new Dictionary<(int, int), int>();

        for(var x1 = 0; x1 < cells.GetLength(0); x1++) {
            for(var y1 = 0; y1 < cells.GetLength(1); y1++) {
                if(cells[x1,y1] == null) continue;
                var running = 0;
                //var dirBlacklist = new HashSet<Vector3>(); 
                for(var x2 = 0; x2 < cells.GetLength(0); x2++) {
                    for(var y2 = 0; y2 < cells.GetLength(1); y2++) {
                        if(cells[x2,y2] == null) continue;
                        var c1pos = cells[x1,y1].position;
                        var c2pos = cells[x2,y2].position;
                        if (Mathf.Abs(x1 - x2) > searchRadius || Mathf.Abs(y1 - y2) > searchRadius) continue;
                        var dir = c2pos - c1pos;
                        //if (dirBlacklist.Contains(dir)) continue;
                        if (!Physics.Raycast(c1pos + Vector3.up, dir, (c2pos - c1pos).magnitude,
                            LayerMask.GetMask("Wall"))) {
                            running++;
                        } else {
                            //dirBlacklist.Add(dir);
                        }
                    }
                }
                numVisFrom.Add((x1,y1), running);
            }
        }

        float max = numVisFrom.Max(x => x.Value);
        
        for (var x1 = 0; x1 < cells.GetLength(0); x1++) {
            for (var y1 = 0; y1 < cells.GetLength(1); y1++) {
                if(cells[x1,y1] == null) continue;
                var t = numVisFrom[(x1, y1)] / max;
                cells[x1, y1].GetComponent<Renderer>().material.SetColor("_BaseColor", heatmapGradient.Evaluate(t));
                cells[x1, y1].GetComponent<HeatmapCell>().numVis = numVisFrom[(x1, y1)];
                cells[x1, y1].GetComponent<HeatmapCell>().numVisFreq = t;
            }
        }
    }

    private void CalculateLocalExtrema() {
        for (var x1 = 0; x1 < cells.GetLength(0); x1++) {
            for (var y1 = 0; y1 < cells.GetLength(1); y1++) {
                if(cells[x1,y1] == null) continue;
                var extrema = true;
                var dir = 0;
                var baseNumVis = cells[x1, y1].GetComponent<HeatmapCell>().numVis;
                
                for(var x2 = 0; x2 < cells.GetLength(0); x2++) {
                    for(var y2 = 0; y2 < cells.GetLength(1); y2++) {
                        if(cells[x2,y2] == null) continue;
                        var otherCellNumVis = cells[x2, y2].GetComponent<HeatmapCell>().numVis;
                        var c1pos = cells[x1,y1].position;
                        var c2pos = cells[x2,y2].position;
                        if (Vector3.Distance(c1pos, c2pos) > extremaRadius) continue;
                        var lt = baseNumVis < otherCellNumVis;
                        var gt = baseNumVis > otherCellNumVis;
                        var eq = baseNumVis == otherCellNumVis;
                        switch (dir) {
                            case 0:
                                dir = lt ? -1 : (gt ? 1 : 0);
                                break;
                            case 1: {
                                if (lt) extrema = false;
                                break;
                            }
                            default: {
                                if (gt) extrema = false;
                                break;
                            }
                        }
                    }
                }
                if (extrema) {
                    //cells[x1, y1].GetComponent<Renderer>().material.SetColor("_BaseColor", dir == -1 ? Color.black : Color.yellow);
                    cells[x1, y1].GetComponent<HeatmapCell>().isExtrema = dir == 1;
                }
            }
        }

        var clumps = new List<List<(int, int)>>();
        for (var x1 = 0; x1 < cells.GetLength(0); x1++) {
            for (var y1 = 0; y1 < cells.GetLength(1); y1++) {
                if(cells[x1, y1] == null) continue;
                if(cells[x1, y1].GetComponent<HeatmapCell>().isExtrema) {
                    /*var alreadyIn = false;
                    foreach (var clump in clumps) {
                        if (!clump.Contains((x1, y1))) continue;
                        alreadyIn = true;
                        break;
                    }

                    if (!alreadyIn) {*/
                        var clump = new List<(int, int)>();
                        var frontier = new Queue<(int, int)>();
                        frontier.Enqueue((x1, y1));
                        while(frontier.Count != 0) {
                            var cell = frontier.Dequeue();
                            cells[cell.Item1, cell.Item2].GetComponent<HeatmapCell>().isExtrema = false;
                            if(clump.Contains(cell)) continue;
                            clump.Add(cell);
                            foreach (var neighbor in FindNeighbors(cell).Where(x => cells[x.Item1, x.Item2].GetComponent<HeatmapCell>().isExtrema)) {
                                if(cells[neighbor.Item1, neighbor.Item2].GetComponent<HeatmapCell>().isExtrema)
                                    frontier.Enqueue(neighbor);
                            }
                        }
                        clumps.Add(clump);
                    //}
                }
            }
        }
        Debug.Log(clumps.Count);
        foreach (var clump in clumps) {
            var midX = clump.Aggregate(0, (x, next) => x + next.Item1) / clump.Count;
            var midY = clump.Aggregate(0, (y, next) => y + next.Item2) / clump.Count;
            Debug.Log(midX);
            Debug.Log(midY);
            Debug.Log(clumps.Count);
            cells[midX, midY].GetComponent<HeatmapCell>().isExtrema = true;
            cells[midX, midY].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.yellow);

        }
    }

    private void Reduce((int, int) coords) {
        /*var canReduce = true;
        var neighbors = FindNeighbors(coords);
        foreach (var neighbor in neighbors) {
            foreach (var neighbor2 in neighbors) {
                var obj1 = cells[neighbor.Item1, neighbor.Item2];
                var obj2 = cells[neighbor2.Item1, neighbor2.Item2];
                var hit = Physics.Raycast(obj1.position, obj2.position - obj1.position,
                    Vector3.Distance(obj1.position, obj2.position), invalidMask);
                if (hit) {
                    canReduce = false;
                    break;
                }
            }
            if(!canReduce) break;
        }

        if (canReduce) {
            foreach (var neighbor in neighbors) {
                Destroy(cells[neighbor.Item1, neighbor.Item2].gameObject);
                cells[neighbor.Item1, neighbor.Item2] = cells[coords.Item1, coords.Item2];
            }
        }*/
    }

    private List<(int, int)> FindNeighbors((int, int) coords) {
        var neighbors = new List<(int, int)>();
        for (var i = coords.Item1 > 0 ? coords.Item1 - 1 : 0; i <= coords.Item1 + 1; i++) {
            if(i >= cells.GetLength(0) - 1) continue;
            for (var j = coords.Item2 > 0 ? coords.Item2 - 1 : 0; j <= coords.Item2 + 1; j++) {
                if(j >= cells.GetLength(1) - 1) continue;
                if(i == 0 && j == 0) continue;
                if (cells[i, j] != null) {
                    neighbors.Add((i, j));
                }
            }
        }

        return neighbors;
    }
}
