# Getting Started

This tutorial walks you through building a small game with Game Wizard from scratch. By the end, you'll understand how to set up a project, write the YAML configuration files, and wire everything together.

We'll build a simplified version of the included [visual novel example](../examples/visual-novel/) — an intro cutscene, a world map with one NPC, a dialog choice, and an ending.

## Table of Contents

1. [Project Setup](#1-project-setup)
2. [Install .NET Dependencies](#2-install-net-dependencies)
3. [Create the Root Scene](#3-create-the-root-scene)
4. [Write the Game Config](#4-write-the-game-config)
5. [Create Your First Cutscene](#5-create-your-first-cutscene)
6. [Add a World Map](#6-add-a-world-map)
7. [Connect the Intro to the Map](#7-connect-the-intro-to-the-map)
8. [Add an NPC Dialog with a Choice](#8-add-an-npc-dialog-with-a-choice)
9. [Wire Up the Choice Menu](#9-wire-up-the-choice-menu)
10. [Add Game State](#10-add-game-state)
11. [Add an Ending](#11-add-an-ending)
12. [Configure Input Actions](#12-configure-input-actions)
13. [Run Your Game](#13-run-your-game)
14. [Next Steps](#14-next-steps)

---

## 1. Project Setup

Start with a Godot 4.6+ project that has C# / .NET support enabled.

**Download Game Wizard** from [github.com/akalman/game-wizard](https://github.com/akalman/game-wizard) by clicking **Code → Download ZIP**.

**Extract** the ZIP and copy the `engine/` and `modules/` directories into an `addons/game-wizard/` folder inside your Godot project:

```
your-project/
  project.godot
  addons/
    game-wizard/      ← from the ZIP
      engine/
      modules/
  scenes/             ← you'll create this
  sprites/            ← your game art
```

## 2. Install .NET Dependencies

Game Wizard uses .NET libraries (such as YamlDotNet) that need to be restored before the project will compile. Installation steps can vary based on how Godot and any configured external editor are set up.

### Windows with VSCode as external editor

1. Open your Godot project folder in VS Code or launch VS Code by opening a script in the Godot UI.
2. Open the integrated terminal (**Terminal → New Terminal** or `` Ctrl+` ``).
3. The default terminal in VS Code when linked to Godot on Windows is typically PowerShell or WSL. In either case, run:

```bash
dotnet restore
```

This reads the `.csproj` file in your project and downloads the required NuGet packages. You only need to run this once (or again if dependencies change).

## 3. Create the Root Scene

Every Game Wizard game needs a root Godot scene with a `GameController` node.

1. In Godot, create a new 2D scene.
2. Attach the `GameController.cs` script from `addons/game-wizard/engine/GameController.cs` to the root node.
3. In the Inspector, create a new plain text file called `game.yaml`and set the **Game Config Path** export in your new scene to this file (e.g., `res://game.yaml`).
4. Save this scene (e.g., `res://my-game.tscn`).
5. Set this scene as your project's **Main Scene** in Project Settings → Application → Run → Main Scene.

## 4. Write the Game Config

Create `game.yaml` in your project root. This is the top-level configuration file that defines your entire game structure — what modules to load, what scenes exist, and how they connect together.

```yaml
name: My First Game

modules:
  - res://addons/game-wizard/modules/core/config.yaml

initial-scene: intro

scenes:

  intro:
    template: core.dialog-cutscene
    config: res://scenes/intro.yaml
```

The `modules` list tells Game Wizard which template modules to load — here we load the `core` module which provides the `dialog-cutscene`, `menu`, and `landmark-overworld` templates. The `initial-scene` field tells the engine which scene to load first. Each entry under `scenes` pairs a scene ID with a template and a config file path.

## 5. Create Your First Cutscene

Create `scenes/intro.yaml`. This is a **dialog cutscene config** — the YAML format specific to the `core.dialog-cutscene` template that controls what characters appear, what they say, and how the dialog flows.

```yaml
initial-sequence: conversation

style:
  background: res://sprites/background.png
  banner:
    background: res://sprites/banner.png
    height: 50
  characters:
    outer-margin: 50
  dialog-box:
    background: res://sprites/dialog-box.png
    height: 300
    text-margin: ( 30 20 )

characters:
  hero:
    sprite: res://sprites/hero.png

sequences:

  conversation:
    transitions:
      - action: end
    frames:
      - type: add-character
        side: left
        character: hero
      - type: set-text
        character: hero
        text: |
          [b]Hero:[/b] Hello! Welcome to my game.
      - type: set-text
        character: hero
        text: |
          [b]Hero:[/b] Let me show you around.
```

The `style` section controls the visual layout — background images and sizing for the banner, character area, and dialog box. The `characters` section declares the character sprites available in this scene. The `sequences` section contains the actual dialog flow.

Each sequence has a list of `frames` — individual dialog actions — and a list of `transitions` that determine what happens when all frames have been shown. Here, the `conversation` sequence has an `add-character` frame (which runs automatically), followed by two `set-text` frames (which each pause and wait for the player to press `advance`). When the frames are exhausted, the `end` transition action fires, which emits a `terminal-frame.conversation` output from the scene.

> **Tip:** You'll need actual image files at the `res://` paths above. For quick testing, you can use any PNG images or borrow the sprites from the [visual novel example](../examples/visual-novel/sprites/).

## 6. Add a World Map

Create `scenes/world-map.yaml`. This is a **landmark overworld config** — the YAML format for the `core.landmark-overworld` template that defines a map with clickable locations.

```yaml
map:
  sprite: res://sprites/world-map.png
  scaling: fit-width

landmarks:

  npc-house:
    sprite: res://sprites/landmark-icon.png
    size: ( 200 200 )
    offset: ( -0.3 0 )
```

The `map` section specifies the background image and how it should be scaled to the viewport. The `landmarks` section defines the clickable buttons on the map. Each landmark has a sprite, a size, and an `offset` that positions it relative to the center of the screen. When clicked, the landmark emits a `navigate` output with the landmark ID as the argument — so clicking `npc-house` emits `navigate.npc-house`.

## 7. Connect the Intro to the Map

Now update `game.yaml` to add the world map scene and a transition connecting the intro to it:

```yaml
name: My First Game

modules:
  - res://addons/game-wizard/modules/core/config.yaml

initial-scene: intro

scenes:

  intro:
    template: core.dialog-cutscene
    config: res://scenes/intro.yaml
    transitions:
      - edge: terminal-frame.conversation move to world-map

  world-map:
    template: core.landmark-overworld
    config: res://scenes/world-map.yaml
```

The `transitions` list on a scene defines what happens when the scene emits an output. Each transition has an `edge` string — here, `terminal-frame.conversation move to world-map` means "when the intro emits `terminal-frame` with argument `conversation`, unload the intro and load the world map." The `move to` action navigates to a sibling scene, replacing the current one.

## 8. Add an NPC Dialog with a Choice

Create `scenes/npc-dialog.yaml`. This dialog cutscene introduces a new concept: **interludes**, which let a dialog pause and hand off control to a child scene (like a choice menu), then resume based on the result.

```yaml
initial-sequence: greeting

style:
  background: res://sprites/background.png
  banner:
    background: res://sprites/banner.png
    height: 50
  characters:
    outer-margin: 50
  dialog-box:
    background: res://sprites/dialog-box.png
    height: 300
    text-margin: ( 30 20 )

characters:
  hero:
    sprite: res://sprites/hero.png
  npc:
    sprite: res://sprites/npc.png

interludes:
  question:
    transitions:
      - source: terminal-select.yes
        action: start happy-response
      - source: terminal-select.no
        action: start sad-response

sequences:

  greeting:
    transitions:
      - action: interlude question
    frames:
      - type: add-character
        side: left
        character: hero
      - type: add-character
        side: right
        character: npc
      - type: set-text
        character: npc
        text: |
          [b]NPC:[/b] Hello traveler! Would you like a flag?

  happy-response:
    transitions:
      - action: end
    frames:
      - type: set-text
        character: npc
        text: |
          [b]NPC:[/b] Wonderful! Here you go!

  sad-response:
    transitions:
      - action: end
    frames:
      - type: set-text
        character: npc
        text: |
          [b]NPC:[/b] Oh... maybe next time then.
```

The `greeting` sequence ends with `action: interlude question`, which emits a `dialog-interlude.question` output. In `game.yaml`, we'll connect this to a choice menu. When that menu closes, the interlude's `transitions` check the `source` — the output that caused the child to end — and route to the appropriate response sequence. The `source` value matches the `<output-id>.<output-arg>` from the child scene's closing edge.

## 9. Wire Up the Choice Menu

Create `scenes/npc-choice.yaml`. This is a **menu config** — the YAML format for the `core.menu` template that defines a list of clickable options.

```yaml
initial-page: dialog-choice

style:
  background: res://sprites/dark-overlay.png
  options:
    sprite: res://sprites/button.png
    size: ( 800 200 )

pages:
  dialog-choice:
    cancel-action: none
    options:
      yes:
        action: end
        label: "Yes please!"
      no:
        action: end
        label: "No thanks."
```

The `style` section defines the look of the menu. Menus are organized into `pages`, each with a list of `options`. Each option has a `label` (the text shown on the button) and an `action`. The action `end` emits a `terminal-select` output with the option ID as the argument — so clicking "Yes please!" emits `terminal-select.yes`. The `cancel-action` controls what happens when the cancel input is pressed; `none` means nothing.

Now update `game.yaml` to connect the NPC dialog and the choice menu:

```yaml
name: My First Game

modules:
  - res://addons/game-wizard/modules/core/config.yaml

initial-scene: intro

scenes:

  intro:
    template: core.dialog-cutscene
    config: res://scenes/intro.yaml
    transitions:
      - edge: terminal-frame.conversation move to world-map

  world-map:
    template: core.landmark-overworld
    config: res://scenes/world-map.yaml
    transitions:
      - edge: navigate.npc-house spawn npc-dialog

  npc-dialog:
    template: core.dialog-cutscene
    config: res://scenes/npc-dialog.yaml
    transitions:
      - edge: dialog-interlude.question spawn npc-choice
      - edge: terminal-frame.happy-response ends
      - edge: terminal-frame.sad-response ends

  npc-choice:
    template: core.menu
    config: res://scenes/npc-choice.yaml
    transitions:
      - edge: terminal-select.yes ends
      - edge: terminal-select.no ends
```

Two new edge types appear here. The `spawn` action loads a child scene on top of the current one — the world map spawns the NPC dialog, and the dialog spawns the choice menu. The `ends` action unloads the current scene and returns focus to the parent below it. This creates a stack: the world map stays loaded while the dialog is on top, and the dialog stays loaded while the choice menu is on top.

The full flow is now:
1. Intro cutscene → `move to` world map
2. Click NPC landmark → `spawn` NPC dialog (map stays loaded underneath)
3. Dialog reaches interlude → `spawn` choice menu (dialog stays loaded underneath)
4. Player picks an option → choice menu `ends` back to dialog
5. Dialog interlude routes to the appropriate response sequence
6. Response sequence finishes → dialog `ends` back to the world map

## 10. Add Game State

Let's track whether the player said yes or no using a **flag** — a named value that can be set to one of a predefined list of strings. Flags are declared in the `state` section of `game.yaml` and can be read by conditions and modified by state updates on transitions.

```yaml
name: My First Game

modules:
  - res://addons/game-wizard/modules/core/config.yaml

initial-scene: intro

state:
  flags:
    npc-quest:
      values: [ unseen, accepted, declined ]
      initial-value: unseen

scenes:

  intro:
    template: core.dialog-cutscene
    config: res://scenes/intro.yaml
    transitions:
      - edge: terminal-frame.conversation move to world-map

  world-map:
    template: core.landmark-overworld
    config: res://scenes/world-map.yaml
    transitions:
      - edge: navigate.npc-house spawn npc-dialog

  npc-dialog:
    template: core.dialog-cutscene
    config: res://scenes/npc-dialog.yaml
    transitions:
      - edge: dialog-interlude.question spawn npc-choice
      - edge: terminal-frame.happy-response ends
      - edge: terminal-frame.sad-response ends

  npc-choice:
    template: core.menu
    config: res://scenes/npc-choice.yaml
    transitions:
      - edge: terminal-select.yes ends
        updates:
          - set flag npc-quest to accepted
      - edge: terminal-select.no ends
        updates:
          - set flag npc-quest to declined
```

The `state.flags` section declares a flag called `npc-quest` that starts as `unseen` and can be set to `accepted` or `declined`. The `updates` list on each transition specifies state changes that happen when that transition fires — `set flag npc-quest to accepted` sets the flag to the new value.

Now update `scenes/world-map.yaml` to use a **condition** to hide the NPC landmark after the player interacts with them:

```yaml
map:
  sprite: res://sprites/world-map.png
  scaling: fit-width

landmarks:

  npc-house:
    sprite: res://sprites/landmark-icon.png
    size: ( 200 200 )
    offset: ( -0.3 0 )
    when:
      - flag npc-quest in [unseen]
```

The `when` clause is a list of conditions that must all be true for the landmark to be visible. The condition `flag npc-quest in [unseen]` evaluates to true only when the flag's current value is `unseen`. Once the player makes a choice and the flag changes, the landmark disappears.

## 11. Add an Ending

Create `scenes/ending.yaml`:

```yaml
initial-sequence: ending

style:
  background: res://sprites/background.png
  banner:
    background: res://sprites/banner.png
    height: 50
  characters:
    outer-margin: 50
  dialog-box:
    background: res://sprites/dialog-box.png
    height: 300
    text-margin: ( 30 20 )

characters:
  hero:
    sprite: res://sprites/hero.png

sequences:
  ending:
    transitions:
      - action: start done
    frames:
      - type: add-character
        side: left
        character: hero
      - type: set-text
        character: hero
        text: |
          [b]Hero:[/b] Well, that was an adventure. Time to go home!

  done:
    transitions:
      - action: end
```

Add an ending landmark to `scenes/world-map.yaml` that only appears after the NPC interaction:

```yaml
landmarks:

  npc-house:
    sprite: res://sprites/landmark-icon.png
    size: ( 200 200 )
    offset: ( -0.3 0 )
    when:
      - flag npc-quest in [unseen]

  ending:
    sprite: res://sprites/landmark-icon.png
    size: ( 200 200 )
    offset: ( 0.3 0 )
    when:
      - flag npc-quest in [accepted,declined]
```

And add the ending scene and transitions to `game.yaml`:

```yaml
  world-map:
    template: core.landmark-overworld
    config: res://scenes/world-map.yaml
    transitions:
      - edge: navigate.npc-house spawn npc-dialog
      - edge: navigate.ending spawn ending

  ending:
    template: core.dialog-cutscene
    config: res://scenes/ending.yaml
    transitions:
      - edge: terminal-frame.done quit
```

The `quit` edge action immediately exits the application when the ending cutscene finishes.

## 12. Configure Input Actions

Game Wizard templates need Godot input actions to be defined. Go to **Project → Project Settings → Input Map** and add these actions:

| Action Name | Suggested Key Binding |
|---|---|
| `core.dialog-cutscene.advance` | Space |
| `core.dialog-cutscene.skip` | Escape |
| `core.menu.select` | (none needed — mouse clicks work) |
| `core.menu.cancel` | Escape |
| `core.landmark-overworld.select` | (none needed — mouse clicks work) |

The action names follow the pattern `<module-id>.<template-id>.<input-name>` and must match exactly. Each module's README documents the input actions its templates require.

## 13. Run Your Game

Make sure your root scene (with the `GameController` node) is set as the main scene, and hit Play!

The game flow will be:
1. **Intro cutscene** — press Space to advance through dialog
2. **World map** — click the NPC landmark
3. **NPC dialog** — press Space to advance, then a choice menu appears
4. **Choice menu** — click an option
5. **NPC response** — press Space to see the response, then return to the map
6. **World map** — the NPC is gone, click the ending landmark
7. **Ending cutscene** — press Space, then the game quits

## 14. Next Steps

Now that you have a working game, here are some things to explore:

- **Add more NPCs:** Add more landmarks, dialog scenes, and choices. See the [visual novel example](../examples/visual-novel/) for a game with three NPCs and branching outcomes.
- **Conditional transitions:** Use `when` clauses on transitions in `game.yaml` to create different outcomes based on earlier choices. When multiple transitions match the same output, the first one whose conditions pass wins — put more specific transitions before less specific ones.
- **Multi-page menus:** The menu template supports `load <page-id>` actions for navigating between pages within a single menu.
- **Dialog branching:** Use routing sequences (sequences with no frames, only conditional transitions) to branch dialog flow based on game state. See how the example uses `route-coversation` sequences.
- **Parent/child scenes:** The `spawn` / `ends` pattern lets you layer scenes. A dialog scene can spawn a menu on top of it, get the result back via an interlude, and continue.

For complete references, see:
- [Game Config Reference](yaml-game-config.md) — the `game.yaml` specification
- [Module Config Reference](yaml-module-config.md) — the module `config.yaml` specification and how to create your own modules
- [Core Module README](../modules/core/README.md) — full reference for all core module templates
- [Core Concepts](core-concepts.md) — architecture deep dive
