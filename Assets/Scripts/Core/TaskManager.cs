using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Mellifera.Data;
using Mellifera.Events;
using Mellifera.Systems;
using Mellifera.Units;

namespace Mellifera.Core
{
    public class TaskManager : MonoBehaviour
    {
        [Header("Task Configuration")]
        [SerializeField] private int maxConcurrentTasks = 20;
        [SerializeField] private float taskUpdateInterval = 1f;
        
        [Header("Task Generation")]
        [SerializeField] private bool autoGenerateTasks = true;
        [SerializeField] private float taskGenerationInterval = 5f;
        
        private List<BeeTask> allTasks = new List<BeeTask>();
        private List<BeeTask> availableTasks = new List<BeeTask>();
        private List<BeeTask> assignedTasks = new List<BeeTask>();
        private List<BeeTask> completedTasks = new List<BeeTask>();
        
        private float taskUpdateTimer = 0f;
        private float taskGenerationTimer = 0f;
        
        public int TotalTaskCount => allTasks.Count;
        public int AvailableTaskCount => availableTasks.Count;
        public int AssignedTaskCount => assignedTasks.Count;
        public int CompletedTaskCount => completedTasks.Count;
        
        public System.Action<BeeTask> OnTaskCreated;
        public System.Action<BeeTask> OnTaskAssigned;
        public System.Action<BeeTask> OnTaskCompleted;
        public System.Action<BeeTask> OnTaskFailed;
        
        private void Start()
        {
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            
            UpdateTasks();
            
            if (autoGenerateTasks)
            {
                GenerateTasksIfNeeded();
            }
        }
        
        private void SubscribeToEvents()
        {
            GameEvents.OnCycleEnd += HandleCycleEnd;
            HiveCell.OnCellBuilt += HandleCellBuilt;
            Brood.OnBroodNeedsFeeding += HandleBroodNeedsFeeding;
            QueenBee.OnQueenHungry += HandleQueenHungry;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnCycleEnd -= HandleCycleEnd;
            HiveCell.OnCellBuilt -= HandleCellBuilt;
            Brood.OnBroodNeedsFeeding -= HandleBroodNeedsFeeding;
            QueenBee.OnQueenHungry -= HandleQueenHungry;
        }
        
        private void UpdateTasks()
        {
            taskUpdateTimer += Time.deltaTime;
            if (taskUpdateTimer >= taskUpdateInterval)
            {
                taskUpdateTimer = 0f;
                UpdateTaskProgress();
                UpdateTaskCategories();
            }
        }
        
        private void UpdateTaskProgress()
        {
            foreach (BeeTask task in assignedTasks.ToList())
            {
                if (task.IsInProgress)
                {
                    task.UpdateProgress(taskUpdateInterval);
                    
                    if (task.IsCompleted)
                    {
                        CompleteTask(task);
                    }
                }
            }
        }
        
        private void UpdateTaskCategories()
        {
            availableTasks.Clear();
            assignedTasks.Clear();
            completedTasks.Clear();
            
            foreach (BeeTask task in allTasks)
            {
                switch (task.status)
                {
                    case TaskStatus.Available:
                        availableTasks.Add(task);
                        break;
                    case TaskStatus.Assigned:
                    case TaskStatus.InProgress:
                        assignedTasks.Add(task);
                        break;
                    case TaskStatus.Completed:
                        completedTasks.Add(task);
                        break;
                }
            }
        }
        
        private void GenerateTasksIfNeeded()
        {
            taskGenerationTimer += Time.deltaTime;
            if (taskGenerationTimer >= taskGenerationInterval)
            {
                taskGenerationTimer = 0f;
                GenerateAutomaticTasks();
            }
        }
        
        private void GenerateAutomaticTasks()
        {
            // Generate resource collection tasks
            GenerateResourceTasks();
            
            // Generate construction tasks
            GenerateConstructionTasks();
            
            // Generate maintenance tasks
            GenerateMaintenanceTasks();
        }
        
        private void GenerateResourceTasks()
        {
            ResourceManager resourceManager = GameManager.Instance.ResourceManager;
            if (resourceManager == null) return;
            
            // Check if we need more honey
            if (resourceManager.GetResourceAmount(ResourceType.Honey) < 50f)
            {
                CreateTask(TaskType.Forage, TaskPriority.High, "Collect honey", ResourceType.Honey, 10f);
            }
            
            // Check if we need more pollen
            if (resourceManager.GetResourceAmount(ResourceType.Pollen) < 30f)
            {
                CreateTask(TaskType.Forage, TaskPriority.High, "Collect pollen", ResourceType.Pollen, 10f);
            }
            
            // Check if we need royal jelly
            if (resourceManager.GetResourceAmount(ResourceType.RoyalJelly) < 10f)
            {
                CreateTask(TaskType.ProduceRoyalJelly, TaskPriority.Medium, "Produce royal jelly");
            }
            
            // Check if we need beeswax
            if (resourceManager.GetResourceAmount(ResourceType.Beeswax) < 15f)
            {
                CreateTask(TaskType.ProduceBeeswax, TaskPriority.Medium, "Produce beeswax");
            }
        }
        
