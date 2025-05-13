using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyClass
{
    Sword,
    Hammer,
    Cape
}

public class EnemyController : MonoBehaviour, IMovable
{
    [Header("Paramètres")]
    [SerializeField] private EnemyClass enemyClass = EnemyClass.Sword;
    [SerializeField] private GameObject swordBody;
    [SerializeField] private GameObject capeBody;
    [SerializeField] private GameObject hammerBody;
    [SerializeField] private bool isIdleEnemy;
    [SerializeField] private bool isJumpingEnemy;
    [SerializeField] private bool isRunningEnemy;
    [SerializeField] private bool isIdle = false; // RETIRER SERIALIZEFIELD APRES TEST
    private Animator animator;

    [Header("Cibles")]
    [SerializeField] private GameObject[] targets;
    [SerializeField] private float detectionRadius;
    [SerializeField] private float triggerdDetectionRadius;
    [SerializeField] private float attackRadius = 3f; 
    [SerializeField] private bool backEyes; // Si l'ennemi peut regarder derrière lui
    [SerializeField] private float detectionCooldown = 0.5f;
    private float currentDetectionRadius;
    [SerializeField] private bool isTargetInSight = false; // RETIRER SERIALIZEFIELD APRES TEST
    private Transform currentTarget;
    [SerializeField] private bool isPatroling = false; // RETIRER SERIALIZEFIELD APRES TEST

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
    [SerializeField] private bool isAttacking = false;
    private bool canAttack = true;

    private bool isStunned = false;
    public bool IsStunned => isStunned;

    public void SetStunned(bool state)
    {
        isStunned = state;
    }

    void Awake()
    {
        SetClassVisual();
    }
  
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

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

    void Update()
    {
        animator.SetBool("isGrounded", isGrounded);
        if (isRunningEnemy)
        {
            animator.SetBool("isPatroling", isPatroling);
            animator.SetBool("isChasing", isTargetInSight);
        }
        else
        {
            animator.SetBool("isPatroling", isPatroling || isTargetInSight);
        }
        animator.SetBool("isIdle", isIdle);
        switch (enemyClass)
        {
            case EnemyClass.Sword:
                animator.SetBool("isSlashing", isAttacking);
                break;
            
            case EnemyClass.Hammer:
                animator.SetBool("isHammering", isAttacking);
                break;

            case EnemyClass.Cape:
                animator.SetBool("isDashing", isAttacking);
                break;
        }
    }

