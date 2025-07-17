using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Mellifera.Data;
using Mellifera.Units;
using Mellifera.Core;
using Mellifera.Events;

namespace Mellifera.Controllers
{
    public class ExternalMapController : MonoBehaviour
    {
        [Header("Map Configuration")]
        [SerializeField] private Vector2 mapSize = new Vector2(100f, 20f);
        [SerializeField] private Vector3 hiveEntrancePosition = Vector3.zero;
        [SerializeField] private LayerMask groundLayer = 1;
        
        [Header("Resource Node Prefabs")]
        [SerializeField] private GameObject honeySourcePrefab;
        [SerializeField] private GameObject pollenSourcePrefab;
        [SerializeField] private GameObject resourceNodePrefab;
        
        [Header("Hazard Prefabs")]
        [SerializeField] private GameObject spiderWebPrefab;
        [SerializeField] private GameObject windZonePrefab;
        [SerializeField] private GameObject hazardPrefab;
        
        [Header("Foraging Configuration")]
        [SerializeField] private float beeSpeed = 5f;
        [SerializeField] private float pathRecordingInterval = 0.5f;
        [SerializeField] private int maxConcurrentForagers = 10;
        
        private List<ResourceNode> resourceNodes = new List<ResourceNode>();
        private List<Hazard> hazards = new List<Hazard>();
        private List<ForagingRoute> discoveredRoutes = new List<ForagingRoute>();
        private Dictionary<Bee, ForagingRoute> activeForagers = new Dictionary<Bee, ForagingRoute>();
        private Dictionary<Bee, List<Vector3>> pathRecording = new Dictionary<Bee, List<Vector3>>();
        
        private Camera mapCamera;
        private bool isPioneerMode = false;
        private Bee currentPioneerBee;
        private Vector3 currentTargetPosition;
        private List<Vector3> currentPath = new List<Vector3>();
        
        public List<ResourceNode> ResourceNodes => resourceNodes;
        public List<Hazard> Hazards => hazards;
        public List<ForagingRoute> DiscoveredRoutes => discoveredRoutes;
        public bool IsPioneerMode => isPioneerMode;
        public int ActiveForagersCount => activeForagers.Count;
        
        public System.Action<ResourceNode> OnResourceNodeDiscovered;
        public System.Action<ForagingRoute> OnRouteEstablished;
        public System.Action<Bee, float> OnResourceCollected;
        public System.Action<Bee, Hazard> OnBeeEncounteredHazard;
        
        private void Start()
        {
            InitializeMap();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            
            UpdateResourceNodes();
            UpdateHazards();
            UpdateForagers();
            HandlePioneerInput();
        }
        
        private void SubscribeToEvents()
        {
            GameEvents.OnTick += HandleTick;
            GameEvents.OnNightfall += HandleNightfall;
            GameEvents.OnSeasonChanged += HandleSeasonChanged;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnTick -= HandleTick;
            GameEvents.OnNightfall -= HandleNightfall;
            GameEvents.OnSeasonChanged -= HandleSeasonChanged;
        }
        
        private void InitializeMap()
        {
            mapCamera = Camera.main;
            GenerateResourceNodes();
            GenerateHazards();
        }
        
        private void GenerateResourceNodes()
        {
            // Generate honey sources
            for (int i = 0; i < 3; i++)
            {
                Vector3 position = GetRandomMapPosition();
                ResourceNode honeyNode = new ResourceNode(ResourceType.Honey, position, 100f);
                resourceNodes.Add(honeyNode);
                
                if (honeySourcePrefab != null)
                {
                    GameObject nodeObject = Instantiate(honeySourcePrefab, position, Quaternion.identity, transform);
                    nodeObject.name = $"HoneySource_{i}";
                }
            }
            
            // Generate pollen sources
            for (int i = 0; i < 3; i++)
            {
                Vector3 position = GetRandomMapPosition();
                ResourceNode pollenNode = new ResourceNode(ResourceType.Pollen, position, 80f);
                resourceNodes.Add(pollenNode);
                
                if (pollenSourcePrefab != null)
                {
                    GameObject nodeObject = Instantiate(pollenSourcePrefab, position, Quaternion.identity, transform);
                    nodeObject.name = $"PollenSource_{i}";
                }
            }
        }
        
