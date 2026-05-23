using UnityEngine;

namespace PathNPCTool
{
    /// <summary>
    /// Marks a GameObject as a navigation waypoint for PathNPC.
    /// </summary>
    public class WayPoint : MonoBehaviour
    {
        public Transform waypoint;

        [SerializeField] private float waitTime = 0f;

        public float WaitTime
        {
            get => waitTime;
            set => waitTime = value;
        }

        private void Awake()
        {
            waypoint = transform;
        }
    }
}
