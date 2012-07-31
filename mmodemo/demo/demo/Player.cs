using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using demo.uicontrols;

namespace demo
{
    public class Player : Character
    {
        public Player(string n, Scene s) :
            base(n, s)
        {
            atk = 134;
            def = 22;
        }


        

        Dictionary<int, Quest> quests = new Dictionary<int, Quest>();
        Dictionary<int, Quest> quests_completed = new Dictionary<int, Quest>();

        List<Npc> visiblenpcs = new List<Npc>();
        List<int> completedquest = new List<int>();

        private bool interacting = false;
        public bool Interacting
        {
            get
            {
                return interacting;
            }
        }

        public long ClientID { get; set; }

        public List<int> CompletedQuests
        {
            get
            {
                return completedquest;
            }
        }

        void UpdateSceneScroll()
        {
            if (scene.State == demo.Scene.SceneState.Battle)
                return;
            Vector4 vp = scene.Viewport;
            float x = position.X - vp.Z * 0.5f;
            float y = position.Y - vp.W * 0.5f;

            x = MathHelper.Clamp(x, 0.0f, (GameConst.BackgroundScale) * 2048.0f - GameConst.ScreenWidth);
            y = MathHelper.Clamp(y, 0.0f, (GameConst.BackgroundScale) * 2048.0f - GameConst.ScreenHeight);

            scene.SetViewportPos(x, y);
        }


        private void UpdateQuestTrack(Quest q)
        {

            UIElement trackd = UIMgr.GetUIControlByName("dlg_questtrck");
            if (trackd != null)
            {
                if (scene.State == demo.Scene.SceneState.Map)
                {
                    if (trackd.State == RenderChunk.RenderChunkState.Hide)
                        trackd.State = RenderChunk.RenderChunkState.FadeIn;
                }
                UITextBlock t = trackd.GetUIControlByName("text_questname") as UITextBlock;
                if (t != null)
                {
                    t.Text = q.Name;
                }
                t = trackd.GetUIControlByName("text_questtrack") as UITextBlock;
                if (t != null)
                {
                    t.Text = q.Script.GetTrackString(0);
                }
            }

        }


        public void AddQuest(Quest q)
        {
            if (!quests.ContainsKey(q.ID))
            {
                Quest qnew = new Quest(q);
                qnew.Script.Player = this;
                if (qnew.Script.GetTakeable())
                {
                    quests.Add(q.ID, qnew);
                    qnew.Script.Takeup();
                    UpdateQuestTrack(qnew);
                }
                else
                    qnew = null;
            }

        }

        public void AddVisibleNpc(Npc npc)
        {
            visiblenpcs.Add(npc);
        }

        List<Npc> _removenpclist = new List<Npc>();
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
            foreach (KeyValuePair<int, Quest> q in quests_completed)
            {
                foreach (Npc npc in visiblenpcs)
                {
                    if (npc.ExistQuest(q.Value.ID))
                    {
                        npc.Title.IconType = CharacterTitle.IconTypes.QuestCompleted;
                        _removenpclist.Add(npc);
                    }
                }
                foreach (Npc npc in _removenpclist)
                {
                    visiblenpcs.Remove(npc);
                }
                _removenpclist.Clear();
            }

            foreach (KeyValuePair<int, Quest> q in quests)
            {
                foreach (Npc npc in visiblenpcs)
                {
                    if (npc.ExistQuest(q.Value.ID))
                    {
                        npc.Title.IconType = CharacterTitle.IconTypes.QuestNonCompleted;
                        _removenpclist.Add(npc);
                    }
                }
                foreach (Npc npc in _removenpclist)
                {
                    visiblenpcs.Remove(npc);
                }
                _removenpclist.Clear();
            }
            foreach (Npc npc in visiblenpcs)
            {
                if (npc.ExistTakeableQuest(this))
                {
                    npc.Title.IconType = CharacterTitle.IconTypes.TakeQuest;
                    //visiblenpcs.Remove(npc);
                }
                else
                    npc.Title.IconType = CharacterTitle.IconTypes.None;
            }
            visiblenpcs.Clear();

