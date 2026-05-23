using System.Collections.Generic;
using UnityEngine;

namespace PathNPCTool
{
    [System.Serializable]
    public struct Paths
    {
        public string pathName;
        public List<WayPoint> waypoints;
        public Color color;
        public bool ShowPath;
        public bool showLabel;
        public bool loopPath;
    }
}
