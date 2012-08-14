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
using System.Reflection;

namespace demo.uicontrols
{
    public class UIElement : RenderChunk, IDisposable
    {
        protected double duration = -1.0;
        private double lifetime = -1.0;
        private List<UIElement> childs = new List<UIElement>();
        private UIElement parent;
        protected UIElementState uistate = UIElementState.Normal;
        protected Rectangle[] staterects = new Rectangle[(int)UIElementState.Lastword];
        protected string name;
        protected object receiver;
        protected object userdata;
        protected Color fontcolor;

        protected Rectangle srcrect;
        protected Point rendersize;
        protected bool sizedirty = true;
        protected Rectangle controlrect; //控件实际尺寸

        protected Dictionary<string, UIElement> controls = new Dictionary<string, UIElement>();

        public event EventHandler OnClose;

        public Rectangle CurrentRect
        {
            get
            {
                return staterects[(int)uistate];
            }
        }

        public Rectangle SourceRect
        {
            get
            {
                return srcrect;
            }
            set
            {
                srcrect = value;
                sizedirty = true;
            }
        }

        public override Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
                sizedirty = true;

            }
        }

        public object UserData
        {
            get
            {
                return userdata;
            }
            set
            {
               userdata = value;
            }
        }

        public Color FontColor
        {
            get
            {
                return fontcolor;
            }
            set
            {
                fontcolor = value;
            }
        }

        public Point RenderSize
        {
            get
            {
                return rendersize;
            }
            set
            {
                rendersize = value;
                controlrect.Width = rendersize.X;
                controlrect.Height = rendersize.Y;
                sizedirty = true;
            }
        }

        public Rectangle ControlRect
        {
            get
            {
                /*if (parent != null)
                {
                    controlrect.X = (int)(position.X + parent.position.X);
                    controlrect.Y = (int)(position.Y + parent.position.Y);
                    controlrect.Width = rendersize.X;
                    controlrect.Height = rendersize.X;
                }
                else
                {
                    controlrect.X = (int)(position.X);
                    controlrect.Y = (int)(position.Y);
                    controlrect.Width = rendersize.X;
                    controlrect.Height = rendersize.X;
                }*/
                return controlrect;
            }
        }

        public override Vector2 Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                //throw new NotSupportedException(ToString() + ":setSize");
                base.Size = value;
            }
        }

        public override Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                if (parent != null)
                {
                    controlrect.X = (int)(position.X + parent.ControlRect.X);
                    controlrect.Y = (int)(position.Y + parent.ControlRect.Y);
                }
                else
                {
                    controlrect.X = (int)position.X;
                    controlrect.Y = (int)position.Y;
                }
                foreach (UIElement e in childs)
                {
                    e.Position = e.position;
                }
               
            }
        }

        public List<UIElement> Childs
        {
            get
            {
                return childs;
            }
        }

        public void AddChild(UIElement e)
        {
            childs.Add(e);
            e.Parent = this;
        }

        public UIElement Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
                sizedirty = true;
            }
        }

        public UIElementState UIState
        {
            get
            {
                return uistate;
            }
            set
            {
                uistate = value;
            }
        }

        public double Duration
        {
            get
            {
                return duration;
            }
            set
            {
                duration = value;
                lifetime = duration;
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

        public object Receiver
        {
            get
            {
                return receiver;
            }
            set
            {
                receiver = value;
            }
        }

        public UIElement() :
            base(null, 0)
        {

        }

        public UIElement(Texture2D intex, int layer) :
            base(intex, layer)
        {

        }

        public void Close()
        {
            if (OnClose != null)
            {
                OnClose(this, new EventArgs());
                OnClose = null;
            }
        }

        protected virtual void CalculateSize()
        {
            //if (sizedirty)
            {
                if (srcrect.Width == 0)
                {
                    if (staterects[(int)UIElementState.Normal].Width != 0)
                        srcrect.Width = staterects[(int)UIElementState.Normal].Width;
                    else
                    {
                        if (texture != null)
                        {
                            srcrect.Width = texture.Width;
                        }
                    }
                }
                if (srcrect.Height == 0)
                {
                    if (staterects[(int)UIElementState.Normal].Height != 0)
                        srcrect.Height = staterects[(int)UIElementState.Normal].Height;
                    else
                    {
                        if (texture != null)
                        {
                            srcrect.Height = texture.Height;
                        }
                    }
                }
                Vector2 _size = Size;
                if (rendersize.X == 0)
                {
                    _size.X = 1.0f;
                    if (texture != null)
                        controlrect.Width = srcrect.Width;
                }
                else
                {
                    if (texture != null)
                    {
                        _size.X = (float)rendersize.X / (float)srcrect.Width;
                    }
                    else
                        _size.Y = 1.0f;
                }
                if (rendersize.Y == 0)
                {
                    _size.Y = 1.0f;
                    if (texture != null)
                        controlrect.Height = srcrect.Height;
                }
                else
                {
                    if (texture != null)
                    {
                        _size.Y = (float)rendersize.Y / (float)srcrect.Height;
                    }
                    else
                        _size.Y = 1.0f;
                }
                Size = _size;
                //sizedirty = false;
                Position = position; //make controlrect
            }
        }

        
        public override void Update(GameTime gametime)
        {
   
            if (sizedirty)
            {
                CalculateSize();
                sizedirty = false;
            }
            if (duration > 0 && state == RenderChunkState.Show)
            {
                lifetime -= gametime.ElapsedGameTime.TotalSeconds;
                if (lifetime < 0)
                    State = RenderChunkState.FadeOutToDel;
            }
            foreach (UIElement e in childs)
            {
                e.Update(gametime);
               
            }
           
            base.Update(gametime);
        }

        public override void Render(SpriteBatch sb)
        {
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            Vector2 pos = new Vector2();
            pos.X = controlrect.X;
            pos.Y = controlrect.Y;
            sb.Draw(Texture, pos, srcrect, this.Color, 0, Vector2.Zero, Size, SpriteEffects.None, 0.0f);

            foreach (UIElement e in childs)
            {
                e.Render(sb);
            }
            base.Render(sb);
        }

        private void AddStateRect(UIElementState state, int x, int y, int w, int h)
        {
            staterects[(int)state] = new Rectangle(x, y, w, h);
        }

        public virtual void Initialize(UIControlTemplate.UIControlTemplate tmp)
        {
            if (tmp.texture != "")
                texture = GameConst.Content.Load<Texture2D>(@"ui/" + tmp.texture);
            else
                texture = null;
            foreach (UIControlTemplate.UIControlStateRect rect in tmp.rects)
            {
                UIElementState us = UIElementState.Normal;
                try
                {
                    us = (UIElementState)Enum.Parse(typeof(UIElementState), rect.name);
                }
                catch (ArgumentException e)
                {
                    Log.WriteLine(e.Message);
                }
                finally
                {
                    AddStateRect(us, rect.x, rect.y, rect.w, rect.h);
                }
            }
            SourceRect = staterects[(int)UIElementState.Normal];
            foreach (UIControlTemplate.UIControlTemplate t in tmp.childs)
            {
                AddChild(UIMgr.CreateUIControl(t));
            }

            //x += "xx";
        }

        public void RemoveUIControl(string name)
        {
            if (controls.ContainsKey(name))
            {
                controls[name].Dispose();
                controls.Remove(name);
            }
        }

        public UIElement AddUIControl(string tmpname, string name, int x, int y, int w, int h, double dur, object r)
        {
            if (controls.ContainsKey(name))
            {
                return null;
            }
            UIControlTemplate.UIControlTemplate tmp = GameConst.Content.Load<UIControlTemplate.UIControlTemplate>(@"uicontrols/" + tmpname);
            return AddUIControl(UIMgr.CreateUIControl(tmp), name, x, y, w, h, dur, r);
        }

        public UIElement AddUIControl(UIElement control, string name, int x, int y, int w, int h, double dur, object r)
        {
            if (controls.ContainsKey(name))
            {
                return null;
            }
            else
            {
                control.Duration = dur;
                control.Layer = layer + 1;
                control.Name = name;
                int ax = x;
                int ay = y;
                UIMgr.GenerateCoord(x, y, out ax, out ay, control.Texture);
                control.Position = new Vector2(ax, ay);
                control.RenderSize = new Point(w, h);
                control.Receiver = r;

                EventInfo ei = control.GetType().GetEvent("OnClick");
                if (ei != null)
                {
                    string mname = name + "_OnClick";
                    Type tDelegate = ei.EventHandlerType;
                    try
                    {
                        Delegate d = Delegate.CreateDelegate(tDelegate, r, mname);
                        if (d != null)
                        {
                            MethodInfo miAddHandler = ei.GetAddMethod();
                            if (miAddHandler != null)
                            {
                                object[] addHandlerArgs = { d };
                                miAddHandler.Invoke(control, addHandlerArgs);
                            }
                        }
                    }
                    catch
                    {

                    }
                }


                ei = control.GetType().GetEvent("OnClose");
                if (ei != null)
                {
                    string mname = name + "_OnClose";
                    Type tDelegate = ei.EventHandlerType;
                    try
                    {
                        Delegate d = Delegate.CreateDelegate(tDelegate, r, mname);
                        if (d != null)
                        {
                            MethodInfo miAddHandler = ei.GetAddMethod();
                            if (miAddHandler != null)
                            {
                                object[] addHandlerArgs = { d };
                                miAddHandler.Invoke(control, addHandlerArgs);
                            }
                        }
                    }
                    catch
                    {
                        Log.WriteLine("UI Command not found.");
                    }
                }

                //this.child
                //control.ValidParameter();
                AddChild(control);
                controls[name] = control;
                UIMgr.AddUIControlToRenderList(control);
                return control;
            }
        }


        public virtual int HandleMessage(UIMessage msg, object p1, object p2)
        {
            int result = 0;
            foreach(UIElement e in childs)
            {
                result += e.HandleMessage(msg, p1, p2);
            }
            return result;
        }

        protected sealed override void OnChangeState(RenderChunkState state)
        {
            base.OnChangeState(state);
            foreach (UIElement e in childs)
            {
                e.State = state;
            }
        }

        public UIElement GetUIControlByName(string name)
        {
            if (!controls.ContainsKey(name))
            {
                return null;
            }
            return controls[name];
        }

        public void Dispose()
        {
            
        }
    }
}
