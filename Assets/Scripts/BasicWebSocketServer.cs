using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System;
using System.IO;

public class BasicWebSocketServer : MonoBehaviour
{
    private WebSocketServer wss;
    private List<string> colors = new List<string> { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#00FFFF", "#FF00FF" };
    private static List<string> historialMensajes = new List<string>();
    private static bool hostExists = false; // Variable estática para rastrear si ya existe un host
    private bool isHost = false; // Variable para indicar si esta instancia es el host

    private static int conectados = 0;

    public RawImage rawImage;
    public TMPro.TMP_Text conectadosText; // Agrega una referencia al TMP_Text

    void Start()
    {
        if (!hostExists)
        {
            isHost = true;
            hostExists = true;
            Debug.Log("Esta instancia es el host.");
            try
            {
                wss = new WebSocketServer(7777);
                // Asignar colores a los usuarios conectados
                ChatBehavior.SetColors(colors);
                // Agregar el comportamiento del chat al servidor
                wss.AddWebSocketService<ChatBehavior>("/");
                wss.Start();
                rawImage.enabled = true;
                Debug.Log("Servidor WebSocket iniciado en ws://127.0.0.1:7777/");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error al iniciar el servidor WebSocket: " + ex.Message);
                isHost = false;
                hostExists = false;
                rawImage.enabled = false;
            }
        }
        else
        {
            rawImage.enabled = false;
            Debug.Log("Esta instancia no es el host.");
        }
        UpdateConectadosText(); // Asegúrate de actualizar el texto al iniciar
    }

    void OnDestroy()
    {
        if (isHost && wss != null)
        {
            // Guardar el historial de mensajes en un archivo de texto cuando se cierre el servidor
            string fechaActual = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            File.WriteAllLines("chatlog" + fechaActual + ".txt", historialMensajes);
            wss.Stop();
            wss = null;
            hostExists = false; // Restablecer la variable estática cuando el host se destruye
            Debug.Log("Servidor WebSocket detenido.");
        }
    }

    private void UpdateConectadosText()
    {
        if (conectadosText != null)
        {
            conectadosText.text = "Conectados: " + conectados.ToString();
        }
    }

    public static void IncrementConectados()
    {
        conectados++;
        UpdateConectadosTextStatic();
    }

    public static void DecrementConectados()
    {
        conectados--;
        UpdateConectadosTextStatic();
    }

    private static void UpdateConectadosTextStatic()
{
    var instances = FindObjectsOfType<BasicWebSocketServer>();
    foreach (var instance in instances)
    {
        Debug.Log("Instancia: " + instance);
        if (instance != null)
        {
            instance.UpdateConectadosText();
        }
    }
}

    public class ChatBehavior : WebSocketBehavior
    {
        private string userId;
        private string userColor;
        private static List<string> colors;

        // Método para asignar colores desde el servidor
        public static void SetColors(List<string> colorList)
        {
            colors = colorList;
        }

        protected override void OnOpen()
        {
            // Asignar nombre e id en funcion al numero de sesiones y colores disponibles
            userId = "Usuario" + Sessions.Count;
            if (colors != null && colors.Count > 0)
            {
                userColor = colors[Sessions.Count % colors.Count];
            }
            else
            {
                userColor = "#FFFFFF"; // Color blanco por defecto si la lista está vacía
            }

            Debug.Log($" <color={userColor}>{userId}</color> conectado con color {userColor}.");
            string messageFormated = $"{userId} se ha conectado.";
            historialMensajes.Add(messageFormated);
            Sessions.Broadcast($"<color={userColor}>{userId}</color> se ha conectado.");
            BasicWebSocketServer.IncrementConectados();
            UpdateConectadosTextStatic();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            //Mensaje del chat
            string message = $"<color={userColor}>{userId}</color>: {e.Data}";
            // Mensaje para el chatlog
            string messageFormated = $"{userId}: {e.Data}";
            Debug.Log($"Mensaje recibido: {message}");
            Sessions.Broadcast(message);
            historialMensajes.Add(messageFormated);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Debug.Log($"Usuario {userId} desconectado.");
            string message = $"<color={userColor}>{userId}</color> se ha desconectado.";
            string messageFormated = $"{userId} se ha desconectado.";
            Sessions.Broadcast(message);
            historialMensajes.Add(messageFormated);
            BasicWebSocketServer.DecrementConectados();
            UpdateConectadosTextStatic();
        }
    }
}