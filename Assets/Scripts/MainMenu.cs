using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private GameObject controlesPanel; // Arrastra aquí el panel contenedor de los controles

    void Start()
    {
        // Nos aseguramos de que la imagen de controles empiece oculta al cargar el menú
        if (controlesPanel != null)
        {
            controlesPanel.SetActive(false);
        }
    }

    public void Jugar()
    {
        // Carga la escena del juego (Índice 1)
        SceneManager.LoadScene(1);
    }

    public void Salir()
    {
        Debug.Log("El jugador ha salido del juego.");
        Application.Quit();
    }

    // NUEVO: Muestra la pantalla de controles
    public void MostrarControles()
    {
        if (controlesPanel != null)
        {
            controlesPanel.SetActive(true);
        }
    }

    // NUEVO: Oculta la pantalla de controles
    public void OcultarControles()
    {
        if (controlesPanel != null)
        {
            controlesPanel.SetActive(false);
        }
    }
}