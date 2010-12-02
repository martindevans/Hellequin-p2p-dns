using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HandyCollections.MerkleHashTree
{
    /// <summary>
    /// A node which contains some data which is passed through a hash function to add to a merkle tree
    /// </summary>
    /// <typeparam name="D">The type of the data</typeparam>
    /// <typeparam name="H">the type of the hash</typeparam>
    public class DataNode<D, H>
        :Node<H>
    {
        /// <summary>
        /// Gets the hash of this data
        /// </summary>
        /// <value>The hash.</value>
        public override H Hash
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the data
        /// </summary>
        /// <value>The data.</value>
        public D Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataNode&lt;D, H&gt;"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="hashFunction">The hash function.</param>
        public DataNode(D data, Func<D, H> hashFunction)
        {
            Hash = hashFunction(data);
            Data = data;
        }
    }
}
