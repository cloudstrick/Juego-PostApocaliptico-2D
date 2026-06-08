using System.Collections; // NUEVO: Necesario para usar Corrutinas y tiempos de espera
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private GameObject controlesPanel;

    [Header("Transición Iris (Modelo 3)")]
    [SerializeField] private Animator irisAnimator; // Arrastra el IrisObject del Menú aquí
    [SerializeField] private float transitionDelay = 1f; // Lo que tarda el círculo en cerrarse

    private bool isTransitioning = false;

    void Start()
    {
        if (controlesPanel != null)
        {
            controlesPanel.SetActive(false);
        }
    }

    public void Jugar()
    {
        // Evitamos que el jugador pulse el botón muchas veces seguidas
        if (!isTransitioning)
        {
            StartCoroutine(LoadLevelRoutine());
        }
    }

    // Corrutina para esperar que el Iris se cierre antes de cambiar de escena
    private IEnumerator LoadLevelRoutine()
    {
        isTransitioning = true;

        if (irisAnimator != null)
        {
            irisAnimator.SetTrigger("StartTransition"); // Dispara la animación Iris_Close
        }

        // Esperamos el segundo que tarda en cerrarse a negro total
        yield return new WaitForSeconds(transitionDelay);

        // Cargamos el Escenario 1 (LV01)
        SceneManager.LoadScene(1);
    }

    public void Salir()
    {
        Debug.Log("El jugador ha salido del juego.");
        Application.Quit();
    }

    public void MostrarControles()
    {
        if (controlesPanel != null)
        {
            controlesPanel.SetActive(true);
        }
    }

    public void OcultarControles()
    {
        if (controlesPanel != null)
        {
            controlesPanel.SetActive(false);
        }
    }
}