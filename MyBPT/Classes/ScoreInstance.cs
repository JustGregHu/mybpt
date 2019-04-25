using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MyBPT.Classes
{
    class ScoreInstance
    {
        int amount;
        string playername;

        public ScoreInstance(int amount, string playername)
        {
            this.amount = amount;
            this.playername = playername;
        }

        public int Amount { get => amount; set => amount = value; }
        public string Playername { get => playername; set => playername = value; }
    }
}