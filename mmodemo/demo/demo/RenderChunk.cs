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
    public class RenderChunk : IComparable
    {
        public enum RenderChunkState
        {
            Show = 0,
            FadeIn,
            FadeOut,
            FadeOutToDel,
            Hide,
            Invisible,
            Delete,
        };

        public enum CoordinateSystemType
        {
            World,
            Screen,
            Local,
        }


        

        protected int layer = 0;
        protected Texture2D texture = null;
        protected Vector2 position = new Vector2();
        private Vector2 size = new Vector2(1, 1);
        protected Vector2 speed = new Vector2();
        protected Scene scene;
        protected Color color = Color.White;
        protected Color origincolor = Color.White;
        protected RenderChunkState state;
        protected CoordinateSystemType coordsystem = CoordinateSystemType.World;
        private bool highlight;
        

        private float stateAniCurTime = -1.0f;
        private float stateAniDuration = 1.0f;

        public RenderChunk()
        {
        }

        public RenderChunk(Texture2D intexture, int inlayer)
        {
            texture = intexture;
            layer = inlayer;
        }

        public int Layer
        {
            get
            {
                return layer;
            }
            set
            {
                layer = value;
            }
        }

        public virtual Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }

        public virtual Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public virtual Vector2 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value * GameConst.ForegroundScale;
            }
        }

        public CoordinateSystemType CoordinateSystem
        {
            get
            {
                return coordsystem;
            }
            set
            {
                coordsystem = value;
            }
        }

        public Vector2 Speed
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

        public Vector2 ActualSize
        {
            get
            {
                try
                {
                    return new Vector2(texture.Width * size.X, texture.Height * size.Y);
                }
                catch
                {
                    return Vector2.Zero;
                }
            }
        }

        public Vector2 TextureSize
        {
            get
            {
                try
                {
                    return new Vector2(texture.Width, texture.Height);
                }
                catch
                {
                    return Vector2.Zero;
                }
            }
        }

        public virtual Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public Color OriginColor
        {
            get
            {
                return origincolor;
            }
            set
            {
                origincolor = value;
            }
        }

        public virtual Vector2 FrameSize
        {
            get
            {
                return ActualSize;
            }
        }

        public bool HighLight
        {
            get
            {
                return highlight;
            }
            set
            {
                highlight = value;
            }
        }


        public RenderChunkState State
        {
            get
            {
                return state;
            }
            set
            {
                OnChangeState(value);
                state = value;
            }
        }

        public int CompareTo(Object obj)
        {
            RenderChunk bg = obj as RenderChunk;
            return Layer - bg.Layer;
        }

        public virtual void Render(SpriteBatch sb)
        {
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            GameConst.RenderCountPerFrame++;
        }

        public virtual void Update(GameTime gametime)
        {
            UpdateStateAnimation(gametime);
        }

        protected virtual void OnChangeState(RenderChunkState state)
        {
            switch (state)
            {
                case RenderChunkState.FadeOut:
                case RenderChunkState.FadeOutToDel:
                    {
                        stateAniCurTime = stateAniDuration;
                        break;
                    }
                case RenderChunkState.FadeIn:
                    {
                        stateAniCurTime = stateAniDuration;
                        color.A = 0;
                        break;
                    }
            }
        }

        private void UpdateStateAnimation(GameTime gametime)
        {
            switch (state)
            {
                case RenderChunkState.FadeOut:
                case RenderChunkState.FadeOutToDel:
                    {
                        if (stateAniCurTime >= 0.0f)
                        {
                            color.A = (byte)(MathHelper.SmoothStep(origincolor.ToVector4().W, 0.0f, (stateAniDuration - stateAniCurTime) / stateAniDuration) * 255.0f);
                            stateAniCurTime -= (float)gametime.ElapsedGameTime.TotalSeconds;
                            if (stateAniCurTime < 0.0f)
                            {
                                color.A = 0;
                                state = state == RenderChunkState.FadeOut ? RenderChunkState.Hide : RenderChunkState.Delete;
                            }
                        }
                        break;
                    }
                case RenderChunkState.FadeIn:
                    {
                        if (stateAniCurTime >= 0.0f)
                        {
                            color.A = (byte)(MathHelper.SmoothStep(0.0f, origincolor.ToVector4().W, (stateAniDuration - stateAniCurTime) / stateAniDuration) * 255.0f);
                            stateAniCurTime -= (float)gametime.ElapsedGameTime.TotalSeconds;
                            if (stateAniCurTime < 0.0f)
                            {
                                color.A = origincolor.A;
                                state = RenderChunkState.Show;
                            }
                        }
                        break;
                    }
            }
        }
    }
}
