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

namespace demo
{
    public class Npc : Character
    {
        Dictionary<int, Quest> quests = new  Dictionary<int, Quest>();

        public Npc(string n, Scene s) :
            base(n, s)
        {
        }

        public Npc() :
            base()
        {
        }


        public bool ExistQuest()
        {
            return quests.Count > 0;
        }

        public bool ExistTakeableQuest(Player player)
        {
            foreach (KeyValuePair<int, Quest> q in quests)
            {
                if (!player.CompletedQuests.Contains(q.Key))
                    return true;
            }
            return false;
        }

        public bool ExistQuest(int id)
        {
            return quests.ContainsKey(id);
        }

        public Quest GetFirstQuest(Player player)
        {
            foreach (KeyValuePair<int, Quest> q in quests)
            {
                if (!player.CompletedQuests.Contains(q.Key))
                    return q.Value; ;
            }
            return null;
        }


        public void AddQuest(string name)
        {
            Quest quest = Quest.GetQuestFromStorageByName(name);
            if (quest != null)
                quests.Add(quest.ID, quest);
        }

        protected override void UpdateMovement(GameTime gametime)
        {
            throw new System.NotImplementedException();
        }
    }
}
