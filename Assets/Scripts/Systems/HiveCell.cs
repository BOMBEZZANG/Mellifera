using UnityEngine;
using System;
using Mellifera.Data;
using Mellifera.Events;
using Mellifera.Units;

namespace Mellifera.Systems
{
    public class HiveCell : MonoBehaviour
    {
        [Header("Cell Configuration")]
        [SerializeField] private HiveCellData cellData;
        [SerializeField] private GameObject eggPrefab;
        [SerializeField] private GameObject broodPrefab;
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer cellRenderer;
        [SerializeField] private Color[] cellTypeColors;
        [SerializeField] private GameObject constructionIndicator;
        [SerializeField] private GameObject temperatureIndicator;
        
        private Brood currentBrood;
        
        public HiveCellType CellType => cellData.cellType;
        public float CurrentAmount => cellData.currentAmount;
        public float MaxCapacity => cellData.maxCapacity;
        public float Temperature => cellData.temperature;
        public bool IsEmpty => cellData.IsEmpty;
        public bool IsFull => cellData.IsFull;
        public bool IsConstructed => cellData.IsConstructed;
        public bool NeedsHeating => cellData.NeedsHeating;
        public float FillPercentage => cellData.FillPercentage;
        public bool IsOccupied => cellData.isOccupied;
        
        public static event Action<HiveCell> OnCellBuilt;
        public static event Action<HiveCell, float> OnResourceStored;
        public static event Action<HiveCell, float> OnResourceRemoved;
        public static event Action<HiveCell> OnCellHeated;
        public static event Action<HiveCell> OnEggPlaced;
        
        private void Start()
        {
            InitializeCell();
            UpdateVisuals();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeCell()
        {
            if (cellData == null)
            {
                cellData = new HiveCellData(HiveCellType.Basic);
            }
            
            UpdateVisuals();
        }
        
        private void SubscribeToEvents()
        {
            GameEvents.OnTick += HandleTick;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnTick -= HandleTick;
            Brood.OnBroodMatured -= HandleBroodMatured;
            Brood.OnBroodDied -= HandleBroodDied;
        }
        
        private void HandleTick(float deltaTime)
        {
            UpdateTemperature(deltaTime);
            UpdateVisuals();
        }
        
        private void UpdateTemperature(float deltaTime)
        {
            if (cellData.cellType == HiveCellType.Nursery)
            {
                // Temperature naturally decays toward ambient temperature (20°C)
                float ambientTemperature = 20f;
                float temperatureDecayRate = 2f; // degrees per second
                
                if (cellData.temperature > ambientTemperature)
                {
                    cellData.temperature -= temperatureDecayRate * deltaTime;
                    cellData.temperature = Mathf.Max(ambientTemperature, cellData.temperature);
                }
            }
        }
        
        public bool Build(float constructionAmount)
        {
            if (cellData.IsConstructed) return false;
            
            cellData.constructionProgress += constructionAmount;
            cellData.constructionProgress = Mathf.Clamp01(cellData.constructionProgress);
            
            if (cellData.IsConstructed)
            {
                cellData.needsConstruction = false;
                OnCellBuilt?.Invoke(this);
            }
            
            UpdateVisuals();
            return true;
        }
        
        public bool StoreResource(float amount)
        {
            if (!cellData.IsConstructed || !CanStoreResource(amount)) return false;
            
            cellData.currentAmount += amount;
            OnResourceStored?.Invoke(this, amount);
            UpdateVisuals();
            return true;
        }
        
        public bool RemoveResource(float amount)
        {
            if (cellData.currentAmount < amount) return false;
            
            cellData.currentAmount -= amount;
            OnResourceRemoved?.Invoke(this, amount);
            UpdateVisuals();
            return true;
        }
        
        public bool CanStoreResource(float amount)
        {
            return cellData.IsConstructed && cellData.CanStore(amount) && !cellData.isOccupied;
        }
        
        public bool PlaceEgg()
        {
            if (cellData.cellType != HiveCellType.Nursery || !cellData.IsEmpty || !cellData.IsConstructed)
                return false;
            
            if (eggPrefab != null)
            {
                GameObject eggObject = Instantiate(eggPrefab, transform.position, transform.rotation, transform);
                currentBrood = eggObject.GetComponent<Brood>();
                
                if (currentBrood != null)
                {
                    cellData.isOccupied = true;
                    OnEggPlaced?.Invoke(this);
                    
                    // Subscribe to brood events
                    Brood.OnBroodMatured += HandleBroodMatured;
                    Brood.OnBroodDied += HandleBroodDied;
                    
                    return true;
                }
            }
            
            return false;
        }
        
        private void HandleBroodMatured(Brood brood)
        {
            if (brood == currentBrood)
            {
                cellData.isOccupied = false;
                currentBrood = null;
            }
        }
        
        private void HandleBroodDied(Brood brood)
        {
            if (brood == currentBrood)
            {
                cellData.isOccupied = false;
                currentBrood = null;
            }
        }
        
        public bool Heat(float heatAmount)
        {
            if (cellData.cellType != HiveCellType.Nursery) return false;
            
            cellData.temperature += heatAmount;
            cellData.temperature = Mathf.Min(cellData.targetTemperature, cellData.temperature);
            
            OnCellHeated?.Invoke(this);
            return true;
        }
        
        public bool FeedBrood(float honeyAmount, float pollenAmount)
        {
            if (currentBrood != null && currentBrood.NeedsFeeding)
            {
                return currentBrood.Feed(honeyAmount, pollenAmount);
            }
            return false;
        }
        
        public void SetCellType(HiveCellType newType)
        {
            if (cellData.IsEmpty && cellData.IsConstructed)
            {
                cellData.cellType = newType;
                cellData.maxCapacity = GetCapacityForType(newType);
                cellData.targetTemperature = GetTargetTemperatureForType(newType);
                UpdateVisuals();
            }
        }
        
        private float GetCapacityForType(HiveCellType type)
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
        
        private float GetTargetTemperatureForType(HiveCellType type)
        {
            switch (type)
            {
                case HiveCellType.Nursery: return 35f;
                default: return 25f;
            }
        }
        
        private void UpdateVisuals()
        {
            if (cellRenderer != null && cellTypeColors != null)
            {
                int colorIndex = (int)cellData.cellType;
                if (colorIndex < cellTypeColors.Length)
                {
                    Color cellColor = cellTypeColors[colorIndex];
                    
                    if (!cellData.IsConstructed)
                    {
                        cellColor.a = 0.3f + (cellData.constructionProgress * 0.7f);
                    }
                    
                    cellRenderer.color = cellColor;
                }
            }
            
            if (constructionIndicator != null)
            {
                constructionIndicator.SetActive(!cellData.IsConstructed);
            }
            
            if (temperatureIndicator != null)
            {
                temperatureIndicator.SetActive(cellData.NeedsHeating);
            }
        }
    }
}