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
    class HoverStone : RenderChunk
    {
        private float amplitude;
        private float frequency;
        private Vector2 originPos;
   
        public float AMP
        {
            get
            {
                return amplitude;
            }
            set
            {
                amplitude = value;
            }
        }

        public float TimeOfCycle
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
            }
        }

        public HoverStone() :
            base(null, 0)
        {

        }

        public string TextureFileName { get; set; }

        public HoverStone(Texture2D intexture, int inlayer) :
                    base(intexture, inlayer)
        {

        }

        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                originPos = value;
            }
        }

        public override void Update(GameTime gametime)
        {
            position.Y = (float)Math.Sin(gametime.TotalGameTime.TotalSeconds * frequency) * amplitude + originPos.Y;
            base.Update(gametime);
        }

        public override void Render(SpriteBatch sb)
        {
            //Rectangle dest = new Rectangle(Position.X, Position.Y, Size.X, Size.Y);
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
           
            Vector2 pos = Position;
            pos.X -= Scene.Viewport.X;
            pos.Y -= Scene.Viewport.Y;

            Rectangle n = new Rectangle((int)Position.X, (int)Position.Y, (int)(texture.Width * Size.X), (int)(texture.Height * Size.Y));
            Rectangle p = new Rectangle((int)scene.Viewport.X, (int)scene.Viewport.Y, (int)scene.Viewport.Z, (int)scene.Viewport.W);
            if (!n.Intersects(p))
                return;

            sb.Draw(texture, pos, null, HighLight ? Color.Red : this.Color, 0.0f, TextureSize * 0.5f, Size, SpriteEffects.None, 0.0f);

            base.Render(sb);
        }
    }
}
