using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Beetle;
using System.Windows.Forms;
using NetSyncObject;

namespace demo
{
    public partial class MainGame : Microsoft.Xna.Framework.Game
    {
        private void ReceiveMessage(PacketRecieveMessagerArgs e)
        {
            ProjectXServer.Messages.ProtobufAdapter adapter = (ProjectXServer.Messages.ProtobufAdapter)e.Message;
            if (adapter.Message is ProjectXServer.Messages.PlayerLoginResultMsg)
            {
                ProjectXServer.Messages.PlayerLoginResultMsg plrm = (ProjectXServer.Messages.PlayerLoginResultMsg)adapter.Message;
                if (plrm.Result == ProjectXServer.Messages.LoginResult.Failed_AlreadyLogin)
                {
                    MessageBox.Show("重复登录");
                }
                else if (plrm.Result == ProjectXServer.Messages.LoginResult.Failed)
                {
                    MessageBox.Show("登录失败");
                }
                else
                {
                    try
                    {
                        dlgLogin.Invoke(new Action(() =>
                        {
                            dlgLogin.Close();
                            dlgLogin.Dispose();
                            dlgLogin = null;
                        }));
                     }
                    catch (InvalidOperationException ie)
                    {
                        MessageBox.Show(ie.Message);
                    }
                    ClientID = plrm.ClientID;
                }
            }
            else if (adapter.Message is ProjectXServer.Messages.PlayerLoginMsg)
            {
                ProjectXServer.Messages.PlayerLoginMsg plm = (ProjectXServer.Messages.PlayerLoginMsg)adapter.Message;
                PlayerNet pn = new PlayerNet();
                pn.ClientID = plm.ClientID;
                pn.Name = plm.Name;
                CreatePlayer(pn);
            }
            else if (adapter.Message is ProjectXServer.Messages.PlayerLogoutMsg)
            {
                ProjectXServer.Messages.PlayerLogoutMsg plm = (ProjectXServer.Messages.PlayerLogoutMsg)adapter.Message;
                PlayerNet pn = new PlayerNet();
                pn.ClientID = plm.ClientID;
                pn.Name = plm.Name;
                DestoryPlayer(pn);
            }
        }

        private void channel_ChannelDisposed(object sender, ChannelDisposedEventArgs e)
        {
        }

        public static void LoginToServer(string username, string password)
        {
            ProjectXServer.Messages.PlayerLoginMsg plm = new ProjectXServer.Messages.PlayerLoginMsg();
            plm.Name = username;
            ProjectXServer.Messages.ProtobufAdapter.Send(clientchannel, plm);
        }

    }
}