            UIElement d = UIMgr.GetUIControlByName("dialognpc");
            if (d != null && d.State == RenderChunk.RenderChunkState.Show)
            {
                if (interactivetarget != null)
                {
                    float dist = Vector2.Distance(interactivetarget.Picture.Position, position);
                    if (dist > 300)
                    {
                        interactivetarget = null;
                        d.State = RenderChunk.RenderChunkState.FadeOutToDel;

                    }
                }
            }
        }

        protected override void UpdateMovement(GameTime gametime)
        {
            Vector2 dir = target - position;
            if (dir.Length() < GameConst.DistanceFactor)
            {
                position = target;
                State = CharacterState.Landing;
                if (facedirmethod == DirMethod.AutoDectect)
                {
                    if (interactivetarget != null)
                    {
                        if (interactivetarget.Position.X > position.X)
                            pic.Direction = new Vector2(interactivetarget.Picture.Direction.X >= 0 ? 1 : -1, 0); //跟交互目标相对
                        else
                            pic.Direction = new Vector2(interactivetarget.Picture.Direction.X >= 0 ? -1 : 1, 0); //跟交互目标相对

                    }
                    else
                    {
                        pic.Direction = new Vector2(dir.X >= 0 ? 1 : -1, 0); //停止时面向移动方向
                    }
                }
                else if (facedirmethod == DirMethod.Fixed)
                {
                    pic.Direction = fixedfacedir;
                }

                SendOnArrived(this);
                
            }
            else
            {
                dir.Normalize();
                position += ((float)gametime.ElapsedGameTime.TotalSeconds * speed) * dir;
                pic.Position = position;// - pic.FrameSize * 0.5f;
            }
            UpdateSceneScroll();
            base.UpdateMovement(gametime);
        }

        public void CloseBattleMenu()
        {
            UIDialog dialog = UIMgr.GetUIControlByName("playerbattlemenu") as UIDialog;
            if (dialog != null)
            {
                dialog.State = RenderChunk.RenderChunkState.FadeOutToDel;
            }
        }

        public void MakeBattleMenu()
        {
            UIDialog dialog = UIMgr.GetUIControlByName("playerbattlemenu") as UIDialog;
            if (dialog == null)
            {
                dialog = UIMgr.AddUIControl("UIDialog", "playerbattlemenu", (int)150, (int)GameConst.ScreenHeight / 2 - 90, 70, 100, -1, 99, this) as UIDialog;
            }
            if (dialog != null)
            {
                UITextButton btn = dialog.GetUIControlByName("atkbtn") as UITextButton;
                if (btn == null)
                {
                    btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                }
                if (btn != null)
                {
                    btn.Text = "攻击";
                    btn.FontColor = Color.DeepSkyBlue;
                    dialog.AddUIControl(btn, "atkbtn", 2, 2, 66, 20, -1, this);
                }
                btn = dialog.GetUIControlByName("magbtn") as UITextButton;
                if (btn == null)
                {
                    btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                }
                if (btn != null)
                {
                    btn.Text = "法术";
                    btn.FontColor = Color.DeepSkyBlue;
                    dialog.AddUIControl(btn, "magbtn", 2, 23, 66, 20, -1, this);
                }
                btn = dialog.GetUIControlByName("itembtn") as UITextButton;
                if (btn == null)
                {
                    btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                }
                if (btn != null)
                {
                    btn.Text = "道具";
                    btn.FontColor = Color.DeepSkyBlue;
                    dialog.AddUIControl(btn, "itembtn", 2, 44, 66, 20, -1, this);
                }
            }
        }


        protected void atkbtn_OnClick(object sender, EventArgs e)
        {
            UITextButton btn = sender as UITextButton;
            btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
            this.op = OperateType.Attack;

        }

        protected void magbtn_OnClick(object sender, EventArgs e)
        {
            UITextButton btn = sender as UITextButton;
            btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
            this.op = OperateType.Magic;

        }

        protected void itembtn_OnClick(object sender, EventArgs e)
        {
            UITextButton btn = sender as UITextButton;
            btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;

        }

