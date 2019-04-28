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
    /// Képernyőbillentyűzet létrehozásához szükséges billenytűelem.
    /// </summary>
    public class KeyboardKeys
    {
        Button button;
        char key;

        public char Key { get => key; set => key = value; }
        internal Button Button { get => button; set => button = value; }

        /// <summary>
        /// Létrehoz egy billentyűt
        /// </summary>
        /// <param name="button">A látható, lenyomható billenyűt képviselő gombelem</param>
        /// <param name="key">Később ezzel a karakterrel tér az objektum vissza.</param>
        public KeyboardKeys(Button button, char key)
        {
            this.button = button;
            this.key = key;
        }
    }
}