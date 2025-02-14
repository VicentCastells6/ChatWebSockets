using UnityEngine;
using UnityEngine.UI;

public class HostFlick : MonoBehaviour
{
    public RawImage imageToBlink; // Referencia a la imagen que deseas hacer parpadear
    public float blinkInterval = 0.5f; // Intervalo de parpadeo en segundos

    private float timer;
    private Color originalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (imageToBlink == null)
        {
            Debug.LogError("No se ha asignado ninguna imagen para parpadear.");
            return;
        }

        originalColor = imageToBlink.color; // Guardar el color original de la imagen
    }

    // Update is called once per frame
    void Update()
    {
        if (imageToBlink != null)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.PingPong(timer / blinkInterval, 1.0f); // Calcular la opacidad usando PingPong
            imageToBlink.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha); // Aplicar la opacidad
        }
    }
}
