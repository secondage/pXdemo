using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Windows.Forms;
using demo.uicontrols;
using System.Reflection;
using System.Runtime.InteropServices;
using Beetle;
using NetSyncObject;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Diagnostics;
using ProjectMercury;
using ProjectMercury.Renderers;
using demo.animation;
using System.Xml.Linq;

namespace demo
{


    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public partial class MainGame : Microsoft.Xna.Framework.Game
    {
        public enum EditorOp
        {
            Move,
            Copy,
            Cut,
            Paste,
            Delete,
            Create,
            Save,
            Scale,
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Scene CurrentScene;
        private Player player;
        private SpriteFont mainfont;
        private static TcpChannel clientchannel;
        private LoginDialog dlgLogin;
        private long ClientID;
        private bool ContentLoadCompleted = false;

        private ParticleEffectWrapper peTrails;
        private ParticleEffectWrapper peSpawn;
        private ParticleEffectWrapper peClick;

        private Texture2D loadingTexture;

        public static bool IsEditorMode { get; set; }

        private System.Threading.Tasks.Task contentLoadingTask = null;

        Texture2D[] cloudTextureArray = new Texture2D[15];

        //assembly of Mercury Particle System
       

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            GameConst.ScreenWidth = 1024;
            GameConst.ScreenHeight = 768;
            GameConst.Graphics = graphics;
            GameConst.Content = Content;
            GameConst.GameWindow = this.Window;
            graphics.PreferredBackBufferWidth = GameConst.ScreenWidth;
            graphics.PreferredBackBufferHeight = GameConst.ScreenHeight;
            graphics.DeviceCreated += new EventHandler<EventArgs>(Graphics_DeviceCreated);
            Content.RootDirectory = "Content";

           
            
           
            Beetle.TcpUtils.Setup(100, 1, 1);
#if WINDOWS
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = false;
            this.Window.Title = "mmodemo";
            Control ctrl = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
            ctrl.KeyUp += new KeyEventHandler(MainWindow_KeyUp);
            //fixme:
            //keydown 无法响应。。
            ctrl.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            ctrl.Move += new EventHandler(MainWindow_Move);
            ctrl.MouseMove += new MouseEventHandler(MainWindow_MouseMove);
            ctrl.MouseUp += new MouseEventHandler(MainWindow_MouseUp);
            ctrl.MouseWheel += new MouseEventHandler(MainWindow_MouseWheel);
            ctrl.MouseDown += new MouseEventHandler(MainWindow_MouseDown);
            ctrl.MouseClick += new MouseEventHandler(MainWindow_MouseClick);
            GameCursor.Initialize(Cursor.Current.Handle);
            GameCursor.SetCursor(GameCursor.CursorType.Normal);
#endif
        }
        /// <summary>
        /// 窗口内按键弹起消息Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void MainWindow_KeyUp(object sender, KeyEventArgs args)
        {
            if (args.KeyCode == System.Windows.Forms.Keys.E && args.Control)
            {
                MainGame.IsEditorMode = !MainGame.IsEditorMode;
                Control window = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
                if (MainGame.IsEditorMode)
                {
                    window.Invoke(new Action(() =>
                    {
                        this.Window.Title = "Editor mode";
                    }));
                }
                else
                {
                    if (player != null)
                    {
                        window.Invoke(new Action(() =>
                        {
                            this.Window.Title = player.Name;
                        }));
                        player.UpdateSceneScroll();
                    }
                }
            }
            else if (args.KeyCode == System.Windows.Forms.Keys.S && args.Control)
            {
                if (MainGame.IsEditorMode)
                {
                    CurrentScene.EditorOperate(EditorOp.Save, 0, 0);
                }
            }
            else if (args.KeyCode == System.Windows.Forms.Keys.S)
            {
                if (!MainGame.IsEditorMode)
                    CurrentScene.IntoBattle();
            }
        }
        /// <summary>
        /// 窗口内按键按下消息Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void MainWindow_KeyDown(object sender, KeyEventArgs args)
        {

        }
        /// <summary>
        /// 窗口移动消息消息Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void MainWindow_Move(object sender, EventArgs args)
        {
            if (dlgLogin != null)
            {
                dlgLogin.Location = new System.Drawing.Point((this.Window.ClientBounds.Width - dlgLogin.Size.Width) / 2 + this.Window.ClientBounds.X,
                                                        (this.Window.ClientBounds.Height - dlgLogin.Size.Height) * 3 / 4 + this.Window.ClientBounds.Y);
            }
        }
        /// <summary>
        /// 鼠标在窗口内按下消息Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void MainWindow_MouseDown(object sender, MouseEventArgs args)
        {
            UIMgr.HandleMessage(UIMessage.MouseDown, args.X, args.Y);
        }
        /// <summary>
        /// 鼠标在窗口内点击消息Callback
        /// </summary>
        Vector3 clickPos = new Vector3();
        protected void MainWindow_MouseClick(object sender, MouseEventArgs args)
        {
            int result = UIMgr.HandleMessage(UIMessage.MouseClick, args.X, args.Y);
            if (result != 0)
                return;

            if (!MainGame.IsEditorMode)
            {
                if (args.Button == MouseButtons.Left)
                {
                    OnMouseLeftClick(args.X, args.Y);
                }
            }
        }
        private int _mousex;
        private int _mousey;
        private MouseButtons _mousebutton;
        /// <summary>
        /// 鼠标在窗口内移动消息Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void MainWindow_MouseMove(object sender, MouseEventArgs args)
        {
            _mousex = args.X;
            _mousey = args.Y;
            _mousebutton = args.Button;
            int result = UIMgr.HandleMessage(UIMessage.MouseMove, args.X, args.Y);
            if (result != 0)
                return;
            if (MainGame.IsEditorMode)
            {
                if (args.Button == MouseButtons.Left)
                {
                    CurrentScene.EditorOperate(EditorOp.Move, args.X, args.Y);
                    CurrentScene.HighLightChunkByPoint(args.X, args.Y);

                    Vector3 p = new Vector3(args.X, args.Y, 0);
                    peTrails.Trigger(ref p);
                }
                else
                {
                    CurrentScene.HighLightChunkByPoint(args.X, args.Y);
                }
            }
            else
            {
                CurrentScene.HighLightCharacterByPoint(args.X, args.Y);
            }
        }
        /// <summary>
        /// 鼠标按键在窗口内弹起消息Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void MainWindow_MouseUp(object sender, MouseEventArgs args)
        {
            if (MainGame.IsEditorMode)
            {
                if (args.Button == MouseButtons.Left)
                {
                    CurrentScene.DoEditorMenu(0, 0, false);
                }
                else if (args.Button == MouseButtons.Right)
                {
                    CurrentScene.DoEditorMenu(args.X, args.Y, true);
                }
            }
            UIMgr.HandleMessage(UIMessage.MouseUp, args.X, args.Y);
        }
        /// <summary>
        /// 鼠标在窗口内滚轮消息Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void MainWindow_MouseWheel(object sender, MouseEventArgs args)
        {
            if (MainGame.IsEditorMode)
            {
                if (args.Delta != 0)
                    CurrentScene.EditorOperate(EditorOp.Scale, args.Delta, args.Delta);
            }
        }
        /// <summary>
        /// 图形设备创建Callback，可以在此处修改图形设备的参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void Graphics_DeviceCreated(Object sender, EventArgs args)
        {
            GraphicsDeviceManager graphics = sender as GraphicsDeviceManager;
            graphics.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Immediate;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            CurrentScene = new Scene("scene1", 0, 0, GameConst.ScreenWidth, GameConst.ScreenHeight);
            //CurrentScene.ActualSize = new Vector4(0, 0, 2048 * GameConst.BackgroundScale, 2048 * GameConst.BackgroundScale);
            CurrentScene.ActualSize = new Vector4(0, 0, 20000, 2048 * GameConst.BackgroundScale);

            //init net client
            try
            {
                string ipaddress = "127.0.0.1";
                int port = 9610;
                using(System.IO.Stream stream = TitleContainer.OpenStream("servers.xml"))
                {
                    XDocument doc = XDocument.Load(stream);

                    XNode x = doc.NextNode;
                    XElement serverelement = doc.Element("Server");
                    if (serverelement != null)
                    {
                        ipaddress = serverelement.Element("Ip").Value;
                        port = int.Parse(serverelement.Element("Port").Value);
                    }
                }
                clientchannel = TcpServer.CreateClient(ipaddress, port);
                clientchannel.SetPackage<HeadSizePackage>().ReceiveMessage = ReceiveMessage;
                clientchannel.ChannelDisposed += new EventChannelDisposed(channel_ChannelDisposed);
                clientchannel.BeginReceive();
            }
            catch (Exception e_)
            {
                MessageBox.Show(e_.Message);
            }

            //this.Window.ClientBounds


            base.Initialize();
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ParticleEffectWrapper.Initialise(this.graphics);

            loadingTexture = Content.Load<Texture2D>("ui/loading");

            GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, GameConst.ScreenWidth, GameConst.ScreenHeight);
            var source = new CancellationTokenSource();
            var token = source.Token;
            contentLoadingTask = new System.Threading.Tasks.Task(new Action(() =>
                {
                    try
                    {
                        //token.ThrowIfCancellationRequested();
                        mainfont = Content.Load<SpriteFont>(@"font/YaHeiCh16");
                        GameConst.CurrentFont = mainfont;

                        UIMgr.ControlsTexture = Content.Load<Texture2D>(@"ui/controls");

                        CharacterTitle.TakeQuestTexture = Content.Load<Texture2D>(@"questicon/take");
                        CharacterTitle.QuestCompletedTexture = Content.Load<Texture2D>(@"questicon/done");
                        CharacterTitle.QuestNonCompletedTexture = Content.Load<Texture2D>(@"questicon/notdone");


                        // load cloud texture
                        for (int i = 0; i < 15; ++i)
                        {
                            cloudTextureArray[i] = Content.Load<Texture2D>(@"cloud/yun_b" + string.Format("{0:d2}", i));
                        }

                        CharacterTitle.BlockTexture = Content.Load<Texture2D>(@"effect/block");

                       


                        Texture2D texminimap = Content.Load<Texture2D>(@"minimap/scene1");
                        Texture2D texminimapchar = Content.Load<Texture2D>(@"minimap/charactericon");
                        Texture2D texmapmask = Content.Load<Texture2D>(@"minimap/mapmask");
                        CurrentScene.InitMiniMap(texminimap, texminimapchar, texmapmask, 0, GameConst.ScreenHeight - 256, 256, 256);

                       
                        UIMgr.AddUIControl("Dialog_Leader", "leader_dlg", (int)UILayout.Right, (int)UILayout.Top, 0, 0, -1, 99, this);
                        CurrentScene.GenerateClouds(cloudTextureArray);
                        CurrentScene.SortRenderChunksByLayer();


                        //init character
                        Content.Load<CharacterDefinition.PicDef>(@"chardef/char3");
                        CurrentScene.LoadGameData();
                        CurrentScene.LoadBackground();

                        peTrails = new ParticleEffectWrapper();
                        peTrails.Load("magictrail");
                        peTrails.Layer = 100;
                        CurrentScene.AddRenderChunk(peTrails);
                        peSpawn = new ParticleEffectWrapper();
                        peSpawn.Load("BeamMeUp");
                        peSpawn.Layer = 100;
                        CurrentScene.AddRenderChunk(peSpawn);
                        peClick = new ParticleEffectWrapper();
                        peClick.Load("clickeffect");
                        peClick.Layer = 100;
                        CurrentScene.AddRenderChunk(peClick);


                        Thread.Sleep(10);
                        ContentLoadCompleted = true;
                    }
                    catch (ContentLoadException e)
                    {
                        throw new ContentLoadException("载入资源发生错误，程序关闭: " + e.Message);
                    }
                }
            ), token);

            contentLoadingTask.ContinueWith(task =>
            {
                MessageBox.Show(task.Exception.InnerException.Message);
                this.Exit();
            }
            , TaskContinuationOptions.OnlyOnFaulted);

            contentLoadingTask.ContinueWith(task =>
            {
                MessageBox.Show(task.Exception.Message);
                this.Exit();
            }
           , TaskContinuationOptions.OnlyOnCanceled);

            contentLoadingTask.Start();

            if (clientchannel != null)
            {
                dlgLogin = new LoginDialog();
                System.Drawing.Size size = dlgLogin.Size;
                size.Width = (int)((float)(size.Width) * GameConst.ForegroundScale.X);
                size.Height = (int)((float)(size.Height) * GameConst.ForegroundScale.Y);
                dlgLogin.Location = new System.Drawing.Point((this.Window.ClientBounds.Width - size.Width) / 2 + this.Window.ClientBounds.X,
                                                        (this.Window.ClientBounds.Height - size.Height) * 3 / 4 + this.Window.ClientBounds.Y);
                dlgLogin.Size = size;
                dlgLogin.Show(System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle));
            }
            else
            {
                PlayerLoginSelfMsg msg = new PlayerLoginSelfMsg();
                msg.ClientID = 0;
                msg.Name = "player1";
                msg.Position = new float[] { GameConst.ScreenWidth / 2, GameConst.ScreenHeight / 2 };
                msg.Speed = GameConst.PlayerSpeed;
                msg.ATK = 500;
                msg.DEF = 30;
                msg.HP = 1000;
                msg.MaxHP = 1000;
                CreateLocalPlayer(msg);
            }
        }
        /// <summary>
        /// 删除角色实例
        /// </summary>
        /// <param name="pn">角色属性描述</param>
        private void DestoryPlayer(PlayerLogoutMsg pn)
        {
            NetPlayer p = CurrentScene.FindNetPlayer(pn.ClientID);
            if (p != null)
            {
                if (CurrentScene.State == Scene.SceneState.Map)
                {
                    p.Picture.State = RenderChunk.RenderChunkState.FadeOutToDel;
                    p.Title.State = RenderChunk.RenderChunkState.FadeOutToDel;
                }
                else
                {
                    p.Picture.State = RenderChunk.RenderChunkState.Delete;
                    p.Title.State = RenderChunk.RenderChunkState.Delete;
                }
                CurrentScene.DelNetPlayer(p);
            }

        }
        /// <summary>
        /// 创建本地Player角色
        /// </summary>
        /// <param name="pn">角色创建属性msg，描述角色基本属性</param>
        private void CreateLocalPlayer(PlayerLoginSelfMsg pn)
        {
            contentLoadingTask.Wait();
            player = new Player(pn.Name, CurrentScene);
            CharacterDefinition.PicDef pd = Content.Load<CharacterDefinition.PicDef>(@"chardef/char3");
            CharacterPic cpic = new CharacterPic(pd, 15);
            player.Picture = cpic;
            CharacterTitle title = new CharacterTitle(GameConst.CurrentFont);
            title.Layer = 15;
            title.NameString = pn.Name;
            title.Character = player;
            player.Title = title;
            player.Position = new Vector2(pn.Position[0], pn.Position[1]);
            player.Speed = pn.Speed;//GameConst.PlayerSpeed;
            player.ATK = pn.ATK;//GameConst.PlayerAtk;
            player.DEF = pn.DEF;//GameConst.PlayerAtk;
            player.HP = pn.HP;//GameConst.PlayerHP;
            player.MaxHP = pn.MaxHP;// GameConst.PlayerHP;
            //player.AddActionSet("Idle", CharacterState.Spawn, CharacterActionSetChangeFactor.EffectCompleted, "Spawn");
            if (peSpawn != null)
            {
                Vector3 p3 = new Vector3(pn.Position[0], pn.Position[1], 0);
                peSpawn.Trigger(ref p3);
            }

            player.AddActionSet("Idle", CharacterState.Spawn, CharacterActionSetChangeFactor.Time, 2.0);
            player.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.Immediate, null);
            player.ClientID = pn.ClientID;
            CurrentScene.AddCharacter(player);
            CurrentScene.Player = player;
            player.TrailParticle = peTrails;
            player.ResetSceneScroll(true);
            /*Control ctrl = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
            ctrl.Invoke(new Action(() =>
            {
                this.Window.Title = pn.Name;
            }));*/

        }
        /// <summary>
        /// 创建远端Player实例
        /// </summary>
        /// <param name="pn">远端角色属性描述msg</param>
        private void CreatePlayer(PlayerLoginMsg pn)
        {
            NetPlayer playernet = new NetPlayer(pn.Name, CurrentScene);
            CharacterDefinition.PicDef pd = Content.Load<CharacterDefinition.PicDef>(@"chardef/char3");
            CharacterPic cpic = new CharacterPic(pd, 15);
            playernet.Picture = cpic;
            CharacterTitle title = new CharacterTitle(GameConst.CurrentFont);
            title.Layer = 15;
            title.NameString = pn.Name;
            title.Character = playernet;
            playernet.Title = title;
            playernet.Position = new Vector2(pn.Position[0], pn.Position[1]);
            playernet.Speed = pn.Speed;// GameConst.PlayerSpeed;
            playernet.ATK = pn.ATK;//GameConst.PlayerAtk;
            playernet.DEF = pn.DEF;//GameConst.PlayerAtk;
            playernet.HP = pn.HP;//GameConst.PlayerHP;
            playernet.MaxHP = pn.MaxHP;// GameConst.PlayerHP;
            //playernet.AddActionSet("Idle", CharacterState.Spawn, CharacterActionSetChangeFactor.EffectCompleted, "Spawn");
            playernet.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.Immediate, null);
            playernet.ClientID = pn.ClientID;
            //playernet.AddPreRenderEffect("Spawn", spawnEffect);
            CurrentScene.AddNetPlayer(playernet);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param _name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                this.Exit();
            if (!ContentLoadCompleted)
                return;
            UpdateInput();
            InterpolatioAnimationMgr.Update(gameTime);
            CurrentScene.Update(gameTime);
            ParticleEffectWrapper.SetTransformation(Matrix.CreateTranslation(-CurrentScene.Viewport.X, -CurrentScene.Viewport.Y, 0));
            UIMgr.Update(gameTime);
            //player.Update(gameTime);


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param _name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        Matrix idmatrix = new Matrix();
        float _loadingangle = 0;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            if (!ContentLoadCompleted)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(loadingTexture, new Vector2(GameConst.ScreenWidth - loadingTexture.Width / 2, GameConst.ScreenHeight - loadingTexture.Height / 2),
                                  null, Color.White, _loadingangle, new Vector2(loadingTexture.Width / 2, loadingTexture.Height / 2), 1.0f, SpriteEffects.None, 1.0f);
                spriteBatch.End();
                _loadingangle += 0.1f;
                Thread.Sleep(20);
                return;
            }

            GameConst.RenderCountPerFrame = 0;
            CurrentScene.RenderPrepositive();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            CurrentScene.Render(spriteBatch);
            spriteBatch.End();
            CurrentScene.RenderPostpositive();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            UIMgr.Render(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            if (player != null)
                spriteBatch.DrawString(mainfont, string.Format("{0:d}, {1:d} {2:d}", (int)player.Position.X, (int)player.Position.Y, GameConst.RenderCountPerFrame), Vector2.Zero, Color.Red);
            spriteBatch.End();
            base.Draw(gameTime);
        }


        HiPerfTimer clicktimer = new HiPerfTimer();
        double clicktime = 0;
        /// <summary>
        /// 鼠标左键单击消息响应
        /// </summary>
        /// <param name="cx">鼠标指针x坐标</param>
        /// <param name="cy">鼠标指针y坐标</param>
        private void OnMouseLeftClick(int cx, int cy)
        {
            if (CurrentScene.State == Scene.SceneState.Map)
            {
                if (clicktimer == null)
                {
                    clicktimer = new HiPerfTimer();
                    clicktimer.Start();
                }

                clicktime = clicktimer.GetTotalDuration();

                if (clicktime > 0.2 && (player.State == CharacterState.Idle || player.State == CharacterState.Moving))
                {
                    clicktimer.Stop();
                    clicktimer.Start();
                    int sresult = CurrentScene.SelectCharacter();
                    if (sresult == 0)
                    {
                        clickPos.X = CurrentScene.Viewport.X + cx;
                        clickPos.Y = CurrentScene.Viewport.Y + cy;
                        if (peClick != null)
                            peClick.Trigger(ref clickPos);

                        player.Target = new Vector2(CurrentScene.Viewport.X + cx, CurrentScene.Viewport.Y + cy);
                        if (player.State == CharacterState.Idle)
                        {
                            player.AddActionSet("Launch", CharacterState.Launch, CharacterActionSetChangeFactor.AnimationCompleted, null);
                            player.AddActionSet("Moving", CharacterState.Moving, CharacterActionSetChangeFactor.ArriveTarget, player.Target);
                            player.AddActionSet("Landing", CharacterState.Landing, CharacterActionSetChangeFactor.AnimationCompleted, null);
                            player.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.Immediate, null);
                            if (ClientID != 0)
                            {
                                player.StartMoveSyncTimer();
                                SendRequestMovementMsg(player);
                            }
                        }
                        else
                        {
                            if (ClientID != 0)
                                SendTargetChangedMsg(player);
                        }
                        if (!player.Interacting)
                            player.InteractiveTarget = null;
                    }
                    else if (sresult == 1) //选中其他角色
                    {
                        if (player.State == CharacterState.Idle)
                        {
                            if (ClientID != 0)
                            {
                                player.StartMoveSyncTimer();
                                SendRequestMovementMsg(player);
                            }
                        }
                        else
                        {
                            if (ClientID != 0)
                                SendTargetChangedMsg(player);
                        }
                    }
                }
                else
                {
                    if ((player.State == CharacterState.Idle || player.State == CharacterState.Moving))
                    {
                        Debug.WriteLine("too fast to click");
                    }
                }
            }
            else if (CurrentScene.State == Scene.SceneState.Battle)
            {
                CurrentScene.ConfirmOperateTarget();
            }
        }

        [Flags]
        enum ScrollDir
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8,
        }
        /// <summary>
        /// 控制屏幕卷轴
        /// </summary>
        /// <param name="dir">卷轴方向</param>
        private void HandleScreenScroll(ScrollDir dir)
        {
            Vector4 vp = CurrentScene.Viewport;
            bool isscrolled = false;//是否需要更新viewport
            if (CurrentScene.State == Scene.SceneState.Map)
            {
                if (MainGame.IsEditorMode)
                {
                    if (dir.HasFlag(ScrollDir.Down))
                    {
                        vp.Y += GameConst.ScrollSpeed;
                        isscrolled = true;
                    }
                    if (dir.HasFlag(ScrollDir.Up))
                    {
                        vp.Y -= GameConst.ScrollSpeed;
                        isscrolled = true;
                    }
                    if (dir.HasFlag(ScrollDir.Left))
                    {
                        vp.X -= GameConst.ScrollSpeed;
                        isscrolled = true;
                    }
                    if (dir.HasFlag(ScrollDir.Right))
                    {
                        vp.X += GameConst.ScrollSpeed;
                        isscrolled = true;
                    }
                    if (isscrolled)
                    {
                        MouseEventArgs mea = new MouseEventArgs(_mousebutton, 0, _mousex, _mousey, 0);
                        MainWindow_MouseMove(this, mea);
                        CurrentScene.Viewport = vp;
                        Debug_ClipScroll();
                    }

                }
            }
        }

        
        KeyboardState _ksLast = new KeyboardState();
        /// <summary>
        /// 更新输入， 跟消息响应机制不同， 本方法每帧采样一次， 来判断输入状态
        /// 对应没有消息循环机制的平台，但当framerate比较低的时候，会有输入
        /// 响应不及时的问题
        /// </summary>
        public void UpdateInput()
        {
            if (player == null)
                return;
            //若game窗口未获取输入焦点，则忽略之
            Control ctrl = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
            if (Form.ActiveForm == null || !Form.ActiveForm.Equals(ctrl))
                return;
            KeyboardState ks = Keyboard.GetState();
            if (CurrentScene.State == Scene.SceneState.Map)
            {
                ScrollDir dir = ScrollDir.None;
                if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                {
                    dir |= ScrollDir.Down;
                }
                if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                {
                    dir |= ScrollDir.Up;
                }
                if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    dir |= ScrollDir.Left;
                }
                if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    dir |= ScrollDir.Right;
                }
                HandleScreenScroll(dir);
            }

            //backup input status
            _ksLast = ks;
        }


        /// <remarks>Debug Only</remarks>
        public void Debug_ClipScroll()
        {
            Vector4 vp = CurrentScene.Viewport;
            //vp.X = MathHelper.Clamp(vp.X, 0.0f, (GameConst.BackgroundScale) * 2048.0f - GameConst.ScreenWidth);
            //vp.Y = MathHelper.Clamp(vp.Y, 0.0f, (GameConst.BackgroundScale) * 2048.0f - GameConst.ScreenHeight);
            vp.X = MathHelper.Clamp(vp.X, 0.0f, CurrentScene.ActualSize.Z);
            vp.Y = MathHelper.Clamp(vp.Y, 0.0f, CurrentScene.ActualSize.W - GameConst.ScreenHeight);

            CurrentScene.Viewport = vp;
        }
    }
}
