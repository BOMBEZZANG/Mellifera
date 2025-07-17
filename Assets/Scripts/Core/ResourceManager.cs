using UnityEngine;
using System;
using Mellifera.Data;
using Mellifera.Events;

namespace Mellifera.Core
{
    public class ResourceManager : MonoBehaviour
    {
        [Header("Starting Resources")]
        [SerializeField] private ResourceInventory startingResources = new ResourceInventory
        {
            honey = 100f,
            pollen = 50f,
            beeswax = 20f,
            royalJelly = 10f
        };
        
        [Header("Consumption Rates")]
        [SerializeField] private float honeyConsumptionPerBeePerCycle = 2f;
        [SerializeField] private float royalJellyConsumptionPerQueenPerCycle = 5f;
        
        private ResourceInventory currentResources;
        
        public static event Action<ResourceType, float> OnResourceChanged;
        public static event Action<ResourceType> OnResourceDepleted;
        public static event Action<ResourceType, float> OnResourceAdded;
        public static event Action<ResourceType, float> OnResourceConsumed;
        
        public ResourceInventory CurrentResources => currentResources;
        
        private void Awake()
        {
            InitializeResources();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeResources()
        {
            currentResources = new ResourceInventory
            {
                honey = startingResources.honey,
                pollen = startingResources.pollen,
                beeswax = startingResources.beeswax,
                royalJelly = startingResources.royalJelly
            };
        }
        
        private void SubscribeToEvents()
        {
            GameEvents.OnCycleEnd += HandleCycleEnd;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnCycleEnd -= HandleCycleEnd;
        }
        
        private void HandleCycleEnd(int cycleNumber)
        {
            ConsumeCyclicResources();
        }
        
        private void ConsumeCyclicResources()
        {
            if (GameManager.Instance.BeeManager != null)
            {
                int workerBeeCount = GameManager.Instance.BeeManager.GetWorkerBeeCount();
                int queenBeeCount = GameManager.Instance.BeeManager.GetQueenBeeCount();
                
                float honeyConsumption = workerBeeCount * honeyConsumptionPerBeePerCycle;
                float royalJellyConsumption = queenBeeCount * royalJellyConsumptionPerQueenPerCycle;
                
                TimeManager timeManager = GameManager.Instance.TimeManager;
                if (timeManager != null)
                {
                    honeyConsumption *= timeManager.GetHoneyConsumptionMultiplier();
                }
                
                TryConsumeResource(ResourceType.Honey, honeyConsumption);
                TryConsumeResource(ResourceType.RoyalJelly, royalJellyConsumption);
            }
        }
        
        public void AddResource(ResourceType type, float amount)
        {
            if (amount <= 0) return;
            
            float oldAmount = currentResources.GetResource(type);
            currentResources.AddResource(type, amount);
            
            OnResourceAdded?.Invoke(type, amount);
            OnResourceChanged?.Invoke(type, currentResources.GetResource(type));
        }
        
        public bool TryConsumeResource(ResourceType type, float amount)
        {
            if (amount <= 0) return true;
            
            float oldAmount = currentResources.GetResource(type);
            if (currentResources.TryConsumeResource(type, amount))
            {
                OnResourceConsumed?.Invoke(type, amount);
                OnResourceChanged?.Invoke(type, currentResources.GetResource(type));
                
                if (currentResources.GetResource(type) <= 0)
                {
                    OnResourceDepleted?.Invoke(type);
                }
                
                return true;
            }
            return false;
        }
        
        public float GetResourceAmount(ResourceType type)
        {
            return currentResources.GetResource(type);
        }
        
        public bool HasResource(ResourceType type, float amount)
        {
            return currentResources.GetResource(type) >= amount;
        }
        
        public bool ConvertResources(ResourceType inputType1, float inputAmount1, 
                                   ResourceType inputType2, float inputAmount2, 
                                   ResourceType outputType, float outputAmount)
        {
            if (HasResource(inputType1, inputAmount1) && HasResource(inputType2, inputAmount2))
            {
                if (TryConsumeResource(inputType1, inputAmount1) && 
                    TryConsumeResource(inputType2, inputAmount2))
                {
                    AddResource(outputType, outputAmount);
                    return true;
                }
            }
            return false;
        }
    }
}