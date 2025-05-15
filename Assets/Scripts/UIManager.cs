using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    void Start()
    {
        if (PlayerManager.Instance != null) PlayerManager.Instance.enabled = false;   
    }
    // public static UIManager Instance;
    /*
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }*/

    public void Quit()
    {
        // Quitte l'application
        Application.Quit();
        Debug.Log("Application fermée");
    }

    public void Play()
    {  
        if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.SetSpawnID(0);
        if (PlayerManager.Instance != null) 
        {
            PlayerManager.Instance.enabled = true;
            PlayerManager.Instance.playerMaxHealth = 100;
            PlayerManager.Instance.playerHealth = 100;
        }
        SceneFader.Instance.FadeToScene("Floor1");
    }
}
