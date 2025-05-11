using UnityEngine;

public class Health : MonoBehaviour
{
    // Attributs
    [SerializeField] private float maxHealth;
    private float currentHealth;

    // Méthode pour recevoir des dégâts
    // Réduit la santé actuelle en fonction des dégâts reçus
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    // Méthode pour soigner
    // Augmente la santé actuelle en fonction de la quantité de soin
    public void Heal(float amount)
    {
        currentHealth += amount;
    }

    // Méthode pour récupérer la santé actuelle
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}