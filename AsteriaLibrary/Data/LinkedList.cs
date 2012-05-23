using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Data
{
    /// <summary>
    /// Simple collection of LinkedNode instances.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkedList<T>
    {
        #region Fields
        private object locker = new object();
        private List<LinkedNode<T>> nodes;
        #endregion

        #region Properties
        public LinkedNode<T> First
        {
            get
            {
                if (nodes.Count > 0)
                    return nodes[0];
                else
                    return null;
            }
        }

        public LinkedNode<T> Last
        {
            get
            {
                lock (locker)
                {
                    return nodes[nodes.Count - 1];
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new LinkedList instance.
        /// </summary>
        /// <param name="initialSize"></param>
        public LinkedList(int initialSize)
        {
            nodes = new List<LinkedNode<T>>(initialSize);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new node to the list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public LinkedNode<T> Add(T item)
        {
            LinkedNode<T> previous = null;
            LinkedNode<T> node = new LinkedNode<T>(item, null);

            lock (locker)
            {
                if (nodes.Count > 0)
                {
                    previous = nodes[nodes.Count - 1]; // this is the last node in the list.
                    previous.Next = node;
                }
                node.Previous = previous;
                nodes.Add(node);
            }
            return node;
        }

        /// <summary>
        /// Removes a node from the list.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public LinkedNode<T> Remove(LinkedNode<T> node)
        {
            lock (locker)
            {
                if (node.Previous != null)
                    node.Previous.Next = node.Next;

                if (node.Next != null)
                    node.Next.Previous = node.Previous;

                nodes.Remove(node);
            }
            return node;
        }
        #endregion
    }
}
