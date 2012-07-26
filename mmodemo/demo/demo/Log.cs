using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace demo
{
    public static class Log
    {
        public static void WriteLine(string line)
        {
            Debug.WriteLine(line);
        }
    }
}
