using UnityEngine;
using System.Collections.Generic;

namespace Mellifera.Data
{
    [System.Serializable]
    public enum ForagingMode
    {
        Pioneer,
        Automate
    }
    
    [System.Serializable]
    public enum HazardType
    {
        Spider,
        Wind,
        Rain,
        Predator
    }
    
    [System.Serializable]
    public class ResourceNode
    {
        public string nodeId;
        public ResourceType resourceType;
        public Vector3 position;
        public float currentAmount;
        public float maxAmount;
        public float regenerationRate;
        public float harvestRate;
        public bool isDiscovered;
        public bool isAccessible;
        public List<Vector3> savedPath;
        public float lastHarvestTime;
        public int timesHarvested;
        
        public ResourceNode(ResourceType type, Vector3 pos, float maxCapacity)
        {
            nodeId = System.Guid.NewGuid().ToString();
            resourceType = type;
            position = pos;
            currentAmount = maxCapacity;
            maxAmount = maxCapacity;
            regenerationRate = 1f; // per second
            harvestRate = 2f; // per second
            isDiscovered = false;
            isAccessible = true;
            savedPath = new List<Vector3>();
            lastHarvestTime = 0f;
            timesHarvested = 0;
        }
        
        public bool CanHarvest => currentAmount > 0 && isAccessible;
        public float FillPercentage => currentAmount / maxAmount;
        public bool IsExhausted => currentAmount <= 0;
        public bool HasSavedPath => savedPath.Count > 0;
        
        public float Harvest(float amount)
        {
            float harvestedAmount = Mathf.Min(amount, currentAmount);
            currentAmount -= harvestedAmount;
            lastHarvestTime = Time.time;
            timesHarvested++;
            return harvestedAmount;
        }
        
        public void Regenerate(float deltaTime)
        {
            if (currentAmount < maxAmount)
            {
                currentAmount = Mathf.Min(maxAmount, currentAmount + regenerationRate * deltaTime);
            }
        }
        
        public void SetPath(List<Vector3> path)
        {
            savedPath = new List<Vector3>(path);
            isDiscovered = true;
        }
        
        public void ClearPath()
        {
            savedPath.Clear();
        }
    }
    
    [System.Serializable]
    public class Hazard
    {
        public string hazardId;
        public HazardType hazardType;
        public Vector3 position;
        public float radius;
        public float damage;
        public float activationChance;
        public bool isActive;
        public float duration;
        public float timer;
        
        public Hazard(HazardType type, Vector3 pos, float hazardRadius, float hazardDamage)
        {
            hazardId = System.Guid.NewGuid().ToString();
            hazardType = type;
            position = pos;
            radius = hazardRadius;
            damage = hazardDamage;
            activationChance = 0.1f;
            isActive = false;
            duration = 5f;
            timer = 0f;
        }
        
        public bool IsInRange(Vector3 targetPosition)
        {
            return Vector3.Distance(position, targetPosition) <= radius;
        }
        
        public void Activate()
        {
            isActive = true;
            timer = 0f;
        }
        
        public void Deactivate()
        {
            isActive = false;
            timer = 0f;
        }
        
        public void UpdateHazard(float deltaTime)
        {
            if (isActive)
            {
                timer += deltaTime;
                if (timer >= duration)
                {
                    Deactivate();
                }
            }
            else
            {
                if (Random.value < activationChance * deltaTime)
                {
                    Activate();
                }
            }
        }
    }
    
    [System.Serializable]
    public class ForagingRoute
    {
        public string routeId;
        public ResourceNode targetNode;
        public List<Vector3> pathPoints;
        public float totalDistance;
        public float estimatedTime;
        public int useCount;
        public float successRate;
        public float lastUsedTime;
        
        public ForagingRoute(ResourceNode node, List<Vector3> path)
        {
            routeId = System.Guid.NewGuid().ToString();
            targetNode = node;
            pathPoints = new List<Vector3>(path);
            totalDistance = CalculateDistance();
            estimatedTime = CalculateEstimatedTime();
            useCount = 0;
            successRate = 1f;
            lastUsedTime = 0f;
        }
        
        private float CalculateDistance()
        {
            float distance = 0f;
            for (int i = 1; i < pathPoints.Count; i++)
            {
                distance += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
            }
            return distance;
        }
        
        private float CalculateEstimatedTime()
        {
            float averageSpeed = 5f; // Default bee speed
            return totalDistance / averageSpeed;
        }
        
        public void UpdateSuccessRate(bool wasSuccessful)
        {
            float weight = 0.1f;
            successRate = successRate * (1f - weight) + (wasSuccessful ? 1f : 0f) * weight;
        }
        
        public void IncrementUseCount()
        {
            useCount++;
            lastUsedTime = Time.time;
        }
        
        public bool IsRecentlyUsed(float threshold = 10f)
        {
            return Time.time - lastUsedTime < threshold;
        }
    }
}