using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gateway : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private int spawnID; // ID de spawn pour la scène cible

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetSceneName != "")
        {
            SceneTransitionManager.Instance.SetSpawnID(spawnID); // Enregistre l'ID de spawn
            SceneManager.LoadScene(targetSceneName);
        }
    }
    
}

