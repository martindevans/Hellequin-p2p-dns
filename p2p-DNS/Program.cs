using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Concurrent;
using DistributedServiceProvider;
using DistributedServiceProvider.Base;
using DistributedServiceProvider.Contacts;

namespace p2p_DNS
{
    class Program
    {
        static int dnsport = 53;
        static int peerport = 12001;
        static IPAddress LocalIp;
        static Guid networkId = Guid.Parse("f35303bc-f334-4e68-b949-28943ff96955");
        static Identifier512 routingIdentifier = Identifier512.NewIdentifier();

        static ConcurrentDictionary<string, IPAddress> myDomainMappings;
        static DnsServer dnsServer;

        static DistributedRoutingTable routingTable;

        static void Main(string[] args)
        {
            myDomainMappings = ReadDomainMappings("MyDomainMappings.txt");
            ReadSettings(ref dnsport, ref peerport);

            dnsServer = new DnsServer(new Dictionary<string, DomainMapping>()
                {
                    { "hellequin.p2p", new DomainMapping() { Address = IPAddress.Parse("78.105.97.103"), Name = "hellequin.p2p", TimeToLive = TimeSpan.FromSeconds(1234) }}
                });
            dnsServer.Start();

            Identifier512 myId = Identifier512.NewIdentifier();
            routingTable = new DistributedRoutingTable(Identifier512.NewIdentifier(), (a) => new UdpContact(a.LocalIdentifier, networkId, LocalIp, peerport), networkId, new Configuration());

            UdpContact.InitialiseUdp(routingTable, peerport);

            Console.WriteLine("Bootstrapping DHT");
            routingTable.Bootstrap(LoadBootstrapData());

            Console.WriteLine("Bootstrap finished");
            Console.WriteLine("There are " + routingTable.ContactCount + " Contacts");
            
            Console.WriteLine("Press any key to close");
            Console.ReadLine();

            UdpContact.Stop();
        }

        private static IEnumerable<Contact> LoadBootstrapData()
        {
            foreach (var line in File.ReadAllLines("Bootstrap.txt").OmitComments("#", "//"))
            {
                UdpContact udpC = null;

                try
                {
                    string[] split = line.Split(' ');
                    Guid a = Guid.Parse(split[0]);
                    Guid b = Guid.Parse(split[1]);
                    Guid c = Guid.Parse(split[2]);
                    Guid d = Guid.Parse(split[3]);

                    Identifier512 id = new Identifier512(a, b, c, d);

                    IPAddress ip = IPAddress.Parse(split[4]);
                    int port = Int32.Parse(split[5]);

                    udpC = new UdpContact(id, networkId, ip, port);

                    Console.WriteLine("Loaded bootstrap contact " + udpC);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception parsing bootstrap file: " + e);
                }

                if (udpC != null)
                    yield return udpC;
            }
        }

        private static void ReadSettings(ref int dnsport, ref int peerport)
        {
            foreach (var line in File.ReadAllLines("Settings.txt").OmitComments("#", "//").Select(a => a.ToLowerInvariant().Replace(" ", "").Replace("\t", "").Split('=')))
            {
                switch (line[0])
                {
                    case "dnsport": dnsport = Int32.Parse(line[1]); break;
                    case "peerport": peerport = Int32.Parse(line[1]); break;
                    case "localip": LocalIp = IPAddress.Parse(line[1]); break;
                    case "networkid": networkId = Guid.Parse(line[1]); break;
                    case "routingidentifier":
                        var s = line[1].Split(',');
                        routingIdentifier = new Identifier512(Guid.Parse(s[0]), Guid.Parse(s[1]), Guid.Parse(s[2]), Guid.Parse(s[3]));
                        break;
                    default: Console.WriteLine("Unknown setting " + line[0]); break;
                }
            }
        }

        private static ConcurrentDictionary<string, IPAddress> ReadDomainMappings(string filepath)
        {
            var result = new ConcurrentDictionary<string, IPAddress>();

            foreach (var line in File.ReadAllLines(filepath).OmitComments("#", "//"))
            {
                string[] splitBits = line.Replace(" ", "").Split('=');

                if (splitBits.Length > 2)
                    Console.WriteLine("Trying to assign more than one IP to a single domain? \"" + line + "\"");
                else if (splitBits.Length < 2)
                {
                    Console.WriteLine("Ignoring \"" + line + "\"");
                    continue;
                }

                IPAddress ip;
                if (!IPAddress.TryParse(splitBits[1], out ip))
                    Console.WriteLine("Cannot parse \"" + splitBits[1] + "\" into an IPAddress");
                else
                {
                    Console.WriteLine("Mapping " + splitBits[0] + " to " + ip);
                    result[splitBits[0].ToLowerInvariant()] = IPAddress.Parse(splitBits[1]);
                }
            }

            return result;
        }
    }
}