        private void GenerateConstructionTasks()
        {
            HiveCell[] allCells = FindObjectsOfType<HiveCell>();
            
            foreach (HiveCell cell in allCells)
            {
                if (!cell.IsConstructed)
                {
                    CreateTask(TaskType.Build, TaskPriority.Medium, $"Build {cell.CellType} cell", cell.transform.position);
                }
            }
        }
        
        private void GenerateMaintenanceTasks()
        {
            HiveCell[] nurseryCells = FindObjectsOfType<HiveCell>()
                .Where(cell => cell.CellType == HiveCellType.Nursery && cell.NeedsHeating)
                .ToArray();
            
            foreach (HiveCell cell in nurseryCells)
            {
                CreateTask(TaskType.Thermoregulate, TaskPriority.High, "Heat nursery cell", cell.transform.position);
            }
        }
        
        private void HandleCycleEnd(int cycleNumber)
        {
            // Clean up completed tasks periodically
            if (cycleNumber % 5 == 0)
            {
                CleanupCompletedTasks();
            }
        }
        
        private void HandleCellBuilt(HiveCell cell)
        {
            // Remove build tasks for this cell
            RemoveTasksForTarget(cell.gameObject);
        }
        
        private void HandleBroodNeedsFeeding(Brood brood)
        {
            CreateTask(TaskType.Supply, TaskPriority.Critical, "Feed larva", brood.transform.position);
        }
        
        private void HandleQueenHungry(QueenBee queen)
        {
            CreateTask(TaskType.Supply, TaskPriority.Critical, "Feed queen royal jelly", queen.transform.position);
        }
        
        public BeeTask CreateTask(TaskType taskType, TaskPriority priority, string description)
        {
            if (allTasks.Count >= maxConcurrentTasks) return null;
            
            BeeTask newTask = new BeeTask(taskType, priority, description);
            return AddTask(newTask);
        }
        
        public BeeTask CreateTask(TaskType taskType, TaskPriority priority, string description, Vector3 position)
        {
            if (allTasks.Count >= maxConcurrentTasks) return null;
            
            BeeTask newTask = new BeeTask(taskType, priority, description, position);
            return AddTask(newTask);
        }
        
        public BeeTask CreateTask(TaskType taskType, TaskPriority priority, string description, ResourceType resourceType, float amount)
        {
            if (allTasks.Count >= maxConcurrentTasks) return null;
            
            BeeTask newTask = new BeeTask(taskType, priority, description, resourceType, amount);
            return AddTask(newTask);
        }
        
        private BeeTask AddTask(BeeTask task)
        {
            allTasks.Add(task);
            OnTaskCreated?.Invoke(task);
            return task;
        }
        
        public bool AssignTask(BeeTask task)
        {
            if (task != null && task.IsAvailable)
            {
                task.AssignTask();
                OnTaskAssigned?.Invoke(task);
                return true;
            }
            return false;
        }
        
        public void StartTask(BeeTask task)
        {
            if (task != null && task.IsAssigned)
            {
                task.StartTask();
            }
        }
        
        public void CompleteTask(BeeTask task)
        {
            if (task != null && (task.IsInProgress || task.IsAssigned))
            {
                task.CompleteTask();
                OnTaskCompleted?.Invoke(task);
            }
        }
        
        public void FailTask(BeeTask task)
        {
            if (task != null && !task.IsCompleted)
            {
                task.FailTask();
                OnTaskFailed?.Invoke(task);
            }
        }
        
        public List<BeeTask> GetAvailableTasks()
        {
            return availableTasks.OrderByDescending(t => t.GetPriorityValue()).ToList();
        }
        
        public List<BeeTask> GetTasksByType(TaskType taskType)
        {
            return allTasks.Where(t => t.taskType == taskType).ToList();
        }
        
        public List<BeeTask> GetTasksByPriority(TaskPriority priority)
        {
            return allTasks.Where(t => t.priority == priority).ToList();
        }
        
        public BeeTask GetHighestPriorityTask()
        {
            return availableTasks.OrderByDescending(t => t.GetPriorityValue()).FirstOrDefault();
        }
        
        private void RemoveTasksForTarget(GameObject target)
        {
            var tasksToRemove = allTasks.Where(t => t.targetObject == target).ToList();
            foreach (var task in tasksToRemove)
            {
                allTasks.Remove(task);
            }
        }
        
        private void CleanupCompletedTasks()
        {
            var oldCompletedTasks = completedTasks.Where(t => 
                (System.DateTime.Now - t.completedTime).TotalMinutes > 10).ToList();
            
            foreach (var task in oldCompletedTasks)
            {
                allTasks.Remove(task);
            }
        }
        
        public void ClearAllTasks()
        {
            allTasks.Clear();
            availableTasks.Clear();
            assignedTasks.Clear();
            completedTasks.Clear();
        }
        
        public void SetMaxConcurrentTasks(int maxTasks)
        {
            maxConcurrentTasks = maxTasks;
        }
    }
}