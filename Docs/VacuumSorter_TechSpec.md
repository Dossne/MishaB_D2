# VacuumSorter Tech Spec

## 1. Document purpose

This document is the implementation source of truth for the Unity prototype **VacuumSorter**.

It is written for staged execution by an AI coding agent.  
The agent must implement strictly one stage per run, stop after the stage is complete, and wait for user feedback before continuing.

## 2. Product summary

### 2.1 High concept

VacuumSorter is a mobile casual 3D prototype where the player controls a robot vacuum with a U-shaped front scoop and pushes piles of physics objects into matching holes.

### 2.2 Prototype goal

Validate these hypotheses:

1. Moving and separating a chaotic pile of objects feels pleasant.
2. Sorting through physical interaction feels satisfying and readable.
3. Increasing scoop size creates a clear feeling of power growth.

### 2.3 Platform and session

- Platforms: iOS, Android
- Engine: Unity
- Camera: fixed top-down or fixed isometric with tilt
- Session length target: 1-3 minutes
- Genre: casual / puzzle-action

## 3. Core gameplay

### 3.1 Core loop

1. Level starts.
2. Objects spawn above the center of the arena.
3. Objects fall and form a mixed pile.
4. Player moves the robot with an on-screen joystick.
5. Robot pushes groups of objects with the scoop.
6. Correct objects are delivered into matching holes.
7. Score increases.
8. Level ends when required objects are sorted.

### 3.2 Controls

- Touch joystick on screen
- Direction controls movement direction
- Stick magnitude controls speed
- Movement must support:
  - high responsiveness
  - small precise movements
  - smooth acceleration
  - quick deceleration on release

### 3.3 Arena

- Flat 3D arena
- Central spawn zone for object rain
- Peripheral sorting holes around the center
- Short travel distance between pile and holes
- Robot must not fall into holes

### 3.4 Objects

- 2-3 object types in MVP
- Type must be readable through color and optionally symbol / mesh
- Objects use Rigidbody-based physics
- Objects collide, stack, slide, and accumulate into a dense pile

### 3.5 Holes

- Each hole accepts exactly one object type
- Correct object:
  - is pulled in
  - disappears
  - gives score
- Wrong object:
  - is ignored or rejected

### 3.6 Progression in MVP

- 10 levels
- Main scaling levers:
  - more total objects
  - more object types
  - more holes
  - slightly harder arena geometry
- 1 upgrade parameter:
  - scoop size

## 4. Implementation rules for Codex

### 4.1 Stage execution

For every run:

1. Read `AGENTS.md`
2. Read this file
3. Implement only the requested stage
4. Do not implement later stages
5. Stop after the stage is complete
6. Ask the user to test the stage in Unity or in a build
7. Ask the user to report bugs or UX issues
8. Wait for explicit confirmation before starting the next stage

If bugs are reported for the current stage, fix them before moving further.

### 4.2 Vertical slice rule

Every stage must end with a runnable slice.

This does not mean full gameplay must exist from Stage 0.  
It means the current stage must provide a coherent, bootable, testable interaction slice for that stage's scope.

### 4.3 Dependency rule

All non-MonoBehaviour manager-like classes must receive dependencies through constructor or explicit initialization methods using `ServiceLocator`.

Direct scene lookups from arbitrary code are not allowed except inside bootstrap/provider components whose responsibility is to expose serialized references.

### 4.4 Asset reference rule

Gameplay assets must not be discovered through Unity Editor-only APIs or Editor-only asset lookup workflows.

Do not use Editor-only functionality such as `AssetDatabase`, editor utilities, editor-generated lookup passes, or other editor-time asset discovery to obtain gameplay assets at runtime.

Gameplay asset references must be provided through serialized references stored in ScriptableObject configs and accessed through `ConfigurationProvider` registered in `ServiceLocator`.

If a runtime system needs access to prefabs, materials, item definitions, level definitions, or other gameplay assets, those references must be serialized into the relevant config asset and then read through `ConfigurationProvider`.

### 4.5 Text rendering rule

