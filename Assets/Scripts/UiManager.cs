using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Referencias de Salud y HUD")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI gemText;
    [SerializeField] private GameObject[] lifeIcons;

    private int gemCount = 0;

    [Header("Menús de Interfaz (Paneles)")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel; // NUEVO: Arrastra tu panel de muerte aquí

    private bool isPaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void AddGem()
    {
        gemCount++;
        gemText.text = "x " + gemCount.ToString();
    }

    public void UpdateHUD(float currentHealth, int currentLives)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                lifeIcons[i].SetActive(i < currentLives);
            }
        }
    }

    // --- NUEVO: MOSTRAR PANTALLA DE MUERTE ---
    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Desbloqueamos el cursor del mouse para que el jugador pueda hacer clic en el botón
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // --- LÓGICA DE PAUSA ---
    public void OnPause(InputAction.CallbackContext context)
    {
        // Evitamos pausar si el panel de Game Over ya está activo
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;

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
        EventSystem.current.SetSelectedGameObject(null);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Carga la escena con índice 0 (Menú Principal)
    }
}