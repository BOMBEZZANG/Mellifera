# Mellifera: Hive OS - Game Prototype

A 2D pixel art colony survival simulation game developed in Unity for PC, where players manage a bee colony to survive their first winter.

## Project Overview

**Genre:** Colony Survival Simulation / Management / Base-Building  
**Art Style:** 2D Pixel Art  
**Target Platform:** PC (Steam)  
**Development Engine:** Unity Engine  

**Core Concept:** Players become the collective consciousness of a bee colony, managing worker bees with finite lifespans, navigating generational changes, and adapting to the environment to survive the harsh realities of nature.

## Prototype Goal

Validate the core gameplay loop: "Survive the First Winter" - Test if players can successfully manage a small, aging bee colony by balancing internal resource management and external exploration.

## Architecture

This project follows a **modular, event-driven architecture** to avoid hard-coded dependencies between systems. The TimeManager broadcasts events that other systems listen to, rather than being directly called.

### Core Code Structure

```
Assets/Scripts/
├── Core/                    # Core game managers
│   ├── GameManager.cs       # Main game state management
│   ├── TimeManager.cs       # Time flow and cycles
│   ├── ResourceManager.cs   # Resource inventory and consumption
│   ├── BeeManager.cs        # Bee lifecycle management
│   ├── TaskManager.cs       # Task assignment system
│   └── ConfigurationManager.cs # Game balancing configuration
├── Units/                   # Game entities
│   ├── Bee.cs              # Base bee behavior
│   ├── QueenBee.cs         # Queen-specific behavior
│   └── Brood.cs            # Egg/Larva/Pupa lifecycle
├── Systems/                 # Game systems
│   └── HiveCell.cs         # Hexagonal cell management
├── Controllers/             # Game controllers
│   └── ExternalMapController.cs # Foraging map management
├── Data/                    # Data structures
│   ├── ResourceType.cs     # Resource definitions
│   ├── BeeData.cs          # Bee-related data structures
│   ├── HiveCellData.cs     # Cell-related data
│   ├── TaskData.cs         # Task system data
│   ├── ForagingData.cs     # Foraging-related data
│   └── GameConfiguration.cs # ScriptableObject for balancing
├── Events/                  # Event system
│   └── GameEvents.cs       # Central event broadcasting
└── UI/                     # User interface
    ├── ResourceDisplayUI.cs # Resource and time display
    ├── NotificationSystem.cs # Game notifications
    └── CommandPanel.cs      # Player commands
```

## Key Features

### 1. Time & Season System
- **1 Cycle (Day):** 5 real-world minutes
- **Day/Night Cycle:** Day phase (3.5 mins), Night phase (1.5 mins)
- **Seasonal Rotation:** Spring (10) → Summer (15) → Autumn (10) → Winter (10) cycles
- **Winter Challenge:** No external activities, doubled honey consumption

### 2. Resource & Entropy System
- **4 Core Resources:** Honey, Pollen, Beeswax, Royal Jelly
- **Continuous Consumption:** Bees consume Honey, Queen consumes Royal Jelly
- **Resource Conversion:**
  - Honey + Pollen → Royal Jelly
  - Honey + Pollen → Beeswax

### 3. Units & Life Cycle
- **Queen Bee:** Lays eggs, requires Royal Jelly
- **Worker Bees:** Finite lifespan (20 cycles), various roles
- **Brood:** Egg (3 days) → Larva (6) → Pupa (12) → Adult

### 4. Hive Management
- **Cell Types:** Basic, Nursery, Honey Storage, Pollen Storage
- **Temperature Management:** Nursery cells require heating
- **Construction:** Bees build and maintain hive infrastructure

### 5. Foraging System
- **Pioneer-then-Automate:** Player guides first exploration, then automates
- **External Map:** 2D side-scrolling with resource nodes and hazards
- **Pathfinding:** Routes are recorded and reused

### 6. Task Management
- **Automatic Assignment:** Idle bees assigned to highest priority tasks
- **Task Types:** Build, Supply, Clean, Forage, Thermoregulate
- **Priority System:** Critical, High, Medium, Low

## Technical Implementation

### Event-Driven Communication
```csharp
// Example: Time events trigger other systems
GameEvents.OnCycleEnd += HandleCycleEnd;
GameEvents.OnNewDay += HandleNewDay;
GameEvents.OnSeasonChanged += HandleSeasonChanged;
```

### Modular Resource Management
```csharp
// Resources are managed centrally but accessed through events
ResourceManager.OnResourceChanged += HandleResourceChanged;
resourceManager.AddResource(ResourceType.Honey, amount);
```

### Configurable Balancing
All key variables are adjustable through the `GameConfiguration` ScriptableObject:
- Bee lifespan
- Resource consumption rates
- Task priorities
- Seasonal durations
- Population limits

## Setup Instructions

1. **Unity Version:** 2022.3 LTS or later
2. **Dependencies:** 
   - Universal Render Pipeline (URP)
   - Input System
   - TextMeshPro
3. **Setup:**
   - Clone repository
   - Open in Unity
   - Load `SampleScene` in Assets/Scenes/
   - Create a GameConfiguration asset via `Mellifera/Create Game Configuration`

## Configuration

### Game Balance
Edit the `GameConfiguration` ScriptableObject to adjust:
- Cycle duration (default: 5 minutes)
- Bee lifespan (default: 20 cycles)
- Resource consumption rates
- Population limits
- Difficulty multipliers

### Debug Features
- Enable debug mode for additional logging
- Show debug gizmos for visual debugging
- Performance metrics tracking
- Console logging toggle

## Success Criteria

The prototype validates these core questions:
1. Do players recognize "Surviving the Winter" as their main goal?
2. Do players feel pressure from dwindling resources and aging bees?
3. Do players engage in strategic decisions between resource gathering and brood rearing?
4. Upon surviving winter, do players feel accomplishment and desire for growth?

## Performance Requirements

- **Target:** 50 bee units on screen simultaneously
- **Stable Core Loop:** Aging, resource consumption, brood rearing, foraging
- **Optimization:** Event-driven architecture reduces coupling and improves performance

## Future Expansion

The modular architecture supports easy expansion:
- Additional bee types and roles
- More complex hive structures
- Seasonal events and challenges
- Multiplayer colony interactions
- Advanced AI behaviors
- Extended progression systems

## Development Notes

### Key Design Principles
1. **Modularity:** Each system is independent and communicates through events
2. **Configurability:** All balance parameters are easily adjustable
3. **Scalability:** Architecture supports adding new features without major refactoring
4. **Performance:** Efficient update loops and object pooling where needed

### Code Conventions
- Event-driven communication between systems
- ScriptableObject-based configuration
- Namespace organization (`Mellifera.Core`, `Mellifera.Units`, etc.)
- Consistent naming conventions and documentation

## License

This project is developed as a prototype for educational and demonstration purposes.

---

*Generated with Claude Code - A collaborative development approach*