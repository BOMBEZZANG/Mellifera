using UnityEngine;
using Mellifera.Events;
using Mellifera.Data;

namespace Mellifera.Core
{
    public class TimeManager : MonoBehaviour
    {
        [Header("Time Configuration")]
        [SerializeField] private float cycleDuration = 300f; // 5 minutes in seconds
        [SerializeField] private float dayPhaseRatio = 0.7f; // 70% day, 30% night
        
        [Header("Season Configuration")]
        [SerializeField] private int springCycles = 10;
        [SerializeField] private int summerCycles = 15;
        [SerializeField] private int autumnCycles = 10;
        [SerializeField] private int winterCycles = 10;
        
        [Header("Current State")]
        [SerializeField] private int currentCycle = 1;
        [SerializeField] private Season currentSeason = Season.Spring;
        [SerializeField] private bool isDay = true;
        [SerializeField] private float currentTime = 0f;
        
        private float dayDuration;
        private float nightDuration;
        private int totalCyclesInYear;
        
        public int CurrentCycle => currentCycle;
        public Season CurrentSeason => currentSeason;
        public bool IsDay => isDay;
        public float CurrentTime => currentTime;
        public float CycleProgress => currentTime / cycleDuration;
        public float DayProgress => isDay ? (currentTime / dayDuration) : 1f;
        public float NightProgress => !isDay ? ((currentTime - dayDuration) / nightDuration) : 0f;
        
        private void Start()
        {
            dayDuration = cycleDuration * dayPhaseRatio;
            nightDuration = cycleDuration * (1f - dayPhaseRatio);
            totalCyclesInYear = springCycles + summerCycles + autumnCycles + winterCycles;
            
            GameEvents.TriggerNewDay(currentCycle);
            GameEvents.TriggerDaybreak();
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            
            UpdateTime();
        }
        
        private void UpdateTime()
        {
            currentTime += Time.deltaTime;
            GameEvents.TriggerTick(Time.deltaTime);
            
            CheckDayNightTransition();
            CheckCycleTransition();
        }
        
        private void CheckDayNightTransition()
        {
            if (isDay && currentTime >= dayDuration)
            {
                isDay = false;
                GameEvents.TriggerNightfall();
            }
        }
        
        private void CheckCycleTransition()
        {
            if (currentTime >= cycleDuration)
            {
                CompleteCycle();
            }
        }
        
        private void CompleteCycle()
        {
            GameEvents.TriggerCycleEnd(currentCycle);
            
            currentCycle++;
            currentTime = 0f;
            isDay = true;
            
            UpdateSeason();
            
            GameEvents.TriggerNewDay(currentCycle);
            GameEvents.TriggerDaybreak();
        }
        
        private void UpdateSeason()
        {
            Season newSeason = GetSeasonForCycle(currentCycle);
            if (newSeason != currentSeason)
            {
                currentSeason = newSeason;
                GameEvents.TriggerSeasonChanged(currentSeason);
            }
        }
        
        private Season GetSeasonForCycle(int cycle)
        {
            int yearCycle = ((cycle - 1) % totalCyclesInYear) + 1;
            
            if (yearCycle <= springCycles)
                return Season.Spring;
            else if (yearCycle <= springCycles + summerCycles)
                return Season.Summer;
            else if (yearCycle <= springCycles + summerCycles + autumnCycles)
                return Season.Autumn;
            else
                return Season.Winter;
        }
        
        public bool CanForage()
        {
            return isDay && currentSeason != Season.Winter;
        }
        
        public float GetHoneyConsumptionMultiplier()
        {
            return currentSeason == Season.Winter ? 2f : 1f;
        }
    }
}