# Module Config Reference (`config.yaml`)

This document covers the specification for module configuration files and how to create your own module locally within your project.

## Table of Contents

- [What is a Module?](#what-is-a-module)
- [Module Config Schema](#module-config-schema)
- [Fields](#fields)
- [Templates](#templates)
- [Template Outputs](#template-outputs)
- [Creating Your Own Module](#creating-your-own-module)
  - [Directory Structure](#directory-structure)
  - [Write the Module Config](#write-the-module-config)
  - [Create the Godot Scene](#create-the-godot-scene)
  - [Create the Template Controller](#create-the-template-controller)
  - [Create the Plugins Scene](#create-the-plugins-scene)
  - [Load the Module](#load-the-module)

---

## What is a Module?

A module is a package that bundles one or more reusable scene templates together with their Godot scenes, C# controllers, and YAML plugins. Modules are loaded in your `game.yaml` to make their templates available.

Game Wizard ships with the [core module](../modules/core/README.md). You can also create your own modules locally within your project to define custom scene types.

## Module Config Schema

Each module has a `config.yaml` at its root:

```yaml
id: <module-id>
plugins: <path-to-plugins.tscn>

templates:
  <template-id>:
    scene: <path-to-template.tscn>
    inputs: [ <input-name>, ... ]
    outputs:
      <output-id>:
        default: <edge-type>
        allowed: [ <edge-type>, ... ]
```

All paths within the module config are **relative to the directory containing the config file**.

## Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | string | Yes | A unique identifier for the module. Used as a prefix when referencing templates (e.g., `core.dialog-cutscene`). |
| `plugins` | string | Yes | Relative path to a `.tscn` file containing the module's plugin nodes. |
| `templates` | map | Yes | The templates provided by this module. |

## Templates

Each template entry defines a reusable scene type.

| Field | Type | Required | Description |
|---|---|---|---|
| `scene` | string | Yes | Relative path to the Godot `.tscn` file for this template. |
| `inputs` | list of strings | No | Named inputs the template accepts. These map to Godot input actions named `<module-id>.<template-id>.<input-name>`. |
| `outputs` | map | Yes | Named outputs the template can emit. |

## Template Outputs

Each output defines an event the template can fire, along with rules about what edge types are valid for it.

| Field | Type | Required | Description |
|---|---|---|---|
| `default` | edge type | Yes | The default edge type used when a transition isn't explicitly defined for this output. |
| `allowed` | list of edge types | Yes | The edge types that are valid for this output. Transitions using an unlisted type will cause an error. |

**Edge types** (used in `default` and `allowed`):

| Value | Description |
|---|---|
| `to-sibling` | Navigate to a sibling scene (`move to`) |
| `to-child` | Spawn a child scene (`spawn`) |
| `to-parent` | Return to the parent scene (`ends`) |
| `to-self` | Stay on the current scene (`do nothing`) |

### Example

Here is the core module's config as a reference:

```yaml
id: core
plugins: plugins.tscn

templates:

  menu:
    scene: menu/menu.tscn
    inputs: [ select, cancel ]
    outputs:
      terminal-select:
        default: to-parent
        allowed: [ to-sibling, to-parent ]
      closed:
        default: to-parent
        allowed: [ to-sibling, to-parent ]

  dialog-cutscene:
    scene: dialog-cutscene/dialog-cutscene.tscn
    inputs: [ advance, skip ]
    outputs:
      dialog-interlude:
        default: to-parent
        allowed: [ to-sibling, to-child, to-parent ]
      terminal-frame:
        default: to-parent
        allowed: [ to-sibling, to-parent ]

  landmark-overworld:
    scene: landmark-overworld/landmark-overworld.tscn
    inputs: [ select ]
    outputs:
      navigate:
        default: to-self
        allowed: [ to-sibling, to-child, to-parent, to-self ]
```

---

## Creating Your Own Module

You can create custom modules locally within your project. A module needs a config file, a Godot scene for each template, a C# controller for each template, and a plugins scene.

### Directory Structure

Create a directory for your module. A typical layout looks like:

```
your-project/
  modules/
    my-module/
      config.yaml
      plugins.tscn
      my-template/
        my-template.tscn
        MyTemplateController.cs
        schema/
          MyTemplateConfig.cs
```

### Write the Module Config

Create `modules/my-module/config.yaml`:

```yaml
id: my-module
plugins: plugins.tscn

templates:
  my-template:
    scene: my-template/my-template.tscn
    inputs: [ confirm ]
    outputs:
      finished:
        default: to-parent
        allowed: [ to-sibling, to-parent ]
```

This declares a module with ID `my-module` containing one template called `my-template`. The template accepts a `confirm` input and can emit a `finished` output.

### Create the Godot Scene

Create a Godot scene at `modules/my-module/my-template/my-template.tscn` with a **Node2D** root. You'll attach your controller script to this root node.

The scene can contain any child nodes your template needs (sprites, labels, containers, etc.). Use `[Export]` properties on the controller to bind them.

### Create the Template Controller

Create a C# class that extends `TemplateController<T>`, where `T` is your config schema class:

```csharp
using System.Collections.Generic;
using GameWizard.Engine;

namespace MyProject.MyModule;

public partial class MyTemplateController : TemplateController<MyTemplateConfig>
{
    protected override void InitializeScene()
    {
        // Read from Config (your deserialized YAML) and set up the scene
    }

    public override bool HandleInput(IDictionary<string, bool> inputs)
    {
        if (inputs["confirm"])
        {
            EmitOutput("finished", "done");
            return true;  // input was consumed
        }
        return false;
    }

    public override void HandleFocus(string sourceScene, string outputId)
    {
        // Called when this scene regains focus after a child scene ends
    }
}
```

Key methods:
- **`InitializeScene()`** — Called once when the scene is loaded. Use `Config` to access the deserialized YAML config.
- **`HandleInput(inputs)`** — Called each frame with the state of all declared inputs. Return `true` if the input was consumed.
- **`HandleFocus(sourceScene, outputId)`** — Called when a child scene ends and this scene regains focus.
- **`EmitOutput(outputId, outputArg)`** — Call this to fire an output event, which triggers transitions in `game.yaml`.

Your config schema class is a plain C# class that YamlDotNet will deserialize into:

```csharp
namespace MyProject.MyModule;

public class MyTemplateConfig
{
    public string Title { get; set; }
    public string Message { get; set; }
}
```

### Create the Plugins Scene

Create a Godot scene at `modules/my-module/plugins.tscn` with a **Node2D** root. If your template config uses custom YAML types that need special deserialization, add child nodes with scripts extending `PluginController` and override `RegisterDeserializer` to register your custom `IYamlTypeConverter` implementations.

If you don't need custom YAML deserialization, the plugins scene can be empty (just a root Node2D with no children).

### Load the Module

Add your module to the `modules` list in your `game.yaml`:

```yaml
modules:
  - res://addons/game-wizard/modules/core/config.yaml
  - res://modules/my-module/config.yaml
```

Then use it in your scenes:

```yaml
scenes:
  my-scene:
    template: my-module.my-template
    config: res://scenes/my-scene-config.yaml
```

Create the scene config YAML file with fields matching your `MyTemplateConfig` schema. Game Wizard uses YamlDotNet with hyphenated naming convention, so a C# property named `MyField` maps to `my-field` in YAML.
