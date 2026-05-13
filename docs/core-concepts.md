# Core Concepts

This document explains the architecture and mental model behind Game Wizard. Understanding these concepts will help you design your game's YAML configuration effectively.

## Table of Contents

- [The Scene Graph](#the-scene-graph)
- [Modules and Templates](#modules-and-templates)
- [Outputs and Transitions](#outputs-and-transitions)
- [Edge Types](#edge-types)
- [State](#state)
- [Conditions](#conditions)
- [State Updates](#state-updates)
- [Input Routing](#input-routing)

---

## The Scene Graph

Game Wizard models your game as a **graph of scenes**. Each scene is a named node in the graph, and **transitions** are the directed edges between them. At runtime, the engine maintains a stack of loaded scenes and routes events between them.

You never write code to load or unload scenes. Instead, you declare scenes and their transitions in your `game.yaml`, and the engine handles the lifecycle automatically.

```
┌──────────────┐   navigate.shopkeep    ┌───────────────────┐
│  world-map   │ ────── spawn ────────> │ shopkeep-cutscene │
│  (overworld) │ <───── ends ────────── │   (dialog)        │
└──────────────┘                        └───────────────────┘
```

## Modules and Templates

A **module** is a package that bundles one or more reusable scene templates together with their controllers, plugins, and YAML schemas. You load modules in your `game.yaml` to make their templates available to your game. Each module has its own `config.yaml` and a README documenting the templates it provides.

A **template** is a reusable scene type defined within a module. It specifies the Godot scene file, the inputs it accepts, and the outputs it can emit. You create a scene in your game by picking a template and providing a YAML config file with template-specific settings:

```yaml
scenes:
  my-cutscene:
    template: core.dialog-cutscene
    config: res://scenes/my-cutscene.yaml
```

The template's controller reads that config file and uses it to drive the scene's behavior internally.

Game Wizard ships with the `core` module, which provides three templates for dialog cutscenes, menus, and landmark-based overworld maps. See the [Core Module README](../modules/core/README.md) for the full reference on the templates it provides, including their inputs, outputs, and YAML config formats.

## Outputs and Transitions

Every template defines **outputs** — named events that can cause another scene to be loaded or unloaded. You don't handle these outputs in code. Instead, you define **transitions** in `game.yaml` that react to them:

```yaml
my-cutscene:
  template: core.dialog-cutscene
  config: res://scenes/my-cutscene.yaml
  transitions:
    - edge: terminal-frame.done move to world-map
    - edge: dialog-interlude.question spawn my-choice-menu
```

Each transition has an **edge** that specifies the output to match and what to do next. See [Edge Types](#edge-types) for all the available actions.

## Edge Types

Edges define what happens when a scene emits an output. Each edge string specifies which output to match and what scene navigation action to take.

An edge string follows the general pattern `<output-id>.<output-arg> <action> [destination]`, where the output ID and argument identify the event, and the action determines the navigation behavior.

### `move to`

Unloads the current scene and loads the destination scene. Both scenes are at the same level in the scene stack.

**Format:** `<output-id>.<output-arg> move to <destination-scene-id>`

```yaml
- edge: terminal-frame.done move to world-map
```

### `spawn`

Loads the destination scene on top of the current scene. The current scene stays loaded underneath and will regain focus when the child ends.

**Format:** `<output-id>.<output-arg> spawn <destination-scene-id>`

```yaml
- edge: dialog-interlude.question spawn choice-menu
```

### `ends`

Unloads the current scene and returns focus to the scene below it on the stack. No destination is specified.

**Format:** `<output-id>.<output-arg> ends`

```yaml
- edge: terminal-select.success ends
```

### `do nothing`

Matches the output but doesn't change any scenes. Useful when you only want to apply state updates without navigating.

**Format:** `<output-id>.<output-arg> do nothing`

```yaml
- edge: navigate.button do nothing
  updates:
    - set flag button-pressed to "true"
```

### `quit`

Immediately quits the application. No destination is specified.

**Format:** `<output-id>.<output-arg> quit`

```yaml
- edge: terminal-frame.end quit
```

## State

Game Wizard provides a built-in state system for tracking game progress. State is declared in the `state` section of your `game.yaml`, initialized when the game starts, and persists across scene transitions. State values can be read by [conditions](#conditions) and modified by [state updates](#state-updates).

### Flags

A **flag** is a named value that can be set to one of a predefined list of strings. Flags are useful for tracking story progress, NPC interaction states, and any game state that can be represented as a bounded set of values.

```yaml
state:
  flags:
    quest-status:
      values: [ not-started, in-progress, complete ]
      initial-value: not-started
```

- Flag values are always strings.
- The `initial-value` must be one of the entries in `values`.
- Flag names must be unique across your entire game.

### Counts

_Not yet implemented. Counts will provide integer state values with increment/decrement operations._

### Bags

_Not yet implemented. Bags will provide unordered collection state for tracking sets of items._

## Conditions

Conditions are expressions that evaluate against the current game state. They are used in transition `when` clauses, landmark visibility rules, and dialog sequence routing to conditionally control game flow.

Multiple conditions in a `when` list are combined with **AND** logic — all conditions must be true for the block to match.

### `flag ... in`

Checks if a flag's current value is a member of an expected set of values. The condition is true if the flag's current value matches **any** of the listed values.

**Format:** `flag <flag-name> in [<value>,<value>,...]`

```yaml
when:
  - flag quest-status in [not-started,in-progress]
```

No spaces between values inside the brackets.

## State Updates

State updates modify game state when a transition fires. They are specified in the `updates` list on a transition and run after the scene navigation completes.

### `set flag`

Sets a flag to a new value. The value must be one of the values declared in the flag's `values` list.

**Format:** `set flag <flag-name> to <value>`

```yaml
updates:
  - set flag quest-status to complete
```

## Input Routing

Each template declares named **inputs** in its module config. These map to Godot input actions using the naming convention:

```
<module-id>.<template-id>.<input-name>
```

For example, the input actions for the core module's dialog-cutscene template are:
- `core.dialog-cutscene.advance`
- `core.dialog-cutscene.skip`

You must define these input actions in your Godot project settings (Project → Project Settings → Input Map) and bind them to keys, buttons, or other input events. Each module's README documents the required input actions for its templates.
