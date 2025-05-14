using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private GameObject player;

    public float playerMaxHealth;
    public float playerHealth;
    public float playerMaxStamina;
    public float playerStamina;
    public float playerPositionX;
    public float playerPositionY;
    public int playerDirection;
    public int playerSpawnID;
    public string currentScene;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ne pas détruire cet objet lors du chargement d'une nouvelle scène
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayerHealth()
    {   
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found.");
            return;
        }
        playerHealth = player.GetComponent<Health>().GetCurrentHealth();
        playerMaxHealth = player.GetComponent<Health>().GetMaxHealth();
        playerMaxStamina = player.GetComponent<PlayerController>().GetMaxStamina();
        playerStamina = player.GetComponent<PlayerController>().GetStamina();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in the scene.");
            return;
        }
    }

    void Start()
    {
        playerHealth = playerMaxHealth;
        // StartCoroutine(AutomaticSave());
    }

    // Respawn le joueur avec toute sa vie
    public void Respawn() 
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found.");
            return;
        }
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.SetCurrentHealth(playerHealth.GetMaxHealth());
            SceneTransitionManager.Instance.SetSpawnID(playerSpawnID);
            player.GetComponent<PlayerController>()?.ResetPlayer();
        }
        else
        {
            Debug.LogError("Player Health component not found.");
        }        
    }

}
