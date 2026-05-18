using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Detección Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Configuración de Combate")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Estadísticas de Vida (HP)")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private float hurtCooldown = 1f;
    private float cooldownTimer;

    [Header("Sistema de Vidas (Oportunidades)")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;
    private bool isDead = false;

    // PROPIEDAD PÚBLICA: Permite a otros scripts (como el Escorpión) saber si el jugador murió
    public bool IsDead => isDead;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentLives = 1;

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
            // SOLUCIÓN PUNTO 2: Cada vida equivale a 100 de HP. Se llena automáticamente aquí
            currentHealth = maxHealth;
            UpdateUI(); // Actualizamos el HUD con los nuevos 100 HP de forma limpia
            anim.SetTrigger("Hurt");
        }
        else
        {
            currentHealth = 0; // Aseguramos que el slider quede en 0
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
        if (context.started) anim.SetTrigger("Attack");
    }

    public void ExecuteDamage()
    {
        if (isDead) return;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            ScorpioAI scorpio = enemy.GetComponent<ScorpioAI>();
            if (scorpio != null)
            {
                scorpio.TakeDamage(attackDamage, transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}