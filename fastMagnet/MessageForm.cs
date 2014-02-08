/*
Copyright 2014 Oguz Kartal

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace fastMagnet
{
    public partial class MessageForm : Form
    {
        public enum MessageType
        {
            Info,
            Error
        }

        private string msgText, caption;
        private Timer waiterTmr;
        private int waitMs;
        private MessageType type;

        public MessageForm(string message, string caption, MessageType type,int waitInterval, Size boxSize)
        {
            InitializeComponent();

            this.msgText = message;
            this.caption = caption;

            this.type = type;
            
            this.Width = boxSize.Width;
            this.Height = boxSize.Height;

            this.waitMs = waitInterval;

            this.waiterTmr = new Timer();
            this.waiterTmr.Tick += waiterTmr_Tick;
        }

        public MessageForm(string message, string caption, MessageType type, int waitInterval)
            : this(message,caption, type,waitInterval,new Size(200,120))
        {
        }

        public MessageForm(string message, string caption, MessageType type)
            : this(message, caption, type, 3000, new Size(200,120))
        {
        }

        public MessageForm(string message, string caption)
            : this(message, caption, MessageType.Info)
        {
        }

        void waiterTmr_Tick(object sender, EventArgs e)
        {
            this.waiterTmr.Stop();
            this.Close();
        }

        private void frmInfo_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;

            switch (this.type)
            {
                case MessageType.Info:
                    this.lblMessage.ForeColor = Color.Black;
                    break;
                case MessageType.Error:
                    this.lblMessage.ForeColor = Color.Red;
                    break;
            }

            this.lblCaption.Text = this.caption + " - fastMagnet";
            this.lblMessage.Text = this.msgText;

            this.waiterTmr.Interval = this.waitMs;
            this.waiterTmr.Start();
        }

    }
}
