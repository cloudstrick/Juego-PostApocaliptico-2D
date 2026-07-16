using UnityEngine;
using System.Collections; // Necesario para los retrasos (Victory Screen)

public class CentipedeAI : MonoBehaviour
{
    [Header("Centipede Stats (Elite)")]
    [SerializeField] private int maxHealth = 25; // Mucha más HP para un boss
    [SerializeField] private int currentHealth;
    [SerializeField] private float walkSpeed = 2.5f; // Velocidad Élite
    [SerializeField] private float detectionRange = 10f; // Rango de caza Élite
    [SerializeField] private float attackRange = 1.9f; // Rango de ataque

    [Header("Ajustes de Impacto")]
    [SerializeField] private float knockbackForce = 12f; // Mucho empuje al Player
    [SerializeField] private float knockbackDuration = 0.25f; // Tiempo que el player está aturdido
    private float knockbackTimer;

    [Header("Estadísticas de Daño")]
    [SerializeField] private float damageToPlayer = 20f; // Daño Élite
    [SerializeField] private float attackCooldown = 2.5f; // Velocidad de ataque rápida

    [Header(" drops (Botín Élite al Morir)")]
    [SerializeField] private GameObject specialHealingPrefab; // Tal vez una curación completa?

    [Header("Required Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;

    [Header("Configuración de Audio Élite (NUEVO)")]
    [SerializeField] private AudioSource centipedeAudioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound; // Chillido/Crujido de Boss final

    // Referencias al Jugador
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
        currentHealth = maxHealth;
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
        if (isDead) return; // Si el boss está muerto, no hace nada
        if (knockbackTimer > 0) knockbackTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isDead || playerTransform == null || playerScript == null) return;

        // Si el jugador ya murió por completo, el Boss se calma (o acecha su cuerpo?)
        if (playerScript.IsDead)
        {
            StopMovement();
            return;
        }

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            StopMovement(); // Aturdido
            return;
        }

        HandleBossAI();
    }

    void HandleBossAI()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Lógica de Movimiento y Animación "Walk"
        if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMovement(); // Idle
        }
    }

    void MoveTowardsPlayer()
    {
        float directionX = playerTransform.position.x - transform.position.x;
        rb.linearVelocity = new Vector2(directionX > 0 ? walkSpeed : -walkSpeed, rb.linearVelocity.y);

        // Animación "Speed" para el Animator
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        // Voltear Centipede correctamente Élite (Mirando hacia el Player)
        if (directionX > 0)
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z); // Mirando Derecha (ajustar si tus animaciones están al revés)
        else if (directionX < 0)
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z); // Mirando Izquierda
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetFloat("Speed", 0);
    }

    // NUEVO: Función pública para recibir daño Élite (Hurt/Death)
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            // Trigger Animación "Hurt"
            anim.SetTrigger("Hurt");

            // Sonido de Daño Élite
            if (centipedeAudioSource != null && hurtSound != null)
            {
                centipedeAudioSource.PlayOneShot(hurtSound);
            }
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

        // Trigger Animación "Death" / Bloquear con Bool "IsDead"
        anim.SetBool("IsDead", true);

        // Sonido de Muerte del Centipede final
        if (centipedeAudioSource != null && deathSound != null)
        {
            centipedeAudioSource.PlayOneShot(deathSound);
        }

        // Lógica de Victoria Élite (Llamar UIManager ShowWinScreen)
        if (UIManager.instance != null)
        {
            StartCoroutine(TriggerVictoryRoutine());
        }

        Debug.Log("ELITE DEFEATED: CENTIPEDE IS DEAD.");
    }

    // Esperar a que la animación de muerte Élite termine para mostrar la victoria
    private IEnumerator TriggerVictoryRoutine()
    {
        yield return new WaitForSeconds(2.0f); // Dale tiempo a la animación de muerte
        UIManager.instance.ShowWinScreen();
    }
}