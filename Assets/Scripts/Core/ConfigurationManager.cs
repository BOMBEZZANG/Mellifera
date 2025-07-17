using UnityEngine;
using Mellifera.Data;

namespace Mellifera.Core
{
    public class ConfigurationManager : MonoBehaviour
    {
        public static ConfigurationManager Instance { get; private set; }
        
        [Header("Configuration Asset")]
        [SerializeField] private GameConfiguration gameConfiguration;
        
        [Header("Runtime Configuration")]
        [SerializeField] private bool allowRuntimeChanges = true;
        [SerializeField] private bool saveChangesToFile = false;
        
        private GameConfiguration runtimeConfig;
        
        public GameConfiguration Config => runtimeConfig ?? gameConfiguration;
        
        public System.Action OnConfigurationChanged;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeConfiguration();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeConfiguration()
        {
            if (gameConfiguration == null)
            {
                Debug.LogError("GameConfiguration asset is not assigned to ConfigurationManager!");
                CreateDefaultConfiguration();
            }
            
            // Create runtime copy
            runtimeConfig = CreateRuntimeCopy(gameConfiguration);
            
            // Load saved configuration if available
            LoadConfiguration();
        }
        
        private void CreateDefaultConfiguration()
        {
            gameConfiguration = ScriptableObject.CreateInstance<GameConfiguration>();
            gameConfiguration.ResetToDefaults();
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(gameConfiguration, "Assets/Scripts/Data/DefaultGameConfiguration.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
        
        private GameConfiguration CreateRuntimeCopy(GameConfiguration original)
        {
            if (original == null) return null;
            
            GameConfiguration copy = ScriptableObject.CreateInstance<GameConfiguration>();
            
            // Copy all values
            copy.cycleDuration = original.cycleDuration;
            copy.dayPhaseRatio = original.dayPhaseRatio;
            copy.springCycles = original.springCycles;
            copy.summerCycles = original.summerCycles;
            copy.autumnCycles = original.autumnCycles;
            copy.winterCycles = original.winterCycles;
            
            copy.maxBeePopulation = original.maxBeePopulation;
            copy.beeLifespan = original.beeLifespan;
            copy.beeSpeed = original.beeSpeed;
            copy.beeCarryCapacity = original.beeCarryCapacity;
            copy.beeWorkEfficiency = original.beeWorkEfficiency;
            copy.beeHealthRegenRate = original.beeHealthRegenRate;
            
            copy.queenLifespan = original.queenLifespan;
            copy.queenHungerDecayRate = original.queenHungerDecayRate;
            copy.queenEggLayInterval = original.queenEggLayInterval;
            copy.queenMaxEggsPerLaying = original.queenMaxEggsPerLaying;
            
            copy.eggDuration = original.eggDuration;
            copy.larvaDuration = original.larvaDuration;
            copy.pupaDuration = original.pupaDuration;
            copy.broodNutritionDecayRate = original.broodNutritionDecayRate;
            copy.broodRequiredNutritionForGrowth = original.broodRequiredNutritionForGrowth;
            
            copy.startingHoney = original.startingHoney;
            copy.startingPollen = original.startingPollen;
            copy.startingBeeswax = original.startingBeeswax;
            copy.startingRoyalJelly = original.startingRoyalJelly;
            
            copy.honeyConsumptionPerBeePerCycle = original.honeyConsumptionPerBeePerCycle;
            copy.royalJellyConsumptionPerQueenPerCycle = original.royalJellyConsumptionPerQueenPerCycle;
            copy.winterHoneyConsumptionMultiplier = original.winterHoneyConsumptionMultiplier;
            
            copy.honeyToRoyalJellyRatio = original.honeyToRoyalJellyRatio;
            copy.pollenToRoyalJellyRatio = original.pollenToRoyalJellyRatio;
            copy.royalJellyProductionAmount = original.royalJellyProductionAmount;
            
            copy.honeyToBeeswaxRatio = original.honeyToBeeswaxRatio;
            copy.pollenToBeeswaxRatio = original.pollenToBeeswaxRatio;
            copy.beeswaxProductionAmount = original.beeswaxProductionAmount;
            
            copy.basicCellCapacity = original.basicCellCapacity;
            copy.nurseryCellCapacity = original.nurseryCellCapacity;
            copy.honeyCellCapacity = original.honeyCellCapacity;
            copy.pollenCellCapacity = original.pollenCellCapacity;
            copy.nurseryTargetTemperature = original.nurseryTargetTemperature;
            copy.cellTemperatureDecayRate = original.cellTemperatureDecayRate;
            
            copy.maxConcurrentTasks = original.maxConcurrentTasks;
            copy.taskUpdateInterval = original.taskUpdateInterval;
            copy.taskGenerationInterval = original.taskGenerationInterval;
            copy.taskWorkDuration = original.taskWorkDuration;
            
            copy.foragingBeeSpeed = original.foragingBeeSpeed;
            copy.pathRecordingInterval = original.pathRecordingInterval;
            copy.maxConcurrentForagers = original.maxConcurrentForagers;
            copy.resourceNodeRegenRate = original.resourceNodeRegenRate;
            copy.resourceNodeHarvestRate = original.resourceNodeHarvestRate;
            
            copy.honeyNodeMaxCapacity = original.honeyNodeMaxCapacity;
            copy.pollenNodeMaxCapacity = original.pollenNodeMaxCapacity;
            copy.honeyNodeCount = original.honeyNodeCount;
            copy.pollenNodeCount = original.pollenNodeCount;
            
            copy.spiderDamage = original.spiderDamage;
            copy.spiderRadius = original.spiderRadius;
            copy.spiderActivationChance = original.spiderActivationChance;
            copy.spiderDuration = original.spiderDuration;
            
            copy.windDamage = original.windDamage;
            copy.windRadius = original.windRadius;
            copy.windActivationChance = original.windActivationChance;
            copy.windDuration = original.windDuration;
            
            copy.lowHoneyThreshold = original.lowHoneyThreshold;
            copy.lowPollenThreshold = original.lowPollenThreshold;
            copy.lowRoyalJellyThreshold = original.lowRoyalJellyThreshold;
            copy.notificationDuration = original.notificationDuration;
            copy.maxNotifications = original.maxNotifications;
            
            copy.masterVolume = original.masterVolume;
            copy.sfxVolume = original.sfxVolume;
            copy.musicVolume = original.musicVolume;
            copy.uiVolume = original.uiVolume;
            
            copy.difficultyMultiplier = original.difficultyMultiplier;
            copy.resourceGenerationMultiplier = original.resourceGenerationMultiplier;
            copy.populationGrowthMultiplier = original.populationGrowthMultiplier;
            copy.workEfficiencyMultiplier = original.workEfficiencyMultiplier;
            
            copy.enableDebugMode = original.enableDebugMode;
            copy.showDebugGizmos = original.showDebugGizmos;
            copy.enableConsoleLogging = original.enableConsoleLogging;
            copy.enablePerformanceMetrics = original.enablePerformanceMetrics;
            
            return copy;
        }
        
        private void LoadConfiguration()
        {
            if (!saveChangesToFile) return;
            
            string configPath = GetConfigurationPath();
            if (System.IO.File.Exists(configPath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(configPath);
                    JsonUtility.FromJsonOverwrite(json, runtimeConfig);
                    Debug.Log("Configuration loaded from file");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load configuration: {e.Message}");
                }
            }
        }
        
        private void SaveConfiguration()
        {
            if (!saveChangesToFile) return;
            
            string configPath = GetConfigurationPath();
            try
            {
                string json = JsonUtility.ToJson(runtimeConfig, true);
                System.IO.File.WriteAllText(configPath, json);
                Debug.Log("Configuration saved to file");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save configuration: {e.Message}");
            }
        }
        
        private string GetConfigurationPath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "mellifera_config.json");
        }
        
