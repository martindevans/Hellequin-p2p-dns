using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HandyCollections.MerkleHashTree
{
    /// <summary>
    /// A node which contains a hash in a merkle hash tree (ie. Not a data/leaf node)
    /// </summary>
    /// <typeparam name="H"></typeparam>
    public class HashNode<H>
        :Node<H>
        where H : IEquatable<H>
    {
        #region fields
        private bool leftSetDeferred = false;
        private Node<H> left;
        /// <summary>
        /// Gets or sets the left node
        /// </summary>
        /// <value>The left.</value>
        /// <exception cref="ArgumentException">Thrown if the child nodes do not form a valid hash tree</exception>
        public Node<H> Left
        {
            get { return left; }
            private set
            {
                SetChildNode(ref left, ref leftSetDeferred, ref rightSetDeferred, value);
            }
        }

        private bool rightSetDeferred = false;
        private Node<H> right;
        /// <summary>
        /// Gets or sets the right node
        /// </summary>
        /// <value>The right.</value>
        /// <exception cref="ArgumentException">Thrown if the child nodes do not form a valid hash tree</exception>
        public Node<H> Right
        {
            get { return right; }
            private set
            {
                SetChildNode(ref right, ref rightSetDeferred, ref leftSetDeferred, value);
            }
        }

        /// <summary>
        /// Gets the hash from this node
        /// </summary>
        /// <value>The hash.</value>
        public override H Hash
        {
            get;
            protected set;
        }

        private Func<H, H, H> combineHashes;
        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="HashNode&lt;H&gt;"/> class.
        /// </summary>
        /// <param name="hashValue">The hash value.</param>
        /// <param name="combineHashes">A function which combines the hashes of two child nodes into one new hash</param>
        public HashNode(H hashValue, Func<H, H, H> combineHashes)
        {
            Hash = hashValue;
            this.combineHashes = combineHashes;
        }
        #endregion

        private void SetChildNode(ref Node<H> val, ref bool deferredSet, ref bool otherDeferredSet, Node<H> newValue)
        {
            deferredSet = newValue == null;
            val = newValue;

            if (deferredSet && otherDeferredSet)
                if (!CheckChildren())
                {
                    val = null;
                    deferredSet = false;
                    throw new ArgumentException("Child nodes do not form a valid hash tree");
                }
        }

        private bool CheckChildren()
        {
            return combineHashes(left.Hash, right.Hash).Equals(Hash);
        }
    }
}
