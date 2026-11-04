# Finite Skill Tree & Incremental Game Framework
*A plug-and-play Unity engine and toolkit for building incremental, idle, and clicker games with finite skill trees, prestige mechanics, and internal testing suites.*

---

## Table of Contents
1. [Overview & Features](#overview--features)
2. [Architecture & Folder Structure](#architecture--folder-structure)
3. [Quick Start (Plug and Play in 5 Minutes)](#quick-start-plug-and-play-in-5-minutes)
4. [Subsystem Deep Dives](#subsystem-deep-dives)
   - [1. Mathematical Engine & `BigNumber`](#1-mathematical-engine--bignumber)
   - [2. Multi-Currency Economy & Generators](#2-multi-currency-economy--generators)
   - [3. Finite Skill Tree (Nodebuster / PoE Archetypes)](#3-finite-skill-tree-nodebuster--poe-archetypes)
   - [4. Prestige & Metaprogression Layer (Digseum-style)](#4-prestige--metaprogression-layer-digseum-style)
   - [5. Save/Load & Offline Progress](#5-saveload--offline-progress)
   - [6. UI Components & Graph Visualizer](#6-ui-components--graph-visualizer)
   - [7. Internal Testing Suite & Editor Balance Tools](#7-internal-testing-suite--editor-balance-tools)
5. [Code Examples & Connection Points](#code-examples--connection-points)
6. [Assembly Definitions](#assembly-definitions)

---

## Overview & Features

This framework is built specifically for designers and developers who want a fully decoupled, extensible library to create incremental games without rebuilding core systems from scratch.

- **High-Precision Scientific Math (`BigNumber`)**: Supports unlimited number scaling (up to $10^{9,223,372,036,854,775,807}$) with standard suffix abbreviations (`1.25M`, `4.50B`, `10.00Qa`), scientific (`1.25e6`), and engineering notations.
- **Dynamic Stat Pipeline**: Layered modifier architecture (`Flat`, `PercentAdd`, `PercentMult`) that automatically calculates production multipliers, click powers, generator speeds, and cost discounts.
- **Nodebuster & Path of Exile Inspired Skill Trees**:
  - **Minor Nodes**: Incremental stat buffs.
  - **Notable Nodes**: High-impact multipliers and perks.
  - **Keystone Nodes**: Game-altering build-defining rule changers.
  - **Gateway Nodes**: Bridge connections unlocking outer constellation clusters.
  - **Adjacency Synergy**: Bonus modifier efficiency for active neighboring nodes sharing synergy tags.
  - **Full Respec & Refund Support**: Safe refund validation that prevents breaking child prerequisite chains.
- **Digseum-Inspired Metaprogression & Prestige**:
  - Customizable mathematical reward formulas (`SquareRoot`, `CubeRoot`, `LinearRatio`, `Logarithmic`, `CustomExponent`).
  - Layered reset configurations (currencies, generators, skill trees).
  - Permanent Relic/Exhibit Milestones that persist across all runs.
- **Persistence & Offline Earnings**: Automatic PlayerPrefs / JSON file saving with timestamp-based offline production calculation.
- **Internal Testing & Balance Suite**:
  - In-game Cheat HUD (`F1` or `~`) with time-dilation (`1x`, `5x`, `20x`, `100x`), instant resource grants, skill unlocks, and save wiping.
  - Unity Editor **Skill Tree Graph Validator** (validates broken references and circular prerequisite loops).
  - Unity Editor **Economy Balance Simulator** (simulates 24+ hours of optimal idle progression in milliseconds).

---

## Architecture & Folder Structure

```
Assets/FiniteSkillTreeIncremental/
├── Runtime/
│   ├── Core/
│   │   ├── BigNumber.cs               # Core math struct
│   │   ├── BigNumberNotation.cs       # Notation enum (Standard, Scientific, Engineering)
│   │   ├── Stats/
│   │   │   ├── StatType.cs            # Stat enums & modifier types
│   │   │   ├── StatModifier.cs        # Modifier data container
│   │   │   └── StatPipeline.cs        # Math aggregation engine
│   │   └── Events/
│   │       └── SignalBus.cs           # Decoupled type-safe publish/subscribe bus
│   ├── Currencies/
│   │   ├── CurrencyDefinition.cs      # ScriptableObject for currencies
│   │   ├── CurrencyWallet.cs          # Runtime wallet & transactions
│   │   ├── CurrencyGenerator.cs       # Idle generator model
│   │   └── CurrencyManager.cs         # Central wallet & generator ticker
│   ├── SkillTree/
│   │   ├── SkillNodeType.cs           # Minor, Notable, Keystone, Gateway enums
│   │   ├── SkillNodeDefinition.cs     # ScriptableObject node definition
│   │   ├── SkillTreeDefinition.cs     # ScriptableObject tree graph definition
│   │   ├── SkillNodeState.cs          # Runtime rank & allocation state
│   │   └── SkillTreeController.cs     # Tree allocation & prerequisite engine
│   ├── Prestige/
│   │   ├── PrestigeFormula.cs         # Prestige math & milestone models
│   │   ├── PrestigeLayerDefinition.cs # ScriptableObject layer definition
│   │   └── PrestigeManager.cs         # Reset manager & milestone engine
│   ├── Save/
│   │   ├── SaveDataDTO.cs             # Serializable JSON DTOs
│   │   └── SaveManager.cs             # Auto-save & offline progress manager
│   ├── UI/
│   │   ├── CurrencyDisplayUI.cs       # Real-time formatted currency text
│   │   ├── GeneratorCardUI.cs         # Buyable generator card component
│   │   ├── SkillTreeUI.cs             # Pan/zoom interactive skill tree canvas
│   │   ├── SkillNodeUI.cs             # Visual node button with archetype styling
│   │   ├── SkillTreeConnectorRenderer.cs # Procedural UGUI mesh line renderer
│   │   ├── SkillTooltipUI.cs          # Dynamic hover/tap tooltip card
│   │   └── PrestigeModalUI.cs         # Confirmation & rewards modal
│   └── Testing/
│       ├── DebugCheatManager.cs       # Runtime cheat hooks & time warp
│       └── DebugCheatUI.cs            # Hotkey-activated cheat overlay
├── Editor/
│   ├── BigNumberDrawer.cs             # Custom inspector drawer for BigNumber
│   ├── SkillTreeValidator.cs          # Graph cycle & broken reference checker
│   └── SimulationWindow.cs            # Fast offline balance simulation window
└── Samples/
    ├── DemoScene.unity                # Ready-to-play sample scene
    ├── DemoManager.cs                 # Sample scene connector script
    ├── Data/                          # Sample currencies, skill nodes, and trees
    └── Prefabs/                       # Prefabs (SkillNodeUI, etc.)
```

---

## Quick Start (Plug and Play in 5 Minutes)

### 1. Open the Sample Scene
Navigate to `Assets/FiniteSkillTreeIncremental/Samples/DemoScene.unity` and press **Play**.
- Click **"Mine Gold"** to earn gold.
- Buy **Novice Miners** and **Steam Excavators** to automate generation.
- Click skill tree nodes on the right panel to unlock passive boosts, notable perks, and keystones.
- Press **`F1`** or **`~`** to open the internal cheat menu (grant gold, unlock all skills, fast-forward 1 hour, or warp time up to 100x).
- Click **"Prestige & Transcend"** to reset standard progress for permanent Prestige Shards.

### 2. Creating New Assets
1. **New Currency**: Right-click in Project view $\rightarrow$ `Create` $\rightarrow$ `Incremental` $\rightarrow$ `Currency Definition`.
2. **New Skill Node**: Right-click $\rightarrow$ `Create` $\rightarrow$ `Incremental` $\rightarrow$ `Skill Node Definition`.
3. **New Skill Tree**: Right-click $\rightarrow$ `Create` $\rightarrow$ `Incremental` $\rightarrow$ `Skill Tree Definition`. Drag your skill nodes into the tree's `Nodes` list.
4. **New Prestige Layer**: Right-click $\rightarrow$ `Create` $\rightarrow$ `Incremental` $\rightarrow$ `Prestige Layer Definition`.

---

## Subsystem Deep Dives

### 1. Mathematical Engine & `BigNumber`
`BigNumber` represents values as `mantissa * 10^exponent`. It supports all standard C# arithmetic operators (`+`, `-`, `*`, `/`, `>`, `<`, `==`, `!=`), implicit conversions from `double`/`float`/`int`/`long`, and string parsing.

```csharp
using FiniteSkillTreeIncremental.Core;

BigNumber a = new BigNumber(1.5, 6); // 1.50M
BigNumber b = 500000;                // 500.00K
BigNumber c = a + b;                 // 2.00M

string formatted = c.ToString(BigNumberNotation.Standard, 2); // "2.00M"
string sci = c.ToString(BigNumberNotation.Scientific, 2);      // "2.00e6"
```

### 2. Multi-Currency Economy & Generators
The `CurrencyManager` manages all wallets and generator accumulation ticks:
- `CurrencyDefinition`: ScriptableObject defining ID, display name, icon, formatting rules, and caps.
- `CurrencyWallet`: Tracks current balance, lifetime earned, and rate/second.
- `CurrencyGenerator`: Exponential cost scaling (`BaseCost * GrowthRate^Level`), production rates, and bulk purchase logic.

```csharp
var goldWallet = CurrencyManager.Instance.GetWallet("gold");
if (goldWallet.CanAfford(100))
{
    goldWallet.TrySpend(100);
}
```

### 3. Finite Skill Tree (Nodebuster / PoE Archetypes)
- **Node Types**:
  - `Minor`: Baseline incremental improvements (+15% Gold).
  - `Notable`: Significant multipliers (Triples Click Power).
  - `Keystone`: Rule-defining modifiers (Doubles global production).
  - `Gateway`: Bridge connecting to outer clusters or prestige tiers.
- **Adjacency Synergy**: Setting `SynergyTags` (e.g. `["Mining"]`) allows adjacent allocated nodes to grant stacking percentage bonuses to modifier effectiveness.
- **Prerequisites & Refunding**: The `SkillTreeController` prevents upgrading locked nodes and prevents refunding parent nodes if active child nodes depend on them.

### 4. Prestige & Metaprogression Layer (Digseum-style)
- `PrestigeLayerDefinition`: Configures source currency (e.g. Gold), reward currency (e.g. Prestige Shards), requirement, and reward formula.
- `PrestigeMilestone`: Permanent perks unlocked when lifetime prestiges or lifetime prestige currency thresholds are met (e.g. "+50% Global Production" on 1st prestige).

```csharp
var presMgr = PrestigeManager.Instance;
if (presMgr.CanPrestige(layer))
{
    presMgr.ExecutePrestige(layer);
}
```

### 5. Save/Load & Offline Progress
The `SaveManager` automatically saves game state to `PlayerPrefs` or JSON files on application pause/quit or every 30 seconds.
- **Offline Earnings**: When the player launches the game, the elapsed time since `timestampUnixUtc` is calculated, running generator production up to `maxOfflineHours` (e.g. 24 hours).

### 6. UI Components & Graph Visualizer
- `SkillTreeUI`: Handles mouse drag panning and wheel scroll zooming with customizable limits.
- `SkillTreeConnectorRenderer`: Procedurally renders colored line segments between parent and child nodes on the UGUI canvas, dynamically updating colors (Locked = Gray, Available = Yellow, Active = Cyan, Maxed = Green).
- `SkillNodeUI`: Displays rank, cost, and icon, with distinct dimensions and colors per archetype (Minor, Notable, Keystone, Gateway). Left-click upgrades; right-click refunds.
- `SkillTooltipUI`: Smooth hover card detailing node attributes, archetype, rank, cost, and active synergies.

### 7. Internal Testing Suite & Editor Balance Tools
- **Runtime Debug HUD (`F1` / `~`)**:
  - Fast-forward time scale (1x, 5x, 20x, 100x).
  - Fast-forward 1 hour of generator ticks instantly.
  - Grant +1.00M Gold, +1.00B Gold, +100 Prestige Shards.
  - Unlock all skill tree nodes or wipe save data.
- **Editor Graph Validator**: Access via `Tools` $\rightarrow$ `Incremental` $\rightarrow$ `Skill Tree Validator`. Checks for missing references, orphan dependencies, and circular prerequisite loops (DFS cycle detection).
- **Editor Economy Simulator**: Access via `Tools` $\rightarrow$ `Incremental` $\rightarrow$ `Economy Balance Simulator`. Simulates 24+ hours of automated generator purchases and logs balance and rate curves.

---

## Code Examples & Connection Points

### Listening to Decoupled Signals via `SignalBus`
```csharp
using FiniteSkillTreeIncremental.Core.Events;
using FiniteSkillTreeIncremental.Currencies;
using FiniteSkillTreeIncremental.SkillTree;
using UnityEngine;

public class GameObserver : MonoBehaviour
{
    private void OnEnable()
    {
        SignalBus.Subscribe<CurrencyChangedSignal>(OnCurrencyChanged);
        SignalBus.Subscribe<SkillRankChangedSignal>(OnSkillRankChanged);
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<CurrencyChangedSignal>(OnCurrencyChanged);
        SignalBus.Unsubscribe<SkillRankChangedSignal>(OnSkillRankChanged);
    }

    private void OnCurrencyChanged(CurrencyChangedSignal signal)
    {
        Debug.Log($"Currency {signal.Currency.DisplayName} updated to {signal.NewBalance}");
    }

    private void OnSkillRankChanged(SkillRankChangedSignal signal)
    {
        Debug.Log($"Node {signal.Node.Title} changed from rank {signal.OldRank} to {signal.NewRank}");
    }
}
```

### Adding Custom Stat Modifiers at Runtime
```csharp
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Stats;
using FiniteSkillTreeIncremental.Currencies;

// Add a temporary 2x global production buff (+100% PercentMult)
var buff = new StatModifier(StatType.GlobalProductionMultiplier, ModifierType.PercentMult, new BigNumber(1.0), this);
CurrencyManager.Instance.StatPipeline.AddModifier(buff);

// Remove the buff later
CurrencyManager.Instance.StatPipeline.RemoveModifier(buff);
```

---

## Assembly Definitions
- **`FiniteSkillTreeIncremental.Runtime`**: Standalone runtime assembly referencing `Unity.TextMeshPro`, `Unity.InputSystem`, and `Unity.UGUI`.
- **`FiniteSkillTreeIncremental.Editor`**: Editor tooling assembly containing the inspector property drawers, skill tree graph validator, and simulation windows.
