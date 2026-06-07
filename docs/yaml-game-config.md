# Game Config Reference (`game.yaml`)

This is the complete specification for your game's top-level configuration file. This file is the entry point for Game Wizard — it defines what modules to load, what state to track, and how all your scenes connect together.

## Table of Contents

- [Full Schema](#full-schema)
- [Top-Level Fields](#top-level-fields)
- [Modules](#modules)
- [State](#state)
- [Scenes](#scenes)
- [Transitions](#transitions)
- [Edge Syntax](#edge-syntax)
- [Conditions (`when`)](#conditions-when)
- [State Updates (`updates`)](#state-updates-updates)

---

## Full Schema

```yaml
name: <string>
initial-scene: <string#scene-id>

modules:
  - <string>

state:
  flags:
    <string#flag-id>:
      values: [ <string>, <string>, ... ]
      initial-value: <string>

scenes:
  <string#scene-id>:
    template: <string#template-id>
    config: <string>
    transitions:
      - edge: <transition-edge>
        when:
          - <condition>
        updates:
          - <state-update>
```

## Top-Level Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string | Yes | A human-readable name for your game. |
| `initial-scene` | string | Yes | The ID of the first scene to load when the game starts. Must match a key in `scenes`. |
| `modules` | list of strings | Yes | Paths to module config files to load. Uses Godot `res://` paths. |
| `state` | object | No | Game state definitions. See [State](#state). |
| `scenes` | map | Yes | All scenes in the game. See [Scenes](#scenes). |

## Modules

The `modules` list tells Game Wizard which template modules to load. Each entry is a `res://` path to a module's `config.yaml` file.

```yaml
modules:
  - res://addons/game-wizard/modules/core/config.yaml
```

The core module is included with Game Wizard and provides the `dialog-cutscene`, `menu`, and `landmark-overworld` templates. You reference templates from loaded modules using the format `<string#template-id>` (e.g., `core.dialog-cutscene`).

For information on the module config format and how to create your own modules, see the [Module Config Reference](yaml-module-config.md).

## State

The `state` section defines the game state that persists across scene transitions. State is initialized when the game starts and can be read by conditions and modified by updates.

### Flags

Flags are the primary state primitive. Each flag has a name, a list of allowed values, and an initial value.

```yaml
state:
  flags:
    door-status:
      values: [ locked, unlocked, open ]
      initial-value: locked

    has-key:
      values: [ "true", "false" ]
      initial-value: "false"
```

**Rules:**
- Flag values are always strings.
- The `initial-value` must be one of the entries in `values`.
- Flag names must be unique across your entire game.

### Counts

_Not yet implemented._

### Bags

_Not yet implemented._

## Scenes

Each scene is a named entry in the `scenes` map. The key is the **scene ID** — you use this ID to reference the scene in transitions and as the `initial-scene`.

```yaml
scenes:
  my-scene:
    template: core.dialog-cutscene
    config: res://scenes/my-scene.yaml
    always-active: false
    transitions:
      - edge: terminal-frame.done move to next-scene
```

| Field | Type | Required | Description |
|---|---|---|---|
| `template` | string | Yes | The template to use, in `<string#template-id>` format. |
| `config` | string | Yes | `res://` path to the template-specific YAML config file for this scene. |
| `always-active` | bool | No | If `true`, this scene continues to receive input even when it is not focused. Default: `false`. |
| `transitions` | list | No | Transition rules that define what happens when the scene emits outputs. |

## Transitions

Transitions define how the game responds when a scene emits an output. Each transition has an **edge** (required), an optional list of **conditions** (`when`), and an optional list of **state updates** (`updates`).

```yaml
transitions:
  - edge: navigate.shop spawn shop-cutscene
    when:
      - flag shop-status in [open]
    updates:
      - set flag shop-status to visited
```

When a scene emits an output, the engine iterates through the scene's transitions **in order** and selects the **first** transition whose edge matches the output and whose conditions all evaluate to true. **Order matters** — put more specific transitions (with conditions) before less specific ones.

## Edge Syntax

A `<transition-edge>` string defines what output to match and what scene navigation action to take. The general format is `<output> <action> [destination]`.

### `move to`

Unloads the current scene and loads the destination scene.

**Format:** `<output> move to <destination-scene-id>`

```yaml
- edge: terminal-frame.done move to world-map
```

### `spawn`

Loads the destination scene on top of the current scene. The current scene stays loaded underneath.

**Format:** `<output> spawn <destination-scene-id>`

```yaml
- edge: dialog-interlude.question spawn choice-menu
```

### `ends`

Unloads the current scene and returns focus to the scene below it on the stack.

**Format:** `<output> ends`

```yaml
- edge: terminal-select.success ends
```

### `do nothing`

Matches the output but doesn't change any scenes. Useful when you only want to apply state updates.

**Format:** `<output> do nothing`

```yaml
- edge: navigate.button do nothing
  updates:
    - set flag button-pressed to "true"
```

### `quit`

Immediately quits the application.

**Format:** `<output> quit`

```yaml
- edge: terminal-frame.end quit
```

## Conditions (`when`)

Conditions gate whether a transition is eligible. All conditions in a `when` list must be true for the transition to match (AND logic).

### `flag ... in`

Checks if a flag's current value is a member of an expected set of values. The condition is true if the flag's current value matches **any** of the listed values.

**Format:** `flag <flag-id> in [<value>,<value>,...]`

```yaml
when:
  - flag quest-status in [complete]
  - flag has-key in [true]
```

No spaces between values inside the brackets.

## State Updates (`updates`)

State updates modify game state when a transition fires. They run after the scene transition completes.

### `set flag`

Sets a flag to a new value. The value must be one of the values declared in the flag's `values` list.

**Format:** `set flag <flag-id> to <value>`

```yaml
updates:
  - set flag quest-status to complete
```
