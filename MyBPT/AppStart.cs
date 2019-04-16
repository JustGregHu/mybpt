using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MyBPT.Classes;

namespace MyBPT
{
    [Activity(Label = "MyBPT"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.Landscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout)]
    public class AppStart : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var g = new GameSession();
            SetContentView((View)g.Services.GetService(typeof(View)));
            g.Run();
        }
    }
}

