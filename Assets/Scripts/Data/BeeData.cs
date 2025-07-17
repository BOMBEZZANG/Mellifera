using UnityEngine;

namespace Mellifera.Data
{
    [System.Serializable]
    public enum BeeRole
    {
        Idle,
        ForageHoney,
        ForagePollen,
        ProduceRoyalJelly,
        ProduceBeeswax,
        NurseLarvae,
        Thermoregulate,
        BuildCells,
        CleanCells
    }
    
    [System.Serializable]
    public enum BeeState
    {
        Idling,
        Working,
        Foraging,
        Sleeping,
        Dying
    }
    
    [System.Serializable]
    public class BeeStats
    {
        public float flightSpeed = 5f;
        public float carryCapacity = 2f;
        public float workEfficiency = 1f;
        public float healthRegenRate = 0.1f;
        
        public BeeStats()
        {
            // Default constructor
        }
        
        public BeeStats(float speed, float capacity, float efficiency, float regenRate)
        {
            flightSpeed = speed;
            carryCapacity = capacity;
            workEfficiency = efficiency;
            healthRegenRate = regenRate;
        }
    }
    
    [System.Serializable]
    public enum BroodStage
    {
        Egg,
        Larva,
        Pupa
    }
    
    [System.Serializable]
    public class BroodData
    {
        public BroodStage stage;
        public float stageTimer;
        public float stagesDuration;
        public bool needsFeeding;
        public float nutritionLevel;
        
        public BroodData(BroodStage initialStage, float duration)
        {
            stage = initialStage;
            stageTimer = 0f;
            stagesDuration = duration;
            needsFeeding = false;
            nutritionLevel = 1f;
        }
    }
}