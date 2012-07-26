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
    
    class UITextBlock : UIElement
    {
        private string text;
        private float linespace = 2.0f;
        private List<string> textline = new List<string>();

        

        public static void WrapText(ref List<string> lines, string text, SpriteFont font, int width)
        {
            lines.Clear();
            if (text != null)
            {
                Vector2 m = font.MeasureString(text);
                string rendertext;
                rendertext = "";
                for (int i = 0; i < text.Length; ++i)
                {
                    rendertext += text[i];
                    if (GameConst.CurrentFont.MeasureString(rendertext).X >= width)
                    {
                        if (rendertext.Length == 1)
                            throw new Exception();
                        rendertext = rendertext.Remove(rendertext.Length - 1);
                        lines.Add(rendertext);
                        i--;
                        rendertext = "";
                    }


                }
                lines.Add(rendertext);
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                sizedirty = true;
                linespace = GameConst.CurrentFont.LineSpacing;
            }
        }

        protected override void CalculateSize()
        {
            base.CalculateSize();
            WrapText(ref textline, text, GameConst.CurrentFont, controlrect.Width);
        }


        public override void Render(SpriteBatch sb)
        {
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            Vector2 pos = new Vector2();
            pos.X = controlrect.X;
            pos.Y = controlrect.Y;
            foreach (string t in textline)
            {
                Color color = fontcolor;
                color.A = this.Color.A;
                sb.End();
                sb.Begin();
                sb.DrawString(GameConst.CurrentFont, t, pos, color);
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                pos.Y += linespace;
            }
        }
    }
}