        private void GenerateHazards()
        {
            // Generate spider webs
            for (int i = 0; i < 2; i++)
            {
                Vector3 position = GetRandomMapPosition();
                Hazard spiderHazard = new Hazard(HazardType.Spider, position, 3f, 20f);
                hazards.Add(spiderHazard);
                
                if (spiderWebPrefab != null)
                {
                    GameObject hazardObject = Instantiate(spiderWebPrefab, position, Quaternion.identity, transform);
                    hazardObject.name = $"SpiderWeb_{i}";
                }
            }
            
            // Generate wind zones
            for (int i = 0; i < 1; i++)
            {
                Vector3 position = GetRandomMapPosition();
                Hazard windHazard = new Hazard(HazardType.Wind, position, 5f, 10f);
                hazards.Add(windHazard);
                
                if (windZonePrefab != null)
                {
                    GameObject hazardObject = Instantiate(windZonePrefab, position, Quaternion.identity, transform);
                    hazardObject.name = $"WindZone_{i}";
                }
            }
        }
        
        private Vector3 GetRandomMapPosition()
        {
            float x = Random.Range(-mapSize.x / 2f, mapSize.x / 2f);
            float y = Random.Range(-mapSize.y / 2f, mapSize.y / 2f);
            return new Vector3(x, y, 0f);
        }
        
        private void HandleTick(float deltaTime)
        {
            // Update resource regeneration and hazards
        }
        
        private void HandleNightfall()
        {
            // Return all foraging bees to hive
            RecallAllForagers();
        }
        
        private void HandleSeasonChanged(Season newSeason)
        {
            if (newSeason == Season.Winter)
            {
                // No foraging in winter
                RecallAllForagers();
            }
        }
        
        private void UpdateResourceNodes()
        {
            foreach (ResourceNode node in resourceNodes)
            {
                node.Regenerate(Time.deltaTime);
            }
        }
        
        private void UpdateHazards()
        {
            foreach (Hazard hazard in hazards)
            {
                hazard.UpdateHazard(Time.deltaTime);
            }
        }
        
        private void UpdateForagers()
        {
            foreach (var forager in activeForagers.ToList())
            {
                UpdateForagerMovement(forager.Key, forager.Value);
            }
        }
        
        private void UpdateForagerMovement(Bee bee, ForagingRoute route)
        {
            if (bee == null || route == null) return;
            
            // Move bee along the path
            Vector3 targetPosition = GetNextPathPoint(bee, route);
            Vector3 direction = (targetPosition - bee.transform.position).normalized;
            
            bee.transform.position += direction * beeSpeed * Time.deltaTime;
            
            // Check if reached target
            if (Vector3.Distance(bee.transform.position, targetPosition) < 0.5f)
            {
                AdvanceToNextPathPoint(bee, route);
            }
            
            // Check for hazards
            CheckForHazards(bee);
        }
        
        private Vector3 GetNextPathPoint(Bee bee, ForagingRoute route)
        {
            // Implementation for path following
            return route.pathPoints[0]; // Simplified
        }
        
        private void AdvanceToNextPathPoint(Bee bee, ForagingRoute route)
        {
            // Implementation for advancing along path
        }
        
        private void CheckForHazards(Bee bee)
        {
            foreach (Hazard hazard in hazards)
            {
                if (hazard.isActive && hazard.IsInRange(bee.transform.position))
                {
                    OnBeeEncounteredHazard?.Invoke(bee, hazard);
                    bee.TakeDamage(hazard.damage);
                }
            }
        }
        
        private void HandlePioneerInput()
        {
            if (!isPioneerMode) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                Vector3 worldPosition = mapCamera.ScreenToWorldPoint(mousePosition);
                worldPosition.z = 0f;
                
                SetPioneerTarget(worldPosition);
            }
        }
        
        public void StartPioneerMode(Bee bee)
        {
            if (bee == null || !GameManager.Instance.TimeManager.CanForage()) return;
            
            isPioneerMode = true;
            currentPioneerBee = bee;
            currentPath.Clear();
            currentPath.Add(bee.transform.position);
            
            bee.ChangeState(BeeState.Foraging);
        }
        
        public void SetPioneerTarget(Vector3 targetPosition)
        {
            if (!isPioneerMode || currentPioneerBee == null) return;
            
            currentTargetPosition = targetPosition;
            
            // Find nearest resource node
            ResourceNode nearestNode = FindNearestResourceNode(targetPosition);
            if (nearestNode != null)
            {
                MovePioneerToTarget(nearestNode);
            }
        }
        
        private ResourceNode FindNearestResourceNode(Vector3 position)
        {
            ResourceNode nearest = null;
            float minDistance = float.MaxValue;
            
            foreach (ResourceNode node in resourceNodes)
            {
                float distance = Vector3.Distance(position, node.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = node;
                }
            }
            
            return nearest;
        }
        
