using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Jugar()
    {
        // Carga la escena del juego. Asegúrate de que tu nivel sea el índice 1 en Build Settings
        SceneManager.LoadScene(1);
    }

    public void Salir()
    {
        Debug.Log("El jugador ha salido del juego.");
        Application.Quit(); // Cierra la aplicación (solo funciona en el juego ya exportado/.exe)
    }
}