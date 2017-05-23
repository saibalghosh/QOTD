using System.Threading;
using Android.App;
using Android.OS;

namespace QOTD
{
    [Activity(Theme ="@style/Theme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Thread.Sleep(3000);
            StartActivity(typeof(MainActivity));
        }
    }
}