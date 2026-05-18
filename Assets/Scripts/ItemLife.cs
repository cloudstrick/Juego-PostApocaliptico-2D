using UnityEngine;

public class ItemLife : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // Intentamos sumar una vida. Si devuelve true (tenía menos de 3), se destruye
                if (player.AddLife())
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
