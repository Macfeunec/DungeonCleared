using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private int spawnID;
    [SerializeField] private bool fromBellow;
    [SerializeField] private Direction spawnDirection;
    [SerializeField] private float jumpForce;

    void Start()
    {
        // Vérifie si le spawnID correspond 
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.GetSpawnID() == spawnID)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null  && spawnDirection != Direction.None)
            {
                player.transform.localScale = new Vector3(spawnDirection == Direction.Right ? 1 : -1, 1, 1); // Change l'échelle du joueur pour le faire face à la direction de spawn
            }
            if (player != null)
            {
                player.transform.position = transform.position; // Positionne le joueur au point de spawn

                // Vérifie si le joueur vient d'en bas
                if (fromBellow)
                {
                    PlayerController playerController = player.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.SimulateJump(jumpForce, false, false);                         // Simule le saut du joueur
                        playerController.SimulateHorizontalMovement(spawnDirection, 2f, true, false);   // Simule le mouvement horizontal du joueur
                    }
                }
            }
        }
    }
}

public enum Direction
{
    None,
    Left,
    Right
}
