using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributedServiceProvider.Base;
using System.Net;
using System.IO;
using DistributedServiceProvider.MessageConsumers;
using System.Net.Sockets;
using System.Threading;

namespace DistributedServiceProvider.Contacts
{
    public class UdpContact
        :Contact
    {
        public readonly IPAddress Ip;
        public readonly int Port;

        public UdpContact(Identifier512 id, Guid networkId, IPAddress ip, int port)
            :base(id, networkId)
        {
            Ip = ip;
            Port = port;
        }

        public override TimeSpan Ping(Contact source, TimeSpan timeout)
        {
            UdpContact uSource = source as UdpContact;

            using (MemoryStream m = new MemoryStream())
            {
                using(BinaryWriter w =new BinaryWriter(m))
                {
                    w.Write((byte)PacketFlag.Ping);

                    WriteContact(w, this);

                    var callback = localTable.GetConsumer<Callback>(Callback.CONSUMER_ID);
                    var token = callback.AllocateToken();

                    w.Write(IPAddress.HostToNetworkOrder(token.Id));

                    try
                    {
                        SendUdpMessage(m.ToArray(), Ip, Port);
                        
                        DateTime start = DateTime.Now;
                        if (token.Wait(timeout.Milliseconds))
                        {
                            return DateTime.Now - start;
                        }
                        return TimeSpan.MaxValue;
                    }
                    finally
                    {
                        callback.FreeToken(token);
                    }
                }
            }
        }

        private static void WriteContact(BinaryWriter w, UdpContact c)
        {
            byte[] idBytes = c.Identifier.GetBytes().ToArray();
            w.Write(IPAddress.HostToNetworkOrder(idBytes.Length));
            w.Write(idBytes);

            byte[] netIdBytes = c.NetworkId.ToByteArray();
            w.Write(IPAddress.HostToNetworkOrder(netIdBytes.Length));
            w.Write(netIdBytes);

            w.Write(IPAddress.HostToNetworkOrder(c.Port));

            byte[] addrBytes = c.Ip.GetAddressBytes();
            w.Write(IPAddress.HostToNetworkOrder(addrBytes.Length));
            w.Write(addrBytes);
        }

        private static UdpContact ReadContact(BinaryReader reader)
        {
            int idBytesLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            byte[] idBytes = reader.ReadBytes(idBytesLength);
            Identifier512 id = new Identifier512(idBytes);

            int netIdBytesLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            byte[] netIdBytes = reader.ReadBytes(netIdBytesLength);
            Guid netId = new Guid(netIdBytes);

            int port = IPAddress.NetworkToHostOrder(reader.ReadInt32());

            int addrBytesLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            byte[] addrBytes = reader.ReadBytes(addrBytesLength);
            IPAddress address = new IPAddress(addrBytes);

            return new UdpContact(id, netId, address, port);
        }

        public override void Send(Contact source, Guid consumerId, byte[] message, bool reliable = true, bool ordered = true, int channel = 1)
        {
            base.Send(source, consumerId, message, reliable, ordered, channel);

            using(MemoryStream m = new MemoryStream())
            {
                using(BinaryWriter w = new BinaryWriter(m))
                {
                    w.Write((byte)PacketFlag.Data);

                    WriteContact(w, this);

                    byte[] guidBytes = consumerId.ToByteArray();
                    w.Write(IPAddress.HostToNetworkOrder(guidBytes.Length));
                    w.Write(guidBytes);

                    w.Write(IPAddress.HostToNetworkOrder(message.Length));
                    w.Write(message);
                    
                    SendUdpMessage(m.ToArray(), Ip, Port);
                }
            }
        }

        private static DistributedRoutingTable localTable;
        private static bool listen = true;
        private static Thread listenThread;
        private static UdpClient client;
        public static void InitialiseUdp(DistributedRoutingTable localTable, int port)
        {
            UdpContact.localTable = localTable;

            client = new UdpClient(port);

            listenThread = new Thread(() =>
            {
                while (listen)
                {
                    IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);
                    byte[] bytes = client.Receive(ref groupEP);

                    using (MemoryStream m = new MemoryStream(bytes))
                    {
                        using (BinaryReader r = new BinaryReader(m))
                        {
                            PacketFlag f = (PacketFlag)r.ReadByte();

                            switch (f)
                            {
                                case PacketFlag.Ping: ParsePing(r); break;
                                case PacketFlag.Data: ParseData(r); break;
                                default: Console.WriteLine("Unknown packet type " + f); break;
                            }
                        }
                    }
                }
            });
            listenThread.Start();
        }

        private static void ParsePing(BinaryReader reader)
        {
            UdpContact c = ReadContact(reader);

            long tokenId = IPAddress.NetworkToHostOrder(reader.ReadInt64());

            localTable.DeliverPing(c);

            var callback = localTable.GetConsumer<Callback>(Callback.CONSUMER_ID);
            callback.SendResponse(localTable.LocalContact, c, tokenId, new byte[] { 1, 3, 3, 7 });
        }

        private static void ParseData(BinaryReader reader)
        {
            UdpContact source = ReadContact(reader);

            int guidLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            byte[] guidBytes = reader.ReadBytes(guidLength);
            Guid consumerId = new Guid(guidBytes);

            int msgLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            byte[] msg = reader.ReadBytes(msgLength);

            localTable.Deliver(source, consumerId, msg);
        }

        public static void Stop()
        {
            listen = false;
            if (listenThread != null)
                listenThread.Join();
        }

        private static void SendUdpMessage(byte[] msg, IPAddress destination, int port)
        {
            lock (client)
            {
                client.Send(msg, msg.Length, new IPEndPoint(destination, port));
            }
        }

        private enum PacketFlag
            :byte
        {
            Ping = 0,
            Data = 1
        }

        public override string ToString()
        {
            return "{ " + Ip + ":" + Port + " " + base.Identifier + "}";
        }
    }
}
