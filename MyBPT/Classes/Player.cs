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

namespace MyBPT.Classes {
    class Player {
        string name;
        int score;
        int money;
        int level;

        public Player(string name)
        {
            this.name = name;
        }

        public Player(string name, int score, int money, int level) : this(name)
        {
            this.score = score;
            this.money = money;
            this.level = level;
        }

        public string Name { get => name; set => name = value; }
        public int Score { get => score; set => score = value; }
        public int Money { get => money; set => money = value; }
        public int Level { get => level; set => level = value; }
    }
}