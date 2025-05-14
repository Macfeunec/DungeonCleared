using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
public static class AutoPersistentSceneLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Ne rien faire si on est déjà dans le menu ou dans la PersistentScene
        if (currentScene == "Menu" || currentScene == "PersistentScene")
            return;

        // Charger PersistentScene en additif
        SceneManager.LoadSceneAsync("PersistentScene", LoadSceneMode.Additive);
    }
}*/

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private void Awake()
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

    public IEnumerator LoadSceneRoutine(string sceneName)
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // ⚠️ Ne pas recharger si la scène est déjà chargée
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.LogWarning($"Scene '{sceneName}' is already loaded. Skipping load.");
        }
        else
        {
            // Charger la nouvelle scène en additif
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone)
                yield return null;

            Scene newScene = SceneManager.GetSceneByName(sceneName);
            while (!newScene.isLoaded)
                yield return null;

            // Activer la nouvelle scène
            SceneManager.SetActiveScene(newScene);
        }

        // ⚠️ Décharger l’ancienne scène si elle est différente et non "PersistentScene"
        if (currentScene.name != "PersistentScene" &&
            currentScene.name != sceneName &&
            currentScene.IsValid() &&
            currentScene.isLoaded)
        {
            Debug.Log($"Unloading scene: {currentScene.name}");
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone)
                    yield return null;
            }
            else
            {
                Debug.LogWarning($"Could not unload scene: {currentScene.name}");
            }
        }
    }



    public void ReturnToMenu()
    {
        StartCoroutine(UnloadPersistentSceneAndLoadMenu());
    }

    public void LoadPersistentScene()
    {
        if (!SceneManager.GetSceneByName("PersistentScene").isLoaded)
            SceneManager.LoadSceneAsync("PersistentScene", LoadSceneMode.Additive);
    }

    private IEnumerator UnloadPersistentSceneAndLoadMenu()
    {
        // Décharger PersistentScene si elle est chargée
        if (SceneManager.GetSceneByName("PersistentScene").isLoaded)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("PersistentScene");
            while (!unloadOp.isDone)
                yield return null;
        }

        // Charger le Menu
        SceneManager.LoadScene("Menu");
    }
}
