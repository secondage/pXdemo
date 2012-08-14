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
using System.Xml;
using System.Diagnostics;
using demo.uicontrols;
using System.Threading;

namespace demo
{
    public class Scene
    {
        public enum SceneState
        {
            Map,
            Battle,
            ToMap,
            ToBattle,
        };

        enum Turn
        {
            Player,
            Enemy,
        };


        private Turn turn = Turn.Enemy;
        private Vector4 viewport;
        private Vector4 actualSize;
        private PreRenderEffect countdowneffect;

        private SceneState state = SceneState.Map;

        private Random random = new Random();

        private List<RenderChunk> renderchunks = new List<RenderChunk>();
        private List<RenderChunk> renderchunksdefer = new List<RenderChunk>();
        private List<NetPlayer> netplayers = new List<NetPlayer>();
        private List<Character> characters = new List<Character>();
        private List<Character> charactersdefer = new List<Character>();
        private List<Character> battlecharacters = new List<Character>();
        private List<Spell> spells = new List<Spell>();
        private string name;
        public MiniMap minimap = null;
        private bool battlefini = false;

        private Vector2 wind = new Vector2(1, 0);
        private Player localplayer;
        private System.Object lockThis = new System.Object();

        private Background fadebg;

        public Scene(string _name, int x, int y, int w, int h)
        {
            name = _name;
            viewport = new Vector4(x, y, w, h);
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public SceneState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        public Player Player
        {
            get
            {
                return localplayer;
            }
            set
            {
                localplayer = value;
                localplayer.OnActionCompleted += new EventHandler(Player_OnActionCompleted);
            }
        }




        public Vector4 Viewport
        {
            get
            {
                return viewport;
            }
            set
            {
                viewport = value;
            }
        }

        public Vector4 ActualSize
        {
            get
            {
                return actualSize;
            }
            set
            {
                actualSize = value;
            }
        }

        public Vector2 Wind
        {
            get
            {
                return wind;
            }
            set
            {
                wind = value;
                Vector2.Normalize(wind);
            }
        }

        public void RenderPrepositive()
        {
            if (minimap != null)
            {
                minimap.RenderPre();
            }
        }

        public void RenderPostpositive()
        {
            if (minimap != null)
            {
                minimap.RenderPost();
            }
        }

        public void Render(SpriteBatch sb)
        {
            lock (lockThis)
            {
                foreach (RenderChunk rc in renderchunks)
                {
                    //if (rc is Background || rc is Cloud)
                    //    continue;
                    //if (rc is PreRenderEffect)
                    //    continue;
                    rc.Render(sb);
                }
            }

            if (state == SceneState.Battle && _roundtime >= 0)
            {
                int sec = (int)_roundtime;
                if (sec >= 10)
                {
                    int t = sec / 10;
                    int c = sec - t * 10;

                    Vector2 p = countdowneffect.Position;
                    p.X = (GameConst.ScreenWidth - (int)(countdowneffect.FrameSize.X * 2.0f)) / 2;
                    p.Y = 40.0f;

                    countdowneffect.Position = p;
                    countdowneffect.RenderFrame(t, sb);

                    p.X += countdowneffect.FrameSize.X;
                    countdowneffect.Position = p;
                    countdowneffect.RenderFrame(c, sb);
                }
                else
                {
                    Vector2 p = countdowneffect.Position;
                    p.X = (GameConst.ScreenWidth - (int)(countdowneffect.FrameSize.X)) / 2;
                    p.Y = 40.0f;

                    countdowneffect.Position = p;
                    countdowneffect.RenderFrame(sec, sb);
                }
            }

        }

        List<RenderChunk> _removelist = new List<RenderChunk>();
        double _statetoggletime = 0.0;
        double _statetoggledur = 1.3;

        double _changebgtime = 0.0;
        double _changebgdur = 1.0;

        int _fightseed = 500;
        public void Update(GameTime gametime)
        {
            if (state == SceneState.Map)
            {
                foreach (Character ch in characters)
                {
                    if (!(ch is Player))
                    {
                        if (ch is Npc)
                        {
                            if (localplayer != null)
                            {
                                localplayer.AddVisibleNpc(ch as Npc);
                            }
                        }
                        ch.Update(gametime);
                    }
                }
                if (localplayer != null)
                {
                    localplayer.Update(gametime);
                    if (localplayer.State == CharacterState.Moving)
                    {
                        int f = random.Next(_fightseed);
                        if (f == 0)
                        {
                            if (localplayer.Debug_GetQuest() && !localplayer.CompletedQuests.Contains(2))
                                IntoBattle();
                            _fightseed = random.Next(1000) + 500;
                        }
                        else
                        {
                            _fightseed--;
                        }
                    }
                }
                foreach (Player p in netplayers)
                {
                    p.Update(gametime);
                }
            }
            else if (state == SceneState.Battle)
            {
                foreach (Character ch in characters)
                {
                    if (ch is Player)
                        ch.Update(gametime);
                }
                foreach (Character ch in battlecharacters)
                {
                    ch.Update(gametime);
                }
                foreach (Spell s in spells)
                {
                    s.Update(gametime);
                }
                UpdatePlayerTurn(gametime);
                UpdateBattleResult();
                UpdateNextActionRound(gametime);
            }


            foreach (Character ch in charactersdefer)
            {
                AddCharacter(ch);
            }
            charactersdefer.Clear();

            foreach (RenderChunk rc in renderchunksdefer)
            {
                AddRenderChunk(rc);
            }
            if (renderchunksdefer.Count > 0)
                SortRenderChunksByLayer();
            renderchunksdefer.Clear();


            _removelist.Clear();
            lock (lockThis)
            {
                foreach (RenderChunk rc in renderchunks)
                {
                    if (rc.State == RenderChunk.RenderChunkState.Delete)
                    {
                        _removelist.Add(rc);
                    }
                    else
                        rc.Update(gametime);
                }
                foreach (RenderChunk rc in _removelist)
                {
                    renderchunks.Remove(rc);
                }
            }

            if (state == SceneState.ToBattle || state == SceneState.ToMap)
            {
                _statetoggletime -= gametime.ElapsedGameTime.TotalSeconds;
                if (_statetoggletime < 0.0)
                {
                    state = (state == SceneState.ToBattle ? SceneState.Battle : SceneState.Map);
                    Rebuild();
                }
            }

            if (_changebgtime > 0.0)
            {
                _changebgtime -= gametime.ElapsedGameTime.TotalSeconds;
                if (_changebgtime < 0.0)
                {
                    CompChangeBackground();

                }
            }
            //SortRenderChunksByLayer();
        }

        private void UpdateNextActionRound(GameTime gametime)
        {
            if (_nextactiontime > 0.0)
            {
                _nextactiontime -= gametime.ElapsedGameTime.TotalSeconds;
                if (_nextactiontime <= 0.0)
                {
                    NextActionRound();
                }
            }
        }

        public void AddRenderChunkDefer(RenderChunk rc)
        {
            renderchunksdefer.Add(rc);
        }


        public void AddRenderChunk(RenderChunk rc)
        {
            lock (lockThis)
            {
                renderchunks.Add(rc);
                rc.Scene = this;
            }
        }

        public Character GetCharacterByName(string name)
        {
            foreach (Character ch in characters)
            {
                if (ch.Name == name)
                    return ch;
            }
            return null;
        }


        public void UpdatePlayerPosition(ProjectXServer.Messages.PlayerPositionUpdate msg)
        {
            Player _p = null;

            if (msg.ClientID == localplayer.ClientID)
            {
                _p = localplayer;
            }
            else
            {
                _p = FindNetPlayer(msg.ClientID);
            }
            if (_p != null)
            {
                //由于网络包会先到，所以最后会导致netplayer的target和position重合
                //也就无法根据之间的距离差来求出最后人物停止的方向
                //所以这里做个小的调整，根据服务器计算出的人物朝向来将人物位置稍微调整一下
                _p.Position = new Vector2(msg.Position[0] - (float)(msg.Dir) * 0.001f, msg.Position[1]);

                if (msg.State == (int)CharacterState.Correct)
                {
                    //_p.ClearActionSet();
                    _p.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.Immediate, null);
                }
                if (_p == localplayer)
                    _p.UpdateSceneScroll();
            }

        }

