using UnityEngine;
using PathNPCTool;

/// <summary>
/// Example: stops NPC movement when the player enters a trigger zone,
/// resumes when the player leaves.
/// </summary>
public class DialogueStopExample : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponent<PathNPC>();
        if (npc != null)
            npc.SetCanWalk(false);
    }

    private void OnTriggerExit(Collider other)
    {
        var npc = other.GetComponent<PathNPC>();
        if (npc != null)
            npc.SetCanWalk(true);
    }
}
