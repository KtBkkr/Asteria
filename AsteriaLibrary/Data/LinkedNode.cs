using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Data
{
    /// <summary>
    /// Represents a node stored in a LinkedList
    /// </summary>
    public class LinkedNode<T>
    {
        #region Fields
        private T nodeValue;
        private LinkedNode<T> next;
        private LinkedNode<T> previous;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the nodes value.
        /// </summary>
        public T Value { get { return nodeValue; } }

        /// <summary>
        /// Returns the next node in the list or null.
        /// </summary>
        public LinkedNode<T> Next
        {
            get { return next; }
            internal set { next = value; }
        }

        /// <summary>
        /// Returns the previous node in the list or null.
        /// </summary>
        public LinkedNode<T> Previous
        {
            get { return previous; }
            internal set { previous = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new LinkedNode instance.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="next"></param>
        internal LinkedNode(T value, LinkedNode<T> next)
        {
            this.nodeValue = value;
            this.next = next;
            this.previous = null;
        }

        /// <summary>
        /// Creates a new LinkedNode instance.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="previous"></param>
        /// <param name="next"></param>
        internal LinkedNode(T value, LinkedNode<T> previous, LinkedNode<T> next)
        {
            this.nodeValue = value;
            this.next = next;
            this.previous = previous;
        }
        #endregion
    }
}
