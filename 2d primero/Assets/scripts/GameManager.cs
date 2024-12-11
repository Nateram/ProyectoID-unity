using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public string transitionedFromScene;
    public Vector2 basicRespawnPoint;
    public Vector2 respownPoint;
    private bench currentBench;
    [SerializeField] private Menugameover menuGameover; // Add reference

    // Remove this since we'll use the singleton
    // [SerializeField] private PlayerController playerController;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void SetCurrentBench(bench newBench)
    {
        Debug.Log($"Setting new bench: {newBench}");
        currentBench = newBench;
        respownPoint = newBench.transform.position;
        Debug.Log($"New bench set: {newBench.transform.position}");
    }
    public void respawnplayer()
    {
        Debug.Log($"Current bench: {currentBench}");
        Debug.Log($"Basic respawn point: {basicRespawnPoint}");
        Debug.Log($"Bench respawn point: {respownPoint}");

        // Store reference before checking
        var benchToUse = currentBench;
        
        if(benchToUse != null && benchToUse.interacted)
        {
            Debug.Log($"Respawning at bench position: {respownPoint}");
            PlayerController.Instance.transform.position = respownPoint;
        }
        else
        {
            Debug.Log($"Respawning at basic position: {basicRespawnPoint}");
            PlayerController.Instance.transform.position = basicRespawnPoint;
        }

        PlayerController.Instance.Respawned();
        menuGameover.HideDeathMenu();
    }
}