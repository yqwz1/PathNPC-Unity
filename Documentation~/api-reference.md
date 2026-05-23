# API Reference

## Namespace

All types are in the `PathNPCTool` namespace.

```csharp
using PathNPCTool;
```

---

## PathNPC : MonoBehaviour

The core NPC movement controller. Attach this to any GameObject with a NavMeshAgent.

### Required Components

- `NavMeshAgent` (auto-added via `[RequireComponent]`)

### Serialized Fields

| Field | Type | Default | Description |
|---|---|---|---|
| `paths` | `List<Paths>` | empty | All paths configured on this NPC |
| `NPCspeed` | `float` | `3.5` | Base movement speed |
| `currentPathIndex` | `int` | `0` | Index of the currently active path |
| `currentWaypointIndex` | `int` | `0` | Index of the current target waypoint |
| `CanWalk` | `bool` | `true` | Whether the NPC is allowed to move |
| `agent` | `NavMeshAgent` | `null` | Reference to the NavMeshAgent (auto-assigned at Start) |

### Public Methods

#### `StartPath(int pathIndex)`
Switches the NPC to a different path and begins walking from the first waypoint.

**Parameters:**
- `pathIndex` -- Zero-based index into the paths list

**Example:**
```csharp
npc.StartPath(1); // Switch to second path
```

#### `SetCanWalk(bool canWalk)`
Enables or disables NPC movement. When disabled, the NavMeshAgent is also stopped.

**Parameters:**
- `canWalk` -- `true` to enable movement, `false` to stop

#### `GetCanWalk() : bool`
Returns whether the NPC is currently allowed to walk.

#### `GetPaths() : List<Paths>`
Returns the list of all configured paths.

---

## Paths (struct)

Serializable data structure representing a single named path.

### Fields

| Field | Type | Description |
|---|---|---|
| `pathName` | `string` | Display name for this path |
| `waypoints` | `List<WayPoint>` | Ordered list of waypoint references |
| `color` | `Color` | Color used for gizmo visualization |
| `ShowPath` | `bool` | Whether to draw gizmos for this path |
| `showLabel` | `bool` | Whether to show waypoint labels in Scene View |
| `loopPath` | `bool` | Whether the NPC loops (ping-pongs) on this path |

---

## WayPoint : MonoBehaviour

Marks a GameObject as a navigation waypoint.

### Fields

| Field | Type | Description |
|---|---|---|
| `waypoint` | `Transform` | Reference to this object's transform (auto-assigned) |
| `WaitTime` | `float` | Seconds the NPC pauses at this waypoint (0 = no pause) |

---

## PathNPCEditor (Editor only)

Custom editor for the PathNPC component. Provides:

- UIElements-based custom inspector
- Interactive Scene View waypoint placement
- Path visualization controls
- Waypoint management with undo support

This class is in the `PathNPCTool.Editor` namespace and only compiles in the Unity Editor.
