﻿using System;
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
    public class Background : RenderChunk
    {
        [Flags]
        public enum AutoScrollDirectionType
        {
            Vertical = 1,
            Horizontal = 2,
        };


        private Vector2 autoscrollspeed = new Vector2();
        public Vector2 AutoScrollSpeed
        {
            get
            {
                return autoscrollspeed;
            }
            set
            {
                autoscrollspeed = value;
            }
        }

        private bool autoscrollloop = false;
        public bool AutoScrollLoop
        {
            get
            {
                return autoscrollloop;
            }
            set
            {
                autoscrollloop = value;
            }
        }

        private bool autoscroll = false;
        public bool AutoScroll
        {
            get
            {
                return autoscroll;
            }
            set
            {
                autoscroll = value;
            }
        }

        private Rectangle scroll = new Rectangle(0, 0, GameConst.ScreenWidth, GameConst.ScreenHeight);
        private AutoScrollDirectionType autoscrolldir = AutoScrollDirectionType.Vertical;
        public AutoScrollDirectionType AutoScrollDirection
        {
            get
            {
                return autoscrolldir;
            }
            set
            {
                autoscrolldir = value;
            }
        }

        public Background(Texture2D intexture, int inlayer):
                    base(intexture, inlayer)
        {

        }

        /// <summary>
        /// 返回背景的当前大小，相当于原始纹理尺寸乘上缩放Size
        /// </summary>
        public Vector2 ScreenSize
        {
            get
            {
                return new Vector2((float)Texture.Width * Size.X, (float)Texture.Height * Size.Y);
            }
        }


        Vector2 _movement = new Vector2();
        public override void Update(GameTime gametime)
        {
            if (autoscroll)
            {
                _movement += autoscrollspeed * (float)gametime.ElapsedGameTime.TotalSeconds;
                if (autoscrolldir.HasFlag(AutoScrollDirectionType.Vertical))
                {
                    if (_movement.Y > 1.0f || _movement.Y < -1.0f)
                    {
                        scroll.Y += (int)_movement.Y;
                        _movement.Y = 0;
                    }
                }
                if (autoscrolldir.HasFlag(AutoScrollDirectionType.Horizontal))
                {
                    if (_movement.X > 1.0f || _movement.X < -1.0f)
                    {
                        scroll.X += (int)_movement.X;
                        _movement.X = 0;
                    }
                }

                if (!autoscrollloop)
                {
                    if (scroll.X + GameConst.ScreenWidth >= texture.Width)
                    {
                        scroll.X = texture.Width - GameConst.ScreenWidth;
                    }
                    if (scroll.Y + GameConst.ScreenHeight >= texture.Height)
                    {
                        scroll.Y = texture.Height - GameConst.ScreenHeight;
                    }
                }
         
            }
            else
            {

            }
            base.Update(gametime);
        }

        public override void Render(SpriteBatch sb)
        {
            //Rectangle dest = new Rectangle(Position.X, Position.Y, Size.X, Size.Y);
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;

            Vector2 pos = Position;
            if (coordsystem == CoordinateSystemType.World)
            {
                pos.X -= Scene.Viewport.X * (1.0f - (float)layer / 5.0f * 0.2f);
                pos.Y -= Scene.Viewport.Y;

            }
            
            if (autoscroll && autoscrollloop)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            }
            if (autoscroll)
                sb.Draw(Texture, pos, scroll, this.Color, 0.0f, Vector2.Zero, Size, SpriteEffects.None, 0.0f);
            else
                sb.Draw(Texture, pos, null, this.Color, 0.0f, Vector2.Zero, Size, SpriteEffects.None, 0.0f);
            if (autoscroll && autoscrollloop)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            }

            base.Render(sb);
        }

       
    }
}
