using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace AsteriaClient.Interface.Controls
{
    #region Structs
    public struct SkinStates<T>
    {
        #region Fields
        public T Enabled;
        public T Hovered;
        public T Pressed;
        public T Focused;
        public T Disabled;
        #endregion

        #region Constructors
        public SkinStates(T enabled, T hovered, T pressed, T focused, T disabled)
        {
            Enabled = enabled;
            Hovered = hovered;
            Pressed = pressed;
            Focused = focused;
            Disabled = disabled;
        }
        #endregion
    }

    public struct LayerStates
    {
        public int Index;
        public Color Color;
        public bool Overlay;
    }

    public struct LayerOverlays
    {
        public int Index;
        public Color Color;
    }

    public struct SkinInfo
    {
        public string Name;
        public string Description;
        public string Author;
        public string Version;
    }
    #endregion

    public class SkinList<T> : List<T>
    {
        #region Indexers
        public T this[string index]
        {
            get
            {
                for(int i = 0; i < this.Count; i++)
                {
                    SkinBase s = (SkinBase)(object)this[i];
                    if(s.Name.ToLower() == index.ToLower())
                        return this[i];
                }
                return default(T);
            }

            set
            {
                for(int i = 0; i < this.Count; i++)
                {
                    SkinBase s = (SkinBase)(object)this[i];
                    if(s.Name.ToLower() == index.ToLower())
                        this[i] = value;
                }
            }
        }
        #endregion

        #region Constructors
        public SkinList() : base() { }

        public SkinList(SkinList<T> source)
            : base()
        {
            for (int i = 0; i < source.Count; i++)
            {
                Type[] t = new Type[1];
                t[0] = typeof(T);

                object[] p = new object[1];
                p[0] = source[i];

                this.Add((T)t[0].GetConstructor(t).Invoke(p));
            }
        }
        #endregion
    }

    public class SkinBase
    {
        #region Fields
        public string Name;
        public bool Archive;
        #endregion

        #region Constructors
        public SkinBase() : base()
        {
            Archive = false;
        }

        public SkinBase(SkinBase source)
            : base()
        {
            if (source != null)
            {
                this.Name = source.Name;
                this.Archive = source.Archive;
            }
        }
        #endregion
    }

    public class SkinLayer : SkinBase
    {
        #region Fields
        public SkinImage Image = new SkinImage();
        public int Width;
        public int Height;
        public int OffsetX;
        public int OffsetY;
        public Alignment Alignment;
        public Margins SizingMargins;
        public Margins ContentMargins;
        public SkinStates<LayerStates> States;
        public SkinStates<LayerOverlays> Overlays;
        public SkinText Text = new SkinText();
        public SkinList<SkinAttribute> Attributes = new SkinList<SkinAttribute>();
        #endregion

        #region Constructors
        public SkinLayer()
            : base()
        {
            States.Enabled.Color = Color.White;
            States.Pressed.Color = Color.White;
            States.Focused.Color = Color.White;
            States.Hovered.Color = Color.White;
            States.Disabled.Color = Color.White;

            Overlays.Enabled.Color = Color.White;
            Overlays.Pressed.Color = Color.White;
            Overlays.Focused.Color = Color.White;
            Overlays.Hovered.Color = Color.White;
            Overlays.Disabled.Color = Color.White;
        }

        public SkinLayer(SkinLayer source)
            : base(source)
        {
            if (source != null)
            {
                this.Image = new SkinImage(source.Image);
                this.Width = source.Width;
                this.Height = source.Height;
                this.OffsetX = source.OffsetX;
                this.OffsetY = source.OffsetY;
                this.Alignment = source.Alignment;
                this.SizingMargins = source.SizingMargins;
                this.ContentMargins = source.ContentMargins;
                this.States = source.States;
                this.Overlays = source.Overlays;
                this.Text = new SkinText(source.Text);
                this.Attributes = new SkinList<SkinAttribute>(source.Attributes);
            }
            else
            {
                throw new Exception("Parameter for SkinLayer copy constructor cannot be null.");
            }
        }
        #endregion
    }

    public class SkinText : SkinBase
    {
        #region Fields
        public SkinFont Font;
        public int OffsetX;
        public int OffsetY;
        public Alignment Alignment;
        public SkinStates<Color> Colors;
        #endregion

        #region Constructors
        public SkinText()
            : base()
        {
            Colors.Enabled = Color.White;
            Colors.Pressed = Color.White;
            Colors.Focused = Color.White;
            Colors.Hovered = Color.White;
            Colors.Disabled = Color.White;
        }

        public SkinText(SkinText source)
            : base(source)
        {
            if (source != null)
            {
                this.Font = new SkinFont(source.Font);
                this.OffsetX = source.OffsetX;
                this.OffsetY = source.OffsetY;
                this.Alignment = source.Alignment;
                this.Colors = source.Colors;
            }
        }
        #endregion
    }

    public class SkinFont : SkinBase
    {
        #region Fields
        public SpriteFont Resource = null;
        public string Asset = null;
        public string Addon = null;
        #endregion

        #region Properties
        public int Height
        {
            get
            {
                if (Resource != null)
                    return (int)Resource.MeasureString("AaYy").Y;

                return 0;
            }
        }
        #endregion

        #region Constructors
        public SkinFont()
            : base()
        {
        }

        public SkinFont(SkinFont source)
            : base(source)
        {
            if (source != null)
            {
                this.Resource = source.Resource;
                this.Asset = source.Asset;
            }
        }
        #endregion
    }

    public class SkinImage : SkinBase
    {
        #region Fields
        public Texture2D Resource = null;
        public string Asset = null;
        public string Addon = null;
        #endregion

        #region Constructors
        public SkinImage()
            : base()
        {
        }

        public SkinImage(SkinImage source)
            : base(source)
        {
            this.Resource = source.Resource;
            this.Asset = source.Asset;
        }
        #endregion
    }

    public class SkinCursor : SkinBase
    {
        #region Fields
        public Cursor Resource = null;
        public string Asset = null;
        public string Addon = null;
        #endregion

        #region Constructors
        public SkinCursor()
            : base()
        {
        }

        public SkinCursor(SkinCursor source)
            : base(source)
        {
            this.Resource = source.Resource;
            this.Asset = source.Asset;
        }
        #endregion
    }

    public class SkinControl : SkinBase
    {
        #region Fields
        public string Inherits = null;
        public Size DefaultSize;
        public int ResizerSize;
        public Size MinimumSize;
        public Margins OriginMargins;
        public Margins ClientMargins;
        public SkinList<SkinLayer> Layers = new SkinList<SkinLayer>();
        public SkinList<SkinAttribute> Attributes = new SkinList<SkinAttribute>();
        #endregion

        #region Constructors
        public SkinControl()
            : base()
        {
        }

        public SkinControl(SkinControl source)
            : base(source)
        {
            this.Inherits = source.Inherits;
            this.DefaultSize = source.DefaultSize;
            this.MinimumSize = source.MinimumSize;
            this.OriginMargins = source.OriginMargins;
            this.ClientMargins = source.ClientMargins;
            this.ResizerSize = source.ResizerSize;
            this.Layers = new SkinList<SkinLayer>(source.Layers);
            this.Attributes = new SkinList<SkinAttribute>(source.Attributes);
        }
        #endregion
    }

    public class SkinAttribute : SkinBase
    {
        #region Fields
        public string Value;
        #endregion

        #region Constructors
        public SkinAttribute() : base() { }

        public SkinAttribute(SkinAttribute source)
            : base(source)
        {
            this.Value = source.Value;
        }
        #endregion
    }

    public class Skin : Component
    {
        #region Fields
        SkinXmlDocument doc = null;
        private string name = null;
        private Version version = null;
        private SkinInfo info;
        private SkinList<SkinControl> controls = null;
        private SkinList<SkinFont> fonts = null;
        private SkinList<SkinImage> images = null;
        private SkinList<SkinCursor> cursors = null;
        private SkinList<SkinAttribute> attributes = null;
        private ArchiveManager content = null;
        #endregion

        #region Properties
        public virtual string Name { get { return name; } }
        public virtual Version Version { get { return version; } }
        public virtual SkinInfo Info { get { return info; } }
        public virtual SkinList<SkinControl> Controls { get { return controls; } }
        public virtual SkinList<SkinFont> Fonts { get { return fonts; } }
        public virtual SkinList<SkinImage> Images { get { return images; } }
        public virtual SkinList<SkinCursor> Cursors { get { return cursors; } }
        public virtual SkinList<SkinAttribute> Attributes { get { return attributes; } }
        #endregion
    }
}
