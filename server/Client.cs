using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace server
{
    public class Client
    {
        public static int dataBufferSize = 4096;
        public int id;
        public Player player;
        public TCP tcp;
        public UDP udp;

        private static List<Vector3> coordPos = new List<Vector3>();

        public Client(int clientId)
        {
            this.id = clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
            coordPos.Add(new Vector3(-12, 6, 0));
            coordPos.Add(new Vector3(12, 6, 0));
            coordPos.Add(new Vector3(-12, -6, 0));
            coordPos.Add(new Vector3(12, -6, 0));
        }
        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;

                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome from the server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    Console.WriteLine($"ByteL: {byteLength}");

                    if (byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {ex}");
                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Console.WriteLine($"packetId: {packetId}");

                            Server.packetHandlers[packetId](id, packet);
                        }
                    });

                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }
        public class UDP
        {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int id)
            {
                this.id = id;
            }

            public void Connect(IPEndPoint endPoint)
            {
                this.endPoint = endPoint;
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packetData)
            {
                int packetLength = packetData.ReadInt();
                byte[] packetBytes = packetData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });
            }
            public void Disconnect()
            {
                endPoint = null;
            }
        }
        public void SendIntoGame(string playerName)
        {
            player = new Player(id, playerName, coordPos[id - 1]);

            // spawn a new player on other clients
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    if (client.id != id)
                    {
                        ServerSend.SpawnPlayer(id, client.player);
                    }
                }
            }

            // spawn all players on a new player client
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    ServerSend.SpawnPlayer(client.id, player);
                }
            }
        }

        public void SendBulletIntoGame(Vector3 pos, Vector2 vel)
        {
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    if (client.id != id)
                    {
                        //Console.WriteLine($"Spawning bullet with VEL: x:{(vel * player.bulletSpeedMultiplier).X}, Y: {(vel * player.bulletSpeedMultiplier).Y}. OGVEL: X:{vel.X}, Y:{vel.Y}");
                        ServerSend.SpawnBullet(client.id, pos, vel*player.bulletSpeedMultiplier);
                    }
                }
            }

        }
        public void SendHitIntoGame(int hitPlayerId)
        {
            Player hitPlayer = Server.clients[hitPlayerId].player;
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    ServerSend.PlayerHealth(client.id, hitPlayer);
                }
            }

        }

        public void RemoveBoostFromGame(int boostId, Vector3 boostPos)
        {
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    ServerSend.HandleBoost(client.id, false, boostId, boostPos);//Tell client to remove a boost
                }
            }
        }


        /// <summary>Disconnects the client and stops all network traffic.</summary>
        private void Disconnect()
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                player = null;
            });

            tcp.Disconnect();
            udp.Disconnect();

            ServerSend.PlayerDisconnected(id);
            int connectedClients = Server.ClientsCount();
            if (connectedClients == 1 || connectedClients == 0) Server.StopRound();
        }
    }
}
