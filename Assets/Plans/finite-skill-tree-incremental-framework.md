# Project Overview
- **Game Title**: Finite Skill Tree & Incremental Game Framework
- **High-Level Concept**: A robust, plug-and-play Unity library and toolkit designed to accelerate the development of incremental, idle, and clicker games featuring finite multi-tiered skill trees, multi-currency economies, customizable prestige layers, decoupled event architectures, and comprehensive internal testing/cheat suites.
- **Players**: Single player (Incremental/Idle/Strategy framework)
- **Inspiration / Reference Games**: *Nodebuster* (interconnected node webs, adjacency synergies, booster/converter node archetypes), *Digseum* (multi-tier museum/excavation depth progression, artifact discovery milestones, specialized passive clusters), *Path of Exile* (passive skill tree hierarchy: Minors, Notables, and game-altering Keystones with finite allocation points and respec mechanics), *Antimatter Dimensions* / *Realm Grinder*.
- **Tone / Art Direction**: Clean, modern, functional UI with modular theming capabilities suitable for dark sci-fi, fantasy, or minimalist incremental genres.
- **Target Platform**: PC (Standalone Windows/Mac/Linux), WebGL, and Mobile (Android / iOS responsive UI)
- **Screen Orientation / Resolution**: Landscape 1920x1080 (responsive layout adapts to 16:9, 16:10, and portrait mobile viewports)
- **Render Pipeline**: Universal Render Pipeline (URP 17.3+) / 2D Lit

---

# Game Mechanics

## Core Gameplay Loop
The framework powers a rich, tactical incremental lifecycle heavily inspired by *Nodebuster*, *Digseum*, and *Path of Exile*:
1. **Production & Node Activation**: Players tap or automate core resource streams. Unlocked skill nodes can act not just as passive buffs, but as active generators, converters, and synergy amplifiers (Nodebuster-style).
2. **Cluster & Web Traversal**: Players allocate finite Skill Points or currencies into an interconnected node graph. Nodes fall into distinct archetypes:
   - **Minor Nodes**: Incremental stat buffs (+5% Gold, +10% Mine Speed).
   - **Notable Nodes**: Substantial multipliers, auto-buy unlocks, or cross-resource generation boosts.
   - **Keystone Nodes**: Game-altering rule changes (e.g., "Sacrifice 50% Click Power to triple all automated generators", "Convert 10% of Mana production directly into Gold", or "Adjacent nodes gain +25% effectiveness per connected node").
   - **Gateway / Frontier Nodes**: Bridge connections unlocking new constellation clusters or specialized prestige branches (Digseum-style depth tiers).
3. **Synergy & Adjacency Mechanics**: Optional node adjacency links allowing player placement/pathing decisions to power up neighboring nodes (Nodebuster mechanic).
4. **Respec & Loadout Customization**: Players can refund/respec nodes using respec tokens or free full resets during prestige to adapt their build to active clicking vs idle automation vs prestige push strategies.
5. **Layered Prestige & Relic Milestones (Digseum-style)**: Triggering prestige resets active currencies and generators in exchange for meta-currency (e.g. Archeology Relics / Prestige Shards) and permanent Exhibit/Relic Milestones that unlock persistent keystone perks and deep constellation branches.
6. **Testing & Tuning Suite**: Designers and testers use runtime time-dilation (1x-100x), instant node unlocks, graph cycle validation, and batch idle balance simulation.

## Controls and Input Methods
- **Mouse & Keyboard (PC/WebGL)**: Left Click to produce / purchase / navigate skill nodes; Drag / Mouse Wheel to pan and zoom the Skill Tree canvas; Hotkey (`~` or `F1`) to toggle the Debug Testing Panel.
- **Touch Gestures (Mobile)**: Tap to click/purchase; Pinch to zoom skill tree viewport; One-finger drag to pan tree canvas.
- **New Input System**: Event-driven input integration through `UnityEngine.InputSystem` actions and standard UGUI pointer events.

---

# UI

