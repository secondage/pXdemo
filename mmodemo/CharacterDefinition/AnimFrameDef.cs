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

namespace CharacterDefinition
{
    public class PicDef
    {
        public int width;
        public int height;
        public string texture;
        public List<AnimDef> anims;
    }
    public class AnimDef
    {
        public string name;
        public int framecount;
        public int eventframe;
        public float speed;
        public bool loop;
        public List<AnimFrameDef> frames;
    }
    public class AnimFrameDef
    {
        public int x;
        public int y;
        public int w;
        public int h;
        public string center;
    }
}
