using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private int spawnID;
    [SerializeField] private bool fromBellow;
    [SerializeField] private Direction spawnDirection;
    [SerializeField] private float jumpForce;

    void Start()
    {
        if (SceneTransitionManager.Instance != null &&
            SceneTransitionManager.Instance.GetSpawnID() == spawnID)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;

                if (fromBellow)
                {
                    PlayerController playerController = player.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.SimulateJump(jumpForce, false, false);
                        playerController.SimulateHorizontalMovement(spawnDirection, 2f, true, false);
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
