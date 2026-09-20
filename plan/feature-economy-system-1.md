---
goal: Implement the Fish of My Dreams Economy System
version: 1.0
date_created: 2026-09-20
last_updated: 2026-09-20
owner: Maxim
status: 'Completed'
tags: [feature, economy, fishing, ui, save, unity]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-green)

Implement a complete local economy loop in `Assets/Project/Scenes/MainScene.unity`: sell duplicate fish, earn coins, buy and equip permanent rod and bobber tiers, apply equipment bonuses to catch generation, display responsive uGUI, and persist progress through `PlayerPrefs` for desktop and WebGL.

## 1. Requirements & Constraints

- **REQ-001**: The player shall interact with the fish buyer, rod seller, and bobber seller by pressing the existing `Player/Interact` action bound to `E` while within 3 meters.
- **REQ-002**: The first caught specimen of each species shall remain protected in the aquarium collection and shall never be sellable.
- **REQ-003**: A heavier later specimen shall become the collection record and move the previous record to inventory; every other later specimen shall enter inventory with its exact species, rarity, and weight.
- **REQ-004**: Fish sale value shall increase with rarity, absolute weight, and normalized weight inside the species weight range.
- **REQ-005**: The HUD shall display the current coin balance at all times during gameplay.
- **REQ-006**: The fish buyer UI shall list sellable fish with icon, species, rarity, weight, and price; it shall support individual sale and confirmed sell-all.
- **REQ-007**: Rod and bobber stores shall list five sequential tiers with icon, color, name, price, benefit, ownership, and equipped state.
- **REQ-008**: Purchased equipment shall remain permanently owned and shall be switchable without further payment.
- **REQ-009**: Bobbers shall increase rare-catch probability. Rods shall increase rare-catch probability and bias weight toward the species maximum.
- **REQ-010**: Equipment shall not modify the hook timing window.
- **REQ-011**: Equipped rod and bobber colors shall be applied with `MaterialPropertyBlock` without modifying shared source materials.
- **REQ-012**: Shop UI shall block movement, camera look, and fishing input while open and shall close through `E`, `Escape`, or its close button.
- **REQ-013**: The implementation shall use the existing VContainer root and shall not introduce a second inventory or global singleton MonoBehaviour.
- **REQ-014**: Collection, inventory, coins, owned tiers, and equipped tiers shall persist locally through one versioned JSON save stored in `PlayerPrefs`.
- **REQ-015**: The three existing merchant groups in `MainScene` shall remain visually unchanged except for added interaction components.
- **REQ-016**: UI shall use the existing dark navy, cyan, white, green, and gold visual language and TextMeshPro typography.
- **CON-001**: Do not implement decoration purchases, gems, real ads, or real payments.
- **CON-002**: Preserve the existing dirty `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` change without editing it.
- **CON-003**: Use the existing `FishCatalog` as the single source of fish species and rarity data.
- **PAT-001**: Keep state and calculations in injected services; keep MonoBehaviours responsible for scene input, presentation, and equipment visuals.

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Add data-driven economy, equipment, and save state.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Assets/Project/Scripts/Economy/EquipmentDefinition.cs`, `EquipmentCatalog.cs`, and `EquipmentType.cs` with five sequential rod tiers and five sequential bobber tiers. | ✅ | 2026-09-20 |
| TASK-002 | Replace `Assets/Project/Scripts/Services/SaveService.cs` with a versioned `PlayerPrefs` JSON repository containing collection, inventory, balance, owned equipment, and equipped equipment. | ✅ | 2026-09-20 |
| TASK-003 | Extend `Assets/Project/Scripts/Services/ProgressService.cs` with load, save, remove-one, and remove-all inventory operations while protecting collection fish. | ✅ | 2026-09-20 |
| TASK-004 | Create `Assets/Project/Scripts/Economy/EconomyService.cs` with fish pricing, coin balance, sequential purchase, ownership, and equip operations. | ✅ | 2026-09-20 |
| TASK-005 | Modify `Assets/Project/Scripts/Services/CatchGenerator.cs` to apply equipped rarity luck and rod weight bias without changing hook timing. | ✅ | 2026-09-20 |

### Implementation Phase 2

- GOAL-002: Add merchant interaction and equipment visuals.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Expose `InteractPressedThisFrame` through `IInputService` and `InputService` using the existing `Player/Interact` action. | ✅ | 2026-09-20 |
| TASK-007 | Create `ShopType`, `ShopInteractable`, and `ShopInteractionController`; configure the three existing merchant groups in `MainScene` with 3-meter interaction ranges. | ✅ | 2026-09-20 |
| TASK-008 | Modify `RodView`, `Bobber`, and `FishingController` to apply equipped colors at startup and after equipment changes. | ✅ | 2026-09-20 |
| TASK-009 | Register `SaveService`, `EconomyService`, `EquipmentCatalog`, interaction components, and economy UI components in `GameLifeTimeScope`. | ✅ | 2026-09-20 |

### Implementation Phase 3

- GOAL-003: Build and connect responsive economy UI.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Create coin, fish, rod, and bobber sprite icons under `Assets/Project/Textures/UI/Economy`. | ✅ | 2026-09-20 |
| TASK-011 | Create `EconomyHUD`, `ShopUIController`, and `ShopItemView` scripts using fully qualified uGUI component types and TextMeshPro. | ✅ | 2026-09-20 |
| TASK-012 | Build `EconomyHUD`, `InteractionPrompt`, `ShopPanel`, `ScrollView`, and `ConfirmationPanel` under the existing `Canvas` through the connected Unity Editor. | ✅ | 2026-09-20 |
| TASK-013 | Wire close, purchase, equip, individual sale, sell-all confirmation, dynamic row refresh, and balance refresh behavior. | ✅ | 2026-09-20 |
| TASK-014 | Modify `PauseController` so `Escape` closes the shop before opening the pause menu. | ✅ | 2026-09-20 |

### Implementation Phase 4

- GOAL-004: Validate the complete economy loop and scene integrity.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Recompile scripts and resolve all Unity compiler errors. | ✅ | 2026-09-20 |
| TASK-016 | Validate merchant proximity selection, prompt visibility, `E` open/close, cursor state, movement lock, and pause behavior in Play Mode. | ✅ | 2026-09-20 |
| TASK-017 | Validate individual sale, confirmed sell-all, insufficient funds, sequential unlock, purchase, re-equip, and visual color application. | ✅ | 2026-09-20 |
| TASK-018 | Validate rarity and weight calculations with deterministic sample rolls and confirm the hook window remains unchanged. | ✅ | 2026-09-20 |
| TASK-019 | Validate save reload for collection, inventory, balance, ownership, and equipped tiers. | ✅ | 2026-09-20 |
| TASK-020 | Verify scene references, one EventSystem, visible responsive UI bounds, console errors, Git changes, and preservation of unrelated user changes. | ✅ | 2026-09-20 |

## 3. Alternatives

- **ALT-001**: Store economy state directly in UI MonoBehaviours. Rejected because it couples durable game state to scene lifetime and prevents clean save/load tests.
- **ALT-002**: Create separate rod and bobber prefabs for every tier. Rejected because the requested visual difference is color only and would duplicate meshes, animations, and maintenance.
- **ALT-003**: Use physical trigger colliders as the only merchant detector. Rejected because distance selection is more robust with the current CharacterController and avoids collider-layer coupling.
- **ALT-004**: Use ScriptableObject assets as mutable player progress. Rejected because runtime progress must persist independently from project assets and work in WebGL.

## 4. Dependencies

- **DEP-001**: Existing VContainer installation and `GameLifeTimeScope` scene component.
- **DEP-002**: Existing Unity Input System action `Player/Interact` bound to keyboard `E`.
- **DEP-003**: Existing TextMeshPro essentials and `LiberationSans SDF` font asset.
- **DEP-004**: Existing `FishCatalog`, `ProgressService`, `FishingController`, `RodView`, and `Bobber` systems.
- **DEP-005**: Existing `Canvas`, `GraphicRaycaster`, and single `EventSystem` in `MainScene`.

## 5. Files

- **FILE-001**: `Assets/Project/Scripts/Economy/*` contains equipment data and economy state.
- **FILE-002**: `Assets/Project/Scripts/Interaction/*` contains merchant interaction behavior.
- **FILE-003**: `Assets/Project/Scripts/UI/EconomyHUD.cs`, `ShopUIController.cs`, and `ShopItemView.cs` contain presentation behavior.
- **FILE-004**: `Assets/Project/Scripts/Services/SaveService.cs`, `ProgressService.cs`, `CatchGenerator.cs`, and `GameLifeTimeScope.cs` integrate persistence and DI.
- **FILE-005**: `Assets/Project/Scripts/Fishing/FishingController.cs`, `RodView.cs`, and `Bobber.cs` integrate equipped visuals and catch bonuses.
- **FILE-006**: `Assets/Project/Scripts/Interfaces/IInputService.cs`, `Services/InputService.cs`, and `Pause/PauseController.cs` integrate controls.
- **FILE-007**: `Assets/Project/Configs/EquipmentCatalog.asset` contains tunable balance values and colors.
- **FILE-008**: `Assets/Project/Textures/UI/Economy/*` contains economy icons.
- **FILE-009**: `Assets/Project/Scenes/MainScene.unity` contains merchant components and economy UI hierarchy.

## 6. Testing

- **TEST-001**: At distances above and below 3 meters, verify only the nearest merchant is interactable and the correct prompt and shop type appear.
- **TEST-002**: Register one new species and one duplicate; verify only the duplicate appears in the fish buyer UI and only it can be sold.
- **TEST-003**: Compare prices across rarity and weight boundaries and verify monotonic increase for both inputs.
- **TEST-004**: Attempt non-sequential and unaffordable purchases and verify state and balance remain unchanged.
- **TEST-005**: Purchase each tier sequentially, switch among owned tiers, and verify rod/bobber colors and bonuses match the catalog.
- **TEST-006**: Run large deterministic roll samples with starter and black equipment and verify higher rare-result and mean normalized-weight rates for black equipment.
- **TEST-007**: Save, reconstruct services, and verify exact progress restoration.
- **TEST-008**: Open and close all shops through button, `E`, and `Escape`; verify cursor, camera, movement, pause, and fishing states remain valid.
- **TEST-009**: Validate UI at 1920x1080 and a portrait/mobile aspect ratio without zero-sized or off-screen interactive controls.
- **TEST-010**: Enter Play Mode, exercise the full catch-to-sale-to-purchase loop, and verify no console errors or missing references.

## 7. Risks & Assumptions

- **RISK-001**: Runtime material tinting may color more of the rod texture than only its blue accents; `MaterialPropertyBlock` is used to avoid persistent material mutation.
- **RISK-002**: The current fish catalog reuses two aquarium models across eight species; economy uses catalog identity and is not blocked by model duplication.
- **RISK-003**: Existing `Player/Interact` uses a `Hold` interaction, but `WasPressedThisFrame` reads initial button actuation and provides immediate `E` interaction.
- **ASSUMPTION-001**: The three merchant root objects keep the names `СКУПКА РЫБЫ`, `ПРОДАЖА УДОЧЕК`, and `ПРОДАЖА ПОПЛАВКОВ` during scene setup.
- **ASSUMPTION-002**: Starting equipment is free and owned; starting coin balance is zero.
- **ASSUMPTION-003**: Equipment is purchased in ascending catalog tier order and a purchase automatically equips the item.

## 8. Related Specifications / Further Reading

- Project GDD supplied in the Codex task describing collection protection, duplicate sales, rarity, weight, equipment progression, and local browser saves.
- `Assets/Project/Scripts/Fishing/FishCatalog.cs`
- `Assets/Project/Scripts/Services/ProgressService.cs`
- `Assets/Project/Scripts/Services/CatchGenerator.cs`
