using UnityEngine;

public class CentipedeAI : MonoBehaviour
{
    [Header("Centipede Stats (Elite)")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2.8f;

    // Ocultado del Inspector para evitar confusiones
    private int currentHealth;

    [Header("Ajustes de Impacto")]
    //[SerializeField] private float knockbackForce = 12f;
    //[SerializeField] private float knockbackDuration = 0.2f;
    private float knockbackTimer;

    [Header("Estadísticas de Daño")]
    [SerializeField] private float damageToPlayer = 15f;
    [SerializeField] private float attackCooldown = 10f; // Tiempo de espera largo entre ataques
    private float cooldownTimer;

    [Header("drops (Botín Élite al Morir)")]
    [SerializeField] private GameObject specialHealingPrefab; // Asigna tu prefab "vidas" aquí

    [Header("Required Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;

    [Header("Configuración de Audio Élite (NUEVO)")]
    [SerializeField] private AudioSource centipedeAudioSource;
    [SerializeField] private AudioClip deathSound; // Solo mantuvimos el sonido de muerte

    private Transform playerTransform;
    private PlayerController playerScript;

    private bool isDead = false;
    private Vector3 originalScale;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth; // Se inicializa internamente al 100%
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
        if (isDead) return;
        if (knockbackTimer > 0) knockbackTimer -= Time.deltaTime;
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isDead || playerTransform == null || playerScript == null) return;

        if (playerScript.IsDead)
        {
            StopMovement();
            return;
        }

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            StopMovement();
            return;
        }

        HandleBossAI();
    }

    void HandleBossAI()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Si está a rango de ataque, se detiene y prepara el ataque
        if (distanceToPlayer <= attackRange)
        {
            StopMovement();
            CheckAndAttackPlayer();
        }
        // Si está a rango de detección pero lejos, lo persigue
        else if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMovement();
        }
    }

    private void CheckAndAttackPlayer()
    {
        if (cooldownTimer > 0) return;

        if (playerScript != null && !playerScript.IsDead)
        {
            anim.SetTrigger("Attack"); // Dispara la animación de ataque
            cooldownTimer = attackCooldown; // Activa el cooldown
        }
    }

    // NUEVO: Esta función se llamará automáticamente desde el 4.º frame de la animación
    public void DealDamageAtFrame()
    {
        if (isDead || playerScript == null || playerScript.IsDead) return;

        // Validamos si el jugador sigue estando cerca cuando cae el golpe
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= attackRange)
        {
            playerScript.TakeDamage(damageToPlayer);
        }
    }

    void MoveTowardsPlayer()
    {
        float directionX = playerTransform.position.x - transform.position.x;
        rb.linearVelocity = new Vector2(directionX > 0 ? walkSpeed : -walkSpeed, rb.linearVelocity.y);

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        if (directionX > 0)
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        else if (directionX < 0)
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetFloat("Speed", 0);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt");
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        StopMovement();
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;

        anim.SetBool("IsDead", true);

        // Reproducir el sonido de muerte
        if (centipedeAudioSource != null && deathSound != null)
        {
            centipedeAudioSource.PlayOneShot(deathSound);
        }

        // Soltar prefab de vida al morir
        if (specialHealingPrefab != null)
        {
            Instantiate(specialHealingPrefab, transform.position, Quaternion.identity);
        }

        // Desaparece físicamente del juego tras 1.5 segundos exactos
        Destroy(gameObject, 1.5f);
    }
}