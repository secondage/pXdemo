using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace demo.quest
{
    class Quest_quest2 : QuestScript
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
            maxkill = 2;
        }
        public override string GetTrackString(int idx)
        {
            if (killcount < maxkill)
                return string.Format("已杀死的妖军 : {0:d} / {1:d}", killcount, maxkill);
            else
                return string.Format("已完成");

        }
        public override bool GetAccomplished()
        {
            return killcount >= maxkill;
        }
        public override bool GetTakeable()
        {
            return true;
        }
        public override bool Update(GameAction action, object p1, object p2)
        {
            if ((int)p1 == 2)
            {
                killcount += (int)p2;
                return killcount <= maxkill;
            }
            return false;
        }
        public override void Takeup()
        {
        }
        public override bool Delivery()
        {
            return true;
        }
    }
}
