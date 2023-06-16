using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

class Client
{
    static void Main()
    {
        string strSendMsg = string.Empty;
        // Connect to the server
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        PrintLogo();
        Console.WriteLine("\nType in host IPv4 to connect to the host\n");
        var varIPad = Console.ReadLine();
        try {
            clientSocket.Connect(varIPad, 26); } catch (Exception e) { Console.WriteLine(e); Environment.Exit(1); }
        Console.WriteLine("Connected to server.");

        // Start a new thread to handle receiving messages from the server
        // You can use threads, tasks, or asynchronous methods
        // to handle sending and receiving messages simultaneously
        // ...


        // Start the timer to receive messages
        System.Timers.Timer timer = new System.Timers.Timer(1000); // Set the interval to 1 second (1000 milliseconds)
        timer.Elapsed += (sender, e) => ReceiveMessages(clientSocket);
        timer.Start();


        while (strSendMsg != "exit")
        {
            strSendMsg = Console.ReadLine();
            SendMessages(clientSocket, strSendMsg);
        }
    }

    private static void ReceiveMessages(Socket clientSocket)
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int bytesRead = clientSocket.Receive(buffer);
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received from server: " + message);
        }
    }


    static void SendMessages(Socket clientSocket, string message)
    {
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        clientSocket.Send(messageBytes);
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
