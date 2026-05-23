# Getting Started with Path NPC Tool

This guide walks you through setting up your first NPC with patrol paths.

## Prerequisites

- Unity 2021.3 LTS or newer
- A scene with a baked NavMesh

## Step 1: Install the Package

Add the package via the Unity Package Manager using one of the methods described in the main README.

## Step 2: Create a Waypoint Prefab

The tool needs a prefab to instantiate as waypoint markers in your scene.

1. In your project, right-click in the Project window and select **Create > GameObject**
2. Name it `WaypointMarker`
3. Add the `WayPoint` component (**Add Component > WayPoint**)
4. Optionally add a visual indicator (a small sphere or icon) so you can see waypoints in the scene
5. Drag the GameObject into your Project folder to save it as a prefab
6. Delete the instance from the scene

## Step 3: Add PathNPC to Your NPC

1. Select your NPC GameObject in the Hierarchy
2. Click **Add Component** and search for **Path NPC**
3. A `NavMeshAgent` component is added automatically if one doesn't exist
4. In the Path NPC inspector:
   - Set **NPC Speed** to your desired value (e.g., 3.5)
   - Assign your waypoint prefab to the **Waypoint Prefab** field

## Step 4: Design a Path

1. Click **+ Add New Path** in the inspector
2. Expand the new path card
3. Double-click the path name to rename it (e.g., "Main Patrol")
4. Pick a color for the path gizmo
5. Click the **Place** button
6. Click in the Scene View to place waypoints along the desired route
7. Press **Escape** or click **Stop** when finished

## Step 5: Configure Waypoints

- **Wait Times**: For each waypoint, set the **Wait(s)** value to make the NPC pause there
- **Reorder**: Drag waypoint references in the list to change the order
- **Delete**: Click the **X** button next to a waypoint to remove it

## Step 6: Visualize and Test

- Click **Show Path** to see the colored gizmo lines connecting waypoints
- Click **Show Labels** to see waypoint indices in the Scene View
- Enter **Play Mode** to watch the NPC walk the route

## Step 7: Add More Paths

Repeat Steps 4-6 for additional paths. You can switch between paths at runtime using:

```csharp
GetComponent<PathNPC>().StartPath(pathIndex);
```

## Tips

- Use different colors for different paths to keep them visually distinct
- Enable **Loop** to make the NPC walk back and forth continuously
- The NPC's speed has a slight random variation built in for natural-looking movement
- Waypoints are placed slightly above the click point (Y + 5) to stay visible above terrain
