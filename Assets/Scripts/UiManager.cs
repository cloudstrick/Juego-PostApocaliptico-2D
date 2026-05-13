using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro; // No olvides esto para el texto de la gema

public class UIManager : MonoBehaviour
{
    // Singleton para que cualquier objeto (como la gema) encuentre al UIManager
    public static UIManager instance;

    [Header("Referencias de Salud y HUD")]
    public Slider healthSlider;
    public TextMeshProUGUI gemText; // Arrastra el "x 0" aquí
    private int gemCount = 0;

    [Header("Menú de Pausa")]
    public GameObject pausePanel;
    private bool isPaused = false;

    void Awake()
    {
        // Esto permite que el recolectable diga "UIManager.instance.AddGem()"
        if (instance == null) instance = this;
    }

    // --- LÓGICA DE COLECCIONABLES ---
    public void AddGem()
    {
        gemCount++;
        gemText.text = "x " + gemCount.ToString();
        // Aquí puedes añadir el feedback auditivo que pidió el profe
    }

    // --- LÓGICA DE SALUD ---
    public void UpdateHealth(float currentHealth)
    {
        healthSlider.value = currentHealth;
    }

    // --- LÓGICA DE PAUSA (Lo que ya tenías) ---
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