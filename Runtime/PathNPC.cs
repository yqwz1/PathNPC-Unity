using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PathNPCTool
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PathNPC : MonoBehaviour
    {
        [SerializeField] private List<Paths> paths;
        [SerializeField] private float NPCspeed = 3.5f;
        [SerializeField] private int currentPathIndex = 0;
        [SerializeField] private int currentWaypointIndex = 0;
        [SerializeField] private bool CanWalk = true;
        [SerializeField] private NavMeshAgent agent;

        private void Start()
        {
            if (paths == null || paths.Count == 0)
            {
                Debug.LogError("No paths assigned.", this);
                enabled = false;
                return;
            }

            if (paths[currentPathIndex].waypoints == null ||
                paths[currentPathIndex].waypoints.Count == 0)
            {
                Debug.LogError(
                    $"Path '{paths[currentPathIndex].pathName}' has no waypoints assigned.", this);
                enabled = false;
                return;
            }

            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            agent.speed = NPCspeed;
            SetNextWaypoint();
        }

        private void Update()
        {
            if (CanWalk)
                CheckWaypointProgress();
        }

        private void CheckWaypointProgress()
        {
            if (!paths[currentPathIndex].loopPath)
            {
                if (currentWaypointIndex >= paths[currentPathIndex].waypoints.Count)
                    return;

                if (agent.pathPending)
                    return;

                if (agent.remainingDistance < 0.5f)
                {
                    currentWaypointIndex++;
                    SetNextWaypoint();
                }
            }
            else
            {
                if (currentWaypointIndex <= 0)
                    return;

                if (agent.pathPending)
                    return;

                if (agent.remainingDistance < 0.5f)
                {
                    currentWaypointIndex--;
                    SetWayPointReverse();
                }
            }

            float random = Random.Range(NPCspeed - 0.5f, NPCspeed + 0.5f);
            agent.speed = random;
        }

        private void SetNextWaypoint()
        {
            List<WayPoint> waypoints = paths[currentPathIndex].waypoints;

            if (currentWaypointIndex >= waypoints.Count)
            {
                SetWayPointReverse();
                return;
            }

            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
            agent.updateRotation = true;

            WayPoint waypoint = paths[currentPathIndex].waypoints[currentWaypointIndex];
            if (waypoint.WaitTime > 0)
                WaitOnWaypoint(waypoint);
        }

        /// <summary>
        /// Switches the NPC to a different path by index and starts walking from the first waypoint.
        /// </summary>
        public void StartPath(int pathIndex)
        {
            currentPathIndex = pathIndex;
            currentWaypointIndex = 0;
            SetNextWaypoint();
        }

        /// <summary>
        /// Enables or disables NPC movement.
        /// </summary>
        public void SetCanWalk(bool canWalk)
        {
            CanWalk = canWalk;
            agent.isStopped = !canWalk;
        }

        /// <summary>
        /// Returns whether the NPC is currently allowed to walk.
        /// </summary>
        public bool GetCanWalk()
        {
            return CanWalk;
        }

        /// <summary>
        /// Returns the list of all paths configured on this NPC.
        /// </summary>
        public List<Paths> GetPaths()
        {
            return paths;
        }

        private void WaitOnWaypoint(WayPoint waypoint)
        {
            StartCoroutine(Wait(waypoint.WaitTime));
        }

        private IEnumerator Wait(float waitTime)
        {
            SetCanWalk(false);
            yield return new WaitForSeconds(waitTime);
            SetCanWalk(true);
        }

        private void SetWayPointReverse()
        {
            var path = paths[currentPathIndex];
            path.loopPath = true;
            List<WayPoint> waypoints = paths[currentPathIndex].waypoints;

            if (currentWaypointIndex <= 0)
            {
                path.loopPath = false;
                SetNextWaypoint();
                return;
            }

            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
            agent.updateRotation = true;

            WayPoint waypoint = paths[currentPathIndex].waypoints[currentWaypointIndex];
            if (waypoint.WaitTime > 0)
                WaitOnWaypoint(waypoint);
        }

        private void OnDrawGizmos()
        {
            if (paths == null) return;

            for (int i = 0; i < paths.Count; i++)
            {
                Paths path = paths[i];

                if (!path.ShowPath) continue;
                if (path.waypoints == null || path.waypoints.Count == 0) continue;

                Gizmos.color = path.color;

                for (int j = 0; j < path.waypoints.Count; j++)
                {
                    if (path.waypoints[j] == null) continue;

                    Vector3 currentPosition = path.waypoints[j].transform.position;
                    Gizmos.DrawSphere(currentPosition, 0.2f);

                    if (j + 1 < path.waypoints.Count && path.waypoints[j + 1] != null)
                        Gizmos.DrawLine(currentPosition, path.waypoints[j + 1].transform.position);
                }
            }
        }
    }
}
