using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Mellifera.Units;
using Mellifera.Data;
using Mellifera.Events;

namespace Mellifera.Core
{
    public class BeeManager : MonoBehaviour
    {
        [Header("Bee Configuration")]
        [SerializeField] private int maxBeePopulation = 100;
        [SerializeField] private GameObject workerBeePrefab;
        [SerializeField] private GameObject queenBeePrefab;
        
        [Header("Starting Population")]
        [SerializeField] private int startingWorkerBees = 10;
        [SerializeField] private int startingQueenBees = 1;
        
        [Header("Spawn Points")]
        [SerializeField] private Transform beeSpawnPoint;
        
        private List<Bee> allBees = new List<Bee>();
        private List<QueenBee> queenBees = new List<QueenBee>();
        private List<Bee> workerBees = new List<Bee>();
        private List<Bee> idleBees = new List<Bee>();
        private List<Bee> workingBees = new List<Bee>();
        
        public int TotalBeeCount => allBees.Count;
        public int WorkerBeeCount => workerBees.Count;
        public int QueenBeeCount => queenBees.Count;
        public int IdleBeeCount => idleBees.Count;
        public int WorkingBeeCount => workingBees.Count;
        public List<Bee> AllBees => new List<Bee>(allBees);
        public List<Bee> IdleBees => new List<Bee>(idleBees);
        
        public System.Action<Bee> OnBeeSpawned;
        public System.Action<Bee> OnBeeRemoved;
        public System.Action<int> OnPopulationChanged;
        
        private void Start()
        {
            InitializeStartingPopulation();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            Bee.OnBeeDeath += HandleBeeDeath;
            Bee.OnBeeRoleChanged += HandleBeeRoleChanged;
            Bee.OnBeeStateChanged += HandleBeeStateChanged;
            GameEvents.OnTick += HandleTick;
        }
        
        private void UnsubscribeFromEvents()
        {
            Bee.OnBeeDeath -= HandleBeeDeath;
            Bee.OnBeeRoleChanged -= HandleBeeRoleChanged;
            Bee.OnBeeStateChanged -= HandleBeeStateChanged;
            GameEvents.OnTick -= HandleTick;
        }
        
        private void InitializeStartingPopulation()
        {
            // Spawn queen bees
            for (int i = 0; i < startingQueenBees; i++)
            {
                SpawnQueenBee();
            }
            
            // Spawn worker bees
            for (int i = 0; i < startingWorkerBees; i++)
            {
                SpawnWorkerBee();
            }
        }
        
        private void HandleTick(float deltaTime)
        {
            // Update bee assignments and tasks
            UpdateBeeAssignments();
        }
        
        private void HandleBeeDeath(Bee bee)
        {
            RemoveBee(bee);
        }
        
        private void HandleBeeRoleChanged(Bee bee, BeeRole newRole)
        {
            UpdateBeeCategories();
        }
        
        private void HandleBeeStateChanged(Bee bee, BeeState newState)
        {
            UpdateBeeCategories();
        }
        
        public void RegisterBee(Bee bee)
        {
            if (bee != null && !allBees.Contains(bee))
            {
                allBees.Add(bee);
                
                if (bee is QueenBee queenBee)
                {
                    queenBees.Add(queenBee);
                }
                else
                {
                    workerBees.Add(bee);
                }
                
                UpdateBeeCategories();
                OnBeeSpawned?.Invoke(bee);
                OnPopulationChanged?.Invoke(TotalBeeCount);
            }
        }
        
        public void RemoveBee(Bee bee)
        {
            if (bee != null && allBees.Contains(bee))
            {
                allBees.Remove(bee);
                
                if (bee is QueenBee queenBee)
                {
                    queenBees.Remove(queenBee);
                }
                else
                {
                    workerBees.Remove(bee);
                }
                
                UpdateBeeCategories();
                OnBeeRemoved?.Invoke(bee);
                OnPopulationChanged?.Invoke(TotalBeeCount);
                
                if (bee.gameObject != null)
                {
                    Destroy(bee.gameObject);
                }
            }
        }
        
