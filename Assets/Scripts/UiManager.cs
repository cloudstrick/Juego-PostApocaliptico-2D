using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Singleton para acceso global
    public static UIManager instance;

    [Header("Referencias de Salud y HUD")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI gemText; // Arrastra el "x 0" de las gemas aquí

    // MODIFICACIÓN: Cambiamos el texto único por un arreglo de 3 elementos para los iconos
    [Header("Iconos de Vidas (Asigna los 3 en orden: 1, 2 y 3)")]
    [SerializeField] private GameObject[] lifeIcons;

    private int gemCount = 0;

    [Header("Menú de Pausa")]
    [SerializeField] private GameObject pausePanel;
    private bool isPaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // --- LÓGICA DE COLECCIONABLES ---
    public void AddGem()
    {
        gemCount++;
        gemText.text = "x " + gemCount.ToString();
    }

    // --- LÓGICA DE ACTUALIZACIÓN COMPLETA DEL HUD ---
    public void UpdateHUD(float currentHealth, int currentLives)
    {
        // 1. Actualiza el slider de salud (0 a 100)
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // 2. NUEVO: CONTROL DE ICONOS VISUALES DE VIDA
        // Recorremos el arreglo de 3 iconos mediante un bucle for
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                // Si el índice actual es menor que tus vidas, el icono se enciende (true).
                // De lo contrario, se apaga (false).
                lifeIcons[i].SetActive(i < currentLives);
            }
        }
    }

    // --- LÓGICA DE PAUSA ---
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