using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface.Controls
{
    public class Component : Disposable
    {
        #region Variables
        private Manager manager = null;
        private bool initialized = false;
        #endregion

        #region Properties
        public virtual Manager Manager
        {
            get { return manager; }
            set { manager = value; }
        }

        public virtual bool Initialized
        {
            get { return initialized; }
        }
        #endregion

        #region Constructors
        public Component(Manager manager)
        {
            if (manager != null)
                this.manager = manager;
            else
                throw new Exception("Component couldn't be created. Manager instance is needed.");
        }
        #endregion

        #region Methods
        public virtual void Init()
        {
            initialized = true;
        }

        protected internal virtual void Update(GameTime gameTime)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
