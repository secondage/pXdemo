using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace demo.quest
{
    public abstract class  QuestScript
    {
        protected Quest quest;
        protected Player player;

        public Quest Quest
        {
            get
            {
                return quest;
            }
            set
            {
                quest = value;
            }
        }

        public Player Player
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
            }
        }

        public abstract bool Update(GameAction action, object p1, object p2);
        public abstract void Initialize();
        public abstract int GetNumActions();
        public abstract string GetTrackString(int idx);
        public abstract bool GetAccomplished();
        public abstract bool GetTakeable();
        public abstract bool Delivery();
        public abstract void Takeup();
    }
}
