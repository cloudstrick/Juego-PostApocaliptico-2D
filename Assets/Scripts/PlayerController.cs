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

    [Header("Estadísticas de Vida")]
    [SerializeField] private float maxHealth = 3f;
    private float currentHealth;
    [SerializeField] private float hurtCooldown = 1f;
    private float cooldownTimer;

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
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealth(currentHealth);
        }
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
        anim.SetBool("Grounded", isGrounded);

        if (moveInput.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    public void TakeDamage(float damage)
    {
        if (cooldownTimer > 0 || currentHealth <= 0) return;

        currentHealth -= damage;
        cooldownTimer = hurtCooldown;

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealth(currentHealth);
        }

        if (currentHealth > 0) anim.SetTrigger("Hurt");
        else Die();
    }

    void Die()
    {
        Debug.Log("El leñador ha sido derrotado.");
    }

    // --- ENTRADAS (INPUT SYSTEM) ---
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // PASO 1: Solo activamos la animación visual
            anim.SetTrigger("Attack");
        }
    }

    // PASO 2: Esta función será ejecutada por la propia animación en el frame exacto
    public void ExecuteDamage()
    {
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