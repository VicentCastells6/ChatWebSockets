using UnityEngine;
using UnityEngine.UI;

public class HostFlick : MonoBehaviour
{
    public RawImage imageToBlink; // Referencia a la imagen que deseas hacer parpadear
    public float blinkInterval = 0.5f; // Intervalo de parpadeo en segundos

    private float timer;

    // Con el color manejo la opacidad
    private Color originalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (imageToBlink == null)
        {
            Debug.LogError("No se ha asignado ninguna imagen para parpadear.");
            return;
        }
        // Guardar el color original de la imagen
        originalColor = imageToBlink.color; 
    }


    void Update()
    {
        if (imageToBlink != null)
        {
            timer += Time.deltaTime;
            float opacity = Mathf.PingPong(timer / blinkInterval, 1.0f); // calculo de alternancia de la opacidad, para que parpapee  
            imageToBlink.color = new Color(originalColor.r, originalColor.g, originalColor.b, opacity); // Aplicalo
        }
    }
}
