using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HandyCollections.BinaryTree
{
    /// <summary>
    /// A Binary search tree with support for tree rotations
    /// </summary>
    /// <typeparam name="K">Type of keys</typeparam>
    /// <typeparam name="V">Type of values</typeparam>
    public class BinaryTree<K, V>
    {
        #region fields and properties
        public Node Root
        {
            get;
            private set;
        }

        private IComparer<K> comparer;
        /// <summary>
        /// The comparer to use for items in this collection. Changing this comparer will trigger a heapify operation
        /// </summary>
        public IComparer<K> Comparer
        {
            get
            {
                return comparer;
            }
        }
        #endregion

        #region constructors
        public BinaryTree()
            :this(Comparer<K>.Default)
        {

        }

        public BinaryTree(IComparer<K> comparer)
        {
            this.comparer = comparer;
        }
        #endregion

        #region add/remove
        public virtual Node Add(K key, V value)
        {
            Node n = CreateNode(key, value);

            bool duplicate;
            var v = FindParent(n.Key, out duplicate);

            if (duplicate)
                throw new ArgumentException("Duplicate keys not allowed");

            if (v.Key == null)
                return (Root = n);

            SetChild(v.Key, n, v.Value);

            return n;
        }

        public V Remove(K key)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region search
        /// <summary>
        /// Finds the parent for inserting this key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private KeyValuePair<Node, bool> FindParent(K key, out bool duplicate)
        {
            duplicate = false;
            Node r = Root;

            if (r == null)
                return new KeyValuePair<Node, bool>(null, false);

            while (true)
            {
                if (IsLessThan(key, r.Key))
                {
                    if (r.Left == null)
                        return new KeyValuePair<Node, bool>(r, true);

                    r = r.Left;
                }
                else if (IsEqual(key, r.Key))
                {
                    duplicate = true;
                    return new KeyValuePair<Node, bool>(r, false);
                }
                else
                {
                    if (r.Right == null)
                        return new KeyValuePair<Node, bool>(r, false);

                    r = r.Right;
                }
            }
        }

        public virtual Node Find(K key)
        {
            bool duplicate;
            var v = FindParent(key, out duplicate);

            if (!duplicate)
                throw new KeyNotFoundException("No such key in this tree");

            if (v.Key == null)
            {
                if (Root == null)
                    throw new InvalidOperationException("There are no nodes in this tree");

                return Root;
            }

            if (IsEqual(v.Key.Key, key))
                return v.Key;
            else
                throw new KeyNotFoundException("No such key in this tree");
        }
        #endregion

        #region tree rotation
        public void Rotate(Node pivot, bool rotateRight) //http://webdocs.cs.ualberta.ca/~holte/T26/tree-rotation.html
        {
            Node pivotParent = pivot.Parent;
            bool parentLeftSide = pivotParent != null ? pivotParent.Left == pivot : true;

            Node rotator = rotateRight ? pivot.Left : pivot.Right;
            Node otherSubtree = rotateRight ? pivot.Right : pivot.Left;
            Node insideSubtree = rotator == null ? null : rotateRight ? rotator.Right : rotator.Left;
            Node outsideSubtree = rotator == null ? null : rotateRight ? rotator.Left : rotator.Right;

            SetChild(pivot, null, rotateRight);
            if (pivotParent != null)
                SetChild(pivotParent, null, parentLeftSide);
            if (rotator != null)
                SetChild(rotator, null, !rotateRight);

            SetChild(pivot, insideSubtree, rotateRight);
            if (rotator != null)
                SetChild(rotator, pivot, !rotateRight);
            if (pivotParent != null)
                SetChild(pivotParent, rotator, parentLeftSide);
            else if (rotator != null)
                Root = rotator;
            else
                Root = pivot;
        }
        #endregion

        #region helpers
        private bool IsGreaterThan(K a, K b)
        {
            return Comparer.Compare(a, b) > 0;
        }

        private bool IsLessThan(K a, K b)
        {
            return Comparer.Compare(a, b) < 0;
        }

        private bool IsEqual(K a, K b)
        {
            return Comparer.Compare(a, b) == 0;
        }

        private static Node CreateNode(K key, V value)
        {
            return new Node(key, value);
        }

        private static void SetChild(Node parent, Node child, bool left)
        {
            if (left)
                parent.Left = child;
            else
                parent.Right = child;
        }

        private static void PrintSubtree(Node root, string indent)
        {
            if (root == null)
                Console.WriteLine(indent + "-> null");
            else
            {
                Console.WriteLine(indent + "->" + root);
                PrintSubtree(root.Left, indent + "    |");
                PrintSubtree(root.Right, indent + "    |");
            }
        }
        #endregion

        public class Node
        {
            public readonly K Key;
            public readonly V Value;

            private Node parent;
            public Node Parent
            {
                get
                {
                    return parent;
                }
                protected internal set
                {
                    if (parent != null)
                    {
                        var p = parent;
                        parent = null;
                        if (parent.Left == this)
                            parent.Left = null;
                        else if (parent.Right == this)
                            parent.Right = null;
                        else
                            throw new InvalidOperationException("parent of this node does not count this node as it's child");
                    }

                    parent = value;
                }
            }

            private Node left;
            public Node Left
            {
                get
                {
                    return left;
                }
                protected internal set
                {
                    SetChild(value, ref left);
                }
            }

            private Node right;
            public Node Right
            {
                get
                {
                    return right;
                }
                protected internal set
                {
                    SetChild(value, ref right);
                }
            }

            private void SetChild(Node value, ref Node field)
            {
                if (value != null && value.Parent != null)
                    throw new ArgumentException("Parent must be null");

                if (field != null)
                    field.parent = null;

                field = value;

                if (field != null)
                    field.Parent = this;
            }

            public bool IsRoot
            {
                get
                {
                    return Parent == null;
                }
            }

            public bool IsLeftChild
            {
                get
                {
                    if (Parent == null)
                        throw new InvalidOperationException("Parent cannot be null");
                    return Parent.Left == this;
                }
            }

            public bool IsRightChild
            {
                get
                {
                    if (Parent == null)
                        throw new InvalidOperationException("Parent cannot be null");
                    return Parent.Right == this;
                }
            }

            protected internal Node(K key, V value)
            {
                Key = key;
                Value = value;
            }

            public override string ToString()
            {
                return "Node " + new KeyValuePair<K, V>(Key, Value).ToString();
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is K)
                    return ((K)obj).Equals(Key);
                return false;
            }
        }
    }
}
