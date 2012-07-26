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

namespace UIControlTemplate
{
    public class UIControlStateRect
    {
        public string name;
        public int x;
        public int y;
        public int w;
        public int h;
    }
    public class UIControlTemplate
    {
        public string type;
        public string texture;
        public List<UIControlStateRect> rects;
        public List<UIControlTemplate> childs;
    }
}
