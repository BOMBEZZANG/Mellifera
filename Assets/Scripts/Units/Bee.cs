using UnityEngine;
using System;
using Mellifera.Data;
using Mellifera.Events;
using Mellifera.Core;

namespace Mellifera.Units
{
    public class Bee : MonoBehaviour
    {
        [Header("Bee Identity")]
        [SerializeField] private string beeName;
        [SerializeField] private BeeRole currentRole = BeeRole.Idle;
        [SerializeField] private BeeState currentState = BeeState.Idling;
        
        [Header("Life Stats")]
        [SerializeField] private float lifespan = 20f; // in cycles
        [SerializeField] private float currentAge = 0f;
        [SerializeField] private float health = 100f;
        [SerializeField] private float maxHealth = 100f;
        
        [Header("Bee Stats")]
        [SerializeField] private BeeStats stats = new BeeStats();
        
        [Header("Work State")]
        [SerializeField] private float workTimer = 0f;
        [SerializeField] private float workDuration = 5f;
        [SerializeField] private bool isWorking = false;
        
        public string BeeName => beeName;
        public BeeRole CurrentRole => currentRole;
        public BeeState CurrentState => currentState;
        public float Lifespan => lifespan;
        public float CurrentAge => currentAge;
        public float Health => health;
        public float MaxHealth => maxHealth;
        public BeeStats Stats => stats;
        public float AgeProgress => currentAge / lifespan;
        public bool IsAlive => health > 0 && currentAge < lifespan;
        
        public static event Action<Bee> OnBeeDeath;
        public static event Action<Bee, BeeRole> OnBeeRoleChanged;
        public static event Action<Bee, BeeState> OnBeeStateChanged;
        
        private void Start()
        {
            if (string.IsNullOrEmpty(beeName))
            {
                beeName = GenerateRandomName();
            }
            
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
            GameEvents.OnNightfall += HandleNightfall;
            GameEvents.OnDaybreak += HandleDaybreak;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnTick -= HandleTick;
            GameEvents.OnCycleEnd -= HandleCycleEnd;
            GameEvents.OnNightfall -= HandleNightfall;
            GameEvents.OnDaybreak -= HandleDaybreak;
        }
        
        private void HandleTick(float deltaTime)
        {
            if (!IsAlive) return;
            
            UpdateWork(deltaTime);
            RegenerateHealth(deltaTime);
        }
        
        private void HandleCycleEnd(int cycleNumber)
        {
            if (!IsAlive) return;
            
            Age();
        }
        
        private void HandleNightfall()
        {
            if (currentState != BeeState.Dying)
            {
                ChangeState(BeeState.Sleeping);
            }
        }
        
        private void HandleDaybreak()
        {
            if (currentState == BeeState.Sleeping)
            {
                ChangeState(BeeState.Idling);
            }
        }
        
        private void UpdateWork(float deltaTime)
        {
            if (isWorking)
            {
                workTimer += deltaTime;
                if (workTimer >= workDuration)
                {
                    CompleteWork();
                }
            }
        }
        
        private void RegenerateHealth(float deltaTime)
        {
            if (health < maxHealth)
            {
                health = Mathf.Min(maxHealth, health + stats.healthRegenRate * deltaTime);
            }
        }
        
        public void Age()
        {
            currentAge += 1f;
            
            if (currentAge >= lifespan)
            {
                Die();
            }
        }
        
        public void Die()
        {
            ChangeState(BeeState.Dying);
            OnBeeDeath?.Invoke(this);
        }
        
        public void AssignRole(BeeRole newRole)
        {
            if (currentRole != newRole)
            {
                currentRole = newRole;
                OnBeeRoleChanged?.Invoke(this, newRole);
                
                if (newRole != BeeRole.Idle)
                {
                    ChangeState(BeeState.Working);
                }
                else
                {
                    ChangeState(BeeState.Idling);
                }
            }
        }
        
        public void ChangeState(BeeState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnBeeStateChanged?.Invoke(this, newState);
            }
        }
        
        public void StartWork()
        {
            if (currentState != BeeState.Sleeping && currentState != BeeState.Dying)
            {
                isWorking = true;
                workTimer = 0f;
                ChangeState(BeeState.Working);
            }
        }
        
        public void StopWork()
        {
            isWorking = false;
            workTimer = 0f;
            ChangeState(BeeState.Idling);
        }
        
        private void CompleteWork()
        {
            isWorking = false;
            workTimer = 0f;
            
            PerformRoleBasedWork();
            
            ChangeState(BeeState.Idling);
        }
        
        private void PerformRoleBasedWork()
        {
            ResourceManager resourceManager = GameManager.Instance.ResourceManager;
            if (resourceManager == null) return;
            
            switch (currentRole)
            {
                case BeeRole.ProduceRoyalJelly:
                    if (resourceManager.ConvertResources(ResourceType.Honey, 2f, ResourceType.Pollen, 1f, ResourceType.RoyalJelly, 1f))
                    {
                        // Successfully produced royal jelly
                    }
                    break;
                    
                case BeeRole.ProduceBeeswax:
                    if (resourceManager.ConvertResources(ResourceType.Honey, 3f, ResourceType.Pollen, 1f, ResourceType.Beeswax, 1f))
                    {
                        // Successfully produced beeswax
                    }
                    break;
                    
                case BeeRole.NurseLarvae:
                    // Handle larva feeding logic
                    break;
                    
                case BeeRole.Thermoregulate:
                    // Handle temperature regulation
                    if (resourceManager.TryConsumeResource(ResourceType.Honey, 1f))
                    {
                        // Successfully thermoregulated
                    }
                    break;
            }
        }
        
        public void TakeDamage(float damage)
        {
            health = Mathf.Max(0f, health - damage);
            if (health <= 0)
            {
                Die();
            }
        }
        
        private string GenerateRandomName()
        {
            string[] prefixes = { "Buzz", "Honey", "Pollen", "Wing", "Amber", "Golden", "Sweet", "Busy" };
            string[] suffixes = { "bee", "wing", "buzz", "flight", "worker", "dancer", "gatherer", "keeper" };
            
            return prefixes[UnityEngine.Random.Range(0, prefixes.Length)] + 
                   suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
        }
    }
}