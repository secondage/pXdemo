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
using demo.quest;

namespace demo
{
    [Flags]
    public enum GameAction
    {
        None = 0,
        Kill = 1,
    }
    public class Quest
    {
        private int id;
        private string name;
        private string talk;
        private string talk_completed;
        private string talk_noncompleted;
        private string ok_content;
        private string cancel_content;
        private Quest next;
        private QuestScript script;
        private string scriptname;


        static private Dictionary<string, Quest> queststorage = new Dictionary<string, Quest>();

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public QuestScript Script
        {
            get
            {
                return script;
            }
            set
            {
                script = value;
            }
        }

        public string ScriptName
        {
            get
            {
                return scriptname;
            }
            set
            {
                scriptname = value;
            }
        }
             

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public Quest NextQuest
        {
            get
            {
                return next;
            }
        }

        public String Next
        {
            get
            {
                return next.Name;
            }
            set
            {
                next = GetQuestFromStorageByName(value);
            }
        }

        public string Talk
        {
            get
            {
                return talk;
            }
            set
            {
                talk = value;
            }
        }

        public string Talk_Completed
        {
            get
            {
                return talk_completed;
            }
            set
            {
                talk_completed = value;
            }
        }

        public string Talk_Noncompleted
        {
            get
            {
                return talk_noncompleted;
            }
            set
            {
                talk_noncompleted = value;
            }
        }

        public string OKContent
        {
            get
            {
                return ok_content;
            }
            set
            {
                ok_content = value;
            }
        }

        public string CancelContent
        {
            get
            {
                return cancel_content;
            }
            set
            {
                cancel_content = value;
            }
        }

        static public Quest GetQuestFromStorageByName(string name)
        {
            if (queststorage.ContainsKey(name))
            {
                return queststorage[name];
            }
            else
            {
                try
                {
                    QuestDefinition.QuestDef qd = GameConst.Content.Load<QuestDefinition.QuestDef>(@"quest/" + name);
                    if (qd != null)
                    {
                        Quest q = new Quest(qd);
                        queststorage[name] = q;
                        return q;
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public Quest(Quest q)
        {
            id = q.ID;
            name = q.Name;
            talk = q.Talk;
            talk_completed = q.Talk_Completed;
            talk_noncompleted = q.Talk_Noncompleted;
            //script = Activator.CreateInstance(q.Script.GetType()) as QuestScript;
            Type type = Type.GetType("demo.quest." + q.scriptname);
            if (type != null)
            {
                QuestScript s = type.Assembly.CreateInstance("demo.quest." + q.scriptname) as QuestScript;
                if (s != null)
                {
                    s.Quest = this;
                    s.Initialize();
                    script = s;
                }
            }
            next = NextQuest;
        }

        public Quest(QuestDefinition.QuestDef qd)
        {
            id = qd.id;
            name = qd.name;
            talk = qd.talk;
            talk_completed = qd.talk_done;
            talk_noncompleted = qd.talk_notdone;
            ok_content = qd.btn_ok_content;
            cancel_content = qd.btn_cancel_content;
            Next = qd.next;
            /*Type type = Type.GetType("demo.quest." + qd.actionfunc);
            if (type != null)
            {
                QuestScript s = type.Assembly.CreateInstance("demo.quest." + qd.actionfunc) as QuestScript;
                if (s != null)
                {
                    s.Quest = this;
                    s.Initialize();
                    this.script = s;
                }
            }*/
            scriptname = qd.actionfunc;
        }
    }
}
