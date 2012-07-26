using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace demo
{
    public class PreRenderEffect : RenderChunk
    {
        
        private CharacterAnimation animation;
        private string name;
        private RenderChunk parent;
        private Vector2 framesize;
        private float playspeed = 1.0f;
        

        public event EventHandler OnAnimationFini;

        public bool Loop
        {
            get
            {
                return animation.Loop;
            }
            set
            {
                animation.Loop = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public override Vector2 FrameSize
        {
            get
            {
                return framesize;
            }
        }

        public RenderChunk Parent
        {
            set
            {
                parent = value;
            }
        }

        public float PlaySpeed
        {
            get
            {
                return animation.Speed;
            }
            set
            {
                playspeed = value;
                animation.Speed = playspeed;
            }
        }

        public PreRenderEffect(string n, int w, int h) :
            base(null, 0)
        {
            name = n;
            animation = new CharacterAnimation(name);
            framesize.X = w;
            framesize.Y = h;
        }

        public void RenderFrame(int frame, SpriteBatch sb)
        {
            animation.CurrentFrame = frame;
            Render(sb);
        }


        public void Initialize(ContentManager content)
        {
            string assertname = "effect/" + name;

            animation = new CharacterAnimation(name);
            PreRenderEffectContent.FrameDef[] defs = content.Load<PreRenderEffectContent.FrameDef[]>(assertname);
            animation.FrameCount = defs.Length;
            animation.Speed = playspeed;
            foreach (PreRenderEffectContent.FrameDef def in defs)
            {
                animation.AddFrame(def.x, def.y, def.w, def.h, "center");
            }
            animation.Reset();

            assertname = "effect/" + name + "_pic";
            texture = content.Load<Texture2D>(assertname);
        }

        public override void Update(GameTime gametime)
        {
            if (animation != null)
            {
                animation.Update(gametime);
            }
            base.Update(gametime);
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

            sb.Draw(texture, pos, animation.CurrentRect, this.Color, 0.0f, FrameSize * 0.5f, Size, SpriteEffects.None, 0.0f);

            base.Render(sb);
        }

        public void Play()
        {
            state = RenderChunkState.Show;
            animation.OnAnimationFini += new EventHandler(OnPicAnimationFini);
        }

        private void OnPicAnimationFini(object sender, EventArgs e)
        {
            animation.OnAnimationFini -= new EventHandler(OnPicAnimationFini);
            if (!animation.Loop)
                state = RenderChunkState.Hide;
            if (OnAnimationFini != null)
                OnAnimationFini(this, new EventArgs());
        }
    }
}
