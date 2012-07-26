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


namespace demo
{
    public class MiniMap : RenderChunk, IDisposable
    {
        private List<Character> characters;
        private Texture2D backgroundtexture;
        private Texture2D charactertexture;
        private Texture2D masktexture;
        private Vector2 rendersize = new Vector2();
        public RenderTarget2D rendertarget;
        private SpriteBatch sb;
        private Effect maskEffect;
        private Texture2D overlaytexture;

        
        public override Vector2 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                try
                {
                    rendersize.X = size.X / (float)backgroundtexture.Width;
                    rendersize.Y = size.Y / (float)backgroundtexture.Height;
                }
                catch
                {
                    rendersize = Vector2.One;
                }
            }
        }

        public MiniMap() :
            base(null, 0)
        {

        }

        public List<Character> Characters
        {
            set
            {
                characters = value;
            }
        }

        public void Initialize(Texture2D btex, Texture2D ctex, Texture2D mtex, int w, int h)
        {
            backgroundtexture = btex;
            charactertexture = ctex;
            masktexture = mtex;
            size.X = w;
            size.Y = h;
            rendersize.X = (float)w / (float)backgroundtexture.Width;
            rendersize.Y = (float)h / (float)backgroundtexture.Height;
            rendertarget = new RenderTarget2D(GameConst.Graphics.GraphicsDevice, w, h, false, SurfaceFormat.Color, DepthFormat.None);
            sb = new SpriteBatch(GameConst.Graphics.GraphicsDevice);

            maskEffect = GameConst.Content.Load<Effect>(@"minimap/maskeffect");
            overlaytexture = GameConst.Content.Load<Texture2D>(@"minimap/mapmask2");
        }


        public void RenderPre()
        {
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            GameConst.Graphics.GraphicsDevice.SetRenderTarget(rendertarget);
            GameConst.Graphics.GraphicsDevice.Clear(Color.FromNonPremultiplied(0, 0, 0, 0));

            //GameConst.Graphics.GraphicsDevice.PresentationParameters = new PresentationParameters();
            GameConst.Graphics.GraphicsDevice.Textures[1] = masktexture;
            Vector2 s = new Vector2();
            Vector2 ts = new Vector2();
            Vector2 p = new Vector2();
            float scalex = 2.0f;
            s.X = (float)backgroundtexture.Width / (float)scene.ActualSize.Z;
            s.Y = (float)backgroundtexture.Height / (float)scene.ActualSize.W;
            ts.X = size.X * ((float)scene.ActualSize.Z / (float)GameConst.ScreenWidth);
            ts.Y = size.Y * ((float)scene.ActualSize.W / (float)GameConst.ScreenHeight);

            ts.X = ((float)scene.ActualSize.Z - (float)GameConst.ScreenWidth) / ((float)backgroundtexture.Width * scalex - size.X);
            ts.Y = ((float)scene.ActualSize.W - (float)GameConst.ScreenHeight) / ((float)backgroundtexture.Height * scalex - size.Y);
            //ts.X = scene.Viewport.X / (float)scene.ActualSize.Z

            //p.X -= scene.Viewport.X * s.X * (ts.X / backgroundtexture.Width) * scalex;
            //p.Y -= scene.Viewport.Y * s.Y * (ts.Y / backgroundtexture.Height) * scalex;
            p.X -= scene.Viewport.X / ts.X;
            p.Y -= scene.Viewport.Y / ts.Y;
            sb.Begin(SpriteSortMode.Immediate, null, null, null, null, maskEffect);
            sb.Draw(backgroundtexture, p, null, Color.White, 0.0f, Vector2.Zero, /*new Vector2(ts.X / backgroundtexture.Width, ts.Y / backgroundtexture.Height) * scalex*/Vector2.One * scalex, SpriteEffects.None, 0.0f);
            sb.End();
            //Vector2 p = new Vector2();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (Character ch in characters)
            {
                //ch.Position;
                p.X = ch.Position.X / scene.ActualSize.Z * size.X;
                p.Y = ch.Position.Y / scene.ActualSize.W * size.Y;
                //p += position;
                if (ch is Player)
                    sb.Draw(charactertexture, p, null, Color.Green, 0.0f, new Vector2(charactertexture.Width / 2, charactertexture.Height / 2), new Vector2(0.5f, 0.5f), SpriteEffects.None, 0.0f);
                else if (ch is Npc)
                {
                    if (ch.Picture.State == RenderChunkState.Hide || ch.Picture.State == RenderChunkState.Invisible)
                        continue;
                    Npc npc = ch as Npc;
                    if (npc.ExistQuest())
                        sb.Draw(charactertexture, p, null, Color.Yellow, 0.0f, new Vector2(charactertexture.Width / 2, charactertexture.Height / 2), new Vector2(0.5f, 0.5f), SpriteEffects.None, 0.0f);
                    else
                        sb.Draw(charactertexture, p, null, Color.LightSlateGray, 0.0f, new Vector2(charactertexture.Width / 2, charactertexture.Height / 2), new Vector2(0.5f, 0.5f), SpriteEffects.None, 0.0f);
                }

            }
            sb.End();
            //System.IO.Stream stream = TitleContainer.OpenStream("ship.png");
            //rendertarget.SaveAsPng(stream, 256, 256);

            GameConst.Graphics.GraphicsDevice.SetRenderTarget(null);
            GameConst.Graphics.GraphicsDevice.Clear(Color.Black);
            
        }

        public void RenderPost()
        {
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            sb.Draw(rendertarget, position, null, this.Color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            sb.Draw(overlaytexture, position, null, this.Color, 0.0f, Vector2.Zero, rendersize, SpriteEffects.None, 0.0f);
            sb.End();
        }

        public void Dispose()
        {
            rendertarget.Dispose();
            sb.Dispose();
            rendertarget = null;
        }
    }
}
