using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Detección Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Configuración de Combate")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.41f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackCooldown = 0.5f; // Tiempo de espera entre ataques
    private float attackCooldownTimer;
    private bool isAttacking = false; // NUEVO: Interruptor de seguridad de ataque

    [Header("Estadísticas de Vida (HP)")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float hurtCooldown = 1f;
    private float cooldownTimer;

    // static para mantener datos entre niveles
    public static float currentHealth = 100f;
    public static int currentLives = 1;

    [Header("Sistema de Vidas (Oportunidades)")]
    [SerializeField] private int maxLives = 3;
    private bool isDead = false;

    public bool IsDead => isDead;

    [Header("Configuración de Audio")]
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioClip swingAirSound; // Sonido del hachazo al aire
    [SerializeField] private AudioClip hitEnemySound;  // Sonido del hachazo impactando al enemigo

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private bool isGrounded;
    private Vector3 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        startPosition = transform.position;

        // SEGURIDAD: Si por error del Inspector el cooldown quedó en 0, lo forzamos a 0.5s
        if (attackCooldown <= 0f)
        {
            attackCooldown = 0.5f;
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            currentHealth = maxHealth;
            currentLives = 1;
        }

        UpdateUI();
    }

    void Update()
    {
        if (isDead) return;

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                isAttacking = false; // Apagamos el interruptor al terminar el cooldown por seguridad
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
        anim.SetBool("Grounded", isGrounded);

        if (moveInput.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    public void TakeDamage(float damage)
    {
        if (isDead || cooldownTimer > 0) return;

        currentHealth -= damage;
        cooldownTimer = hurtCooldown;

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt");
            UpdateUI();
        }
        else
        {
            LoseLife();
        }
    }

    private void LoseLife()
    {
        currentLives--;

        if (currentLives > 0)
        {
            currentHealth = maxHealth;
            UpdateUI();
            anim.SetTrigger("Hurt");
        }
        else
        {
            currentHealth = 0;
            UpdateUI();
            ActualDie();
        }
    }

    public void FallIntoAbyss()
    {
        if (isDead) return;

        currentLives--;

        if (currentLives > 0)
        {
            currentHealth = maxHealth;
            transform.position = startPosition;
            rb.linearVelocity = Vector2.zero;

            UpdateUI();
            anim.SetTrigger("Hurt");
        }
        else
        {
            currentHealth = 0;
            UpdateUI();
            ActualDie();
        }
    }

    void ActualDie()
    {
        isDead = true;
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowGameOverScreen();
        }

        Debug.Log("GAME OVER: Te quedaste sin vidas.");
    }

    public bool AddLife()
    {
        if (currentLives >= maxLives || isDead) return false;

        currentLives++;
        UpdateUI();
        return true;
    }

    public bool Heal(float amount)
    {
        if (currentHealth >= maxHealth || isDead) return false;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHUD(currentHealth, currentLives);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isDead) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isDead) return;
        if (context.started && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (isDead) return;

        // Solo permite un nuevo ataque si el temporizador terminó y no estamos atacando activamente
        if (context.started && attackCooldownTimer <= 0 && !isAttacking)
        {
            isAttacking = true; // Encendemos el interruptor de ataque
            anim.SetTrigger("Attack");
            attackCooldownTimer = attackCooldown; // Iniciamos el cooldown

            if (playerAudioSource != null && swingAirSound != null)
            {
                playerAudioSource.PlayOneShot(swingAirSound);
            }
        }
    }

    public void ExecuteDamage()
    {
        if (isDead) return;

        // INTERRUPTOR DE SEGURIDAD: Si el ataque ya se consumió en este swing, ignoramos el daño extra
        if (!isAttacking) return;

        isAttacking = false; // Consumimos el ataque inmediatamente para este swing

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        bool golpeoAAlguien = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            // 1. Detectar si es un Escorpión común
            ScorpioAI scorpio = enemy.GetComponent<ScorpioAI>();
            if (scorpio != null)
            {
                scorpio.TakeDamage(attackDamage, transform.position);
                golpeoAAlguien = true;
            }

            // 2. Detectar si es el Boss Centipede
            CentipedeAI centipede = enemy.GetComponent<CentipedeAI>();
            if (centipede != null)
            {
                centipede.TakeDamage(attackDamage);
                golpeoAAlguien = true;
            }
        }

        if (golpeoAAlguien && playerAudioSource != null && hitEnemySound != null)
        {
            playerAudioSource.PlayOneShot(hitEnemySound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}