using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "LV02";
    [SerializeField] private float transitionDelay = 1f; // Tiempo en segundos que tarda el iris en cerrarse

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTransitioning)
        {
            StartCoroutine(ChangeLevelRoutine());
        }
    }

    private IEnumerator ChangeLevelRoutine()
    {
        isTransitioning = true;

        // Le ordenamos al UIManager que inicie el cierre del Iris
        if (UIManager.instance != null)
        {
            UIManager.instance.TriggerIrisClose();
        }

        // Esperamos a que la animación termine en pantalla
        yield return new WaitForSeconds(transitionDelay);

        // Cargamos el siguiente escenario
        SceneManager.LoadScene(nextSceneName);
    }
}