using UnityEngine;

public class ScorpioAI : MonoBehaviour
{
    [Header("Configuración de Caza")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;

    [Header("Ajustes de Impacto")]
    [SerializeField] private float knockbackForce = 7f;
    [SerializeField] private float knockbackDuration = 0.2f;
    private float knockbackTimer;

    [Header("Estadísticas")]
    [SerializeField] private int health = 4;

    [Header("Configuración de Ataque")]
    [SerializeField] private float damageToPlayer = 15f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float detectPlayerRadius = 1.6f;
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer;

    [Header("Drops (Botín al Morir)")]
    [SerializeField] private GameObject healingItemPrefab;

    [Header("Configuración de Audio (NUEVO)")]
    [SerializeField] private AudioSource scorpioAudioSource;
    [SerializeField] private AudioClip deathSound; // Chillido/Crujido de muerte

    private Rigidbody2D rb;
    private Animator anim;
    private Transform playerTransform;
    private PlayerController playerScript;

    private bool isDead = false;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        originalScale = transform.localScale;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerScript = playerObj.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isDead || playerTransform == null) return;

        // Si el jugador ya murió por completo, el escorpión se calma
        if (playerScript != null && playerScript.IsDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetFloat("speed", 0);
            return;
        }

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            anim.SetFloat("speed", 0);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > detectPlayerRadius)
        {
            float directionX = playerTransform.position.x - transform.position.x;
            rb.linearVelocity = new Vector2(directionX > 0 ? walkSpeed : -walkSpeed, rb.linearVelocity.y);
            anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));

            if (directionX > 0) transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            else if (directionX < 0) transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        }
        else if (distanceToPlayer <= detectPlayerRadius)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetFloat("speed", 0);
            CheckAndAttackPlayer();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetFloat("speed", 0);
        }
    }

    private void CheckAndAttackPlayer()
    {
        if (cooldownTimer > 0) return;

        if (playerScript != null && !playerScript.IsDead)
        {
            anim.SetTrigger("attack");
            playerScript.TakeDamage(damageToPlayer);
            cooldownTimer = attackCooldown;
        }
    }

    public void TakeDamage(int damage, Vector2 playerPosition)
    {
        if (isDead) return;
        health -= damage;
        knockbackTimer = knockbackDuration;

        // Efecto de empuje (Knockback)
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

        // NUEVO: Reproducir sonido de muerte del escorpión
        if (scorpioAudioSource != null && deathSound != null)
        {
            scorpioAudioSource.PlayOneShot(deathSound);
        }

        if (healingItemPrefab != null)
        {
            Instantiate(healingItemPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 1.5f);
    }
}