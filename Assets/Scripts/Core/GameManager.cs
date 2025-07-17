using UnityEngine;
using System;
using Mellifera.Controllers;

namespace Mellifera.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game State")]
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        
        [Header("Manager References")]
        public TimeManager TimeManager { get; private set; }
        public ResourceManager ResourceManager { get; private set; }
        public BeeManager BeeManager { get; private set; }
        public TaskManager TaskManager { get; private set; }
        public ExternalMapController ExternalMapController { get; private set; }
        
        public static event Action<GameState> OnGameStateChanged;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManagers();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Auto-start the game for testing
            StartGame();
        }
        
        private void InitializeManagers()
        {
            TimeManager = GetComponent<TimeManager>() ?? gameObject.AddComponent<TimeManager>();
            ResourceManager = GetComponent<ResourceManager>() ?? gameObject.AddComponent<ResourceManager>();
            BeeManager = GetComponent<BeeManager>() ?? gameObject.AddComponent<BeeManager>();
            TaskManager = GetComponent<TaskManager>() ?? gameObject.AddComponent<TaskManager>();
            ExternalMapController = FindObjectOfType<ExternalMapController>();
        }
        
        public void ChangeGameState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
            
            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }
        }
        
        public void StartGame()
        {
            ChangeGameState(GameState.Playing);
        }
        
        public void PauseGame()
        {
            ChangeGameState(GameState.Paused);
        }
        
        public void ResumeGame()
        {
            ChangeGameState(GameState.Playing);
        }
        
        public void EndGame()
        {
            ChangeGameState(GameState.GameOver);
        }
        
        public void ReturnToMainMenu()
        {
            ChangeGameState(GameState.MainMenu);
        }
    }
}