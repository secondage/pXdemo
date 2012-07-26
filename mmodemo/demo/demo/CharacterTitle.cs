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
    public class CharacterTitle : RenderChunk
    {
        public enum IconTypes
        {
            None,
            TakeQuest,
            QuestCompleted,
            QuestNonCompleted,
        }


        private SpriteFont font;
        private string namestring;
        private Vector2 namestringmeasure;
        private Character character;
        static private Texture2D blocktexture;
        static private Texture2D takequesttexture;
        static private Texture2D donequesttexture;
        static private Texture2D notdonequesttexture;
        private IconTypes icontype = IconTypes.None;

        static public Texture2D TakeQuestTexture
        {
            set
            {
                takequesttexture = value;
            }
        }
        static public Texture2D QuestCompletedTexture
        {
            set
            {
                donequesttexture = value;
            }
        }
        static public Texture2D QuestNonCompletedTexture
        {
            set
            {
                notdonequesttexture = value;
            }
        }

        public IconTypes IconType
        {
            get
            {
                return icontype;
            }
            set
            {
                icontype = value;
            }
        }

        public SpriteFont Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
            }
        }

        public string NameString
        {
            set
            {
                namestring = value;
                namestringmeasure = font.MeasureString(namestring);
            }
        }

        static public Texture2D BlockTexture
        {
            set
            {
                blocktexture = value;
            }
        }

        public Vector2 NameStringMeasure
        {
            get
            {
                return namestringmeasure;
            }
        }

        public Character Character
        {
            set
            {
                character = value;
            }
        }

        public CharacterTitle(SpriteFont _font) :
            base(null, 0)
        {
            if (_font == null)
                throw new NullReferenceException(ToString());
            font = _font;
           
        }

        double _time;
        public override void Update(GameTime gametime)
        {
            _time = gametime.TotalGameTime.TotalSeconds;
            base.Update(gametime);
        }

        Color color1 = new Color();
        Color color2 = new Color();
        public override void Render(SpriteBatch sb)
        {
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            Vector2 pos = new Vector2(Position.X, Position.Y);
            pos.X -= Scene.Viewport.X;
            pos.Y -= Scene.Viewport.Y;
            
            pos.X -= namestringmeasure.X * 0.5f;
            pos.Y -= namestringmeasure.Y * 0.5f;
            //sb.End();
            //sb.Begin();
            sb.DrawString(font, namestring, pos, this.Color);
            //sb.End();
            //sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            Vector2 poshp = new Vector2(pos.X, pos.Y + 30.0f);
           
            color1 = Color.Yellow;
            color1.A = this.Color.A;
            sb.Draw(blocktexture, poshp, null, color1, 0.0f, Vector2.Zero, new Vector2(30.0f, 3.0f), SpriteEffects.None, 0.0f);
            poshp.X += 2.0f;
            poshp.Y += 1.0f;
            
            color2 = Color.Red;
            color2.A = this.Color.A;
            float s = (float)character.HP / (float)character.MaxHP;
            sb.Draw(blocktexture, poshp, null, color2, 0.0f, Vector2.Zero, new Vector2(28.0f * s, 2.0f), SpriteEffects.None, 0.0f);

            Texture2D hot = null;
            switch (icontype)
            {
                case IconTypes.TakeQuest:
                    hot = takequesttexture;
                    break;
                case IconTypes.QuestCompleted:
                    hot = donequesttexture;
                    break;
                case IconTypes.QuestNonCompleted:
                    hot = notdonequesttexture;
                    break;
            }
            if (hot != null)
            {
                Vector2 posicon = new Vector2(Position.X, Position.Y);
                posicon.X -= Scene.Viewport.X;
                posicon.Y -= Scene.Viewport.Y;
                posicon.Y -= (character.Picture.FrameSize.Y * 0.25f + hot.Height / 2) ;
                posicon.X -= hot.Width / 2;

                posicon.Y += (float)Math.Sin(_time * 5.0) * 3.2f;
            
                sb.Draw(hot, posicon, null, this.Color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
            }

            base.Render(sb);
        }
    }
}