    void FixedUpdate()
    {
        // Gérer le mouvement de l'ennemi
        if (!isStunned && !isAttacking) GroundMovement();

        HandleAttack();
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
            backEyes = false;
            currentDetectionRadius = detectionRadius;

            if (isIdleEnemy)
            {
                // Si l'ennemi est inactif, il ne fait rien
                rb.velocity = new Vector2(0f, rb.velocity.y);

                isIdle = true;
            }
            else
            {
                // Si la cible n'est pas dans le champ de vision, l'ennemi patrouille
                GroundPatrol();

                isPatroling = true;
                isIdle = false;
            }
            
        }
        else
        {   
            // Si la cible est dans le champ de vision, l'ennemi la poursuit
            currentDetectionRadius = triggerdDetectionRadius;
            backEyes = true;
            ChaseTarget();

            isPatroling = false;
            isIdle = false;
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
        float speedMultiplier = isRunningEnemy ? 1f : 0.5f;
        rb.velocity = new Vector2(transform.localScale.x * patrolSpeed * speedMultiplier, rb.velocity.y);      
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
        float xDiff = currentTarget.position.x - transform.position.x;
        float direction = Mathf.Sign(xDiff);
        
        if (distance < attackRadius)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);
            Attack();
            return;
        }


        float moveSpeed = isRunningEnemy ? chaseSpeed : chaseSpeed * 0.5f;

        if (Mathf.Abs(xDiff) > 1f)
        {
            transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        }

        float heightDiff = currentTarget.position.y - transform.position.y;

        bool isObstacleDetected = obstacleDetected();
        if ((isGrounded && heightDiff > 2f) || (isGrounded && isObstacleDetected))
        {
            if (isJumpingEnemy) Jump();
        }
    }


    private void HandleAttack()
    {
        if (!isStunned)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

            switch (enemyClass)
            {
                case EnemyClass.Sword:
                    if (info.IsName("KnightAttackSword") && info.normalizedTime >= 1f)
                    {
                        isAttacking = false;
                    }
                    break;
                                
                case EnemyClass.Hammer:
                    // Animation d'attaque
                    break;

                case EnemyClass.Cape:
                    if (info.IsName("KnightDash") && info.normalizedTime >= 1f)
                    {
                        isAttacking = false;
                    }
                    break;
            }
        }
        else 
        {
            isAttacking = false; 
        }
    }

    private void Attack()
    {
        // Attaque la cible et attends un cooldown avant de pouvoir attaquer à nouveau
        if (!canAttack) return;

        isAttacking = true;
        canAttack = false;

        switch (enemyClass)
        {
            case EnemyClass.Sword:
                StartCoroutine(SlashAttack());
                break;
                            
            case EnemyClass.Hammer:
                // Animation d'attaque
                break;

            case EnemyClass.Cape:
                // Animation d'attaque
                break;
        }
    }

    private IEnumerator SlashAttack()
    {
        yield return new WaitForSeconds(0.7f); // Attendre la fin de l'animation d'attaque

        Vector2 origin = new Vector2(transform.position.x + transform.localScale.x * 0.5f, transform.position.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRadius);

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
        canAttack = true;
    }

    private IEnumerator WaitForJumpCooldown()
    {
        yield return new WaitForSeconds(jumpCoolDown);
        canJump = true;
    }

    // Méthode pour détecter les cibles
    private void DetectTargets()
    {
        isTargetInSight = false;
        currentTarget = null;

        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position;

        Vector2 size;
        Vector2 boxCenter;

        if (backEyes)
        {
            // Détection rectangulaire autour de l'ennemi (vision à 360°)
            size = new Vector2(currentDetectionRadius * 2f, currentDetectionRadius * 0.8f);
            boxCenter = origin;
        }
        else
        {
            // Détection rectangulaire devant l'ennemi
            size = new Vector2(currentDetectionRadius, currentDetectionRadius * 0.8f);
            boxCenter = origin + direction * (currentDetectionRadius / 2f);
        }

        // Utiliser OverlapBoxAll pour détecter les cibles dans un rectangle
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, size, 0f);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            foreach (var target in targets)
            {
                if (hit.gameObject == target)
                {
                    isTargetInSight = true;
                    currentTarget = target.transform;
                    return;
                }
            }
        }
    }


    private void SetClassVisual()
    {
        swordBody.SetActive(false);
        capeBody.SetActive(false);
        hammerBody.SetActive(false);

        switch (enemyClass)
        {
            case EnemyClass.Sword:
                swordBody.SetActive(true);
                animator = swordBody.GetComponent<Animator>();
                break;
            case EnemyClass.Cape:
                capeBody.SetActive(true);
                animator = capeBody.GetComponent<Animator>();
                break;
            case EnemyClass.Hammer:
                hammerBody.SetActive(true);
                animator = hammerBody.GetComponent<Animator>();
                break;
        }
    }


    // Méthode pour afficher les Gizmos dans l'éditeur
    // Cela permet de visualiser la portée de détection de l'ennemi
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position;

        Vector2 size;
        Vector2 center;

        if (backEyes)
        {
            size = new Vector2(currentDetectionRadius * 2f, currentDetectionRadius * 0.8f);
            center = origin;
        }
        else
        {
            size = new Vector2(currentDetectionRadius, currentDetectionRadius * 0.8f);
            center = origin + direction * (currentDetectionRadius / 2f);
        }

        Gizmos.DrawWireCube(center, size);
    }

}
