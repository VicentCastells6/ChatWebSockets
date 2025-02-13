using UnityEngine;
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

    void Start()
    {
        wss = new WebSocketServer(7777);
        // Asignar colores a los usuarios conectados
        ChatBehavior.SetColors(colors);
        // Agregar el comportamiento del chat al servidor
        wss.AddWebSocketService<ChatBehavior>("/");
        wss.Start();
        Debug.Log("Servidor WebSocket iniciado en ws://127.0.0.1:7777/");
    }

    void OnDestroy()
    {
        if (wss != null)
        {
            // Guardar el historial de mensajes en un archivo de texto cuando se cierre el servidor
            string fechaActual = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            File.WriteAllLines("chatlog" + fechaActual + ".txt", historialMensajes);
            wss.Stop();
            wss = null;
            Debug.Log("Servidor WebSocket detenido.");
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

            Debug.Log($"Usuario <color={userColor}>{userId}</color> conectado con color {userColor}.");
            Sessions.Broadcast($"<color={userColor}>{userId}</color> se ha conectado.");
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
            Sessions.Broadcast($"{userId} se ha desconectado.");
        }
    }
}