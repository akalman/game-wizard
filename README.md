# Game Wizard

A Godot 4.6+ asset pack for scene orchestration. Define your game's scenes, transitions, and state entirely in YAML — Game Wizard handles the scene lifecycle, input routing, and state management so you can focus on your game's content.

Game Wizard is built around **modules** that provide reusable **scene templates**. The included `core` module ships with templates for dialog cutscenes, menus, and landmark-based overworld maps. You wire them together in a single `game.yaml` file that describes your entire game flow as a graph of scenes and transitions.

## Features

- **YAML-driven game flow** — Define scenes, transitions, conditions, and state updates without writing orchestration code.
- **Modular template system** — Reuse scene templates across your game with different configurations.
- **Scene graph with focus stack** — Parent/child/sibling scene relationships with automatic Z-ordering and focus management.
- **State management** — Track game progress with flags, evaluate conditions, and apply state updates on transitions.
- **Extensible module architecture** — Create your own modules and templates with custom YAML schemas.

## Requirements

- Godot 4.6+ with C# / .NET support
- .NET 8.0+

## Quick Start

1. Download the latest release from [github.com/akalman/game-wizard](https://github.com/akalman/game-wizard) (**Code → Download ZIP**).
2. Upzip the package into your `addons` directory.  You should be left with an `addons/game-wizard` folder.
3. Run `dotnet restore` in your project directory to install dependencies.
4. Create a root 2D Godot scene and attach the `addons/game-wizard/engine/GameController.cs` script to the root.
5. Set the `GameConfigPath` export on the `GameController` to point to your `game.yaml`.
6. Define your game in YAML — see the [Getting Started Guide](docs/getting-started.md).

For a complete working example, see the [example visual novel](examples/visual-novel/) included in this repository.

## Documentation

| Document | Description |
|---|---|
| [Getting Started](docs/getting-started.md) | Step-by-step tutorial for building your first game |
| [Core Concepts](docs/core-concepts.md) | Architecture overview — scenes, transitions, state, and modules |
| [Game Config Reference](docs/yaml-game-config.md) | Full specification for `game.yaml` |
| [Module Config Reference](docs/yaml-module-config.md) | Module `config.yaml` specification and how to create your own modules |
| [Core Module README](modules/core/README.md) | Reference for the built-in `dialog-cutscene`, `menu`, and `landmark-overworld` templates |

## License

See [LICENSE](LICENSE) for details.
