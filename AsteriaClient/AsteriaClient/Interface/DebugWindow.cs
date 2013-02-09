using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface.Controls;

namespace AsteriaClient.Interface
{
    public class DebugWindow : Window
    {
        #region Fields
        Context context;
        #endregion

        #region Constructors
        public DebugWindow(Context context, Manager manager)
            : base(manager)
        {
            this.context = context;

            this.Width = 600;
            this.Height = 400;
            this.Text = "Debug";
        }
        #endregion

        #region Methods
        #endregion
    }
}
