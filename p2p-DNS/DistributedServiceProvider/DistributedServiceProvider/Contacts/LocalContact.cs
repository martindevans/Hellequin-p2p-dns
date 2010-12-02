using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributedServiceProvider.Base;
using System.Diagnostics;
using ProtoBuf;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedServiceProvider.Contacts
{
    [ProtoContract]
    public class LocalContact
        :Contact
    {
        private static ConcurrentDictionary<Identifier512, ConcurrentDictionary<Guid, DistributedRoutingTable>> tables = new ConcurrentDictionary<Identifier512, ConcurrentDictionary<Guid, DistributedRoutingTable>>();

        public DistributedRoutingTable table
        {
            get
            {
                var t = tables[base.Identifier];
                return t[base.NetworkId];
            }
        }

        public LocalContact(DistributedRoutingTable table)
            :base(table.LocalIdentifier, table.NetworkId)
        {
            tables.GetOrAdd(table.LocalIdentifier, new ConcurrentDictionary<Guid, DistributedRoutingTable>())
                .AddOrUpdate(table.NetworkId, table, (a, b) => table);
        }

        public LocalContact()
            :base()
        {

        }


        public override void Send(Contact source, Guid consumerId, byte[] message, bool reliable = true, bool ordered = true, int channel = 1)
        {
            table.Deliver(source, consumerId, message);
        }

        public override TimeSpan Ping(Contact source, TimeSpan timeout)
        {
            table.DeliverPing(source);

            return TimeSpan.FromMilliseconds(1);
        }

        public static void Clear()
        {
            tables.Clear();
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is LocalContact)
            {
                return (obj as LocalContact).Identifier.Equals(Identifier);
            }
            else
                return false;
        }
    }
}
