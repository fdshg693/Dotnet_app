## Project Overview

This is a C# console RPG engine on .NET 8.0. The runtime is a layered system that combines Factory, Strategy, and Manager patterns to drive turn-based combat, equipment, and YAML-configured enemies.

## Architecture & Data Flow

- **Entry/loop**: [GameEngine/Program.cs](GameEngine/Program.cs) initializes the player and hands control to `GameSystem.Encounter()`.
- **Event routing**: [GameEngine/Systems/GameSystem.cs](GameEngine/Systems/GameSystem.cs) decides between shop vs battle (1/3 shop, 2/3 battle).
- **Combat**: [GameEngine/Systems/BattleSystem/BattleManager.cs](GameEngine/Systems/BattleSystem/BattleManager.cs) coordinates turns; player strategy selection lives in [GameEngine/Systems/UserInteraction.cs](GameEngine/Systems/UserInteraction.cs).
- **Composition**: `Player` owns `HealthManager`, `InventoryManager`, `ExperienceManager` (see [GameEngine/Models/Player.cs](GameEngine/Models/Player.cs) and [GameEngine/Manager](GameEngine/Manager)).

```
Program -> GameSystem -> [ShopSystem | BattleSystem] -> Player/Enemy -> Managers
```

## Core Patterns (project-specific)

- **Strategy**: `IAttackStrategy` with `Default`/`Melee`/`Magic`. Mapping is string-based in `AttackStrategy.GetAttackStrategy()` and `EnemyFactory.Create()` (keep names aligned with YAML).
- **Factory**: `EnemyFactory` loads YAML specs at startup and creates enemies; `WeaponFactory` centralizes weapon creation.
- **Managers**: `HealthManager` uses `IEquipmentStatsProvider` from inventory to compute HP/AP/DP; `ExperienceManager` drives level growth.

## Configuration & External Dependencies

- **YAML**: [GameEngine/enemy-specs.yml](GameEngine/enemy-specs.yml) and [GameEngine/weapon-specs.yml](GameEngine/weapon-specs.yml). Specs are deserialized via YamlDotNet (see [GameEngine/Factory/EnemyFactory.cs](GameEngine/Factory/EnemyFactory.cs)).
- **Save system**: MongoDB via Docker Compose (see [docker-compose.yml](docker-compose.yml) and [docs/mongo.md](docs/mongo.md)); persistence code sits in [GameEngine/Manager/SaveDataManager.cs](GameEngine/Manager/SaveDataManager.cs).

## Developer Workflows

- Build: `dotnet build`
- Run: `dotnet run --project GameEngine`
- Tests: `dotnet test`
- Save feature: run `docker-compose up -d` first (MongoDB + Mongo Express).

## Extension Guidelines (existing conventions)

- **Add enemy**: update [GameEngine/enemy-specs.yml](GameEngine/enemy-specs.yml); no code change unless you add a new `AttackStrategy` name.
- **Add attack strategy**: implement `IAttackStrategy`, add mapping in [GameEngine/Models/AttackStrategy.cs](GameEngine/Models/AttackStrategy.cs), and add UI option in [GameEngine/Systems/UserInteraction.cs](GameEngine/Systems/UserInteraction.cs).
- **Add weapons/items**: extend [GameEngine/Factory/WeaponFactory.cs](GameEngine/Factory/WeaponFactory.cs) and surface in [GameEngine/Systems/ShopSystem.cs](GameEngine/Systems/ShopSystem.cs).

## Console Interaction Details

- Strategy selection uses arrow keys and redraws via ANSI escapes (`\x1b[1A`, `\x1b[2K`) in [GameEngine/Systems/UserInteraction.cs](GameEngine/Systems/UserInteraction.cs).
- Input validation favors `ReadPositiveInteger()` with quit options.
