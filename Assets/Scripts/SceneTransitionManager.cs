using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    private int spawnID = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre les scènes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSpawnID(int id)
    {
        spawnID = id;
    }

    public int GetSpawnID()
    {
        return spawnID;
    }
}

