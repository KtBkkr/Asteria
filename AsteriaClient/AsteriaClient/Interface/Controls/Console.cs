using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace AsteriaClient.Interface.Controls
{
    public struct ConsoleMessage
    {
        #region Fields
        public string Text;
        public byte Channel;
        public DateTime Time;
        public bool Send;
        #endregion

        #region Constructors
        public ConsoleMessage(string text, byte channel)
        {
            this.Text = text;
            this.Channel = channel;
            this.Time = DateTime.Now;
            this.Send = true;
        }
        #endregion
    }

    [Flags]
    public enum ConsoleMessageFormats
    {
        None = 0x00,
        ChannelName = 0x01,
        TimeStamp = 0x02,
        All = ChannelName | TimeStamp
    }

    public class ConsoleChannel
    {
        #region Fields
        private string name;
        private byte index;
        private Color color;
        private bool visible;
        #endregion

        #region Constructors
        public ConsoleChannel(byte index, string name, Color color, bool visible)
        {
            this.name = name;
            this.index = index;
            this.color = color;
            this.visible = visible;
        }
        #endregion

        #region Properties
        public virtual byte Index
        {
            get { return index; }
            set { index = value; }
        }

        public virtual Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public virtual bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }
        #endregion
    }

    public class ChannelList : EventedList<ConsoleChannel>
    {
        #region Properties
        public ConsoleChannel this[string name]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    ConsoleChannel s = (ConsoleChannel)this[i];
                    if (s.Name.ToLower() == name.ToLower())
                        return s;
                }
                return default(ConsoleChannel);
            }
            set
            {
                for (int i = 0; i < this.Count; i++)
                {
                    ConsoleChannel s = (ConsoleChannel)this[i];
                    if (s.Name.ToLower() == name.ToLower())
                        this[i] = value;
                }
            }
        }

        public ConsoleChannel this[byte index]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    ConsoleChannel s = (ConsoleChannel)this[i];
                    if (s.Index == index)
                        return s;
                }
                return default(ConsoleChannel);
            }

            set
            {
                for (int i = 0; i < this.Count; i++)
                {
                    ConsoleChannel s = (ConsoleChannel)this[i];
                    if (s.Index == index)
                        this[i] = value;
                }
            }
        }
        #endregion
    }

    public class Console : Container
    {
        #region Fields
        private TextBox txtMain = null;
        private ComboBox cmbMain;
        private ScrollBar sbVert;
        private EventedList<ConsoleMessage> buffer = new EventedList<ConsoleMessage>();
        private ChannelList channels = new ChannelList();
        private List<byte> filter = new List<byte>();
        private ConsoleMessageFormats messageFormat = ConsoleMessageFormats.None;
        private bool channelsVisible = true;
        private bool textBoxVisible = true;
        #endregion

        #region Properties
        public virtual EventedList<ConsoleMessage> MessageBuffer
        {
            get { return buffer; }
            set
            {
                buffer.ItemAdded -= new EventHandler(buffer_ItemAdded);
                buffer = value;
                buffer.ItemAdded += new EventHandler(buffer_ItemAdded);
            }
        }

        public virtual ChannelList Channels
        {
            get { return channels; }
            set
            {
                channels.ItemAdded -= new EventHandler(channels_ItemAdded);
                channels = value;
                channels.ItemAdded += new EventHandler(channels_ItemAdded);
                channels_ItemAdded(null, null);
            }
        }

        public virtual List<byte> ChannelFilter
        {
            get { return filter; }
            set { filter = value; }
        }

        public virtual int SelectedChannel
        {
            set { cmbMain.Text = channels[value].Name; }
            get { return channels[cmbMain.Text].Index; }
        }

        public virtual ConsoleMessageFormats MessageFormat
        {
            get { return messageFormat; }
            set { messageFormat = value; }
        }

        public virtual bool ChannelsVisible
        {
            get { return channelsVisible; }
            set
            {
                cmbMain.Visible = channelsVisible = value;
                if (value && !textBoxVisible) TextBoxVisible = false;
                PositionControls();
            }
        }

        public virtual bool TextBoxVisible
        {
            get { return textBoxVisible; }
            set
            {
                txtMain.Visible = textBoxVisible = value;
                if (!value && channelsVisible) ChannelsVisible = false;
                PositionControls();
            }
        }
        #endregion

        #region Events
        public event ConsoleMessageEventHandler MessageSent;
        #endregion

        #region Constructors
        public Console(Manager manager)
            : base(manager)
        {
            Width = 320;
            Height = 160;
            MinimumHeight = 64;
            MinimumWidth = 64;
            CanFocus = false;

            Resizable = false;
            Movable = false;

            cmbMain = new ComboBox(manager);
            cmbMain.Init();
            cmbMain.Top = Height - cmbMain.Height;
            cmbMain.Left = 0;
            cmbMain.Width = 128;
            cmbMain.Anchor = Anchors.Left | Anchors.Bottom;
            cmbMain.Detached = false;
            cmbMain.DrawSelection = false;
            cmbMain.Visible = channelsVisible;
            Add(cmbMain, false);

            txtMain = new TextBox(manager);
            txtMain.Init();
            txtMain.Top = Height - txtMain.Height;
            txtMain.Left = cmbMain.Width + 1;
            txtMain.Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right;
            txtMain.Detached = false;
            txtMain.Visible = textBoxVisible;
            txtMain.KeyDown += new KeyEventHandler(txtMain_KeyDown);
            txtMain.FocusGained += new EventHandler(txtMain_FocusGained);
            Add(txtMain, false);

            sbVert = new ScrollBar(manager, Orientation.Vertical);
            sbVert.Init();
            sbVert.Top = 2;
            sbVert.Left = Width - 18;
            sbVert.Anchor = Anchors.Right | Anchors.Top | Anchors.Bottom;
            sbVert.Range = 1;
            sbVert.PageSize = 1;
            sbVert.Value = 0;
            sbVert.ValueChanged += new EventHandler(sbVert_ValueChanged);
            Add(sbVert, false);

            ClientArea.Draw += new DrawEventHandler(ClientArea_Draw);

            buffer.ItemAdded += new EventHandler(buffer_ItemAdded);
            channels.ItemAdded += new EventHandler(channels_ItemAdded);
            channels.ItemRemoved += new EventHandler(channels_ItemRemoved);

            PositionControls();
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
            Skin = new SkinControl(Manager.Skin.Controls["Console"]);

            PositionControls();
        }

        protected internal override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        private void PositionControls()
        {
            if (txtMain != null)
            {
                txtMain.Left = channelsVisible ? cmbMain.Width + 1 : 0;
                txtMain.Width = channelsVisible ? Width - cmbMain.Width - 1 : Width;

                if (textBoxVisible)
                {
                    ClientMargins = new Margins(Skin.ClientMargins.Left, Skin.ClientMargins.Top + 4, sbVert.Width + 6, txtMain.Height + 4);
                    sbVert.Height = Height - txtMain.Height - 5;
                }
                else
                {
                    ClientMargins = new Margins(Skin.ClientMargins.Left, Skin.ClientMargins.Top + 4, sbVert.Width + 6, 2);
                    sbVert.Height = Height - 4;
                }
                Invalidate();
            }
        }

        void ClientArea_Draw(object sender, DrawEventArgs e)
        {
            SpriteFont font = Skin.Layers[0].Text.Font.Resource;
            Rectangle r = new Rectangle(e.Rectangle.Left, e.Rectangle.Top, e.Rectangle.Width, e.Rectangle.Height);
            int pos = 0;

            if (buffer.Count > 0)
            {
                EventedList<ConsoleMessage> b = GetFilteredBuffer(filter);
                int c = b.Count;
                int s = (sbVert.Value + sbVert.PageSize);
                int f = s - sbVert.PageSize;

                if (b.Count > 0)
                {
                    for (int i = s - 1; i >= f; i--)
                    {
                        {
                            int x = r.Left + 4;
                            int y = r.Bottom - (pos + 1) * (int)font.LineSpacing;

                            string msg = ((ConsoleMessage)b[i]).Text;
                            string pre = "";
                            ConsoleChannel ch = (channels[((ConsoleMessage)b[i]).Channel] as ConsoleChannel);

                            if ((messageFormat & ConsoleMessageFormats.ChannelName) == ConsoleMessageFormats.ChannelName)
                                pre += string.Format("[{0}]", channels[((ConsoleMessage)b[i]).Channel].Name);

                            if ((messageFormat & ConsoleMessageFormats.TimeStamp) == ConsoleMessageFormats.TimeStamp)
                                pre = string.Format("[{0}]", ((ConsoleMessage)b[i]).Time.ToLongTimeString()) + pre;

                            if (pre != "") msg = pre + " " + msg;

                            string line = "";
                            string[] words = msg.Split(" ".ToCharArray());

                            List<string> wrappedLines = new List<string>();
                            for (int w = 0; w < words.Length; w++)
                            {
                                if (font.MeasureString(line + words[w]).X > ClientWidth)
                                {
                                    wrappedLines.Add(line);
                                    line = words[w] + " ";
                                }
                                else
                                    line += words[w] + " ";
                            }

                            wrappedLines.Add(line);
                            wrappedLines.Reverse();
                            foreach (string l in wrappedLines)
                            {
                                y = r.Bottom - (pos + 1) * (int)font.LineSpacing;
                                e.Renderer.DrawString(font, l, x, y, ch.Color);
                                pos += 1;
                            }
                        }
                    }
                }
            }
        }

        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            int h = txtMain.Visible ? (txtMain.Height + 1) : 0;
            Rectangle r = new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height - h);
            base.DrawControl(renderer, r, gameTime);
        }

        void txtMain_FocusGained(object sender, EventArgs e)
        {
            ConsoleChannel ch = channels[cmbMain.Text];
            if (ch != null) txtMain.TextColor = ch.Color;
        }

        void txtMain_KeyDown(object sender, KeyEventArgs e)
        {
            SendMessage(e);
        }

        private void SendMessage(EventArgs x)
        {
            if (Manager.UseGuide && Guide.IsVisible) return;

            KeyEventArgs k = x as KeyEventArgs;
            ConsoleChannel ch = channels[cmbMain.Text];
            if (ch != null)
            {
                txtMain.TextColor = ch.Color;

                string message = txtMain.Text;
                if ((k.Key == Microsoft.Xna.Framework.Input.Keys.Enter) && message != null && message != "")
                {
                    x.Handled = true;

                    ConsoleMessageEventArgs me = new ConsoleMessageEventArgs(new ConsoleMessage(message, ch.Index));
                    OnMessageSent(me);

                    if (me.Message.Send)
                        buffer.Add(new ConsoleMessage(me.Message.Text, me.Message.Channel));

                    txtMain.Text = "";
                    ClientArea.Invalidate();

                    CalcScrolling();
                }
            }
        }

        protected virtual void OnMessageSent(ConsoleMessageEventArgs e)
        {
            if (MessageSent != null) MessageSent.Invoke(this, e);
        }

        void channels_ItemAdded(object sender, EventArgs e)
        {
            cmbMain.Items.Clear();
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Visible)
                    cmbMain.Items.Add((channels[i] as ConsoleChannel).Name);
            }
        }

        void channels_ItemRemoved(object sender, EventArgs e)
        {
            cmbMain.Items.Clear();
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Visible)
                    cmbMain.Items.Add((channels[i] as ConsoleChannel).Name);
            }
        }

        void buffer_ItemAdded(object sender, EventArgs e)
        {
            CalcScrolling();
            ClientArea.Invalidate();
        }

        private void CalcScrolling()
        {
            if (sbVert != null)
            {
                int line = Skin.Layers[0].Text.Font.Resource.LineSpacing;
                int c = GetFilteredBuffer(filter).Count;
                int p = (int)Math.Ceiling(ClientArea.ClientHeight / (float)line);

                sbVert.Range = c == 0 ? 1 : c;
                sbVert.PageSize = c == 0 ? 1 : p;
                sbVert.Value = sbVert.Range;
            }
        }

        void sbVert_ValueChanged(object sender, EventArgs e)
        {
            ClientArea.Invalidate();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            CalcScrolling();
            base.OnResize(e);
        }

        private EventedList<ConsoleMessage> GetFilteredBuffer(List<byte> filter)
        {
            EventedList<ConsoleMessage> ret = new EventedList<ConsoleMessage>();

            if (filter.Count > 0)
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    if (filter.Contains(((ConsoleMessage)buffer[i]).Channel))
                        ret.Add(buffer[i]);
                }
                return ret;
            }
            else return buffer;
        }
        #endregion
    }
}
