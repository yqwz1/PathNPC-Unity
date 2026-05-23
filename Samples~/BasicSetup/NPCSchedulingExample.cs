using UnityEngine;
using PathNPCTool;

/// <summary>
/// Example: switches an NPC between paths based on a schedule.
/// Attach to the same GameObject as PathNPC.
/// </summary>
[RequireComponent(typeof(PathNPC))]
public class NPCSchedulingExample : MonoBehaviour
{
    [SerializeField] private float switchInterval = 10f;

    private PathNPC pathNPC;
    private float timer;
    private int currentPath;

    private void Start()
    {
        pathNPC = GetComponent<PathNPC>();
        timer = switchInterval;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            currentPath = (currentPath + 1) % pathNPC.GetPaths().Count;
            pathNPC.StartPath(currentPath);
            timer = switchInterval;
        }
    }
}
