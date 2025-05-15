using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    [SerializeField] private int spawnID = 0;

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

    void Start()
    {
        if (spawnID == 0)
        {
            SceneFader.Instance.FadeInAtSpawn(); // Appel de la méthode FadeInAtSpawn si spawnID est 0 (démarrage ou resp)
        }
    }

    // Méthode publique pour définir l'ID de spawn
    public void SetSpawnID(int id)
    {
        spawnID = id;
    }

    // Méthode publique pour obtenir l'ID de spawn
    public int GetSpawnID()
    {
        return spawnID;
    }
}

