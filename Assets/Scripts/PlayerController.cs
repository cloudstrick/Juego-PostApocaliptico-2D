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

    // NUEVA VARIABLE: Para saber si el jugador pasó a mejor vida
    private bool isDead = false;

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
        if (isDead) return; // Si está muerto, detiene los contadores
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isDead) return; // NUEVO: Si está muerto, no calcula físicas ni se mueve

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

    public void TakeDamage(float damage)
    {
        // Si ya está muerto o es invencible por el cooldown, ignora el daño
        if (isDead || cooldownTimer > 0 || currentHealth <= 0) return;

        currentHealth -= damage;
        cooldownTimer = hurtCooldown;

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealth(currentHealth);
        }

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt");
        }
        else
        {
            Die(); // ¡Hora de activar la secuencia de derrota!
        }
    }

    // --- Función que se ejecuta cuando la vida llega a 0 ---
    void Die()
    {
        // 1. Activar animación de muerte en el Animator
        anim.SetTrigger("die"); // O como hayas nombrado tu trigger

        // --- SOLUCIÓN PARA QUE EL ESCORPIÓN NO ATAQUE MAS ---

        // 2. Desactivar el tag para que los escaneos de área no lo encuentren
        transform.tag = "Untagged";

        // 3. Desactivar el BoxCollider2D físico principal
        GetComponent<BoxCollider2D>().enabled = false;

        // 4. Detener el Rigidbody y hacerlo Static para que no se deslice
        rb.linearVelocity = Vector2.zero; // Si usas Unity 2022 o anterior, es rb.velocity
        rb.bodyType = RigidbodyType2D.Static;

        // Opcional: Desactivar cualquier Canvas de vida que flote sobre él
        // GetComponentInChildren<CanvasVidaPlayer>().gameObject.SetActive(false); 

        Debug.Log("El leñador ha sido derrotado.");
    }

    // --- ENTRADAS (INPUT SYSTEM) ---
    // Agregamos candados de 'isDead' a las entradas para que no se pueda controlar el cadáver

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
        }
    }

    public void ExecuteDamage()
    {
        if (isDead) return; // Si murió antes de que cayera el hacha, no hace daño

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