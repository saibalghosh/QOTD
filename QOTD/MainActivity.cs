using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Net.Http;
using Android.Net;
using System.Threading;
using Android.Content.Res;
using System.Text;
using System.IO;

namespace QOTD
{
    [Activity(Icon = "@drawable/icon")]
    public class MainActivity : BaseActivity, View.IOnTouchListener
    {
        View licenseDetails;

        private static readonly EndpointAddress endPoint = new EndpointAddress("https://qotdservice.azurewebsites.net/RandomQuotation.svc");
        private QOTDService.BasicHttpsBinding_IRandomQuotation serviceRef;
        private static readonly string imagePathPrefix = "https://qotdservice.azurewebsites.net/images/";
        protected override int LayoutResource
        {
            get { return Resource.Layout.main; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.main);

            RelativeLayout layoutTouch = FindViewById<RelativeLayout>(Resource.Id.relativeLayoutMain);
            layoutTouch.SetOnTouchListener (this);

            SetupTypefaces();

            ImageView vwInfo = FindViewById<ImageView>(Resource.Id.infoButton);
            vwInfo.Click += VwInfo_Click;

            InitializeQOTDServiceClient();
            GetRandomQuotation();
        }

        private void VwInfo_Click(object sender, EventArgs e)
        {
            View dialogAbout = this.LayoutInflater.Inflate(Resource.Layout.about, null);

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetView(dialogAbout);

            Button btnLicense = dialogAbout.FindViewById<Button>(Resource.Id.btnAboutLicenses);
            btnLicense.Click += BtnLicense_Click;

            TextView tvAboutHeader = dialogAbout.FindViewById<TextView>(Resource.Id.txtAboutTitle);
            TextView tvAboutVersion = dialogAbout.FindViewById<TextView>(Resource.Id.txtAboutVersion);
            TextView tvAboutCopyleft = dialogAbout.FindViewById<TextView>(Resource.Id.txtAboutCopyleft);
            TextView tvAboutRights = dialogAbout.FindViewById<TextView>(Resource.Id.txtAboutRights);
            TextView tvAboutIcons = dialogAbout.FindViewById<TextView>(Resource.Id.txtIcons);
            TextView tvAboutQuill = dialogAbout.FindViewById<TextView>(Resource.Id.txtQuill);
            TextView tvAboutInfo = dialogAbout.FindViewById<TextView>(Resource.Id.txtInfo);
            TextView tvAboutTangerine = dialogAbout.FindViewById<TextView>(Resource.Id.txtTangerine);
            TextView tvAboutCinzel = dialogAbout.FindViewById<TextView>(Resource.Id.txtCinzel);
            TextView tvAboutOpenSans = dialogAbout.FindViewById<TextView>(Resource.Id.txtOpenSans);

            Typeface tfAboutTitle = Typeface.CreateFromAsset(this.ApplicationContext.Assets, "fonts/Tangerine_Regular.ttf");
            Typeface tfAboutTexts = Typeface.CreateFromAsset(this.ApplicationContext.Assets, "fonts/OpenSans-Regular.ttf");

            tvAboutHeader.Typeface = tfAboutTitle;
            tvAboutVersion.Typeface = tfAboutTexts;
            tvAboutCopyleft.Typeface = tfAboutTexts;
            tvAboutRights.Typeface = tfAboutTexts;
            tvAboutIcons.Typeface = tfAboutTexts;
            tvAboutQuill.Typeface = tfAboutTexts;
            tvAboutInfo.Typeface = tfAboutTexts;
            tvAboutTangerine.Typeface = tfAboutTexts;
            tvAboutCinzel.Typeface = tfAboutTexts;
            tvAboutOpenSans.Typeface = tfAboutTexts;

            builder.SetPositiveButton("OK", HandleAboutDismissal);
            builder.Show();
        }

        private void BtnLicense_Click(object sender, EventArgs e)
        {
            licenseDetails = this.LayoutInflater.Inflate(Resource.Layout.license, null);

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetView(licenseDetails);

            TextView tvLicenseDetails = licenseDetails.FindViewById<TextView>(Resource.Id.txtLicenseDetails);

            RunOnUiThread(() => tvLicenseDetails.SetText(Resource.String.license_loader));


            builder.SetTitle(Resource.String.about_license);
            builder.SetPositiveButton("OK", HandleAboutDismissal);

            builder.Create();
            builder.Show();

            ThreadPool.QueueUserWorkItem(o => RenderLicenseInfo());

        }

        private async void RenderLicenseInfo()
        {
            string licInfo;

            licInfo = await ReadLicenseInfo();
            TextView tvLicenseDetails = licenseDetails.FindViewById<TextView>(Resource.Id.txtLicenseDetails);
            RunOnUiThread(() => tvLicenseDetails.Typeface = Typeface.Monospace);
            RunOnUiThread(() => tvLicenseDetails.Text = "");
            RunOnUiThread(() => tvLicenseDetails.Text = licInfo);

        }

        private async Task<String> ReadLicenseInfo()
        {
            AssetManager assets = this.Assets;
            StringBuilder licenseInfo = new StringBuilder();

            using (StreamReader sr = new StreamReader(assets.Open("licenses.txt")))
            {
                licenseInfo.Append(await sr.ReadToEndAsync());
            }
            return licenseInfo.ToString();
        }


