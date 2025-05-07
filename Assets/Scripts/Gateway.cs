using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gateway : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private int spawnID; // ID de spawn pour la scène cible
    [SerializeField] private GatewayPosition position;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetSceneName != "")
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (position == GatewayPosition.Up)
            {
                if (playerController != null) playerController.SimulateJump(32, false, true); 
            }
            else if (position != GatewayPosition.Down)
            {
                Direction direction = position == GatewayPosition.Right ? Direction.Right : 
                            position == GatewayPosition.Left ? Direction.Left : Direction.None;

                if (playerController != null) {
                    playerController.DisableMovement();
                    playerController.SimulateHorizontalMovement(direction, 1f, false, true); 
                }
            }
            SceneTransitionManager.Instance.SetSpawnID(spawnID); // Enregistre l'ID de spawn
            SceneFader.Instance.FadeToScene(targetSceneName);
        }
    }
}

public enum GatewayPosition
{
    Up,
    Down,
    Left,
    Right
}

