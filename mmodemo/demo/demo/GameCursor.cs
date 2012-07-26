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
        private static string[] cursors = new string[(int)CursorType.Lastword];
        private static string curcursor = "";
        private static IntPtr chandle;
        static public void Initialize(IntPtr handle)
        {
            cursors[(int)CursorType.Attack] = "cursors/attack.cur";
            cursors[(int)CursorType.Normal] = "cursors/normal.cur";
            cursors[(int)CursorType.Magic] = "cursors/magic.cur";
            cursors[(int)CursorType.Talk] = "cursors/talk.cur";
            cursors[(int)CursorType.NA] = "cursors/na.cur";
            chandle = handle;
        }
        static public void SetCursor(CursorType type)
        {
            Control ctrl = System.Windows.Forms.Control.FromHandle(GameConst.GameWindow.Handle);
            if (ctrl != null && curcursor != cursors[(int)type])
            {
                Cursor myCursor = new Cursor(chandle);
                IntPtr colorCursorHandle = LoadCursorFromFile(cursors[(int)type]);
                myCursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, myCursor,
                       new object[] { colorCursorHandle });
                ctrl.Cursor = myCursor;
                curcursor = cursors[(int)type];
            }
        }
    }
}
