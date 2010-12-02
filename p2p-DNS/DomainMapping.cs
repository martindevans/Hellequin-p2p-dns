using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace p2p_DNS
{
    class DomainMapping
    {
        public string Name { get; set; }

        public TimeSpan TimeToLive { get; set; }

        public System.Net.IPAddress Address { get; set; }
    }
}
