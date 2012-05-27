using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    public class ExitDialog : Dialog
    {
        #region Fields
        private Button btnYes;
        private Button btnNo;
        private Label lblMessage;
        private ImageBox imgIcon;
        #endregion

        #region Constructors
        public ExitDialog(Manager manager)
            : base(manager)
        {
            //string msg = "Do you really want to exit " + Manager.Game.Window.Title + "?";
            string msg = "Do you really want to exit?";
            ClientWidth = (int)Manager.Skin.Controls["Label"].Layers[0].Text.Font.Resource.MeasureString(msg).X + 48 + 16 + 16 + 16;
            ClientHeight = 120;
            TopPanel.Visible = false;
            IconVisible = true;
            Resizable = false;
            Text = Manager.Game.Window.Title;
            Center();

            imgIcon = new ImageBox(Manager);
            imgIcon.Init();
            imgIcon.Image = Manager.Skin.Images["Icon.Question"].Resource;
            imgIcon.Left = 16;
            imgIcon.Top = 16;
            imgIcon.Width = 48;
            imgIcon.Height = 48;
            imgIcon.SizeMode = SizeMode.Stretched;

            lblMessage = new Label(Manager);
            lblMessage.Init();

            lblMessage.Left = 80;
            lblMessage.Top = 16;
            lblMessage.Width = ClientWidth - lblMessage.Left;
            lblMessage.Height = 48;
            lblMessage.Alignment = Alignment.TopLeft;
            lblMessage.Text = msg;

            btnYes = new Button(Manager);
            btnYes.Init();
            btnYes.Left = (BottomPanel.ClientWidth / 2) - btnYes.Width - 4;
            btnYes.Top = 8;
            btnYes.Text = "Yes";
            btnYes.ModalResult = ModalResult.Yes;

            btnNo = new Button(Manager);
            btnNo.Init();
            btnNo.Left = (BottomPanel.ClientWidth / 2) + 4;
            btnNo.Top = 8;
            btnNo.Text = "No";
            btnNo.ModalResult = ModalResult.No;

            Add(imgIcon);
            Add(lblMessage);
            BottomPanel.Add(btnYes);
            BottomPanel.Add(btnNo);

            DefaultControl = btnNo;
        }
        #endregion

        #region Methods
        public override void Init()
        {
            base.Init();
        }
        #endregion
    }
}
