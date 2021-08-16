using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ConsistentHashing
{
    class Server : IComparable
    {
        struct RingNode
        {
            public string hashId;
            public Server server;
        }

        private const int VICTIMS_QUANTITY = 3;

        private int serverId;

        private string serverHash;

        // Ring
        private RingNode[] ring;

        // Store
        private Dictionary<string, string> elements;

        public Server(int serverId, List<Server> sortedNodes)
        {
            this.serverId = serverId;
            serverHash = GetHash(serverId.ToString());

            elements = new Dictionary<string, string>();

            ring = new RingNode[sortedNodes.Count + 1]; // Add myself to the ring

            IEnumerator<Server> enumerator = sortedNodes.GetEnumerator();

            bool alreadyAddMyself = false;
            for (var i = 0; i < ring.Length; i++)
            {
                if (!enumerator.MoveNext())
                {
                    ring[i].hashId = serverHash;
                    ring[i].server = this;
                    continue;
                }

                if (!alreadyAddMyself && serverHash.CompareTo(enumerator.Current.GetServerHash()) < 0) 
                {
                    alreadyAddMyself = true;

                    ring[i].hashId = serverHash;
                    ring[i].server = this;
                    i++;
                }

                ring[i].hashId = enumerator.Current.GetServerHash();
                ring[i].server = enumerator.Current;
            }
        }

        private static string GetHash(string input)
        {
            var sha1 = SHA1.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = sha1.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public void NotifyNewServer(int newServerId, Server newServer)
        {
            string hashId = GetHash(newServerId.ToString());

            RingNode[] newRing = new RingNode[ring.Length + 1];

            bool alreadyAdded = false;

            int newRingIndx = 0;

            for (int i = 0; i < ring.Length; i++) 
            {
                if (!alreadyAdded && hashId.CompareTo(ring[i].hashId) < 0)
                {
                    alreadyAdded = true;
                    newRing[newRingIndx].hashId = hashId;
                    newRing[newRingIndx].server = newServer;
                    newRingIndx++;
                }
                newRing[newRingIndx].hashId = ring[i].hashId;
                newRing[newRingIndx].server = ring[i].server;
                newRingIndx++;
            }

            // If new node goes at the end of the ring
            if (!alreadyAdded) 
            {
                newRing[newRingIndx].hashId = hashId;
                newRing[newRingIndx].server = newServer;
            }
            ring = newRing;
        }

        public void NotifyRemovedServer(int removedServerId)
        {
            string hashId = GetHash(removedServerId.ToString());

            RingNode[] newRing = new RingNode[ring.Length - 1];

            bool alreadyRemoved = false;

            int ringIndx = 0;

            for (int i = 0; i < newRing.Length; i++)
            {
                if (!alreadyRemoved && hashId.CompareTo(ring[ringIndx].hashId) == 0)
                {
                    alreadyRemoved = true;
                    ringIndx++;
                }
                newRing[i].hashId = ring[ringIndx].hashId;
                newRing[i].server = ring[ringIndx].server;
                ringIndx++;
            }
            ring = newRing;
        }

        private int FindFirstRingPosition(string hash)
        {
            int indx = 0;

            while (indx < ring.Length && ring[indx].hashId.CompareTo(hash) < 0) 
            {
                indx++;
            }

            // Close the ring at the end
            if (indx == ring.Length) 
            {
                indx = 0;
            }

            return indx;
        }

        public void SaveData(string hash, string data)
        {
            Console.WriteLine("Saving data \"" + data + "\" with hash " + hash + " on server " + serverHash);
            elements.TryAdd(hash, data);
        }

        public void SaveDataInRing(string data)
        {
            string hash = GetHash(data);

            int victimPosition = FindFirstRingPosition(hash);

            for (int i = 0; i < Math.Min(ring.Length, VICTIMS_QUANTITY); i++) 
            {
                int currentPosition = (victimPosition + i) % ring.Length;

                if (ring[currentPosition].hashId.Equals(serverHash))
                {
                    SaveData(hash, data);
                }
                else
                {
                    ring[currentPosition].server.SaveData(hash, data);
                }
            }
        }

        public string GetData(string hash)
        {
            Console.WriteLine("Getting data " + hash + " from server " + serverHash);
            string output;
            elements.TryGetValue(hash, out output);
            return output;
        }

        public string GetDataFromRing(string hash)
        {
            int victimPosition = FindFirstRingPosition(hash);

            string output = null;
            for (int i = 0; i < Math.Min(ring.Length, VICTIMS_QUANTITY); i++)
            {
                int currentPosition = (victimPosition + i) % ring.Length;

                if (ring[currentPosition].hashId.Equals(serverHash))
                {
                    output = GetData(hash);
                }
                else
                {
                    output = ring[currentPosition].server.GetData(hash);
                }
                if (output != null)
                {
                    break;
                }
            }
            return output;
        }

        public void ListServers()
        {
            for (int i = 0; i < ring.Length; i++)
            {
                Console.WriteLine(ring[i].server.GetServerSummary());
            }
        }

        public string GetServerSummary()
        {
            return serverId + " : " + serverHash + " : " + elements.Count;
        }

        public int GetServerId()
        {
            return serverId;
        }

        public string GetServerHash()
        {
            return serverHash;
        }

        public int CompareTo(object server)
        {
            if (typeof(Server).IsInstanceOfType(server))
            {
                return serverHash.CompareTo(((Server) server).serverHash);
            }
            throw new ArgumentException();
        }

    }
}
