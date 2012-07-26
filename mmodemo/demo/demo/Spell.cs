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
    public class Spell : Character
    {
        public override bool NeedTitle
        {
            get
            {
                return false;
            }
        }


        protected override void UpdateMovement(GameTime gametime)
        {
            Vector2 dir = target - position;
            if (dir.Length() < GameConst.DistanceFactor)
            {
                position = target;
                State = CharacterState.Landing;
                if (facedirmethod == DirMethod.AutoDectect)
                    pic.Direction = new Vector2(dir.X >= 0 ? 1 : -1, 0); //停止时面向移动方向
                else if (facedirmethod == DirMethod.Fixed)
                {
                    pic.Direction = fixedfacedir;
                }
                SendOnArrived(this);
            }
            else
            {
                dir.Normalize();
                position += ((float)gametime.ElapsedGameTime.TotalSeconds * speed) * dir;
                pic.Position = position;// - pic.FrameSize * 0.5f;
            }
            base.UpdateMovement(gametime);
        }
    }
}
