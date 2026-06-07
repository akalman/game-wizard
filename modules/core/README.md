# Core Module

The `core` module ships with Game Wizard and provides three scene templates for common game patterns.

Load the core module in your `game.yaml`:

```yaml
modules:
  - res://addons/game-wizard/modules/core/config.yaml
```

## Templates

| Template ID | Description |
|---|---|
| `core.dialog-cutscene` | Sequential dialog with characters, branching sequences, and interludes |
| `core.menu` | Multi-page option menus for player choices |
| `core.landmark-overworld` | Map screen with clickable landmark locations |

---

## Dialog Cutscene (`core.dialog-cutscene`)

The dialog cutscene template displays sequential dialog frames with character sprites, text boxes, and branching narrative flow. It supports multiple sequences, routing between them, and interludes that hand off control to child scenes (such as menus for player choices).

### Inputs

| Input | Godot Action | Description |
|---|---|---|
| `advance` | `core.dialog-cutscene.advance` | Advance to the next dialog frame. |
| `skip` | `core.dialog-cutscene.skip` | Skip ahead (not yet implemented). |

### Outputs

| Output ID | Argument | Description |
|---|---|---|
| `terminal-frame` | The sequence ID that ended | Emitted when a sequence transition uses `end`. |
| `dialog-interlude` | The interlude ID | Emitted when a sequence transition uses `interlude <id>`. |

### Config Schema

The config YAML for a `core.dialog-cutscene` scene has the following structure:

```yaml
initial-sequence: <sequence-id>

style:
  background: <res:// path>                       # optional
  banner:
    background: <res:// path>                     # optional
    height: <int>
  characters:
    outer-margin: <int>                           # optional, default: 0
  dialog-box:
    background: <res:// path>                     # optional
    height: <int>
    text-margin: ( <x> <y> )                      # optional

characters:
  <character-id>:
    sprite: <res:// path>

interludes:                                        # optional
  <interlude-id>:
    transitions:
      - source: <output-id>.<output-arg>
        action: <transition-action>
        when:                                      # optional
          - <condition>

sequences:
  <sequence-id>:
    transitions:
      - action: <transition-action>
        when:                                      # optional
          - <condition>
    frames:                                        # optional
      - <dialog-frame>
```

### Style

The `style` section controls the visual layout of the dialog scene.

| Field | Type | Description |
|---|---|---|
| `background` | string | `res://` path to a background image. Covers the entire screen. |
| `banner.background` | string | `res://` path to the banner background image (top area of the screen). |
| `banner.height` | int | Height of the banner area in pixels. |
| `characters.outer-margin` | int | Left and right margin around character sprites in pixels. |
| `dialog-box.background` | string | `res://` path to the dialog box background image. |
| `dialog-box.height` | int | Height of the dialog box area in pixels. |
| `dialog-box.text-margin` | Vector2 | Inner padding for text inside the dialog box. Format: `( <horizontal> <vertical> )`. |

### Characters

Declare all characters that appear in this dialog scene. Each character has an ID and a sprite path.

```yaml
characters:
  protagonist:
    sprite: res://sprites/protagonist.png
  villager:
    sprite: res://sprites/villager.png
```

Character IDs are used in dialog frames to add, remove, and attribute dialog text.

### Sequences

Sequences are the core building block of dialog flow. Each sequence contains an ordered list of **frames** (dialog actions) and a list of **transitions** (what to do when the frames run out).

The player advances through frames one at a time using the `advance` input. When all frames in a sequence have been shown, the engine evaluates the sequence's transitions to determine what happens next.

```yaml
sequences:
  introduction:
    transitions:
      - action: start main-conversation
    frames:
      - type: add-character
        side: left
        character: protagonist
      - type: set-text
        character: protagonist
        text: |
          [b]Hero:[/b] Hello, world!
```

### Dialog Frames

Frames are the individual actions within a sequence. There are three types:

#### `add-character`

Adds a character sprite to the screen. This frame is processed automatically without requiring player input.

```yaml
- type: add-character
  side: left
  character: protagonist
```

| Field | Value | Description |
|---|---|---|
| `type` | `add-character` | |
| `side` | `left` or `right` | Which side of the screen to place the character. Characters on the `right` are horizontally flipped. |
| `character` | string | The character ID as declared in the `characters` section. |

#### `remove-character`

Removes a character sprite from the screen. This frame is processed automatically without requiring player input.

```yaml
- type: remove-character
  character: protagonist
```

| Field | Value | Description |
|---|---|---|
| `type` | `remove-character` | |
| `character` | string | The character ID to remove. |

#### `set-text`

Displays text in the dialog box. This frame pauses and waits for the player to press the `advance` input before continuing.

```yaml
- type: set-text
  character: protagonist
  text: |
    [b]Hero:[/b] This is what I have to say.
    It can span multiple lines.
```

