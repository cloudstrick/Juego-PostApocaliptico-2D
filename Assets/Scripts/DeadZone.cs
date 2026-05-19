using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si lo que cayó al vacío es el jugador
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // Llamamos a la nueva función de caída del jugador
                player.FallIntoAbyss();
            }
        }
    }
}