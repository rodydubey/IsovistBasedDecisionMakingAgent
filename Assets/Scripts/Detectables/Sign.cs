using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Detectable {
    public int id;
    public List<string> directions = new List<string>();
    public List<string> destinations = new List<string>();
    public Transform relevantPoint;
}
