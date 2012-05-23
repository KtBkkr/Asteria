using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteriaClient.Interface.Controls
{
    public class ControlList : EventedList<Control>
    {
        public ControlList() : base() { }
        public ControlList(int capacity) : base(capacity) { }
        public ControlList(IEnumerable<Control> collection) : base(collection) { }
    }

    public class Control : Component
    {
        #region Variables
        public static readonly Color UndefinedColor = new Color(255, 255, 255, 0);
        internal static ControlList Stack = new ControlList();
        
        private Cursor cursor = null;
        private Color color = UndefinedColor;
        private Color textColor = UndefinedColor;
        private Color backColor = Color.Transparent;
        private byte alpha = 255;
        private Anchors anchor = Anchors.Left | Anchors.Top;
        private Anchors resizeEdge = Anchors.All;
        private string text = "Control";
        private bool visible = true;
        private bool enabled = true;
        private SkinControl skin = null;
        private Control parent = null;
        private Control root = null;
        private int left = 0;
        private int top = 0;
        private int width = 64;
        private int height = 64;
        private bool suspended = false;
        private ContextMenu contextMenu = null;
        private long tooltipTimer = 0;
        private long doubleClickTimer = 0;
        private MouseButton doubleClickButton = MouseButton.None;
        private Type toolTipType = typeof(ToolTip);
        private ToolTip toolTip = null;
        private bool doubleClicks = true;
        private bool outlineResizing = false;
        private bool outlineMoving = false;
        private string name = "Control";
        private object tag = null;
        private bool designMode = false;
        private bool partialOutline = true;
        private Rectangle drawingRect = Rectangle.Empty;

        private ControlList controls = new ControlList();
        private Rectangle moveableArea = Rectangle.Empty;
        private bool passive = false;
        private bool detached = false;
        private bool moveable = false;
        private bool resizable = false;
        private bool invalidated = true;
        private bool canFocus = true;
        private int resizerSize = 4;
        private int minimumWidth = 0;
        private int maximumWidth = 4096;
        private int minimumHeight = 0;
        private int maximumHeight = 4096;
        private int topModifier = 0;
        private int leftModifier = 0;
        private int virtualHeight = 64;
        private int virtualWidth = 64;
        private bool stayOnBack = false;
        private bool stayOnTop = false;

        private RenderTarget2D target;
        private Point pressSpot = Point.Zero;
        private int[] pressDiff = new int[4];
        private Alignment resizeArea = Alignment.None;
        private bool hovered = false;
        private bool inside = false;
        private bool[] pressed = new bool[32];
        private bool isMoving = false;
        private bool isResizing = false;
        private Margins margins = new Margins(4, 4, 4, 4);
        private Margins anchorMargins = new Margins();
        private Margins clientMargins = new Margins();
        private Rectangle outlineRect = Rectangle.Empty;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the cursor displaying over the control.
        /// </summary>
        public Cursor Cursor { get { return cursor; } set { cursor = value; } }

        /// <summary>
        /// Gets a list of all child controls.
        /// </summary>
        public virtual IEnumerable<Control> Controls { get { return controls; } }

        /// <summary>
        /// Gets or sets a rectangular area that reacts on moving the control with the mouse.
        /// </summary>
        public virtual Rectangle MoveableArea { get { return moveableArea; } set { moveableArea = value; } }

        /// <summary>
        /// Gets a vlue indicating whether this control is a child control.
        /// </summary>
        public virtual bool IsChild { get { return (parent != null); } }

        /// <summary>
        /// Gets a value indicating whether this control is a parent control.
        /// </summary>
        public virtual bool IsParent { get { return (controls != null && controls.Count > 0); } }

        /// <summary>
        /// Gets or sets the value indicating whether this control is a root control.
        /// </summary>
        public virtual bool IsRoot { get { return (root == this); } }

        /// <summary>
        /// Gets or sets the value indicating whether this control can receive focus.
        /// </summary>
        public virtual bool CanFocus { get { return canFocus; } set { canFocus = value; } }

        /// <summary>
        /// Gets or sets the value indicating whether this control is rendered off the parents texture.
        /// </summary>
        public virtual bool Detached { get { return detached; } set { detached = value; } }

        /// <summary>
        /// Gets or sets the value indicating whether this control can receive user input events.
        /// </summary>
        public virtual bool Passive { get { return passive; } set { passive = value; } }

        /// <summary>
        /// Gets or sets the value indicating whether this control can be moved by the mouse.
        /// </summary>
        public virtual bool Moveable { get { return moveable; } set { moveable = value; } }

        /// <summary>
        /// Gets or sets the value indicating whether this control can be resized by the mouse.
        /// </summary>
        public virtual bool Resizable { get { return resizable; } set { resizable = value; } }

        /// <summary>
        /// Gets or sets the size of the rectangular borders around the control used for resizing by the mouse.
        /// </summary>
        public virtual int ResizerSize { get { return resizerSize; } set { resizerSize = value; } }

        /// <summary>
        /// Gets or sets the ContextMenu associated with this control.
        /// </summary>
        public virtual ContextMenu ContextMenu { get { return contextMenu; } set { contextMenu = value; } }

        /// <summary>
        /// Gets or sets the value indicating whether this control should process mouse double clicks.
        /// </summary>
        public virtual bool DoubleClicks { get { return doubleClicks; } set { doubleClicks = value; } }

        /// <summary>
        /// Gets or sets a value indicating whether this control should use outline resizing.
        /// </summary>
        public virtual bool OutlineResizing { get { return outlineResizing; } set { outlineResizing = value; } }

        /// <summary>
        /// Gets or sets a vlue indicating whether this control should use outline moving.
        /// </summary>
        public virtual bool OutlineMoving { get { return outlineMoving; } set { outlineMoving = value; } }

        /// <summary>
        /// Gets or sets the object that contains data about the control.
        /// </summary>
        public virtual object Tag { get { return tag; } set { tag = value; } }

        /// <summary>
        /// Gets or sets the value indicating the distance from another control. Usable with StackPanel control.
        /// </summary>
        public virtual Margins Margins { get { return margins; } set { margins = value; } }

        /// <summary>
        /// Gets or sets the value indicating whether the control is in design mode.
        /// </summary>
        public virtual bool DesignMode { get { return designMode; } set { designMode = value; } }

        public virtual bool PartialOutline { get { return partialOutline; } set { partialOutline = value; } }

        public virtual string Name { get { return name; } set { name = value; } }

        public virtual bool StayOnBack
        {
            get { return stayOnBack; }
            set
            {
                if (value && stayOnTop)
                    stayOnTop = false;

                stayOnBack = value;
            }
        }

        public virtual bool StayOnTop
        {
            get { return stayOnTop; }
            set
            {
                if (value && stayOnBack)
                    stayOnBack = false;

                stayOnTop = value;
            }
        }

        public virtual bool Focused
        {
            get { return (Manager.FocusedControl == this); }
            set
            {
                this.Invalidate();
                if (value)
                {
                    bool f = Focused;
                    Manager.FocusedControl = this;
                    if (!Suspended && value && !f)
                        OnFocusGained(new EventArgs());

                    if (Focused && Root != null && Root is Container)
                        (Root as Container).ScrollTo(this);
                }
                else
                {
                    bool f = Focused;
                    if (Manager.FocusedControl == this)
                        Manager.FocusedControl = null;

                    if (!Suspended && !value && f)
                        OnFocusLost(new EventArgs());
                }
            }
        }

        public virtual ControlState ControlState
        {
            get
            {
                if (DesignMode)
                    return ControlState.Enabled;
                else if (Suspended)
                    return ControlState.Disabled;
                else
                {
                    if (!enabled)
                        return ControlState.Disabled;

                    if ((IsPressed && inside) || (Focused && IsPressed))
                        return ControlState.Pressed;
                    else if (hovered && !IsPressed)
                        return ControlState.Hovered;
                    else if ((Focused && !inside) || (hovered && IsPressed && !inside) || (Focused && !hovered && inside))
                        return ControlState.Focused;
                    else
                        return ControlState.Enabled;
                }
            }
        }

        public virtual Type ToolTipType
        {
            get { return toolTipType; }
            set
            {
                toolTipType = value;
                if (toolTip != null)
                {
                    toolTip.Dispose();
                    toolTip = null;
                }
            }
        }

        public virtual ToolTip ToolTip
        {
            get
            {
                if (toolTip == null)
                {
                    Type[] t = new Type[1] { typeof(Manager) };
                    object[] p = new object[1] { Manager };

                    toolTip = (ToolTip)toolTipType.GetConstructor(t).Invoke(p);
                    toolTip.Init();
                    toolTip.Visible = false;
                }
                return toolTip;
            }
            set { toolTip = value; }
        }

        internal protected virtual bool IsPressed
        {
            get
            {
                for (int i = 0; i < pressed.Length - 1; i++)
                {
                    if (pressed[i])
                        return true;
                }
                return false;
            }
        }

        internal virtual int TopModifier
        {
            get { return topModifier; }
            set { topModifier = value; }
        }

        internal virtual int LeftModifier
        {
            get { return leftModifier; }
            set { leftModifier = value; }
        }

        internal virtual int VirtualWidth
        {
            get { return virtualWidth; }
            set { virtualWidth = value; }
        }

        internal virtual int VirtualHeight
        {
            get { return virtualHeight; }
            set { virtualHeight = value; }
        }

        /// <summary>
        /// Gets an area where the control is supposed to be drawn.
        /// </summary>
        public Rectangle DrawingRect
        {
            get { return drawingRect; }
            private set { drawingRect = value; }
        }

        public virtual bool Suspended
        {
            get { return suspended; }
            set { suspended = value; }
        }

        internal protected virtual bool Hovered
        {
            get { return hovered; }
        }

        internal protected virtual bool Inside
        {
            get { return inside; }
        }

        internal protected virtual bool[] Pressed
        {
            get { return pressed; }
        }

        protected virtual bool IsMoving
        {
            get { return isMoving; }
            set { isMoving = value; }
        }

        protected virtual bool IsResizing
        {
            get { return isResizing; }
            set { isResizing = value; }
        }

        public virtual Anchors Anchor
        {
            get { return anchor; }
            set
            {
                anchor = value;
                SetAnchorMargins();
                if (!Suspended) OnAnchorChanged(new EventArgs());
            }
        }

        public virtual Anchors ResizeEdge
        {
            get { return resizeEdge; }
            set { resizeEdge = value; }
        }

        public virtual SkinControl Skin
        {
            get { return skin; }
            set
            {
                skin = value;
                ClientMargins = skin.ClientMargins;
            }
        }

        public virtual string Text
        {
            get { return text; }
            set
            {
                text = value;
                Invalidate();
                if (!Suspended) OnTextChanged(new EventArgs());
            }
        }

        public virtual byte Alpha
        {
            get { return alpha; }
            set
            {
                alpha = value;
                if (!Suspended) OnAlphaChanged(new EventArgs());
            }
        }

        public virtual Color BackColor
        {
            get { return backColor; }
            set
            {
                backColor = value;
                Invalidate();
                if (!Suspended) OnBackColorChanged(new EventArgs());
            }
        }

        public virtual Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                {
                    color = value;
                    Invalidate();
                    if (!Suspended) OnColorChanged(new EventArgs());
                }
            }
        }

        public virtual Color TextColor
        {
            get { return textColor; }
            set
            {
                if (value != textColor)
                {
                    textColor = value;
                    Invalidate();
                    if (!Suspended) OnTextColorChanged(new EventArgs());
                }
            }
        }

        public virtual bool Enabled
        {
            get { return enabled; }
            set
            {
                if (Root != null && Root != this && !Root.Enabled && value)
                    return;

                enabled = value;
                Invalidate();

                foreach (Control c in controls)
                    c.Enabled = value;

                if (!Suspended) OnEnabledChanged(new EventArgs());
            }
        }

        public virtual bool Visible
        {
            get { return (visible && (parent == null || parent.Visible)); }
            set
            {
                visible = value;
                Invalidate();
                if (!Suspended) OnVisibleChanged(new EventArgs());
            }
        }

        public virtual Control Parent
        {
            get { return parent; }
            set
            {
                if (parent != value)
                {
                    if (value != null) value.Add(this);
                    else Manager.Add(this);
                }
            }
        }

        public virtual Control Root
        {
            get { return root; }
            private set
            {
                if (root != value)
                {
                    root = value;

                    foreach (Control c in controls)
                        c.Root = root;

                    if (!Suspended) OnRootChanged(new EventArgs());
                }
            }
        }

        public virtual int Left
        {
            get { return left; }
            set
            {
                if (left != value)
                {
                    int old = left;
                    left = value;

                    SetAnchorMargins();
                    if (!Suspended) OnMove(new MoveEventArgs(left, top, old, top));
                }
            }
        }

        public virtual int Top
        {
            get { return top; }
            set
            {
                if (top != value)
                {
                    int old = top;
                    top = value;

                    SetAnchorMargins();
                    if (!Suspended) OnMove(new MoveEventArgs(left, top, left, old));
                }
            }
        }

        public virtual int Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    int old = width;
                    width = value;

                    if (skin != null)
                    {
                        if (width + skin.OriginMargins.Horizontal > MaximumWidth)
                            width = MaximumWidth - skin.OriginMargins.Horizontal;
                    }
                    else
                    {
                        if (width > MaximumWidth) width = MaximumWidth;
                    }

                    if (width < MinimumWidth) width = MinimumWidth;
                    if (width > 0) SetAnchorMargins();
                    if (!Suspended) OnResize(new ResizeEventArgs(width, height, old, height));
                }
            }
        }

        public virtual int Height
        {
            get { return height; }
            set
            {
                int old = height;
                height = value;

                if (skin != null)
                {
                    if (height + skin.OriginMargins.Vertical > MaximumHeight)
                        height = MaximumHeight - skin.OriginMargins.Vertical;
                }
                else
                {
                    if (height > MaximumHeight) height = MaximumHeight;
                }

                if (height < MinimumHeight) height = MinimumHeight;
                if (height > 0) SetAnchorMargins();
                if (!Suspended) OnResize(new ResizeEventArgs(width, height, width, old));
            }
        }

        public virtual int MinimumWidth
        {
            get { return minimumWidth; }
            set
            {
                minimumWidth = value;
                if (minimumWidth < 0) minimumWidth = 0;
                if (minimumWidth > maximumWidth) minimumWidth = maximumWidth;
                if (width < MinimumWidth) Width = MinimumWidth;
            }
        }

        public virtual int MinimumHeight
        {
            get { return minimumHeight; }
            set
            {
                minimumHeight = value;
                if (minimumHeight < 0) minimumHeight = 0;
                if (minimumHeight > maximumHeight) minimumHeight = maximumHeight;
                if (height < MinimumHeight) Height = MinimumHeight;
            }
        }

        public virtual int MaximumWidth
        {
            get
            {
                int max = maximumWidth;
                if (max > Manager.TargetWidth) max = Manager.TargetWidth;
                return max;
            }
            set
            {
                maximumWidth = value;
                if (maximumWidth < minimumWidth) maximumWidth = minimumWidth;
                if (width > MaximumWidth) Width = MaximumWidth;
            }
        }

        public virtual int MaximumHeight
        {
            get
            {
                int max = maximumHeight;
                if (max > Manager.TargetHeight) max = Manager.TargetHeight;
                return max;
            }
            set
            {
                maximumHeight = value;
                if (maximumHeight < minimumHeight) maximumHeight = minimumHeight;
                if (height > MaximumHeight) Height = MaximumHeight;
            }
        }

        public virtual int AbsoluteLeft
        {
            get
            {
                if (parent == null) return left + LeftModifier;
                else if (parent.Skin == null) return parent.AbsoluteLeft + left + LeftModifier;
                else return parent.AbsoluteLeft + left - parent.Skin.OriginMargins.Left + LeftModifier;
            }
        }

        public virtual int AbsoluteTop
        {
            get
            {
                if (parent == null) return top + TopModifier;
                else if (parent.Skin == null) return parent.AbsoluteTop + top + TopModifier;
                else return parent.AbsoluteTop + top - parent.Skin.OriginMargins.Top + TopModifier;
            }
        }

        public virtual int OriginLeft
        {
            get
            {
                if (skin == null) return AbsoluteLeft;
                return AbsoluteLeft - skin.OriginMargins.Left;
            }
        }

        public virtual int OriginTop
        {
            get
            {
                if (skin == null) return AbsoluteTop;
                return AbsoluteTop - skin.OriginMargins.Top;
            }
        }

        public virtual int OriginWidth
        {
            get
            {
                if (skin == null) return width;
                return width + skin.OriginMargins.Left + skin.OriginMargins.Right;
            }
        }

        public virtual int OriginHeight
        {
            get
            {
                if (skin == null) return height;
                return height + skin.OriginMargins.Top + skin.OriginMargins.Bottom;
            }
        }

        public virtual Margins ClientMargins
        {
            get { return clientMargins; }
            set { clientMargins = value; }
        }

        public virtual int ClientLeft
        {
            get
            {
                //if (skin == null) return left;
                return ClientMargins.Left;
            }
        }

        public virtual int ClientTop
        {
            get
            {
                //if (skin == null) return top;
                return ClientMargins.Top;
            }
        }

        public virtual int ClientWidth
        {
            get
            {
                //if (skin == null) return Width;
                return OriginWidth - ClientMargins.Left - ClientMargins.Right;
            }
            set
            {
                Width = value + ClientMargins.Horizontal - skin.OriginMargins.Horizontal;
            }
        }

        public virtual int ClientHeight
        {
            get
            {
                if (skin == null) return Height;
                return OriginHeight - ClientMargins.Top - ClientMargins.Bottom;
            }
            set
            {
                Height = value + ClientMargins.Vertical - skin.OriginMargins.Vertical;
            }
        }

        public virtual Rectangle AbsoluteRect
        {
            get { return new Rectangle(AbsoluteLeft, AbsoluteTop, OriginWidth, OriginHeight); }
        }

        public virtual Rectangle OriginRect
        {
            get { return new Rectangle(OriginLeft, OriginTop, OriginWidth, OriginHeight); }
        }

        public virtual Rectangle ClientRect
        {
            get { return new Rectangle(ClientLeft, ClientTop, ClientWidth, ClientHeight); }
        }

        public virtual Rectangle ControlRect
        {
            get { return new Rectangle(Left, Top, Width, Height); }
            set
            {
                Left = value.Left;
                Top = value.Top;
                Width = value.Width;
                Height = value.Height;
            }
        }

        private Rectangle OutlineRect
        {
            get { return outlineRect; }
            set
            {
                outlineRect = value;
                if (value != Rectangle.Empty)
                {
                    if (outlineRect.Width > MaximumWidth) outlineRect.Width = MaximumWidth;
                    if (outlineRect.Height > MaximumHeight) outlineRect.Height = MaximumHeight;
                    if (outlineRect.Width < MinimumWidth) outlineRect.Width = MinimumWidth;
                    if (outlineRect.Height < MinimumHeight) outlineRect.Height = MinimumHeight;
                }
            }
        }
        #endregion

        #region Events
        public event EventHandler Click;
        public event EventHandler DoubleClick;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MousePress;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseOver;
        public event MouseEventHandler MouseOut;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyPress;
        public event KeyEventHandler KeyUp;
        public event MoveEventHandler Move;
        public event MoveEventHandler ValidateMove;
        public event ResizeEventHandler Resize;
        public event ResizeEventHandler ValidateResize;
        public event DrawEventHandler Draw;
        public event EventHandler MoveBegin;
        public event EventHandler MoveEnd;
        public event EventHandler ResizeBegin;
        public event EventHandler ResizeEnd;
        public event EventHandler ColorChanged;
        public event EventHandler TextColorChanged;
        public event EventHandler BackColorChanged;
        public event EventHandler TextChanged;
        public event EventHandler AnchorChanged;
        public event EventHandler SkinChanging;
        public event EventHandler SkinChanged;
        public event EventHandler ParentChanged;
        public event EventHandler RootChanged;
        public event EventHandler VisibleChanged;
        public event EventHandler EnabledChanged;
        public event EventHandler AlphaChanged;
        public event EventHandler FocusLost;
        public event EventHandler FocusGained;
        public event DrawEventHandler DrawTexture;
        #endregion

        #region Constructors
        public Control(Manager manager)
            : base(manager)
        {
            if (Manager == null)
                throw new Exception("Control cannot be created. Manager instance is needed.");
            else if (Manager.Skin == null)
                throw new Exception("Control cannot be created. No skin loaded.");

            text = Utilities.DerviceControlName(this);
            root = this;

            InitSkin();

            CheckLayer(skin, "Control");

            if (Skin != null)
            {
                SetDefaultSize(width, height);
                SetMinimumSize(MinimumWidth, MinimumHeight);
                ResizerSize = skin.ResizerSize;
            }

            Stack.Add(this);
        }
        #endregion

        #region Deconstructors
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (parent != null) parent.Remove(this);
                else if (Manager != null) Manager.Remove(this);
                if (Manager.OrderList != null) Manager.OrderList.Remove(this);

                // Possibly we added the menu to another parent than this control,
                // so we dispose it manually, because in logic it belongs to this control.
                if (contextMenu != null)
                {
                    contextMenu.Dispose();
                    contextMenu = null;
                }

                // Recursively disposing all controls. The collection might change rom it's children,
                // so we check it on count greater than zero.
                if (controls != null)
                {
                    int c = controls.Count;
                    for (int i = 0; i < c; i++)
                    {
                        if (controls.Count > 0)
                            controls[0].Dispose();
                    }
                }

                // Disposes tooltip owned by manager.
                if (toolTip != null && !Manager.Disposing)
                {
                    toolTip.Dispose();
                    toolTip = null;
                }

                // Removing this control from the global stack.
                Stack.Remove(this);

                if (target != null)
                {
                    target.Dispose();
                    target = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Methods

        #region Private
        private int GetVirtualHeight()
        {
            if (this.Parent is Container && (this.Parent as Container).AutoScroll)
            {
                int maxy = 0;

                foreach (Control c in Controls)
                {
                    if ((c.Anchor & Anchors.Bottom) != Anchors.Bottom && c.Visible)
                    {
                        if (c.Top + c.Height > maxy)
                            maxy = c.Top + c.Height;
                    }
                }

                if (maxy < Height) maxy = Height;

                return maxy;
            }
            else
                return Height;
        }
        #endregion

        #region Handlers
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (MouseUp != null) MouseUp.Invoke(this, e);
        }

        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            if (MouseDown != null) MouseDown.Invoke(this, e);
        }

        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (MouseMove != null) MouseMove.Invoke(this, e);
        }

        protected virtual void OnMouseOver(MouseEventArgs e)
        {
            if (MouseOver != null) MouseOver.Invoke(this, e);
        }

        protected virtual void OnMouseOut(MouseEventArgs e)
        {
            if (MouseOut != null) MouseOut.Invoke(this, e);
        }

        protected virtual void OnClick(MouseEventArgs e)
        {
            if (Click != null) Click.Invoke(this, e);
        }

        protected virtual void OnDoubleClick(MouseEventArgs e)
        {
            if (DoubleClick != null) DoubleClick.Invoke(this, e);
        }

        protected virtual void OnMove(MoveEventArgs e)
        {
            if (parent != null) parent.Invalidate();
            if (Move != null) Move.Invoke(this, e);
        }

        protected virtual void OnResize(ResizeEventArgs e)
        {
            Invalidate();
            if (Resize != null) Resize.Invoke(this, e);
        }

        protected virtual void OnValidateResize(ResizeEventArgs e)
        {
            if (ValidateResize != null) ValidateResize.Invoke(this, e);
        }

        protected virtual void OnValidateMove(MoveEventArgs e)
        {
            if (ValidateMove != null) ValidateMove.Invoke(this, e);
        }

        protected virtual void OnMoveBegin(EventArgs e)
        {
            if (MoveBegin != null) MoveBegin.Invoke(this, e);
        }

        protected virtual void OnMoveEnd(EventArgs e)
        {
            if (MoveEnd != null) MoveEnd.Invoke(this, e);
        }

        protected virtual void OnResizeBegin(EventArgs e)
        {
            if (ResizeBegin != null) ResizeBegin.Invoke(this, e);
        }

        protected virtual void OnResizeEnd(EventArgs e)
        {
            if (ResizeEnd != null) ResizeEnd.Invoke(this, e);
        }

        protected virtual void OnParentResize(object sender, ResizeEventArgs e)
        {
            ProcessAnchor(e);
        }

        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            if (KeyUp != null) KeyUp.Invoke(this, e);
        }

        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (KeyDown != null) KeyDown.Invoke(this, e);
        }

        protected virtual void OnKeyPress(KeyEventArgs e)
        {
            if (KeyPress != null) KeyPress.Invoke(this, e);
        }

        protected internal void OnDraw(DrawEventArgs e)
        {
            if (Draw != null) Draw.Invoke(this, e);
        }

        protected void OnDrawTexture(DrawEventArgs e)
        {
            if (DrawTexture != null) DrawTexture.Invoke(this, e);
        }

        protected virtual void OnColorChanged(EventArgs e)
        {
            if (ColorChanged != null) ColorChanged.Invoke(this, e);
        }

        protected virtual void OnTextColorChanged(EventArgs e)
        {
            if (TextColorChanged != null) TextColorChanged.Invoke(this, e);
        }

        protected virtual void OnBackColorChanged(EventArgs e)
        {
            if (BackColorChanged != null) BackColorChanged.Invoke(this, e);
        }

        protected virtual void OnTextChanged(EventArgs e)
        {
            if (TextChanged != null) TextChanged.Invoke(this, e);
        }

        protected virtual void OnAnchorChanged(EventArgs e)
        {
            if (AnchorChanged != null) AnchorChanged.Invoke(this, e);
        }

        protected internal virtual void OnSkinChanged(EventArgs e)
        {
            if (SkinChanged != null) SkinChanged.Invoke(this, e);
        }

        protected internal virtual void OnSkinChanging(EventArgs e)
        {
            if (SkinChanging != null) SkinChanging.Invoke(this, e);
        }

        protected virtual void OnParentChanged(EventArgs e)
        {
            if (ParentChanged != null) ParentChanged.Invoke(this, e);
        }

        protected virtual void OnRootChanged(EventArgs e)
        {
            if (RootChanged != null) RootChanged.Invoke(this, e);
        }

        protected virtual void OnVisibleChanged(EventArgs e)
        {
            if (VisibleChanged != null) VisibleChanged.Invoke(this, e);
        }

        protected virtual void OnEnabledChanged(EventArgs e)
        {
            if (EnabledChanged != null) EnabledChanged.Invoke(this, e);
        }

        protected virtual void OnAlphaChanged(EventArgs e)
        {
            if (AlphaChanged != null) AlphaChanged.Invoke(this, e);
        }

        protected virtual void OnFocusLost(EventArgs e)
        {
            if (FocusLost != null) FocusLost.Invoke(this, e);
        }

        protected virtual void OnFocusGained(EventArgs e)
        {
            if (FocusGained != null) FocusGained.Invoke(this, e);
        }

        protected virtual void OnMousePress(MouseEventArgs e)
        {
            if (MousePress != null) MousePress.Invoke(this, e);
        }
        #endregion

        #endregion
    }
}
