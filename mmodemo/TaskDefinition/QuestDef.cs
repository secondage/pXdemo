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

namespace QuestDefinition
{
   
    public class QuestDef
    {
        public int id;
        public string name;
        public string talk;
        public string talk_done;
        public string talk_notdone;
        public string btn_ok_content;
        public string btn_cancel_content;
        public string next;
        public string actionfunc;
    }
}
