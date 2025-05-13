using System.Collections;
using UnityEngine;

public class KnockBack : MonoBehaviour
{
    // Attributs
    [SerializeField] private float knockBackResistance = 10f;
    private Rigidbody2D rb;

    void Start()
    {
        // Initialisation du Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on this GameObject.");
        }
    }

    // Méthode pour recevoir un knockback
    // Applique une force de recul au Rigidbody
    public void TakeKnockBack(float knockBackForce, float knockBackDuration)
    {
        if (TryGetComponent<IMovable>(out var movable))
        {
            movable.SetStunned(true);
            StartCoroutine(RecoverFromKnockback(movable, knockBackDuration));
        }
        // Appliquer la force de recul au Rigidbody
        rb.velocity = new Vector2(knockBackForce / knockBackResistance, knockBackForce*2 / knockBackResistance);
    }

    IEnumerator RecoverFromKnockback(IMovable movable, float knockbackDuration)
    {
        yield return new WaitForSeconds(knockbackDuration / knockBackResistance);
        movable.SetStunned(false);
    }

    // Méthode pour mettre à jour la résistance au knockback
    public void SetKnockBackResistance(float newResistance)
    {
        knockBackResistance = newResistance;
    }
}