        public NetPlayer FindNetPlayer(long clientid)
        {
            if (netplayers.Count == 0)
                return null;
            foreach (NetPlayer p in netplayers)
            {
                if (p.ClientID == clientid)
                    return p;
            }
            return null;
        }

        public void DelNetPlayer(NetPlayer ch)
        {
            netplayers.Remove(ch);
        }

        public void AddNetPlayer(NetPlayer ch)
        {
            netplayers.Add(ch);
        }

        public void AddCharacterDef(Character ch)
        {
            charactersdefer.Add(ch);
        }

        public void AddCharacter(Character ch)
        {
            characters.Add(ch);
        }

        public void AddMonster(Character ch)
        {
            battlecharacters.Add(ch);
        }

        public void AddSpell(Spell ch)
        {
            spells.Add(ch);
        }

        public void SetViewportPos(float x, float y)
        {
            viewport.X = x;
            viewport.Y = y;
        }

        public void SetViewportSize(float width, float height)
        {
            viewport.Z = width;
            viewport.W = height;
        }

        public void SortRenderChunksByLayer()
        {
            renderchunks.Sort();
        }

        protected void UpdateBattleResult()
        {
            if (state != SceneState.Battle)
                return;
            if (battlecharacters.Count == 0)
            {
                battlefini = true;
                actionlist.Clear();
                //UIMgr.AddWinDialog((int)UIMgr.UILayout.Center, (int)UIMgr.UILayout.Center, 3.0, 99, new EventHandler(OnWinDialogClose));
                UIElement d = UIMgr.AddUIControl("Dialog_Win", "win_dlg", (int)UILayout.Center, (int)UILayout.Center, 0, 0, 3.0, 99, this);
                if (d != null)
                {
                    d.OnClose += new EventHandler(OnWinDialogClose);
                }
            }
        }

        protected void OnWinDialogClose(object sender, EventArgs e)
        {
            ReturnMap();
        }

        public void IntoBattle()
        {
            if (state != SceneState.Map)
                return;
            state = SceneState.ToBattle;
            _statetoggletime = _statetoggledur;
            battlefini = false;
            foreach (RenderChunk rc in renderchunks)
            {
                if (rc.State != RenderChunk.RenderChunkState.Invisible)
                    rc.State = RenderChunk.RenderChunkState.FadeOut;
            }
            localplayer.Picture.Direction = new Vector2(1, 0);
            localplayer.ClearActionSet();
            localplayer.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.Immediate, null);

            UIMgr.ShowLeaderDialog(false);


            UIElement trackd = UIMgr.GetUIControlByName("dlg_questtrck");
            if (trackd != null)
            {
                trackd.State = RenderChunk.RenderChunkState.FadeOut;
            }

        }

        public void ReturnMap()
        {
            if (state != SceneState.Battle)
                return;
            state = SceneState.ToMap;
            _statetoggletime = _statetoggledur;
            foreach (RenderChunk rc in renderchunks)
            {
                if (rc.State == RenderChunk.RenderChunkState.Show)
                    rc.State = RenderChunk.RenderChunkState.FadeOut;
            }
            UIMgr.ShowLeaderDialog(true);
            localplayer.CloseBattleMenu();
            localplayer.ClearActionSet();

        }

