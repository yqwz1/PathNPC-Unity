# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-05-23

### Added
- Core `PathNPC` component with NavMeshAgent-based movement
- `WayPoint` component with configurable wait times
- `Paths` data structure supporting multiple named routes per NPC
- Custom editor with UIElements-based inspector
- Interactive waypoint placement in Scene View with click-to-place
- Scene View HUD showing placement state
- Per-path gizmo visualization with custom colors
- Per-path waypoint label display
- Ping-pong and loop path modes
- Inline path renaming via double-click
- Path and waypoint deletion with full undo support
- Inspector dashboard with live path/waypoint/missing stats
- Assembly definitions for Runtime and Editor
- UPM package manifest for Git-based installation