        private void MakeQuestUI(Quest quest)
        {
            UIDialog dialog = UIMgr.GetUIControlByName("dialognpc") as UIDialog;
            if (dialog == null)
                dialog = UIMgr.AddUIControl("Dialog_Npc", "dialognpc", (int)UILayout.Center, (int)UILayout.Bottom, 0, 0, -1, 99, this) as UIDialog;
            if (dialog != null)
            {
                UITextBlock text = dialog.GetUIControlByName("npctalk") as UITextBlock;
                if (text == null)
                {
                    text = UIMgr.CreateUIControl("UITextBlock") as UITextBlock;
                }
                if (text != null)
                {
                    if (quests_completed.ContainsKey(quest.ID))
                    {
                        text.Text = quest.Talk_Noncompleted;
                    }
                    else if (quests.ContainsKey(quest.ID))
                    {
                        text.Text = quest.Talk_Noncompleted;
                    }
                    else
                    {
                        text.Text = quest.Talk;
                    }

                    text.FontColor = Color.Black;
                    dialog.AddUIControl(text, "npctalk", 18, 47, 441, 138, -1, this);
                }


                UITextButton btn = dialog.GetUIControlByName("okbtn") as UITextButton;
                if (btn == null)
                {
                    btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                }
                if (btn != null)
                {
                    if (quests_completed.ContainsKey(quest.ID))
                    {
                        if (quest.ID == 3)
                            btn.Text = "真不敢相信，我做到了！";
                        else
                            btn.Text = "已经消灭殆尽了！";
                    }
                    else if (quests.ContainsKey(quest.ID))
                    {
                        btn.Text = "我这就去！";
                    }
                    else
                    {
                        btn.Text = quest.OKContent; ;
                    }
                    btn.UserData = quest;
                    btn.FontColor = Color.DeepSkyBlue;
                    dialog.AddUIControl(btn, "okbtn", 20, 200, 300, 20, -1, this);
                }

                UITextButton btn1 = dialog.GetUIControlByName("cancelbtn") as UITextButton;
                if (btn1 == null)
                {
                    btn1 = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                }
                if (btn1 != null)
                {
                    if (quests_completed.ContainsKey(quest.ID))
                    {
                        btn1.Text = "";
                        btn1.State = RenderChunk.RenderChunkState.Hide;
                    }
                    else if (quests.ContainsKey(quest.ID))
                    {
                        btn1.Text = "";
                        btn1.State = RenderChunk.RenderChunkState.Hide;
                    }
                    else
                    {
                        if (quest.CancelContent != "")
                        {
                            btn1.Text = quest.CancelContent;
                            btn1.State = RenderChunk.RenderChunkState.Show;
                        }
                        else
                        {
                            btn1.Text = "";
                            btn1.State = RenderChunk.RenderChunkState.Hide;
                        }
                    }

                    btn.FontColor = Color.DeepSkyBlue;
                    dialog.AddUIControl(btn1, "cancelbtn", 20, 221, 300, 20, -1, this);
                }




                UIImage npcface1 = dialog.GetUIControlByName("testimage") as UIImage;
                if (npcface1 == null)
                {
                    npcface1 = UIMgr.CreateUIControl("UIImage") as UIImage;
                }
                if (npcface1 != null)
                {
                    npcface1.Texture = GameConst.Content.Load<Texture2D>(@"npcface/NPC01");
                    dialog.AddUIControl(npcface1, "testimage", 0, -npcface1.Texture.Height, 0, 0, -1, this);
                }

            }

        }

