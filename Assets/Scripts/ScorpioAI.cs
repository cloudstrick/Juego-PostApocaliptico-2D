using UnityEngine;

public class ScorpioAI : MonoBehaviour
{
    [Header("Configuración de Caza")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float detectionRange = 5f; // Distancia a la que empieza a perseguirte

    [Header("Ajustes de Impacto")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;
    private float knockbackTimer;

    [Header("Estadísticas")]
    [SerializeField] private int health = 4;

    [Header("Configuración de Ataque")]
    [SerializeField] private float damageToPlayer = 1f;
    [SerializeField] private float attackCooldown = 2.5f; // Más lento (antes era 1.5)
    [SerializeField] private float detectPlayerRadius = 0.7f; // Rango de ataque
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform playerTransform;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Encontrar automáticamente al leñador por su Tag
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

        // Si está bajo el efecto del hachazo, retrocede y no hace nada más
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            anim.SetFloat("speed", 0);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // CASO 1: El jugador está en rango de persecución pero fuera del rango de ataque
        if (distanceToPlayer <= detectionRange && distanceToPlayer > detectPlayerRadius)
        {
            float directionX = playerTransform.position.x - transform.position.x;

            // Moverse hacia el jugador
            float moveDir = directionX > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(moveDir * walkSpeed, rb.linearVelocity.y);
            anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));

            // Voltear el sprite según la dirección (Modifica los signos si tu sprite mira al revés)
            if (directionX > 0) transform.localScale = new Vector3(-1, 1, 1);
            else if (directionX < 0) transform.localScale = new Vector3(1, 1, 1);
        }
        // CASO 2: Está lo suficientemente cerca para morder (Rango de ataque)
        else if (distanceToPlayer <= detectPlayerRadius)
        {
            // Se detiene por completo para no empujar al jugador
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetFloat("speed", 0);

            CheckAndAttackPlayer();
        }
        // CASO 3: El jugador está lejos, el escorpión se queda esperando
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
            anim.SetTrigger("attack"); // Dispara animación de picotazo
            player.TakeDamage(damageToPlayer);
            cooldownTimer = attackCooldown; // Reinicia el contador de lentitud
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
        // Círculo azul: Rango de detección / Círculo amarillo: Rango de ataque
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectPlayerRadius);
    }
}