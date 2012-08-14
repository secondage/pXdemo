using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Beetle;
using System.Windows.Forms;
using NetSyncObject;
using System.Security.Cryptography;

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
                else if (plrm.Result == ProjectXServer.Messages.LoginResult.Failed_Notfound)
                {
                    MessageBox.Show("用户名不存在");
                }
                else if (plrm.Result == ProjectXServer.Messages.LoginResult.Failed_Password)
                {
                    MessageBox.Show("密码错误");
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
            else if (adapter.Message is ProjectXServer.Messages.PlayerTimeSyncMsg)
            {
                ProjectXServer.Messages.PlayerTimeSyncMsg msg = (ProjectXServer.Messages.PlayerTimeSyncMsg)adapter.Message;
                GameConst.ServerDurationTime = msg.Duration;
                GameConst.ServerTotalTime = msg.Total;
            }
            else if (adapter.Message is ProjectXServer.Messages.PlayerPositionUpdate)
            {
               ProjectXServer.Messages.PlayerPositionUpdate msg = (ProjectXServer.Messages.PlayerPositionUpdate)adapter.Message;
               CurrentScene.UpdatePlayerPosition(msg);
            }
            else if (adapter.Message is ProjectXServer.Messages.PlayerMoveRequest)
            {
                ProjectXServer.Messages.PlayerMoveRequest msg = (ProjectXServer.Messages.PlayerMoveRequest)adapter.Message;
                CurrentScene.UpdatePlayerMovement(msg);
            }
            else if (adapter.Message is ProjectXServer.Messages.PlayerTargetChanged)
            {
                ProjectXServer.Messages.PlayerTargetChanged msg = (ProjectXServer.Messages.PlayerTargetChanged)adapter.Message;
                CurrentScene.UpdatePlayerTarget(msg);
            }
        }

        private void channel_ChannelDisposed(object sender, ChannelDisposedEventArgs e)
        {
        }

        public static void LoginToServer(string username, string password)
        {
            MD5 m = new MD5CryptoServiceProvider();
            byte[] s = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(password));
            ProjectXServer.Messages.PlayerLoginRequestMsg plm = new ProjectXServer.Messages.PlayerLoginRequestMsg();
            plm.Name = username;
            plm.Password = BitConverter.ToString(s);

            ProjectXServer.Messages.ProtobufAdapter.Send(clientchannel, plm);
        }

        public static void SendRequestMovementMsg(Character player)
        {
            ProjectXServer.Messages.PlayerMoveRequest msg = new ProjectXServer.Messages.PlayerMoveRequest();
            msg.Target = new float[2];
            msg.Target[0] = player.Target.X;
            msg.Target[1] = player.Target.Y;
            msg.Position = new float[2];
            msg.Position[0] = player.Position.X;
            msg.Position[1] = player.Position.Y;
            ProjectXServer.Messages.ProtobufAdapter.Send(clientchannel, msg);
        }

        public static void SendTargetChangedMsg(Player player)
        {
            ProjectXServer.Messages.PlayerTargetChanged msg = new ProjectXServer.Messages.PlayerTargetChanged();
            msg.Target = new float[2];
            msg.Target[0] = player.Target.X;
            msg.Target[1] = player.Target.Y;
            msg.Position = new float[2];
            msg.Position[0] = player.Position.X;
            msg.Position[1] = player.Position.Y;
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

        public static void SendMoveFinishMsg(Character player)
        {
            ProjectXServer.Messages.PlayerStopRequest msg = new ProjectXServer.Messages.PlayerStopRequest();
            msg.Position = new float[2];
            msg.Position[0] = player.Position.X;
            msg.Position[1] = player.Position.Y;
            ProjectXServer.Messages.ProtobufAdapter.Send(clientchannel, msg);
        }
    }
}
