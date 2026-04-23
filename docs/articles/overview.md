---
uid: overview
---

# Architecture Overview

## Dependency direction

```
ASLInputTool → ASL → HexLib
```

**HexLib** is pure geometry with no game logic. **ASL** adds the domain model (units, boards, rules, persistence). **ASLInputTool** is the WPF editor that depends on both.

## Component model

`Unit` is composed from `IUnitComponent` implementations (e.g., `InfantryComponent`, `LeadershipComponent`, `FirePowerComponent`). Add behaviour by adding components, not by subclassing.

## Rules pipeline

`IRulePipeline<TContext>` executes `IRule<TContext>` implementations in `RulePriority` order. `Core` rules run before `SSR` rules.
