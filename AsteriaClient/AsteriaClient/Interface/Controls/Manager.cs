using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteriaClient.Interface.Controls
{
    /// <summary>
    /// TODO: [MID]
    /// </summary>
    public class Manager : DrawableGameComponent
    {
        private struct ControlStates
        {
            public Control[] Buttons;
            public int Click;
            public Control Over;
        }

        #region Consts 
        internal Version _SkinVersion = new Version(0, 7);
        internal Version _LayoutVersion = new Version(0, 7);
        internal const string _SkinDirectory = ".\\Content\\Skins\\";
        internal const string _LayoutDirectory = ".\\Content\\Layout\\";
        internal const string _DefaultSkin = "Default";
        internal const string _SkinExtension = ".skin";
        internal const int _MenuDelay = 500;
        internal const int _ToolTipDelay = 500;
        internal const int _DoubleClickTime = 500;
        internal const int _TextureResizeIncrement = 32;
        internal const RenderTargetUsage _RenderTargetUsage = RenderTargetUsage.DiscardContents;
        #endregion

        #region Fields
        private bool deviceReset = false;
        private RenderTarget2D renderTarget = null;
        private int targetFrames = 60;
        private long drawTime = 0;
        private long updateTime = 0;
        private GraphicsDeviceManager graphics = null;
        private ArchiveManager content = null;
        private Renderer renderer = null;
        #endregion
    }
}
