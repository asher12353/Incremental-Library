# Finite Skill Tree & Incremental Game Framework

A plug-and-play Unity library and toolkit designed to accelerate the development of incremental, idle, and clicker games featuring finite multi-tiered skill trees, multi-currency economies, customizable prestige layers, decoupled event architectures, and comprehensive internal testing/cheat suites.

## Core Pillars
1. **Arbitrary-Scale Mathematics (`BigNumber`)**: Scientific notation arithmetic struct (`mantissa * 10^exponent`) with full operator support and notation formatting (`Standard`, `Scientific`, `Engineering`, `ShortWords`).
2. **Multi-Currency & Idle Generators**: ScriptableObject-driven currencies, wallets, automated tick generators, offline progress calculations, and automatic PlayerPrefs / JSON persistence.
3. **Finite Skill Tree Subsystem**: Inspired by *Nodebuster* and *Path of Exile*. Supports **Minor**, **Notable**, **Keystone**, and **Gateway** node archetypes, prerequisite graphs, tag-based adjacency synergies, and safe dependency-checked respec refunding.
4. **Prestige & Metaprogression Layer**: Inspired by *Digseum*. Multi-tier prestige resets, configurable mathematical formulas (SquareRoot, CubeRoot, Logarithmic, CustomExponent), and permanent Exhibit/Relic Milestones.
5. **Decoupled Connection Points**: High-performance `StatPipeline` (Flat, PercentAdd, PercentMult) and type-safe `SignalBus` publish/subscribe event channels.
6. **Internal Testing & Designer Suite**: In-game Cheat HUD (`F1` / `~`), Time-Warp acceleration (`1x` to `100x`), Editor Skill Tree Graph Validator (DFS cycle detection), and Editor Economy Balance Simulator.

## Getting Started
- Open `Assets/FiniteSkillTreeIncremental/Samples/DemoScene.unity` and press Play.
- Read full documentation and API guide in [`Assets/FiniteSkillTreeIncremental/DOCUMENTATION.md`](Assets/FiniteSkillTreeIncremental/DOCUMENTATION.md).
