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
      /// <summary>
      /// Egy (adatbázisból származó) pontszám egyedelőfordulásának értékét tárolja. 
      /// </summary>
    class ScoreInstance
    {
        //Tábla oszlopai
        int id;
        int amount;
        string playername;
        
        //Tulajdonságok
        public int Amount { get => amount; set => amount = value; }
        public string Playername { get => playername; set => playername = value; }
        public int Id { get => id; set => id = value; }

        /// <summary>
        /// Létrehoz egy pontszám egyedelőfordulását, és megadja az adatait.
        /// </summary>
        /// <param name="amount">Pontszám</param>
        /// <param name="amount">Pontszám</param>
        /// <param name="id">Egyedelőfordulás azonosítója</param>
        /// <param name="playername">A pontszámot elért játékos neve</param>
        public ScoreInstance(int id, int amount, string playername)
        {
            this.amount = amount;
            this.playername = playername;
            this.id = id;
        }
    }
}