        private void HandleAboutDismissal(object sender, EventArgs e)
        {

        }


        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    GetRandomQuotation();
                    break;
            }

            return true;
        }

        private void SetupTypefaces()
        {
            var txtPerson = FindViewById<TextView>(Resource.Id.txtPersona);
            var txtQuotation = FindViewById<TextView>(Resource.Id.txtAphorism);
            var txtCopyright = FindViewById<TextView>(Resource.Id.textCopyright);
            var txtInstructions = FindViewById<TextView>(Resource.Id.textInstructions);
            txtQuotation.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();

            Typeface aphorismTypeface = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/Tangerine_Regular.ttf");
            Typeface personaTypeface = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/Cinzel-Regular.ttf");
            Typeface copyrightTypeface = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/OpenSans-Regular.ttf");
            Typeface instructionsTypeface = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/OpenSans-Regular.ttf");
            txtQuotation.SetTextSize(Android.Util.ComplexUnitType.Dip, 30);
            txtQuotation.SetTypeface(aphorismTypeface, TypefaceStyle.Normal);
            txtPerson.SetTypeface(personaTypeface, TypefaceStyle.Bold);
            txtCopyright.SetTypeface(copyrightTypeface, TypefaceStyle.Normal);
            txtInstructions.SetTypeface(instructionsTypeface, TypefaceStyle.Normal);
        }

        private void GetRandomQuotation()
        {
            RunOnUiThread(() => FindViewById<ImageView>(Resource.Id.imgPersona).Visibility = ViewStates.Invisible);
            

            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;

            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                RunOnUiThread(() => FindViewById<TextView>(Resource.Id.txtAphorism).Text = "Retrieving...");
                RunOnUiThread(() => FindViewById<TextView>(Resource.Id.txtPersona).Text = "");

                serviceRef.GetRandomQuotationAsync();
            }
            else
            {
                RunOnUiThread(() => FindViewById<TextView>(Resource.Id.txtAphorism).Text = "");
                RunOnUiThread(() => FindViewById<TextView>(Resource.Id.txtPersona).Text = "");
                RunOnUiThread(() => FindViewById<ImageView>(Resource.Id.imgPersona).Visibility = ViewStates.Invisible);

                RunOnUiThread(() => Toast.MakeText(this, "No Network connectivity available. Connect to a network and tap anywhere to continue...", ToastLength.Long).Show());
            }
        }

        private void InitializeQOTDServiceClient()
        {
            try
            {
                BasicHttpsBinding binding = CreateBasicHttpsBinding();

                serviceRef = new QOTDService.BasicHttpsBinding_IRandomQuotation();
                serviceRef.GetRandomQuotationCompleted += ServiceRef_GetRandomQuotationCompleted;
            }

            catch
            {
                RunOnUiThread(() => Toast.MakeText(this, "Failed to establish a network connection. Please try again...", ToastLength.Long).Show());
                return;
            }
        }

        private void ServiceRef_GetRandomQuotationCompleted(object sender, QOTDService.GetRandomQuotationCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RunOnUiThread(() => Toast.MakeText(this, "There was an error retrieving the data. Please try again...", ToastLength.Long).Show());
                return;
            }
            else if (e.Cancelled)
            {
                RunOnUiThread(() => Toast.MakeText(this, "The retrieval operation was cancelled  by the user", ToastLength.Long).Show());
                return;
            }

            TextView txtQuote = FindViewById<TextView>(Resource.Id.txtAphorism);
            TextView txtAuthor = FindViewById<TextView>(Resource.Id.txtPersona);

            RunOnUiThread(() => txtQuote.Text = e.Result[0][0].ToString());
            RunOnUiThread(() => txtAuthor.Text = e.Result[0][1].ToString());
            RunOnUiThread(() => renderAuthorImage(imagePathPrefix + e.Result[0][2].ToString() + ".jpg"));
        }

        private async void renderAuthorImage(string uri)
        {
            var authorImage = FindViewById<ImageView>(Resource.Id.imgPersona);
            using (var bitmap = await GetAuthorImageFromUrl(uri))
                authorImage.SetImageBitmap(bitmap);

            FindViewById<ImageView>(Resource.Id.imgPersona).Visibility = ViewStates.Visible;
        }

        private async Task<Bitmap> GetAuthorImageFromUrl(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var message = await httpClient.GetAsync(url);
                if (message.IsSuccessStatusCode)
                {
                    using (var imageStream = await message.Content.ReadAsStreamAsync())
                    {
                        var bitmap = await BitmapFactory.DecodeStreamAsync(imageStream);
                        return bitmap;
                    }
                }
            }
            return null;
        }

        private static BasicHttpsBinding CreateBasicHttpsBinding()
        {
            BasicHttpsBinding binding = new BasicHttpsBinding
            {
                Name = "basicHttpsBinding",
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647

            };

            TimeSpan timeout = new TimeSpan(0, 0, 30);
            binding.SendTimeout = timeout;
            binding.OpenTimeout = timeout;
            binding.ReceiveTimeout = timeout;
            return binding;

        }
    }
}