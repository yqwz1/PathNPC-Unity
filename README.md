# Path NPC Tool for Unity

A visual, waypoint-based NPC pathing system for Unity. Design NPC patrol routes directly in the Scene View with an interactive custom editor -- no code required to set up paths.

![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black?logo=unity)
![License](https://img.shields.io/badge/License-MIT-blue)
![NavMesh](https://img.shields.io/badge/Requires-NavMesh-orange)

---

## Features

- **Visual Path Editor** -- Click-to-place waypoints directly in the Scene View with real-time preview
- **Multiple Paths per NPC** -- Define several named routes and switch between them at runtime
- **Ping-Pong & Loop** -- NPCs walk forward along waypoints then reverse back automatically, or loop continuously
- **Wait Times** -- Configure per-waypoint pause durations so NPCs idle at specific locations
- **Color-Coded Visualization** -- Each path gets its own color for gizmo lines and spheres
- **Inspector Dashboard** -- Live stats showing total paths, waypoints, and missing references at a glance
- **Scene View HUD** -- Floating overlay shows placement state while editing
- **Inline Rename** -- Double-click a path name in the inspector to rename it
- **Full Undo Support** -- All waypoint creation and deletion is undo-safe
- **NavMesh Integration** -- Built on Unity's NavMeshAgent for reliable pathfinding

---

## Requirements

| Requirement | Version |
|---|---|
| Unity | 2021.3 LTS or newer |
| NavMesh | Scene must have a baked NavMesh |
| Render Pipeline | Any (Built-in, URP, HDRP) |

---

## Installation

### Option A -- Unity Package Manager (Git URL)

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Paste:
   ```
   https://github.com/YOUR_USERNAME/PathNPC-Unity.git
   ```
4. Click **Add**

### Option B -- Local Folder

1. Clone or download this repository
2. In Unity, go to **Window > Package Manager**
3. Click **+** > **Add package from disk...**
4. Select the `package.json` file in the repo root

### Option C -- Copy Into Project

1. Copy the `Runtime/` and `Editor/` folders into your project's `Assets/` directory
2. The scripts will compile automatically

---

## Quick Start

### 1. Prepare Your Scene

Make sure your scene has a baked **NavMesh**:
- Go to **Window > AI > Navigation**
- Select your walkable surfaces
- Click **Bake**

### 2. Create a Waypoint Prefab

1. Create an empty GameObject
2. Add the **WayPoint** component to it
3. Save it as a prefab (drag into your Project folder)

### 3. Set Up an NPC

1. Select your NPC GameObject
2. Click **Add Component > Path NPC**  
   (A NavMeshAgent is added automatically)
3. In the inspector, assign your **Waypoint Prefab**
4. Set the **NPC Speed**

### 4. Create a Path

1. Click **+ Add New Path**
2. Double-click the path name to rename it (e.g., "Patrol Route A")
3. Expand the path and click **Place**
4. Click anywhere in the Scene View to drop waypoints
5. Press **Escape** or click **Stop** when done

### 5. Hit Play

The NPC walks along the waypoints. When it reaches the end, it turns around and walks back.

---

## Inspector Reference

### Header Dashboard

| Stat | Description |
|---|---|
| **Paths** | Total number of paths on this NPC |
| **WayPoints** | Total waypoints across all paths |
| **Missing WayPoints** | Waypoint slots that reference a destroyed or null object |

### NPC Settings

| Field | Description |
|---|---|
| **NPC Speed** | Base movement speed (slight random variation is applied at runtime for natural movement) |
| **Agent** | Reference to the NavMeshAgent (auto-assigned if empty) |
| **Can Walk** | Toggle NPC movement on/off |

### Per-Path Controls

| Button | Action |
|---|---|
| **Place / Stop** | Enter or exit waypoint placement mode |
| **Show / Hide Path** | Toggle gizmo visualization for this path |
| **Show / Hide Labels** | Toggle waypoint index labels in Scene View |
| **Clear WayPoints** | Delete all waypoints in this path (with undo) |
| **Loop / Stop Loop** | Toggle ping-pong looping |
| **Delete Path** | Remove the entire path and its waypoints |

### Per-Waypoint Settings

| Field | Description |
|---|---|
| **Object Slot** | Reference to the WayPoint component |
| **Wait(s)** | Seconds the NPC pauses at this waypoint (0 = no pause) |
| **X** | Delete this individual waypoint |

---

## Runtime API

### PathNPC

```csharp
// Switch the NPC to a different path by index
npc.StartPath(int pathIndex);

// Pause or resume movement (also stops the NavMeshAgent)
npc.SetCanWalk(bool canWalk);

// Check if the NPC is currently walking
bool walking = npc.GetCanWalk();

// Get all configured paths
List<Paths> allPaths = npc.GetPaths();
```

### Usage Examples

**Switch paths from a trigger:**
```csharp
public class PatrolZone : MonoBehaviour
{
    [SerializeField] private int pathIndex;

    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponent<PathNPC>();
        if (npc != null)
            npc.StartPath(pathIndex);
    }
}
```

**Stop NPC during dialogue:**
```csharp
public void StartConversation(GameObject npcObject)
{
    npcObject.GetComponent<PathNPC>().SetCanWalk(false);
}

public void EndConversation(GameObject npcObject)
{
    npcObject.GetComponent<PathNPC>().SetCanWalk(true);
}
```

---

## Project Structure

```
PathNPC-Unity/
  package.json                          # UPM package manifest
  Runtime/
    PathNPC.cs                          # Core NPC movement controller
    Paths.cs                            # Path data structure
    WayPoint.cs                         # Waypoint marker component
    PathNPCTool.Runtime.asmdef          # Runtime assembly definition
  Editor/
    PathNPCEditor.cs                    # Custom inspector & Scene View tools
    PathNPCTool.Editor.asmdef           # Editor assembly definition
  Documentation~/
    getting-started.md                  # Step-by-step setup guide
    api-reference.md                    # Full API documentation
  Samples~/
    BasicSetup/                         # Minimal example scene setup
```

---

## Troubleshooting

| Problem | Solution |
|---|---|
| NPC doesn't move | Check that a NavMesh is baked, the agent is on the NavMesh, and **Can Walk** is enabled |
| Waypoints spawn but NPC ignores them | Ensure waypoints are reachable on the NavMesh (not on unwalkable surfaces) |
| "No paths assigned" error | Add at least one path with waypoints before entering Play mode |
| Waypoint prefab won't place | Assign a prefab with a **WayPoint** component in the inspector |
| Gizmos not visible | Enable **Show Path** for the path, and make sure Gizmos are on in the Scene View |

---

## Contributing

Contributions are welcome. Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

---

## License

This project is licensed under the [MIT License](LICENSE).
