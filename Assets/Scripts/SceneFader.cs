using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    public static SceneFader Instance;
    private bool isFading = false;

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

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Menu") fadeCanvasGroup.alpha = 0;
        else fadeCanvasGroup.alpha = 1;
    }

    public void FadeToMenuAndDestroy()
    {
        if (isFading) return; 
        StartCoroutine(FadeToMenu());
        
    }

    // Coroutine pour gérer la transition de scène vers le menu
    private IEnumerator FadeToMenu()
    {
        yield return StartCoroutine(FadeOut("Menu"));
        ReturnToMenu(); // Détruit les objets persistants
        yield return StartCoroutine(Fade(0));
    }

    // Coroutine pour détruire les objets persistants
    private void ReturnToMenu()
    {
        // Détruit les objets persistants

    }


    // Méthode publique pour déclencher la transition de scène
    // Prend une chaîne de caractères pour le nom de la scène en paramètre
    public void FadeToScene(string sceneName)
    {
        if (isFading) return; 
        StartCoroutine(FadeAndSwitchScenes(sceneName));
    }

    public void RespawnPlayer()
    {
        if (isFading) return; 
        StartCoroutine(FadeAndRespawnPlayer(SceneManager.GetActiveScene().name));
    }

    // Méthode publique pour déclencher la transition de scène au spawn du joueur
    public void FadeInAtSpawn()
    {
        if (isFading) return; 
        StartCoroutine(FadeIn());
    }

    // Coroutine pour gérer la transition de scène
    // Prend une chaîne de caractères pour le nom de la scène en paramètre
    private IEnumerator FadeAndSwitchScenes(string sceneName)
    {
        // PlayerManager.Instance.SaveGame(); // Sauvegarde l'état du jeu avant de changer de scène
        yield return StartCoroutine(FadeOut(sceneName));    // Appelle la coroutine FadeOut qui gère le fondu à la sortie de la scène et la transition de scène
        yield return StartCoroutine(FadeIn());              // Appelle la coroutine FadeIn qui gère le fondu à l'entrée de la nouvelle scène
    }

    private IEnumerator FadeAndRespawnPlayer(string sceneName)
    {
        // Effectuer le fondu à la sortie
        yield return StartCoroutine(FadeOut(sceneName)); // Appelle la coroutine FadeOut

        // Respawn le joueur pendant que l'écran est noir
        PlayerManager.Instance.Respawn();

        // Effectuer le fondu à l'entrée
        yield return StartCoroutine(FadeIn()); // Appelle la coroutine FadeIn
    }

    // Coroutine pour gérer le fondu à la sortie de la scène
    private IEnumerator FadeOut(string sceneName)
    {
        

        yield return StartCoroutine(Fade(1)); // Fade to black

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Attendre que la scène soit presque prête (90% = "ready but not activated")
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f); // Attendre un petit délai supplémentaire si nécessaire

        
        asyncLoad.allowSceneActivation = true; // Activer la scène chargée
        while (!asyncLoad.isDone) // Attendre la fin du chargement complet
        {
            yield return null;
        }

        // Attendre une frame pour être sûr que tout est initialisé
        yield return null;

        

    }


    // Coroutine pour gérer le fondu à l'entrée de la scène
    private IEnumerator FadeIn()
    {
        

        yield return StartCoroutine(Fade(0)); // Appelle la coroutine Fade qui gère le fondu (0 pour fade in)

        // Réactive le mouvement du joueur après le fondu
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.EnableMovement();
        }

        
    }

    // Coroutine pour gérer le fondu
    // Prend un float pour la valeur alpha cible (0 pour fade in, 1 pour fade out)
    private IEnumerator Fade(float targetAlpha)
    {
        isFading = true; // Indique que le fondu est en cours

        fadeCanvasGroup.alpha = 1 - targetAlpha; // Initialise la valeur alpha selon la cible
        float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / fadeDuration;

        while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha; // Assure que la valeur alpha est exactement égale à la cible

        isFading = false; // Indique que le fondu est terminé
    }
}
