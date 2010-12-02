using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HandyCollections.MerkleHashTree
{
    /// <summary>
    /// base class for Merkle hash tree nodes
    /// </summary>
    /// <typeparam name="H"></typeparam>
    public abstract class Node<H>
    {
        /// <summary>
        /// Gets the hash from this node
        /// </summary>
        /// <value>The hash.</value>
        public abstract H Hash { get; protected set; }
    }
}
