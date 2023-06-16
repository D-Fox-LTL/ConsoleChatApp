using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    static Dictionary<Socket, string> dicClientNames = new Dictionary<Socket, string>();
    static Socket socServerSocket;
    static string strServerPassword; // = "your_password"; // Set the server password here
    static Dictionary<string, string> credentials;


    static void Main()
    {
        // Set up server socket
        socServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socServerSocket.Bind(new IPEndPoint(IPAddress.Any, 26));
        socServerSocket.Listen(10);
        PrintLogo();
        Console.WriteLine("Type in your server passwd:");
        strServerPassword = Console.ReadLine();
        Console.WriteLine("Server started. Listening for incoming connections...");

        while (true)
        {
            WhatIsMyIP();
            LoadCredentials();


            // Accept incoming client connection
            Socket clientSocket = socServerSocket.Accept();
            Console.WriteLine("Client connected: " + clientSocket.RemoteEndPoint);

            // Prompt client to enter the password
            SendToClient(clientSocket, "Enter the server password:");
            string password = ReceiveFromClient(clientSocket);

            // Verify the password
            if (VerifyPassword(password))
            {
                // Prompt client to enter credentials
                SendToClient(clientSocket, "Password accepted.");
                SendToClient(clientSocket, "If you've account, type \"login\", else type \"signup\"");
                var whichOne = ReceiveFromClient(clientSocket);
                if (whichOne == "login")
                {

                    SendToClient(clientSocket, "Enter your username:");
                    string lstrClientUsername = ReceiveFromClient(clientSocket);
                    SendToClient(clientSocket, "Enter your password:");
                    string lstrClientPassword = ReceiveFromClient(clientSocket);


                    if (VerifyCredentials(lstrClientUsername, lstrClientPassword))
                    {
                        SendToClient(clientSocket, "Credentials accepted. Enter your name:");
                        string strClientName = ReceiveFromClient(clientSocket);
                        AssignClientName(clientSocket, strClientName);
                        SaveCredentials();

                        // Start a new thread to handle client communication
                        Thread thrClientThread = new Thread(() => HandleClientCommunication(clientSocket));
                        thrClientThread.Start();
                    }
                    else
                    {
                        SendToClient(clientSocket, "Invalid credentials. Connection closed.");
                        clientSocket.Close();
                    }
                    // Start a new thread to handle client communication
                    Thread clientThread = new Thread(() => HandleClientCommunication(clientSocket));
                    clientThread.Start();
                }
                else if (whichOne == "signup")
                {
                    SendToClient(clientSocket, "Enter your username: ");
                    string lstrClientUsername = ReceiveFromClient(clientSocket);
                    SendToClient(clientSocket, "Enter your password: ");
                    string lstrClientPassword = ReceiveFromClient(clientSocket);

                    CredentialsData credentialsData = new CredentialsData
                    {
                        pblstrUsername = lstrClientUsername,
                        pblstrPassword = lstrClientPassword
                    };
                    string json = JsonConvert.SerializeObject(credentialsData, Formatting.Indented);

                    // Step 4: Write the JSON string to a file
                    string filePath = "credentials.json";
                    File.WriteAllText(filePath, json);

                    SaveCredentials();

                    Console.WriteLine("Credentials saved to JSON file.");
                }
                else
                {
                    SendToClient(clientSocket, "Error. Invalid string. Connection closed.");
                    clientSocket.Close();
                }
            }
            else
            {
                SendToClient(clientSocket, "Invalid password. Connection closed.");
                clientSocket.Close();
            }
        }
    }

    static bool VerifyCredentials(string username, string password)
    {
        string storedPassword;
        if (credentials.TryGetValue(username, out storedPassword))
        {
            return password == storedPassword;
        }
        return false;
    }

    static void LoadCredentials()
    {
        try
        {
            string json = File.ReadAllText("credentials.json");
            var credentialsData = JsonConvert.DeserializeObject<CredentialsData>(json);
            credentials = credentialsData.Credentials;
        }
        catch (FileNotFoundException)
        {
            credentials = new Dictionary<string, string>();
        }
    }

    static void SaveCredentials()
    {
        var credentialsData = new CredentialsData { Credentials = credentials };
        string json = JsonConvert.SerializeObject(credentialsData, Formatting.Indented);
        File.WriteAllText("credentials.json", json);
    }

    static bool VerifyPassword(string password)
    {
        return password == strServerPassword;
    }

    static void HandleClientCommunication(Socket clientSocket)
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            try
            {
                string message = ReceiveFromClient(clientSocket);

                Console.WriteLine("Received from " + GetClientName(clientSocket) + ": " + message);

                // Broadcast the message to all connected clients
                foreach (Socket client in dicClientNames.Keys)
                {
                    SendToClient(client, GetClientName(clientSocket) + ": " + message);
                }
            }
            catch (SocketException)
            {
                // Client disconnected
                string clientName = GetClientName(clientSocket);
                Console.WriteLine("Client disconnected: " + clientName);
                RemoveClient(clientSocket);
                BroadcastClientDisconnection(clientName);
                break;
            }
        }
    }

    static void AssignClientName(Socket clientSocket, string name)
    {
        dicClientNames[clientSocket] = name;
        SendToClient(clientSocket, "Welcome, " + name + "!");
        BroadcastClientConnection(name);
    }

    static string GetClientName(Socket clientSocket)
    {
        return dicClientNames[clientSocket];
    }

    static void RemoveClient(Socket clientSocket)
    {
        string clientName = GetClientName(clientSocket);
        dicClientNames.Remove(clientSocket);
        Console.WriteLine("Removed client: " + clientName);
    }

    static void SendToClient(Socket clientSocket, string message)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        clientSocket.Send(buffer);
    }

    static string ReceiveFromClient(Socket clientSocket)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = clientSocket.Receive(buffer);
        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        return message.Trim();
    }
    //Function for finding out IPa
    static void WhatIsMyIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        Console.WriteLine("My IPv4 Address is : \n");
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                Console.WriteLine(ip.ToString());
            }
        }
    }

    static void BroadcastClientConnection(string clientName)
    {
        string message = clientName + " has joined the chat.";
        Console.WriteLine(message);
        foreach (Socket client in dicClientNames.Keys)
        {
            SendToClient(client, message);
        }
    }

    static void BroadcastClientDisconnection(string clientName)
    {
        string message = clientName + " has left the chat.";
        Console.WriteLine(message);
        foreach (Socket client in dicClientNames.Keys)
        {
            SendToClient(client, message);
        }
    }
    //Will print logo
    static void PrintLogo()
    {
        Console.WriteLine("\n" +
                "\n" +
                "\t ______            ______  ___________  ______    _____    _____   \n" +
                "\t/      \\ |      | /      \\      |      /      \\  |     \\  |     \\ \n" +
                "\t|        |      | |      |      |      |      |  |      | |      | \n" +
                "\t|        |      | |      |      |      |      |  |_____/  |_____/  \n" +
                "\t|        |------| |------|      |      |------|  |        |        \n" +
                "\t|        |      | |      |      |      |      |  |        |        \n" +
                "\t|        |      | |      |      |      |      |  |        |        \n" +
                "\t\\______/ |      | |      |      |      |      |  |        |        \n" +
                "\t__________________________________________________________________");
    }
}



