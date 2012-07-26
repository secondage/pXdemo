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
    class Cloud : RenderChunk
    {
        public Cloud(Texture2D intexture, int inlayer) :
                    base(intexture, inlayer)
        {

        }

        public override void Update(GameTime gametime)
        {
            //move
            position += (float)gametime.ElapsedGameTime.TotalSeconds * speed * scene.Wind;
            if (position.X > scene.ActualSize.Z)
            {
                position.X = -ActualSize.X - 10.0f;
            }
            base.Update(gametime);
        }

        public override void Render(SpriteBatch sb)
        {
            //Rectangle dest = new Rectangle(Position.X, Position.Y, Size.X, Size.Y);
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            SpriteEffects se = new SpriteEffects();

            Vector2 pos = Position;
            pos.X -= Scene.Viewport.X;
            pos.Y -= Scene.Viewport.Y;

            Rectangle n = new Rectangle((int)Position.X, (int)Position.Y, (int)(texture.Width * size.X), (int)(texture.Height * size.Y));
            Rectangle p = new Rectangle((int)scene.Viewport.X, (int)scene.Viewport.Y, (int)scene.Viewport.Z, (int)scene.Viewport.W);
            if (!n.Intersects(p))
                return;

            sb.Draw(texture, pos, null, this.Color, 0.0f, Vector2.Zero, size, se, 0.0f);

            base.Render(sb);
        }
    }
}
