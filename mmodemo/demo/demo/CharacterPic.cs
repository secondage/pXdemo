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
    public class CharacterPic : RenderChunk
    {

        private Dictionary<string, CharacterAnimation> Animations = new Dictionary<string, CharacterAnimation>();
        private CharacterAnimation currentAnim = null;
        private Vector2 dir = new Vector2(1, 0);
        private float angle = 0.0f;//MathHelper.ToRadians(180.0f);
        private Vector2 framesize;
        private float originangle = 0.0f;
        private bool hover = false;
        private RenderChunk child;
        private bool highlight;

        private Dictionary<string, Texture2D> animTextures = new Dictionary<string, Texture2D>();

        public CharacterPic(CharacterDefinition.PicDef pd, int inlayer) :
            base(null, inlayer)
        {
            Initialize(pd);
        }


        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
            }
        }

        public float OriginAngle
        {
            get
            {
                return originangle;
            }
            set
            {
                originangle = value;
            }
        }

        public CharacterAnimation CurrentAnimation
        {
            get
            {
                return currentAnim;
            }
        }

        public Vector2 Direction
        {
            get
            {
                return dir;
            }
            set
            {
                dir = value;
                dir.Normalize();
                if (dir.X > 0.0f)
                {
                    if (dir.Y > 0.0f)
                    {
                        _endAngle = (float)Math.Acos((double)dir.X);
                    }
                    else if (dir.Y < 0.0f)
                    {
                        _endAngle = -(float)Math.Acos((double)dir.X);
                    }
                    else
                    {
                        _endAngle = originangle;
                    }
                }
                else if (dir.X < 0.0f)
                {
                    if (dir.Y > 0.0f)
                    {
                        _endAngle = -(float)Math.Acos((double)-dir.X);
                    }
                    else if (dir.Y < 0.0f)
                    {
                        _endAngle = (float)Math.Acos((double)-dir.X);
                    }
                    else
                    {
                        _endAngle = originangle;
                    }
                }
                else
                {
                    _endAngle = originangle;
                }
                _endAngle = MathHelper.Clamp(_endAngle, -0.52f, 0.52f);
                _startAngle = angle;
                _rotatingTime = _rotatingDur;
            }
        }

        public override Vector2 FrameSize
        {
            get
            {
                return framesize;
            }
        }

        public bool Hover
        {
            get
            {
                return hover;
            }
            set
            {
                hover = value;
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

        public int OriginDir
        {
            set
            {
                if (value == 0)
                    originangle = 0.0f;
                else
                    originangle = MathHelper.ToRadians(0);
            }
        }

        public RenderChunk Child
        {
            get
            {
                return child;
            }
            set
            {
                child = value;
            }
        }

        public bool SetCurrentAnimationByName(string name/*, float speed, bool loop*/)
        {
            if (currentAnim.Name == name)
                return true;
            if (Animations.ContainsKey(name))
            {
                currentAnim = Animations[name];
                //currentAnim.Speed = speed * currentAnim.Speed;
                //currentAnim.Loop = loop;
                currentAnim.Reset();
                return true;
            }
            Log.WriteLine(string.Format("Invalid animation name {0}", name));
            return false;
        }

        public void AddCharacterDefinition(CharacterDefinition.PicDef pd)
        {
            Texture2D tex = GameConst.Content.Load<Texture2D>(@"character/" + pd.texture);
            if (tex != null)
            {
                foreach (CharacterDefinition.AnimDef ad in pd.anims)
                {
                    CharacterAnimation cani = new CharacterAnimation(ad.name);
                    cani.FrameCount = ad.framecount;
                    cani.EventFrame = ad.eventframe;
                    cani.Loop = ad.loop;
                    cani.Speed = ad.speed;
                    foreach (CharacterDefinition.AnimFrameDef fd in ad.frames)
                    {
                        cani.AddFrame(fd.x, fd.y, fd.w, fd.h, fd.center);
                        cani.Reset();
                    }
                    Animations[ad.name] = cani;
                    animTextures[ad.name] = tex;
                }
            }
        }

        public void Initialize(CharacterDefinition.PicDef pd)
        {
            Texture2D tex = GameConst.Content.Load<Texture2D>(@"character/" + pd.texture);
            framesize.X = pd.width;
            framesize.Y = pd.height;
            Size = new Vector2(pd.size, pd.size);
            foreach (CharacterDefinition.AnimDef ad in pd.anims)
            {
                CharacterAnimation cani = new CharacterAnimation(ad.name);
                cani.FrameCount = ad.framecount;
                cani.EventFrame = ad.eventframe;
                cani.Loop = ad.loop;
                cani.Speed = ad.speed;
                foreach (CharacterDefinition.AnimFrameDef fd in ad.frames)
                {
                    cani.AddFrame(fd.x, fd.y, fd.w, fd.h, fd.center);
                    cani.Reset();
                }
                Animations[ad.name] = cani;
                animTextures[ad.name] = tex;
            }
            currentAnim = Animations["Idle"];
        }


        public override void Render(SpriteBatch sb)
        {
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            
            Vector2 pos = Position;
            if (coordsystem == CoordinateSystemType.World)
            {
                pos.X -= Scene.Viewport.X;
                pos.Y -= Scene.Viewport.Y;
            }
            /*Rectangle n = new Rectangle((int)Position.X, (int)Position.Y, (int)(currentAnim.CurrentRect.Width * size.X), (int)(currentAnim.CurrentRect.Height * size.Y));
            Rectangle p = new Rectangle((int)scene.Viewport.X, (int)scene.Viewport.Y, (int)scene.Viewport.Z, (int)scene.Viewport.W);
            if (!n.Intersects(p))
                return;*/


            sb.Draw(animTextures[currentAnim.Name], 
                pos, 
                currentAnim.CurrentRect, 
                highlight ? Color.Red : this.Color, 
                angle, currentAnim.CurrentOrigin, 
                Size, 
                dir.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 
                0.0f);

            if (child != null)
            {
                child.Render(sb);
            }

            base.Render(sb);
        }

        double _flytime = 5.0;
        public override void Update(GameTime gametime)
        {
            if (currentAnim != null)
            {
                currentAnim.Update(gametime);
                framesize.X = currentAnim.CurrentRect.Width;
                framesize.Y = currentAnim.CurrentRect.Height;
            }

            

            if (child != null)
            {
                child.Position = position;// -child.FrameSize * 0.5f;
                child.Update(gametime);
            }

            UpdateRotating(gametime);

            if (hover)
                position.Y += (float)Math.Sin(gametime.TotalGameTime.TotalSeconds * _flytime) * 0.3f;

            base.Update(gametime);
        }

        float _endAngle;
        float _startAngle;
        float _rotatingDur = 0.3f;
        float _rotatingTime = -0.0f;
        private void UpdateRotating(GameTime gametime)
        {
            if (_rotatingTime > 0.0f)
            {
                angle = MathHelper.Lerp(_startAngle, _endAngle, (_rotatingDur - _rotatingTime) / _rotatingDur);
                _rotatingTime -= (float)gametime.ElapsedGameTime.TotalSeconds;
                if (_rotatingTime < 0.0f)
                    angle = _endAngle;
            }
        }
    }

}
