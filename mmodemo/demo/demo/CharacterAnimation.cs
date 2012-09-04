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
    public class CharacterAnimation
    {
        public class AnimationFrame
        {
            public Rectangle rect;
            public AligningMode align = AligningMode.None;
            public Vector2 center = new Vector2();
        }

        [Flags]
        public enum AligningMode
        {
            None = 0,
            Left = 2,
            Top = 4,
            Right = 8,
            Bottom = 16,
            Center = 32,
        }

        private Rectangle currentRect = new Rectangle();
        private Vector2 currentOrigin = new Vector2(0, 0);
        private int frameCount = 0;
        private int currentFrame = 0;
        private List<AnimationFrame> frameRects = new List<AnimationFrame>();
        private float speed;
        private string name;
        private bool loop;
        private int eventframe = -1;
       
        public event EventHandler OnAnimationFini;
        public event EventHandler OnAnimationEvent;

        public CharacterAnimation(string n)
        {
            name = n;
        }
    
        public int FrameCount
        {
            get
            {
                return frameCount;
            }
            set
            {
                frameCount = value;
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

        public int EventFrame
        {
            get
            {
                return eventframe;
            }
            set
            {
                try
                {
                    if (value >= frameCount)
                        throw new Exception(ToString() + "event frame must less than framecount.");
                    eventframe = value;
                }
                catch
                {
                    eventframe = -1;
                }
            }
        }

        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                loop = value;
            }
        }

       
        public int CurrentFrame
        {
            get
            {
                return currentFrame;
            }
            set
            {
                currentFrame = value;
                if (currentFrame >= frameCount)
                {
                    currentFrame = loop ? 0 : frameCount - 1;
                }
                else if (currentFrame < 0)
                {
                    currentFrame = loop ? frameCount - 1 : 0;
                }
                currentRect = frameRects[currentFrame].rect;
                CalculateOrigin(frameRects[currentFrame]);
            }
        }



        public Rectangle CurrentRect
        {
            get
            {
                return currentRect;
            }

        }

        public Vector2 CurrentOrigin
        {
            get
            {
                return currentOrigin;
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

        public void Reset()
        {
            currentFrame = 0;
            currentRect = frameRects[currentFrame].rect;
            CalculateOrigin(frameRects[currentFrame]);
        }

        public void AddFrame(int x, int y, int w, int h, string center)
        {
            AnimationFrame f = new AnimationFrame();
            f.rect = new Rectangle(x, y, w, h);
            f.align = AligningMode.None;
            if (center.Contains("center"))
                f.align |= AligningMode.Center;
            if (center.Contains("left"))
                f.align |= AligningMode.Left;
            if (center.Contains("right"))
                f.align |= AligningMode.Right;
            if (center.Contains("top"))
                f.align |= AligningMode.Top;
            if (center.Contains("bottom"))
                f.align |= AligningMode.Bottom;
            if (f.align == AligningMode.None)
            {
                string[] coord = center.Split('_');
                f.center.X = (float)Convert.ToDouble(coord[0]);
                f.center.Y = (float)Convert.ToDouble(coord[1]);
            }
            frameRects.Add(f);
        }

        private void CalculateOrigin(AnimationFrame f)
        {
            if (f.align == AligningMode.None)
            {
                currentOrigin = f.center;
            }
            else
            {
#if WINDOWS_PHONE			
                if (((int)f.align & (int)AligningMode.Center) != 0)
#else
				if (f.align.HasFlag(AligningMode.Center))
#endif				
                {
                    currentOrigin.X = f.rect.Width / 2;
                    currentOrigin.Y = f.rect.Height / 2;
                }
#if WINDOWS_PHONE				
                if (((int)f.align & (int)AligningMode.Left) != 0)
#else
				if (f.align.HasFlag(AligningMode.Left))
#endif								
                {
                    currentOrigin.X = 0;
                }
#if WINDOWS_PHONE				
                else if (((int)f.align & (int)AligningMode.Right) != 0)
#else
                else if (f.align.HasFlag(AligningMode.Right))
#endif								
                {
                    currentOrigin.X = f.rect.Width;
                }
#if WINDOWS_PHONE				
                if (((int)f.align & (int)AligningMode.Top) != 0)
#else			
				if (f.align.HasFlag(AligningMode.Top))
#endif					
                {
                    currentOrigin.Y = 0;
                }
#if WINDOWS_PHONE				
                else if (((int)f.align & (int)AligningMode.Bottom) != 0)
#else
				else if (f.align.HasFlag(AligningMode.Bottom))
#endif				
                {
                    currentOrigin.Y = f.rect.Height;
                }
            }
        }

        double _frametime = 0.0;
        double _frameinterval = 1.0 / 15.0;
        public void Update(GameTime gametime)
        {
            _frametime += gametime.ElapsedGameTime.TotalSeconds * speed;
            if (_frametime > _frameinterval)
            {
                _frametime = 0.0;
                currentFrame++;
                if (currentFrame == eventframe && OnAnimationEvent != null)
                {
                    OnAnimationEvent(this, new EventArgs());
                    OnAnimationEvent = null;
                }
                if (currentFrame >= frameCount)
                {
                    currentFrame = loop ? 0 : frameCount - 1;
                    if (OnAnimationFini != null)
                    {
                        OnAnimationFini(this, new EventArgs());
                        OnAnimationFini = null;
                    }
                }
                currentRect = frameRects[currentFrame].rect;
                CalculateOrigin(frameRects[currentFrame]);
            }
            else if (_frametime < -_frameinterval)
            {
                _frametime = 0.0;
                currentFrame--;
                if (currentFrame == eventframe && OnAnimationEvent != null)
                {
                    OnAnimationEvent(this, new EventArgs());
                    OnAnimationEvent = null;
                }
                if (currentFrame < 0)
                {
                    currentFrame = loop ? frameCount - 1 : 0;
                    if (OnAnimationFini != null)
                    {
                        OnAnimationFini(this, new EventArgs());
                        OnAnimationFini = null;
                    }
                }
                currentRect = frameRects[currentFrame].rect;
                CalculateOrigin(frameRects[currentFrame]);
            }
            else
            {
                currentRect = frameRects[currentFrame].rect;
                CalculateOrigin(frameRects[currentFrame]);
            }
        }
    }
}
