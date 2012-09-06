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
#if WINDOWS
using System.Windows.Forms;
#endif

namespace demo.uicontrols
{
    public class UIButton : UIElement
    {
#if WINDOWS
        public event MouseEventHandler OnClick;
#else
        public event EventHandler OnClick;
#endif		
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }

        public override void Render(SpriteBatch sb)
        {
            base.Render(sb);
            /*Vector2 pos = new Vector2();
            pos.X = controlrect.X;
            pos.Y = controlrect.Y;
            foreach (string t in textline)
            {
                sb.DrawString(GameConst.CurrentFont, t, pos, this.Color);
                pos.Y += linespace;
            }*/
        }

        public override void Initialize(UIControlTemplate.UIControlTemplate tmp)
        {
            base.Initialize(tmp);
        }

        protected override void CalculateSize()
        {
            base.CalculateSize();
        }

        public override int HandleMessage(UIMessage msg, object p1, object p2)
        {
            if (state != RenderChunkState.Show)
                return 0;
            int result = 0;
            switch (msg)
            {
                case UIMessage.MouseMove:
                    {
                        if (controlrect.Contains((int)p1, (int)p2))
                        {
                            if (uistate == UIElementState.Normal)
                            {
                                SourceRect = staterects[(int)UIElementState.MouseIn];
                                uistate = UIElementState.MouseIn;
                            }
                        }
                        else
                        {
                            if (uistate == UIElementState.MouseIn)
                            {
                                SourceRect = staterects[(int)UIElementState.Normal];
                                uistate = UIElementState.Normal;
                            }
                            else if (uistate == UIElementState.Down)
                            {
                                SourceRect = staterects[(int)UIElementState.Normal];
                                uistate = UIElementState.Normal;
                                Position -= new Vector2(2, 2);
                            }
                        }
                        break;
                    }
                case UIMessage.MouseDown:
                    {
                        if (controlrect.Contains((int)p1, (int)p2))
                        {
                            if (uistate != UIElementState.Down)
                            {
                                uistate = UIElementState.Down;
                                Position += new Vector2(2, 2);
                                result++;
                            }
                        }
                        break;
                    }
                case UIMessage.MouseUp:
                    {
                        if (controlrect.Contains((int)p1, (int)p2))
                        {
                            if (uistate == UIElementState.Down)
                            {
                                uistate = UIElementState.MouseIn;
                                Position -= new Vector2(2, 2);
                            }
                        }
                        break;
                    }
                case UIMessage.MouseClick:
                    {
#if WINDOWS_PHONE
                        if (controlrect.Contains(Convert.ToInt32(p1), Convert.ToInt32(p2)))
#else					
						if (controlrect.Contains((int)p1, (int)p2))	
#endif						
                        {
                            if (OnClick != null)
                            {
#if WINDOWS_PHONE							
                                OnClick(this, new EventArgs());
#else
								OnClick(this, new MouseEventArgs(MouseButtons.Left, 1, (int)p1, (int)p2, 0));
#endif								
                            }
                            result++;
                        }
                        break;
                    }

            }
            return base.HandleMessage(msg, p1, p2) + result;
        }
    }



}
