---
name: unity-mcp
description: Control Unity Editor via MCP — create scenes, edit scripts, manage GameObjects, run builds, and debug games through natural language
author: IvanMurzak
tags: [unity, game-development, mcp, editor, build, debug]
tools: [opencode, claude, cursor, codex]
---

# Unity MCP — AI Game Developer

Control your Unity Editor from this agent using the Model Context Protocol (MCP).

## Prerequisites

1. Unity Editor must be running with the project open
2. The MCP server must be started in Unity: **Window → AI Game Developer → Start Server**
3. The server listens on the port shown in the AI Game Developer window

## When to Use This Skill

- When the user asks to create/modify Unity scenes, GameObjects, or components
- When the user wants to edit C# scripts in the Unity project
- When the user wants to build the game for Android/iOS/standalone
- When the user wants to debug runtime issues or inspect the scene
- When the user wants to run the game in Play mode

## Available Tools

The MCP server exposes these tool categories:

### Scene Tools
- `scene_list` — List all scenes in the project
- `scene_create` — Create a new scene
- `scene_open` — Open a scene
- `scene_save` — Save the current scene

### GameObject Tools
- `gameobject_create` — Create a new GameObject
- `gameobject_list` — List GameObjects in the scene
- `gameobject_modify` — Modify a GameObject's properties
- `gameobject_delete` — Delete a GameObject

### Component Tools
- `component_add` — Add a component to a GameObject
- `component_modify` — Modify component properties
- `component_remove` — Remove a component

### Script Tools
- `script_create` — Create a new C# script
- `script_edit` — Edit an existing C# script
- `script_compile` — Check for compilation errors

### Build Tools
- `build_android` — Build APK for Android
- `build_ios` — Build for iOS
- `build_standalone` — Build standalone player

### Runtime Tools
- `playmode_enter` — Enter Play mode
- `playmode_exit` — Exit Play mode
- `console_read` — Read console messages
- `screenshot_capture` — Capture the Game view

### Asset Tools
- `asset_list` — List assets in the project
- `asset_create_material` — Create a material
- `asset_import` — Import assets

## Usage Examples

```
"Create a red cube at position (0, 1, 0)"
"Add a Rigidbody to the Player object"
"Build an APK for Android"
"What errors are in the console?"
"Create a new scene called 'Menu'"
"Run the game and tell me what happens"
```

## Important Notes

- Always check the Unity Console for errors after making changes
- The MCP server must be running for tools to work
- For Android builds, ensure the Android SDK is configured in Unity
- Changes in Play mode are temporary unless saved to a prefab