Use TextMeshPro for in-game text, HUD text, popup text, floating text, labels, and any other player-facing text rendering.

If `Assets/Font/bangerscyrillic.otf` is present in the project, use the corresponding TextMeshPro font asset based on `bangerscyrillic` instead of the default Unity font for visible player-facing game text, unless the user explicitly asks for another font.

Do not use legacy Unity UI text components for gameplay or UI text unless the user explicitly asks for an exception.

### 4.6 Scope rule
- Keep diffs small
- Prefer boring and readable code
- Avoid framework overgrowth
- Avoid changing project-wide settings unless strictly needed
- Be careful with scene/prefab serialization churn

## 5. Project structure

### 5.1 Root feature rule

All game implementation must live under:

- `Assets/Project/`

Each feature gets its own folder:

- `Assets/Project/<FeatureName>/`

One feature = one folder.

### 5.2 Required subfolder naming

Inside each feature folder, organize files by asset type:

- `FeatureNameSrc` for scripts
- `FeatureNamePfs` for prefabs
- `FeatureNameArt` for sprites, animations, animator controllers, icons, VFX textures
- `FeatureNameCfg` for ScriptableObject assets

Additional asset-type folders are allowed when needed:

- `FeatureNameScn` for scenes
- `FeatureNameMat` for materials
- `FeatureNameAud` for audio assets

Rule for configs:

- ScriptableObject assets (`.asset`) go into `FeatureNameCfg`
- ScriptableObject C# classes that define them go into `FeatureNameSrc`

### 5.3 Planned feature folders

Recommended top-level feature folders for MVP:

- `Assets/Project/Bootstrap/`
- `Assets/Project/MainUI/`
- `Assets/Project/GameScene/`
- `Assets/Project/GameCamera/`
- `Assets/Project/PlayerInput/`
- `Assets/Project/Robot/`
- `Assets/Project/Items/`
- `Assets/Project/SortTargets/`
- `Assets/Project/LevelFlow/`
- `Assets/Project/Scoring/`
- `Assets/Project/Feedback/`
- `Assets/Project/Progression/`
- `Assets/Project/Meta/`

This list is the default plan. If a small support feature is needed, it may be added as a separate folder without restructuring existing features.

## 6. Main architecture

### 6.1 Bootstrap objects in the main scene

The main scene must eventually contain at least:

- `ServiceLocator` MonoBehaviour
- `GameManager` MonoBehaviour
- `MainUiProvider` MonoBehaviour
- camera rig root
- gameplay roots required by the active stage

### 6.2 `ServiceLocator`

Responsibilities:

- store references to shared scene services and providers
- expose accessors for serialized scene dependencies
- expose access to `ConfigurationProvider`
- be initialized early in bootstrap

Constraints:

- it is a scene MonoBehaviour
- it is serialized in the scene
- it should not become a hidden global static singleton with uncontrolled lifecycle
- it may expose a minimal static current reference only if needed for scene bootstrap, but gameplay code should still rely on explicit dependency passing

### 6.3 `GameManager`

Responsibilities:

- bootstrap manager creation order
- initialize non-MonoBehaviour runtime systems
- drive update order if central ticking is used
- own orderly shutdown/deinitialization

Requirements:

- MonoBehaviour in the main scene
- serialized reference to `ServiceLocator`
- registers itself first in `ServiceLocator`

### 6.4 `ConfigurationProvider`

Responsibilities:

- central access point to ScriptableObject configs
- stores references to feature configs created in later stages

Requirements:

- ScriptableObject asset
- referenced by `ServiceLocator`
- registered second in `ServiceLocator`

### 6.5 `MainUiProvider`

Responsibilities:

- root provider for all UI scene references
- expose UI elements only through serialized fields

Requirements:

- MonoBehaviour in the main scene
- has `Canvas` in Overlay mode
- has `CanvasScaler`
- reference resolution set for `1920 x 1080`
- contains and references:
  - `FloatingTextParent`
  - `HudParent`
  - `PopupParent`

## 7. Data model guidelines


