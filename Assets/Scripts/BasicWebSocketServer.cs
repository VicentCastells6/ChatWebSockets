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
    private List<string> colors = new List<string> 
    { 
        "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#00FFFF", "#FF00FF",
        "#800000", "#808000", "#008000", "#800080", "#008080", "#000080",
        "#FFA500", "#A52A2A", "#8A2BE2", "#5F9EA0", "#D2691E", "#FF7F50"
    };
    private static List<string> historialMensajes = new List<string>();
    private static bool hostExists = false; // Variable estática para rastrear si ya existe un host
    private bool isHost = false; // Variable para indicar si esta instancia es el host

    public static  int conectados = 0;

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
            wss.WebSocketServices["/"].Sessions.Broadcast("El servidor se ha cerrado.");
            wss.Stop();
            wss = null;
            hostExists = false; // Restablecer la variable estática cuando el host se destruye
            Debug.Log("Servidor WebSocket detenido.");
        }
    }

    public void UpdateConectadosText()
    {
        if (conectadosText != null)
        {
            Debug.Log("Actualizando texto de conectados.");
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
        private static System.Random random = new System.Random();

        // Método para asignar colores desde el servidor
        public static void SetColors(List<string> colorList)
        {
            colors = colorList;
        }

        protected override void OnOpen()
        {
            BasicWebSocketServer.IncrementConectados();
            // Asignar nombre e id en funcion al numero de sesiones y colores disponibles
            userId = "Usuario" + Sessions.Count;
            if (colors != null && colors.Count > 0)
            {
                userColor = colors[random.Next(colors.Count)];
            }
            else
            {
                userColor = "#FFFFFF"; // Color blanco por defecto si la lista está vacía
            }

            Debug.Log($" <color={userColor}>{userId}</color> conectado con color {userColor}.");
            string messageFormated = $"{userId} se ha conectado.";
            historialMensajes.Add(messageFormated);
            Sessions.Broadcast($"<color={userColor}>{userId}</color> se ha conectado.");
            // Incrementar el contador de conectados

            Sessions.Broadcast("$$Conectados: " + BasicWebSocketServer.conectados.ToString());
            
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            try {
                if (!BasicWebSocketServer.hostExists)
                {
                    Sessions.Broadcast("El servidor no está disponible.");
                    return;
                }
                else{
                    //Mensaje del chat
                    string message = $"<color={userColor}>{userId}</color>: {e.Data}";
                    // Mensaje para el chatlog
                    string messageFormated = $"{userId}: {e.Data}";
                    Debug.Log($"Mensaje recibido: {message}");
                    Sessions.Broadcast(message);
                    historialMensajes.Add(messageFormated);
                }
            } catch (Exception ex) {
                Debug.LogError("Error al enviar mensaje: " + ex.Message);
            }
            
        }

        protected override void OnClose(CloseEventArgs e)
        {
            BasicWebSocketServer.DecrementConectados();
            Debug.Log($"Usuario {userId} desconectado.");
            string message = $"<color={userColor}>{userId}</color> se ha desconectado.";
            string messageFormated = $"{userId} se ha desconectado.";
            Sessions.Broadcast(message);
            historialMensajes.Add(messageFormated);
            
            Sessions.Broadcast("$$Conectados: " + BasicWebSocketServer.conectados.ToString());

        }
    }
}