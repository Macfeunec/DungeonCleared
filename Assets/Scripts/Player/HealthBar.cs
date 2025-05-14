using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
     private GameObject player;
    private PlayerController playerController;
    private Health playerHealth;

    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider healthLerpSlider;
    [SerializeField] private float lerpSpeed;
    private float maxHealth;
    private float currentHealth;

    [Header("Stamina Bar")]
    [SerializeField] private Slider staminaSlider;
    private float maxStamina;
    private float currentStamina;

    private void Start()
    {
        if (healthSlider == null)
        {
            Debug.LogError("HealthSlider is not assigned in the inspector.");
        }
        if (healthLerpSlider == null)
        {
            Debug.LogError("HealthLerpSlider is not assigned in the inspector.");
        }
        if (staminaSlider == null)
        {
            Debug.LogError("StaminaSlider is not assigned in the inspector.");
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerHealth = player.GetComponent<Health>();
        }
        else
        {
            Debug.LogError("Player not found in the scene.");
        }


        healthSlider.minValue = 0;
        healthLerpSlider.minValue = 0;
        staminaSlider.minValue = 0;
    }

    void Update()
    {
        if (player != null)
        {
            HandleHealthBar();
            HandleStaminaBar();
        }
        else
        {
            maxHealth = PlayerManager.Instance.playerMaxHealth;
            currentHealth = PlayerManager.Instance.playerHealth;
            maxStamina = 1000f;
            currentStamina = 1000f;

            healthSlider.maxValue = maxHealth;
            healthLerpSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            healthLerpSlider.value = currentHealth;

            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
            Debug.Log("Player not found in the scene. Health and Stamina bars will not update.");
        }
    }

    private void HandleHealthBar()
    {
        maxHealth = playerHealth.GetMaxHealth();

        healthSlider.maxValue = maxHealth;
        healthLerpSlider.maxValue = maxHealth;

        currentHealth = playerHealth.GetCurrentHealth();

        if (healthSlider.value != currentHealth)
        {
            healthSlider.value = currentHealth;
        }
        if (healthLerpSlider.value != currentHealth)
        {
            healthLerpSlider.value = Mathf.Lerp(healthLerpSlider.value, currentHealth, Time.deltaTime * lerpSpeed);
        }
    }

    private void HandleStaminaBar()
    {
        maxStamina = playerController.GetMaxStamina();

        staminaSlider.maxValue = maxStamina;

        currentStamina = playerController.GetStamina();

        if (staminaSlider.value != currentStamina)
        {
            staminaSlider.value = currentStamina;
        }
    }
}
