using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private GameObject player;

    public float playerHealth;
    public float playerPositionX;
    public float playerPositionY;
    public int playerDirection;
    public int playerSpawnID;
    public string currentScene;

    void Awake()
    {
        player = this.gameObject;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(AutomaticSave());
    }

    // Coroutine pour sauvegarder automatiquement le jeu
    private IEnumerator AutomaticSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // Sauvegarde toutes les 60 secondes
            if (player.GetComponent<PlayerController>().isGrounded) SaveGame();
        }
    }

    // Méthode pour sauvegarder le jeu
    public void SaveGame()
    {
        if (player == null) return;

        playerHealth = player.GetComponent<Health>().GetCurrentHealth();
        playerPositionX = player.transform.position.x;
        playerPositionY = player.transform.position.y;
        playerDirection = player.transform.localScale.x > 0 ? 1 : -1;
        playerSpawnID = SceneTransitionManager.Instance.GetSpawnID();
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Sauvegarder
        PlayerPrefs.SetFloat("PlayerHealth", playerHealth);
        PlayerPrefs.SetFloat("PlayerPositionX", playerPositionX);
        PlayerPrefs.SetFloat("PlayerPositionY", playerPositionY);
        PlayerPrefs.SetInt("PlayerDirection", playerDirection);
        PlayerPrefs.SetInt("PlayerSpawnID", playerSpawnID);
        PlayerPrefs.SetString("CurrentScene", currentScene);
        PlayerPrefs.Save();
    }
}
