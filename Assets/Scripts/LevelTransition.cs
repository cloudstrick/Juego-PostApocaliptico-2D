using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "LV02"; // Coloca el nombre exacto de tu segundo escenario

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Cargamos el segundo escenario
            SceneManager.LoadScene(nextSceneName);
        }
    }
}