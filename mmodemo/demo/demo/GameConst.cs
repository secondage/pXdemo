﻿using System;
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
        static private float backgroundScale = 3.0f;
        static private float distanceFactor = 15.0f;
        static private Vector2 nametitleoffset = new Vector2(0, 7.0f);
        static private GraphicsDeviceManager graphics;
        static private ContentManager content;
        static private SpriteFont currentfont;
        static private GameWindow currentgamewindow;
        static private int fixedenemynum = -1;

        static public float PlayerSpeed = 300;
        static public int PlayerAtk = 100;
        static public int PlayerHP = 200;
        static public int RenderCountPerFrame = 0;

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