        public void SetDifficultyMultiplier(float multiplier)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.difficultyMultiplier = Mathf.Clamp(multiplier, 0.1f, 5f);
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetResourceGenerationMultiplier(float multiplier)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.resourceGenerationMultiplier = Mathf.Clamp(multiplier, 0.1f, 3f);
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetPopulationGrowthMultiplier(float multiplier)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.populationGrowthMultiplier = Mathf.Clamp(multiplier, 0.1f, 3f);
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetWorkEfficiencyMultiplier(float multiplier)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.workEfficiencyMultiplier = Mathf.Clamp(multiplier, 0.1f, 3f);
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetCycleDuration(float duration)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.cycleDuration = Mathf.Clamp(duration, 60f, 1200f); // 1-20 minutes
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetBeeLifespan(float lifespan)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.beeLifespan = Mathf.Clamp(lifespan, 5f, 100f);
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetMaxBeePopulation(int population)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.maxBeePopulation = Mathf.Clamp(population, 10, 500);
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetStartingResources(float honey, float pollen, float beeswax, float royalJelly)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.startingHoney = Mathf.Max(0f, honey);
            runtimeConfig.startingPollen = Mathf.Max(0f, pollen);
            runtimeConfig.startingBeeswax = Mathf.Max(0f, beeswax);
            runtimeConfig.startingRoyalJelly = Mathf.Max(0f, royalJelly);
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetVolumeSettings(float master, float sfx, float music, float ui)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.masterVolume = Mathf.Clamp01(master);
            runtimeConfig.sfxVolume = Mathf.Clamp01(sfx);
            runtimeConfig.musicVolume = Mathf.Clamp01(music);
            runtimeConfig.uiVolume = Mathf.Clamp01(ui);
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void SetDebugSettings(bool debugMode, bool showGizmos, bool enableLogging, bool enableMetrics)
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.enableDebugMode = debugMode;
            runtimeConfig.showDebugGizmos = showGizmos;
            runtimeConfig.enableConsoleLogging = enableLogging;
            runtimeConfig.enablePerformanceMetrics = enableMetrics;
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void ResetToDefaults()
        {
            if (!allowRuntimeChanges) return;
            
            runtimeConfig.ResetToDefaults();
            OnConfigurationChanged?.Invoke();
            SaveConfiguration();
        }
        
        public void ReloadFromAsset()
        {
            if (gameConfiguration != null)
            {
                runtimeConfig = CreateRuntimeCopy(gameConfiguration);
                OnConfigurationChanged?.Invoke();
            }
        }
        
        public void ApplyConfiguration()
        {
            OnConfigurationChanged?.Invoke();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveConfiguration();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveConfiguration();
            }
        }
        
        private void OnDestroy()
        {
            SaveConfiguration();
        }
    }
}