| Field | Value | Description |
|---|---|---|
| `type` | `set-text` | |
| `character` | string | The character ID speaking (for attribution). |
| `text` | string | The text to display. Supports Godot BBCode tags like `[b]`, `[i]`, `[color]`, etc. |

### Sequence Transitions

When all frames in a sequence have been displayed, the engine evaluates the sequence's `transitions` list in order. The first transition whose `when` conditions all pass (or that has no conditions) is selected.

#### Transition Actions

| Syntax | Description |
|---|---|
| `start <sequence-id>` | Jump to another sequence within this same dialog scene. |
| `interlude <interlude-id>` | Emit a `dialog-interlude` output, pausing the dialog to hand off control (e.g., to a choice menu). |
| `end` | Emit a `terminal-frame` output, signaling that this dialog is finished. |

#### Conditional Branching

Use `when` clauses to branch sequences based on game state. The first matching transition wins. A transition with no `when` clause always matches, so place it last as a fallback.

```yaml
sequences:
  route:
    transitions:
      - action: start first-visit
        when:
          - flag npc-status in [unseen]
      - action: start return-visit
```

### Interludes

Interludes handle the return path after a child scene (spawned via `dialog-interlude`) ends. When the child scene closes (via `ends`), the dialog cutscene receives the output that caused the child to end and uses the interlude's transitions to decide which sequence to continue with.

```yaml
interludes:
  choice:
    transitions:
      - source: terminal-select.option-a
        action: start option-a-response
      - source: terminal-select.option-b
        action: start option-b-response
```

| Field | Type | Description |
|---|---|---|
| `source` | string | The `<output-id>.<output-arg>` from the child scene's edge that caused it to `end`. |
| `action` | transition action | What to do next: `start <sequence>`, `interlude <interlude>`, or `end`. |
| `when` | list of conditions | Optional. Conditions that must be met for this transition to match. |

### Full Example

```yaml
initial-sequence: greeting

style:
  background: res://sprites/bg-dark.png
  banner:
    background: res://sprites/banner-bg.png
    height: 50
  characters:
    outer-margin: 50
  dialog-box:
    background: res://sprites/dialog-bg.png
    height: 350
    text-margin: ( 30 20 )

characters:
  hero:
    sprite: res://sprites/hero.png
  merchant:
    sprite: res://sprites/merchant.png

interludes:
  buy-choice:
    transitions:
      - source: terminal-select.buy
        action: start bought
      - source: terminal-select.decline
        action: start declined

sequences:

  greeting:
    transitions:
      - action: start first-visit
        when:
          - flag merchant-met in [false]
      - action: start return-visit
    frames: []

  first-visit:
    transitions:
      - action: interlude buy-choice
    frames:
      - type: add-character
        side: left
        character: hero
      - type: add-character
        side: right
        character: merchant
      - type: set-text
        character: merchant
        text: |
          [b]Merchant:[/b] Welcome! Would you like to buy a sword?

  return-visit:
    transitions:
      - action: interlude buy-choice
    frames:
      - type: add-character
        side: left
        character: hero
      - type: add-character
        side: right
        character: merchant
      - type: set-text
        character: merchant
        text: |
          [b]Merchant:[/b] Back again! Changed your mind?

  bought:
    transitions:
      - action: end
    frames:
      - type: set-text
        character: merchant
        text: |
          [b]Merchant:[/b] Great choice! Here you go!

  declined:
    transitions:
      - action: end
    frames:
      - type: set-text
        character: merchant
        text: |
          [b]Merchant:[/b] Maybe next time then!
```

---

## Menu (`core.menu`)

The menu template displays a list of clickable options organized into pages. It is commonly used for player choices during dialog interludes, title screens, pause menus, and settings screens.

### Inputs

| Input | Godot Action | Description |
|---|---|---|
| `select` | `core.menu.select` | Select/confirm the current option (button clicks work automatically). |
| `cancel` | `core.menu.cancel` | Cancel/back action for the current page. |

### Outputs

| Output ID | Argument | Description |
|---|---|---|
| `terminal-select` | The option ID that was selected | Emitted when an option with `action: end` is clicked. |
| `closed` | — | Emitted when the menu is closed via cancel (not yet fully implemented). |

### Config Schema

```yaml
initial-page: <page-id>

style:
  background: <res:// path>                       # optional
  options:
    sprite: <res:// path>
    size: ( <width> <height> )

pages:
  <page-id>:
    cancel-action: <menu-action>
    options:
      <option-id>:
        label: <string>
        action: <menu-action>
```

### Style

| Field | Type | Description |
|---|---|---|
| `background` | string | `res://` path to a background image for the menu. |
| `options.sprite` | string | `res://` path to the button background texture used for each option. |
| `options.size` | Vector2 | Size of each option button. Format: `( <width> <height> )`. |

### Pages and Options

Menus are organized into **pages**. Each page has a list of **options**. Each option has a label (displayed text) and an action (what happens when clicked).

