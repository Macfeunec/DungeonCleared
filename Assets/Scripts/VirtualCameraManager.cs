using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VirtualCameraManager : MonoBehaviour
{
    public static VirtualCameraManager Instance;

    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    public Cinemachine.CinemachineConfiner2D confiner;
    public GameObject confinerBounds;
    public GameObject player;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            SceneManager.sceneLoaded += OnSceneLoaded; // S'abonner à l'événement de chargement de scène
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(SetupCameraNextFrame());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Relancer la configuration de la caméra à chaque chargement de scène
        StartCoroutine(SetupCameraNextFrame());
    }

    private IEnumerator SetupCameraNextFrame()
    {
        yield return null;

        player = GameObject.FindGameObjectWithTag("Player");
        confinerBounds = GameObject.FindGameObjectWithTag("cameraBounds");

        virtualCamera = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        confiner = GetComponent<Cinemachine.CinemachineConfiner2D>();

        if (player != null) virtualCamera.Follow = player.transform;
        else Debug.LogError("Player not found");

        if (confinerBounds != null) confiner.m_BoundingShape2D = confinerBounds.GetComponent<PolygonCollider2D>();
        else Debug.LogError("Confiner bounds not found");
    }

    private void OnDestroy()
    {
        // Se désabonner de l'événement pour éviter les erreurs
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
