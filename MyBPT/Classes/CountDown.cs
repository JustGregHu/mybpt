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
    /// <summary>
    /// Visszaszámláló objektum (Timer használatával)
    /// </summary>
    public class CountDown
    {
        //Változók
        int timeleft;
        Timer timer;

        /// <summary>
        /// Létrehoz egy visszaszámlálót és beállítja a hozzá tartozó időközt 1mp-re
        /// </summary>
        public CountDown()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 1000;
            timer.Enabled = true;
        }

        /// <summary>
        /// Vissaadja a hátralévő másodpercek számát, illetve az rajta keresztül meg is adható
        /// </summary>
        public int Timeleft { get => timeleft; set => timeleft = value; }

        /// <summary>
        /// Elindítja az időzítőt a megadott hátralévő másodpercek értévékvel
        /// </summary>
        /// <param name="timeleft">Hátralévő másodpercek száma</param>
        public void StartTimer(int timeleft)
        {
            this.timeleft = timeleft-1;
            timer.Start();
        }

        /// <summary>
        /// Leállítja a visszaszámlállst
        /// </summary>
        public void StopTimer()
        {
            timer.Stop();
        }

        /// <summary>
        /// Az időközönként előforduló funkciós
        /// </summary>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.timeleft--;
        }

    }
}