using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using MyBPT.Classes;

namespace MyBPT
{
    [Activity(Label = "MyBPT"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.ReverseLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout)]
    public class AppStart : Microsoft.Xna.Framework.AndroidGameActivity
    {
        GameSession g;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SQLiteConnectionBuddy.SQLiteConnectionHelper.CopyEmbeddedDatabase("mybpt.db", this);
            g = new GameSession();
            SetContentView((View)g.Services.GetService(typeof(View)));
            g.Run();
        }
    }
}

