using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float maxSpeed = 8f;
    private float moveInput;

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


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Lecture des inputs
        moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            isJumping = true;
            coyoteTimeCounter = 0f; 
        }
    }

    void FixedUpdate()
    {
        // Gérer les mouvements du joueur
        HandleMovements();

        // Gérer le coyote time
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.fixedDeltaTime;
    }


    private void HandleMovements()
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

        // Appliquer les mouvements au Rigidbody
        rb.velocity = new Vector2(xMovement, yMovement);
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