## Wireframes & Screen Flow
```
+-----------------------------------------------------------------------------------------+
| [Top Bar]  Gold: 1.25e8 (+4.20e5/s)  |  Gems: 450  |  Prestige Shards: 12 (+8 on reset) |
+------------------------------------+----------------------------------------------------+
| [Generators Panel (Left 35%)]      | [Interactive Skill Tree Viewport (Right 65%)]      |
|                                    |                                                    |
| > Miner Tier 1  (Lvl 24) [Buy 100] |          (Tier 1: Base Gold)                       |
|   +12.5 Gold/s                     |                 |                                  |
| > Mana Well     (Lvl 10) [Buy 500] |        +--------+--------+                         |
|   +80.0 Mana/s                     |        |                 |                         |
|                                    |  (T2: Click Power)  (T2: Auto Mine)                |
| > [Prestige: Reset for 8 Shards]   |        |                 |                         |
|                                    |        +--------+--------+                         |
| [Toggle Debug Cheat HUD (F1)]      |                 |                                  |
|                                    |          (T3: Shard Boost)                         |
+------------------------------------+----------------------------------------------------+
| [Floating / Docked Debug Panel]                                                         |
| [Time Scale: 1x | 5x | 20x | 100x]  [+1e6 Gold]  [+100 Shards]  [Unlock All]  [Wipe]    |
+-----------------------------------------------------------------------------------------+
```

## Key UI Components
- **`SkillTreeUI`**: Pan and zoom viewport with automatic Canvas line rendering connecting prerequisite nodes to target nodes with dynamic status colors (Locked = Gray, Available = Yellow/Glow, Maxed = Green).
- **`SkillNodeUI`**: Prefab component displaying node icon, rank indicator (e.g. `3/5`), cost label, and click handler.
- **`SkillTooltipUI`**: Dynamic hover/tap card displaying node title, description, dynamic effect preview (current vs next level stats), and cost details.
- **`CurrencyDisplayUI`**: Real-time formatted text with customizable notation (`Standard` e.g. 1.25M, `Scientific` e.g. 1.25e6, `Engineering`, or `Short Words`).
- **`DebugCheatUI`**: Collapsible in-game cheat overlay containing time manipulation sliders, instant resource injectors, unlock-all toggles, and save data inspectors.

---

# Key Asset & Context

## Assembly Structure
- `FiniteSkillTreeIncremental.Runtime.asmdef` (Standalone runtime engine)
- `FiniteSkillTreeIncremental.Editor.asmdef` (Custom inspectors, graph validators, simulation tool)

## Core Class & Struct Architecture
1. **`BigNumber`**: Immutable arbitrary-precision scientific notation struct (`mantissa` double, `exponent` long).
   - Operators: `+`, `-`, `*`, `/`, `>`, `<`, `>=`, `<=`, `==`, `!=`.
   - String Formatting: Supports `StandardSuffix` (K, M, B, T, Qa, Qi, Sx, Sp, Oc, No, Dc...), `Scientific` (1.23e12), `Engineering` (12.34e9).
2. **`StatModifier` & `StatPipeline`**:
   - `StatType` (e.g. `GoldMultiplier`, `ClickEfficiency`, `GeneratorSpeed`, `PrestigeBonus`).
   - `ModifierType`: `Flat`, `PercentAdd`, `PercentMult`.
   - Dynamic aggregation with cached dirty flags for high-performance evaluation.
3. **`CurrencyDefinition` & `CurrencyWallet`**:
   - `CurrencyDefinition` (ScriptableObject): `Id`, `DisplayName`, `Icon`, `FormatType`, `MaxCapacity`.
   - `CurrencyWallet`: Balance tracking, earning/spending transactions, change events (`OnBalanceChanged(BigNumber newBal)`).
   - `CurrencyGenerator`: Automated periodic/delta generation, modified by active stats.
4. **`SkillNodeDefinition` & `SkillTreeDefinition`**:
   - `SkillNodeType`: `Minor` (small incremental), `Notable` (medium multiplier/utility), `Keystone` (build-defining game rule modifier), `Gateway` (bridge unlocking new clusters).
   - `SkillNodeDefinition` (ScriptableObject): `NodeId`, `Title`, `Description`, `Icon`, `NodeType`, `MaxRank`, `CostPerRank` (supports fixed or exponential scaling formulas), `Prerequisites` (Node + required rank), `AdjacentSynergyTags` (optional Nodebuster-style adjacency buffs), `ModifiersPerRank` (Stat modifiers applied to gameplay).
   - `SkillTreeDefinition` (ScriptableObject): Graph of nodes, root nodes, category/cluster tags, respec cost configuration.
   - `SkillTreeController`: Evaluates unlocked ranks, validates prerequisites and adjacency bonuses, handles point allocation and refund/respec operations, injects stat modifiers into the player's stat pipeline.
