using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gateway : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private int spawnID; // ID de spawn pour la scène cible
    [SerializeField] private GatewayPosition position;
    [SerializeField] private bool isEndGateway;

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
                    other.GetComponent<Rigidbody2D>().velocity = Vector2.zero; // Stop the player movement
                    playerController.SimulateHorizontalMovement(direction, 1f, false, true); 
                }
            }
            SceneTransitionManager.Instance.SetSpawnID(spawnID); // Enregistre l'ID de spawn
            PlayerManager.Instance.SavePlayerHealth();
            SceneFader.Instance.FadeToScene(targetSceneName);
        }
        else if (isEndGateway)
        {
            SceneFader.Instance.FadeToMenuAndDestroy();
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

