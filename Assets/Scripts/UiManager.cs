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

    public static int gemCount = 0;

    [Header("Menús de Interfaz (Paneles)")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI totalGemsText;

    [Header("Transición Iris (Modelo 3)")]
    [SerializeField] private Animator irisAnimator; // NUEVO: Arrastra el objeto Iris aquí

    private bool isPaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            gemCount = 0;
        }

        UpdateHUD(PlayerController.currentHealth, PlayerController.currentLives);

        if (gemText != null)
        {
            gemText.text = "x " + gemCount.ToString();
        }
    }

    // NUEVA FUNCIÓN: Dispara el trigger de cierre en el Animator del Iris
    public void TriggerIrisClose()
    {
        if (irisAnimator != null)
        {
            irisAnimator.SetTrigger("StartTransition");
        }
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

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ShowWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);

            if (totalGemsText != null)
            {
                totalGemsText.text = "Objetivos recolectados: " + gemCount.ToString();
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;
        if (winPanel != null && winPanel.activeSelf) return;

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
        SceneManager.LoadScene(0);
    }
}