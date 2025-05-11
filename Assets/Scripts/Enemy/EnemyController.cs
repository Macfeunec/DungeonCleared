using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IMovable
{
    [Header("Paramètres")]
    [SerializeField] private bool isMeleeEnemy;

    [Header("Cibles")]
    [SerializeField] private GameObject[] targets;
    [SerializeField] private float detectionRadius;
    [SerializeField] private float triggerdDetectionRadius;
    [SerializeField] private float attackRadius = 3f; 
    [SerializeField] private bool backEyes; // Si l'ennemi peut regarder derrière lui
    [SerializeField] private float detectionCooldown = 0.5f;
    private float currentDetectionRadius;
    private bool isTargetInSight = false;
    private Transform currentTarget;

    [Header("Mouvement")]
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCoolDown = 1f;
    private bool canJump = true;
    private Rigidbody2D rb;
    private LayerMask collisionLayer;
    private bool isGrounded;

    [Header("Ennemi")]
    [SerializeField] private float damage;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float knockBackDuration = 0.5f;
    private bool isAttacking = false;

    private bool isStunned = false;
    public bool IsStunned => isStunned;

    public void SetStunned(bool state)
    {
        isStunned = state;
    }
  
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collisionLayer = LayerMask.GetMask("Collision");
        StartCoroutine(DetectionRoutine());
    }

    private IEnumerator DetectionRoutine()
    {
        while (true)
        {
            DetectTargets();
            yield return new WaitForSeconds(detectionCooldown);
        }
    }

    void FixedUpdate()
    {
        // Gérer le mouvement de l'ennemi
        if (!isStunned && !isAttacking) GroundMovement();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si le joueur touche le sol
        if (IsInLayerMask(other.gameObject, collisionLayer))
        {
            isGrounded = true;
            Debug.Log("Sol détecté !");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Vérifie si le joueur quitte le sol
        if (IsInLayerMask(other.gameObject, collisionLayer))
        {
            isGrounded = false;
            Debug.Log("Sol quitté !");
        }
    }

    // Vérifie si l'objet est dans le LayerMask
    // Cette méthode n'est pas de moi, mais elle fonctionne très bien
    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0; 
    }

    private void GroundMovement()
    {
        // Vérifier si la cible est dans le champ de vision
        if (!isTargetInSight) 
        {
            // Si la cible n'est pas dans le champ de vision, l'ennemi patrouille
            currentDetectionRadius = detectionRadius;
            backEyes = false;
            GroundPatrol();
        }
        else
        {   
            // Si la cible est dans le champ de vision, l'ennemi la poursuit
            currentDetectionRadius = triggerdDetectionRadius;
            backEyes = true;
            ChaseTarget();
        }
    }

    // Méthode pour gérer le mouvement de patrouille de l'ennemi
    // L'ennemi se déplace dans une direction jusqu'à ce qu'il détecte un obstacle
    private void GroundPatrol()
    {
        bool isObstacleDetected = obstacleDetected();
        if (isObstacleDetected)
        {
            // Inverser la direction de l'ennemi
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        // Déplacer l'ennemi
        rb.velocity = new Vector2(transform.localScale.x * patrolSpeed, rb.velocity.y);      
    }

    // Méthode pour détecter les obstacles
    private bool obstacleDetected()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 origin = new Vector2(transform.position.x + direction*0.5f, transform.position.y - 0.5f);
        Vector2 rotation = new Vector2(direction, -1f);
        float distance = 1f;

        // Vérifier si l'ennemi est au bord d'une plateforme
        RaycastHit2D hit = Physics2D.Raycast(origin, rotation, distance, collisionLayer);

        if (hit.collider == null)
        {
            Debug.Log("Bord détecté !");
            return true;
        }

        // Vérifier si l'ennemi est en face d'un mur
        Vector2 inFront = new Vector2(direction, 0f);
        RaycastHit2D hitWall = Physics2D.Raycast(origin, inFront, distance, collisionLayer);

        if (hitWall.collider != null)
        {
            Debug.Log("Mur détecté !");
            return true;
        }

        return false;
    }

    // Méthode pour gérer le mouvement de l'ennemi lorsqu'il poursuit une cible
    // L'ennemi se déplace vers la cible et l'attaque si elle est à portée
    private void ChaseTarget()
    {
        if (currentTarget == null) return;

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance < attackRadius)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            HandleAttack();
            return;
        }

        float moveSpeed = chaseSpeed;
        float xDiff = currentTarget.position.x - transform.position.x;
        float direction = Mathf.Sign(xDiff);

        if (Mathf.Abs(xDiff) > 0.2f)
        {
            transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        }

        float heightDiff = currentTarget.position.y - transform.position.y;

        bool isObstacleDetected = obstacleDetected();
        if ((isGrounded && heightDiff > 2f) || (isGrounded && isObstacleDetected))
        {
            Jump();
        }
    }

    // Méthode pour gérer l'attaque de l'ennemi
    // L'ennemi attaque la cible si elle est à portée
    private void HandleAttack()
    {
        // Attaque la cible et attends un cooldown avant de pouvoir attaquer à nouveau
        if (isMeleeEnemy)
        {
            if (isAttacking) return;
            isAttacking = true;

            // Animation d'attaque

            // Attaque de mêlée
            Collider2D[] hits = Physics2D.OverlapCircleAll(currentTarget.position, attackRadius);
            foreach (var hit in hits)
            {
                if (hit == null) continue;

                foreach (var target in targets)
                {
                    if (hit.gameObject == target)
                    {
                        // Appliquer des dégâts à la cible
                        hit.GetComponent<Health>().TakeDamage(damage);
                        hit.GetComponent<KnockBack>().TakeKnockBack(transform.localScale.x * damage * 8, knockBackDuration);
                    }
                }

                // Attendre le cooldown avant de pouvoir attaquer à nouveau
                StartCoroutine(WaitForAttackCooldown());
                return;
            }
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            if (!canJump) return;
            canJump = false;

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            // Attendre le cooldown avant de pouvoir sauter à nouveau
            StartCoroutine(WaitForJumpCooldown());
            return;
        }
    }

    private IEnumerator WaitForAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private IEnumerator WaitForJumpCooldown()
    {
        yield return new WaitForSeconds(jumpCoolDown);
        canJump = true;
    }

    // Méthode pour détecter les cibles
    // Cette méthode utilise un cercle de détection pour trouver les cibles
    private void DetectTargets()
    {
        isTargetInSight = false;
        currentTarget = null;
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left; // direction vers laquelle l'ennemi regarde
        Vector2 origin = transform.position;
        Collider2D[] hits;

        if (backEyes)
        {
            // Vision circulaire
            hits = Physics2D.OverlapCircleAll(origin, currentDetectionRadius);
        }
        else
        {
            // Vision frontale
            hits = Physics2D.OverlapCircleAll(origin + direction * currentDetectionRadius * 0.5f, currentDetectionRadius * 0.5f);
        }

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            foreach (var target in targets)
            {
                if (hit.gameObject == target)
                {
                    isTargetInSight = true;
                    currentTarget = target.transform;
                    return; // on s'arrête dès qu'une cible est trouvée
                }
            }
        }
    }

    // Méthode pour afficher les Gizmos dans l'éditeur
    // Cela permet de visualiser la portée de détection de l'ennemi
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        if (backEyes)
        {
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
        else
        {
            Vector3 center = transform.position + (Vector3)(direction * detectionRadius * 0.5f);
            Gizmos.DrawWireSphere(center, detectionRadius * 0.5f);
        }
    }



}