The prototype should prefer ScriptableObject-driven content where useful:

Gameplay prefabs and other runtime gameplay assets must also be referenced through serialized ScriptableObject fields resolved via `ConfigurationProvider`, not through Editor-only asset lookup.

- game-wide config
- robot tuning
- item type definitions
- hole definitions
- level definitions
- progression definitions
- feedback tuning

Recommended SO classes:

- `BootstrapConfig`
- `InputConfig`
- `RobotConfig`
- `ItemTypeConfig`
- `LevelConfig`
- `ScoreConfig`
- `FeedbackConfig`
- `UpgradeConfig`

Only add a config when it is used by the current stage.

## 8. Stage plan

## Stage 0. Bootstrap architecture

### Goal

Prepare the minimum runtime architecture and scene bootstrap used by all future stages.

### Required implementation

- Create `GameManager` MonoBehaviour
- Create `ServiceLocator` MonoBehaviour
- Create `ConfigurationProvider` ScriptableObject
- Prepare `ConfigurationProvider` as the centralized runtime entry point for serialized gameplay config and asset references
- Add `GameManager` and `ServiceLocator` to the main scene
- `GameManager` has serialized reference to `ServiceLocator`
- `ServiceLocator` has serialized reference to `ConfigurationProvider`
- Register `GameManager` in `ServiceLocator` first
- Register `ConfigurationProvider` in `ServiceLocator` second
- Define clear initialize / update / shutdown flow for manager-like classes
- Establish the rule that gameplay assets are accessed through serialized references in ScriptableObjects registered in `ConfigurationProvider`, not through Unity Editor-only asset discovery APIs
- Keep bootstrap code intentionally small and readable
### Runnable slice at end of stage

The project boots into the main scene without gameplay errors and logs or otherwise demonstrates that bootstrap order is working.

### Current player interaction loop after stage

Player can launch the scene/build and reach a stable initialized application state with the shared runtime architecture active.

### Files and folders to create

- `Assets/Project/Bootstrap/BootstrapSrc/`
- `Assets/Project/Bootstrap/BootstrapCfg/`
- `Assets/Project/GameScene/GameSceneScn/`

Expected assets:

- `Assets/Project/Bootstrap/BootstrapSrc/GameManager.cs`
- `Assets/Project/Bootstrap/BootstrapSrc/ServiceLocator.cs`
- `Assets/Project/Bootstrap/BootstrapSrc/ConfigurationProvider.cs`
- `Assets/Project/Bootstrap/BootstrapCfg/ConfigurationProvider.asset`
- main scene asset inside `Assets/Project/GameScene/GameSceneScn/`

## Stage 1. Basic UI shell

### Goal

Create the main UI root and a clean structure for future HUD, popups, and floating texts.

### Required implementation

- Create `MainUiProvider` MonoBehaviour
- Add it to the main scene
- Register it in `ServiceLocator`
- Create Overlay Canvas setup
- Add and serialize child roots:
  - `FloatingTextParent`
  - `HudParent`
  - `PopupParent`
- Prepare simple placeholder HUD labels:
  - score label
  - level label
  - state/status label
- Keep all UI access through `MainUiProvider` fields

### Runnable slice at end of stage

Project boots with stable bootstrap and visible UI skeleton.

### Current player interaction loop after stage

Player launches the game and sees a working UI shell with placeholder HUD elements and no broken references.

### Files and folders to create

- `Assets/Project/MainUI/MainUISrc/`
- `Assets/Project/MainUI/MainUIPfs/`
- `Assets/Project/MainUI/MainUIArt/`

Expected assets:

- `Assets/Project/MainUI/MainUISrc/MainUiProvider.cs`
- optional HUD prefab assets inside `Assets/Project/MainUI/MainUIPfs/`

## Stage 2. Arena and camera slice

### Goal

Create the playable scene shell: arena floor, walls/bounds, spawn center marker, target positions, and fixed readable camera.

### Required implementation

