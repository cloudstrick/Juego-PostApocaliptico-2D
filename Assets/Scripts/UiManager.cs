using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // <--- AGREGA ESTA LÍNEA

public class UIManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public Slider healthSlider;
    public GameObject pausePanel;

    private bool isPaused = false;

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        // 1. Limpiamos el foco de la UI para que el teclado vuelva al juego
        EventSystem.current.SetSelectedGameObject(null); // <--- ESTA ES LA MAGIA

        // 2. Cerramos el panel y reanudamos el tiempo
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // 3. Volvemos a bloquear el cursor para que no estorbe
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}