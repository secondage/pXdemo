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
            ProtobufAdapter adapter = (ProtobufAdapter)e.Message;
            if (adapter.Message is PlayerLoginResultMsg)
            {
                PlayerLoginResultMsg plrm = (PlayerLoginResultMsg)adapter.Message;
                if (plrm.Result == LoginResult.Failed_AlreadyLogin)
                {
                    MessageBox.Show("重复登录");
                }
                else if (plrm.Result == LoginResult.Failed)
                {
                    MessageBox.Show("登录失败");
                }
                else if (plrm.Result == LoginResult.Failed_Notfound)
                {
                    MessageBox.Show("用户名不存在");
                }
                else if (plrm.Result == LoginResult.Failed_Password)
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
            else if (adapter.Message is PlayerLoginSelfMsg)
            {
                PlayerLoginSelfMsg plm = (PlayerLoginSelfMsg)adapter.Message;
                CreateLocalPlayer(plm);
            }
            else if (adapter.Message is PlayerLoginMsg)
            {
                PlayerLoginMsg plm = (PlayerLoginMsg)adapter.Message;
                CreatePlayer(plm);
            }
            else if (adapter.Message is PlayerLogoutMsg)
            {
                PlayerLogoutMsg plm = (PlayerLogoutMsg)adapter.Message;
                DestoryPlayer(plm);
            }
            else if (adapter.Message is PlayerTimeSyncMsg)
            {
                PlayerTimeSyncMsg msg = (PlayerTimeSyncMsg)adapter.Message;
                GameConst.ServerDurationTime = msg.Duration;
                GameConst.ServerTotalTime = msg.Total;
            }
            else if (adapter.Message is PlayerPositionUpdate)
            {
               PlayerPositionUpdate msg = (PlayerPositionUpdate)adapter.Message;
               CurrentScene.UpdatePlayerPosition(msg);
            }
            else if (adapter.Message is PlayerMoveRequest)
            {
                PlayerMoveRequest msg = (PlayerMoveRequest)adapter.Message;
                CurrentScene.UpdatePlayerMovement(msg);
            }
            else if (adapter.Message is PlayerTargetChanged)
            {
                PlayerTargetChanged msg = (PlayerTargetChanged)adapter.Message;
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
            PlayerLoginRequestMsg plm = new PlayerLoginRequestMsg();
            plm.Name = username;
            plm.Password = BitConverter.ToString(s);

            ProtobufAdapter.Send(clientchannel, plm);
        }

        public static void SendRequestMovementMsg(Character player)
        {
            PlayerMoveRequest msg = new PlayerMoveRequest();
            msg.Target = new float[2];
            msg.Target[0] = player.Target.X;
            msg.Target[1] = player.Target.Y;
            msg.Position = new float[2];
            msg.Position[0] = player.Position.X;
            msg.Position[1] = player.Position.Y;
            ProtobufAdapter.Send(clientchannel, msg);
        }

        public static void SendTargetChangedMsg(Player player)
        {
            PlayerTargetChanged msg = new PlayerTargetChanged();
            msg.Target = new float[2];
            msg.Target[0] = player.Target.X;
            msg.Target[1] = player.Target.Y;
            msg.Position = new float[2];
            msg.Position[0] = player.Position.X;
            msg.Position[1] = player.Position.Y;
            ProtobufAdapter.Send(clientchannel, msg);
        }

        public static void SendMoveReportMsg(Character player)
        {
            PlayerPositioReport msg = new PlayerPositioReport();
            msg.Position = new float[2];
            msg.Position[0] = player.Position.X;
            msg.Position[1] = player.Position.Y;
            ProtobufAdapter.Send(clientchannel, msg);
        }

        public static void SendMoveFinishMsg(Character player)
        {
            PlayerStopRequest msg = new PlayerStopRequest();
            msg.Position = new float[2];
            msg.Position[0] = player.Position.X;
            msg.Position[1] = player.Position.Y;
            ProtobufAdapter.Send(clientchannel, msg);
        }
    }
}
