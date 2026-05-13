using UnityEngine;

public class ScorpioAI : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private Transform edgeCheck; // Objeto al frente de los pies
    [SerializeField] private LayerMask groundLayer;
    [Header("Ajustes de Impacto")]
    [SerializeField] private float knockbackForce = 5f;
    private float knockbackTimer;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Estadísticas")]
    [SerializeField] private int health = 4;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isFacingRight = false;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // 1. Solo procesamos movimiento y giro si el temporizador de golpe terminó
        if (knockbackTimer <= 0)
        {
            // Moverse normalmente
            float velocityX = isFacingRight ? walkSpeed : -walkSpeed;
            rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
            anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));

            // 2. Detectar el borde SOLO cuando no estamos retrocediendo por un golpe
            bool isNearEdge = !Physics2D.OverlapCircle(edgeCheck.position, 0.2f, groundLayer);

            if (isNearEdge)
            {
                Flip();
            }
        }
        else
        {
            // El temporizador baja mientras el escorpión está en el aire por el golpe
            knockbackTimer -= Time.fixedDeltaTime;

            // Mientras retrocede, la animación de velocidad debería ser 0
            anim.SetFloat("speed", 0);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        // Girar el sprite
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    public void TakeDamage(int damage, Vector2 playerPosition)
    {
        if (isDead) return;
        health -= damage;

        // Activamos el temporizador de retroceso
        knockbackTimer = knockbackDuration;

        Vector2 knockbackDirection = (transform.position - (Vector3)playerPosition).normalized;

        // IMPORTANTE: Reseteamos la velocidad antes de aplicar la fuerza para que el impulso sea limpio
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
}
