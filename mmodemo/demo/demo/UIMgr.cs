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
using demo.uicontrols;
using System.Reflection;
using System.Reflection.Emit;

namespace demo
{
    public enum UIMessage
    {
        MouseMove = 1,
        MouseDown,
        MouseUp,
        MouseClick,
    };
    public enum UIElementState
    {
        Normal,
        Disable,
        MouseIn,
        MouseOut,
        Down,
        Up,
        Lastword,
    };
    public enum UILayout
    {
        Free = 0,
        Center = -1,
        Right = -2,
        Left = -3,
        Top = -4,
        Bottom = -5,
    }
    public class UIMgr
    {
        static private Texture2D controlstexture;
        static private List<UIElement> renderlist = new List<UIElement>();
        static Dictionary<string, UIElement> controls = new Dictionary<string, UIElement>();

        static public Texture2D ControlsTexture
        {
            set
            {
                controlstexture = value;
            }
            get
            {
                return controlstexture;
            }
        }

        static public void GenerateCoord(int x, int y, out int ox, out int oy, Texture2D texture)
        {
            ox = x;
            oy = y;
            if (x < 0)
            {
                switch (x)
                {
                    case (int)UILayout.Center:
                        {
                            ox = (GameConst.ScreenWidth - texture.Width) / 2;
                            break;
                        }
                    case (int)UILayout.Right:
                        {
                            ox = (GameConst.ScreenWidth - texture.Width);
                            break;
                        }
                    case (int)UILayout.Left:
                        {
                            ox = 0;
                            break;
                        }
                }
            }
            if (y < 0)
            {
                switch (y)
                {
                    case (int)UILayout.Center:
                        {
                            oy = (GameConst.ScreenHeight - texture.Height) / 2;
                            break;
                        }
                    case (int)UILayout.Bottom:
                        {
                            oy = (GameConst.ScreenHeight - texture.Height);
                            break;
                        }
                    case (int)UILayout.Top:
                        {
                            oy = 0;
                            break;
                        }
                }
            }
        }

        static public void ShowLeaderDialog(bool show)
        {
            if (GetUIControlByName("leader_dlg") != null)
            {
                GetUIControlByName("leader_dlg").State = show ? RenderChunk.RenderChunkState.FadeIn : RenderChunk.RenderChunkState.FadeOut;
            }
        }



        static public UIElement GetUIControlByName(string name)
        {
            if (!controls.ContainsKey(name))
            {
                return null;
            }
            return controls[name];
        }

        static public void AddUIControlToRenderList(UIElement e)
        {
            try
            {
                renderlist.Add(e);
                renderlist.Sort();
            }
            catch (NullReferenceException)
            {

            }
        }

        static public UIElement AddUIControl(string tmpname, string name, int x, int y, int w, int h, double dur, int layer, object r)
        {
            if (controls.ContainsKey(name))
            {
                return null;
            }
            UIControlTemplate.UIControlTemplate tmp = GameConst.Content.Load<UIControlTemplate.UIControlTemplate>(@"uicontrols/" + tmpname);
            return AddUIControl(UIMgr.CreateUIControl(tmp), name, x, y, w, h, dur, layer, r);
        }

        static public UIElement AddUIControl(UIElement control, string name, int x, int y, int w, int h, double dur, int layer, object r)
        {
            if (controls.ContainsKey(name))
            {
                return null;
            }
            else
            {
                control.Duration = dur;
                control.Layer = layer;
                control.Name = name;
                int ax = x;
                int ay = y;
                GenerateCoord(x, y, out ax, out ay, control.Texture);
                if (x < 0)
                    control.LayoutHorizontal = (UILayout)x;
                if (y < 0)
                    control.LayoutVerticality = (UILayout)y;
                control.Position = new Vector2(ax, ay);
                control.RenderSize = new Point(w, h);
                control.Receiver = r;

                EventInfo ei = control.GetType().GetEvent("OnClick");
                if (ei != null)
                {
                    string mname = name + "_OnClick";
                    Type tDelegate = ei.EventHandlerType;
                    Delegate d = Delegate.CreateDelegate(tDelegate, r, mname);
                    if (d != null)
                    {
                        MethodInfo miAddHandler = ei.GetAddMethod();
                        object[] addHandlerArgs = { d };
                        miAddHandler.Invoke(control, addHandlerArgs);
                    }
                }

                //control.ValidParameter();
                controls[name] = control;
                renderlist.Add(control);
                renderlist.Sort();
                return control;
            }
        }

        static List<UIElement> _removelist = new List<UIElement>();
        static public void Update(GameTime gametime)
        {
            _removelist.Clear();
            for (int i = 0; i < renderlist.Count; ++i)
            {
                UIElement rc = renderlist[i];
                if (rc.State == RenderChunk.RenderChunkState.Delete)
                {
                    _removelist.Add(rc);
                }
                else
                    rc.Update(gametime);
            }
            foreach (UIElement rc in _removelist)
            {
                rc.Close();
                rc.Dispose();
                controls.Remove(rc.Name);
                renderlist.Remove(rc);
            }
        }

        static public void Render(SpriteBatch sb)
        {
            for (int i = 0; i < renderlist.Count; ++i)
            {
                UIElement rc = renderlist[i];
                rc.Render(sb);
            }
        }

        static public UIElement CreateUIControl(string tmpname)
        {
            UIControlTemplate.UIControlTemplate tmp = GameConst.Content.Load<UIControlTemplate.UIControlTemplate>(@"uicontrols/" + tmpname);
            return CreateUIControl(tmp);
        }

        static public UIElement CreateUIControl(UIControlTemplate.UIControlTemplate tmp)
        {
            string typestr = "demo.uicontrols." + tmp.type;
            Type type = Type.GetType(typestr);
            if (type != null)
            {
                UIElement control = type.Assembly.CreateInstance(typestr) as UIElement;
                if (control != null)
                {
                    control.Initialize(tmp);
                    return control;
                }
            }
            return null;
        }

        static public int HandleMessage(UIMessage msg, object p1, object p2)
        {
            int result = 0;
            foreach (KeyValuePair<string, UIElement> u in controls)
            {
                result += u.Value.HandleMessage(msg, p1, p2);
            }
            return result;
        }
    }
}