        private void MakeBossUI()
        {
            UIDialog dialog = UIMgr.GetUIControlByName("dialognpc") as UIDialog;
            if (dialog == null)
                dialog = UIMgr.AddUIControl("Dialog_Npc", "dialogboss", (int)UILayout.Center, (int)UILayout.Bottom, 0, 0, -1, 99, this) as UIDialog;
            if (dialog != null)
            {
                UITextBlock text = dialog.GetUIControlByName("npctalk") as UITextBlock;
                if (text == null)
                {
                    text = UIMgr.CreateUIControl("UITextBlock") as UITextBlock;
                }
                if (text != null)
                {
                    text.Text = "什么？无知的凡人，你真的对自己的性命不再珍惜了吗？";

                    text.FontColor = Color.Black;
                    dialog.AddUIControl(text, "npctalk", 18, 47, 441, 138, -1, this);
                }


                UITextButton btn = dialog.GetUIControlByName("okbtn") as UITextButton;
                if (btn == null)
                {
                    btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                }
                if (btn != null)
                {

                    btn.Text = "受死吧！";
                    btn.FontColor = Color.DeepSkyBlue;
                    dialog.AddUIControl(btn, "okbtn", 20, 200, 300, 20, -1, this);
                }

                UITextButton btn1 = dialog.GetUIControlByName("cancelbtn") as UITextButton;
                if (btn1 == null)
                {
                    btn1 = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                }
                if (btn1 != null)
                {
                    btn1.Text = "我先想一下。。。";
                    btn.FontColor = Color.DeepSkyBlue;
                    dialog.AddUIControl(btn1, "cancelbtn", 20, 221, 300, 20, -1, this);
                }

                UIImage npcface1 = dialog.GetUIControlByName("testimage") as UIImage;
                if (npcface1 == null)
                {
                    npcface1 = UIMgr.CreateUIControl("UIImage") as UIImage;
                }
                if (npcface1 != null)
                {
                    npcface1.Texture = GameConst.Content.Load<Texture2D>(@"npcface/NPC02");
                    dialog.AddUIControl(npcface1, "testimage", 0, -npcface1.Texture.Height, 0, 0, -1, this);
                }

            }

        }

        public void InteractiveToNpc(Npc npc)
        {
            if (npc.ExistTakeableQuest(this))
            {
                Quest quest = npc.GetFirstQuest(this);
                if (quest != null)
                {
                    interacting = true;
                    MakeQuestUI(quest);
                }

            }
            else
            {
                if (npc.Name == "boss")
                {
                    MakeBossUI();
                }
            }
        }

        protected override void Notify(GameAction action, object p1, object p2)
        {
            UpdateQuest(action, p1, p2);
            base.Notify(action, p1, p2);
        }

        private bool DeliveryQuest(Quest q)
        {
            return q.Script.Delivery();
        }

        private bool UpdateQuest(GameAction action, object p1, object p2)
        {
            int uidirty = 0;
            foreach (KeyValuePair<int, Quest> p in quests)
            {
                if (p.Value.Script.Update(action, p1, p2))
                {
                    UpdateQuestTrack(p.Value);
                    if (p.Value.Script.GetAccomplished())
                        quests_completed.Add(p.Key, p.Value);
                }
            }
            foreach (KeyValuePair<int, Quest> p in quests_completed)
            {
                //quests.Remove(p.Key);
            }
            return uidirty != 0;
        }

        protected void npctalk_OnClose(object sender, EventArgs e)
        {
            interacting = false;
            interactivetarget = null;
        }

        protected void okbtn_OnClick(object sender, EventArgs e)
        {
            UITextButton btn = sender as UITextButton;
            Quest q = btn.UserData as Quest;
            if (q != null)
            {
                if (quests_completed.ContainsKey(q.ID))
                {
                    //完成任务
                    if (DeliveryQuest(quests_completed[q.ID]))
                    {
                        quests_completed.Remove(q.ID);
                        quests.Remove(q.ID);
                        completedquest.Add(q.ID);
                        if (q.NextQuest != null)
                        {
                            MakeQuestUI(q.NextQuest);
                        }
                        else
                        {
                            btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
                        }
                    }
                    if (quests.Count == 0)
                    {
                         UIElement trackd = UIMgr.GetUIControlByName("dlg_questtrck");
                         if (trackd != null)
                         {
                             trackd.State = RenderChunk.RenderChunkState.FadeOut;
                         }
                    }
                }
                else if (quests.ContainsKey(q.ID))
                {
                    btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
                }
                else
                {
                    AddQuest(btn.UserData as Quest);
                    btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
                }
            }
            else
            {
                //boss dlg
                btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
                scene.IntoBattle();
            }

        }

        protected void cancelbtn_OnClick(object sender, EventArgs e)
        {
            UITextButton btn = sender as UITextButton;
            btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
            
        }


        public bool Debug_GetQuest()
        {
            return quests.Count > 0;
        }

        //public void UpdateQuest
    }
}
