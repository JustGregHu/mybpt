using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MyBPT.Classes
{
    class CountDown
    {
        int timeleft;
        Timer timer;
        public CountDown()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 1000;
            timer.Enabled = true;
        }

        public int Timeleft { get => timeleft; set => timeleft = value; }

        public void StartTimer(int timeleft)
        {
            this.timeleft = timeleft-1;
            timer.Start();
        }

        public void StopTimer()
        {
            timer.Stop();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.timeleft--;
        }

    }
}