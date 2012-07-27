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

namespace EntityDefinition
{
    public class EntityDefinition
    {
        public string name;
        public string type;
        public int tmpid;
        public List<string> pics;
        public int hp;
        public int maxhp;
        public int atk;
        public int def;
        public int speed;
        public float size;
    }
}
