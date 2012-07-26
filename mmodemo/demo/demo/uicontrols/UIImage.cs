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


namespace demo.uicontrols
{
    class UIImage : UIElement
    {
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }
        public override void Render(SpriteBatch sb)
        {
            base.Render(sb);
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            Vector2 pos = Position;

            sb.Draw(Texture, pos, srcrect, this.Color, 0, Vector2.Zero, Size, SpriteEffects.None, 0.0f);
        }
    }
}
