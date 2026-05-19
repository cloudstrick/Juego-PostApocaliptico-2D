using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Le ordenamos al UIManager abrir la pantalla de victoria
            if (UIManager.instance != null)
            {
                UIManager.instance.ShowWinScreen();
            }
        }
    }
}