5. **`PrestigeLayerDefinition` & `PrestigeManager`**:
   - `PrestigeLayerDefinition` (ScriptableObject): `LayerId`, `SourceCurrency`, `RewardCurrency`, `FormulaType` (Polynomial, Exponential, Custom), `BaseRequirement`, `ResetTargets` (currencies, generators, skills to reset).
   - `PrestigeManager`: Evaluates pending reward preview, triggers reset lifecycle (`OnPrePrestige`, `OnPostPrestige`), awards prestige currency, and manages persistent milestones.
6. **`SaveManager` & `SaveDataDTO`**:
   - JSON-serializable DTOs capturing wallet balances, skill node ranks, prestige counts, and last logout timestamp for offline earnings calculation.
7. **`DebugCheatManager` & `SimulationWindow`**:
   - Runtime Time-warp controller (`Time.timeScale` and manual tick acceleration).
   - Editor Balance Simulation Window (simulates 1,000 ticks or 24 hours in milliseconds to graph currency growth curves).

---

# Implementation Steps

### Step 1: Core Mathematical Engine & BigNumber
- **Description**: Implement high-performance `BigNumber` struct with full arithmetic operator overloads, comparisons, parsing, serialization, and multi-format string formatters (Standard suffixes, Scientific, Engineering). Add unit tests.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/Core/BigNumber.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Core/BigNumberNotation.cs`
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Stat Modifier & Decoupled Event System
- **Description**: Create stat pipeline (`StatModifier`, `StatValue`, `ModifierType`) and lightweight event channel system (`GameEventChannel`, `SignalBus`) for plug-and-play decoupled gameplay and UI communication.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/Core/Stats/StatType.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Core/Stats/StatModifier.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Core/Stats/StatPipeline.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Core/Events/SignalBus.cs`
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Multi-Currency & Generator Subsystem
- **Description**: Implement `CurrencyDefinition` ScriptableObjects, runtime `CurrencyWallet`, and `CurrencyGenerator` with tick-rate delta accumulation, offline production hooks, and transaction event callbacks.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/Currencies/CurrencyDefinition.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Currencies/CurrencyWallet.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Currencies/CurrencyGenerator.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Currencies/CurrencyManager.cs`
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 2
- **Parallelizable**: Yes

### Step 4: Finite Skill Tree Subsystem (Nodebuster / PoE Archetypes)
- **Description**: Implement `SkillNodeType` enum (Minor, Notable, Keystone, Gateway), `SkillNodeDefinition` (cost formulas, max ranks, prerequisites, adjacency synergy bonuses, stat modifiers), `SkillTreeDefinition` with cluster/tag groupings, `SkillTreeController` with allocation/respec refund logic, and prerequisite validation engine.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/SkillTree/SkillNodeType.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/SkillTree/SkillNodeDefinition.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/SkillTree/SkillTreeDefinition.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/SkillTree/SkillNodeState.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/SkillTree/SkillTreeController.cs`
- **Assigned role**: developer
- **Dependencies**: Step 2, Step 3
- **Parallelizable**: No

### Step 5: Prestige & Metaprogression Layer (Digseum-style Milestones)
- **Description**: Implement `PrestigeLayerDefinition`, reward mathematical formulas, and `PrestigeManager` handling layered resets, persistence masks, permanent relic/exhibit milestone rewards, respec point refills, and lifetime statistics.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/Prestige/PrestigeLayerDefinition.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Prestige/PrestigeFormula.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Prestige/PrestigeMilestone.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Prestige/PrestigeManager.cs`
- **Assigned role**: developer
- **Dependencies**: Step 3, Step 4
- **Parallelizable**: No

### Step 6: Save/Load & Offline Progress System
- **Description**: Implement JSON serialization DTOs, versioning, atomic file/PlayerPrefs storage, and offline time-elapsed progression calculations.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/Save/SaveDataDTO.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Save/SaveManager.cs`
- **Assigned role**: developer
- **Dependencies**: Step 3, Step 4, Step 5
- **Parallelizable**: Yes