        private void MovePioneerToTarget(ResourceNode targetNode)
        {
            if (currentPioneerBee == null || targetNode == null) return;
            
            // Record path during movement
            StartCoroutine(RecordPathToPioneerTarget(targetNode));
        }
        
        private System.Collections.IEnumerator RecordPathToPioneerTarget(ResourceNode targetNode)
        {
            Vector3 startPosition = currentPioneerBee.transform.position;
            Vector3 targetPosition = targetNode.position;
            
            while (Vector3.Distance(currentPioneerBee.transform.position, targetPosition) > 0.5f)
            {
                // Move bee toward target
                Vector3 direction = (targetPosition - currentPioneerBee.transform.position).normalized;
                currentPioneerBee.transform.position += direction * beeSpeed * Time.deltaTime;
                
                // Record path point
                if (Time.time % pathRecordingInterval < Time.deltaTime)
                {
                    currentPath.Add(currentPioneerBee.transform.position);
                }
                
                yield return null;
            }
            
            // Reached target
            CompletePioneerRoute(targetNode);
        }
        
        private void CompletePioneerRoute(ResourceNode targetNode)
        {
            if (currentPioneerBee == null || targetNode == null) return;
            
            // Finalize path
            currentPath.Add(targetNode.position);
            
            // Create new route
            ForagingRoute newRoute = new ForagingRoute(targetNode, currentPath);
            discoveredRoutes.Add(newRoute);
            
            // Update resource node
            targetNode.SetPath(currentPath);
            targetNode.isDiscovered = true;
            
            // Collect resources
            float collectedAmount = targetNode.Harvest(currentPioneerBee.Stats.carryCapacity);
            if (collectedAmount > 0)
            {
                OnResourceCollected?.Invoke(currentPioneerBee, collectedAmount);
                
                // Add resource to inventory
                GameManager.Instance.ResourceManager.AddResource(targetNode.resourceType, collectedAmount);
            }
            
            // Return to hive
            ReturnBeeToHive(currentPioneerBee);
            
            // Events
            OnResourceNodeDiscovered?.Invoke(targetNode);
            OnRouteEstablished?.Invoke(newRoute);
            
            // End pioneer mode
            EndPioneerMode();
        }
        
        public void EndPioneerMode()
        {
            isPioneerMode = false;
            currentPioneerBee = null;
            currentTargetPosition = Vector3.zero;
            currentPath.Clear();
        }
        
        public bool AssignBeeToRoute(Bee bee, ForagingRoute route)
        {
            if (bee == null || route == null || activeForagers.Count >= maxConcurrentForagers) return false;
            
            if (!activeForagers.ContainsKey(bee))
            {
                activeForagers[bee] = route;
                route.IncrementUseCount();
                bee.ChangeState(BeeState.Foraging);
                return true;
            }
            
            return false;
        }
        
        public bool AssignBeeToResourceNode(Bee bee, ResourceNode node)
        {
            ForagingRoute route = discoveredRoutes.FirstOrDefault(r => r.targetNode == node);
            if (route != null)
            {
                return AssignBeeToRoute(bee, route);
            }
            return false;
        }
        
        public void ReturnBeeToHive(Bee bee)
        {
            if (bee != null)
            {
                activeForagers.Remove(bee);
                pathRecording.Remove(bee);
                
                // Move bee back to hive
                bee.transform.position = hiveEntrancePosition;
                bee.ChangeState(BeeState.Idling);
            }
        }
        
        public void RecallAllForagers()
        {
            foreach (Bee bee in activeForagers.Keys.ToList())
            {
                ReturnBeeToHive(bee);
            }
            
            activeForagers.Clear();
        }
        
        public List<ResourceNode> GetDiscoveredNodes()
        {
            return resourceNodes.Where(node => node.isDiscovered).ToList();
        }
        
        public List<ResourceNode> GetNodesByType(ResourceType resourceType)
        {
            return resourceNodes.Where(node => node.resourceType == resourceType).ToList();
        }
        
        public ResourceNode GetBestResourceNode(ResourceType resourceType)
        {
            return resourceNodes
                .Where(node => node.resourceType == resourceType && node.isDiscovered && node.CanHarvest)
                .OrderByDescending(node => node.currentAmount)
                .FirstOrDefault();
        }
        
        public void SetMapSize(Vector2 newSize)
        {
            mapSize = newSize;
        }
        
        public void SetHiveEntrance(Vector3 position)
        {
            hiveEntrancePosition = position;
        }
    }
}