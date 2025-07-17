using UnityEngine;

namespace Mellifera.Data
{
    [System.Serializable]
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
    
    [System.Serializable]
    public enum ResourceType
    {
        Honey,
        Pollen,
        Beeswax,
        RoyalJelly
    }
    
    [System.Serializable]
    public class ResourceAmount
    {
        public ResourceType type;
        public float amount;
        
        public ResourceAmount(ResourceType resourceType, float resourceAmount)
        {
            type = resourceType;
            amount = resourceAmount;
        }
    }
    
    [System.Serializable]
    public class ResourceInventory
    {
        public float honey;
        public float pollen;
        public float beeswax;
        public float royalJelly;
        
        public float GetResource(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Honey: return honey;
                case ResourceType.Pollen: return pollen;
                case ResourceType.Beeswax: return beeswax;
                case ResourceType.RoyalJelly: return royalJelly;
                default: return 0f;
            }
        }
        
        public void SetResource(ResourceType type, float amount)
        {
            switch (type)
            {
                case ResourceType.Honey: honey = amount; break;
                case ResourceType.Pollen: pollen = amount; break;
                case ResourceType.Beeswax: beeswax = amount; break;
                case ResourceType.RoyalJelly: royalJelly = amount; break;
            }
        }
        
        public void AddResource(ResourceType type, float amount)
        {
            SetResource(type, GetResource(type) + amount);
        }
        
        public bool TryConsumeResource(ResourceType type, float amount)
        {
            float currentAmount = GetResource(type);
            if (currentAmount >= amount)
            {
                SetResource(type, currentAmount - amount);
                return true;
            }
            return false;
        }
    }
}