- Create flat arena geometry
- Add boundary solution so objects and robot stay in play area
- Create target placement points around the center
- Add fixed game camera with top-down or isometric angle
- Ensure camera framing keeps the full arena readable on mobile aspect ratios
- Add simple visual placeholders for arena zones

### Runnable slice at end of stage

Project launches into a readable empty arena ready for gameplay features.

### Current player interaction loop after stage

Player can launch the game and inspect the empty arena from the final gameplay camera point of view.

### Files and folders to create

- `Assets/Project/GameCamera/GameCameraSrc/`
- `Assets/Project/GameCamera/GameCameraPfs/`
- `Assets/Project/GameScene/GameScenePfs/`
- `Assets/Project/GameScene/GameSceneArt/`

Expected assets:

- `Assets/Project/GameCamera/GameCameraSrc/GameCameraController.cs`
- arena prefab(s) in `Assets/Project/GameScene/GameScenePfs/`
- camera rig prefab in `Assets/Project/GameCamera/GameCameraPfs/`

## Stage 3. Player input and robot movement

### Goal

Add controllable robot movement with responsive joystick input in the arena.

### Required implementation

- Create on-screen joystick UI
- Create input abstraction that can later support touch and editor testing
- Add robot prefab with visible U-shaped scoop
- Implement movement using joystick direction and magnitude
- Support:
  - acceleration smoothing
  - quick deceleration
  - precise low-speed movement
- Prevent robot from leaving arena bounds
- Robot must not tip over or behave unstably

### Runnable slice at end of stage

Player can move the robot around an empty arena using the joystick.

### Current player interaction loop after stage

Player starts the game, drags the joystick, drives the robot, releases input, and feels responsive movement suitable for later sorting.

### Files and folders to create

- `Assets/Project/PlayerInput/PlayerInputSrc/`
- `Assets/Project/PlayerInput/PlayerInputPfs/`
- `Assets/Project/Robot/RobotSrc/`
- `Assets/Project/Robot/RobotPfs/`
- `Assets/Project/Robot/RobotCfg/`
- `Assets/Project/Robot/RobotArt/`

Expected assets:

- `Assets/Project/PlayerInput/PlayerInputSrc/JoystickView.cs`
- `Assets/Project/PlayerInput/PlayerInputSrc/PlayerInputReader.cs`
- `Assets/Project/Robot/RobotSrc/RobotController.cs`
- `Assets/Project/Robot/RobotSrc/RobotConfig.cs`
- `Assets/Project/Robot/RobotCfg/RobotConfig.asset`
- `Assets/Project/Robot/RobotPfs/Robot.prefab`

## Stage 4. Falling pile slice

### Goal

Introduce the core physical fantasy: objects fall into the center and form a pile that the robot can push.

### Required implementation

- Create item type runtime model
- Create at least 2 placeholder item types
- Spawn all level items at level start above the center
- Use Rigidbody-based falling and collisions
- Tune item size/mass/drag so pile is readable and pushable
- Robot scoop must push multiple objects at once
- Add simple level-start spawn sequence

### Runnable slice at end of stage

Player controls the robot and physically pushes a mixed pile of spawned objects.

### Current player interaction loop after stage

Player launches the level, watches items fall into a pile, drives into the pile, and pushes groups of objects around the arena.

### Files and folders to create

- `Assets/Project/Items/ItemsSrc/`
- `Assets/Project/Items/ItemsPfs/`
- `Assets/Project/Items/ItemsCfg/`
- `Assets/Project/Items/ItemsArt/`
- `Assets/Project/LevelFlow/LevelFlowSrc/`
- `Assets/Project/LevelFlow/LevelFlowCfg/`

Expected assets:

- `Assets/Project/Items/ItemsSrc/ItemView.cs`
- `Assets/Project/Items/ItemsSrc/ItemTypeConfig.cs`
- `Assets/Project/Items/ItemsCfg/ItemType_01.asset`
- `Assets/Project/Items/ItemsCfg/ItemType_02.asset`
- `Assets/Project/Items/ItemsPfs/` item prefabs
- `Assets/Project/LevelFlow/LevelFlowSrc/LevelBootstrap.cs`

