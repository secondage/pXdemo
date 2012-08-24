using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace demo.effects
{
    public class NumberAnimation : RenderChunk
    {
        private int maxbit = 0; //最大位数
        private int tvalue = 0;
        private PreRenderEffect[] numbereffects;
        private Color numcolor;
        
        public NumberAnimation(int value)
        {
            Value = value;
        }

        public override Color Color
        {
            get
            {
                return numcolor;
            }
            set
            {
                numcolor = value;
                for (int i = 0; i < maxbit; ++i)
                {
                    numbereffects[i].Color = numcolor;
                }
            }
        }

        public int Value {
            get
            {
                return tvalue;
            }
            set
            {
                tvalue = value;
                char[] ns = tvalue.ToString().ToCharArray();
                maxbit = ns.Length;
                Initialize(ns);
            }
        }

        private void Initialize(char[] ns)
        {
            numbereffects = new PreRenderEffect[maxbit];
            for (int i = 0; i < maxbit; ++i)
            {
                numbereffects[i] = new PreRenderEffect("number", 64, 64);
                numbereffects[i].Initialize(GameConst.Content);
                numbereffects[i].PlaySpeed = 0.0f;
                numbereffects[i].Color = new Microsoft.Xna.Framework.Color(1.0f, 0.0f, 0.0f);
                numbereffects[i].Frame = Convert.ToInt32(ns[i].ToString());
            }
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
                float s = value.X - ((float)maxbit * 0.5f - 0.5f) * numbereffects[0].FrameSize.X * 0.5f;
                for (int i = 0; i < maxbit; ++i)
                {
                    numbereffects[i].Position = new Vector2(i * numbereffects[0].FrameSize.X * 0.5f + s, value.Y);
                }
            }
        }

        public void Play(Scene scene)
        {
            for (int i = 0; i < maxbit; ++i)
            {
                numbereffects[i].Layer = 8000;
                numbereffects[i].State = RenderChunkState.FadeOutToDel;
                scene.AddRenderChunk(numbereffects[i]);
            }
            this.State = RenderChunkState.FadeOutToDel;
            scene.AddRenderChunk(this);

            scene.SortRenderChunksByLayer();
        }

        public override void Update(GameTime gametime)
        {
            for (int i = 0; i < maxbit; ++i)
            {
               numbereffects[i].Position = new Vector2(numbereffects[i].Position.X, numbereffects[i].Position.Y - (float)gametime.ElapsedGameTime.TotalSeconds * 200.0f);
            }
            base.Update(gametime);
        }

        public override void Render(SpriteBatch sb)
        {
            base.Render(sb);
        }
    }
}
