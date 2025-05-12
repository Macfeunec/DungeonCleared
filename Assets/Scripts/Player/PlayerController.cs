using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour, IMovable
{
    [Header("Mouvement")]
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float maxSpeed = 8f;
    private float moveInput;
    [SerializeField] private bool isMovingEnabled;
    [SerializeField] private bool isStopped = false;

    [Header("Attaque")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackCooldown = 1f;
    private bool isAttacking = false;
    private bool isAttackCooldownOver = true;

    [Header("Saut")]
    [SerializeField] private float jumpForce = 21f;
    [SerializeField] private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;
    private bool isJumping;
    
    [Header("Gravity Control")]
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private float lowJumpMultiplier = 2f;    
    
    [Header("Sol")]
    [SerializeField] private LayerMask groundLayer;
    public bool isGrounded;

    [Header("Références")]
    private Rigidbody2D rb;
    private Animator animator;

    private bool isStunned = false;
    public bool IsStunned => isStunned;

    public void SetStunned(bool state)
    {
        isStunned = state;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        isMovingEnabled = false; 
        isAttackCooldownOver = true;
    }

    void Update()
    {
        // Lecture des inputs
        if (isMovingEnabled && !isStunned)
        {
            // Vérifie si le joueur a appuyé sur le bouton d'attaque
            if (Input.GetButtonDown("Attack") && isAttackCooldownOver)
            {
                if (isAttacking)
                {
                    // Si le joueur attaque déjà, on arrête l'attaque
                    animator.SetTrigger("RestartAttack");
                }
                isAttacking = true;
                StartCoroutine(WaitForAttackCooldown());
                // DisableMovement();
            }

            // Vérifie si le joueur appuie sur les touches de mouvement
            moveInput = Input.GetAxisRaw("Horizontal");

            // Vérifie si le joueur a appuyé sur le bouton de saut
            if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
            {
                isJumping = true;
                coyoteTimeCounter = 0f; 
            }
        }

        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isStunned", isStunned);
    }

    void FixedUpdate()
    {
        // Gérer les mouvements du joueur et l'attaque
        HandleAttack();
        HandleMovements();

        // Gérer le coyote time
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    // Méthode publique pour activer le mouvement
    public void EnableMovement()
    {
        isMovingEnabled = true;
    }

    // Méthode publique pour désactiver le mouvement
    public void DisableMovement()
    {
        isMovingEnabled = false;
    }

    // Méthode privée pour gérer les mouvements du joueur
    private void HandleMovements()
    {
        // Appliquer les mouvements au Rigidbody
        if (isMovingEnabled && !isStunned && !isStopped)
        {
            float xMovement = rb.velocity.x; // Conserver la vitesse horizontale actuelle
            float yMovement = rb.velocity.y; // Conserver la vitesse verticale actuelle

            // Appliquer le mouvement horizontal
            float targetSpeed = moveInput * maxSpeed;
            float speedDiff = targetSpeed - xMovement;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = speedDiff * accelRate;
            xMovement += movement * Time.fixedDeltaTime;

            // Appliquer le mouvement horizontal
            if (isJumping)
            {
                yMovement = jumpForce;
                isJumping = false;
            }
            // Augmenter la vitesse de chute si le joueur tombe
            if (yMovement < 0)
            {
                // Le joueur tombe -> on augmente la gravité pour qu’il tombe plus vite
                yMovement += Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (yMovement > 0 && !Input.GetButton("Jump"))
            {
                // Le joueur monte MAIS a relâché le bouton -> saut plus court
                yMovement += Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
            // Limiter la vélocité verticale pour éviter une chute trop rapide
            if (yMovement < -20f)
            {
                yMovement = -20f;
            }

            rb.velocity = new Vector2(xMovement, yMovement);
            if (Mathf.Abs(xMovement) > 0.1f) transform.localScale = new Vector3(Mathf.Sign(xMovement), 1, 1); // Retourner le sprite selon la direction
        }
        else if (isStunned || !isMovingEnabled)
        {
            // Si le joueur est étourdi, on ne modifie pas la vélocité horizontale
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
        else if (isStopped)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    // Méthode privée pour gérer l'attaque
    private void HandleAttack()
    {
        if (!isStunned)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("PlayerAttack") && info.normalizedTime >= 1f)
            {
                isAttacking = false;
            }
        }
        else 
        {
            isAttacking = false; 
        }
    }

    // Coroutine pour gérer le cooldown
    private IEnumerator WaitForAttackCooldown()
    {
        isAttackCooldownOver = false;
        yield return new WaitForSeconds(attackCooldown);
        isAttackCooldownOver = true;
    }


    // Méthode publique pour simuler un saut
    public void SimulateJump(float jumpForce, bool untilFloorTouched, bool forever)
    {
        StartCoroutine(SimulateJumpCoroutine(jumpForce, untilFloorTouched, forever));
    }

    private IEnumerator SimulateJumpCoroutine(float jumpForce, bool untilFloorTouched, bool forever)
    {
        if (rb == null) yield break;

        // Une fois au sol (ou si not untilFloorTouched), applique une fois le saut
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        // Si untilFloorTouched est activé, continue le saut jusqu'à toucher le sol
        while ((untilFloorTouched && !isGrounded) || forever)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            yield return null; // Attend une frame avant de continuer
        }
    }

    // Méthode publique pour simuler un mouvement horizontal
    public void SimulateHorizontalMovement(Direction direction, float multiplier, bool untilFloorTouched, bool forever)
    {
        StartCoroutine(SimulateHorizontalMovementCoroutine(direction, multiplier, untilFloorTouched, forever));
    }

    private IEnumerator SimulateHorizontalMovementCoroutine(Direction direction, float multiplier, bool untilFloorTouched, bool forever)
    {
        if (rb == null) yield break;

        float xMovement = (direction == Direction.Left) ? -maxSpeed * multiplier :
                        (direction == Direction.Right) ? maxSpeed * multiplier : 0f;

        // Une fois au sol (ou si not untilFloorTouched), applique une fois le mouvement
        rb.velocity = new Vector2(xMovement, rb.velocity.y);

        // Si untilFloorTouched est activé, continue le mouvement jusqu'à toucher le sol
        while ((untilFloorTouched && !isGrounded) || forever)
        {
            rb.velocity = new Vector2(xMovement, rb.velocity.y);
            yield return null; // Attend une frame avant de continuer
        }
    }

    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si le joueur touche le sol
        if (IsInLayerMask(other.gameObject, groundLayer))
        {
            isGrounded = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Vérifie si le joueur quitte le sol
        if (IsInLayerMask(other.gameObject, groundLayer))
        {
            isGrounded = false;
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }
}
