using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Mellifera.Core;
using Mellifera.Data;
using Mellifera.Events;
using Mellifera.Units;

namespace Mellifera.UI
{
    [System.Serializable]
    public enum NotificationType
    {
        Info,
        Warning,
        Critical,
        Success
    }
    
    [System.Serializable]
    public class Notification
    {
        public string message;
        public NotificationType type;
        public float duration;
        public System.DateTime timestamp;
        public bool isRead;
        
        public Notification(string msg, NotificationType notType, float dur = 5f)
        {
            message = msg;
            type = notType;
            duration = dur;
            timestamp = System.DateTime.Now;
            isRead = false;
        }
    }
    
    public class NotificationSystem : MonoBehaviour
    {
        [Header("Notification UI")]
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private ScrollRect notificationScrollRect;
        [SerializeField] private TextMeshProUGUI notificationCountText;
        
        [Header("Notification Settings")]
        [SerializeField] private int maxNotifications = 50;
        [SerializeField] private float defaultDuration = 5f;
        [SerializeField] private bool autoHideNotifications = true;
        
        [Header("Notification Colors")]
        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private Color successColor = Color.green;
        
        [Header("Audio")]
        [SerializeField] private AudioClip infoSound;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip criticalSound;
        [SerializeField] private AudioClip successSound;
        
        private List<Notification> notifications = new List<Notification>();
        private Queue<GameObject> notificationPool = new Queue<GameObject>();
        private AudioSource audioSource;
        
        public System.Action<Notification> OnNotificationAdded;
        public System.Action<Notification> OnNotificationRemoved;
        
        private void Start()
        {
            InitializeSystem();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeSystem()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            
            // Initialize notification pool
            for (int i = 0; i < 10; i++)
            {
                CreateNotificationObject();
            }
        }
        
        private void CreateNotificationObject()
        {
            if (notificationPrefab != null)
            {
                GameObject notificationObj = Instantiate(notificationPrefab, notificationContainer);
                notificationObj.SetActive(false);
                notificationPool.Enqueue(notificationObj);
            }
        }
        
        private void SubscribeToEvents()
        {
            // Resource events
            ResourceManager.OnResourceDepleted += HandleResourceDepleted;
            ResourceManager.OnResourceChanged += HandleResourceChanged;
            
            // Bee events
            Bee.OnBeeDeath += HandleBeeDeath;
            QueenBee.OnQueenHungry += HandleQueenHungry;
            QueenBee.OnEggsLaid += HandleEggsLaid;
            
            // Brood events
            Brood.OnBroodNeedsFeeding += HandleBroodNeedsFeeding;
            Brood.OnBroodMatured += HandleBroodMatured;
            Brood.OnBroodDied += HandleBroodDied;
            
            // Time events
            GameEvents.OnSeasonChanged += HandleSeasonChanged;
            GameEvents.OnNewDay += HandleNewDay;
            
            // Game events
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }
        