```yaml
pages:
  main-menu:
    cancel-action: none
    options:
      start:
        label: Start Game
        action: end
      settings:
        label: Settings
        action: load settings-page

  settings-page:
    cancel-action: load main-menu
    options:
      back:
        label: Back
        action: load main-menu
```

| Field | Type | Description |
|---|---|---|
| `cancel-action` | menu action | What happens when the player presses the `cancel` input on this page. |
| `options` | map | The options displayed on this page. Keys are option IDs. |
| `options.<id>.label` | string | The display text for this option. |
| `options.<id>.action` | menu action | What happens when this option is selected. |

### Menu Actions

| Syntax | Description |
|---|---|
| `end` | Emit a `terminal-select` output with the option ID as the argument. |
| `load <page-id>` | Navigate to a different page within this menu. |
| `none` | Do nothing. |

### Full Example

```yaml
initial-page: dialog-choice

style:
  background: res://sprites/darken-overlay.png
  options:
    sprite: res://sprites/choice-button.png
    size: ( 800 200 )

pages:
  dialog-choice:
    cancel-action: none
    options:
      accept:
        action: end
        label: |
          "Yes, I'll help you!"
      decline:
        action: end
        label: |
          "Sorry, I'm busy."
```

---

## Landmark Overworld (`core.landmark-overworld`)

The landmark overworld template displays a map image with clickable landmark buttons positioned at specific coordinates. Landmarks can be conditionally shown or hidden based on game state, making it useful for world maps, level selectors, and hub areas.

### Inputs

| Input | Godot Action | Description |
|---|---|---|
| `select` | `core.landmark-overworld.select` | Select action (landmark clicks work automatically via mouse). |

### Outputs

| Output ID | Argument | Description |
|---|---|---|
| `navigate` | The landmark ID that was clicked | Emitted when the player clicks a visible landmark button. |

### Config Schema

```yaml
map:
  sprite: <res:// path>
  scaling: <scaling-mode>

landmarks:
  <landmark-id>:
    sprite: <res:// path>
    size: ( <width> <height> )
    offset: ( <x> <y> )
    when:                                          # optional
      - <condition>
```

### Map

| Field | Type | Description |
|---|---|---|
| `sprite` | string | `res://` path to the map background image. |
| `scaling` | string | How to scale the map image. See scaling modes below. |

**Scaling modes:**

| Value | Description |
|---|---|
| `actual-size` | Display at original resolution. |
| `fit-width` | Scale proportionally to fill the viewport width. |
| `fit-height` | Scale proportionally to fill the viewport height. |

### Landmarks

Each landmark is a clickable button placed on the map.

| Field | Type | Description |
|---|---|---|
| `sprite` | string | `res://` path to the landmark button texture. |
| `size` | Vector2 | Size of the landmark button. Format: `( <width> <height> )`. |
| `offset` | Vector2 | Position offset from screen center, normalized to viewport dimensions. Format: `( <x> <y> )`. See [Positioning](#positioning). |
| `when` | list of conditions | Optional. Conditions that must be met for the landmark to be visible. |

#### Positioning

Landmark positions use an **offset from center** system. The offset is multiplied by the half-viewport size `(960, 540)` for a 1920×1080 viewport:

- `( 0 0 )` = center of the screen
- `( -1 -1 )` = top-left corner
- `( 1 1 )` = bottom-right corner
- `( -0.5 0 )` = halfway between center and left edge, vertically centered

More precisely, the final position is calculated as: `(offset + (1,1)) * (960, 540)`.

#### Conditional Visibility

Use `when` clauses to show landmarks only when certain conditions are met. Landmark visibility is re-evaluated each time the overworld regains focus (e.g., when a child scene ends).

```yaml
landmarks:
  secret-area:
    sprite: res://sprites/landmark.png
    size: ( 100 100 )
    offset: ( 0.5 0.5 )
    when:
      - flag secret-unlocked in [true]
```

### Full Example

```yaml
map:
  sprite: res://sprites/world-map.png
  scaling: fit-width

landmarks:
  village:
    sprite: res://sprites/landmark-icon.png
    size: ( 200 200 )
    offset: ( -0.5 -0.3 )

  dungeon:
    sprite: res://sprites/landmark-icon.png
    size: ( 200 200 )
    offset: ( 0.3 0.2 )
    when:
      - flag dungeon-unlocked in [true]

  final-boss:
    sprite: res://sprites/landmark-boss.png
    size: ( 250 250 )
    offset: ( 0.0 0.5 )
    when:
      - flag dungeon-unlocked in [true]
      - flag has-sword in [true]
```

---

## Vector2 Format

Several fields across the core module templates use a `Vector2` format for 2D values. The syntax is:

```
( <x> <y> )
```

Examples:
- `( 800 200 )` — width 800, height 200
- `( 30 20 )` — horizontal margin 30, vertical margin 20
- `( -0.5 0.3 )` — offset from center

Note the spaces inside the parentheses — they are required.
