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
    class UIDialog : UIElement
    {
        public UIDialog()
        {

        }

             
        public override void Initialize(UIControlTemplate.UIControlTemplate tmp)
        {
            base.Initialize(tmp);
        }

        public override int HandleMessage(UIMessage msg, object p1, object p2)
        {
            if (state != RenderChunkState.Show)
                return 0;
            return base.HandleMessage(msg, p1, p2);
        }
    }
}
