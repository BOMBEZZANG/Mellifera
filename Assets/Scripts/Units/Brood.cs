using UnityEngine;
using System;
using Mellifera.Data;
using Mellifera.Events;
using Mellifera.Core;

namespace Mellifera.Units
{
    public class Brood : MonoBehaviour
    {
        [Header("Brood Configuration")]
        [SerializeField] private BroodStage currentStage = BroodStage.Egg;
        [SerializeField] private float stageTimer = 0f;
        
        [Header("Stage Durations (in cycles)")]
        [SerializeField] private float eggDuration = 3f;
        [SerializeField] private float larvaDuration = 6f;
        [SerializeField] private float pupaDuration = 12f;
        
        [Header("Nutrition")]
        [SerializeField] private float nutritionLevel = 1f;
        [SerializeField] private float nutritionDecayRate = 0.1f;
        [SerializeField] private float requiredNutritionForGrowth = 0.8f;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject workerBeePrefab;
        
        private bool needsFeeding = false;
        private float currentStageDuration;
        
        public BroodStage CurrentStage => currentStage;
        public float StageTimer => stageTimer;
        public float StageProgress => stageTimer / currentStageDuration;
        public float NutritionLevel => nutritionLevel;
        public bool NeedsFeeding => needsFeeding;
        
        public static event Action<Brood, BroodStage> OnStageChanged;
        public static event Action<Brood> OnBroodNeedsFeeding;
        public static event Action<Brood> OnBroodMatured;
        public static event Action<Brood> OnBroodDied;
        
        private void Start()
        {
            currentStageDuration = GetStageDuration(currentStage);
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            GameEvents.OnTick += HandleTick;
            GameEvents.OnCycleEnd += HandleCycleEnd;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnTick -= HandleTick;
            GameEvents.OnCycleEnd -= HandleCycleEnd;
        }
        
        private void HandleTick(float deltaTime)
        {
            UpdateNutrition(deltaTime);
            UpdateGrowth(deltaTime);
        }
        
        private void HandleCycleEnd(int cycleNumber)
        {
            if (currentStage == BroodStage.Larva)
            {
                needsFeeding = true;
                OnBroodNeedsFeeding?.Invoke(this);
            }
        }
        
        private void UpdateNutrition(float deltaTime)
        {
            if (currentStage == BroodStage.Larva)
            {
                nutritionLevel -= nutritionDecayRate * deltaTime;
                nutritionLevel = Mathf.Max(0f, nutritionLevel);
                
                if (nutritionLevel <= 0f)
                {
                    Die();
                }
            }
        }
        
        private void UpdateGrowth(float deltaTime)
        {
            if (CanGrow())
            {
                stageTimer += deltaTime;
                
                if (stageTimer >= currentStageDuration)
                {
                    AdvanceStage();
                }
            }
        }
        
        private bool CanGrow()
        {
            if (currentStage == BroodStage.Larva)
            {
                return nutritionLevel >= requiredNutritionForGrowth;
            }
            return true;
        }
        
        private void AdvanceStage()
        {
            stageTimer = 0f;
            
            switch (currentStage)
            {
                case BroodStage.Egg:
                    currentStage = BroodStage.Larva;
                    needsFeeding = true;
                    break;
                    
                case BroodStage.Larva:
                    currentStage = BroodStage.Pupa;
                    needsFeeding = false;
                    break;
                    
                case BroodStage.Pupa:
                    Mature();
                    return;
            }
            
            currentStageDuration = GetStageDuration(currentStage);
            OnStageChanged?.Invoke(this, currentStage);
        }
        
        private void Mature()
        {
            OnBroodMatured?.Invoke(this);
            SpawnWorkerBee();
            Destroy(gameObject);
        }
        
        private void SpawnWorkerBee()
        {
            if (workerBeePrefab != null)
            {
                GameObject newBee = Instantiate(workerBeePrefab, transform.position, transform.rotation);
                Bee beeComponent = newBee.GetComponent<Bee>();
                
                if (beeComponent != null)
                {
                    // Register the new bee with the BeeManager
                    BeeManager beeManager = GameManager.Instance.BeeManager;
                    if (beeManager != null)
                    {
                        beeManager.RegisterBee(beeComponent);
                    }
                }
            }
        }
        
        public bool Feed(float honeyAmount, float pollenAmount)
        {
            if (currentStage != BroodStage.Larva) return false;
            
            ResourceManager resourceManager = GameManager.Instance.ResourceManager;
            if (resourceManager != null)
            {
                if (resourceManager.TryConsumeResource(ResourceType.Honey, honeyAmount) &&
                    resourceManager.TryConsumeResource(ResourceType.Pollen, pollenAmount))
                {
                    nutritionLevel = Mathf.Min(1f, nutritionLevel + 0.3f);
                    needsFeeding = false;
                    return true;
                }
            }
            return false;
        }
        
        private void Die()
        {
            OnBroodDied?.Invoke(this);
            Destroy(gameObject);
        }
        
        private float GetStageDuration(BroodStage stage)
        {
            switch (stage)
            {
                case BroodStage.Egg: return eggDuration * 300f; // Convert cycles to seconds
                case BroodStage.Larva: return larvaDuration * 300f;
                case BroodStage.Pupa: return pupaDuration * 300f;
                default: return 300f;
            }
        }
    }
}