## Stage 5. Sorting target slice

### Goal

Add sorting holes and validate the complete basic interaction: push the right object into the right target.

### Required implementation

- Create sorting hole prefab and runtime logic
- Create at least 2 hole types matching item types
- Correct item:
  - recognized
  - pulled/animated into hole
  - removed from play
- Wrong item:
  - ignored or softly rejected
- Add simple counters for required sorted objects
- Keep visual readability high

### Runnable slice at end of stage

A full core sorting loop exists for a minimal level.

### Current player interaction loop after stage

Player drives the robot, separates the pile, pushes objects into matching holes, and sees correct objects counted and removed.

### Files and folders to create

- `Assets/Project/SortTargets/SortTargetsSrc/`
- `Assets/Project/SortTargets/SortTargetsPfs/`
- `Assets/Project/SortTargets/SortTargetsCfg/`
- `Assets/Project/SortTargets/SortTargetsArt/`
- `Assets/Project/Scoring/ScoringSrc/`

Expected assets:

- `Assets/Project/SortTargets/SortTargetsSrc/SortTargetView.cs`
- `Assets/Project/SortTargets/SortTargetsSrc/SortTargetConfig.cs`
- `Assets/Project/SortTargets/SortTargetsCfg/` target config assets
- `Assets/Project/SortTargets/SortTargetsPfs/` target prefabs
- `Assets/Project/Scoring/ScoringSrc/ScoreService.cs`

## Stage 6. Round completion and HUD loop

### Goal

Turn the core interaction into a complete short level loop with clear objective, progress, and restartability.

### Required implementation

- Add level objective tracking
- Add visible HUD updates:
  - score
  - remaining required objects
  - level number
- Add win state when objective is complete
- Add simple restart flow
- Add next-level trigger placeholder
- Keep scene reload or reset logic safe and simple

### Runnable slice at end of stage

Player can complete a level, see the result, and restart or continue.

### Current player interaction loop after stage

Player starts a level, sorts required objects, reaches completion, sees end-of-level feedback/state, and can restart or advance.

### Files and folders to create

- `Assets/Project/LevelFlow/LevelFlowPfs/`
- `Assets/Project/Scoring/ScoringCfg/`
- `Assets/Project/Meta/MetaSrc/`

Expected assets:

- `Assets/Project/LevelFlow/LevelFlowSrc/LevelStateController.cs`
- `Assets/Project/LevelFlow/LevelFlowSrc/LevelCompletionService.cs`
- `Assets/Project/Meta/MetaSrc/RestartButtonPresenter.cs`

## Stage 7. Content-driven levels

### Goal

Move from hardcoded setup to ScriptableObject-driven level content and prepare the planned 10-level MVP path.

### Required implementation

- Create `LevelConfig` structure
- Move stage content parameters into config assets:
  - item counts
  - item types
  - spawn settings
  - target layout
  - arena obstacle/layout variations where needed
- Create 10 playable level configs
- Implement level loading by index
- Keep level geometry variations lightweight and prototype-friendly

### Runnable slice at end of stage

The game supports multiple configured levels and can load them in sequence.

### Current player interaction loop after stage

Player plays short levels with changing object mixes and layouts instead of replaying one hardcoded test scene.

### Files and folders to create

- `Assets/Project/LevelFlow/LevelFlowCfg/`
- `Assets/Project/GameScene/GameScenePfs/` additional arena/layout prefabs if needed

Expected assets:

- `Assets/Project/LevelFlow/LevelFlowSrc/LevelConfig.cs`
- `Assets/Project/LevelFlow/LevelFlowCfg/Level_01.asset` through `Level_10.asset`

## Stage 8. Feedback and juice

### Goal

Make successful sorting and pile interaction feel more satisfying without damaging readability.

### Required implementation

- Add successful sort feedback:
  - pull-in motion
  - scale/fade or similar
  - particles or glow
- Add floating text support through `FloatingTextParent`
- Add audio hook integration or placeholders
- Add haptic hook interface or placeholder
- Add subtle feedback when robot engages with dense object mass
- Keep all feedback optional and data-driven where practical

