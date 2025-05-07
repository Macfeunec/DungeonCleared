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


    // This method is called to fade to a new scene
    public void FadeToScene(string sceneName)
    {
        if (isFading) return; 
        StartCoroutine(FadeAndSwitchScenes(sceneName));
    }

    public void FadeInAtSpawn()
    {
        if (isFading) return; 
        StartCoroutine(FadeIn());
    }

    // This coroutine handles the fading effect and scene switching
    private IEnumerator FadeAndSwitchScenes(string sceneName)
    {
        isFading = true;

        yield return StartCoroutine(FadeOut(sceneName));
        yield return StartCoroutine(FadeIn());

        isFading = false;
    }

    // This coroutine fades the screen out and loads the new scene
    private IEnumerator FadeOut(string sceneName)
    {
        yield return StartCoroutine(Fade(1));

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

        yield return null;
    }

    // This coroutine fades the screen in and enables player movement
    private IEnumerator FadeIn()
    {
        CameraFollow cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
        {
            cam.DisableLinger();
            cam.ForceSnapToTarget();
            yield return new WaitForSeconds(0.1f);
            cam.EnableLinger();
        }

        yield return StartCoroutine(Fade(0)); 

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.EnableMovement();
        }
    }

    // This coroutine handles the actual fading effect
    private IEnumerator Fade(float targetAlpha)
    {
        fadeCanvasGroup.alpha = 1 - targetAlpha;
        float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / fadeDuration;

        while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
