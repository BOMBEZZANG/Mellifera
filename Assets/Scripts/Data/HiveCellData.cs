using UnityEngine;

namespace Mellifera.Data
{
    [System.Serializable]
    public enum HiveCellType
    {
        Basic,
        Nursery,
        HoneyStorage,
        PollenStorage
    }
    
    [System.Serializable]
    public class HiveCellData
    {
        public HiveCellType cellType;
        public float maxCapacity;
        public float currentAmount;
        public float temperature;
        public float targetTemperature;
        public bool isOccupied;
        public bool needsConstruction;
        public float constructionProgress;
        
        public HiveCellData(HiveCellType type)
        {
            cellType = type;
            maxCapacity = GetDefaultCapacity(type);
            currentAmount = 0f;
            temperature = 20f; // Default temperature
            targetTemperature = GetDefaultTargetTemperature(type);
            isOccupied = false;
            needsConstruction = true;
            constructionProgress = 0f;
        }
        
        private float GetDefaultCapacity(HiveCellType type)
        {
            switch (type)
            {
                case HiveCellType.Basic: return 1f;
                case HiveCellType.Nursery: return 1f;
                case HiveCellType.HoneyStorage: return 10f;
                case HiveCellType.PollenStorage: return 8f;
                default: return 1f;
            }
        }
        
        private float GetDefaultTargetTemperature(HiveCellType type)
        {
            switch (type)
            {
                case HiveCellType.Nursery: return 35f; // Brood needs warm temperature
                default: return 25f;
            }
        }
        
        public bool CanStore(float amount)
        {
            return currentAmount + amount <= maxCapacity;
        }
        
        public bool IsEmpty => currentAmount <= 0f && !isOccupied;
        public bool IsFull => currentAmount >= maxCapacity;
        public float FillPercentage => currentAmount / maxCapacity;
        public bool IsConstructed => constructionProgress >= 1f;
        public bool NeedsHeating => cellType == HiveCellType.Nursery && temperature < targetTemperature;
    }
}