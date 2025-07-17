using UnityEngine;
using System;
using Mellifera.Data;
using Mellifera.Core;
using Mellifera.Systems;

namespace Mellifera.Units
{
    public class QueenBee : Bee
    {
        [Header("Queen Specific")]
        [SerializeField] private float hungerGauge = 100f;
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float hungerDecayRate = 5f; // per cycle
        
        [Header("Egg Laying")]
        [SerializeField] private float eggLayTimer = 0f;
        [SerializeField] private float eggLayInterval = 30f; // seconds
        [SerializeField] private float eggLayEfficiency = 1f;
        [SerializeField] private int maxEggsPerLaying = 3;
        
        public float HungerGauge => hungerGauge;
        public float MaxHunger => maxHunger;
        public float HungerProgress => hungerGauge / maxHunger;
        public bool IsHungry => hungerGauge < maxHunger * 0.3f;
        public bool CanLayEggs => hungerGauge > maxHunger * 0.5f;
        
        public static event Action<QueenBee> OnQueenHungry;
        public static event Action<QueenBee, int> OnEggsLaid;
        
        private void Start()
        {
            AssignRole(BeeRole.Idle); // Queens don't work like regular bees
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            
            UpdateHunger();
            UpdateEggLaying();
        }
        
        private void UpdateHunger()
        {
            if (hungerGauge > 0)
            {
                hungerGauge -= hungerDecayRate * Time.deltaTime / 300f; // Decay over 5 minute cycle
                hungerGauge = Mathf.Max(0f, hungerGauge);
                
                if (IsHungry)
                {
                    OnQueenHungry?.Invoke(this);
                }
            }
        }
        
        private void UpdateEggLaying()
        {
            if (CanLayEggs)
            {
                eggLayTimer += Time.deltaTime;
                if (eggLayTimer >= eggLayInterval)
                {
                    LayEggs();
                    eggLayTimer = 0f;
                }
            }
        }
        
        public bool FeedRoyalJelly(float amount)
        {
            ResourceManager resourceManager = GameManager.Instance.ResourceManager;
            if (resourceManager != null && resourceManager.TryConsumeResource(ResourceType.RoyalJelly, amount))
            {
                hungerGauge = Mathf.Min(maxHunger, hungerGauge + amount * 20f);
                return true;
            }
            return false;
        }
        
        private void LayEggs()
        {
            HiveCell[] availableCells = FindAvailableNurseryCells();
            if (availableCells.Length == 0) return;
            
            int eggsToLay = Mathf.Min(maxEggsPerLaying, availableCells.Length);
            eggsToLay = Mathf.RoundToInt(eggsToLay * eggLayEfficiency);
            
            int eggsLaid = 0;
            for (int i = 0; i < eggsToLay && i < availableCells.Length; i++)
            {
                if (availableCells[i].PlaceEgg())
                {
                    eggsLaid++;
                }
            }
            
            if (eggsLaid > 0)
            {
                OnEggsLaid?.Invoke(this, eggsLaid);
            }
        }
        
        private HiveCell[] FindAvailableNurseryCells()
        {
            HiveCell[] allCells = FindObjectsOfType<HiveCell>();
            System.Collections.Generic.List<HiveCell> availableCells = new System.Collections.Generic.List<HiveCell>();
            
            foreach (HiveCell cell in allCells)
            {
                if (cell.CellType == HiveCellType.Nursery && cell.IsEmpty)
                {
                    availableCells.Add(cell);
                }
            }
            
            return availableCells.ToArray();
        }
        
        public void SetEggLayEfficiency(float efficiency)
        {
            eggLayEfficiency = Mathf.Clamp01(efficiency);
        }
        
        public void ReduceHungerDecay(float reduction)
        {
            hungerDecayRate = Mathf.Max(0f, hungerDecayRate - reduction);
        }
    }
}