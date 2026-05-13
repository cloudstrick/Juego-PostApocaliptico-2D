using System.Collections;
using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    private bool recogido = false;
    private SpriteRenderer sr;

    private void Start()
    {
        // Obtenemos el componente para poder cambiar su transparencia
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !recogido)
        {
            recogido = true;
            StartCoroutine(EfectoFlotarYDesvanecer());
        }
    }

    private IEnumerator EfectoFlotarYDesvanecer()
    {
        // 1. Notificar al UIManager
        if (UIManager.instance != null)
        {
            UIManager.instance.AddGem();
        }

        // 2. Configuración del efecto
        float duracion = 1.2f; // "Lentamente": aumentamos el tiempo (antes era 0.5)
        float tiempo = 0f;

        Vector3 posicionInicial = transform.position;
        // Subirá 2 unidades hacia arriba
        Vector3 posicionFinal = posicionInicial + Vector3.up * 2f;

        Color colorOriginal = sr.color;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;

            // Movimiento suave hacia arriba
            transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);

            // Desvanecimiento: cambiamos el Alpha de 1 (opaco) a 0 (transparente)
            float nuevoAlpha = Mathf.Lerp(1f, 0f, t);
            sr.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, nuevoAlpha);

            yield return null;
        }

        // 3. Destrucción final
        Destroy(gameObject);
    }
}