        /// <summary>
        /// rebuild scene data
        /// </summary>
        public void Rebuild()
        {
            if (state == SceneState.Map)
            {
                localplayer.FaceDirMethod = Character.DirMethod.AutoDectect;
                foreach (RenderChunk rc in renderchunks)
                {
                    if (rc.State != RenderChunk.RenderChunkState.Invisible)
                        rc.State = RenderChunk.RenderChunkState.FadeIn;
                }
                foreach (Character ch in characters)
                {
                    //rc.State = RenderChunk.RenderChunkState.Show;
                    if (ch is Player)
                    {
                        ch.PopPosition();
                        ch.State = CharacterState.Idle;
                        //ch.Picture.State = RenderChunk.RenderChunkState.FadeIn;
                    }
                }
                foreach (Character ch in battlecharacters)
                {
                    ch.Picture.State = RenderChunk.RenderChunkState.Delete;
                    ch.Title.State = RenderChunk.RenderChunkState.Delete;
                }
                battlecharacters.Clear();
                foreach (Spell ch in spells)
                {
                    ch.Picture.State = RenderChunk.RenderChunkState.Delete;
                }
                spells.Clear();
                UIElement trackd = UIMgr.GetUIControlByName("dlg_questtrck");
                if (trackd != null)
                {
                    if (localplayer.Debug_GetQuest())
                        trackd.State = RenderChunk.RenderChunkState.FadeIn;
                }
                ChangeBackground(SceneState.Map);
            }
            else if (state == SceneState.Battle)
            {
                localplayer.FaceDirMethod = Character.DirMethod.Fixed;
                localplayer.FixedDir = new Vector2(1, 0);
                foreach (RenderChunk rc in renderchunks)
                {
                    //rc.State = RenderChunk.RenderChunkState.Show;
                    if (rc is Background || rc is Cloud)
                    {
                        rc.State = RenderChunk.RenderChunkState.FadeIn;
                    }
                }
                foreach (Character ch in characters)
                {
                    //rc.State = RenderChunk.RenderChunkState.Show;
                    if (ch is Player)
                    {
                        ch.PushPosition();
                        ch.Position = new Vector2(75, GameConst.ScreenHeight / 2) + new Vector2(viewport.X, viewport.Y);
                        ch.State = CharacterState.Idle;
                        ch.Picture.State = RenderChunk.RenderChunkState.FadeIn;
                        ch.Title.State = RenderChunk.RenderChunkState.FadeIn;
                    }
                }
                int numenemy = random.Next(1, 4);
                if (GameConst.FixedEnemyNum != -1)
                    numenemy = GameConst.FixedEnemyNum;
                if (localplayer.CompletedQuests.Contains(2))
                    numenemy = 1;
                int es = 1;
                if (localplayer.CompletedQuests.Contains(1)) // quest 1 completed
                    es = 2;
                if (localplayer.CompletedQuests.Contains(2)) // quest 2 completed
                    es = 3;
                //debug to boss rush
                //es = 3;
                //numenemy = 1;
                for (int i = 0; i < numenemy; ++i)
                {
                    string mtmp = "";
                    if (es == 1)
                        mtmp = "demon";
                    else if (es == 2)
                        mtmp = "evilspirit";
                    else if (es == 3)
                        mtmp = "boss";
                    Monster monster = Character.CreateCharacter(mtmp, this) as Monster;//new Monster(es == 1 ? "魔军" : "妖军" + (i + 1).ToString(), this);
                    if (monster != null)
                    {
                        monster.Layer = 15;
                        monster.FaceDirMethod = Character.DirMethod.Fixed;
                        monster.FixedDir = new Vector2(-1, 0);
                        monster.Picture.Direction = monster.FixedDir;

                        monster.Position = new Vector2(GameConst.ScreenWidth - monster.Picture.FrameSize.X * 0.5f, (i + 1) * GameConst.ScreenHeight / (numenemy + 1)) + new Vector2(viewport.X, viewport.Y);
                        monster.OnActionCompleted += new EventHandler(Monster_OnActionCompleted);
                        //monster.Picture.Size = new Vector2(3, 3);
                        AddMonster(monster);
                    }
                }
                //ChangeBackground(SceneState.Battle);
                SortRenderChunksByLayer();
                NextRound();
            }
        }