        private void UnsubscribeFromEvents()
        {
            ResourceManager.OnResourceDepleted -= HandleResourceDepleted;
            ResourceManager.OnResourceChanged -= HandleResourceChanged;
            
            Bee.OnBeeDeath -= HandleBeeDeath;
            QueenBee.OnQueenHungry -= HandleQueenHungry;
            QueenBee.OnEggsLaid -= HandleEggsLaid;
            
            Brood.OnBroodNeedsFeeding -= HandleBroodNeedsFeeding;
            Brood.OnBroodMatured -= HandleBroodMatured;
            Brood.OnBroodDied -= HandleBroodDied;
            
            GameEvents.OnSeasonChanged -= HandleSeasonChanged;
            GameEvents.OnNewDay -= HandleNewDay;
            
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        private void HandleResourceDepleted(ResourceType resourceType)
        {
            string message = $"{resourceType} has been depleted!";
            AddNotification(message, NotificationType.Critical);
        }
        
        private void HandleResourceChanged(ResourceType resourceType, float newAmount)
        {
            // Only show notifications for very low resources
            if (newAmount < 10f && newAmount > 0f)
            {
                string message = $"Low {resourceType}: {newAmount:F1} remaining";
                AddNotification(message, NotificationType.Warning);
            }
        }
        
        private void HandleBeeDeath(Bee bee)
        {
            string message = $"{bee.BeeName} has died (Age: {bee.CurrentAge:F1}/{bee.Lifespan:F1})";
            AddNotification(message, NotificationType.Info);
        }
        
        private void HandleQueenHungry(QueenBee queen)
        {
            string message = "The Queen is hungry and needs Royal Jelly!";
            AddNotification(message, NotificationType.Critical);
        }
        
        private void HandleEggsLaid(QueenBee queen, int eggCount)
        {
            string message = $"The Queen laid {eggCount} eggs";
            AddNotification(message, NotificationType.Success);
        }
        
        private void HandleBroodNeedsFeeding(Brood brood)
        {
            string message = $"Larva needs feeding in {brood.CurrentStage} stage";
            AddNotification(message, NotificationType.Warning);
        }
        
        private void HandleBroodMatured(Brood brood)
        {
            string message = "A new worker bee has emerged!";
            AddNotification(message, NotificationType.Success);
        }
        
        private void HandleBroodDied(Brood brood)
        {
            string message = $"Brood died in {brood.CurrentStage} stage";
            AddNotification(message, NotificationType.Warning);
        }
        
        private void HandleSeasonChanged(Season newSeason)
        {
            string message = $"Season changed to {newSeason}";
            NotificationType type = newSeason == Season.Winter ? NotificationType.Critical : NotificationType.Info;
            AddNotification(message, type);
        }
        
        private void HandleNewDay(int dayNumber)
        {
            if (dayNumber % 10 == 0) // Every 10th day
            {
                string message = $"Day {dayNumber} - Colony Progress Update";
                AddNotification(message, NotificationType.Info);
            }
        }
        
        private void HandleGameStateChanged(GameState newState)
        {
            string message = $"Game State: {newState}";
            AddNotification(message, NotificationType.Info);
        }
        
        public void AddNotification(string message, NotificationType type, float duration = 0f)
        {
            if (duration <= 0f)
                duration = defaultDuration;
            
            Notification notification = new Notification(message, type, duration);
            notifications.Add(notification);
            
            // Remove oldest notifications if we exceed max
            while (notifications.Count > maxNotifications)
            {
                notifications.RemoveAt(0);
            }
            
            // Display the notification
            DisplayNotification(notification);
            
            // Play sound
            PlayNotificationSound(type);
            
            // Update counter
            UpdateNotificationCounter();
            
            OnNotificationAdded?.Invoke(notification);
        }
        
        private void DisplayNotification(Notification notification)
        {
            GameObject notificationObj = GetNotificationObject();
            if (notificationObj == null) return;
            
            // Configure notification UI
            ConfigureNotificationUI(notificationObj, notification);
            
            // Show notification
            notificationObj.SetActive(true);
            
            // Auto-hide after duration
            if (autoHideNotifications)
            {
                StartCoroutine(HideNotificationAfterDelay(notificationObj, notification.duration));
            }
        }
        
        private GameObject GetNotificationObject()
        {
            if (notificationPool.Count > 0)
            {
                return notificationPool.Dequeue();
            }
            
            // Create new one if pool is empty
            CreateNotificationObject();
            return notificationPool.Count > 0 ? notificationPool.Dequeue() : null;
        }
        
        private void ConfigureNotificationUI(GameObject notificationObj, Notification notification)
        {
            // Set text
            TextMeshProUGUI messageText = notificationObj.GetComponentInChildren<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = $"[{notification.timestamp:HH:mm}] {notification.message}";
            }
            
            // Set color based on type
            Image background = notificationObj.GetComponent<Image>();
            if (background != null)
            {
                background.color = GetColorForType(notification.type);
            }
            
            // Set close button
            Button closeButton = notificationObj.GetComponentInChildren<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => HideNotification(notificationObj));
            }
        }
        
        private Color GetColorForType(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Info: return infoColor;
                case NotificationType.Warning: return warningColor;
                case NotificationType.Critical: return criticalColor;
                case NotificationType.Success: return successColor;
                default: return infoColor;
            }
        }
        
        private void PlayNotificationSound(NotificationType type)
        {
            if (audioSource == null) return;
            
            AudioClip soundToPlay = null;
            switch (type)
            {
                case NotificationType.Info: soundToPlay = infoSound; break;
                case NotificationType.Warning: soundToPlay = warningSound; break;
                case NotificationType.Critical: soundToPlay = criticalSound; break;
                case NotificationType.Success: soundToPlay = successSound; break;
            }
            
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }
        
        private IEnumerator HideNotificationAfterDelay(GameObject notificationObj, float delay)
        {
            yield return new WaitForSeconds(delay);
            HideNotification(notificationObj);
        }
        
        private void HideNotification(GameObject notificationObj)
        {
            if (notificationObj != null)
            {
                notificationObj.SetActive(false);
                notificationPool.Enqueue(notificationObj);
            }
        }
        
        private void UpdateNotificationCounter()
        {
            if (notificationCountText != null)
            {
                int unreadCount = notifications.Count;
                notificationCountText.text = unreadCount > 0 ? unreadCount.ToString() : "";
            }
        }
        
        public void ClearAllNotifications()
        {
            notifications.Clear();
            
            // Hide all notification objects
            foreach (Transform child in notificationContainer)
            {
                if (child.gameObject.activeSelf)
                {
                    HideNotification(child.gameObject);
                }
            }
            
            UpdateNotificationCounter();
        }
        
        public void MarkAllAsRead()
        {
            foreach (var notification in notifications)
            {
                notification.isRead = true;
            }
            UpdateNotificationCounter();
        }
        
        public List<Notification> GetNotifications()
        {
            return new List<Notification>(notifications);
        }
        
        public List<Notification> GetNotificationsByType(NotificationType type)
        {
            return notifications.FindAll(n => n.type == type);
        }
    }
}