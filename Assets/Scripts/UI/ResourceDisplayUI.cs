using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mellifera.Core;
using Mellifera.Data;
using Mellifera.Events;

namespace Mellifera.UI
{
    public class ResourceDisplayUI : MonoBehaviour
    {
        [Header("Resource Display")]
        [SerializeField] private TextMeshProUGUI honeyText;
        [SerializeField] private TextMeshProUGUI pollenText;
        [SerializeField] private TextMeshProUGUI beeswaxText;
        [SerializeField] private TextMeshProUGUI royalJellyText;
        
        [Header("Resource Icons")]
        [SerializeField] private Image honeyIcon;
        [SerializeField] private Image pollenIcon;
        [SerializeField] private Image beeswaxIcon;
        [SerializeField] private Image royalJellyIcon;
        
        [Header("Population Display")]
        [SerializeField] private TextMeshProUGUI totalPopulationText;
        [SerializeField] private TextMeshProUGUI workerBeesText;
        [SerializeField] private TextMeshProUGUI queenBeesText;
        
        [Header("Time Display")]
        [SerializeField] private TextMeshProUGUI currentCycleText;
        [SerializeField] private TextMeshProUGUI currentSeasonText;
        [SerializeField] private TextMeshProUGUI timeOfDayText;
        [SerializeField] private Image dayNightIndicator;
        [SerializeField] private Color dayColor = Color.yellow;
        [SerializeField] private Color nightColor = Color.blue;
        
        [Header("Progress Bars")]
        [SerializeField] private Slider cycleProgressBar;
        [SerializeField] private Slider dayProgressBar;
        
        [Header("Warning Indicators")]
        [SerializeField] private GameObject lowHoneyWarning;
        [SerializeField] private GameObject lowPollenWarning;
        [SerializeField] private GameObject lowRoyalJellyWarning;
        [SerializeField] private GameObject winterWarning;
        
        [Header("Thresholds")]
        [SerializeField] private float lowHoneyThreshold = 20f;
        [SerializeField] private float lowPollenThreshold = 15f;
        [SerializeField] private float lowRoyalJellyThreshold = 5f;
        
        private ResourceManager resourceManager;
        private BeeManager beeManager;
        private TimeManager timeManager;
        
