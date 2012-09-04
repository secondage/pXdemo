using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace demo
{
    public class GameConst
    {
        static public int ChFrameWidth = 192;
        static private int screenWidth = 1024;
        static private int screenHeight = 768;
        static private float scrollSpeed = 20.0f;
        static private float backgroundScale = 1.5f;
        static private float distanceFactor = 15.0f;
        static private Vector2 nametitleoffset = new Vector2(0, 7.0f);
        static private GraphicsDeviceManager graphics;
        static private ContentManager content;
        static private SpriteFont currentfont;
        static private GameWindow currentgamewindow;
        static private int fixedenemynum = -1;
        //static private Vector2 scale = new Vector2(0.78125f, 0.78125f);
        static private Vector2 scale = new Vector2(1, 1);
        static private Vector2 uiscale =  new Vector2(0.625f, 0.625f);

        static public float PlayerSpeed = 500;
        static public int PlayerAtk = 100;
        static public int PlayerHP = 200;
        static public int RenderCountPerFrame = 0;

        static public double ServerDurationTime = 0;
        static public double ServerTotalTime = 0;

        static public int BossRushMode = 0;
        static public int BossRushMode1Offset = 60;
        static public Type[] SkipRenderTypeList = {/*typeof(Cloud), typeof(Background), typeof(CharacterPic),
                                                    typeof(CharacterTitle), typeof(HoverStone)*/ };

        static private float _viewportScrollRange = 300;


        static public int FixedEnemyNum
        {
            get
            {
                return fixedenemynum;
            }
            set
            {
                fixedenemynum = value;
            }
        }

        static public ContentManager Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }
        static public GameWindow GameWindow
        {
            get
            {
                return currentgamewindow;
            }
            set
            {
                currentgamewindow = value;
            }
        }
        static public GraphicsDeviceManager Graphics
        {
            get
            {
                return graphics;
            }
            set
            {
                graphics = value;
            }
        }
        static public SpriteFont CurrentFont
        {
            get
            {
                return currentfont;
            }
            set
            {
                currentfont = value;
            }
        }
        static public int ScreenWidth
        {
            get
            {
                return screenWidth;
            }
            set
            {
                screenWidth = value;
            }
        }


        static public int ScreenHeight
        {
            get
            {
                return screenHeight;
            }
            set
            {
                screenHeight = value;
            }
        }

        static public float ViewportScrollRange
        {
            get
            {
                return _viewportScrollRange;
            }
            set
            {
                _viewportScrollRange = value;
            }
        }



        static public Vector2 NameTitleOffset
        {
            get
            {
                return nametitleoffset;
            }
            set
            {
                nametitleoffset = value;
            }
        }

        static public float ScrollSpeed
        {
            get
            {
                return scrollSpeed;
            }
            set
            {
                scrollSpeed = value;
            }
        }

        static public float BackgroundScale
        {
            get
            {
                return backgroundScale;
            }
            set
            {
                backgroundScale = value;
            }
        }

        static public Vector2 ForegroundScale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }


        static public Vector2 UIScale
        {
            get
            {
                return uiscale;
            }
            set
            {
                uiscale = value;
            }
        }

        static public float DistanceFactor
        {
            get
            {
                return distanceFactor;
            }
            set
            {
                distanceFactor = value;
            }
        }
    }
}
