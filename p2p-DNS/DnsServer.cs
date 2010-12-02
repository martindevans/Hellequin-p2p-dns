using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ARSoft.Tools.Net.Dns;
using System.Net.Sockets;
using System.Threading;

namespace p2p_DNS
{
    class DnsServer
        :IDisposable
    {
        ARSoft.Tools.Net.Dns.DnsServer server;
        IDictionary<string, DomainMapping> mappingLookup;

        Thread lookupThread;

        public DnsServer(IDictionary<string, DomainMapping> mappingLookup)
        {
            this.mappingLookup = mappingLookup;

            server = new ARSoft.Tools.Net.Dns.DnsServer(IPAddress.Any, 10, 10, ProcessQuery);
            server.ExceptionThrown += (o, e) =>
                {
                    Console.WriteLine("DnsServer threw an exception " + o + " " + e);
                };
        }

        public void Start()
        {
            server.Start();
        }

        public void Stop()
        {
            server.Stop();
        }

        private DnsMessage ProcessQuery(DnsMessageBase queryBase, IPAddress clientAddress, ProtocolType protocol)
        {
            DnsMessage query = queryBase as DnsMessage;

            foreach (DnsQuestion q in query.Questions)
            {
                if (q.RecordType == RecordType.A)
                {
                    if (!q.Name.EndsWith(".p2p"))
                    {
                        query.ReturnCode = ReturnCode.Refused;
                        return query;
                    }

                    Console.WriteLine("DNS LOOKUP: " + q.Name);

                    DomainMapping mapping;
                    if (!mappingLookup.TryGetValue(q.Name, out mapping))
                    {
                        query.ReturnCode = ReturnCode.ServerFailure;
                        return query;
                    }

                    query.AnswerRecords.Add(new ARecord(mapping.Name, mapping.TimeToLive.Seconds, mapping.Address));
                    return query;
                }
                else
                    query.ReturnCode = ReturnCode.NotImplemented;
            }

            return query;
        }


        public void Dispose()
        {
            ((IDisposable)server).Dispose();
        }
    }
}
