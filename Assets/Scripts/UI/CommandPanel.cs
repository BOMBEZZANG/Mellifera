using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mellifera.Core;
using Mellifera.Data;
using Mellifera.Units;
using Mellifera.Controllers;
using Mellifera.Systems;

namespace Mellifera.UI
{
    public class CommandPanel : MonoBehaviour
    {
        [Header("Build Commands")]
        [SerializeField] private Button buildBasicCellButton;
        [SerializeField] private Button buildNurseryCellButton;
        [SerializeField] private Button buildHoneyStorageButton;
        [SerializeField] private Button buildPollenStorageButton;
        
        [Header("Role Assignment")]
        [SerializeField] private TMP_Dropdown roleAssignmentDropdown;
        [SerializeField] private Button assignRoleButton;
        [SerializeField] private TextMeshProUGUI selectedBeeInfoText;
        
        [Header("Foraging Commands")]
        [SerializeField] private Button startPioneerButton;
        [SerializeField] private Button dispatchForagersButton;
        [SerializeField] private TMP_Dropdown foragingTargetDropdown;
        
        [Header("Colony Commands")]
        [SerializeField] private Button thermoregulateButton;
        [SerializeField] private Button feedQueenButton;
        [SerializeField] private Button feedLarvaeButton;
        
        [Header("Game Controls")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private Button speedDownButton;
        [SerializeField] private TextMeshProUGUI gameSpeedText;
        
        private Bee selectedBee;
        private HiveCell selectedCell;
        private bool isBuildMode = false;
        private HiveCellType currentBuildType = HiveCellType.Basic;
        private float currentGameSpeed = 1f;
        
        private BeeManager beeManager;
        private TaskManager taskManager;
        private ExternalMapController externalMapController;
        private ResourceManager resourceManager;
        
        private void Start()
        {
            InitializeReferences();
            InitializeUI();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeReferences()
        {
            beeManager = GameManager.Instance.BeeManager;
            taskManager = GameManager.Instance.TaskManager;
            externalMapController = GameManager.Instance.ExternalMapController;
            resourceManager = GameManager.Instance.ResourceManager;
        }
        
        private void InitializeUI()
        {
            // Build commands
            if (buildBasicCellButton != null)
                buildBasicCellButton.onClick.AddListener(() => StartBuildMode(HiveCellType.Basic));
            
            if (buildNurseryCellButton != null)
                buildNurseryCellButton.onClick.AddListener(() => StartBuildMode(HiveCellType.Nursery));
            
            if (buildHoneyStorageButton != null)
                buildHoneyStorageButton.onClick.AddListener(() => StartBuildMode(HiveCellType.HoneyStorage));
            
            if (buildPollenStorageButton != null)
                buildPollenStorageButton.onClick.AddListener(() => StartBuildMode(HiveCellType.PollenStorage));
            
            // Role assignment
            if (roleAssignmentDropdown != null)
                SetupRoleDropdown();
            
            if (assignRoleButton != null)
                assignRoleButton.onClick.AddListener(AssignSelectedRole);
            
            // Foraging commands
            if (startPioneerButton != null)
                startPioneerButton.onClick.AddListener(StartPioneerMode);
            
            if (dispatchForagersButton != null)
                dispatchForagersButton.onClick.AddListener(DispatchForagers);
            
            if (foragingTargetDropdown != null)
                SetupForagingTargetDropdown();
            
            // Colony commands
            if (thermoregulateButton != null)
                thermoregulateButton.onClick.AddListener(ThermoregulateHive);
            
            if (feedQueenButton != null)
                feedQueenButton.onClick.AddListener(FeedQueen);
            
            if (feedLarvaeButton != null)
                feedLarvaeButton.onClick.AddListener(FeedLarvae);
            
            // Game controls
            if (pauseButton != null)
                pauseButton.onClick.AddListener(TogglePause);
            
            if (speedUpButton != null)
                speedUpButton.onClick.AddListener(IncreaseGameSpeed);
            
            if (speedDownButton != null)
                speedDownButton.onClick.AddListener(DecreaseGameSpeed);
            
            UpdateUI();
        }
        
        private void SetupRoleDropdown()
        {
            if (roleAssignmentDropdown == null) return;
            
            roleAssignmentDropdown.ClearOptions();
            
            var options = new System.Collections.Generic.List<string>();
            foreach (BeeRole role in System.Enum.GetValues(typeof(BeeRole)))
            {
                options.Add(role.ToString());
            }
            
            roleAssignmentDropdown.AddOptions(options);
        }
        
        private void SetupForagingTargetDropdown()
        {
            if (foragingTargetDropdown == null || externalMapController == null) return;
            
            foragingTargetDropdown.ClearOptions();
            
            var options = new System.Collections.Generic.List<string>();
            var discoveredNodes = externalMapController.GetDiscoveredNodes();
            
            foreach (var node in discoveredNodes)
            {
                options.Add($"{node.resourceType} - {node.currentAmount:F1}/{node.maxAmount:F1}");
            }
            
            if (options.Count == 0)
            {
                options.Add("No discovered resources");
            }
            
            foragingTargetDropdown.AddOptions(options);
        }
        
        private void SubscribeToEvents()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
            
            if (beeManager != null)
            {
                beeManager.OnPopulationChanged += HandlePopulationChanged;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            
            if (beeManager != null)
            {
                beeManager.OnPopulationChanged -= HandlePopulationChanged;
            }
        }
        
        private void HandleGameStateChanged(GameState newState)
        {
            UpdateUI();
        }
        
        private void HandlePopulationChanged(int newPopulation)
        {
            UpdateUI();
        }
        
        private void Update()
        {
            HandleInput();
            UpdateUI();
        }
        
        private void HandleInput()
        {
            // Handle mouse clicks for selection
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
            
            // Handle build mode
            if (isBuildMode && Input.GetMouseButtonDown(0))
            {
                HandleBuildPlacement();
            }
            
            // Cancel build mode
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelBuildMode();
            }
        }
        
        private void HandleMouseClick()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            
            // Try to select a bee
            Collider2D beeCollider = Physics2D.OverlapPoint(mousePosition);
            if (beeCollider != null)
            {
                Bee bee = beeCollider.GetComponent<Bee>();
                if (bee != null)
                {
                    SelectBee(bee);
                    return;
                }
                
                // Try to select a hive cell
                HiveCell cell = beeCollider.GetComponent<HiveCell>();
                if (cell != null)
                {
                    SelectCell(cell);
                    return;
                }
            }
            
            // Deselect if clicked on empty space
            DeselectAll();
        }
        
