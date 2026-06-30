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

    [Header("Estadísticas de Vida (HP)")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float hurtCooldown = 1f;
    private float cooldownTimer;

    // MODIFICACIÓN CLAVE: Ahora son 'public static' para que viajen entre escenas sin borrarse
    public static float currentHealth = 100f;
    public static int currentLives = 1;

    [Header("Sistema de Vidas (Oportunidades)")]
    [SerializeField] private int maxLives = 3;
    private bool isDead = false;

    public bool IsDead => isDead;

    [Header("Configuración de Audio (NUEVO)")]
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

        // SOLUCIÓN: Si la escena actual es el nivel 1 (Índice 1 en Build Settings), vaciamos todo a los valores iniciales
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
        if (context.started)
        {
            anim.SetTrigger("Attack");

            // NUEVO: Reproducir sonido de hachazo al aire
            if (playerAudioSource != null && swingAirSound != null)
            {
                playerAudioSource.PlayOneShot(swingAirSound);
            }
        }
    }

    public void ExecuteDamage()
    {
        if (isDead) return;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        bool golpeoAAlguien = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            ScorpioAI scorpio = enemy.GetComponent<ScorpioAI>();
            if (scorpio != null)
            {
                scorpio.TakeDamage(attackDamage, transform.position);
                golpeoAAlguien = true; // El golpe fue exitoso contra un enemigo
            }
        }

        // NUEVO: Si golpeó con éxito, reproducir sonido de impacto
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