using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Views;
using System.Collections.Generic;
using Android.Util;
using AndroidUri = Android.Net.Uri;
using Android.Gms.Common;
using Plugin.Geolocator;
//using Android.Gms.Tasks;
using System;


using System.Threading.Tasks;


namespace Ryde_Android
{
    [Activity(Label = "Ryde_Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : ListActivity
    {
        public static readonly int InstallGooglePlayServicesId = 1000;
        public static readonly string Tag = "MapDemo";

        private List<SampleActivity> _activities;
        private bool _isGooglePlayServicesInstalled;

        TextView locationLat, locationLong;


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch (resultCode)
            {
                case Result.Ok:
                    // Try again.
                    _isGooglePlayServicesInstalled = true;
                    break;

                default:
                    Log.Debug("MainActivity", "Unknown resultCode {0} for request {1}", resultCode, requestCode);
                    break;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _isGooglePlayServicesInstalled = TestIfGooglePlayServicesIsInstalled();
            InitializeListView();
        }

        protected override async void OnListItemClick(ListView l, View v, int position, long id)
        {
            if (position == 0)
            {
                LocationProvider lp = new LocationProvider();
                string currPosition = await lp.getCurrentLocationAsync();

                AndroidUri geoUri = AndroidUri.Parse("geo:" + currPosition);
                Intent mapIntent = new Intent(Intent.ActionView, geoUri);
                StartActivity(mapIntent);
                return;
            }

            SampleActivity activity = _activities[position];
            activity.Start(this);
        }

        async Task GetLocationAsync()
        {

            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;
                var position = await locator.GetPositionAsync(20000);

                locationLat.Text = position.Latitude.ToString();
                locationLong.Text = position.Longitude.ToString();
            }
            catch (Exception ex)
            {
                locationLat.Text = "Unable to get location: " + ex.ToString();
            }
        }

        private void InitializeListView()
        {
            if (_isGooglePlayServicesInstalled)
            {
                _activities = new List<SampleActivity>
                                  {
                                      new SampleActivity(Resource.String.mapsAppText, Resource.String.mapsAppTextDescription, null),
                                      new SampleActivity(Resource.String.activity_label_axml, Resource.String.activity_description_axml, typeof(BasicDemoActivity)),
                                      new SampleActivity(Resource.String.activity_label_locationdemo, Resource.String.activity_description_locationdemo, typeof(BasicLocation)),
                                      new SampleActivity(Resource.String.activity_label_mapwithoverlays, Resource.String.activity_description_mapwithoverlays, typeof(MapWithOverlaysActivity))
                                  };

                ListAdapter = new SimpleMapDemoActivityAdapter(this, _activities);
            }
            else
            {
                Log.Error("MainActivity", "Google Play Services is not installed");
                ListAdapter = new SimpleMapDemoActivityAdapter(this, null);
            }
        }

        private bool TestIfGooglePlayServicesIsInstalled()
        {
            int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info(Tag, "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                string errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error(Tag, "There is a problem with Google Play Services on this device: {0} - {1}", queryResult, errorString);
                Dialog errorDialog = GoogleApiAvailability.Instance.GetErrorDialog(this, queryResult, InstallGooglePlayServicesId);
                ErrorDialogFragment dialogFrag = new ErrorDialogFragment(errorDialog);

                dialogFrag.Show(FragmentManager, "GooglePlayServicesDialog");
            }
            return false;
        }
    }
}

