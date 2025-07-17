using UnityEngine;
using System;

namespace Mellifera.Data
{
    [System.Serializable]
    public enum TaskType
    {
        Build,
        Supply,
        Clean,
        Forage,
        Thermoregulate,
        ProduceRoyalJelly,
        ProduceBeeswax
    }
    
    [System.Serializable]
    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    
    [System.Serializable]
    public enum TaskStatus
    {
        Available,
        Assigned,
        InProgress,
        Completed,
        Failed
    }
    
    [System.Serializable]
    public class BeeTask
    {
        public string taskId;
        public TaskType taskType;
        public TaskPriority priority;
        public TaskStatus status;
        public string description;
        public Vector3 targetPosition;
        public ResourceType resourceType;
        public float requiredAmount;
        public float duration;
        public float progress;
        public GameObject targetObject;
        public DateTime creationTime;
        public DateTime assignedTime;
        public DateTime completedTime;
        
        public BeeTask(TaskType type, TaskPriority taskPriority, string taskDescription)
        {
            taskId = System.Guid.NewGuid().ToString();
            taskType = type;
            priority = taskPriority;
            status = TaskStatus.Available;
            description = taskDescription;
            targetPosition = Vector3.zero;
            resourceType = ResourceType.Honey;
            requiredAmount = 0f;
            duration = 5f;
            progress = 0f;
            targetObject = null;
            creationTime = DateTime.Now;
        }
        
        public BeeTask(TaskType type, TaskPriority taskPriority, string taskDescription, Vector3 position)
            : this(type, taskPriority, taskDescription)
        {
            targetPosition = position;
        }
        
        public BeeTask(TaskType type, TaskPriority taskPriority, string taskDescription, 
                   ResourceType resource, float amount)
            : this(type, taskPriority, taskDescription)
        {
            resourceType = resource;
            requiredAmount = amount;
        }
        
        public bool IsCompleted => status == TaskStatus.Completed;
        public bool IsAvailable => status == TaskStatus.Available;
        public bool IsAssigned => status == TaskStatus.Assigned;
        public bool IsInProgress => status == TaskStatus.InProgress;
        public bool IsFailed => status == TaskStatus.Failed;
        public float ProgressPercentage => duration > 0 ? (progress / duration) : 0f;
        
        public void UpdateProgress(float deltaTime)
        {
            if (status == TaskStatus.InProgress)
            {
                progress += deltaTime;
                if (progress >= duration)
                {
                    CompleteTask();
                }
            }
        }
        
        public void AssignTask()
        {
            status = TaskStatus.Assigned;
            assignedTime = DateTime.Now;
        }
        
        public void StartTask()
        {
            status = TaskStatus.InProgress;
        }
        
        public void CompleteTask()
        {
            status = TaskStatus.Completed;
            completedTime = DateTime.Now;
        }
        
        public void FailTask()
        {
            status = TaskStatus.Failed;
        }
        
        public void ResetTask()
        {
            status = TaskStatus.Available;
            progress = 0f;
        }
        
        public int GetPriorityValue()
        {
            return (int)priority;
        }
        
        public string GetStatusString()
        {
            return status.ToString();
        }
        
        public string GetTaskInfo()
        {
            return $"{taskType}: {description} ({status}) - {ProgressPercentage:P0}";
        }
    }
}