using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private bool isIdleEnemy;
    [SerializeField] private bool isJumpingEnemy;
    [SerializeField] private bool isRunningEnemy;
    [SerializeField] private bool isFriendlyEnemy;
    private bool isIdle = false;
    private Animator animator;

    [Header("Cibles")]
    [SerializeField] private float detectionRadius;
    [SerializeField] private float triggerdDetectionRadius;
    [SerializeField] private float attackRadius = 3f; 
    private bool backEyes; // Si l'ennemi peut regarder derrière lui
    [SerializeField] private float detectionCooldown = 0.5f;
    private GameObject[] targets;
    private float currentDetectionRadius;
    private bool isTargetInSight = false; 
    private Transform currentTarget;
    private bool isPatroling = false; 

    [Header("Mouvement")]
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 30f;
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
    private bool canAttack = true;
    private bool isDead = false;

    private bool isStunned = false;
    public bool IsStunned => isStunned;

    public void SetStunned(bool state)
    {
        isStunned = state;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        SetClassVisual();
        DisableTrail();

        // S'abonner à l'événement de chargement de scène
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Se désabonner de l'événement pour éviter les erreurs
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        isTargetInSight = false;

        collisionLayer = LayerMask.GetMask("Collision");
        StartCoroutine(DetectionRoutine());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mettre à jour la référence au joueur après le chargement de la scène
        if (!isFriendlyEnemy)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targets = new GameObject[1];
                targets[0] = player;
            }
            else
            {
                Debug.LogWarning("Player not found in the scene!");
                targets = new GameObject[0]; // Initialiser avec une liste vide
            }
        }
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
    }

    void FixedUpdate()
    {
        if (!isDead)
        {
            // Gérer le mouvement de l'ennemi
            GroundMovement();
            HandleAttack();
        }
        else
        {
            if (isGrounded) rb.bodyType = RigidbodyType2D.Static;
        }
        
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
        if (isStunned)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
            currentDetectionRadius = triggerdDetectionRadius;
            backEyes = true;
            DetectTargets();
        }
        else if (isAttacking)
        {
            float xMovement = rb.velocity.x; // Conserver la vitesse horizontale actuelle
            float targetSpeed = 0f;
            float speedDiff = targetSpeed - xMovement;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = speedDiff * accelRate;
            xMovement += movement * Time.fixedDeltaTime;

            rb.velocity = new Vector2(xMovement, rb.velocity.y);
        }
        else
        {
            // Vérifier si la cible est dans le champ de vision
            if (!isTargetInSight) 
            {
                backEyes = false;
                currentDetectionRadius = detectionRadius;

                if (isIdleEnemy)
                {
                    // Si l'ennemi est inactif, il ne fait rien
                    float xMovement = rb.velocity.x; // Conserver la vitesse horizontale actuelle
                    float targetSpeed = 0f;
                    float speedDiff = targetSpeed - xMovement;
                    float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
                    float movement = speedDiff * accelRate;
                    xMovement += movement * Time.fixedDeltaTime;

                    rb.velocity = new Vector2(xMovement, rb.velocity.y);

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
        float xMovement = rb.velocity.x; // Conserver la vitesse horizontale actuelle
        float targetSpeed = transform.localScale.x * patrolSpeed * speedMultiplier;
        float speedDiff = targetSpeed - xMovement;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = speedDiff * accelRate;
        xMovement += movement * Time.fixedDeltaTime;

        rb.velocity = new Vector2(xMovement, rb.velocity.y);    
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

        if (Mathf.Abs(xDiff) > 1f)
        {
            float moveSpeed = isRunningEnemy ? chaseSpeed : chaseSpeed * 0.5f;

            transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);

            float xMovement = rb.velocity.x; // Conserver la vitesse horizontale actuelle
            float targetSpeed = direction * moveSpeed;
            float speedDiff = targetSpeed - xMovement;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = speedDiff * accelRate;
            xMovement += movement * Time.fixedDeltaTime;

            rb.velocity = new Vector2(xMovement, rb.velocity.y);    
            
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
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (info.IsName("KnightAttackSword") && info.normalizedTime >= 1f)
        {
            isAttacking = false;
            animator.SetBool("isSlashing", isAttacking);
        }

        /*
        switch (enemyClass)
        {
            case EnemyClass.Sword:
                if (info.IsName("KnightAttackSword") && info.normalizedTime >= 1f)
                {
                    isAttacking = false;
                    animator.SetBool("isSlashing", isAttacking);
                }
                break;
                            
            case EnemyClass.Hammer:
                animator.SetBool("isHammering", isAttacking);
                break;

            case EnemyClass.Cape:
                if (info.IsName("KnightDash") && info.normalizedTime >= 1f)
                {
                    isAttacking = false;
                    animator.SetBool("isDashing", isAttacking);
                }
                break;
        }*/
    }

    private void Attack()
    {
        // Attaque la cible et attends un cooldown avant de pouvoir attaquer à nouveau
        if (!canAttack) return;

        isAttacking = true;
        canAttack = false;

        animator.SetBool("isSlashing", isAttacking);
        StartCoroutine(SlashAttack()); // A MODIFIER

        /*
        switch (enemyClass)
        {
            case EnemyClass.Sword:
                animator.SetBool("isSlashing", isAttacking);
                StartCoroutine(SlashAttack());
                break;
                            
            case EnemyClass.Hammer:
                animator.SetBool("isHammering", isAttacking);
                break;

            case EnemyClass.Cape:
                animator.SetBool("isDashing", isAttacking);
                break;
        }*/
    }

    private IEnumerator SlashAttack()
    {
        yield return new WaitForSeconds(0.4f);
        EnableTrail();
        yield return new WaitForSeconds(0.3f); 

        if (isDead) yield break;

        Vector2 origin = new Vector2(transform.position.x + transform.localScale.x, transform.position.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRadius);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            foreach (var target in targets)
            {
                if (hit.gameObject == target)
                {
                    float direction = Mathf.Sign(hit.transform.position.x - transform.position.x);
                    // Appliquer des dégâts à la cible
                    hit.GetComponent<Health>().TakeDamage(damage);
                    hit.GetComponent<KnockBack>().TakeKnockBack(direction * damage, knockBackDuration);
                }
            }

            // Attendre le cooldown avant de pouvoir attaquer à nouveau
            StartCoroutine(WaitForAttackCooldown());            
        }

        yield return new WaitForSeconds(0.3f);
        DisableTrail();
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
        currentTarget = null;

        // Vérifier si la liste des cibles est initialisée
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("Targets list is not initialized or empty.");
            isTargetInSight = false;
            return;
        }

        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position;

        Vector2 size;
        Vector2 boxCenter;

        if (backEyes)
        {
            // Détection rectangulaire autour de l'ennemi (vision à 360°)
            size = new Vector2(currentDetectionRadius * 2f, currentDetectionRadius);
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

        isTargetInSight = false;
    }

    public void Die()
    {
        DisableTrail();
        isDead = true;
    }

    // Méthode pour afficher les effets de swing de l'épée
    public void EnableTrail()
    {
        trailRenderer.emitting = true;
    }

    public void DisableTrail()
    {
        trailRenderer.emitting = false;
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
                trailRenderer = swordBody.GetComponentInChildren<TrailRenderer>();
                break;
            case EnemyClass.Cape:
                capeBody.SetActive(true);
                animator = capeBody.GetComponent<Animator>();
                trailRenderer = capeBody.GetComponentInChildren<TrailRenderer>();
                break;
            case EnemyClass.Hammer:
                hammerBody.SetActive(true);
                animator = hammerBody.GetComponent<Animator>();
                trailRenderer = hammerBody.GetComponentInChildren<TrailRenderer>();
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