        private void HandleBuildPlacement()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            
            // Create build task at mouse position
            if (taskManager != null)
            {
                string description = $"Build {currentBuildType} cell";
                taskManager.CreateTask(TaskType.Build, TaskPriority.Medium, description, mousePosition);
            }
            
            CancelBuildMode();
        }
        
        private void SelectBee(Bee bee)
        {
            selectedBee = bee;
            selectedCell = null;
            UpdateSelectedBeeInfo();
        }
        
        private void SelectCell(HiveCell cell)
        {
            selectedCell = cell;
            selectedBee = null;
            UpdateSelectedBeeInfo();
        }
        
        private void DeselectAll()
        {
            selectedBee = null;
            selectedCell = null;
            UpdateSelectedBeeInfo();
        }
        
        private void UpdateSelectedBeeInfo()
        {
            if (selectedBeeInfoText == null) return;
            
            if (selectedBee != null)
            {
                selectedBeeInfoText.text = $"Selected: {selectedBee.BeeName}\n" +
                                         $"Role: {selectedBee.CurrentRole}\n" +
                                         $"State: {selectedBee.CurrentState}\n" +
                                         $"Age: {selectedBee.CurrentAge:F1}/{selectedBee.Lifespan:F1}\n" +
                                         $"Health: {selectedBee.Health:F1}/{selectedBee.MaxHealth:F1}";
            }
            else if (selectedCell != null)
            {
                selectedBeeInfoText.text = $"Selected: {selectedCell.CellType} Cell\n" +
                                         $"Capacity: {selectedCell.CurrentAmount:F1}/{selectedCell.MaxCapacity:F1}\n" +
                                         $"Temperature: {selectedCell.Temperature:F1}°C\n" +
                                         $"Constructed: {(selectedCell.IsConstructed ? "Yes" : "No")}";
            }
            else
            {
                selectedBeeInfoText.text = "No selection";
            }
        }
        