### Runnable slice at end of stage

The same level loop now provides readable visual/audio/tactile reward moments.

### Current player interaction loop after stage

Player pushes pile clusters, lands successful sorts, and receives more satisfying feedback for correct actions.

### Files and folders to create

- `Assets/Project/Feedback/FeedbackSrc/`
- `Assets/Project/Feedback/FeedbackPfs/`
- `Assets/Project/Feedback/FeedbackCfg/`
- `Assets/Project/Feedback/FeedbackArt/`
- `Assets/Project/Feedback/FeedbackAud/`

Expected assets:

- `Assets/Project/Feedback/FeedbackSrc/FloatingTextPresenter.cs`
- `Assets/Project/Feedback/FeedbackSrc/FeedbackConfig.cs`
- `Assets/Project/Feedback/FeedbackCfg/FeedbackConfig.asset`

## Stage 9. Scoop upgrade loop

### Goal

Validate the progression hypothesis that increasing scoop size feels like meaningful power growth.

### Required implementation

- Add between-level upgrade step or simple progression reward
- Implement scoop size upgrade as the first real upgrade
- Upgrade must visibly affect robot interaction capacity
- Show current upgrade state in HUD or between-level UI
- Persist upgrade state at least during app session

### Runnable slice at end of stage

Player can improve scoop size and feel stronger on later levels.

### Current player interaction loop after stage

Player completes levels, receives or applies scoop growth, starts the next level, and can push larger groups of objects.

### Files and folders to create

- `Assets/Project/Progression/ProgressionSrc/`
- `Assets/Project/Progression/ProgressionCfg/`
- `Assets/Project/Progression/ProgressionPfs/`

Expected assets:

- `Assets/Project/Progression/ProgressionSrc/UpgradeService.cs`
- `Assets/Project/Progression/ProgressionSrc/UpgradeConfig.cs`
- `Assets/Project/Progression/ProgressionCfg/UpgradeConfig.asset`

## Stage 10. MVP tuning

### Goal

Prepare the prototype for real playtesting with final MVP tuning.

### Required implementation`r`n`r`n- Tune movement, pile density, object counts, and level pacing
- Verify the 10-level path remains short-session playable

### Runnable slice at end of stage

The prototype is playable as an MVP and tuned for short-session playtests.

### Current player interaction loop after stage

Player can play the MVP progression from level to level in a tuned short-session flow.

### Files and folders to create`r`n`r`nNo new feature folder is required unless tuning work needs small supporting assets.

## 9. Acceptance criteria by system

### 9.1 Movement

- Robot responds immediately to joystick direction changes
- Small joystick displacement allows precise movement
- Releasing input stops movement quickly
- Movement feels controllable on mobile

### 9.2 Physics pile

- Objects form a visible pile instead of scattering uncontrollably
- Robot can push several objects together
- Pile interaction remains readable

### 9.3 Sorting

- Correct targets are easy to identify
- Correct items are accepted reliably
- Wrong items do not accidentally count as correct

### 9.4 Readability

- Arena remains visually understandable
- UI does not obstruct core play area
- Feedback reinforces actions without hiding gameplay

### 9.5 Technical

- No uncontrolled scene dependency access
- Service references flow through `ServiceLocator`
- Each stage remains bootable and testable

## 10. Stage completion response template for Codex

After finishing any stage, Codex must report:

1. What was implemented
2. Which files / prefabs / ScriptableObjects / scenes / folders were added or changed
3. What gameplay loop or interaction loop is currently available to the player
4. What was not verified because Unity/editor/runtime testing was unavailable
5. A direct request to:
   - test the stage in Unity or a build
   - report bugs or UX issues
   - confirm when to continue to the next stage

## 11. Non-goals for MVP

Do not expand scope into these areas unless the user explicitly asks:

- live ops
- economy depth beyond one upgrade axis
- advanced AI opponents
- procedural generation
- complex save/load framework
- large reusable architecture layers not required by the prototype




