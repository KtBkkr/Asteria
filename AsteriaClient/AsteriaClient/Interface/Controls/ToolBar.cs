﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface.Controls
{
    public class ToolBar : Control
    {
        #region Fields
        private int row = 0;
        private bool fullRow = false;
        #endregion

        #region Properties
        public virtual int Row
        {
            get { return row; }
            set
            {
                row = value;
                if (row < 0) row = 0;
                if (row > 7) row = 7;
            }
        }

        public virtual bool FullRow
        {
            get { return fullRow; }
            set { fullRow = value; }
        }
        #endregion

        #region Constructors
        public ToolBar(Manager manager)
            : base(manager)
        {
            Left = 0;
            Top = 0;
            Width = 64;
            Height = 24;
            CanFocus = false;
        }
        #endregion

        #region Methods
        public override void Init()
        {
            base.Init();
        }

        protected internal override void InitSkin()
        {
            base.InitSkin();
            Skin = new SkinControl(Manager.Skin.Controls["ToolBar"]);
        }

        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);
        }
        #endregion
    }
}