        public Bee SpawnWorkerBee()
        {
            if (TotalBeeCount >= maxBeePopulation || workerBeePrefab == null) return null;
            
            Vector3 spawnPosition = beeSpawnPoint != null ? beeSpawnPoint.position : Vector3.zero;
            GameObject beeObject = Instantiate(workerBeePrefab, spawnPosition, Quaternion.identity);
            Bee bee = beeObject.GetComponent<Bee>();
            
            if (bee != null)
            {
                RegisterBee(bee);
            }
            
            return bee;
        }
        
        public QueenBee SpawnQueenBee()
        {
            if (TotalBeeCount >= maxBeePopulation || queenBeePrefab == null) return null;
            
            Vector3 spawnPosition = beeSpawnPoint != null ? beeSpawnPoint.position : Vector3.zero;
            GameObject beeObject = Instantiate(queenBeePrefab, spawnPosition, Quaternion.identity);
            QueenBee queenBee = beeObject.GetComponent<QueenBee>();
            
            if (queenBee != null)
            {
                RegisterBee(queenBee);
            }
            
            return queenBee;
        }
        
        private void UpdateBeeCategories()
        {
            idleBees.Clear();
            workingBees.Clear();
            
            foreach (Bee bee in workerBees)
            {
                if (bee.CurrentRole == BeeRole.Idle || bee.CurrentState == BeeState.Idling)
                {
                    idleBees.Add(bee);
                }
                else if (bee.CurrentState == BeeState.Working || bee.CurrentState == BeeState.Foraging)
                {
                    workingBees.Add(bee);
                }
            }
        }
        
        private void UpdateBeeAssignments()
        {
            // Auto-assign idle bees to available tasks
            if (GameManager.Instance.TaskManager != null)
            {
                var availableTasks = GameManager.Instance.TaskManager.GetAvailableTasks();
                
                foreach (var task in availableTasks)
                {
                    if (idleBees.Count == 0) break;
                    
                    Bee suitableBee = FindSuitableBeeForTask(task);
                    if (suitableBee != null)
                    {
                        AssignBeeToTask(suitableBee, task);
                    }
                }
            }
        }
        
        private Bee FindSuitableBeeForTask(BeeTask task)
        {
            // Find an idle bee that can perform this task
            foreach (Bee bee in idleBees)
            {
                if (CanBeePerformTask(bee, task))
                {
                    return bee;
                }
            }
            return null;
        }
        
        private bool CanBeePerformTask(Bee bee, BeeTask task)
        {
            // Check if bee is available and healthy
            if (bee.CurrentState == BeeState.Sleeping || bee.CurrentState == BeeState.Dying)
                return false;
            
            // Check if bee can perform the task type
            switch (task.taskType)
            {
                case TaskType.Forage:
                    return GameManager.Instance.TimeManager.CanForage();
                case TaskType.Build:
                case TaskType.Supply:
                case TaskType.Clean:
                case TaskType.Thermoregulate:
                    return true;
                default:
                    return false;
            }
        }
        
        private void AssignBeeToTask(Bee bee, BeeTask task)
        {
            BeeRole roleForTask = GetRoleForTask(task);
            bee.AssignRole(roleForTask);
        }
        
        private BeeRole GetRoleForTask(BeeTask task)
        {
            switch (task.taskType)
            {
                case TaskType.Forage:
                    return task.resourceType == ResourceType.Honey ? BeeRole.ForageHoney : BeeRole.ForagePollen;
                case TaskType.Build:
                    return BeeRole.BuildCells;
                case TaskType.Supply:
                    return BeeRole.NurseLarvae;
                case TaskType.Clean:
                    return BeeRole.CleanCells;
                case TaskType.Thermoregulate:
                    return BeeRole.Thermoregulate;
                default:
                    return BeeRole.Idle;
            }
        }
        
        public List<Bee> GetBeesByRole(BeeRole role)
        {
            return allBees.Where(bee => bee.CurrentRole == role).ToList();
        }
        
        public List<Bee> GetBeesByState(BeeState state)
        {
            return allBees.Where(bee => bee.CurrentState == state).ToList();
        }
        
        public int GetWorkerBeeCount()
        {
            return workerBees.Count;
        }
        
        public int GetQueenBeeCount()
        {
            return queenBees.Count;
        }
        
        public void AssignBeeRole(Bee bee, BeeRole role)
        {
            if (bee != null && allBees.Contains(bee))
            {
                bee.AssignRole(role);
            }
        }
        
        public void SetMaxPopulation(int newMax)
        {
            maxBeePopulation = newMax;
        }
    }
}