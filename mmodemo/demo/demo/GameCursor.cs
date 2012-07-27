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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;

namespace demo
{
    public class GameCursor
    {
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string fileName);

        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(IntPtr cursorHandle);

        [DllImport("user32.dll")]
        public static extern uint DestroyCursor(IntPtr cursorHandle);
        public enum CursorType
        {
            Normal,
            NA,
            Attack,
            Magic,
            Talk,
            Lastword,
        };
        private static string[] cursorfns = new string[(int)CursorType.Lastword];
        private static Cursor[] cursors = new Cursor[(int)CursorType.Lastword];
        private static Cursor curcursor = null;
        private static IntPtr chandle;
        static public void Initialize(IntPtr handle)
        {
            cursorfns[(int)CursorType.Attack] = "cursors/attack.cur";
            cursorfns[(int)CursorType.Normal] = "cursors/normal.cur";
            cursorfns[(int)CursorType.Magic] = "cursors/magic.cur";
            cursorfns[(int)CursorType.Talk] = "cursors/talk.cur";
            cursorfns[(int)CursorType.NA] = "cursors/na.cur";
            chandle = handle;
            for (int i = 0; i < (int)CursorType.Lastword; ++i)
            {
                Cursor myCursor = new Cursor(chandle);
                IntPtr colorCursorHandle = LoadCursorFromFile(cursorfns[i]);
                myCursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, myCursor,
                       new object[] { colorCursorHandle });
                cursors[i] = myCursor;
            }
        }
        static public void SetCursor(CursorType type)
        {
            Control ctrl = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
            if (ctrl != null && curcursor != cursors[(int)type])
            {
                //Cursor myCursor = new Cursor(chandle);
                //IntPtr colorCursorHandle = LoadCursorFromFile(cursors[(int)type]);
                //myCursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, myCursor,
                  //     new object[] { colorCursorHandle });
                ctrl.Cursor = cursors[(int)type];
                curcursor = cursors[(int)type];
            }
        }
    }
}
