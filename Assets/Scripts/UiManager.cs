using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Necesario para cambiar de escenas

public class UIManager : MonoBehaviour
{
    public Slider healthSlider;
    public GameObject pausePanel;
    private bool isPaused = false;

    void Update()
    {
        // Detectar si presionas la tecla de pausa (ej: Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void UpdateHealth(float currentHealth)
    {
        healthSlider.value = currentHealth;
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Congela el tiempo del juego
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Reanuda el tiempo
        isPaused = false;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f; // Importante reanudar antes de salir
        SceneManager.LoadScene(0); // Carga la escena del menú (index 0)
    }
}