### Step 7: UI Suite & Skill Tree Graph Visualizer
- **Description**: Build plug-and-play UGUI components: dynamic currency displays, generator cards, interactive Skill Tree canvas with pan/zoom controls, procedural line connectors (`SkillTreeConnectorRenderer`), distinct node UI styling for Minor / Notable / Keystone / Gateway archetypes, hover/tap tooltips, respec button controls, and prestige modals.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/UI/CurrencyDisplayUI.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/UI/GeneratorCardUI.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/UI/SkillTreeUI.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/UI/SkillNodeUI.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/UI/SkillTreeConnectorRenderer.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/UI/SkillTooltipUI.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/UI/PrestigeModalUI.cs`
- **Assigned role**: developer
- **Dependencies**: Step 3, Step 4, Step 5
- **Parallelizable**: No

### Step 8: Internal Testing Tools & Editor Simulator
- **Description**: Create in-game `DebugCheatUI` (hotkey-activated HUD with time scale sliders, resource cheats, skill unlocks, save wiping) and Unity Editor `SimulationWindow` + `SkillTreeValidator` (validating broken references, cycle detection, economy balance graphing).
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/Testing/DebugCheatManager.cs`
  - `Assets/FiniteSkillTreeIncremental/Runtime/Testing/DebugCheatUI.cs`
  - `Assets/FiniteSkillTreeIncremental/Editor/SkillTreeValidator.cs`
  - `Assets/FiniteSkillTreeIncremental/Editor/SimulationWindow.cs`
  - `Assets/FiniteSkillTreeIncremental/Editor/BigNumberDrawer.cs`
- **Assigned role**: developer
- **Dependencies**: Step 6, Step 7
- **Parallelizable**: Yes

### Step 9: Assembly Definitions, Sample Scene & Complete Integration
- **Description**: Configure asmdef files, create sample ScriptableObject assets (Gold, Gems, Prestige Shards, 10-node Skill Tree, 3 Generators), and build a ready-to-run playable demo scene connecting all 5 pillars.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/Runtime/FiniteSkillTreeIncremental.Runtime.asmdef`
  - `Assets/FiniteSkillTreeIncremental/Editor/FiniteSkillTreeIncremental.Editor.asmdef`
  - `Assets/FiniteSkillTreeIncremental/Samples/DemoScene.unity`
  - `Assets/FiniteSkillTreeIncremental/Samples/DemoManager.cs`
- **Assigned role**: developer
- **Dependencies**: Step 1 through Step 8
- **Parallelizable**: No

---

### Step 10: Verification & Testing
- **Description**: Comprehensive validation covering compilation, runtime behavior, play mode testing, simulation tooling, and UI interactions.
- **Assigned role**: developer
- **Dependencies**: Step 1 through Step 9
- **Parallelizable**: No

### Step 11: Comprehensive Documentation & User Guide
- **Description**: Write complete, crystal-clear markdown documentation (`README.md` and `Assets/FiniteSkillTreeIncremental/DOCUMENTATION.md`) detailing architecture, setup steps, plug-and-play component guides, scriptable asset workflows, code snippets, and API references.
- **Files**:
  - `Assets/FiniteSkillTreeIncremental/DOCUMENTATION.md`
  - `README.md`
- **Assigned role**: developer
- **Dependencies**: Step 1 through Step 10
- **Parallelizable**: No

1. **BigNumber Arithmetic & Formatting Tests**:
   - Verify addition, multiplication, division, and exponent boundaries (`1e1000+`).
   - Test notation formatting: `1,000` -> `1.00K`, `1,000,000` -> `1.00M`, `1e15` -> `1.00Qa` / `1.00e15`.
2. **Currency & Generator Verification**:
   - Verify accumulation rates per second match configured generator base rate * active stat multipliers.
   - Verify spend/canAfford safety guards (cannot spend below zero).
3. **Skill Tree Graph & Prerequisite Verification**:
   - Test multi-rank node upgrades and rank capping.
   - Test prerequisite locking (child node cannot be purchased until parent node reaches required rank).
   - Test stat modifier application on purchase and reversal on reset.
4. **Prestige Reset & Persistence Verification**:
   - Trigger prestige reset: verify primary currencies and generators reset to initial levels while prestige currency increases and permanent skill nodes persist according to configuration.
5. **Testing & Cheat Tool Verification**:
   - Test time-scale warp (1x to 100x) updates generators accurately.
   - Test "+1e6 Currency", "Unlock All Nodes", and "Wipe Save" actions in runtime debug panel.
   - Run Editor `SkillTreeValidator` to ensure graph cycle detection correctly flags invalid node loops.
