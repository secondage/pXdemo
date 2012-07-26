using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace demo.quest
{
    class Quest_quest3 : QuestScript
    {
        private int killcount;
        private int maxkill;

    

        public override int GetNumActions()
        {
            return 1;
        }
        public override void Initialize()
        {
            killcount = 0;
            maxkill = 1;
        }
        public override string GetTrackString(int idx)
        {
            if (killcount < maxkill)
                return string.Format("去杀死邪神吧", killcount, maxkill);
            else
                return string.Format("已完成");

        }
        public override bool GetAccomplished()
        {
            return killcount == maxkill;
        }
        public override bool GetTakeable()
        {
            return true;
        }
        public override bool Update(GameAction action, object p1, object p2)
        {
            if ((int)p1 == 3)
            {
                killcount += (int)p2;
                return killcount <= maxkill;
            }
            return false;
        }
        public override void Takeup()
        {
            Npc npc = player.Scene.GetCharacterByName("boss") as Npc;
            if (npc != null)
            {
                npc.Picture.State = RenderChunk.RenderChunkState.FadeIn;
                npc.Title.State = RenderChunk.RenderChunkState.FadeIn;
            }
        }
        public override bool Delivery()
        {
            Npc npc = player.Scene.GetCharacterByName("boss") as Npc;
            if (npc != null)
            {
                npc.Picture.State = RenderChunk.RenderChunkState.Invisible;
                npc.Title.State = RenderChunk.RenderChunkState.Invisible;
            }
            return true;
        }
    }
}
