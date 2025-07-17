using UnityEngine;

namespace Mellifera.Data
{
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "Mellifera/Game Configuration")]
    public class GameConfiguration : ScriptableObject
    {
        [Header("Time Configuration")]
        public float cycleDuration = 300f; // 5 minutes in seconds
        public float dayPhaseRatio = 0.7f; // 70% day, 30% night
        public int springCycles = 10;
        public int summerCycles = 15;
        public int autumnCycles = 10;
        public int winterCycles = 10;
        
        [Header("Bee Configuration")]
        public int maxBeePopulation = 100;
        public float beeLifespan = 20f; // in cycles
        public float beeSpeed = 5f;
        public float beeCarryCapacity = 2f;
        public float beeWorkEfficiency = 1f;
        public float beeHealthRegenRate = 0.1f;
        
        [Header("Queen Configuration")]
        public float queenLifespan = 100f; // in cycles
        public float queenHungerDecayRate = 5f; // per cycle
        public float queenEggLayInterval = 30f; // seconds
        public int queenMaxEggsPerLaying = 3;
        
        [Header("Brood Configuration")]
        public float eggDuration = 3f; // in cycles
        public float larvaDuration = 6f; // in cycles
        public float pupaDuration = 12f; // in cycles
        public float broodNutritionDecayRate = 0.1f;
        public float broodRequiredNutritionForGrowth = 0.8f;
        
        [Header("Resource Configuration")]
        public float startingHoney = 100f;
        public float startingPollen = 50f;
        public float startingBeeswax = 20f;
        public float startingRoyalJelly = 10f;
        
        [Header("Consumption Rates")]
        public float honeyConsumptionPerBeePerCycle = 2f;
        public float royalJellyConsumptionPerQueenPerCycle = 5f;
        public float winterHoneyConsumptionMultiplier = 2f;
        
        [Header("Resource Conversion")]
        public float honeyToRoyalJellyRatio = 2f;
        public float pollenToRoyalJellyRatio = 1f;
        public float royalJellyProductionAmount = 1f;
        
        public float honeyToBeeswaxRatio = 3f;
        public float pollenToBeeswaxRatio = 1f;
        public float beeswaxProductionAmount = 1f;
        
        [Header("Hive Cell Configuration")]
        public float basicCellCapacity = 1f;
        public float nurseryCellCapacity = 1f;
        public float honeyCellCapacity = 10f;
        public float pollenCellCapacity = 8f;
        public float nurseryTargetTemperature = 35f;
        public float cellTemperatureDecayRate = 2f; // degrees per second
        
        [Header("Task Configuration")]
        public int maxConcurrentTasks = 20;
        public float taskUpdateInterval = 1f;
        public float taskGenerationInterval = 5f;
        public float taskWorkDuration = 5f;
        
        [Header("Foraging Configuration")]
        public float foragingBeeSpeed = 5f;
        public float pathRecordingInterval = 0.5f;
        public int maxConcurrentForagers = 10;
        public float resourceNodeRegenRate = 1f; // per second
        public float resourceNodeHarvestRate = 2f; // per second
        
        [Header("Resource Node Configuration")]
        public float honeyNodeMaxCapacity = 100f;
        public float pollenNodeMaxCapacity = 80f;
        public int honeyNodeCount = 3;
        public int pollenNodeCount = 3;
        
        [Header("Hazard Configuration")]
        public float spiderDamage = 20f;
        public float spiderRadius = 3f;
        public float spiderActivationChance = 0.1f;
        public float spiderDuration = 5f;
        
        public float windDamage = 10f;
        public float windRadius = 5f;
        public float windActivationChance = 0.05f;
        public float windDuration = 10f;
        
        [Header("UI Configuration")]
        public float lowHoneyThreshold = 20f;
        public float lowPollenThreshold = 15f;
        public float lowRoyalJellyThreshold = 5f;
        public float notificationDuration = 5f;
        public int maxNotifications = 50;
        
        [Header("Audio Configuration")]
        public float masterVolume = 1f;
        public float sfxVolume = 0.8f;
        public float musicVolume = 0.6f;
        public float uiVolume = 0.7f;
        
        [Header("Balance Tweaks")]
        [Range(0.1f, 5f)]
        public float difficultyMultiplier = 1f;
        [Range(0.1f, 3f)]
        public float resourceGenerationMultiplier = 1f;
        [Range(0.1f, 3f)]
        public float populationGrowthMultiplier = 1f;
        [Range(0.1f, 3f)]
        public float workEfficiencyMultiplier = 1f;
        
        [Header("Debug Configuration")]
        public bool enableDebugMode = false;
        public bool showDebugGizmos = false;
        public bool enableConsoleLogging = true;
        public bool enablePerformanceMetrics = false;
        
        // Helper methods for easy access
        public float GetAdjustedCycleDuration()
        {
            return cycleDuration / difficultyMultiplier;
        }
        
        public float GetAdjustedLifespan()
        {
            return beeLifespan * difficultyMultiplier;
        }
        
        public float GetAdjustedResourceGeneration()
        {
            return resourceGenerationMultiplier;
        }
        
        public float GetAdjustedWorkEfficiency()
        {
            return beeWorkEfficiency * workEfficiencyMultiplier;
        }
        
        public float GetAdjustedHoneyConsumption()
        {
            return honeyConsumptionPerBeePerCycle * difficultyMultiplier;
        }
        
        public float GetAdjustedRoyalJellyConsumption()
        {
            return royalJellyConsumptionPerQueenPerCycle * difficultyMultiplier;
        }
        
        public int GetTotalCyclesInYear()
        {
            return springCycles + summerCycles + autumnCycles + winterCycles;
        }
        
        public float GetDayDuration()
        {
            return cycleDuration * dayPhaseRatio;
        }
        
        public float GetNightDuration()
        {
            return cycleDuration * (1f - dayPhaseRatio);
        }
        
        public float GetBroodStageDuration(BroodStage stage)
        {
            switch (stage)
            {
                case BroodStage.Egg: return eggDuration * cycleDuration;
                case BroodStage.Larva: return larvaDuration * cycleDuration;
                case BroodStage.Pupa: return pupaDuration * cycleDuration;
                default: return cycleDuration;
            }
        }
        
        public float GetCellCapacity(HiveCellType cellType)
        {
            switch (cellType)
            {
                case HiveCellType.Basic: return basicCellCapacity;
                case HiveCellType.Nursery: return nurseryCellCapacity;
                case HiveCellType.HoneyStorage: return honeyCellCapacity;
                case HiveCellType.PollenStorage: return pollenCellCapacity;
                default: return basicCellCapacity;
            }
        }
        
        public float GetTargetTemperature(HiveCellType cellType)
        {
            switch (cellType)
            {
                case HiveCellType.Nursery: return nurseryTargetTemperature;
                default: return 25f;
            }
        }
        
        public void ResetToDefaults()
        {
            cycleDuration = 300f;
            dayPhaseRatio = 0.7f;
            springCycles = 10;
            summerCycles = 15;
            autumnCycles = 10;
            winterCycles = 10;
            
            maxBeePopulation = 100;
            beeLifespan = 20f;
            beeSpeed = 5f;
            beeCarryCapacity = 2f;
            beeWorkEfficiency = 1f;
            beeHealthRegenRate = 0.1f;
            
            queenLifespan = 100f;
            queenHungerDecayRate = 5f;
            queenEggLayInterval = 30f;
            queenMaxEggsPerLaying = 3;
            
            eggDuration = 3f;
            larvaDuration = 6f;
            pupaDuration = 12f;
            broodNutritionDecayRate = 0.1f;
            broodRequiredNutritionForGrowth = 0.8f;
            
            startingHoney = 100f;
            startingPollen = 50f;
            startingBeeswax = 20f;
            startingRoyalJelly = 10f;
            
            honeyConsumptionPerBeePerCycle = 2f;
            royalJellyConsumptionPerQueenPerCycle = 5f;
            winterHoneyConsumptionMultiplier = 2f;
            
            difficultyMultiplier = 1f;
            resourceGenerationMultiplier = 1f;
            populationGrowthMultiplier = 1f;
            workEfficiencyMultiplier = 1f;
        }
        
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Mellifera/Create Game Configuration")]
        public static void CreateGameConfiguration()
        {
            GameConfiguration config = CreateInstance<GameConfiguration>();
            UnityEditor.AssetDatabase.CreateAsset(config, "Assets/Scripts/Data/GameConfiguration.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.Selection.activeObject = config;
        }
        #endif
    }
}