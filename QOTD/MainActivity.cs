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

namespace QOTD
{
    [Activity(MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : BaseActivity, View.IOnTouchListener
    {

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

            InitializeQOTDServiceClient();
            serviceRef.GetRandomQuotationAsync();
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    serviceRef.GetRandomQuotationAsync();
                    break;
            }

            return true;
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
                return;
            }
        }

        private void ServiceRef_GetRandomQuotationCompleted(object sender, QOTDService.GetRandomQuotationCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }
            else if (e.Cancelled)
            {
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