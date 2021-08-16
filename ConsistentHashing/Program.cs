using System;
using System.Collections.Generic;

namespace ConsistentHashing
{
    static class Program
    {
        static int ServerCounter = 0;
        static List<Server> ServerNodes = new List<Server>();

        static void AddServer()
        {
            ServerCounter++;
            ServerNodes.Sort();

            Server newServer = new Server(ServerCounter, ServerNodes);
            foreach (Server itServer in ServerNodes) {
                itServer.NotifyNewServer(ServerCounter, newServer);
            }
            ServerNodes.Add(newServer);
        }

        static void RemoveServer()
        {
            Console.WriteLine("Enter serverId:");
            int serverId = Convert.ToInt32(Console.ReadLine());

            // Remove from the master ring
            ServerNodes.RemoveAll(server =>
            {
                return server.GetServerId() == serverId;
            });

            foreach (Server itServer in ServerNodes)
            {
                itServer.NotifyRemovedServer(serverId);
            }
            
        }

        static void ListServers()
        {
            ServerNodes.Sort();

            foreach (Server itServer in ServerNodes) {
                Console.WriteLine(itServer.GetServerSummary());
            }

        }

        static void SendDataToServer()
        {
            Console.WriteLine("Enter destination serverId:");
            int serverId = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Enter data:");
            string data = Console.ReadLine();

            Server destination = ServerNodes.Find(server => 
            {
                return server.GetServerId() == serverId;
            });
            destination.SaveDataInRing(data);
        }

        static void GetDataFromServer()
        {
            Console.WriteLine("Enter source serverId:");
            int serverId = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Enter hash:");
            string data = Console.ReadLine();

            Server source = ServerNodes.Find(server =>
            {
                return server.GetServerId() == serverId;
            });

            string result = source.GetDataFromRing(data);
            if (result != null)
            {
                Console.WriteLine("RESULT: " + result);
            }
            else 
            {
                Console.WriteLine("RESULT NOT FOUND.");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting ConsistentHashing.");

            while (true) {
                Console.WriteLine("Select option:");

                Console.WriteLine("1) Add server.");
                Console.WriteLine("2) Remove server.");
                Console.WriteLine("3) List master ring.");
                Console.WriteLine("4) Send data to server.");
                Console.WriteLine("5) Get data from server.");

                int option = Convert.ToInt32(Console.ReadLine());

                Console.Clear();

                switch (option)
                {
                    case 1:
                        AddServer();
                        break;
                    case 2:
                        RemoveServer();
                        break;
                    case 3:
                        ListServers();
                        break;
                    case 4:
                        SendDataToServer();
                        break;
                    case 5:
                        GetDataFromServer();
                        break;
                    default:
                        Console.WriteLine("Invalid option!");
                        break;
                }
            }
        }
    }
}
