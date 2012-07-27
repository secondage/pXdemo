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
    public enum CharacterState
    {
        Spawn,
        Idle,
        Moving,
        Launch,
        Landing,
        Attack,
        Attack2,
        Attack3,
        BeAttack,
        Return,
        Dead,
        Dying,  //垂死
    };
    public enum CharacterActionSetChangeFactor
    {
        AnimationCompleted,
        ArriveTarget,
        ArriveAttackTarget,
        ArriveInteractiveTarget,
        EffectCompleted,
        Immediate,
        Time,
    };
    public class CharacterActionSet
    {
        public CharacterActionSet(string n, CharacterState s, CharacterActionSetChangeFactor f, object o)
        {
            animname = n;
            state = s;
            switch (f)
            {
                case CharacterActionSetChangeFactor.AnimationCompleted:
                    break;
                case CharacterActionSetChangeFactor.ArriveTarget:
                    target = (Vector2)o;
                    break;
                case CharacterActionSetChangeFactor.Time:
                    duration = (double)o;
                    break;
                case CharacterActionSetChangeFactor.ArriveAttackTarget:
                    interactive = (Character)o;
                    break;
                case CharacterActionSetChangeFactor.ArriveInteractiveTarget:
                    interactive = (Character)o;
                    break;
                case CharacterActionSetChangeFactor.EffectCompleted:
                    effectname = (string)o;
                    break;
            }
            factor = f;
         
        }
        public string animname;
        public CharacterState state;
        public double duration;
        public CharacterActionSetChangeFactor factor;
        public Vector2 target = new Vector2();
        public Character interactive;
        public string effectname;
    };
    public class Character
    {
        public enum DirMethod
        {
            AutoDectect = 0,
            Fixed,
        }


        public enum OperateType
        {
            None,
            Attack,
            Magic,
            Item,
        };

        public event EventHandler OnActionCompleted;
        public event EventHandler OnArrived;
        //public event EventHandler OnActionSetsOver;

        protected int templateid;

        protected int hp = 100;
        protected int maxhp = 100;
        protected int atk = 10;
        protected int def = 10;

        protected CharacterPic pic;
        protected CharacterTitle title;
        protected Vector2 position = new Vector2();
        protected Vector2 positionbackup = new Vector2();
        protected string name;
        protected Vector2 target;
        protected float speed = 1.0f;
        protected CharacterState state = CharacterState.Idle;
        protected Scene scene;
        protected Character attacktarget;
        protected OperateType op = OperateType.None;   //回合操作 
        protected Character optarget; //回合操作对象
        protected Character interactivetarget;
        protected Vector2 fixedfacedir = new Vector2(1, 0);
        protected DirMethod facedirmethod = DirMethod.AutoDectect;

        private List<CharacterActionSet> actionsets = new List<CharacterActionSet>();
        private CharacterActionSet currentactionset = null;

        protected Dictionary<string, PreRenderEffect> effects = new Dictionary<string, PreRenderEffect>();

        public Character(string n, Scene s)
        {
            name = n;
            scene = s;
        }

        public Character()
        {
        }

        public int TemplateID
        {
            get
            {
                return templateid;
            }
            set
            {
                templateid = value;
            }
        }

        public int Layer
        {
            get
            {
                return pic != null ? pic.Layer : -1;
            }
            set
            {
                if (pic != null)
                    pic.Layer = value;
                if (title != null)
                    title.Layer = value + 1;
            }
        }

        public virtual bool NeedTitle
        {
            get
            {
                return true;
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

        public int HP
        {
            get
            {
                return hp;
            }
            set
            {
                hp = value;
            }
        }

        public int MaxHP
        {
            get
            {
                return maxhp;
            }
            set
            {
                maxhp = value;
            }
        }

        public int ATK
        {
            get
            {
                return atk;
            }
            set
            {
                atk = value;
            }
        }

        public int DEF
        {
            get
            {
                return def;
            }
            set
            {
                def = value;
            }
        }

        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        public CharacterPic Picture
        {
            get
            {
                return pic;
            }
            set
            {
                pic = value;
                if (scene != null)
                {
                    scene.AddRenderChunk(pic);
                }
            }
        }
        public CharacterTitle Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                if (scene != null)
                {
                    scene.AddRenderChunk(title);
                }
                title.Position = position - new Vector2(0, pic.FrameSize.Y * pic.Size.Y * 0.5f) - GameConst.NameTitleOffset;
            }
        }

        public Vector2 FixedDir
        {
            get
            {
                return fixedfacedir;
            }
            set
            {
                fixedfacedir = value;
            }
        }

        public DirMethod FaceDirMethod
        {
            get
            {
                return facedirmethod;
            }
            set
            {
                facedirmethod = value;
            }
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                pic.Position = position;// -pic.FrameSize * 0.5f;
            }
        }

        public OperateType Operate
        {
            get
            {
                return op;
            }
            set
            {
                op = value;
            }
        }

        public Character OperateTarget
        {
            get
            {
                return optarget;
            }
            set
            {
                optarget = value;
            }
        }

        public Vector2 Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
                pic.Direction = target - position;
            }
        }

        public Character AttackTarget
        {
            get
            {
                return attacktarget;
            }
            set
            {
                attacktarget = value;
                if (attacktarget != null)
                {
                    if (pic.Direction.X < 0.0f)
                        Target = attacktarget.Picture.Position + new Vector2(attacktarget.Picture.FrameSize.X * 0.5f, 0.0f);
                    else
                        Target = attacktarget.Picture.Position - new Vector2(attacktarget.Picture.FrameSize.X * 0.5f, 0.0f);
                }

            }
        }

        public Character InteractiveTarget
        {
            get
            {
                return interactivetarget;
            }
            set
            {
                interactivetarget = value;
                if (interactivetarget != null)
                {
                    if (position.X < interactivetarget.position.X)
                    {
                        Target = interactivetarget.Picture.Position - new Vector2(interactivetarget.Picture.FrameSize.X * 1.1f, 0.0f);
                    }
                    else
                    {
                        Target = interactivetarget.Picture.Position + new Vector2(interactivetarget.Picture.FrameSize.X * 1.1f, 0.0f);
                    }
                    /*
                    if (pic.Direction.X < 0.0f)
                        Target = interactivetarget.Picture.Position + new Vector2(interactivetarget.Picture.FrameSize.X * 0.8f, 0.0f);
                    else
                        Target = interactivetarget.Picture.Position - new Vector2(interactivetarget.Picture.FrameSize.X * 0.8f, 0.0f);*/
                }

            }
        }

        public Scene Scene
        {
            get
            {
                return scene;
            }
            set
            {
                scene = value;
            }
        }




        public CharacterState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                switch (state)
                {
                    case CharacterState.Launch:
                        {
                            //pic.SetCurrentAnimationByName("Launch");
                            //pic.CurrentAnimation.OnAnimationFini += new EventHandler(OnPicAnimationFini);
                            pic.Hover = false;
                            /*if (OnActionCompleted != null)
                            {
                                OnActionCompleted(this, new EventArgs());
                            }*/
                            break;
                        }
                    case CharacterState.Moving:
                        {
                            //pic.SetCurrentAnimationByName("Moving");
                            pic.Hover = false;
                            /*if (OnActionCompleted != null)
                            {
                                OnActionCompleted(this, new EventArgs());
                            }*/
                            break;
                        }
                    case CharacterState.Idle:
                        {
                            //pic.SetCurrentAnimationByName("Idle");
                            pic.Hover = true;
                            /*if (OnActionCompleted != null)
                            {
                                OnActionCompleted(this, new EventArgs());
                            }*/
                            break;
                        }
                    case CharacterState.Spawn:
                        {
                            //pic.SetCurrentAnimationByName("Idle");
                            pic.Hover = false;
                            /*if (effects.ContainsKey("Spawn"))
                            {
                                effects["Spawn"].Play();
                                pic.Child = effects["Spawn"];
                                effects["Spawn"].OnAnimationFini += new EventHandler(OnEffectAnimationFini);
                            }*/
                            /*if (OnActionCompleted != null)
                            {
                                OnActionCompleted(this, new EventArgs());
                            }*/
                            break;
                        }
                    case CharacterState.Landing:
                        {
                            //pic.SetCurrentAnimationByName("Landing");
                            //pic.CurrentAnimation.OnAnimationFini += new EventHandler(OnPicAnimationFini);
                            pic.Hover = false;
                            /*if (OnActionCompleted != null)
                            {
                                OnActionCompleted(this, new EventArgs());
                            }*/
                            break;
                        }
                    case CharacterState.Attack:
                        {
                            //pic.SetCurrentAnimationByName("Attack2");
                            //pic.CurrentAnimation.OnAnimationFini += new EventHandler(OnPicAnimationFini);
                            //pic.CurrentAnimation.OnAnimationEvent += new EventHandler(OnPicAnimationEvent);
                            pic.Hover = false;
                            /*if (OnActionCompleted != null)
                            {
                                OnActionCompleted(this, new EventArgs());
                            }*/
                            break;
                        }
                    case CharacterState.Dying:
                        {
                            //pic.SetCurrentAnimationByName("Dead");
                            //pic.CurrentAnimation.OnAnimationFini += new EventHandler(OnPicAnimationFini);
                            pic.Hover = false;

                            break;
                        }
                    case CharacterState.Dead:
                        {
                            pic.Hover = false;
                            /*if (OnActionCompleted != null)
                            {
                                OnActionCompleted(this, new EventArgs());
                            }*/
                            break;
                        }
                }
            }
        }


        public void AddPreRenderEffect(string name, PreRenderEffect effect)
        {
            effects[name] = effect;
        }

        protected void OnPicAnimationEvent(object sender, EventArgs e)
        {
            CharacterAnimation anim = sender as CharacterAnimation;
            if (anim.Name.Contains("Attack"))
            {
                if (attacktarget != null)
                {
                    attacktarget.BeAttack(this);
                    attacktarget = null;
                }
            }

        }

        protected void OnPicAnimationFini(object sender, EventArgs e)
        {
            /*CharacterAnimation anim = sender as CharacterAnimation;
            anim.OnAnimationFini -= new EventHandler(OnPicAnimationFini);
            if (anim.Name == "Launch")
            {
                State = CharacterState.Moving;
            }
            else if (anim.Name == "Landing")
            {
                State = CharacterState.Idle;
            }
            else if (anim.Name.Contains("Attack"))
            {
                State = CharacterState.Launch;
            }
            else if (anim.Name == "Dead")
            {
                State = CharacterState.Dead;
            }
            */
        }

        protected void OnEffectAnimationFini(object sender, EventArgs e)
        {
            PreRenderEffect effect = sender as PreRenderEffect;
            effect.OnAnimationFini -= new EventHandler(OnEffectAnimationFini);
            if (effect.Name == "spawn")
            {
                state = CharacterState.Idle;
                pic.Hover = true;
            }
        }

        protected virtual void SendOnArrived(object sender)
        {
            if (OnArrived != null)
            {
                OnArrived(sender, new EventArgs());
                OnArrived = null;
            }
        }

        public virtual void Update(GameTime gametime)
        {
            try
            {
                UpdateActionSet(gametime);
                if (state == CharacterState.Moving)
                    UpdateMovement(gametime);
                //pic.Update(gametime);

                if (title != null)
                    title.Position = position - new Vector2(0, pic.FrameSize.Y * pic.Size.Y * 0.5f) - GameConst.NameTitleOffset;
            }
            catch
            {
                Log.WriteLine(ToString() + "::Update");
            }
        }

        protected virtual void Notify(GameAction action, object p1, object p2)
        {

        }

        protected virtual void UpdateMovement(GameTime gametime)
        {

        }

        public void PushPosition()
        {
            positionbackup = Position;
        }

        public void PopPosition()
        {
            Position = positionbackup;
        }

        public void BeAttack(Character offense)
        {
            try
            {
                float dhp = offense.ATK * 1.5f - def * 1.3f;
                hp -= (int)dhp;
                ClearActionSet();
                AddActionSet("BeAttack", CharacterState.BeAttack, CharacterActionSetChangeFactor.AnimationCompleted, null);
                if (hp <= 0)
                {
                    //ClearActionSet();
                    //AddActionSet("Dying", CharacterState.Dying, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    AddActionSet("Dead", CharacterState.Dead, CharacterActionSetChangeFactor.AnimationCompleted, null);
                    offense.Notify(GameAction.Kill, this.templateid, 1);
                }
            }
            catch (NullReferenceException)
            {
                Log.WriteLine(ToString() + ":must had offense");
            }
        }


        public void ClearActionSet()
        {
            actionsets.Clear();
            currentactionset = null;
        }

        public void AddActionSet(string animname, CharacterState state, CharacterActionSetChangeFactor factor, object o)
        {
            CharacterActionSet cas = new CharacterActionSet(animname, state, factor, o);
            actionsets.Add(cas);
        }

        public void AddActionSetPre(string animname, CharacterState state, CharacterActionSetChangeFactor factor, object o)
        {
            CharacterActionSet cas = new CharacterActionSet(animname, state, factor, o);
            actionsets.Insert(0, cas);
        }

        private void UpdateActionSet(GameTime gametime)
        {
            while (actionsets.Count > 0 && currentactionset == null)
            {
                currentactionset = actionsets[0];
                if (pic.SetCurrentAnimationByName(currentactionset.animname))
                {
                    Log.WriteLine(string.Format("Now action is {0}", currentactionset.animname));
                    State = currentactionset.state;
                    switch (currentactionset.factor)
                    {
                        case CharacterActionSetChangeFactor.AnimationCompleted:
                            {
                                pic.CurrentAnimation.OnAnimationFini += new EventHandler(OnUpdateActionSets);
                                pic.CurrentAnimation.OnAnimationEvent += new EventHandler(OnPicAnimationEvent);
                                break;
                            }
                        case CharacterActionSetChangeFactor.ArriveTarget:
                            {
                                Target = currentactionset.target;
                                OnArrived += new EventHandler(OnUpdateActionSets);
                                break;
                            }
                        case CharacterActionSetChangeFactor.ArriveAttackTarget:
                            {
                                AttackTarget = currentactionset.interactive;
                                OnArrived += new EventHandler(OnUpdateActionSets);
                                break;
                            }
                        case CharacterActionSetChangeFactor.ArriveInteractiveTarget:
                            {
                                InteractiveTarget = currentactionset.interactive;
                                OnArrived += new EventHandler(OnUpdateActionSets);
                                break;
                            }
                        case CharacterActionSetChangeFactor.EffectCompleted:
                            {
                                if (effects.ContainsKey(currentactionset.effectname))
                                {
                                    effects[currentactionset.effectname].Play();
                                    pic.Child = effects[currentactionset.effectname];
                                    effects[currentactionset.effectname].OnAnimationFini += new EventHandler(OnUpdateActionSets);
                                }
                                else
                                {
                                    OnUpdateActionSets(this, new EventArgs());
                                }
                                break;
                            }
                        case CharacterActionSetChangeFactor.Immediate:
                            {
                                OnUpdateActionSets(this, new EventArgs());
                                break;
                            }
                    }
                }
                else
                {
                    actionsets.RemoveAt(0);
                    currentactionset = null;
                }
            }

            if (currentactionset != null)
            {

            }
        }


        private void OnUpdateActionSets(object sender, EventArgs e)
        {
            if (actionsets.Count > 0)
            {
                actionsets.RemoveAt(0);
                if (actionsets.Count == 0 && currentactionset != null)
                {
                    if (OnActionCompleted != null && currentactionset.factor != CharacterActionSetChangeFactor.Immediate)
                    {
                        OnActionCompleted(this, new EventArgs());
                        //OnActionCompleted = null;
                    }
                }
            }
            currentactionset = null;
        }

        static public Character CreateCharacter(string path, Scene scene, string name = "")
        {
            EntityDefinition.EntityDefinition ed = GameConst.Content.Load<EntityDefinition.EntityDefinition>(@"template/" + path);

            if (ed != null)
            {
                Type type = Type.GetType("demo." + ed.type);
                if (type != null)
                {
                    Character c = type.Assembly.CreateInstance("demo." + ed.type) as Character;
                    if (c != null)
                    {
                        c.scene = scene;
                        c.templateid = ed.tmpid;
                        c.hp = ed.hp;
                        c.maxhp = ed.maxhp;
                        c.atk = ed.atk;
                        c.def = ed.def;
                        if (name != "")
                            c.name = name;
                        else
                            c.name = ed.name;
                        c.speed = ed.speed;
                        CharacterDefinition.PicDef pd = GameConst.Content.Load<CharacterDefinition.PicDef>(@"chardef/" + ed.pics[0]);
                        CharacterPic cpic = new CharacterPic(pd, 0);
                        if (ed.pics.Count > 0)
                        {
                            for (int i = 1; i < ed.pics.Count; ++i)
                            {
                                CharacterDefinition.PicDef pd1 = GameConst.Content.Load<CharacterDefinition.PicDef>(@"chardef/" + ed.pics[i]);
                                cpic.AddCharacterDefinition(pd1);
                            }
                        }
                        cpic.State = RenderChunk.RenderChunkState.FadeIn;
                        c.Picture = cpic;
                        if (c.NeedTitle)
                        {
                            CharacterTitle title = new CharacterTitle(GameConst.CurrentFont);
                            title.NameString = ed.name;
                            title.Character = c;
                            c.Title = title;
                        }
                        c.pic.Size = new Vector2(ed.size, ed.size);
                        c.State = CharacterState.Idle;
                        return c;
                    }
                }
            }
            return null;
        }
    }


}
