using UnityEngine;

public class Health : MonoBehaviour
{
    // Attributs
    [Header("Santé")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    private bool isDead = false;

    [Header("Personnage")]
    [SerializeField] private bool isPlayer;
    [SerializeField] private bool isInvincible;
    private Rigidbody2D rb;
    private Animator animator;
    

    [Header("Effets")]
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private GameObject deathEffect;

    // Méthode d'initialisation
    // Définit la santé actuelle à la santé maximale
    void Start()
    {
        if (isPlayer)
        {
            currentHealth = PlayerManager.Instance.playerHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Vérifie si la santé actuelle est inférieure ou égale à zéro
        if (currentHealth <= 0 && !isInvincible)
        {
            if (!isDead) 
            {
                isDead = true;
                Die();
            }
            
        }
    }

    // Méthode pour recevoir des dégâts
    // Réduit la santé actuelle en fonction des dégâts reçus
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
    }

    // Méthode pour soigner
    // Augmente la santé actuelle en fonction de la quantité de soin
    public void Heal(float amount)
    {
        currentHealth += amount;
    }

    private void Die()
    {
        Debug.Log(this.gameObject.name + " died!");
        if (isPlayer)
        {
            if (TryGetComponent<PlayerController>(out var player))
            {
                player.Die();
                animator = player.GetComponent<Animator>();
            }
        }
        else
        {
            if (TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.Die();
                animator = enemy.GetComponentInChildren<Animator>();
            }
        }

        if (animator != null) animator.SetBool("isDying", true);

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
    }



    // Méthode pour récupérer la santé actuelle
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentHealth(float health)
    {
        currentHealth = health;
    }
}