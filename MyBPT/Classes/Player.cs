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
        int money;
        int level;

        public Player(string name)
        {
            this.name = name;
        }

        public Player(string name, int money, int level) : this(name)
        {
            this.money = money;
            this.level = level;
        }

        public void UpdateLevel(int terminuscount)
        {
            if (terminuscount>0)
            {
                level = terminuscount;
            }
        }

        public void AddMoney(int newmoney)
        {
            money += newmoney;
            if (money<0)
            {
                money = 0;
            }
        }

        public bool CanAfford(int cost)
        {
            if (cost <= Money)
            {
                return true;
            }
            return false;
        }

        public string Name { get => name; set => name = value; }
        public int Money { get => money; set => money = value; }
        public int Level { get => level; set => level = value; }
    }
}