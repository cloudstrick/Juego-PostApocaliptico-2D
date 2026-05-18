using UnityEngine;

public class ItemHealing : MonoBehaviour
{
    [SerializeField] private float healAmount = 15f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // Intentamos curar al jugador. Si devuelve true (no estaba al 100%), se destruye
                if (player.Heal(healAmount))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}