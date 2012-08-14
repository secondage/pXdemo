using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace demo
{
    public class HiPerfTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        // 构造函数
        public HiPerfTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // 不支持高性能计数器
                throw new Win32Exception();
            }
        }

        // 开始计时器
        public void Start()
        {
            // 来让等待线程工作
            Thread.Sleep(0);

            QueryPerformanceCounter(out startTime);
        }

        // 停止计时器
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        // 返回计时器经过时间(单位：秒)
        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }

        public double GetDuration()
        {
            QueryPerformanceCounter(out stopTime);
            double t = (double)(stopTime - startTime) / (double)freq;
            startTime = stopTime;
            return t;
        }

        public double GetTotalDuration()
        {
            long _s;
            QueryPerformanceCounter(out _s);
            double t = (double)(_s - startTime) / (double)freq;
            return t;
        }
    }
}