        private void StartBuildMode(HiveCellType cellType)
        {
            isBuildMode = true;
            currentBuildType = cellType;
            
            // Visual feedback for build mode
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        
        private void CancelBuildMode()
        {
            isBuildMode = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        
        private void AssignSelectedRole()
        {
            if (selectedBee == null || roleAssignmentDropdown == null || beeManager == null) return;
            
            int selectedIndex = roleAssignmentDropdown.value;
            BeeRole selectedRole = (BeeRole)selectedIndex;
            
            beeManager.AssignBeeRole(selectedBee, selectedRole);
        }
        
        private void StartPioneerMode()
        {
            if (selectedBee == null || externalMapController == null) return;
            
            externalMapController.StartPioneerMode(selectedBee);
        }
        
        private void DispatchForagers()
        {
            if (beeManager == null || externalMapController == null) return;
            
            var idleBees = beeManager.IdleBees;
            var discoveredNodes = externalMapController.GetDiscoveredNodes();
            
            foreach (var bee in idleBees)
            {
                if (discoveredNodes.Count > 0)
                {
                    var bestNode = externalMapController.GetBestResourceNode(ResourceType.Honey);
                    if (bestNode != null)
                    {
                        externalMapController.AssignBeeToResourceNode(bee, bestNode);
                    }
                }
            }
        }
        
        private void ThermoregulateHive()
        {
            if (taskManager == null) return;
            
            taskManager.CreateTask(TaskType.Thermoregulate, TaskPriority.High, "Thermoregulate hive");
        }
        
        private void FeedQueen()
        {
            if (taskManager == null) return;
            
            taskManager.CreateTask(TaskType.Supply, TaskPriority.Critical, "Feed queen royal jelly");
        }
        
        private void FeedLarvae()
        {
            if (taskManager == null) return;
            
            taskManager.CreateTask(TaskType.Supply, TaskPriority.High, "Feed larvae");
        }
        
        private void TogglePause()
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
            }
            else if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
            }
        }
        
        private void IncreaseGameSpeed()
        {
            currentGameSpeed = Mathf.Min(currentGameSpeed * 2f, 8f);
            Time.timeScale = currentGameSpeed;
            UpdateGameSpeedText();
        }
        
        private void DecreaseGameSpeed()
        {
            currentGameSpeed = Mathf.Max(currentGameSpeed / 2f, 0.25f);
            Time.timeScale = currentGameSpeed;
            UpdateGameSpeedText();
        }
        
        private void UpdateGameSpeedText()
        {
            if (gameSpeedText != null)
            {
                gameSpeedText.text = $"Speed: {currentGameSpeed:F2}x";
            }
        }
        
        private void UpdateUI()
        {
            bool isPlaying = GameManager.Instance.CurrentState == GameState.Playing;
            bool hasBeeSelected = selectedBee != null;
            bool hasIdleBees = beeManager != null && beeManager.IdleBeeCount > 0;
            
            // Update button states
            if (assignRoleButton != null)
                assignRoleButton.interactable = hasBeeSelected && isPlaying;
            
            if (startPioneerButton != null)
                startPioneerButton.interactable = hasBeeSelected && isPlaying;
            
            if (dispatchForagersButton != null)
                dispatchForagersButton.interactable = hasIdleBees && isPlaying;
            
            if (pauseButton != null)
            {
                var buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = GameManager.Instance.CurrentState == GameState.Playing ? "Pause" : "Resume";
                }
            }
            
            // Update dropdowns
            if (Time.time % 2f < Time.deltaTime) // Update every 2 seconds
            {
                SetupForagingTargetDropdown();
            }
            
            UpdateGameSpeedText();
        }
    }
}