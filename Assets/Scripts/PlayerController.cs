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
    [SerializeField] private Transform attackPoint; // Objeto vacío frente al player
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayers; // Selecciona la capa "Enemies"

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Movimiento horizontal
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // Actualizar Animator
        anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
        anim.SetBool("Grounded", isGrounded);

        // Giro de Sprite
        if (moveInput.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < 0) transform.localScale = new Vector3(-1, 1, 1);
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
        // Solo atacamos cuando se presiona el botón (no al soltarlo)
        if (context.started)
        {
            PerformAttack();
        }
    }

    // --- LÓGICA DE ATAQUE ---

    private void PerformAttack()
    {
        // 1. Activar animación de ataque
        anim.SetTrigger("Attack");

        // 2. Detectar enemigos en el rango
        // Usamos OverlapCircleAll para encontrar todo en el área de impacto
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // 3. Aplicar daño
        foreach (Collider2D enemy in hitEnemies)
        {
            ScorpioAI scorpio = enemy.GetComponent<ScorpioAI>();
            if (scorpio != null)
            {
                // Pasamos '1' de daño y nuestra posición actual
                scorpio.TakeDamage(1, transform.position);
            }
        }
    }

    // Dibujar el rango de ataque en el editor para facilitar el ajuste
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}