using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Data
{
    public class ObjectPoolSafe<T> where T : new()
    {
        #region Delegates
        public delegate T ObjectCreator();
        #endregion

        #region Fields
        private Stack<T> pool = new Stack<T>();
        private object locker = new object();
        private ObjectCreator creator;
        #endregion

        #region Constructors
        public ObjectPoolSafe()
        {
            this.creator = CreateDefaultInstance;
        }

        public ObjectPoolSafe(ObjectCreator objectCreator)
        {
            this.creator = objectCreator;
        }
        #endregion

        #region Methods
        private T CreateDefaultInstance()
        {
            return new T();
        }

        public T Get()
        {
            lock (locker)
            {
                if (pool.Count == 0)
                    return creator();
                else
                    return pool.Pop();
            }
        }

        public void Free(T item)
        {
            lock (locker)
            {
                if (pool.Contains(item))
                    throw new InvalidOperationException("Object already returned to pool!");

                pool.Push(item);
            }
        }

        public void EmptyPool()
        {
            lock (locker)
            {
                pool.Clear();
            }
        }
        #endregion
    }

    public class ObjectPool<T> where T : new()
    {
        #region Delegates
        public delegate T ObjectCreator();
        #endregion

        #region Fields
        private Stack<T> pool = new Stack<T>();
        private ObjectCreator creator;
        #endregion

        #region Constructors
        public ObjectPool()
        {
            this.creator = CreateDefaultInstance;
        }

        public ObjectPool(ObjectCreator objectCreator)
        {
            this.creator = objectCreator;
        }
        #endregion

        #region Methods
        private T CreateDefaultInstance()
        {
            return new T();
        }

        public T Get()
        {
            if (pool.Count == 0)
                return creator();
            else
                return pool.Pop();
        }

        public void Free(T item)
        {
            if (pool.Contains(item))
                throw new InvalidOperationException("Object already returned to pool!");

            pool.Push(item);
        }

        public void EmptyPool()
        {
            pool.Clear();
        }
        #endregion
    }
}
