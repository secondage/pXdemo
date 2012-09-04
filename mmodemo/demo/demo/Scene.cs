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
using System.IO;
#if WINDOWS
using System.Windows.Forms;
#endif
using System.Xml.Linq;
using demo.animation;

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
        //private List<RenderChunk> renderchunksdefer = new List<RenderChunk>();
        private List<NetPlayer> netplayers = new List<NetPlayer>();
        private List<Character> characters = new List<Character>();
        //private List<Character> charactersdefer = new List<Character>();
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
            for (int i = 0; i < renderchunks.Count; ++i)
            {
                RenderChunk rc = renderchunks[i];
                //if (rc is Background || rc is Cloud)
                //    continue;
                /*if (rc is CharacterTitle)
                    continue;
                if (rc is Cloud)
                    continue;
                if (rc is CharacterPic)
                    continue;
                if (rc is PreRenderEffect)
                    continue;*/
                if (rc.SkipRender())
                    continue;
                rc.Render(sb);
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
                for (int i = 0; i < characters.Count; ++i)
                {
                    if (!(characters[i] is Player))
                    {
                        if (characters[i] is Npc)
                        {
                            if (localplayer != null)
                            {
                                localplayer.AddVisibleNpc(characters[i] as Npc);
                            }
                        }
                        characters[i].Update(gametime);
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
                for (int i = 0; i < netplayers.Count; ++i)
                {
                    netplayers[i].Update(gametime);
                }
            }
            else if (state == SceneState.Battle)
            {
                for (int i = 0; i < characters.Count; ++i)
                {
                    if (characters[i] is Player)
                        characters[i].Update(gametime);
                }
                for (int i = 0; i < battlecharacters.Count; ++i)
                {
                    battlecharacters[i].Update(gametime);
                }
                for (int i = 0; i < spells.Count; ++i)
                {
                    spells[i].Update(gametime);
                }
                UpdatePlayerTurn(gametime);
                UpdateBattleResult();
                UpdateNextActionRound(gametime);
            }

            for (int i = 0; i < renderchunks.Count; ++i)
            {
                if (renderchunks[i].State == RenderChunk.RenderChunkState.Delete)
                {
                    renderchunks.Remove(renderchunks[i]);
                }
                else
                {
                    renderchunks[i].Update(gametime);
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

        public void AddRenderChunk(RenderChunk rc)
        {
            lock (lockThis)
            {
                renderchunks.Add(rc);
                rc.Scene = this;
                SortRenderChunksByLayer();
            }
        }

        public Character GetCharacterByName(string name)
        {
            //fixme : modify to map
            for (int i = 0; i < characters.Count; ++i)
            {
                if (characters[i].Name == name)
                    return characters[i];
            }
            return null;
        }

        public void UpdatePlayerPosition(PlayerPositionUpdate msg)
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
            //fixme : modify to map
            for (int i = 0; i < netplayers.Count; ++i)
            {
                if (netplayers[i].ClientID == clientid)
                    return netplayers[i];
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

        private void SetViewportPos(ref Vector2 v)
        {
            viewport.X = v.X;
            viewport.Y = v.Y;
        }
       
        public void SetViewportPosDefer(float x, float y)
        {
            Animation<Vector2>.CreateAnimation2Value(new Vector2(viewport.X, viewport.Y),
                                                     new Vector2(x, y),
                                                     GameConst.ViewportScrollResetTime,
                                                     Vector2.Lerp,
                                                     SetViewportPos);

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
            for (int i = 0; i < renderchunks.Count; ++i)
            {
                if (renderchunks[i].State != RenderChunk.RenderChunkState.Invisible)
                    renderchunks[i].State = RenderChunk.RenderChunkState.FadeOut;
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
            for (int i = 0; i < renderchunks.Count; ++i)
            {
                if (renderchunks[i].State == RenderChunk.RenderChunkState.Show)
                    renderchunks[i].State = RenderChunk.RenderChunkState.FadeOut;
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
                for (int i = 0; i < renderchunks.Count; ++i)
                {
                    if (renderchunks[i].State != RenderChunk.RenderChunkState.Invisible)
                        renderchunks[i].State = RenderChunk.RenderChunkState.FadeIn;
                }
                for (int i = 0; i < characters.Count; ++i)
                {
                    //rc.State = RenderChunk.RenderChunkState.Show;
                    if (characters[i] is Player)
                    {
                        characters[i].PopPosition();
                        characters[i].State = CharacterState.Idle;
                        //ch.Picture.State = RenderChunk.RenderChunkState.FadeIn;
                    }
                }
                for (int i = 0; i < battlecharacters.Count; ++i)
                {
                    battlecharacters[i].Picture.State = RenderChunk.RenderChunkState.Delete;
                    battlecharacters[i].Title.State = RenderChunk.RenderChunkState.Delete;
                }
                battlecharacters.Clear();
                for (int i = 0; i < spells.Count; ++i)
                {
                    spells[i].Picture.State = RenderChunk.RenderChunkState.Delete;
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
                for (int i = 0; i < renderchunks.Count; ++i)
                {
                    //rc.State = RenderChunk.RenderChunkState.Show;
                    if (renderchunks[i] is Background || renderchunks[i] is Cloud)
                    {
                        renderchunks[i].State = RenderChunk.RenderChunkState.FadeIn;
                    }
                }
                for (int i = 0; i < characters.Count; ++i)
                {
                    //rc.State = RenderChunk.RenderChunkState.Show;
                    if (characters[i] is Player)
                    {
                        characters[i].PushPosition();
                        characters[i].Position = new Vector2(75, GameConst.ScreenHeight / 2) + new Vector2(viewport.X, viewport.Y);
                        characters[i].State = CharacterState.Idle;
                        characters[i].Picture.State = RenderChunk.RenderChunkState.FadeIn;
                        characters[i].Title.State = RenderChunk.RenderChunkState.FadeIn;
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
#if WINDOWS_PHONE						
                    Monster monster = Character.CreateCharacter(mtmp, this, "monster") as Monster;//new Monster(es == 1 ? "魔军" : "妖军" + (i + 1).ToString(), this);
#else
					Monster monster = Character.CreateCharacter(mtmp, this) as Monster;//new Monster(es == 1 ? "魔军" : "妖军" + (i + 1).ToString(), this);
#endif					
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

                
                using(System.IO.Stream stream = TitleContainer.OpenStream(name + ".xml"))
				{
                XDocument doc = XDocument.Load(stream);
                XElement defelement = doc.Element("SceneDef");
                if (defelement != null)
                {
                    XElement configelement = defelement.Element("Config");
                    if (configelement != null)
                    {
                        if (configelement.Element("FixedEnemyNum") != null)
                        {
                            GameConst.FixedEnemyNum = int.Parse(configelement.Element("FixedEnemyNum").Value);
                        }
                        if (configelement.Element("PlayerSpeed") != null)
                        {
                            GameConst.PlayerSpeed = int.Parse(configelement.Element("PlayerSpeed").Value);
                        }
                        if (configelement.Element("PlayerHP") != null)
                        {
                            GameConst.PlayerHP = int.Parse(configelement.Element("PlayerHP").Value);
                        }
                        if (configelement.Element("PlayerATK") != null)
                        {
                            GameConst.PlayerAtk = int.Parse(configelement.Element("PlayerATK").Value);
                        }
                        if (configelement.Element("BossRushMode") != null)
                        {
                            GameConst.BossRushMode = int.Parse(configelement.Element("BossRushMode").Value);
                        }
                        if (configelement.Element("BossRushMode1Offset") != null)
                        {
                            GameConst.BossRushMode1Offset = int.Parse(configelement.Element("BossRushMode1Offset").Value);
                        }
						if (configelement.Element("ViewportScrollRange") != null)
						{
							GameConst.ViewportScrollRange = float.Parse(configelement.Element("ViewportScrollRange").Value);
						}
                        if (configelement.Element("ViewportScrollResetTime") != null)
                        {
                            GameConst.ViewportScrollResetTime = float.Parse(configelement.Element("ViewportScrollResetTime").Value);
                        }
                    }
                    foreach (XElement element in defelement.Elements("NpcDef"))
                    {
                        XElement npcelement = element.Element("Npc");
                        if (npcelement != null)
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

                            if (npcelement.Element("Name") != null)
                            {
                                npcname = npcelement.Element("Name").Value;
                            }
                            if (npcelement.Element("HP") != null)
                            {
                                hp = int.Parse(npcelement.Element("HP").Value);
                            }
                            if (npcelement.Element("ScreenX") != null)
                            {
                                x = int.Parse(npcelement.Element("ScreenX").Value);
                            }
                            if (npcelement.Element("ScreenY") != null)
                            {
                                y = int.Parse(npcelement.Element("ScreenY").Value);
                            }
                            if (npcelement.Element("Speed") != null)
                            {
                                speed = int.Parse(npcelement.Element("Speed").Value);
                            }
                            if (npcelement.Element("Picture") != null)
                            {
                                pic = npcelement.Element("Picture").Value;
                            }
                            if (npcelement.Element("Layer") != null)
                            {
                                layer = int.Parse(npcelement.Element("Layer").Value);
                            }
                            if (npcelement.Element("Quest") != null)
                            {
                                //layer = int.Parse(npcelement.Element("Layer").Value);
                                foreach (XElement qelement in npcelement.Elements("Quest").Elements())
                                {
                                    quests.Add(qelement.Name.LocalName);
                                }
                            }
                            if (npcelement.Element("Visible") != null)
                            {
                                visible = bool.Parse(npcelement.Element("Visible").Value);
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

                    foreach (XElement element in defelement.Elements("HoverStoneDef"))
                    {
                        XElement stoneelement = element.Element("HoverStone");
                        if (stoneelement != null)
                        {
                            HoverStone hs = new HoverStone();
                            if (stoneelement.Element("Texture") != null)
                            {
                                hs.TextureFileName = stoneelement.Element("Texture").Value;
                                hs.Texture = GameConst.Content.Load<Texture2D>(@"stone/" + stoneelement.Element("Texture").Value);
                            }
                            if (stoneelement.Element("Position") != null)
                            {
                                float x = 0, y = 0;
                                if (stoneelement.Element("Position").Element("X") != null)
                                {
                                    x = float.Parse(stoneelement.Element("Position").Element("X").Value);
                                }
                                if (stoneelement.Element("Position").Element("Y") != null)
                                {
                                    y = float.Parse(stoneelement.Element("Position").Element("Y").Value);
                                }

                                hs.Position = new Vector2(x, y);
                            }
                            if (stoneelement.Element("Layer") != null)
                            {
                                hs.Layer = int.Parse(stoneelement.Element("Layer").Value);
                            }
                            if (stoneelement.Element("Size") != null)
                            {
                                float x = 0, y = 0;
                                if (stoneelement.Element("Size").Element("X") != null)
                                {
                                    x = float.Parse(stoneelement.Element("Size").Element("X").Value);
                                }
                                if (stoneelement.Element("Size").Element("Y") != null)
                                {
                                    y = float.Parse(stoneelement.Element("Size").Element("Y").Value);
                                }

                                hs.Size = new Vector2(x, y);
                            }
                            if (stoneelement.Element("Range") != null)
                            {
                                hs.AMP = float.Parse(stoneelement.Element("Range").Value);
                            }
                            if (stoneelement.Element("CycleTime") != null)
                            {
                                hs.TimeOfCycle = float.Parse(stoneelement.Element("CycleTime").Value);
                            }
                            AddRenderChunk(hs);
                        }
                    }
                }
               
       }
                //create task track 
                UIDialog dialog = UIMgr.AddUIControl("UIDialog", "dlg_questtrck", /*755, 174,*/(int)UILayout.Right, (int)UILayout.Center, 247, 345, -1, 99, this) as UIDialog;
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
            for (int i = 0; i < battlecharacters.Count; ++i)
            {
                if (battlecharacters[i].TemplateID == 3)
                {
                    if (battlecharacters[i].HP <= battlecharacters[i].MaxHP / 2)
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
                        if (localplayer.OperateTarget != localplayer)
                        {
#if WINDOWS_PHONE						
                            Spell fireball = Character.CreateCharacter("fireball", this, "fireball") as Spell;
#else
							Spell fireball = Character.CreateCharacter("fireball", this) as Spell;
#endif														
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

                                localplayer.AddActionSet("Attack2", CharacterState.Attack2, CharacterActionSetChangeFactor.AnimationCompleted, null);
                                localplayer.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.Immediate, null);
                            }
                        }
                        else
                        {
                            localplayer.HP += 100;
                            localplayer.HP = (int)MathHelper.Min((float)localplayer.HP, (float)localplayer.MaxHP);
                            //play number animation
                            effects.NumberAnimation na = new effects.NumberAnimation(100);
                            na.Position = new Vector2(localplayer.Position.X, localplayer.Position.Y - localplayer.Picture.FrameSize.Y * 0.5f);
                            na.Color = new Color(0.0f, 1.0f, 0.0f);
                            na.Play(this);
                            GoNextActionRound();
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
            for (int i = 0; i < battlecharacters.Count; ++i)
            {
                battlecharacters[i].Order = new ActionOrder(battlecharacters[i], random.Next(10, 20));
                actionlist.Add(battlecharacters[i].Order);
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
                for (int i = 0; i < renderchunks.Count; ++i)
                {
                    if (renderchunks[i] is Cloud)
                    {
                        //rc.State = RenderChunk.RenderChunkState.FadeOut;
                        renderchunks[i].Position = new Vector2((float)random.NextDouble() * (float)GameConst.ScreenWidth * 0.8f,
                                                    (float)random.NextDouble() * (float)GameConst.ScreenHeight)
                                                    + new Vector2(viewport.X, viewport.Y);

                    }
                }
                localplayer.Position = new Vector2(localplayer.Position.X, localplayer.Position.Y - GameConst.BossRushMode1Offset);

                for (int i = 0; i < battlecharacters.Count; ++i)
                {
                    if (battlecharacters[i].TemplateID == 3)
                    {
                        battlecharacters[i].Position = new Vector2(battlecharacters[i].Position.X,
                                                                    battlecharacters[i].Position.Y + GameConst.BossRushMode1Offset);
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
                for (int i = 0; i < renderchunks.Count; ++i)
                {
                    if (renderchunks[i] is Cloud)
                    {
                        //rc.State = RenderChunk.RenderChunkState.FadeOut;
                        renderchunks[i].Position = new Vector2((float)random.NextDouble() * (float)GameConst.ScreenWidth * 0.8f,
                                                    (float)random.NextDouble() * (float)GameConst.ScreenHeight)
                                                    + new Vector2(viewport.X, viewport.Y);

                    }
                }
                localplayer.Position = new Vector2(localplayer.Position.X, localplayer.Picture.FrameSize.Y + viewport.Y);
                for (int i = 0; i < battlecharacters.Count; ++i)
                {
                    if (battlecharacters[i].TemplateID == 3)
                    {
                        battlecharacters[i].Position = new Vector2(battlecharacters[i].Position.X,
                                                                GameConst.ScreenHeight - battlecharacters[i].Picture.FrameSize.Y * 0.5f + viewport.Y);
                        battlecharacters[i].Picture.Angle = MathHelper.ToRadians(30.0f);
                        battlecharacters[i].Picture.OriginAngle = MathHelper.ToRadians(30.0f);
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
                    for (int i = 0; i < renderchunks.Count; ++i)
                    {
                        if (renderchunks[i] is Cloud)
                        {
                            renderchunks[i].State = RenderChunk.RenderChunkState.FadeIn;
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
                    for (int i = 0; i < renderchunks.Count; ++i)
                    {
                        if (renderchunks[i] is Cloud)
                        {
                            renderchunks[i].State = RenderChunk.RenderChunkState.FadeIn;
                            renderchunks[i].Position = new Vector2((float)random.NextDouble() * actualSize.Z, (float)random.NextDouble() * actualSize.W * 0.9f);
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
                    for (int i = 0; i < renderchunks.Count; ++i)
                    {
                        if (renderchunks[i] is Cloud)
                        {
                            renderchunks[i].State = RenderChunk.RenderChunkState.FadeIn;
                            renderchunks[i].Position = new Vector2((float)random.NextDouble() * actualSize.Z, (float)random.NextDouble() * actualSize.W * 0.9f);
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
                    for (int i = 0; i < renderchunks.Count; ++i)
                    {
                        if (renderchunks[i] is Cloud)
                        {
                            renderchunks[i].State = RenderChunk.RenderChunkState.FadeOut;
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

#if WINDOWS
        private void SaveHoverStones()
        {
            //System.IO.Stream stream = TitleContainer.OpenStream(name + ".xml");
            try
            {
                FileStream streamread = new FileStream(name + ".xml", FileMode.Open);
                if (streamread != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(streamread);
                    streamread.Close();
                    streamread.Dispose();
                    FileStream streamwrite = new FileStream(name + ".xml", FileMode.Create);
                    if (streamwrite != null)
                    {
                        XmlNodeList constdef = doc.GetElementsByTagName("HoverStoneDef", "");
                        if (constdef.Count > 0)
                        {
                            constdef[0].RemoveAll();
                            for (int i = 0; i < renderchunks.Count; ++i)
                            {

                                if (renderchunks[i] is HoverStone)
                                {
                                    HoverStone rc = renderchunks[i] as HoverStone;
                                    XmlElement xe = doc.CreateElement("HoverStone");
                                    XmlElement etexturename = doc.CreateElement("Texture");
                                    etexturename.InnerText = "stone1";
                                    xe.AppendChild(etexturename);

                                    XmlElement epos = doc.CreateElement("Position");
                                    XmlElement eposx = doc.CreateElement("X");
                                    eposx.InnerText = rc.Position.X.ToString();
                                    XmlElement eposy = doc.CreateElement("Y");
                                    eposy.InnerText = rc.Position.Y.ToString();
                                    epos.AppendChild(eposx);
                                    epos.AppendChild(eposy);
                                    xe.AppendChild(epos);

                                    XmlElement elayer = doc.CreateElement("Layer");
                                    elayer.InnerText = rc.Layer.ToString();
                                    xe.AppendChild(elayer);

                                    XmlElement esize = doc.CreateElement("Size");
                                    XmlElement esizex = doc.CreateElement("X");
                                    esizex.InnerText = rc.Size.X.ToString();
                                    XmlElement esizey = doc.CreateElement("Y");
                                    esizey.InnerText = rc.Size.Y.ToString();
                                    esize.AppendChild(esizex);
                                    esize.AppendChild(esizey);
                                    xe.AppendChild(esize);

                                    XmlElement erange = doc.CreateElement("Range");
                                    erange.InnerText = rc.AMP.ToString();
                                    xe.AppendChild(erange);

                                    XmlElement ectime = doc.CreateElement("CycleTime");
                                    ectime.InnerText = rc.TimeOfCycle.ToString();
                                    xe.AppendChild(ectime);

                                    constdef[0].AppendChild(xe);
                                }
                            }
                        }
                        doc.Save(streamwrite);
                        streamwrite.Close();
                        streamwrite.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("SaveHoverStones Failed : " + e.Message);
            }

            // XmlNodeList hsdef = doc.GetElementsByTagName("HoverStoneDef", "");

        }

        public void DoEditorMenu(int x, int y, bool showit)
        {
            if (!showit)
            {
                UIDialog dialoge = UIMgr.GetUIControlByName("editorcontextmenu") as UIDialog;
                if (dialoge != null)
                {
                    dialoge.State = RenderChunk.RenderChunkState.Delete;
                }
                return;
            }

            UIDialog dialog = UIMgr.GetUIControlByName("editorcontextmenu") as UIDialog;
            if (dialog == null)
            {
                dialog = UIMgr.AddUIControl("UIDialog", "editorcontextmenu", x + 20, y, 70, 100, -1, 99, this) as UIDialog;
            }
            else
            {
                dialog.Position = new Vector2(x + 20, y);
                dialog.RemoveUIControl("delbtn");
                dialog.RemoveUIControl("addbtn");
                if (_hostChunk != null)
                {
                    UITextButton btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                    if (btn != null)
                    {
                        btn.Text = "删除";
                        btn.FontColor = Color.DeepSkyBlue;
                        dialog.AddUIControl(btn, "delbtn", 2, 2, 66, 20, -1, this);
                    }
                }
                else
                {
                    UITextButton btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                    if (btn != null)
                    {
                        btn.Text = "新建";
                        btn.FontColor = Color.DeepSkyBlue;
                        dialog.AddUIControl(btn, "addbtn", 2, 2, 66, 20, -1, this);
                    }
                }
                return;
            }
            if (dialog != null)
            {
                if (_hostChunk != null)
                {
                    UITextButton btn = dialog.GetUIControlByName("delbtn") as UITextButton;
                    if (btn == null)
                    {
                        btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                    }
                    if (btn != null)
                    {
                        btn.Text = "删除";
                        btn.FontColor = Color.DeepSkyBlue;
                        dialog.AddUIControl(btn, "delbtn", 2, 2, 66, 20, -1, this);
                    }
                }
                else
                {
                    UITextButton btn = dialog.GetUIControlByName("addbtn") as UITextButton;
                    if (btn == null)
                    {
                        btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                    }
                    if (btn != null)
                    {
                        btn.Text = "新建";
                        btn.FontColor = Color.DeepSkyBlue;
                        dialog.AddUIControl(btn, "addbtn", 2, 2, 66, 20, -1, this);
                    }
                }
            }
        }

        protected void delbtn_OnClick(object sender, MouseEventArgs e)
        {
            UITextButton btn = sender as UITextButton;
            btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
            if (_hostChunk != null)
            {
                _hostChunk.State = RenderChunk.RenderChunkState.Delete;
            }
        }

        protected void addbtn_OnClick(object sender, MouseEventArgs e)
        {
            UITextButton btn = sender as UITextButton;
            btn.Parent.State = RenderChunk.RenderChunkState.FadeOutToDel;
            HoverStone hs = new HoverStone();
            hs.TextureFileName = "stone1";
            hs.Texture = GameConst.Content.Load<Texture2D>(@"stone/stone1");
            hs.Position = new Vector2(viewport.X + e.X, viewport.Y + e.Y);
            hs.Layer = Convert.ToInt32(random.Next(18,22));
            hs.Size = new Vector2(1, 1);
            hs.AMP = 10;
            hs.TimeOfCycle = 2;
            AddRenderChunk(hs);
        }

        public void EditorOperate(MainGame.EditorOp op, int x, int y)
        {
            switch (op)
            {
                case MainGame.EditorOp.Move:
                    if (_hostChunk != null)
                    {
                        _hostChunk.Position = new Vector2(viewport.X + x, viewport.Y + y);
                    }
                    break;
                case MainGame.EditorOp.Scale:
                    {
                        if (_hostChunk != null)
                        {
                            Vector2 s = _hostChunk.Size;
                            float f = Math.Max(x, y);
                            //Log.WriteLine(x.ToString());
                            s += new Vector2(f * 0.01f, f * 0.01f);
                            s.X = MathHelper.Clamp(s.X, 0.5f, 10.0f);
                            s.Y = MathHelper.Clamp(s.Y, 0.5f, 10.0f);
                            _hostChunk.Size = s;
                        }
                    }
                    break;
                case MainGame.EditorOp.Save:
                    SaveHoverStones();
                    break;
            }
        }
#endif
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


        RenderChunk _hostChunk = null;
        public void HighLightChunkByPoint(int _x, int _y)
        {
            int x = _x + (int)viewport.X;
            int y = _y + (int)viewport.Y;
            RenderChunk host = null;
            for (int i = 0; i < renderchunks.Count; ++i)
            {
                RenderChunk rc = renderchunks[i];
                if (!(rc is HoverStone))
                    continue;
                Rectangle rect = new Rectangle((int)(rc.Position.X - rc.FrameSize.X * 0.3f),
                                                (int)(rc.Position.Y - rc.FrameSize.Y * 0.3f),
                                                (int)(rc.FrameSize.X * 0.6f),
                                                (int)(rc.FrameSize.Y * 0.6f));
                if (rect.Contains(new Point(x, y)))
                {
                    host = rc;
                    break;
                }
            }
            if (host != null)
            {
                if (_hostCharacter != null)
                    _hostCharacter.Picture.HighLight = false;
                host.HighLight = true;
                _hostChunk = host;
            }
            else
            {
                if (_hostChunk != null)
                {
                    _hostChunk.HighLight = false;
                    _hostChunk = null;
                }
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
            for (int i = 0; i < characters.Count; ++i)
            {
                if (characters[i] is Player && localplayer.Operate != Character.OperateType.Magic)
                    continue;
                if (characters[i].Picture.State != RenderChunk.RenderChunkState.Show)
                    continue;
                Rectangle rect = new Rectangle((int)(characters[i].Position.X - characters[i].Picture.FrameSize.X * 0.3f),
                                                (int)(characters[i].Position.Y - characters[i].Picture.FrameSize.Y * 0.3f),
                                                (int)(characters[i].Picture.FrameSize.X * 0.6f),
                                                (int)(characters[i].Picture.FrameSize.Y * 0.6f));
                if (rect.Contains(new Point(x, y)))
                {
                    host = characters[i];
                    break;
                }
            }
            if (host == null)
            {
                for (int i = 0; i < battlecharacters.Count; ++i)
                {
                    Rectangle rect = new Rectangle((int)(battlecharacters[i].Position.X - battlecharacters[i].Picture.FrameSize.X * 0.3f),
                                                    (int)(battlecharacters[i].Position.Y - battlecharacters[i].Picture.FrameSize.Y * 0.3f),
                                                    (int)(battlecharacters[i].Picture.FrameSize.X * 0.6f),
                                                    (int)(battlecharacters[i].Picture.FrameSize.Y * 0.6f));
                    if (rect.Contains(new Point(x, y)))
                    {
                        host = battlecharacters[i];
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
#if WINDOWS
                if (host is Player)
                {
                    if (localplayer.Operate == Character.OperateType.Magic)
                    {
                        GameCursor.SetCursor(GameCursor.CursorType.Magic);
                    }
                }
                else if (host is Npc)
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
#endif
            }
            else
            {
#if WINDOWS
                GameCursor.SetCursor(GameCursor.CursorType.Normal);
#endif
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
                int layer = random.Next(5, 30);

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


        internal void UpdatePlayerMovement(PlayerMoveRequest msg)
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

        internal void UpdatePlayerTarget(PlayerTargetChanged msg)
        {
            Player _p = FindNetPlayer(msg.ClientID);
            if (_p != null)
            {
                _p.Target = new Vector2(msg.Target[0], msg.Target[1]);
            }
        }

    }

}
