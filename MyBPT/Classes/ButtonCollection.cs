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
    class ButtonCollection {
        Dictionary<string, Button> collection = new Dictionary<string, Button>();
        public Dictionary<string, Button> GetButtons() {
            return collection;
        }

        public void AddTexture(string newtbuttonid, Button newbutton) {
            collection.Add(newtbuttonid, newbutton);
        }
    }
}