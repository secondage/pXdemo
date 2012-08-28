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
        private PreRenderEffect spawnEffect;
        private SpriteFont mainfont;
        private static TcpChannel clientchannel;
        private LoginDialog dlgLogin;
        private long ClientID;
        private SpriteBatchRenderer spritebatchrenderer;
        private bool ContentLoadCompleted = false;

        public static bool IsEditorMode { get; set; }

        private System.Threading.Tasks.Task contentLoadingTask = null;

        Texture2D[] cloudTextureArray = new Texture2D[15];

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
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

#endif
            Control ctrl = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
            ctrl.Move += new EventHandler(MainWindow_Move);
            ctrl.MouseMove += new MouseEventHandler(MainWindow_MouseMove);
            ctrl.MouseUp += new MouseEventHandler(MainWindow_MouseUp);
            ctrl.MouseWheel += new MouseEventHandler(MainWindow_MouseWheel);
            ctrl.MouseDown += new MouseEventHandler(MainWindow_MouseDown);
            ctrl.MouseClick += new MouseEventHandler(MainWindow_MouseClick);
            GameCursor.Initialize(Cursor.Current.Handle);
            GameCursor.SetCursor(GameCursor.CursorType.Normal);

        }

        protected void MainWindow_Move(object sender, EventArgs args)
        {
            if (dlgLogin != null)
            {
                dlgLogin.Location = new System.Drawing.Point((this.Window.ClientBounds.Width - dlgLogin.Size.Width) / 2 + this.Window.ClientBounds.X,
                                                        (this.Window.ClientBounds.Height - dlgLogin.Size.Height) * 3 / 4 + this.Window.ClientBounds.Y);
            }
        }


        protected void MainWindow_MouseDown(object sender, MouseEventArgs args)
        {
           UIMgr.HandleMessage(UIMessage.MouseDown, args.X, args.Y);
        }

        protected void MainWindow_MouseClick(object sender, MouseEventArgs args)
        {
           int result = UIMgr.HandleMessage(UIMessage.MouseClick, args.X, args.Y);
           if (result != 0)
               return;

           if (!MainGame.IsEditorMode)
           {
               if (args.Button == MouseButtons.Left)
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
                               PreRenderEffect clickeffect = new PreRenderEffect("click", 128, 128);
                               clickeffect.Initialize(Content);
                               clickeffect.Layer = 4;
                               clickeffect.PlaySpeed = 3.0f;
                               clickeffect.Loop = false;
                               clickeffect.Position = new Vector2(CurrentScene.Viewport.X + args.X, CurrentScene.Viewport.Y + args.Y);
                               clickeffect.Play();
                               clickeffect.OnAnimationFini += new EventHandler(clickeffect_OnAnimationFini);
                               CurrentScene.AddRenderChunk(clickeffect);

                               //if (player.State == CharacterState.Idle)
                               //   player.State = CharacterState.Launch;
                               player.Target = new Vector2(CurrentScene.Viewport.X + args.X, CurrentScene.Viewport.Y + args.Y);
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
                                   //player.AddActionSetPre("Moving", CharacterState.Moving, -1, CharacterActionSetChangeFactor.ArriveTarget);
                                   //player.AddActionSet("Landing", CharacterState.Landing, -1, CharacterActionSetChangeFactor.AnimationCompleted);
                                   //player.AddActionSet("Idle", CharacterState.Idle, -1, CharacterActionSetChangeFactor.AnimationCompleted);
                               }


                               if (!player.Interacting)
                                   player.InteractiveTarget = null;
                           }
                           else if (sresult == 1)
                           {
                               //if (player.State == Character.CharacterState.Idle)
                               //   player.State = Character.CharacterState.Launch;
                               //player.Target = new Vector2(CurrentScene.Viewport.X + ms.X, CurrentScene.Viewport.Y + ms.Y);
                               //GameCursor.SetCursor(GameCursor.CursorType.Talk);
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
                               int x = 0;
                               x++;
                               Debug.WriteLine("too fast to click");
                           }
                       }
                   }
                   else if (CurrentScene.State == Scene.SceneState.Battle)
                   {
                       CurrentScene.ConfirmOperateTarget();
                   }
               }
           }
        }

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
                    pe.Trigger(ref p);
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

        protected void MainWindow_MouseWheel(object sender, MouseEventArgs args)
        {
            if (MainGame.IsEditorMode)
            {
                if (args.Delta != 0)
                    CurrentScene.EditorOperate(EditorOp.Scale, args.Delta, args.Delta);
            }
        }


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
                System.IO.Stream stream = TitleContainer.OpenStream("servers.xml");
                if (stream != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);
                    XmlNodeList constdef = doc.GetElementsByTagName("Server", "");
                    if (constdef.Count > 0)
                    {
                        for (int i = 0; i < constdef[0].ChildNodes.Count; ++i)
                        {
                            XmlNode node = constdef[0].ChildNodes[i];
                            if (node != null)
                            {
                                switch (node.Name)
                                {
                                    case "Ip":
                                        {
                                            ipaddress = (node.FirstChild.Value);
                                            break;
                                        }
                                    case "Port":
                                        {
                                            port = Convert.ToInt32(node.FirstChild.Value);
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
                clientchannel = TcpServer.CreateClient(ipaddress, port);
                clientchannel.SetPackage<ProjectXServer.Messages.HeadSizePackage>().ReceiveMessage = ReceiveMessage;
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

        ParticleEffect pe;
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //spritebatchrenderer = new SpriteBatchRenderer();
            spritebatchrenderer = new SpriteBatchRenderer
            {
                GraphicsDeviceService = this.graphics,
                Transformation = Matrix.CreateTranslation(0, 0, 1f)
            }; 

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


                        spawnEffect = new PreRenderEffect("spawn", 256, 256);
                        spawnEffect.Initialize(Content);
                        spawnEffect.Loop = false;
                        spawnEffect.Scene = CurrentScene;
                        spawnEffect.PlaySpeed = 2.0f;

                        //init character
                        Content.Load<CharacterDefinition.PicDef>(@"chardef/char3");
                        CurrentScene.LoadGameData();
                        CurrentScene.LoadBackground();


                        Texture2D texminimap = Content.Load<Texture2D>(@"minimap/scene1");
                        Texture2D texminimapchar = Content.Load<Texture2D>(@"minimap/charactericon");
                        Texture2D texmapmask = Content.Load<Texture2D>(@"minimap/mapmask");
                        CurrentScene.InitMiniMap(texminimap, texminimapchar, texmapmask, 0, GameConst.ScreenHeight - 256, 256, 256);

                        CharacterTitle.BlockTexture = Content.Load<Texture2D>(@"effect/block");
                        //UIMgr.AddUIControl("Dialog_Leader", "leader_dlg", (int)UILayout.Right, (int)UILayout.Top, 0, 0, -1, 99, this);
                        CurrentScene.GenerateClouds(cloudTextureArray);
                        CurrentScene.SortRenderChunksByLayer();

                        spritebatchrenderer.GraphicsDeviceService = this.graphics;
                        spritebatchrenderer.LoadContent(null);
                        pe = GameConst.Content.Load<ParticleEffect>(@"particles/magictrail");
                        for (int i = 0; i < pe.Emitters.Count; i++)
                            pe.Emitters[i].ParticleTexture = GameConst.Content.Load<Texture2D>(@"particles/" + pe.Emitters[i].ParticleTextureAssetPath);
                        for (int i = 0; i < pe.Emitters.Count; i++)
                            pe.Emitters[i].Initialise();
                        

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
                ProjectXServer.Messages.PlayerLoginSelfMsg msg = new ProjectXServer.Messages.PlayerLoginSelfMsg();
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

        private void DestoryPlayer(ProjectXServer.Messages.PlayerLogoutMsg pn)
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

        private void CreateLocalPlayer(ProjectXServer.Messages.PlayerLoginSelfMsg pn)
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
            player.AddActionSet("Idle", CharacterState.Spawn, CharacterActionSetChangeFactor.EffectCompleted, "Spawn");
            player.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.Immediate, null);
            player.AddPreRenderEffect("Spawn", spawnEffect);
            player.ClientID = pn.ClientID;
            CurrentScene.AddCharacter(player);
            CurrentScene.Player = player;
            player.TrailParticle = pe;
            player.UpdateSceneScroll();
            Control ctrl = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
            ctrl.Invoke(new Action(() =>
            {
                this.Window.Title = pn.Name;
            }));

        }

        private void CreatePlayer(ProjectXServer.Messages.PlayerLoginMsg pn)
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
            CurrentScene.Update(gameTime);
            if (pe != null)
            {
                spritebatchrenderer.Transformation = Matrix.CreateTranslation(-CurrentScene.Viewport.X, -CurrentScene.Viewport.Y, 0);
                pe.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
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
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            if (!ContentLoadCompleted)
                return;

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

            //idmatrix
            Vector3 p = new Vector3();
            spritebatchrenderer.RenderEffect(pe, ref idmatrix, ref idmatrix, ref idmatrix, ref p);
            base.Draw(gameTime);
        }

        MouseState _msLast = new MouseState();
        KeyboardState _ksLast = new KeyboardState();
        HiPerfTimer clicktimer = new HiPerfTimer();
        double clicktime = 0;
        private int _mousex;
        private int _mousey;
        private MouseButtons _mousebutton;
       
        public void UpdateInput()
        {
            if (player == null)
                return;
            Control ctrl = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
            if (Form.ActiveForm == null || !Form.ActiveForm.Equals(ctrl))
                return;

            KeyboardState ks = Keyboard.GetState();
            Vector4 vp = CurrentScene.Viewport;
            if (CurrentScene.State == Scene.SceneState.Map)
            {
                bool isscroll = false;
                if (MainGame.IsEditorMode)
                {
                    if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                    {
                        vp.Y += GameConst.ScrollSpeed;
                        isscroll = true;
                    }
                    if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                    {
                        vp.Y -= GameConst.ScrollSpeed;
                        isscroll = true;
                    }
                    if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                    {
                        vp.X -= GameConst.ScrollSpeed;
                        isscroll = true;
                    }
                    if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                    {
                        vp.X += GameConst.ScrollSpeed;
                        isscroll = true;
                    }
                    if (MainGame.IsEditorMode)
                    {
                        if (isscroll)
                        {
                            MouseEventArgs mea = new MouseEventArgs(_mousebutton, 0, _mousex, _mousey, 0);
                            MainWindow_MouseMove(this, mea);
                            CurrentScene.Viewport = vp;
                            Debug_ClipScroll();
                        }
                    }
                }
                if (ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.E) && _ksLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.E))
                {
                    if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
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
                }
                if (ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.S) && _ksLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                {
                    if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                    {
                        if (MainGame.IsEditorMode)
                        {
                            CurrentScene.EditorOperate(EditorOp.Save, 0, 0);
                        }
                    }
                }
            }
            else if (CurrentScene.State == Scene.SceneState.Battle)
            {
                if (!MainGame.IsEditorMode)
                {
                    if (ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.R) && _ksLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R))
                    {
                        CurrentScene.BattleRound(1);
                    }
                }
            }
            if (ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.S) && _ksLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                if (!MainGame.IsEditorMode)
                    CurrentScene.IntoBattle();
                //dialog.Position += new Vector2(4,0);
            }
            if (ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.D) && _ksLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                //CurrentScene.ReturnMap();
                if (MainGame.IsEditorMode)
                {
                    effects.NumberAnimation na = new effects.NumberAnimation(1789);
                    na.Position = new Vector2(200, 200);
                    na.Play(CurrentScene);
                }
            }

            /* if (ks.IsKeyUp(Keys.S))
             {
                 IAsyncResult result;
                 result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
                 storagedevice = StorageDevice.EndShowSelector(result);
                 result = storagedevice.BeginOpenContainer("mmodemo", null, null);
                 result.AsyncWaitHandle.WaitOne();
                 StorageContainer container = storagedevice.EndOpenContainer(result);
                 result.AsyncWaitHandle.Close();
                 string filename = "rt.png";
                 Stream file = container.OpenFile(filename, FileMode.OpenOrCreate);
                 CurrentScene.minimap.rendertarget.SaveAsPng(file, 256, 256);
                 file.Close();

                 // Dispose the container.

                 container.Dispose();
             }*/
           
            //mouse
            MouseState ms = Mouse.GetState();


            int msgr = 0;
            /*if (ms.X != _msLast.X || ms.Y != _msLast.Y)
            {
                msgr += UIMgr.HandleMessage(UIMessage.MouseMove, ms.X, ms.Y);
            }
            if (ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                msgr += UIMgr.HandleMessage(UIMessage.MouseDown, ms.X, ms.Y);
            }
            else if (ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                msgr += UIMgr.HandleMessage(UIMessage.MouseUp, ms.X, ms.Y);
                if (_msLast.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    msgr += UIMgr.HandleMessage(UIMessage.MouseClick, ms.X, ms.Y);
            }*/
            /*if (msgr == 0)
            {
                //if (CurrentScene.State == Scene.SceneState.Battle)
                if (!MainGame.IsEditorMode)
                {
                    CurrentScene.HighLightCharacterByPoint(ms.X, ms.Y);
                    if (ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && _msLast.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                    {
                        if (ms.X >= 0 && ms.X <= CurrentScene.Viewport.Z && ms.Y >= 0 && ms.Y <= CurrentScene.Viewport.W)
                        {
                            if (CurrentScene.State == Scene.SceneState.Map)
                            {

                                if (clicktimer == null)
                                {
                                    clicktimer = new HiPerfTimer();
                                    clicktimer.Start();
                                }

                                clicktime = clicktimer.GetTotalDuration();

                                if (clicktime > 0.3 && (player.State == CharacterState.Idle || player.State == CharacterState.Moving))
                                {
                                    clicktimer.Stop();
                                    clicktimer.Start();
                                    int result = CurrentScene.SelectCharacter();
                                    if (result == 0)
                                    {
                                        PreRenderEffect clickeffect = new PreRenderEffect("click", 128, 128);
                                        clickeffect.Initialize(Content);
                                        clickeffect.Layer = 4;
                                        clickeffect.PlaySpeed = 3.0f;
                                        clickeffect.Loop = false;
                                        clickeffect.Position = new Vector2(CurrentScene.Viewport.X + ms.X, CurrentScene.Viewport.Y + ms.Y);
                                        clickeffect.Play();
                                        clickeffect.OnAnimationFini += new EventHandler(clickeffect_OnAnimationFini);
                                        CurrentScene.AddRenderChunk(clickeffect);

                                        //if (player.State == CharacterState.Idle)
                                        //   player.State = CharacterState.Launch;
                                        player.Target = new Vector2(CurrentScene.Viewport.X + ms.X, CurrentScene.Viewport.Y + ms.Y);
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
                                            //player.AddActionSetPre("Moving", CharacterState.Moving, -1, CharacterActionSetChangeFactor.ArriveTarget);
                                            //player.AddActionSet("Landing", CharacterState.Landing, -1, CharacterActionSetChangeFactor.AnimationCompleted);
                                            //player.AddActionSet("Idle", CharacterState.Idle, -1, CharacterActionSetChangeFactor.AnimationCompleted);
                                        }


                                        if (!player.Interacting)
                                            player.InteractiveTarget = null;
                                    }
                                    else if (result == 1)
                                    {
                                        //if (player.State == Character.CharacterState.Idle)
                                        //   player.State = Character.CharacterState.Launch;
                                        //player.Target = new Vector2(CurrentScene.Viewport.X + ms.X, CurrentScene.Viewport.Y + ms.Y);
                                        //GameCursor.SetCursor(GameCursor.CursorType.Talk);
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
                                        int x = 0;
                                        x++;
                                        Debug.WriteLine("too fast to click");
                                    }
                                }
                            }
                        }
                        else if (CurrentScene.State == Scene.SceneState.Battle)
                        {
                            CurrentScene.ConfirmOperateTarget();
                        }
                    }
                }
            }*/
            //back input status
            _msLast = ms;
            _ksLast = ks;
        }

        protected void clickeffect_OnAnimationFini(object sender, EventArgs e)
        {
            PreRenderEffect effect = sender as PreRenderEffect;
            effect.OnAnimationFini -= new EventHandler(clickeffect_OnAnimationFini);
            effect.State = RenderChunk.RenderChunkState.Delete;
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
