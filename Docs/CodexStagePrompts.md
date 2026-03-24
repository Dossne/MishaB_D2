# Codex Stage Prompts

Use these prompts as-is for staged development of VacuumSorter.

For every run, the agent must:

1. Read and follow `AGENTS.md`
2. Read and follow `Docs/VacuumSorter_TechSpec.md`
3. Implement only the requested stage
4. Stop after the stage
5. Report scope, changed files/assets, current gameplay loop, and unverified items
6. Ask the user to test and report bugs
7. Do not use Unity Editor-only functionality to fetch gameplay assets; use serialized references in ScriptableObjects accessed through `ConfigurationProvider`
8. Use `TextMeshPro` for in-game text, HUD text, popup text, labels, and floating text
9. If `Assets/Font/bangerscyrillic.otf` exists, use the corresponding `bangerscyrillic` TextMeshPro font asset instead of the default Unity font for visible game text unless explicitly told otherwise

---


```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current interaction loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 0 from Docs/VacuumSorter_TechSpec.md:
"Bootstrap architecture"

Required result for this run:
- Add GameManager (MonoBehaviour) to control bootstrap / initialization order / update order / shutdown order.
- Add ServiceLocator (MonoBehaviour) to provide shared dependencies and serialized scene references.
- Add ConfigurationProvider (ScriptableObject) as centralized access to config assets and serialized gameplay asset references.
- GameManager must have a serialized reference to ServiceLocator.
- ServiceLocator must have a serialized reference to ConfigurationProvider.
- Register GameManager in ServiceLocator first.
- Register ConfigurationProvider in ServiceLocator second.
- Prepare the main scene bootstrap so the project launches into a stable initialized state.
- Establish the rule that gameplay assets are not fetched via Unity Editor-only APIs and instead come from serialized references stored in ScriptableObjects exposed through ConfigurationProvider.
- Keep the architecture small and readable.

Do not implement anything from Stage 1 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 1 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current interaction loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 1 from Docs/VacuumSorter_TechSpec.md:
"Basic UI shell"

Required result for this run:
- Add MainUiProvider (MonoBehaviour) to the main scene.
- Register MainUiProvider in ServiceLocator.
- Create the root UI hierarchy under MainUiProvider.
- Add Canvas in Overlay mode.
- Add CanvasScaler configured to reference 1920x1080.
- Add and reference child roots:
  - FloatingTextParent
  - HudParent
  - PopupParent
- Add simple placeholder HUD elements for score, level, and state.
- Use TextMeshPro for all visible UI text in this stage.
- If `Assets/Font/bangerscyrillic.otf` exists, use the corresponding `bangerscyrillic` TextMeshPro font asset for visible UI text in this stage instead of the default Unity font.
- Ensure all UI references are accessed only through MainUiProvider fields.

Do not implement anything from Stage 2 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 2 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current interaction loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 2 from Docs/VacuumSorter_TechSpec.md:
"Arena and camera slice"

Required result for this run:
- Create the playable scene shell with a flat arena.
- Add arena bounds/walls so later robot and objects stay inside the play space.
- Add a clear center area for object spawning.
- Add placeholder positions/anchors for future sorting holes around the center.
- Add a fixed readable gameplay camera.
- Ensure the arena remains readable on mobile-like aspect ratios.
- Keep visuals simple and prototype-oriented.

Do not implement anything from Stage 3 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 3 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 3 from Docs/VacuumSorter_TechSpec.md:
"Player input and robot movement"

Required result for this run:
- Add on-screen joystick input.
- Add input handling suitable for touch gameplay and convenient editor testing.
- Create the robot with a visible U-shaped front scoop.
- Implement responsive movement driven by joystick direction and magnitude.
- Support smooth acceleration, fast deceleration, and precise low-speed positioning.
- Keep the robot inside the arena.
- Make the robot stable and readable in motion.

Do not implement anything from Stage 4 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 4 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 4 from Docs/VacuumSorter_TechSpec.md:
"Falling pile slice"

Required result for this run:
- Add item runtime model and at least 2 placeholder item types.
- Spawn all level items above the arena center at level start.
- Let items fall with Rigidbody physics and form a pile.
- Tune item physics so the pile is readable and can be pushed.
- Ensure the robot scoop can push multiple items at once.
- Keep the stage focused on pile creation and physical interaction.

Do not implement anything from Stage 5 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 5 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 5 from Docs/VacuumSorter_TechSpec.md:
"Sorting target slice"

Required result for this run:
- Add sorting holes/targets matching item types.
- Correct items must be accepted, removed from play, and counted.
- Wrong items must be ignored or rejected safely.
- Keep target visuals readable by type.
- Complete the first full minimal sorting loop.

Do not implement anything from Stage 6 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 6 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 6 from Docs/VacuumSorter_TechSpec.md:
"Round completion and HUD loop"

Required result for this run:
- Add level objective tracking.
- Update HUD with score, remaining required objects, and level number.
- Add a win/completion state.
- Add simple restart flow.
- Add a placeholder or basic next-level transition.
- Keep level flow simple, stable, and easy to test.

Do not implement anything from Stage 7 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 7 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 7 from Docs/VacuumSorter_TechSpec.md:
"Content-driven levels"

Required result for this run:
- Move level content into LevelConfig ScriptableObjects.
- Support config-driven item counts, item types, spawn settings, and target layout.
- Add 10 playable level configs for the MVP path.
- Implement loading levels by index.
- Keep level geometry variations lightweight and prototype-safe.

Do not implement anything from Stage 8 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 8 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 8 from Docs/VacuumSorter_TechSpec.md:
"Feedback and juice"

Required result for this run:
- Add readable successful-sort feedback.
- Add pull-in / absorb feel for correct items entering holes.
- Add floating text support using FloatingTextParent.
- Add sound hooks or placeholder audio integration.
- Add haptic hooks or placeholder tactile integration.
- Add subtle feedback for pushing into dense object piles.
- Keep gameplay readable and do not damage sorting clarity.

Do not implement anything from Stage 9 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 9 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 9 from Docs/VacuumSorter_TechSpec.md:
"Scoop upgrade loop"

Required result for this run:
- Add a simple between-level upgrade flow or reward step.
- Implement scoop size as the first upgrade.
- Make the scoop upgrade visibly affect gameplay capacity.
- Show current upgrade state in UI.
- Persist upgrade state at least during the current app session.
- Validate the feeling of player power growth.

Do not implement anything from Stage 10 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 10 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/VacuumSorter_TechSpec.md

Continue staged implementation of VacuumSorter.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 10 from Docs/VacuumSorter_TechSpec.md:
"MVP tuning"

Required result for this run:
- Tune movement, pile density, level pacing, and core readability.
- Keep the 10-level MVP path playable inside short mobile sessions.
- Prepare the prototype for real hypothesis testing.

Do not implement anything after Stage 10 in this run.

When done, stop and wait for my feedback.
```







