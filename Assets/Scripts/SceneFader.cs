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
        fadeCanvasGroup.alpha = 1;
    }


    // Méthode publique pour déclencher la transition de scène
    // Prend une chaîne de caractères pour le nom de la scène en paramètre
    public void FadeToScene(string sceneName)
    {
        if (isFading) return; 
        StartCoroutine(FadeAndSwitchScenes(sceneName));
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
        PlayerManager.Instance.SaveGame(); // Sauvegarde l'état du jeu avant de changer de scène

        isFading = true;

        yield return StartCoroutine(FadeOut(sceneName));    // Appelle la coroutine FadeOut qui gère le fondu à la sortie de la scène et la transition de scène
        yield return StartCoroutine(FadeIn());              // Appelle la coroutine FadeIn qui gère le fondu à l'entrée de la nouvelle scène

        isFading = false;
    }

    // Coroutine pour gérer le fondu à la sortie de la scène
    private IEnumerator FadeOut(string sceneName)
    {
        yield return StartCoroutine(Fade(1)); // Appelle la coroutine Fade qui gère le fondu (1 pour fade out)

        // Charge la nouvelle scène de manière asynchrone
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return null; // Attend une frame après le Start
    }

    // Coroutine pour gérer le fondu à l'entrée de la scène
    private IEnumerator FadeIn()
    {
        // Script de caméra pour gérer le mouvement de la caméra
        // Inutilisé car Cinémachine est utilisé pour le mouvement de la caméra
        /* 
        CameraFollow cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
        {
            cam.DisableLinger();                    
            cam.ForceSnapToTarget();
            yield return new WaitForSeconds(0.1f);
            cam.EnableLinger();
        }
        */

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
        fadeCanvasGroup.alpha = 1 - targetAlpha; // Initialise la valeur alpha selon la cible ()
        float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / fadeDuration;

        // Boucle jusqu'à ce que la valeur alpha atteigne la cible
        while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = targetAlpha; // Assure que la valeur alpha est exactement égale à la cible
    }
}
