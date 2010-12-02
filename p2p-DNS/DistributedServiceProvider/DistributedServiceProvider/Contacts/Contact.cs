using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using DistributedServiceProvider.Base;
using DistributedServiceProvider.Contacts;
using System.Threading;

namespace DistributedServiceProvider.Contacts
{
    /// <summary>
    /// A contact point for a remote routing table
    /// </summary>
    [ProtoContract, ProtoInclude(3, typeof(LocalContact)), ProtoInclude(4, typeof(UdpContact))]
    public abstract class Contact
    {
        /// <summary>
        /// The identifier of the remote routing table
        /// </summary>
        [ProtoMember(1)]
        public readonly Identifier512 Identifier;

        [ProtoMember(2)]
        private byte[] networkIdBytes;
        /// <summary>
        /// The id of the network which the routing table oeprates on
        /// </summary>
        public Guid NetworkId
        {
            get
            {
                return new Guid(networkIdBytes);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="identifier">The identifier of the DistributedRoutingTable this contact represents</param>
        /// <param name="networkId">The network id.</param>
        public Contact(Identifier512 identifier, Guid networkId)
        {
            Identifier = identifier;
            networkIdBytes = networkId.ToByteArray();
        }

        protected Contact()
        {

        }

        private static long sentBytes;
        public static long SentBytes
        {
            get
            {
                return Interlocked.Read(ref sentBytes);
            }
        }
        /// <summary>
        /// Sends a message to the consumer with the given Id
        /// </summary>
        /// <param name="consumerId">The consumer id.</param>
        /// <param name="message">The message.</param>
        /// <returns>The response fromthe remote consumer, or null if there was no response</returns>
        public virtual void Send(Contact source, Guid consumerId, byte[] message, bool reliable = true, bool ordered = true, int channel = 1)
        {
            Interlocked.Add(ref sentBytes, message.Length);
        }

        /// <summary>
        /// Pings this instance.
        /// </summary>
        /// <returns>The response time, or Timespan.MaxValue if it timed out</returns>
        public abstract TimeSpan Ping(Contact source, TimeSpan timeout);
    }
}
