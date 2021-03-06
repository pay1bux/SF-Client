﻿using MetroFramework.Forms;
using DiscordRpcNet;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using SF_Client.DiscordRPC;

namespace SF_Client
{
    public partial class Main : MetroForm
    {
        public Main()
        {
            InitializeComponent();
            new SplashScreen().ShowDialog();
            language.SelectedIndex = 0;
            Main.CheckForIllegalCrossThreadCalls = false;
        }

        private string Username { get; set; }

        private void Discord_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/vkYs2Xx");
        }

        private void Login_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(userNameTextbox.Text) || string.IsNullOrEmpty(userPassTextbox.Text))
                MessageBox.Show("Please enter a username and password", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                metroTabControl1.SelectedIndex = 0;
                webBrowser1.Navigate("http://www.seafight.com/");
                return;
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.DocumentText.Contains("<div class=\"bgcdw_errors_flash\">") || webBrowser1.DocumentText.Contains("data-error=\"bgc.error.password_isTooShort\""))
            {
                MessageBox.Show("Please enter correct username and password", "Attention", MessageBoxButtons.OK);
                return;
            }
            else if (webBrowser1.DocumentText.Contains("bgcdw_login_form_username"))
            {
                Username = userNameTextbox.Text;
                HtmlElement lname = webBrowser1.Document.GetElementById("bgcdw_login_form_username");
                HtmlElement lpass = webBrowser1.Document.GetElementById("bgcdw_login_form_password");
                if (!lname.GetAttribute("value").Equals(userNameTextbox.Text))
                {
                    lname.SetAttribute("value", userNameTextbox.Text);
                }
                if (!lpass.GetAttribute("value").Equals(userPassTextbox.Text))
                {
                    lpass.SetAttribute("value", userPassTextbox.Text);
                }
                try
                {
                    foreach (HtmlElement element in webBrowser1.Document.All)
                    {
                        if (element.GetAttribute("className") == "bgcdw_button bgcdw_login_form_login")
                        {
                            element.InvokeMember("click");
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            else if (e.Url.AbsoluteUri.Contains("&loginDone=true"))
            {
                webBrowser1.Navigate(webBrowser1.Url.AbsoluteUri.Replace("/index.es?action=internalStart&loginDone=true", "/index.es?action=internalMap"));
            }
            else if (webBrowser1.Url.ToString().Contains("/index.es?action=internalMap"))
            {
                try
                {
                    string swf = webBrowser1.Document.GetElementById("sffclient").GetAttribute("data");
                    string var = "";
                    foreach (HtmlElement item in webBrowser1.Document.GetElementsByTagName("param"))
                    {
                        if (item.Name == "FlashVars")
                        {
                            var = item.GetAttribute("value").Replace(item.GetAttribute("value").Split(new string[] { "isFastReg=1&" }, StringSplitOptions.None)[1].Split(new string[] { "&quality" }, StringSplitOptions.None)[0], "lang=" + language.GetItemText(language.SelectedItem).ToLower());
                            var = var.Replace(var.Split(new string[] { "platform=" }, StringSplitOptions.None)[1].Split(new string[] { "&" }, StringSplitOptions.None)[0], "PC_CLIENT");
                            break;
                        }
                    }
                    FlashPlayer _ = new FlashPlayer()
                    {
                        FlashVariables = var,
                        SwfClient = swf,
                        Username = this.Username,
                    };
                    _.Show();
                    webBrowser1.Navigate(webBrowser1.Url.AbsoluteUri.Replace("?action=internalMap", "?action=internalStart"));
                    this.WindowState = FormWindowState.Minimized;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Lanuguage = language.SelectedIndex;
            Properties.Settings.Default.Username = userNameTextbox.Text;
            Properties.Settings.Default.Password = userPassTextbox.Text;
            Properties.Settings.Default.DiscordRPC_Active = DiscordRPC_Active.Checked;
            Properties.Settings.Default.DiscordRPC_Description = DiscordRPC_Description.Text;

            Properties.Settings.Default.Save();

            DiscordRpc.Shutdown();

            Application.Exit();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            language.SelectedIndex = Properties.Settings.Default.Lanuguage;
            userNameTextbox.Text = Properties.Settings.Default.Username;
            userPassTextbox.Text = Properties.Settings.Default.Password;
            DiscordRPC_Image.Image = Images.ImageList[Properties.Settings.Default.DiscordRPC_Image];
            DiscordRPC_Active.Checked = Properties.Settings.Default.DiscordRPC_Active;
            DiscordRPC_Description.Text = Properties.Settings.Default.DiscordRPC_Description;
        }

        void UpdatePresence()
        {
            if (DiscordRPC_Active.Checked)
            {
                DiscordRpc.EventHandlers handlers = new DiscordRpc.EventHandlers();
                DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
                DiscordRpc.Initialize("738137354589437972", ref handlers, true, null);
                presence.details = DiscordRPC_Description.Text;
                presence.largeImageKey = Properties.Settings.Default.DiscordRPC_Image;
                presence.largeImageText = "Seafight";
                presence.startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                DiscordRpc.UpdatePresence(ref presence);
            }
            else
            {
                DiscordRpc.Shutdown();
            }
        }

        private void DiscordRPC_Image_MouseEnter(object sender, EventArgs e)
        {
            DiscordRPC_Image_Change.BringToFront();
        }

        private void DiscordRPC_Image_Change_MouseLeave(object sender, EventArgs e)
        {
            DiscordRPC_Image.BringToFront();
        }

        private void DiscordRPC_Image_Change_Click(object sender, EventArgs e)
        {
            SelectImage _ = new SelectImage();
            _.ShowDialog();
            if (_.Selected != null)
            {
                Properties.Settings.Default.DiscordRPC_Image = _.Selected;
                DiscordRPC_Image.Image = Images.ImageList[Properties.Settings.Default.DiscordRPC_Image];
                UpdatePresence();
            }
        }

        private void DiscordRPC_Active_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePresence();
        }

        private void DiscordRPC_Description_TextChanged(object sender, EventArgs e)
        {
            UpdatePresence();
        }
    }
}
