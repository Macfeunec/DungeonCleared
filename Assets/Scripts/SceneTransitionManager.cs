using UnityEditor.SearchService;
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

    void Start()
    {
        Debug.Log("Player spawned");
        if (spawnID == 0)
        {
            Debug.Log("Starting fade in at spawn");
            SceneFader.Instance.FadeInAtSpawn(); // Appel de la méthode FadeInAtSpawn si spawnID est 0
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

