using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        private Form window = null;
        private Cursor cursor = null;
        private bool deviceReset = false;
        private RenderTarget2D renderTarget = null;
        private int targetFrames = 60;
        private long drawTime = 0;
        private long updateTime = 0;
        private GraphicsDeviceManager graphics = null;
        private ArchiveManager content = null;
        private Renderer renderer = null;
        private InputSystem input = null;
        private bool inputEnabled = true;
        private List<Component> components = null;
        private ControlList controls = null;
        private ControlList orderList = null;
        private Skin skin = null;
        private string skinName = _DefaultSkin;
        private string layoutDirectory = _LayoutDirectory;
        private string skinDirectory = _SkinDirectory;
        private string skinExtension = _SkinExtension;
        private Control focusedControl = null;
        private ModalContainer modalWindow = null;
        private float globalDepth = 0.0f;
        private int toolTipDelay = _ToolTipDelay;
        private bool toolTipsEnabled = true;
        private int menuDelay = _MenuDelay;
        private int doubleClickTime = _DoubleClickTime;
        private int textureResizeIncrement = _TextureResizeIncrement;
        private bool logUnhandledExceptions = true;
        private ControlStates states = new ControlStates();
        private KeyboardLayout keyboardLayout = null;
        private List<KeyboardLayout> keyboardLayouts = new List<KeyboardLayout>();
        private bool disposing = false;
        private bool useGuide = false;
        private bool autoUnfocus = true;
        private bool autoCreateRenderTarget = true;
        #endregion

        #region Properties
        public virtual bool Disposing { get { return disposing; } }

        public virtual Form Window { get { return window; } }

        public virtual Cursor Cursor
        {
            get { return cursor; }
            set
            {
                cursor = value;
                SetCursor(cursor);
            }
        }

        public virtual new Game Game { get { return base.Game; } }

        public virtual new GraphicsDevice GraphicsDevice { get { return base.GraphicsDevice; } }

        public virtual GraphicsDeviceManager Graphics { get { return graphics; } }

        public virtual Renderer Renderer { get { return renderer; } }

        public virtual ArchiveManager Content { get { return content; } }

        public virtual InputSystem Input { get { return input; } }

        public virtual IEnumerable<Component> Components { get { return components; } }

        public virtual IEnumerable<Control> Controls { get { return controls; } }

        public virtual float GlobalDepth { get { return globalDepth; } set { globalDepth = value; } }

        public virtual int ToolTipDelay { get { return toolTipDelay; } set { toolTipDelay = value; } }

        public virtual int MenuDelay { get { return menuDelay; } set { menuDelay = value; } }

        public virtual int DoubleClickTime { get { return doubleClickTime; } set { doubleClickTime = value; } }

        public virtual int TextureResizeIncrement { get { return textureResizeIncrement; } set { textureResizeIncrement = value; } }

        public virtual bool ToolTipsEnabled { get { return toolTipsEnabled; } set { toolTipsEnabled = value; } }

        public virtual bool LogUnhandledExceptions { get { return logUnhandledExceptions; } set { logUnhandledExceptions = value; } }

        public virtual bool InputEnabled { get { return inputEnabled; } set { inputEnabled = value; } }

        public virtual RenderTarget2D RenderTarget { get { return renderTarget; } set { renderTarget = value; } }

        public virtual int TargetFrames { get { return targetFrames; } set { targetFrames = value; } }

        public virtual List<KeyboardLayout> KeyboardLayouts
        {
            get { return keyboardLayouts; }
            set { keyboardLayouts = value; }
        }

        /// <summary>
        /// Gets or sets a vlue indicating if Guide component can be used.
        /// </summary>
        public bool UseGuide
        {
            get { return useGuide; }
            set { useGuide = value; }
        }

        public virtual bool AutoUnfocus
        {
            get { return autoUnfocus; }
            set { autoUnfocus = value; }
        }

        public virtual bool AutoCreateRenderTarget
        {
            get { return autoCreateRenderTarget; }
            set { autoCreateRenderTarget = value; }
        }

        public virtual KeyboardLayout KeyboardLayout
        {
            get
            {
                if (keyboardLayout == null)
                {
                    int id = System.Windows.Forms.InputLanguage.CurrentInputLanguage.Culture.KeyboardLayoutId;
                    for (int i = 0; i < keyboardLayouts.Count; i++)
                    {
                        if (keyboardLayouts[i].LayoutList.Contains(id))
                            return keyboardLayouts[i];
                    }
                    keyboardLayout = new KeyboardLayout();
                }
                return keyboardLayout;
            }
            set
            {
                keyboardLayout = value;
            }
        }

        /// <summary>
        /// Gets or sets the initial directory for looking for the skins in.
        /// </summary>
        public virtual string SkinDirectory
        {
            get
            {
                if (!skinDirectory.EndsWith("\\"))
                {
                    skinDirectory += "\\";
                }
                return skinDirectory;
            }
            set
            {
                skinDirectory = value;
                if (!skinDirectory.EndsWith("\\"))
                {
                    skinDirectory += "\\";
                }
            }
        }

        /// <summary>
        /// Gets or sets the initial directory for looking for the layout files in.
        /// </summary>
        public virtual string LayoutDirectory
        {
            get
            {
                if (!layoutDirectory.EndsWith("\\"))
                {
                    layoutDirectory += "\\";
                }
                return layoutDirectory;
            }
            set
            {
                layoutDirectory = value;
                if (!layoutDirectory.EndsWith("\\"))
                {
                    layoutDirectory += "\\";
                }
            }
        }

        /// <summary>
        /// Gets or sets the file extension for archived skin files.
        /// </summary>
        public string SkinExtension
        {
            get
            {
                if (!skinExtension.StartsWith("."))
                {
                    skinExtension = "." + skinExtension;
                }
                return skinExtension;
            }
            set
            {
                skinExtension = value;
                if (!skinExtension.StartsWith("."))
                {
                    skinExtension = "." + skinExtension;
                }
            }
        }

        public virtual int TargetWidth
        {
            get
            {
                if (renderTarget != null)
                {
                    return renderTarget.Width;
                }
                else return ScreenWidth;
            }
        }

        public virtual int TargetHeight
        {
            get
            {
                if (renderTarget != null)
                {
                    return renderTarget.Height;
                }
                else return ScreenHeight;
            }
        }

        /// <summary>
        /// Gets current width of the screen in pixels.
        /// </summary>
        public virtual int ScreenWidth
        {
            get
            {
                if (GraphicsDevice != null)
                {
                    return GraphicsDevice.PresentationParameters.BackBufferWidth;
                }
                else return 0;
            }

        }

        /// <summary>
        /// Gets current height of the screen in pixels.
        /// </summary>
        public virtual int ScreenHeight
        {
            get
            {
                if (GraphicsDevice != null)
                {
                    return GraphicsDevice.PresentationParameters.BackBufferHeight;
                }
                else return 0;
            }
        }

        /// <summary>
        /// Gets or sets new skin used by all controls.
        /// </summary>
        public virtual Skin Skin
        {
            get
            {
                return skin;
            }
            set
            {
                SetSkin(value);
            }
        }

        /// <summary>
        /// Returns currently active modal window.
        /// </summary>
        public virtual ModalContainer ModalWindow
        {
            get
            {
                return modalWindow;
            }
            internal set
            {
                modalWindow = value;

                if (value != null)
                {
                    value.ModalResult = ModalResult.None;

                    value.Visible = true;
                    value.Focused = true;
                }
            }
        }

        /// <summary>
        /// Returns currently focused control.
        /// </summary>
        public virtual Control FocusedControl
        {
            get
            {
                return focusedControl;
            }
            internal set
            {
                if (value != null && value.Visible && value.Enabled)
                {
                    if (value != null && value.CanFocus)
                    {
                        if (focusedControl == null || (focusedControl != null && value.Root != focusedControl.Root) || !value.IsRoot)
                        {
                            if (focusedControl != null && focusedControl != value)
                                focusedControl.Focused = false;

                            focusedControl = value;
                        }
                    }
                    else if (value != null && !value.CanFocus)
                    {
                        if (focusedControl != null && value.Root != focusedControl.Root)
                        {
                            if (focusedControl != value.Root)
                                focusedControl.Focused = false;

                            focusedControl = value.Root;
                        }
                        else if (focusedControl == null)
                            focusedControl = value.Root;
                    }
                    BringToFront(value.Root);
                }
                else if (value == null)
                {
                    focusedControl = value;
                }
            }
        }

        internal virtual ControlList OrderList { get { return orderList; } }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the GraphicsDevice settings are changed.
        /// </summary>
        public event DeviceEventHandler DeviceSettingsChanged;

        /// <summary>
        /// Occurs when the skin is about to change.
        /// </summary>
        public event SkinEventHandler SkinChanging;

        /// <summary>
        /// Occurs when the skin changes.
        /// </summary>
        public event SkinEventHandler SkinChanged;

        /// <summary>
        /// Occurs when game window is about to close.
        /// </summary>
        public event WindowClosingEventHandler WindowClosing;    
        #endregion

        #region Constructors
        public Manager(Game game, GraphicsDeviceManager graphics, string skin)
            : base(game)
        {
            disposing = false;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleUnhadledExceptions);

            menuDelay = SystemInformation.MenuShowDelay;
            doubleClickTime = SystemInformation.DoubleClickTime;

            window = (Form)Form.FromHandle(Game.Window.Handle);
            window.FormClosing += new FormClosingEventHandler(Window_FormClosing);

            content = new ArchiveManager(Game.Services);
            input = new InputSystem(this, new InputOffset(0, 0, 1f, 1f));
            components = new List<Component>();
            controls = new ControlList();
            orderList = new ControlList();

            this.graphics = graphics;
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(PrepareGraphicsDevice);

            skinName = skin;

            states.Buttons = new Control[32];
            states.Click = -1;
            states.Over = null;

            input.MouseDown += new MouseEventHandler(MouseDownProcess);
            input.MouseUp += new MouseEventHandler(MouseUpProcess);
            input.MousePress += new MouseEventHandler(MousePressProcess);
            input.MouseMove += new MouseEventHandler(MouseMoveProcess);

            input.KeyDown += new KeyEventHandler(KeyDownProcess);
            input.KeyUp += new KeyEventHandler(KeyUpProcess);
            input.KeyPress += new KeyEventHandler(KeyPressProcess);

            keyboardLayouts.Add(new KeyboardLayout());
            keyboardLayouts.Add(new CzechKeyboardLayout());
            keyboardLayouts.Add(new GermanKeyboardLayout());
        }

        internal void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool ret = false;
            WindowClosingEventArgs ex = new WindowClosingEventArgs();
            if (WindowClosing != null)
            {
                WindowClosing.Invoke(this, ex);
                ret = ex.Cancel;
            }

            e.Cancel = ret;
        }

        public Manager(Game game, string skin)
            : this(game, game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager, skin)
        {
        }

        public Manager(Game game, GraphicsDeviceManager graphics)
            : this(game, graphics, _DefaultSkin)
        {
        }

        public Manager(Game game)
            : this(game, game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager, _DefaultSkin)
        {
        }
        #endregion

        #region Destructors
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.disposing = true;

                // Recursively disposing all controls added to the manager and its child controls.
                if (controls != null)
                {
                    int c = controls.Count;
                    for (int i = 0; i < c; i++)
                    {
                        if (controls.Count > 0) controls[0].Dispose();
                    }
                }

                // Disposing all components added to manager.
                if (components != null)
                {
                    int c = components.Count;
                    for (int i = 0; i < c; i++)
                    {
                        if (components.Count > 0) components[0].Dispose();
                    }
                }

                if (content != null)
                {
                    content.Unload();
                    content.Dispose();
                    content = null;
                }

                if (renderer != null)
                {
                    renderer.Dispose();
                    renderer = null;
                }
                if (input != null)
                {
                    input.Dispose();
                    input = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Methods
        private void SetCursor(Cursor cursor)
        {
            window.Cursor = cursor;
        }

        private void InitSkins()
        {
            // Initializing skins for every control created, even not visible or 
            // not added to the manager or another parent.
            foreach (Control c in Control.Stack)
            {
                c.InitSkin();
            }
        }

        private void InitControls()
        {
            // Initializing all controls created, even not visible or 
            // not added to the manager or another parent.
            foreach (Control c in Control.Stack)
            {
                c.Init();
            }
        }

        private void SortLevel(ControlList cs)
        {
            if (cs != null)
            {
                foreach (Control c in cs)
                {
                    if (c.Visible)
                    {
                        OrderList.Add(c);
                        SortLevel(c.Controls as ControlList);
                    }
                }
            }
        }

        /// <summary>
        /// Method used as an event handler for the GraphicsDeviceManager.PreparingDeviceSettings event.
        /// </summary>
        protected virtual void PrepareGraphicsDevice(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = _RenderTargetUsage;
            int w = e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth;
            int h = e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight;

            foreach (Control c in Controls)
            {
                SetMaxSize(c, w, h);
            }


            if (DeviceSettingsChanged != null) DeviceSettingsChanged.Invoke(new DeviceEventArgs(e));
        }

        private void SetMaxSize(Control c, int w, int h)
        {
            if (c.Width > w)
            {
                w -= (c.Skin != null) ? c.Skin.OriginMargins.Horizontal : 0;
                c.Width = w;
            }
            if (c.Height > h)
            {
                h -= (c.Skin != null) ? c.Skin.OriginMargins.Vertical : 0;
                c.Height = h;
            }

            foreach (Control cx in c.Controls)
            {
                SetMaxSize(cx, w, h);
            }
        }

        /// <summary>
        /// Initializes the controls manager.
        /// </summary>    
        public override void Initialize()
        {
            base.Initialize();

            if (autoCreateRenderTarget)
            {
                if (renderTarget != null)
                    renderTarget.Dispose();

                renderTarget = CreateRenderTarget();
            }

            GraphicsDevice.DeviceReset += new System.EventHandler<System.EventArgs>(GraphicsDevice_DeviceReset);

            input.Initialize();
            renderer = new Renderer(this);
            SetSkin(skinName);
        }

        public virtual RenderTarget2D CreateRenderTarget()
        {
            return CreateRenderTarget(ScreenWidth, ScreenHeight);
        }

        public virtual RenderTarget2D CreateRenderTarget(int width, int height)
        {
            Input.InputOffset = new InputOffset(0, 0, ScreenWidth / (float)width, ScreenHeight / (float)height);
            return new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, GraphicsDevice.PresentationParameters.MultiSampleCount, _RenderTargetUsage);
        }

        /// <summary>
        /// Sets and loads the new skin.
        /// </summary>
        public virtual void SetSkin(string name)
        {
            Skin skin = new Skin(this, name);
            SetSkin(skin);
        }

        public virtual void SetSkin(Skin skin)
        {
            if (SkinChanging != null) SkinChanging.Invoke(new EventArgs());
            if (this.skin != null)
            {
                Remove(this.skin);
                this.skin.Dispose();
                this.skin = null;
                GC.Collect();
            }
            this.skin = skin;
            this.skin.Init();
            Add(this.skin);
            skinName = this.skin.Name;

            if (this.skin.Cursors["Default"] != null)
                SetCursor(this.skin.Cursors["Default"].Resource);

            InitSkins();
            if (SkinChanged != null) SkinChanged.Invoke(new EventArgs());

            InitControls();
        }

        /// <summary>
        /// Brings the control to the front of the z-order.
        /// </summary>
        public virtual void BringToFront(Control control)
        {
            if (control != null && !control.StayOnBack)
            {
                ControlList cs = (control.Parent == null) ? controls as ControlList : control.Parent.Controls as ControlList;
                if (cs.Contains(control))
                {
                    cs.Remove(control);
                    if (!control.StayOnTop)
                    {
                        int pos = cs.Count;
                        for (int i = cs.Count - 1; i >= 0; i--)
                        {
                            if (!cs[i].StayOnTop)
                                break;

                            pos = i;
                        }
                        cs.Insert(pos, control);
                    }
                    else
                        cs.Add(control);
                }
            }
        }

        /// <summary>
        /// Sends the control to the back of the z-order.
        /// </summary>
        public virtual void SendToBack(Control control)
        {
            if (control != null && !control.StayOnTop)
            {
                ControlList cs = (control.Parent == null) ? controls as ControlList : control.Parent.Controls as ControlList;
                if (cs.Contains(control))
                {
                    cs.Remove(control);
                    if (!control.StayOnBack)
                    {
                        int pos = 0;
                        for (int i = 0; i < cs.Count; i++)
                        {
                            if (!cs[i].StayOnBack)
                                break;

                            pos = i;
                        }
                        cs.Insert(pos, control);
                    }
                    else
                        cs.Insert(0, control);
                }
            }
        }

        /// <summary>
        /// Called when the manager needs to be updated.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            updateTime += gameTime.ElapsedGameTime.Ticks;
            double ms = TimeSpan.FromTicks(updateTime).TotalMilliseconds;

            if (targetFrames == 0 || ms == 0 || ms >= (1000f / targetFrames))
            {
                TimeSpan span = TimeSpan.FromTicks(updateTime);
                gameTime = new GameTime(gameTime.TotalGameTime, span);
                updateTime = 0;

                if (inputEnabled)
                    input.Update(gameTime);

                if (components != null)
                {
                    foreach (Component c in components)
                        c.Update(gameTime);
                }

                ControlList list = new ControlList(controls);
                if (list != null)
                {
                    foreach (Control c in list)
                        c.Update(gameTime);
                }

                OrderList.Clear();
                SortLevel(controls);
            }
        }

        /// <summary>
        /// Adds a component or a control to the manager.
        /// </summary>
        public virtual void Add(Component component)
        {
            if (component != null)
            {
                if (component is Control && !controls.Contains(component as Control))
                {
                    Control c = (Control)component;

                    if (c.Parent != null) c.Parent.Remove(c);

                    controls.Add(c);
                    c.Manager = this;
                    c.Parent = null;
                    if (focusedControl == null) c.Focused = true;

                    DeviceSettingsChanged += new DeviceEventHandler((component as Control).OnDeviceSettingsChanged);
                    SkinChanging += new SkinEventHandler((component as Control).OnSkinChanging);
                    SkinChanged += new SkinEventHandler((component as Control).OnSkinChanged);
                }
                else if (!(component is Control) && !components.Contains(component))
                {
                    components.Add(component);
                    component.Manager = this;
                }
            }
        }

        /// <summary>
        /// Removes a component or a control from the manager.
        /// </summary>
        public virtual void Remove(Component component)
        {
            if (component != null)
            {
                if (component is Control)
                {
                    Control c = component as Control;
                    SkinChanging -= c.OnSkinChanging;
                    SkinChanged -= c.OnSkinChanged;
                    DeviceSettingsChanged -= c.OnDeviceSettingsChanged;

                    if (c.Focused) c.Focused = false;
                    controls.Remove(c);
                }
                else
                    components.Remove(component);
            }
        }

        public virtual void Prepare(GameTime gameTime)
        {
        }

        /// <summary>
        /// Renders all controls added to the manager.
        /// </summary>
        public virtual void BeginDraw(GameTime gameTime)
        {
            Draw(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (renderTarget != null)
            {
                drawTime += gameTime.ElapsedGameTime.Ticks;
                double ms = TimeSpan.FromTicks(drawTime).TotalMilliseconds;

                if (targetFrames == 0 || (ms == 0 || ms >= (1000f / targetFrames)))
                {
                    TimeSpan span = TimeSpan.FromTicks(drawTime);
                    gameTime = new GameTime(gameTime.TotalGameTime, span);
                    drawTime = 0;

                    if ((controls != null))
                    {
                        ControlList list = new ControlList();
                        list.AddRange(controls);

                        foreach (Control c in list)
                        {
                            c.PrepareTexture(renderer, gameTime);
                        }

                        GraphicsDevice.SetRenderTarget(renderTarget);
                        GraphicsDevice.Clear(Color.Transparent);

                        if (renderer != null)
                        {
                            foreach (Control c in list)
                                c.Render(renderer, gameTime);
                        }
                    }
                    GraphicsDevice.SetRenderTarget(null);
                }
            }
            else
            {
                throw new Exception("Manager.RenderTarget has to be specified. Assign a render target or set Manager.AutoCreateRenderTarget property to true.");
            }
        }

        /// <summary>
        /// Draws texture resolved from RenderTarget used for rendering.
        /// </summary>
        public virtual void EndDraw()
        {
            EndDraw(new Rectangle(0, 0, ScreenWidth, ScreenHeight));
        }

        /// <summary>
        /// Draws texture resolved from RenderTarget to specified rectangle.
        /// </summary>
        public virtual void EndDraw(Rectangle rect)
        {
            if (renderTarget != null && !deviceReset)
            {
                renderer.Begin(BlendingMode.Default);
                renderer.Draw(RenderTarget, rect, Color.White);
                renderer.End();
            }
            else if (deviceReset)
                deviceReset = false;
        }

        public virtual Control GetControl(string name)
        {
            foreach (Control c in Controls)
            {
                if (c.Name.ToLower() == name.ToLower())
                    return c;
            }
            return null;
        }

        private void HandleUnhadledExceptions(object sender, UnhandledExceptionEventArgs e)
        {
            if (LogUnhandledExceptions)
            {
                LogException(e.ExceptionObject as Exception);
            }
        }

        private void GraphicsDevice_DeviceReset(object sender, System.EventArgs e)
        {
            deviceReset = true;
            if (AutoCreateRenderTarget)
            {
                if (renderTarget != null) RenderTarget.Dispose();
                RenderTarget = CreateRenderTarget();
            }
        }

        public virtual void LogException(Exception e)
        {
            string an = Assembly.GetEntryAssembly().Location;
            Assembly asm = Assembly.GetAssembly(typeof(Manager));
            string path = Path.GetDirectoryName(an);
            string fn = path + "\\" + Path.GetFileNameWithoutExtension(asm.Location) + ".log";

            File.AppendAllText(fn, "////////////////////////////////////////////////////////////////\n" +
                                   "    Date: " + DateTime.Now.ToString() + "\n" +
                                   "Assembly: " + Path.GetFileName(asm.Location) + "\n" +
                                   " Version: " + asm.GetName().Version.ToString() + "\n" +
                                   " Message: " + e.Message + "\n" +
                                   "////////////////////////////////////////////////////////////////\n" +
                                   e.StackTrace + "\n" +
                                   "////////////////////////////////////////////////////////////////\n\n", Encoding.Default);
        }
        #endregion

        #region Input
        private bool CheckParent(Control control, Point pos)
        {
            if (control.Parent != null && !CheckDetached(control))
            {
                Control parent = control.Parent;
                Control root = control.Root;
                Rectangle pr = new Rectangle(parent.AbsoluteLeft, parent.AbsoluteTop, parent.Width, parent.Height);
                Margins margins = root.Skin.ClientMargins;
                Rectangle rr = new Rectangle(root.AbsoluteLeft + margins.Left, root.AbsoluteTop + margins.Top, root.OriginWidth - margins.Horizontal, root.OriginHeight - margins.Vertical);

                return (rr.Contains(pos) && pr.Contains(pos));
            }
            return true;
        }

        private bool CheckState(Control control)
        {
            bool modal = (ModalWindow == null) ? true : (ModalWindow == control.Root);

            return (control != null && !control.Passive && control.Visible && control.Enabled && modal);
        }

        private bool CheckOrder(Control control, Point pos)
        {
            if (!CheckPosition(control, pos)) return false;

            for (int i = OrderList.Count - 1; i > OrderList.IndexOf(control); i--)
            {
                Control c = OrderList[i];

                if (!c.Passive && CheckPosition(c, pos) && CheckParent(c, pos))
                    return false;
            }

            return true;
        }

        private bool CheckDetached(Control control)
        {
            bool ret = control.Detached;
            if (control.Parent != null)
            {
                if (CheckDetached(control.Parent)) ret = true;
            }
            return ret;
        }

        private bool CheckPosition(Control control, Point pos)
        {
            return (control.AbsoluteLeft <= pos.X &&
                    control.AbsoluteTop <= pos.Y &&
                    control.AbsoluteLeft + control.Width >= pos.X &&
                    control.AbsoluteTop + control.Height >= pos.Y &&
                    CheckParent(control, pos));
        }

        private bool CheckButtons(int index)
        {
            for (int i = 0; i < states.Buttons.Length; i++)
            {
                if (i == index) continue;
                if (states.Buttons[i] != null) return false;
            }

            return true;
        }

        private void TabNextControl(Control control)
        {
            int start = OrderList.IndexOf(control);
            int i = start;

            do
            {
                if (i < OrderList.Count - 1) i += 1;
                else i = 0;
            }
            while ((OrderList[i].Root != control.Root || !OrderList[i].CanFocus || OrderList[i].IsRoot || !OrderList[i].Enabled) && i != start);

            OrderList[i].Focused = true;
        }

        private void TabPrevControl(Control control)
        {
            int start = OrderList.IndexOf(control);
            int i = start;

            do
            {
                if (i > 0) i -= 1;
                else i = OrderList.Count - 1;
            }
            while ((OrderList[i].Root != control.Root || !OrderList[i].CanFocus || OrderList[i].IsRoot || !OrderList[i].Enabled) && i != start);
            OrderList[i].Focused = true;
        }

        private void ProcessArrows(Control control, KeyEventArgs kbe)
        {
            Control c = control;
            if (c.Parent != null && c.Parent.Controls != null)
            {
                int index = -1;

                if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Left && !kbe.Handled)
                {
                    int miny = int.MaxValue;
                    int minx = int.MinValue;
                    for (int i = 0; i < (c.Parent.Controls as ControlList).Count; i++)
                    {
                        Control cx = (c.Parent.Controls as ControlList)[i];
                        if (cx == c || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cay = (int)(c.Top + (c.Height / 2));
                        int cby = (int)(cx.Top + (cx.Height / 2));

                        if (Math.Abs(cay - cby) <= miny && (cx.Left + cx.Width) >= minx && (cx.Left + cx.Width) <= c.Left)
                        {
                            miny = Math.Abs(cay - cby);
                            minx = cx.Left + cx.Width;
                            index = i;
                        }
                    }
                }
                else if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Right && !kbe.Handled)
                {
                    int miny = int.MaxValue;
                    int minx = int.MaxValue;
                    for (int i = 0; i < (c.Parent.Controls as ControlList).Count; i++)
                    {
                        Control cx = (c.Parent.Controls as ControlList)[i];
                        if (cx == c || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cay = (int)(c.Top + (c.Height / 2));
                        int cby = (int)(cx.Top + (cx.Height / 2));

                        if (Math.Abs(cay - cby) <= miny && cx.Left <= minx && cx.Left >= (c.Left + c.Width))
                        {
                            miny = Math.Abs(cay - cby);
                            minx = cx.Left;
                            index = i;
                        }
                    }
                }
                else if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Up && !kbe.Handled)
                {
                    int miny = int.MinValue;
                    int minx = int.MaxValue;
                    for (int i = 0; i < (c.Parent.Controls as ControlList).Count; i++)
                    {
                        Control cx = (c.Parent.Controls as ControlList)[i];
                        if (cx == c || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cax = (int)(c.Left + (c.Width / 2));
                        int cbx = (int)(cx.Left + (cx.Width / 2));

                        if (Math.Abs(cax - cbx) <= minx && (cx.Top + cx.Height) >= miny && (cx.Top + cx.Height) <= c.Top)
                        {
                            minx = Math.Abs(cax - cbx);
                            miny = cx.Top + cx.Height;
                            index = i;
                        }
                    }
                }
                else if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Down && !kbe.Handled)
                {
                    int miny = int.MaxValue;
                    int minx = int.MaxValue;
                    for (int i = 0; i < (c.Parent.Controls as ControlList).Count; i++)
                    {
                        Control cx = (c.Parent.Controls as ControlList)[i];
                        if (cx == c || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cax = (int)(c.Left + (c.Width / 2));
                        int cbx = (int)(cx.Left + (cx.Width / 2));

                        if (Math.Abs(cax - cbx) <= minx && cx.Top <= miny && cx.Top >= (c.Top + c.Height))
                        {
                            minx = Math.Abs(cax - cbx);
                            miny = cx.Top;
                            index = i;
                        }
                    }
                }

                if (index != -1)
                {
                    (c.Parent.Controls as ControlList)[index].Focused = true;
                    kbe.Handled = true;
                }
            }
        }

        private void MouseDownProcess(object sender, MouseEventArgs e)
        {
            ControlList c = new ControlList();
            c.AddRange(OrderList);

            if (autoUnfocus && focusedControl != null && focusedControl.Root != modalWindow)
            {
                bool hit = false;
                foreach (Control cx in Controls)
                {
                    if (cx.AbsoluteRect.Contains(e.Position))
                    {
                        hit = true;
                        break;
                    }
                }
                if (!hit)
                {
                    for (int i = 0; i < Control.Stack.Count; i++)
                    {
                        if (Control.Stack[i].Visible && Control.Stack[i].Detached && Control.Stack[i].AbsoluteRect.Contains(e.Position))
                        {
                            hit = true;
                            break;
                        }
                    }
                }
                if (!hit) focusedControl.Focused = false;
            }

            for (int i = c.Count - 1; i >= 0; i--)
            {
                if (CheckState(c[i]) && CheckPosition(c[i], e.Position))
                {
                    states.Buttons[(int)e.Button] = c[i];
                    c[i].SendMessage(Message.MouseDown, e);

                    if (states.Click == -1)
                    {
                        states.Click = (int)e.Button;

                        if (FocusedControl != null)
                            FocusedControl.Invalidate();

                        c[i].Focused = true;
                    }
                    return;
                }
            }

            if (ModalWindow != null)
                SystemSounds.Beep.Play();
        }

        private void MouseUpProcess(object sender, MouseEventArgs e)
        {
            Control c = states.Buttons[(int)e.Button];
            if (c != null)
            {
                if (CheckPosition(c, e.Position) && CheckOrder(c, e.Position) && states.Click == (int)e.Button && CheckButtons((int)e.Button))
                {
                    c.SendMessage(Message.Click, e);
                }
                states.Click = -1;
                c.SendMessage(Message.MouseUp, e);
                states.Buttons[(int)e.Button] = null;
                MouseMoveProcess(sender, e);
            }
        }

        private void MousePressProcess(object sender, MouseEventArgs e)
        {
            Control c = states.Buttons[(int)e.Button];
            if (c != null)
            {
                if (CheckPosition(c, e.Position))
                    c.SendMessage(Message.MousePress, e);
            }
        }

        private void MouseMoveProcess(object sender, MouseEventArgs e)
        {
            ControlList c = new ControlList();
            c.AddRange(OrderList);

            for (int i = c.Count - 1; i >= 0; i--)
            {
                bool chpos = CheckPosition(c[i], e.Position);
                bool chsta = CheckState(c[i]);

                if (chsta && ((chpos && states.Over == c[i]) || (states.Buttons[(int)e.Button] == c[i])))
                {
                    c[i].SendMessage(Message.MouseMove, e);
                    break;
                }
            }

            for (int i = c.Count - 1; i >= 0; i--)
            {
                bool chpos = CheckPosition(c[i], e.Position);
                bool chsta = CheckState(c[i]) || (c[i].ToolTip.Text != "" && c[i].ToolTip.Text != null && c[i].Visible);

                if (chsta && !chpos && states.Over == c[i] && states.Buttons[(int)e.Button] == null)
                {
                    states.Over = null;
                    c[i].SendMessage(Message.MouseOut, e);
                    break;
                }
            }

            for (int i = c.Count - 1; i >= 0; i--)
            {
                bool chpos = CheckPosition(c[i], e.Position);
                bool chsta = CheckState(c[i]) || (c[i].ToolTip.Text != "" && c[i].ToolTip.Text != null && c[i].Visible);

                if (chsta && chpos && states.Over != c[i] && states.Buttons[(int)e.Button] == null)
                {
                    if (states.Over != null)
                        states.Over.SendMessage(Message.MouseOut, e);

                    states.Over = c[i];
                    c[i].SendMessage(Message.MouseOver, e);
                    break;
                }
                else if (states.Over == c[i]) break;
            }
        }

        void KeyDownProcess(object sender, KeyEventArgs e)
        {
            Control c = FocusedControl;
            if (c != null && CheckState(c))
            {
                if (states.Click == -1)
                    states.Click = (int)MouseButton.None;

                states.Buttons[(int)MouseButton.None] = c;
                c.SendMessage(Message.KeyDown, e);

                if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
                    c.SendMessage(Message.Click, new MouseEventArgs(new MouseState(), MouseButton.None, Point.Zero));
            }
        }

        void KeyUpProcess(object sender, KeyEventArgs e)
        {
            Control c = states.Buttons[(int)MouseButton.None];
            if (c != null)
            {
                if (e.Key == Microsoft.Xna.Framework.Input.Keys.Space)
                    c.SendMessage(Message.Click, new MouseEventArgs(new MouseState(), MouseButton.None, Point.Zero));

                states.Click = -1;
                states.Buttons[(int)MouseButton.None] = null;
                c.SendMessage(Message.KeyUp, e);
            }
        }

        void KeyPressProcess(object sender, KeyEventArgs e)
        {
            Control c = states.Buttons[(int)MouseButton.None];
            if (c != null)
            {
                c.SendMessage(Message.KeyPress, e);

                if ((e.Key == Microsoft.Xna.Framework.Input.Keys.Right ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Left ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Up ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Down) && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    ProcessArrows(c, e);
                    KeyDownProcess(sender, e);
                }
                else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Tab && !e.Shift && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    TabNextControl(c);
                    KeyDownProcess(sender, e);
                }
                else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Tab && e.Shift && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    TabPrevControl(c);
                    KeyDownProcess(sender, e);
                }
            }
        }
        #endregion
    }
}
