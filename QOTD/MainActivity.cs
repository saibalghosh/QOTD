using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace QOTD
{
    [Activity(MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : BaseActivity
    {

        protected override int LayoutResource
        {
            get { return Resource.Layout.main; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            //SupportActionBar.SetHomeButtonEnabled(false);
        }
    }
}

