using System;
using UnityEngine;

public class GridTester : MonoBehaviour {
    public WorldGrid grid;
    public Transform zeroPt;

    private void Start() {
        grid.SetZero(zeroPt.position);
        grid.Generate(zeroPt);
    }
}
