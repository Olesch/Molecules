using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridInfo<DirectionsInfo> 
{
    public bool enabled;
    public bool visited;
    public Dictionary<DirectionsInfo, bool> dirs = new Dictionary<DirectionsInfo, bool>();
}
