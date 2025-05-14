using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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
    }

    public void Quit()
    {
        // Quitte l'application
        Application.Quit();
        Debug.Log("Application fermée");
    }

    public void Play()
    {  
        SceneFader.Instance.FadeToScene("Floor1");
    }
}
