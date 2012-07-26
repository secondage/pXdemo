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
    public class UIButton : UIElement
    {
        private string text;

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public UIButton(Texture2D intex, int layer):
            base(null, layer)
        {

        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }

        public override void Render(SpriteBatch sb)
        {
            base.Render(sb);
        }

        public override void Initialize(UIControlTemplate.UIControlTemplate tmp)
        {
            base.Initialize(tmp);
        }
    }



}