        private void Start()
        {
            InitializeReferences();
            SubscribeToEvents();
            UpdateDisplay();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeReferences()
        {
            resourceManager = GameManager.Instance.ResourceManager;
            beeManager = GameManager.Instance.BeeManager;
            timeManager = GameManager.Instance.TimeManager;
        }
        
        private void SubscribeToEvents()
        {
            ResourceManager.OnResourceChanged += HandleResourceChanged;
            GameEvents.OnNewDay += HandleNewDay;
            GameEvents.OnSeasonChanged += HandleSeasonChanged;
            GameEvents.OnTick += HandleTick;
            
            if (beeManager != null)
            {
                beeManager.OnPopulationChanged += HandlePopulationChanged;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            ResourceManager.OnResourceChanged -= HandleResourceChanged;
            GameEvents.OnNewDay -= HandleNewDay;
            GameEvents.OnSeasonChanged -= HandleSeasonChanged;
            GameEvents.OnTick -= HandleTick;
            
            if (beeManager != null)
            {
                beeManager.OnPopulationChanged -= HandlePopulationChanged;
            }
        }
        
        private void HandleResourceChanged(ResourceType resourceType, float newAmount)
        {
            UpdateResourceDisplay(resourceType, newAmount);
            UpdateWarnings();
        }
        
        private void HandleNewDay(int dayNumber)
        {
            UpdateTimeDisplay();
        }
        
        private void HandleSeasonChanged(Season newSeason)
        {
            UpdateTimeDisplay();
            UpdateWarnings();
        }
        
        private void HandleTick(float deltaTime)
        {
            UpdateTimeDisplay();
        }
        
        private void HandlePopulationChanged(int newPopulation)
        {
            UpdatePopulationDisplay();
        }
        
        private void UpdateDisplay()
        {
            UpdateResourceDisplays();
            UpdatePopulationDisplay();
            UpdateTimeDisplay();
            UpdateWarnings();
        }
        
        private void UpdateResourceDisplays()
        {
            if (resourceManager == null) return;
            
            UpdateResourceDisplay(ResourceType.Honey, resourceManager.GetResourceAmount(ResourceType.Honey));
            UpdateResourceDisplay(ResourceType.Pollen, resourceManager.GetResourceAmount(ResourceType.Pollen));
            UpdateResourceDisplay(ResourceType.Beeswax, resourceManager.GetResourceAmount(ResourceType.Beeswax));
            UpdateResourceDisplay(ResourceType.RoyalJelly, resourceManager.GetResourceAmount(ResourceType.RoyalJelly));
        }
        
        private void UpdateResourceDisplay(ResourceType resourceType, float amount)
        {
            string displayText = $"{amount:F1}";
            
            switch (resourceType)
            {
                case ResourceType.Honey:
                    if (honeyText != null) honeyText.text = displayText;
                    break;
                case ResourceType.Pollen:
                    if (pollenText != null) pollenText.text = displayText;
                    break;
                case ResourceType.Beeswax:
                    if (beeswaxText != null) beeswaxText.text = displayText;
                    break;
                case ResourceType.RoyalJelly:
                    if (royalJellyText != null) royalJellyText.text = displayText;
                    break;
            }
        }
        
        private void UpdatePopulationDisplay()
        {
            if (beeManager == null) return;
            
            if (totalPopulationText != null)
                totalPopulationText.text = beeManager.TotalBeeCount.ToString();
            
            if (workerBeesText != null)
                workerBeesText.text = beeManager.WorkerBeeCount.ToString();
            
            if (queenBeesText != null)
                queenBeesText.text = beeManager.QueenBeeCount.ToString();
        }
        
        private void UpdateTimeDisplay()
        {
            if (timeManager == null) return;
            
            if (currentCycleText != null)
                currentCycleText.text = $"Day {timeManager.CurrentCycle}";
            
            if (currentSeasonText != null)
                currentSeasonText.text = timeManager.CurrentSeason.ToString();
            
            if (timeOfDayText != null)
                timeOfDayText.text = timeManager.IsDay ? "Day" : "Night";
            
            if (dayNightIndicator != null)
                dayNightIndicator.color = timeManager.IsDay ? dayColor : nightColor;
            
            if (cycleProgressBar != null)
                cycleProgressBar.value = timeManager.CycleProgress;
            
            if (dayProgressBar != null)
                dayProgressBar.value = timeManager.IsDay ? timeManager.DayProgress : timeManager.NightProgress;
        }
        
        private void UpdateWarnings()
        {
            if (resourceManager == null) return;
            
            // Resource warnings
            if (lowHoneyWarning != null)
                lowHoneyWarning.SetActive(resourceManager.GetResourceAmount(ResourceType.Honey) < lowHoneyThreshold);
            
            if (lowPollenWarning != null)
                lowPollenWarning.SetActive(resourceManager.GetResourceAmount(ResourceType.Pollen) < lowPollenThreshold);
            
            if (lowRoyalJellyWarning != null)
                lowRoyalJellyWarning.SetActive(resourceManager.GetResourceAmount(ResourceType.RoyalJelly) < lowRoyalJellyThreshold);
            
            // Winter warning
            if (winterWarning != null && timeManager != null)
                winterWarning.SetActive(timeManager.CurrentSeason == Season.Winter);
        }
        
        private void Update()
        {
            // Update display every frame for smooth progress bars
            if (timeManager != null)
            {
                if (cycleProgressBar != null)
                    cycleProgressBar.value = timeManager.CycleProgress;
                
                if (dayProgressBar != null)
                    dayProgressBar.value = timeManager.IsDay ? timeManager.DayProgress : timeManager.NightProgress;
            }
        }
    }
}