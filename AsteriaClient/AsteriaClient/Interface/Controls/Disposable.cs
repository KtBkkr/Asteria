using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    public abstract class Disposable : Unknown, IDisposable
    {
        #region Variables
        private static int count = 0;
        #endregion

        #region Properties
        public static int Count { get { return count; } }
        #endregion

        #region Constructors
        protected Disposable()
        {
            count += 1;
        }

        ~Disposable()
        {
            Dispose(false);
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                count -= 1;
        }
        #endregion
    }
}
