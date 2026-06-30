using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic instance;

    void Awake()
    {
        // Sistema de persistencia (Singleton)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Evita que este objeto se destruya al cambiar de escena
        }
        else
        {
            Destroy(gameObject); // Si ya existe uno, destruye el duplicado
        }
    }
}
