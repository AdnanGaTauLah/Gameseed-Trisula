using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Prefabs")]
    [SerializeField] private List<GameObject> levelPrefabs;

    [Header("Setup")]
    [Tooltip("The parent object where the level prefab will be placed.")]
    [SerializeField] private Transform levelContainer;

    private GameObject currentLevelInstance;
    private int currentLevelIndex = 0;

    // This is a basic "singleton" pattern to allow other scripts to easily access the LevelManager
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Load the first level when the game starts
        LoadLevel(currentLevelIndex);
    }

    /// <summary>
    /// Loads a level by its index in the prefab list.
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        // 1. Destroy the old level if it exists
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        // 2. Check if the requested level index is valid
        if (levelIndex < 0 || levelIndex >= levelPrefabs.Count)
        {
            Debug.LogError($"LevelManager: Invalid level index {levelIndex}.");
            return;
        }

        // 3. Instantiate the new level prefab
        currentLevelIndex = levelIndex;
        GameObject levelPrefab = levelPrefabs[currentLevelIndex];

        // Instantiate the prefab and parent it to the container for a clean hierarchy
        currentLevelInstance = Instantiate(levelPrefab, levelContainer);

        Debug.Log($"Level {currentLevelIndex + 1} loaded.");
    }

    /// <summary>
    /// Reloads the current level.
    /// </summary>
    public void ReloadCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    /// <summary>
    /// Loads the next level in the list.
    /// </summary>
    public void LoadNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex >= levelPrefabs.Count)
        {
            // If we're at the end, either loop back to the start or show a "Game Complete" screen
            Debug.Log("All levels complete! Looping back to level 1.");
            nextLevelIndex = 0;
        }
        LoadLevel(nextLevelIndex);
    }
}
