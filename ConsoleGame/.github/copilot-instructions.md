や「ネスト構造の初期化を安全にやるパターン（defaultdict vs dataclass + __post_init__）# GameEngine - AI Coding Agent Instructions

## Project Overview
This is a C# console-based RPG game engine built on .NET 8.0 with a layered architecture implementing Factory, Strategy, and Manager patterns. The game features turn-based combat, equipment systems, and YAML-driven enemy configuration.

## Architecture & Key Patterns

### Core System Flow
- **Entry Point**: `Program.cs` initializes player and starts main game loop via `GameSystem.Encounter()`
- **Event System**: Random encounters trigger either Shop or Battle events (1/3 shop, 2/3 battle ratio)
- **Combat Flow**: Turn-based via `GameSystem.BattleStart()` with player strategy selection through `UserInteraction.SelectAttackStrategy()`

### Critical Design Patterns
- **Strategy Pattern**: Attack strategies (`Default`/`Melee`/`Magic`) via `IAttackStrategy` - used by both players and enemies
- **Factory Pattern**: Enemy creation from YAML specs via `EnemyFactory.Create()` and weapon generation via `WeaponFactory`
- **Manager Pattern**: Composition-based resource management (`HealthManager`, `InventoryManager`, `ExperienceManager`)

### Data Flow Architecture
```
Program -> GameSystem -> [ShopSystem | BattleSystem] -> Player/Enemy interactions -> Managers
```

## Configuration & Dependencies
や「ネスト構造の初期化を安全にやるパターン（defaultdict vs dataclass + __post_init__）
### YAML Configuration System
- **Enemy Specs**: `enemy-specs.yml` defines all enemy types with HP, AP, DP, AttackStrategy, Experience
- **Factory Integration**: `EnemyFactory` deserializes YAML at startup using YamlDotNet
- **Strategy Mapping**: String-to-strategy conversion in both `EnemyFactory.Create()` and `AttackStrategy.GetAttackStrategy()`

### Required Dependencies
- **YamlDotNet** (v16.3.0): YAML deserialization for enemy configuration
- **.NET 8.0**: Target framework with nullable reference types enabled

## Key Development Patterns

### Interface Segregation
- `ICharacter`: Base combat interface for HP, attack, damage
- `IPlayer`: Extends ICharacter with inventory, gold, equipment methods  
- `IEnemy`: Extends ICharacter with enemy-specific properties
- `IAttackStrategy`: Combat strategy interface with `ExecuteAttack()` and `GetAttackStrategyName()`

### Manager Composition Pattern
Player class uses composition with:
- `HealthManager`: HP/DP calculations with equipment stat integration
- `InventoryManager`: Weapon/potion storage implementing `IEquipmentStatsProvider`
- `ExperienceManager`: Level progression and stat growth

### Console Interaction Patterns
- **Strategy Selection**: Arrow key navigation with real-time console updating in `UserInteraction.SelectAttackStrategy()`
- **Input Validation**: Robust positive integer input with quit options via `UserInteraction.ReadPositiveInteger()`
- **Console Management**: ANSI escape sequences for cursor control (`\x1b[1A`, `\x1b[2K`)

## Build & Deployment

### Development Commands
```bash
dotnet build                                    # Standard build
dotnet run --project GameEngine               # Run development version
```

### Production Deployment
- **YAML File Handling**: `enemy-specs.yml` marked as `<CopyToOutputDirectory>Always</CopyToOutputDirectory>`
- **Cross-Platform**: Linux builds available in `publish/linux-x64/`
- **Asset Management**: YAML config copied to output directory automatically

## Extension Points

### Adding New Enemies
1. Add entry to `enemy-specs.yml` with required fields: Name, HP, AttackStrategy, Experience, AP, DP
2. No code changes needed - factory automatically deserializes new entries

### Adding Attack Strategies
1. Implement `IAttackStrategy` with `ExecuteAttack()` and `GetAttackStrategyName()`
2. Update `AttackStrategy.GetAttackStrategy()` switch statement
3. Add to `UserInteraction.SelectAttackStrategy()` array
4. Update YAML specs to reference new strategy name

### Adding Weapons/Items
1. Extend `WeaponFactory` with new weapon types
2. Add to `ShopSystem` purchase options
3. Ensure `IWeapon` implementation includes AP/DP stats for `HealthManager` integration

## Critical Implementation Notes

### YAML Integration
- Static constructor in `EnemyFactory` loads specs once at startup
- Error handling for missing/malformed YAML files
- String-based strategy mapping requires exact name matching between YAML and code

### Manager Dependencies
- `HealthManager` depends on `IEquipmentStatsProvider` for stat calculations
- `Player` composition requires all three managers at construction
- Cross-manager communication through player interface methods

### Console State Management
- Game loop continues until `player.IsAlive == false`
- Console clearing uses ANSI sequences - ensure terminal compatibility
- Strategy selection blocks on user input with arrow key navigation

This architecture prioritizes extensibility through YAML configuration and clean separation of concerns via the manager pattern.