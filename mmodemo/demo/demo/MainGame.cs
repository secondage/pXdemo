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

namespace demo
{


    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
       

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Scene CurrentScene;
        private Player player;
        private PreRenderEffect spawnEffect;
        private SpriteFont mainfont;

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
#if WINDOWS
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = false;
            this.Window.Title = "mmodemo";

#endif
            GameCursor.Initialize(Cursor.Current.Handle);
            GameCursor.SetCursor(GameCursor.CursorType.Normal);

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
            CurrentScene.ActualSize = new Vector4(0, 0, 2048 * GameConst.BackgroundScale, 2048 * GameConst.BackgroundScale);
            player = new Player("player1", CurrentScene);
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

            GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, 1024, 768);

            mainfont = Content.Load<SpriteFont>(@"font/YaHeiCh16");
            GameConst.CurrentFont = mainfont;

            UIMgr.ControlsTexture = Content.Load<Texture2D>(@"ui/controls");

            CharacterTitle.TakeQuestTexture = Content.Load<Texture2D>(@"questicon/take");
            CharacterTitle.QuestCompletedTexture = Content.Load<Texture2D>(@"questicon/done");
            CharacterTitle.QuestNonCompletedTexture = Content.Load<Texture2D>(@"questicon/notdone");

            // add backgroud
            /*Texture2D texbg0 = Content.Load<Texture2D>(@"background/yun_a0");
            Texture2D texbg1 = Content.Load<Texture2D>(@"background/yun_a1");
            Texture2D texbg2 = Content.Load<Texture2D>(@"background/yun_a2");
            Texture2D texbg3 = Content.Load<Texture2D>(@"background/yun_b1");

            Background bg = new Background(texbg0, 0);
            bg.Size = new Vector2(GameConst.BackgroundScale, GameConst.BackgroundScale);
            CurrentScene.AddRenderChunk(bg);
            bg = new Background(texbg1, 1);
            bg.Size = new Vector2(GameConst.BackgroundScale, GameConst.BackgroundScale);
            CurrentScene.AddRenderChunk(bg);
            bg = new Background(texbg2, 2);
            bg.Size = new Vector2(GameConst.BackgroundScale, GameConst.BackgroundScale);
            CurrentScene.AddRenderChunk(bg);
            bg = new Background(texbg3, 3);
            bg.Size = new Vector2(GameConst.BackgroundScale, GameConst.BackgroundScale);
            CurrentScene.AddRenderChunk(bg);*/
            CurrentScene.LoadBackground();

            // load cloud texture
            for (int i = 0; i < 15; ++i)
            {
                cloudTextureArray[i] = Content.Load<Texture2D>(@"cloud/yun_b" + string.Format("{0:d2}", i));
            }

            CurrentScene.GenerateClouds(cloudTextureArray);

            spawnEffect = new PreRenderEffect("spawn", 256, 256);
            spawnEffect.Initialize(Content);
            spawnEffect.Loop = false;
            spawnEffect.Scene = CurrentScene;
            spawnEffect.PlaySpeed = 2.0f;

            //init character
            CharacterDefinition.PicDef pd = Content.Load<CharacterDefinition.PicDef>(@"chardef/char3");
            //CharacterPic cpic = new CharacterPic(texchar1, 15, new Vector2(150, 150));
            CharacterPic cpic = new CharacterPic(pd, 15);
            //CharacterDefinition.PicDef pd1 = Content.Load<CharacterDefinition.PicDef>(@"chardef/boss1_2");
            //cpic.AddCharacterDefinition(pd1);
            player.Picture = cpic;
            CharacterTitle title = new CharacterTitle(GameConst.CurrentFont);
            title.Layer = 15;
            title.NameString = "player1";
            title.Character = player;
            player.Title = title;
            player.Position = new Vector2(1024 / 2, 768 / 2);
            player.Speed = 350.0f;


            player.AddActionSet("Idle", CharacterState.Spawn, CharacterActionSetChangeFactor.EffectCompleted, "Spawn");
            player.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.Immediate, null);
            player.AddPreRenderEffect("Spawn", spawnEffect);
            CurrentScene.AddCharacter(player);
            CurrentScene.Player = player;


            CurrentScene.LoadGameData();
            CurrentScene.SortRenderChunksByLayer();
            player.Speed = GameConst.PlayerSpeed;

            Texture2D texminimap = Content.Load<Texture2D>(@"minimap/scene1");
            Texture2D texminimapchar = Content.Load<Texture2D>(@"minimap/charactericon");
            Texture2D texmapmask = Content.Load<Texture2D>(@"minimap/mapmask");
            CurrentScene.InitMiniMap(texminimap, texminimapchar, texmapmask, 0, GameConst.ScreenHeight - 256, 256, 256);

            CharacterTitle.BlockTexture = Content.Load<Texture2D>(@"effect/block");

            //UIMgr.AddLeaderDialog((int)UIMgr.UILayout.Right, (int)UIMgr.UILayout.Top, -1, 99, null);

            UIMgr.AddUIControl("Dialog_Leader", "leader_dlg", (int)UILayout.Right, (int)UILayout.Top, 0, 0, -1, 99, this);

            UITextBlock text = UIMgr.CreateUIControl("UITextBlock") as UITextBlock;
            if (text != null)
            {
                text.SourceRect = new Rectangle(0, 0, 100, 100);
                text.Text = @"ÄãÄãÄãÄãÄãÄãÄãÄãÄã44444444444ÄãÄãÄãÄãÄãÄã ÄãÄãÄãÄã333ÄãÄãÄãÄãÄãÄãÄãÄãÄãÄã3111ÄãÄãÄãÄãÄãÄãÄãÄãÄãÄãÄãÄãÄã";
                text.SourceRect = new Rectangle(0, 0, 200, 100);
                //text.Size = new Vector2(1, 1);
                //UIMgr.AddUIControl(text, "testtext", 200, 200, 0, 0, -1, 99, this);
            }

            
            //GameCursor.SetCursor(GameCursor.CursorType.Normal);

            /*
                        dialog = UIMgr.AddUIControl("Dialog_Npc", "dialognpc", (int)UILayout.Center, (int)UILayout.Bottom, 0, 0, -1, 99, this) as UIDialog;
                        if (dialog != null)
                        {
                            UITextButton btn = UIMgr.CreateUIControl("UITextButton") as UITextButton;
                            if (btn != null)
                            {
                                btn.Text = "ÄãÄãÄãÄãÄãÄãÄãÄãÄã444444";
                                dialog.AddUIControl(btn, "testbtn", 20, 200, 300, 20, -1, this);
                            }
                            UIImage npcface1 = UIMgr.CreateUIControl("UIImage") as UIImage;
                            if (npcface1 != null)
                            {
                                npcface1.Texture = Content.Load<Texture2D>(@"npcface/NPC01");
                                dialog.AddUIControl(npcface1, "testimage", 0, -npcface1.Texture.Height, 0, 0, -1, this);
                            }
                        }
            */
            // TODO: use this.Content to load your game content here
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

            UpdateInput();
            CurrentScene.Update(gameTime);
            UIMgr.Update(gameTime);
            //player.Update(gameTime);


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param _name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

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
            spriteBatch.DrawString(mainfont, string.Format("{0:d}, {1:d} {2:d}", (int)player.Position.X, (int)player.Position.Y, GameConst.RenderCountPerFrame), Vector2.Zero, Color.Red);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        MouseState _msLast = new MouseState();
        KeyboardState _ksLast = new KeyboardState();
        public void UpdateInput()
        {
            KeyboardState ks = Keyboard.GetState();
            Vector4 vp = CurrentScene.Viewport;
            if (CurrentScene.State == Scene.SceneState.Map)
            {
                if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                {
                    vp.Y += GameConst.ScrollSpeed;
                }
                if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                {
                    vp.Y -= GameConst.ScrollSpeed;
                }
                if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    vp.X -= GameConst.ScrollSpeed;
                }
                if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    vp.X += GameConst.ScrollSpeed;
                }
            }
            else if (CurrentScene.State == Scene.SceneState.Battle)
            {
                if (ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.R) && _ksLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R))
                {
                    CurrentScene.BattleRound(1);
                }
            }
            if (ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.S) && _ksLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                CurrentScene.IntoBattle();
                //dialog.Position += new Vector2(4,0);
            }
            if (ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.D) && _ksLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                CurrentScene.ReturnMap();
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
            CurrentScene.Viewport = vp;
            Debug_ClipScroll();
            //mouse
            MouseState ms = Mouse.GetState();


            int msgr = 0;
            if (ms.X != _msLast.X || ms.Y != _msLast.Y)
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
            }

            if (msgr == 0)
            {

                //if (CurrentScene.State == Scene.SceneState.Battle)
                CurrentScene.HighLightCharacterByPoint(ms.X, ms.Y);
                if (ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && _msLast.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                {
                    if (ms.X >= 0 && ms.X <= CurrentScene.Viewport.Z && ms.Y >= 0 && ms.Y <= CurrentScene.Viewport.W)
                    {
                        //
                        if (CurrentScene.State == Scene.SceneState.Map)
                        {

                            if (player.State == CharacterState.Idle || player.State == CharacterState.Moving)
                            {
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
                                        player.AddActionSet("Idle", CharacterState.Idle, CharacterActionSetChangeFactor.AnimationCompleted, null);
                                    }
                                    else
                                    {
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
            vp.X = MathHelper.Clamp(vp.X, 0.0f, (GameConst.BackgroundScale) * 2048.0f - GameConst.ScreenWidth);
            vp.Y = MathHelper.Clamp(vp.Y, 0.0f, (GameConst.BackgroundScale) * 2048.0f - GameConst.ScreenHeight);
            CurrentScene.Viewport = vp;
        }
    }
}
