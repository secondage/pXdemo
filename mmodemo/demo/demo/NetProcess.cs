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
                CreatePlayer(plm);
            }
            else if (adapter.Message is ProjectXServer.Messages.PlayerLogoutMsg)
            {
                ProjectXServer.Messages.PlayerLogoutMsg plm = (ProjectXServer.Messages.PlayerLogoutMsg)adapter.Message;
                DestoryPlayer(plm);
            }
            else if (adapter.Message is ProjectXServer.Messages.PlayerPositionUpdate)
            {
                ProjectXServer.Messages.PlayerPositionUpdate msg = (ProjectXServer.Messages.PlayerPositionUpdate)adapter.Message;
                CurrentScene.SyncPlayer(msg);
            }
        }

        private void channel_ChannelDisposed(object sender, ChannelDisposedEventArgs e)
        {
        }

        public static void LoginToServer(string username, string password)
        {
            ProjectXServer.Messages.PlayerLoginRequestMsg plm = new ProjectXServer.Messages.PlayerLoginRequestMsg();
            plm.Name = username;

            ProjectXServer.Messages.ProtobufAdapter.Send(clientchannel, plm);
        }

        public static void SendRequestMovementMsg(Character player)
        {
            ProjectXServer.Messages.PlayerMoveRequest msg = new ProjectXServer.Messages.PlayerMoveRequest();
            msg.Target = new float[2];
            msg.Target[0] = player.Target.X;
            msg.Target[1] = player.Target.Y;
            ProjectXServer.Messages.ProtobufAdapter.Send(clientchannel, msg);
        }

        public static void SendMoveReportMsg(Character player)
        {
            ProjectXServer.Messages.PlayerPositioReport msg = new ProjectXServer.Messages.PlayerPositioReport();
            msg.Position = new float[2];
            msg.Position[0] = player.Position.X;
            msg.Position[1] = player.Position.Y;
            ProjectXServer.Messages.ProtobufAdapter.Send(clientchannel, msg);
        }

    }
}