        public void LoadGameData()
        {
            try
            {
                countdowneffect = new PreRenderEffect("number", 64, 64);
                countdowneffect.Initialize(GameConst.Content);
                countdowneffect.Loop = false;
                countdowneffect.Scene = this;
                countdowneffect.CoordinateSystem = RenderChunk.CoordinateSystemType.Screen;
                countdowneffect.PlaySpeed = 1.0f;


                System.IO.Stream stream = TitleContainer.OpenStream(name + ".xml");
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                XmlNodeList constdef = doc.GetElementsByTagName("Config", "");
                if (constdef.Count > 0)
                {
                    for (int i = 0; i < constdef[0].ChildNodes.Count; ++i)
                    {
                        XmlNode node = constdef[0].ChildNodes[i];
                        if (node != null)
                        {
                            switch (node.Name)
                            {
                                case "FixedEnemyNum":
                                    {
                                        GameConst.FixedEnemyNum = Convert.ToInt32(node.FirstChild.Value);
                                        break;
                                    }
                                case "PlayerSpeed":
                                    {
                                        GameConst.PlayerSpeed = Convert.ToInt32(node.FirstChild.Value);
                                        break;
                                    }
                                case "PlayerHP":
                                    {
                                        GameConst.PlayerHP = Convert.ToInt32(node.FirstChild.Value);

                                        break;
                                    }
                                case "PlayerATK":
                                    {
                                        GameConst.PlayerAtk = Convert.ToInt32(node.FirstChild.Value);
                                        break;
                                    }
                                case "BossRushMode":
                                    {
                                        GameConst.BossRushMode = Convert.ToInt32(node.FirstChild.Value);
                                        break;
                                    }
                                case "BossRushMode1Offset":
                                    {
                                        GameConst.BossRushMode1Offset = Convert.ToInt32(node.FirstChild.Value);
                                        break;
                                    }
                            }
                        }
                    }
                }
                XmlNodeList npcdef = doc.GetElementsByTagName("NpcDef", "");
                for (int i = 0; i < npcdef[0].ChildNodes.Count; ++i)
                {
                    XmlNode node = npcdef[0].ChildNodes[i];
                    if (node.Name == "Npc")
                    {
                        string npcname = "";
                        float x = 0;
                        float y = 0;
                        int hp = 0;
                        float speed = 0;
                        string pic = "";
                        int layer = 0;
                        bool visible = true;
                        List<string> quests = new List<string>();

                        for (int j = 0; j < node.ChildNodes.Count; ++j)
                        {
                            XmlNode an = node.ChildNodes[j];
                            switch (an.Name)
                            {
                                case "Name":
                                    {
                                        npcname = an.FirstChild.Value;
                                        break;
                                    }
                                case "Hp":
                                    {
                                        hp = Convert.ToInt32(an.FirstChild.Value);
                                        break;
                                    }
                                case "ScreenX":
                                    {
                                        x = (float)Convert.ToDouble(an.FirstChild.Value);
                                        break;
                                    }
                                case "ScreenY":
                                    {
                                        y = (float)Convert.ToDouble(an.FirstChild.Value);
                                        break;
                                    }
                                case "Speed":
                                    {
                                        speed = (float)Convert.ToDouble(an.FirstChild.Value);
                                        break;
                                    }
                                case "Picture":
                                    {
                                        pic = an.FirstChild.Value;
                                        break;
                                    }
                                case "Layer":
                                    {
                                        layer = Convert.ToInt32(an.FirstChild.Value);
                                        break;
                                    }
                                case "Quest":
                                    {
                                        for (int k = 0; k < an.ChildNodes.Count; ++k)
                                        {
                                            XmlNode qn = an.ChildNodes[k];
                                            quests.Add(qn.Name);
                                        }
                                        break;
                                    }
                                case "Visible":
                                    {
                                        visible = Convert.ToBoolean(an.FirstChild.Value);
                                        break;
                                    }
                            }
                        }
                        Npc npc = new Npc(npcname, this);
                        CharacterDefinition.PicDef pd = GameConst.Content.Load<CharacterDefinition.PicDef>(@"chardef/" + pic);
                        CharacterPic cpic = new CharacterPic(pd, layer);
                        npc.Picture = cpic;


                        CharacterTitle title = new CharacterTitle(GameConst.CurrentFont);
                        title.NameString = npcname;
                        title.Layer = layer;
                        title.Character = npc;

                        npc.Position = new Vector2(x, y);
                        npc.HP = hp;

                        npc.State = CharacterState.Idle;
                        npc.Speed = speed;
                        npc.Title = title;
                        npc.Picture.State = visible ? RenderChunk.RenderChunkState.Show : RenderChunk.RenderChunkState.Invisible;
                        npc.Title.State = visible ? RenderChunk.RenderChunkState.Show : RenderChunk.RenderChunkState.Invisible;
                        foreach (string qn in quests)
                        {
                            npc.AddQuest(qn);
                        }
                        AddCharacter(npc);
                        //npc.
                    }

                }
                stream.Close();
                stream.Dispose();



                //create task track 
                UIDialog dialog = UIMgr.AddUIControl("UIDialog", "dlg_questtrck", 755, 174, 247, 345, -1, 99, this) as UIDialog;
                if (dialog != null)
                {
                    UITextBlock text = UIMgr.CreateUIControl("UITextBlock") as UITextBlock;
                    if (text != null)
                    {
                        text.FontColor = Color.DarkGray;
                        dialog.AddUIControl(text, "text_questname", 18, 18, 211, 24, -1, this);
                    }
                    UITextBlock text1 = UIMgr.CreateUIControl("UITextBlock") as UITextBlock;
                    if (text1 != null)
                    {
                        text1.FontColor = Color.Gold;
                        dialog.AddUIControl(text1, "text_questtrack", 28, 50, 190, 24, -1, this);
                    }
                }
                dialog.State = RenderChunk.RenderChunkState.Hide;

            }
            catch (Exception e)
            {
                //MessageBox box;
                Debug.WriteLine(e.Message);
            }
        }

        protected void OnPlayerTurnDlgClose(object sender, EventArgs e)
        {
            localplayer.MakeBattleMenu();
            _roundtime = _roundtimedur;
        }

        protected void OnEnemyTurnDlgClose(object sender, EventArgs e)
        {
            _roundtime = -1.0;
            BattleRound(1);
        }

        /// <summary>
        /// 战斗回合
        /// </summary>

        double _roundtimedur = 30.0;
        double _roundtime = -1.0;
        int roundturn = -1;
        protected void NextRound()
        {
            if (battlefini)
                return;
            if (state != SceneState.Battle)
                return;
            if (roundturn == -1)
                roundturn = 1;//random.Next(2);
            else
                roundturn++;

            UIElement d = UIMgr.AddUIControl("Dialog_PlayerTurn", "playerturn_dlg", (int)UILayout.Center, (int)UILayout.Center, 0, 0, 0.5, 99, this);
            d.OnClose += new EventHandler(OnPlayerTurnDlgClose);
            turn = Turn.Player;
            localplayer.OperateTarget = null;
            localplayer.Operate = Character.OperateType.None;
            foreach (Character ch in battlecharacters)
            {
                if (ch.TemplateID == 3)
                {
                    if (ch.HP <= ch.MaxHP / 2)
                    {
                        if (!_battlebackgroundchanged)
                            ChangeBackground(SceneState.Battle);
                    }
                }
            }
        }

        /// <summary>
        /// player局的倒计时函数，用来判断player是否做出动作和动作目标的选择
        /// 若倒计时时间到,则随机指派一个攻击目标
        /// </summary>
        /// <param name="gametime"></param>
        protected void UpdatePlayerTurn(GameTime gametime)
        {
            if (_roundtime > 0.0)
            {
                if (localplayer.Operate != Character.OperateType.None && localplayer.OperateTarget != null)
                {
                    _roundtime = -1.0;
                    BattleRound(0);
                }
                else
                {
                    _roundtime -= gametime.ElapsedGameTime.TotalSeconds;
                    if (_roundtime < 0.0)
                    {
                        //player.AttackTarget = battlecharacters[random.Next(battlecharacters.Count)];
                        localplayer.OperateTarget = battlecharacters[random.Next(battlecharacters.Count)];
                        localplayer.Operate = Character.OperateType.Attack;
                        localplayer.CloseBattleMenu();
                        BattleRound(0);
                    }
                }
            }
        }




