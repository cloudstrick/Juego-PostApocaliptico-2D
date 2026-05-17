using UnityEngine;

public class ScorpioAI : MonoBehaviour
{
    [Header("Configuración de Caza")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float detectionRange = 5f;

    [Header("Ajustes de Impacto")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;
    private float knockbackTimer;

    [Header("Estadísticas")]
    [SerializeField] private int health = 4;

    [Header("Configuración de Ataque")]
    [SerializeField] private float damageToPlayer = 1f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float detectPlayerRadius = 0.7f;
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform playerTransform;
    private bool isDead = false;

    // VARIABLE NUEVA: Para guardar tu escala de 1.5
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Guardamos la escala que pusiste en el Inspector (1.5, 1.5, 1)
        originalScale = transform.localScale;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isDead || playerTransform == null) return;

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            anim.SetFloat("speed", 0);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // CASO 1: Persecución activa
        if (distanceToPlayer <= detectionRange && distanceToPlayer > detectPlayerRadius)
        {
            float directionX = playerTransform.position.x - transform.position.x;

            rb.linearVelocity = new Vector2(directionX > 0 ? walkSpeed : -walkSpeed, rb.linearVelocity.y);
            anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));

            // CORRECCIÓN DE ESCALA: Ahora multiplicamos por tu tamaño original
            if (directionX > 0)
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            else if (directionX < 0)
                transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        }
        // CASO 2: Rango de ataque
        else if (distanceToPlayer <= detectPlayerRadius)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetFloat("speed", 0);

            CheckAndAttackPlayer();
        }
        // CASO 3: Fuera de rango
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetFloat("speed", 0);
        }
    }

    private void CheckAndAttackPlayer()
    {
        if (cooldownTimer > 0) return;

        PlayerController player = playerTransform.GetComponent<PlayerController>();
        if (player != null)
        {
            anim.SetTrigger("attack");
            player.TakeDamage(damageToPlayer);
            cooldownTimer = attackCooldown;
        }
    }

    public void TakeDamage(int damage, Vector2 playerPosition)
    {
        if (isDead) return;

        health -= damage;
        knockbackTimer = knockbackDuration;

        Vector2 knockbackDirection = (transform.position - (Vector3)playerPosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockbackDirection.x * knockbackForce, 3f), ForceMode2D.Impulse);

        if (health > 0) anim.SetTrigger("hurt");
        else Die();
    }

    private void Die()
    {
        isDead = true;
        anim.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectPlayerRadius);
    }
}