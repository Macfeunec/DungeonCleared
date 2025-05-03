using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private int spawnID;

    void Start()
    {
        if (SceneTransitionManager.Instance != null &&
            SceneTransitionManager.Instance.GetSpawnID() == spawnID)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;
            }
        }
    }
}
