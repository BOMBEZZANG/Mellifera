using UnityEngine;
using System;
using Mellifera.Data;

namespace Mellifera.Events
{
    public static class GameEvents
    {
        public static event Action<int> OnNewDay;
        public static event Action OnNightfall;
        public static event Action OnDaybreak;
        public static event Action<float> OnTick;
        public static event Action<int> OnCycleEnd;
        public static event Action<Season> OnSeasonChanged;
        
        public static void TriggerNewDay(int dayNumber)
        {
            OnNewDay?.Invoke(dayNumber);
        }
        
        public static void TriggerNightfall()
        {
            OnNightfall?.Invoke();
        }
        
        public static void TriggerDaybreak()
        {
            OnDaybreak?.Invoke();
        }
        
        public static void TriggerTick(float deltaTime)
        {
            OnTick?.Invoke(deltaTime);
        }
        
        public static void TriggerCycleEnd(int cycleNumber)
        {
            OnCycleEnd?.Invoke(cycleNumber);
        }
        
        public static void TriggerSeasonChanged(Season newSeason)
        {
            OnSeasonChanged?.Invoke(newSeason);
        }
    }
    
}