        List<ActionOrder> actionlist = new List<ActionOrder>();
        double _nextactiontime = -1.0;
        double _nextactiontimedur = 0.5;
        private void NextActionRound()
        {
            if (actionlist.Count > 0)
            {
                if (actionlist.Count == 1)
                {
                    actionlist.Clear();
                    NextRound();
                    return;
                }
                if (actionlist[0].character is Player)
                {
                    if (localplayer.Operate == Character.OperateType.Attack)
                    {
                        localplayer.AddActionSet("Launch", CharacterState.Launch, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        localplayer.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveAttackTarget, localplayer.OperateTarget);
                        localplayer.AddActionSet("Landing", CharacterState.Landing, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        localplayer.AddActionSet("Attack", CharacterState.Attack, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        localplayer.AddActionSet("Launch", CharacterState.Launch, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        localplayer.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveTarget, localplayer.Position);
                        localplayer.AddActionSet("Landing", CharacterState.Landing, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        localplayer.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    }
                    else if (localplayer.Operate == Character.OperateType.Magic)
                    {
                        Spell fireball = Character.CreateCharacter("fireball", this) as Spell;
                        if (fireball != null)
                        {
                            fireball.Layer = 15;
                            fireball.FaceDirMethod = Character.DirMethod.Fixed;
                            fireball.FixedDir = new Vector2(1, 0);
                            fireball.Picture.Direction = fireball.FixedDir;

                            fireball.Position = localplayer.Position + new Vector2(100, 0);
                            fireball.OnActionCompleted += new EventHandler(Spell_OnActionCompleted);
                            AddSpell(fireball);

                            fireball.AddActionSet("Launch", CharacterState.Launch, CharacterActionSetChangeFactor.AnimationCompleted, null);
                            fireball.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveAttackTarget, localplayer.OperateTarget);
                            fireball.AddActionSet("Attack", CharacterState.Attack, CharacterActionSetChangeFactor.AnimationCompleted, null);
                            fireball.AddActionSet("Idle", CharacterState.Dead, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        }
                    }
                }
                else
                {
                    Monster monster = actionlist[0].character as Monster;
                    monster.AddActionSet("Launch", CharacterState.Launch, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    monster.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveAttackTarget, localplayer);
                    monster.AddActionSet("Landing", CharacterState.Landing, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    monster.AddActionSet("Attack", CharacterState.Attack, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    monster.AddActionSet("Launch", CharacterState.Launch, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    monster.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveTarget, monster.Position);
                    monster.AddActionSet("Landing", CharacterState.Landing, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    monster.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.AnimationCompleted, null);
                }

                actionlist[0].character.Order = null;
                actionlist.RemoveAt(0);


            }
        }


        public void BattleRound(int turn)
        {
            //generate actionlist
            actionlist.Clear();
            actionlist.Add(new ActionOrder(null, 100));
            localplayer.Order = new ActionOrder(localplayer, random.Next(10, 20));
            actionlist.Add(localplayer.Order);
            foreach (Monster m in battlecharacters)
            {
                m.Order = new ActionOrder(m, random.Next(10, 20));
                actionlist.Add(m.Order);
            }

            actionlist.Sort();

            GoNextActionRound();
            if (turn == 0)
            {
                //posb = player.Position;
                //player.State = CharacterState.Launch;
                //player.OnActionCompleted += new EventHandler(Player_OnActionCompleted);


            }
            else if (turn == 1)
            {
                //pick monster
                if (battlecharacters.Count > 0)
                {
                    //mposb = monster.Position;
                    //monster.AttackTarget = player;
                    //monster.State = CharacterState.Launch;

                }
                //monster.OnActionCompleted += new EventHandler(Monster_OnActionCompleted);
            }
        }

        private void GoNextActionRound()
        {
            _nextactiontime = _nextactiontimedur;
        }


        protected void Spell_OnActionCompleted(object sender, EventArgs e)
        {
            Spell spell = sender as Spell;
            if (spell.State == CharacterState.Dead)
            {
                spell.Picture.State = RenderChunk.RenderChunkState.FadeOutToDel;
                spells.Remove(spell);
                //NextRound();
                GoNextActionRound();
                return;
            }

        }


        protected void Monster_OnActionCompleted(object sender, EventArgs e)
        {
            Monster monster = sender as Monster;
            /*if (monster.State == CharacterState.BeAttack)
            {
                monster.State = CharacterState.Idle;
                return;
            }*/
            if (monster.State == CharacterState.Dead)
            {
                monster.Picture.State = RenderChunk.RenderChunkState.FadeOutToDel;
                monster.Title.State = RenderChunk.RenderChunkState.FadeOutToDel;
                battlecharacters.Remove(monster);
                actionlist.Remove(monster.Order);
                if (monster.TemplateID == 3)
                {
                    Npc npc = localplayer.Scene.GetCharacterByName("boss") as Npc;
                    if (npc != null)
                    {
                        npc.Picture.State = RenderChunk.RenderChunkState.Invisible;
                        npc.Title.State = RenderChunk.RenderChunkState.Invisible;
                    }
                }
                return;
            }

            GoNextActionRound();
            /*if (monster.State == CharacterState.Idle)
            {
                if (monster.AttackTarget != null)
                {
                    monster.State = CharacterState.Attack;
                    //do attack
                    //monster.AttackTarget.BeAttack(monster);
                    //monster.AttackTarget = null;
                }
                else
                {
                    NextRound();
                }
            }
            else if (monster.State == CharacterState.Dead)
            {
                monster.Picture.State = RenderChunk.RenderChunkState.FadeOutToDel;
                monster.Title.State = RenderChunk.RenderChunkState.FadeOutToDel;
                battlecharacters.Remove(monster);
            }
            else if (monster.State == CharacterState.Launch)
            {
                if (monster.AttackTarget == null)
                {
                    monster.Target = mposb;
                    //monster.OnActionCompleted -= new EventHandler(Monster_OnActionCompleted);
                }
            }*/
        }



        protected void Player_OnActionCompleted(object sender, EventArgs e)
        {
            if (localplayer.InteractiveTarget != null)
            {
                localplayer.InteractiveToNpc(localplayer.InteractiveTarget as Npc);
            }
            else
            {
                if (localplayer.State == CharacterState.Dead)
                {
                    ReturnMap();
                    return;
                }
                GoNextActionRound();
                /*if (player.State != CharacterState.BeAttack)
                {
                    //NextRound();
                    NextActionRound();
                }
                else
                {
                    player.State = CharacterState.Idle;
                }*/
            }
            /*if (player.State == CharacterState.Idle)
            {
                if (player.AttackTarget != null)
                {
                    player.State = CharacterState.Attack;
                    //do attack
                    //player.AttackTarget.BeAttack(player);
                    //player.AttackTarget = null;
                }
                else
                {
                    player.OnActionCompleted -= new EventHandler(Player_OnActionCompleted);
                    NextRound();
                }
            }
            else if (player.State == CharacterState.Launch)
            {
                if (player.AttackTarget == null)
                {
                    player.Target = posb;
                    //player.OnActionCompleted -= new EventHandler(Player_OnActionCompleted);
                }
            }
            else if (player.State == CharacterState.Dead)
            {
                player.Target = posb;
                //player.OnActionCompleted -= new EventHandler(Player_OnActionCompleted);
            }*/
        }

        protected void Player_OnArrived(object sender, EventArgs e)
        {
            if (localplayer.InteractiveTarget != null)
            {
                localplayer.InteractiveToNpc(localplayer.InteractiveTarget as Npc);
            }
        }

        bool _battlebackgroundchanged = false;
        private void CompChangeBackground()
        {
            if (GameConst.BossRushMode == 0)
            {
                foreach (Background bg in battlebackgrundlist)
                {
                    bg.State = RenderChunk.RenderChunkState.FadeIn;
                    renderchunks.Add(bg);
                }
                _battlebackgroundchanged = true;
                SortRenderChunksByLayer();
            }
            else if (GameConst.BossRushMode == 1)
            {
                fadebg.State = RenderChunk.RenderChunkState.FadeOutToDel;
                foreach (Background bg in battlebackgrundlist)
                {
                    bg.State = RenderChunk.RenderChunkState.FadeIn;
                    renderchunks.Add(bg);
                }
                SortRenderChunksByLayer();
                foreach (RenderChunk rc in renderchunks)
                {
                    if (rc is Cloud)
                    {
                        //rc.State = RenderChunk.RenderChunkState.FadeOut;
                        rc.Position = new Vector2((float)random.NextDouble() * (float)GameConst.ScreenWidth * 0.8f,
                                                    (float)random.NextDouble() * (float)GameConst.ScreenHeight)
                                                    + new Vector2(viewport.X, viewport.Y);

                    }
                }
                localplayer.Position = new Vector2(localplayer.Position.X, localplayer.Position.Y - GameConst.BossRushMode1Offset);
                
                foreach (Character ch in battlecharacters)
                {
                    if (ch.TemplateID == 3)
                    {
                        ch.Position = new Vector2(ch.Position.X, ch.Position.Y + GameConst.BossRushMode1Offset);
                      
                    }
                }
                this.wind = new Vector2(0, -10);
                _battlebackgroundchanged = true;
            }
            else if (GameConst.BossRushMode == 2)
            {
                fadebg.State = RenderChunk.RenderChunkState.FadeOutToDel;
                foreach (Background bg in battlebackgrundlist)
                {
                    bg.State = RenderChunk.RenderChunkState.FadeIn;
                    renderchunks.Add(bg);
                }
                SortRenderChunksByLayer();
                foreach (RenderChunk rc in renderchunks)
                {
                    if (rc is Cloud)
                    {
                        //rc.State = RenderChunk.RenderChunkState.FadeOut;
                        rc.Position = new Vector2((float)random.NextDouble() * (float)GameConst.ScreenWidth * 0.8f,
                                                    (float)random.NextDouble() * (float)GameConst.ScreenHeight)
                                                    + new Vector2(viewport.X, viewport.Y);

                    }
                }
                localplayer.Position = new Vector2(localplayer.Position.X, localplayer.Picture.FrameSize.Y + viewport.Y);

                foreach (Character ch in battlecharacters)
                {
                    if (ch.TemplateID == 3)
                    {
                        ch.Position = new Vector2(ch.Position.X, GameConst.ScreenHeight - ch.Picture.FrameSize.Y * 0.5f + viewport.Y);
                        ch.Picture.Angle = MathHelper.ToRadians(30.0f);
                        ch.Picture.OriginAngle = MathHelper.ToRadians(30.0f);
                    }
                }
                this.wind = new Vector2(-8, -8);
                _battlebackgroundchanged = true;
                localplayer.Picture.Angle = MathHelper.ToRadians(30.0f);
                localplayer.Picture.OriginAngle = MathHelper.ToRadians(30.0f);
            }
        }


        private void ChangeBackground(SceneState state)
        {
            if (state == SceneState.Map)
            {
                if (GameConst.BossRushMode == 0)
                {
                    foreach (Background bg in battlebackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeOut;
                        renderchunks.Remove(bg);
                    }
                    foreach (Background bg in mapbackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeIn;
                    }
                    foreach (RenderChunk rc in renderchunks)
                    {
                        if (rc is Cloud)
                        {
                            rc.State = RenderChunk.RenderChunkState.FadeIn;
                        }
                    }
                    _battlebackgroundchanged = false;

                }
                else if (GameConst.BossRushMode == 1)
                {
                    foreach (Background bg in battlebackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeOut;
                        renderchunks.Remove(bg);
                    }
                    foreach (Background bg in mapbackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeIn;
                    }
                    foreach (RenderChunk rc in renderchunks)
                    {
                        if (rc is Cloud)
                        {
                            rc.State = RenderChunk.RenderChunkState.FadeIn;
                        }
                    }
                    foreach (RenderChunk rc in renderchunks)
                    {
                        if (rc is Cloud)
                        {
                            //generate position
                            rc.Position = new Vector2((float)random.NextDouble() * actualSize.Z, (float)random.NextDouble() * actualSize.W * 0.9f);
                        }
                    }
                    this.wind = new Vector2(1, 0);
                    _battlebackgroundchanged = false;
                }
                else if (GameConst.BossRushMode == 2)
                {
                    foreach (Background bg in battlebackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeOut;
                        renderchunks.Remove(bg);
                    }
                    foreach (Background bg in mapbackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeIn;
                    }
                    foreach (RenderChunk rc in renderchunks)
                    {
                        if (rc is Cloud)
                        {
                            rc.State = RenderChunk.RenderChunkState.FadeIn;
                        }
                    }
                   foreach (RenderChunk rc in renderchunks)
                    {
                        if (rc is Cloud)
                        {
                            //generate position
                            rc.Position = new Vector2((float)random.NextDouble() * actualSize.Z, (float)random.NextDouble() * actualSize.W * 0.9f);
                        }
                    }
                    this.wind = new Vector2(1, 0);
                    _battlebackgroundchanged = false;
                    localplayer.Picture.Angle = 0.0f;
                    localplayer.Picture.OriginAngle = 0.0f;
                }

            }
            else
            {
                if (GameConst.BossRushMode == 0)
                {
                    foreach (Background bg in mapbackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeOut;
                        
                    }
                    _changebgtime = _changebgdur;
                    foreach (RenderChunk rc in renderchunks)
                    {
                        if (rc is Cloud)
                        {
                            rc.State = RenderChunk.RenderChunkState.FadeOut;
                        }
                    }
                }
                else if (GameConst.BossRushMode == 1)
                {
                    foreach (Background bg in mapbackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeOut;
                        
                    }

                    _changebgtime = _changebgdur;
                    fadebg.State = RenderChunk.RenderChunkState.FadeIn;
                    AddRenderChunk(fadebg);
                    SortRenderChunksByLayer();
                    //localplayer.PushPosition();
                   

                }
                else if (GameConst.BossRushMode == 2)
                {
                    foreach (Background bg in mapbackgrundlist)
                    {
                        bg.State = RenderChunk.RenderChunkState.FadeOut;
                       
                    }

                    _changebgtime = _changebgdur;
                    /*foreach (RenderChunk rc in renderchunks)
                    {
                        if (rc is Cloud)
                        {
                            rc.State = RenderChunk.RenderChunkState.FadeOut;
                        }
                    }*/
                    //localplayer.PushPosition();
                    fadebg.State = RenderChunk.RenderChunkState.FadeIn;
                    AddRenderChunk(fadebg);
                    SortRenderChunksByLayer();
                    /*localplayer.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveTarget, new Vector2(localplayer.Position.X, localplayer.Picture.FrameSize.Y * 0.5f + viewport.Y));
                    localplayer.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    foreach (Character ch in battlecharacters)
                    {
                        if (ch.TemplateID == 3)
                        {
                            ch.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveTarget, new Vector2(ch.Position.X, GameConst.ScreenHeight - ch.Picture.FrameSize.Y * 0.5f + viewport.Y));
                            ch.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        }
                    }*/

                }
            }

        }

        List<Background> mapbackgrundlist = new List<Background>();
        List<Background> battlebackgrundlist = new List<Background>();
        public void LoadBackground()
        {
            Texture2D texbg0 = GameConst.Content.Load<Texture2D>(@"background/yun_a0");
            Texture2D texbg1 = GameConst.Content.Load<Texture2D>(@"background/yun_a1");
            Texture2D texbg2 = GameConst.Content.Load<Texture2D>(@"background/yun_a2");
            Texture2D texbg3 = GameConst.Content.Load<Texture2D>(@"background/yun_b1");

            Background bg = new Background(texbg0, 0);
            bg.Size = new Vector2(GameConst.BackgroundScale, GameConst.BackgroundScale);
            AddRenderChunk(bg);
            mapbackgrundlist.Add(bg);
            bg = new Background(texbg1, 1);
            bg.Size = new Vector2(GameConst.BackgroundScale, GameConst.BackgroundScale);
            AddRenderChunk(bg);
            mapbackgrundlist.Add(bg);
            bg = new Background(texbg2, 2);
            bg.Size = new Vector2(GameConst.BackgroundScale, GameConst.BackgroundScale);
            AddRenderChunk(bg);
            mapbackgrundlist.Add(bg);
            bg = new Background(texbg3, 3);
            bg.Size = new Vector2(GameConst.BackgroundScale, GameConst.BackgroundScale);
            AddRenderChunk(bg);
            mapbackgrundlist.Add(bg);

            Texture2D fadetexture = GameConst.Content.Load<Texture2D>(@"effect/fade");
            fadebg = new Background(fadetexture, 9999);
            fadebg.Size = new Vector2(GameConst.ScreenWidth / fadetexture.Width, GameConst.ScreenHeight / fadetexture.Height);
            //fadebg.State = RenderChunk.RenderChunkState.Hide;
            //AddRenderChunk(fadebg);


            if (GameConst.BossRushMode == 0)
            {
                Texture2D btexbg0 = GameConst.Content.Load<Texture2D>(@"background/bg_boss");
                Texture2D btexbg1 = GameConst.Content.Load<Texture2D>(@"background/bg_boss_loop1");
                Background bbg = new Background(btexbg0, 0);
                bbg.Size = Vector2.One;
                bbg.CoordinateSystem = RenderChunk.CoordinateSystemType.Screen;
                bbg.AutoScroll = true;
                bbg.AutoScrollDirection = Background.AutoScrollDirectionType.Horizontal;
                bbg.AutoScrollLoop = false;
                bbg.AutoScrollSpeed = new Vector2(4, 4);
                battlebackgrundlist.Add(bbg);
                bbg = new Background(btexbg1, 1);
                bbg.Size = Vector2.One;
                bbg.CoordinateSystem = RenderChunk.CoordinateSystemType.Screen;
                bbg.AutoScroll = true;
                bbg.AutoScrollDirection = Background.AutoScrollDirectionType.Vertical;
                bbg.AutoScrollLoop = true;
                bbg.AutoScrollSpeed = new Vector2(200, 200);
                battlebackgrundlist.Add(bbg);
            }
            else
            {
                Texture2D btexbg0 = GameConst.Content.Load<Texture2D>(@"background/bg_boss_2");
                Background bbg = new Background(btexbg0, 0);
                bbg.Size = Vector2.One;
                bbg.CoordinateSystem = RenderChunk.CoordinateSystemType.Screen;
                bbg.AutoScroll = false;
                battlebackgrundlist.Add(bbg);
            }
        }

        public int SelectCharacter()
        {
            if (_hostCharacter != null)
            {
                if (_hostCharacter is Npc)
                {
                    /*if (player.State == CharacterState.Idle)
                        player.State = CharacterState.Launch;
                    player.InteractiveTarget = _hostCharacter;
                    player.OnArrived += new EventHandler(Player_OnArrived);*/
                    if (_hostCharacter != localplayer.InteractiveTarget)
                    {
                        localplayer.AddActionSet("Launch", CharacterState.Launch, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        localplayer.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveInteractiveTarget, _hostCharacter);
                        localplayer.AddActionSet("Landing", CharacterState.Landing, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        localplayer.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.AnimationCompleted, null);
                        localplayer.InteractiveTarget = _hostCharacter;
                    }
                    return 1;
                }
            }
            return 0;
        }


        public void ConfirmOperateTarget()
        {
            if (_hostCharacter != null && _roundtime > 0)
            {
                //若直接选中monster，为直接攻击
                if (localplayer.Operate == Character.OperateType.None)
                    localplayer.Operate = Character.OperateType.Attack;
                localplayer.OperateTarget = _hostCharacter;
                _hostCharacter.Picture.HighLight = false;
                _hostCharacter = null;
                localplayer.CloseBattleMenu();
            }
        }

        Character _hostCharacter = null;
        public void HighLightCharacterByPoint(int _x, int _y)
        {
            int x = _x + (int)viewport.X;
            int y = _y + (int)viewport.Y;
            Character host = null;
            if (state == SceneState.Battle)
            {
                /*if (player.Operate != Character.OperateType.None)
                {
                    if (_hostCharacter != null)
                    {
                        _hostCharacter.Picture.HighLight = false;
                        _hostCharacter = null;
                    }
                    return;
                }*/
            }
            foreach (Character ch in characters)
            {
                if (ch is Player)
                    continue;
                if (ch.Picture.State != RenderChunk.RenderChunkState.Show)
                    continue;
                Rectangle rect = new Rectangle((int)(ch.Position.X - ch.Picture.FrameSize.X * 0.3f),
                                                (int)(ch.Position.Y - ch.Picture.FrameSize.Y * 0.3f),
                                                (int)(ch.Picture.FrameSize.X * 0.6f),
                                                (int)(ch.Picture.FrameSize.Y * 0.6f));
                if (rect.Contains(new Point(x, y)))
                {
                    host = ch;
                    break;
                }
            }
            if (host == null)
            {
                foreach (Character ch in battlecharacters)
                {
                    Rectangle rect = new Rectangle((int)(ch.Position.X - ch.Picture.FrameSize.X * 0.3f),
                                                    (int)(ch.Position.Y - ch.Picture.FrameSize.Y * 0.3f),
                                                    (int)(ch.Picture.FrameSize.X * 0.6f),
                                                    (int)(ch.Picture.FrameSize.Y * 0.6f));
                    if (rect.Contains(new Point(x, y)))
                    {
                        host = ch;
                        break;
                    }
                }
            }
            if (host != null)
            {
                if (_hostCharacter != null)
                    _hostCharacter.Picture.HighLight = false;
                host.Picture.HighLight = true;
                _hostCharacter = host;
                if (host is Npc)
                {
                    GameCursor.SetCursor(GameCursor.CursorType.Talk);
                }
                else if (host is Monster)
                {
                    if (localplayer.Operate == Character.OperateType.Attack || localplayer.Operate == Character.OperateType.None)
                    {
                        GameCursor.SetCursor(GameCursor.CursorType.Attack);
                    }
                    else if (localplayer.Operate == Character.OperateType.Magic)
                    {
                        GameCursor.SetCursor(GameCursor.CursorType.Magic);
                    }
                }
            }
            else
            {
                GameCursor.SetCursor(GameCursor.CursorType.Normal);
                if (_hostCharacter != null)
                {
                    _hostCharacter.Picture.HighLight = false;
                    _hostCharacter = null;
                }
            }
        }

        public void InitMiniMap(Texture2D btex, Texture2D ctex, Texture2D mtex, int x, int y, int w, int h)
        {
            minimap = new MiniMap();
            minimap.Characters = characters;
            minimap.NetPlayers = netplayers;
            minimap.Scene = this;
            minimap.Initialize(btex, ctex, mtex, w, h);
            minimap.Position = new Vector2(x, y);
            AddRenderChunk(minimap);
        }

        public void GenerateClouds(Texture2D[] texarray)
        {
            //generate count, at least 5
            int numclouds = random.Next(10, texarray.Length * 2);
            for (int i = 0; i < numclouds; ++i)
            {
                //generate texture id
                Texture2D tex = texarray[random.Next(texarray.Length)];
                //generate layer
                int layer = random.Next(15, 30);

                Cloud cloud = new Cloud(tex, layer);

                //generate size
                float s = MathHelper.Clamp((float)random.NextDouble() + 0.3f, 0.0f, 1.5f);
                cloud.Size = new Vector2(s, s);

                //generate alpha
                float a = MathHelper.Clamp((float)random.NextDouble() + 0.7f, 0.0f, 1.0f);
                cloud.OriginColor = Color.FromNonPremultiplied(new Vector4(1, 1, 1, a));

                cloud.State = RenderChunk.RenderChunkState.FadeIn;

                //generate position
                cloud.Position = new Vector2((float)random.NextDouble() * actualSize.Z, (float)random.NextDouble() * actualSize.W * 0.9f);

                //generate speed, size is the scale factor
                float sf = (float)random.NextDouble() * (30000000.0f / (cloud.ActualSize.X * cloud.ActualSize.Y));
                cloud.Speed = new Vector2(sf, sf);

                AddRenderChunk(cloud);
            }
        }



        internal void UpdatePlayerMovement(ProjectXServer.Messages.PlayerMoveRequest msg)
        {
            Player _p = FindNetPlayer(msg.ClientID);
            if (_p != null)
            {
                _p.Target = new Vector2(msg.Target[0], msg.Target[1]);
                _p.AddActionSet("Launch", CharacterState.Launch, CharacterActionSetChangeFactor.AnimationCompleted, null);
                _p.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveTarget, _p.Target);
                _p.AddActionSet("Landing", CharacterState.Landing, CharacterActionSetChangeFactor.AnimationCompleted, null);
                _p.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.AnimationCompleted, null);
            }
        }

        internal void UpdatePlayerTarget(ProjectXServer.Messages.PlayerTargetChanged msg)
        {
            Player _p = FindNetPlayer(msg.ClientID);
            if (_p != null)
            {
                _p.Target = new Vector2(msg.Target[0], msg.Target[1]);
            }
        }
    }
}
