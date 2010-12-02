using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributedServiceProvider.Base;

namespace DistributedServiceProvider.Contacts
{
    public class UdpContact
        :Contact
    {
        public readonly Identifier512 LocalContact;

        public UdpContact(Identifier512 localContact)
        {
            LocalContact = localContact;
        }
        public override TimeSpan Ping(Contact source, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}
