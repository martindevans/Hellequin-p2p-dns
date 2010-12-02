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
            Console.WriteLine("My Routing Identifier is " + myId.ToString());
            routingTable = new DistributedRoutingTable(Identifier512.NewIdentifier(), (a) => new UdpContact(a.LocalIdentifier), Guid.Parse("f35303bc-f334-4e68-b949-28943ff96955"), new Configuration());

            Console.WriteLine("Press any key to close");
            Console.ReadLine();
        }

        private static void ReadSettings(ref int dnsport, ref int peerport)
        {
            foreach (var line in File.ReadAllLines("Settings.txt").OmitComments("#", "//").Select(a => a.ToLowerInvariant().Replace(" ", "").Split('=')))
            {
                switch (line[0])
                {
                    case "dnsport": dnsport = Int32.Parse(line[1]); break;
                    case "peerport": peerport = Int32.Parse(line[1]); break;
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
