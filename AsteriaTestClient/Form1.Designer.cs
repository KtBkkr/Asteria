namespace AsteriaClient
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.logBox = new System.Windows.Forms.ListBox();
            this.groupCharMngt = new System.Windows.Forms.GroupBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCreateCharacter = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDeleteCharacter = new System.Windows.Forms.Button();
            this.groupData = new System.Windows.Forms.GroupBox();
            this.lblStance = new System.Windows.Forms.Label();
            this.lblRotation = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblMana = new System.Windows.Forms.Label();
            this.lblHealth = new System.Windows.Forms.Label();
            this.lblPosition = new System.Windows.Forms.Label();
            this.lblExperience = new System.Windows.Forms.Label();
            this.lblLevel = new System.Windows.Forms.Label();
            this.lblGold = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblClass = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelmana = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupCharSelect = new System.Windows.Forms.GroupBox();
            this.lstCharacters = new System.Windows.Forms.DataGridView();
            this.groupChat = new System.Windows.Forms.GroupBox();
            this.txtChat = new System.Windows.Forms.TextBox();
            this.chatBox = new System.Windows.Forms.ListBox();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Class = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.level = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnQuit = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.comboClass = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.groupCharMngt.SuspendLayout();
            this.groupData.SuspendLayout();
            this.groupCharSelect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstCharacters)).BeginInit();
            this.groupChat.SuspendLayout();
            this.SuspendLayout();
            // 
            // logBox
            // 
            this.logBox.FormattingEnabled = true;
            this.logBox.HorizontalScrollbar = true;
            this.logBox.Location = new System.Drawing.Point(411, 12);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(506, 472);
            this.logBox.TabIndex = 0;
            // 
            // groupCharMngt
            // 
            this.groupCharMngt.Controls.Add(this.label12);
            this.groupCharMngt.Controls.Add(this.comboClass);
            this.groupCharMngt.Controls.Add(this.btnStart);
            this.groupCharMngt.Controls.Add(this.btnCreateCharacter);
            this.groupCharMngt.Controls.Add(this.txtName);
            this.groupCharMngt.Controls.Add(this.label11);
            this.groupCharMngt.Controls.Add(this.label1);
            this.groupCharMngt.Controls.Add(this.btnDeleteCharacter);
            this.groupCharMngt.Location = new System.Drawing.Point(12, 12);
            this.groupCharMngt.Name = "groupCharMngt";
            this.groupCharMngt.Size = new System.Drawing.Size(393, 212);
            this.groupCharMngt.TabIndex = 1;
            this.groupCharMngt.TabStop = false;
            this.groupCharMngt.Text = "Character Management";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(272, 129);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(99, 35);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start Selected";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnCreateCharacter
            // 
            this.btnCreateCharacter.Location = new System.Drawing.Point(272, 43);
            this.btnCreateCharacter.Name = "btnCreateCharacter";
            this.btnCreateCharacter.Size = new System.Drawing.Size(99, 37);
            this.btnCreateCharacter.TabIndex = 3;
            this.btnCreateCharacter.Text = "Create Character";
            this.btnCreateCharacter.UseVisualStyleBackColor = true;
            this.btnCreateCharacter.Click += new System.EventHandler(this.btnCreateCharacter_Click);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(70, 38);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(196, 20);
            this.txtName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name:";
            // 
            // btnDeleteCharacter
            // 
            this.btnDeleteCharacter.Location = new System.Drawing.Point(272, 170);
            this.btnDeleteCharacter.Name = "btnDeleteCharacter";
            this.btnDeleteCharacter.Size = new System.Drawing.Size(99, 23);
            this.btnDeleteCharacter.TabIndex = 2;
            this.btnDeleteCharacter.Text = "Delete Selected";
            this.btnDeleteCharacter.UseVisualStyleBackColor = true;
            this.btnDeleteCharacter.Click += new System.EventHandler(this.btnDeleteCharacter_Click);
            // 
            // groupData
            // 
            this.groupData.Controls.Add(this.btnQuit);
            this.groupData.Controls.Add(this.btnLogout);
            this.groupData.Controls.Add(this.lblStance);
            this.groupData.Controls.Add(this.lblRotation);
            this.groupData.Controls.Add(this.lblType);
            this.groupData.Controls.Add(this.lblMana);
            this.groupData.Controls.Add(this.lblHealth);
            this.groupData.Controls.Add(this.lblPosition);
            this.groupData.Controls.Add(this.lblExperience);
            this.groupData.Controls.Add(this.lblLevel);
            this.groupData.Controls.Add(this.lblGold);
            this.groupData.Controls.Add(this.lblName);
            this.groupData.Controls.Add(this.lblClass);
            this.groupData.Controls.Add(this.label10);
            this.groupData.Controls.Add(this.label9);
            this.groupData.Controls.Add(this.label8);
            this.groupData.Controls.Add(this.label7);
            this.groupData.Controls.Add(this.label6);
            this.groupData.Controls.Add(this.label5);
            this.groupData.Controls.Add(this.label4);
            this.groupData.Controls.Add(this.labelmana);
            this.groupData.Controls.Add(this.label3);
            this.groupData.Controls.Add(this.label14);
            this.groupData.Controls.Add(this.label2);
            this.groupData.Location = new System.Drawing.Point(12, 12);
            this.groupData.Name = "groupData";
            this.groupData.Size = new System.Drawing.Size(393, 212);
            this.groupData.TabIndex = 2;
            this.groupData.TabStop = false;
            this.groupData.Text = "Game Data";
            this.groupData.Visible = false;
            // 
            // lblStance
            // 
            this.lblStance.AutoSize = true;
            this.lblStance.Location = new System.Drawing.Point(234, 116);
            this.lblStance.Name = "lblStance";
            this.lblStance.Size = new System.Drawing.Size(53, 13);
            this.lblStance.TabIndex = 9;
            this.lblStance.Text = "Unknown";
            // 
            // lblRotation
            // 
            this.lblRotation.AutoSize = true;
            this.lblRotation.Location = new System.Drawing.Point(234, 170);
            this.lblRotation.Name = "lblRotation";
            this.lblRotation.Size = new System.Drawing.Size(53, 13);
            this.lblRotation.TabIndex = 9;
            this.lblRotation.Text = "Unknown";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(234, 95);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(53, 13);
            this.lblType.TabIndex = 9;
            this.lblType.Text = "Unknown";
            // 
            // lblMana
            // 
            this.lblMana.AutoSize = true;
            this.lblMana.Location = new System.Drawing.Point(234, 62);
            this.lblMana.Name = "lblMana";
            this.lblMana.Size = new System.Drawing.Size(53, 13);
            this.lblMana.TabIndex = 9;
            this.lblMana.Text = "Unknown";
            // 
            // lblHealth
            // 
            this.lblHealth.AutoSize = true;
            this.lblHealth.Location = new System.Drawing.Point(234, 42);
            this.lblHealth.Name = "lblHealth";
            this.lblHealth.Size = new System.Drawing.Size(53, 13);
            this.lblHealth.TabIndex = 9;
            this.lblHealth.Text = "Unknown";
            // 
            // lblPosition
            // 
            this.lblPosition.AutoSize = true;
            this.lblPosition.Location = new System.Drawing.Point(84, 170);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(53, 13);
            this.lblPosition.TabIndex = 9;
            this.lblPosition.Text = "Unknown";
            // 
            // lblExperience
            // 
            this.lblExperience.AutoSize = true;
            this.lblExperience.Location = new System.Drawing.Point(84, 138);
            this.lblExperience.Name = "lblExperience";
            this.lblExperience.Size = new System.Drawing.Size(53, 13);
            this.lblExperience.TabIndex = 9;
            this.lblExperience.Text = "Unknown";
            // 
            // lblLevel
            // 
            this.lblLevel.AutoSize = true;
            this.lblLevel.Location = new System.Drawing.Point(84, 116);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(53, 13);
            this.lblLevel.TabIndex = 9;
            this.lblLevel.Text = "Unknown";
            // 
            // lblGold
            // 
            this.lblGold.AutoSize = true;
            this.lblGold.Location = new System.Drawing.Point(84, 94);
            this.lblGold.Name = "lblGold";
            this.lblGold.Size = new System.Drawing.Size(53, 13);
            this.lblGold.TabIndex = 9;
            this.lblGold.Text = "Unknown";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(84, 42);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(53, 13);
            this.lblName.TabIndex = 8;
            this.lblName.Text = "Unknown";
            // 
            // lblClass
            // 
            this.lblClass.AutoSize = true;
            this.lblClass.Location = new System.Drawing.Point(84, 62);
            this.lblClass.Name = "lblClass";
            this.lblClass.Size = new System.Drawing.Size(53, 13);
            this.lblClass.TabIndex = 8;
            this.lblClass.Text = "Unknown";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(178, 170);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 13);
            this.label10.TabIndex = 7;
            this.label10.Text = "Rotation:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(31, 170);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "Position:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(184, 116);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Stance:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(183, 94);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(52, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "TypeID:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(15, 138);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Experience:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(42, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Level:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(46, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Gold:";
            // 
            // labelmana
            // 
            this.labelmana.AutoSize = true;
            this.labelmana.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelmana.Location = new System.Drawing.Point(193, 62);
            this.labelmana.Name = "labelmana";
            this.labelmana.Size = new System.Drawing.Size(42, 13);
            this.labelmana.TabIndex = 1;
            this.labelmana.Text = "Mana:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(187, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Health:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(42, 62);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(41, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "Class:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(40, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            // 
            // groupCharSelect
            // 
            this.groupCharSelect.Controls.Add(this.lstCharacters);
            this.groupCharSelect.Location = new System.Drawing.Point(12, 230);
            this.groupCharSelect.Name = "groupCharSelect";
            this.groupCharSelect.Size = new System.Drawing.Size(393, 254);
            this.groupCharSelect.TabIndex = 3;
            this.groupCharSelect.TabStop = false;
            this.groupCharSelect.Text = "Character Selection";
            // 
            // lstCharacters
            // 
            this.lstCharacters.AllowUserToAddRows = false;
            this.lstCharacters.AllowUserToDeleteRows = false;
            this.lstCharacters.AllowUserToResizeRows = false;
            this.lstCharacters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lstCharacters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lstCharacters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.name,
            this.Class,
            this.level});
            this.lstCharacters.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.lstCharacters.Location = new System.Drawing.Point(6, 19);
            this.lstCharacters.MultiSelect = false;
            this.lstCharacters.Name = "lstCharacters";
            this.lstCharacters.ReadOnly = true;
            this.lstCharacters.RowHeadersVisible = false;
            this.lstCharacters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lstCharacters.ShowCellErrors = false;
            this.lstCharacters.ShowCellToolTips = false;
            this.lstCharacters.ShowEditingIcon = false;
            this.lstCharacters.ShowRowErrors = false;
            this.lstCharacters.Size = new System.Drawing.Size(381, 229);
            this.lstCharacters.TabIndex = 0;
            this.lstCharacters.SelectionChanged += new System.EventHandler(this.lstCharacters_SelectionChanged);
            // 
            // groupChat
            // 
            this.groupChat.Controls.Add(this.txtChat);
            this.groupChat.Controls.Add(this.chatBox);
            this.groupChat.Location = new System.Drawing.Point(12, 230);
            this.groupChat.Name = "groupChat";
            this.groupChat.Size = new System.Drawing.Size(393, 254);
            this.groupChat.TabIndex = 4;
            this.groupChat.TabStop = false;
            this.groupChat.Text = "Chat Output";
            this.groupChat.Visible = false;
            // 
            // txtChat
            // 
            this.txtChat.Location = new System.Drawing.Point(6, 228);
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new System.Drawing.Size(381, 20);
            this.txtChat.TabIndex = 1;
            // 
            // chatBox
            // 
            this.chatBox.FormattingEnabled = true;
            this.chatBox.Location = new System.Drawing.Point(6, 23);
            this.chatBox.Name = "chatBox";
            this.chatBox.Size = new System.Drawing.Size(381, 199);
            this.chatBox.TabIndex = 0;
            // 
            // id
            // 
            this.id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.id.HeaderText = "ID";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            // 
            // name
            // 
            this.name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.name.FillWeight = 93.55151F;
            this.name.HeaderText = "Name";
            this.name.Name = "name";
            this.name.ReadOnly = true;
            // 
            // Class
            // 
            this.Class.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Class.HeaderText = "Class";
            this.Class.Name = "Class";
            this.Class.ReadOnly = true;
            // 
            // level
            // 
            this.level.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.level.FillWeight = 90.81004F;
            this.level.HeaderText = "Level";
            this.level.Name = "level";
            this.level.ReadOnly = true;
            // 
            // btnLogout
            // 
            this.btnLogout.Location = new System.Drawing.Point(303, 84);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(73, 23);
            this.btnLogout.TabIndex = 10;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // btnQuit
            // 
            this.btnQuit.Location = new System.Drawing.Point(303, 116);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(73, 23);
            this.btnQuit.TabIndex = 10;
            this.btnQuit.Text = "Quit";
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(29, 67);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 1;
            this.label11.Text = "Class:";
            // 
            // comboClass
            // 
            this.comboClass.FormattingEnabled = true;
            this.comboClass.Items.AddRange(new object[] {
            "Mage - Fist full o boom stick!"});
            this.comboClass.Location = new System.Drawing.Point(70, 64);
            this.comboClass.Name = "comboClass";
            this.comboClass.Size = new System.Drawing.Size(196, 21);
            this.comboClass.TabIndex = 4;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(123, 138);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(23, 24);
            this.label12.TabIndex = 5;
            this.label12.Text = ":)";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 496);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.groupData);
            this.Controls.Add(this.groupCharSelect);
            this.Controls.Add(this.groupChat);
            this.Controls.Add(this.groupCharMngt);
            this.Name = "Form1";
            this.Text = "Asteria Test Client";
            this.groupCharMngt.ResumeLayout(false);
            this.groupCharMngt.PerformLayout();
            this.groupData.ResumeLayout(false);
            this.groupData.PerformLayout();
            this.groupCharSelect.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lstCharacters)).EndInit();
            this.groupChat.ResumeLayout(false);
            this.groupChat.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox logBox;
        private System.Windows.Forms.GroupBox groupCharMngt;
        private System.Windows.Forms.Button btnCreateCharacter;
        private System.Windows.Forms.Button btnDeleteCharacter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.GroupBox groupData;
        private System.Windows.Forms.GroupBox groupCharSelect;
        private System.Windows.Forms.DataGridView lstCharacters;
        private System.Windows.Forms.GroupBox groupChat;
        private System.Windows.Forms.TextBox txtChat;
        private System.Windows.Forms.ListBox chatBox;
        private System.Windows.Forms.Label lblStance;
        private System.Windows.Forms.Label lblRotation;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblHealth;
        private System.Windows.Forms.Label lblPosition;
        private System.Windows.Forms.Label lblExperience;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Label lblGold;
        private System.Windows.Forms.Label lblClass;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblMana;
        private System.Windows.Forms.Label labelmana;
        private System.Windows.Forms.ComboBox comboClass;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Class;
        private System.Windows.Forms.DataGridViewTextBoxColumn level;
        private System.Windows.Forms.Label label12;
    }
}

