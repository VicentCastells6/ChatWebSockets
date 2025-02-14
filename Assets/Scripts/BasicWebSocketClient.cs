using UnityEngine;
using WebSocketSharp;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BasicWebSocketClient : MonoBehaviour
{
    private WebSocket ws;
    public TMP_InputField inputField;
    public TMP_Text chatHistory;
    public ScrollRect scrollRect;
    public Button sendButton; // Agrega una referencia al botón de enviar

    // Lista para almacenar los mensajes recibidos
    private List<string> receivedMessages = new List<string>();

    void Start()
    {
        chatHistory.text = "";
        StartCoroutine(ConnectToServer());

        inputField.onEndEdit.AddListener(HandleEnterKey);
    }

    // Método que permite que si le das al enter o al intro del teclado se envíe el mensaje.
    private void HandleEnterKey(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessageToServer();
        }
    }

    // Metodo que se encarga de conectarse al servidor para que no se solape.
    IEnumerator ConnectToServer()
    {
        // Comprobacion de que el server está encendido
        while (ws == null || ws.ReadyState != WebSocketState.Open)
        {
            ws = new WebSocket("ws://127.0.0.1:7777/");

            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("WebSocket conectado correctamente.");
            };

            // Metodo que se encarga de recibir los mensajes del servidor.
            ws.OnMessage += (sender, e) =>
            {
                // Almacenar el mensaje recibido en la lista
                receivedMessages.Add(e.Data);
            };

            ws.OnError += (sender, e) =>
            {
                Debug.LogError("Error en el WebSocket: " + e.Message);
            };

            ws.OnClose += (sender, e) =>
            {
                Debug.Log("WebSocket cerrado. Código: " + e.Code + ", Razón: " + e.Reason);
            };

            // Conectar al servidor
            ws.ConnectAsync();

            // Esperar un segundo antes de intentar conectarse de nuevo, para que entre despues de que el servidor esté encendido.
            yield return new WaitForSeconds(1);
        }
    }

    void Update()
    {
        // Si hay mensajes en la lista, actualizar la UI
        if (receivedMessages.Count > 0)
        {
            foreach (var message in receivedMessages)
            {
                chatHistory.text += message + "\n";
            }
            // Limpiar la lista después de procesar los mensajes
            receivedMessages.Clear(); 

            // Asegurar que el scroll se mueva al final para mostrar el último mensaje
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // Metodo que se encarga de enviar el mensaje al servidor.
    public void SendMessageToServer()
    {
        if (inputField == null)
        {
            Debug.LogError("El campo de entrada no está asignado.");
            return;
        }

        if (ws == null)
        {
            Debug.LogError("El WebSocket no está inicializado.");
            return;
        }

        if (ws.ReadyState == WebSocketState.Open)
        {
            string message = inputField.text;

            ws.Send(message);
            inputField.text = "";
        }
        else
        {
            Debug.LogError("No se puede enviar el mensaje. La conexión no está abierta.");
        }
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }
}