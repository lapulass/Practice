using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestServer {
    static class Program {

        static Server server;

        static void Main(string[] args)
        {

            server = new TestServer.Server();
            server.InitailizeServer();

            Thread serverthread = new Thread(ServerRun);
            serverthread.Start();


            // Shut Down Process
            Console.ReadLine();
        }

        static void ServerRun()
        {
            // Main Server Logic
            server.RunServer();
            // Other Process
            Console.ReadLine();
        }
    }
}

