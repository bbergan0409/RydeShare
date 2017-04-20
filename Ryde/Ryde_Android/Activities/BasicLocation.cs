using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using System.Threading.Tasks;
using Android.Content.PM;
using Android;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Plugin.Geolocator;

namespace Ryde_Android
{
    [Activity(Label = "Ryde_Android")]
    public class BasicLocation : Activity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        private const int RequestLocationId = 0;
        private readonly string[] PermissionsLocation =
        {
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessMockLocation
        };


        View layout;
        TextView locationText;
        Button buttonGetLocation;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.BasicLocation);

            layout = FindViewById(Resource.Layout.BasicLocation);

            EditText phoneNumberText = FindViewById<EditText>(Resource.Id.PhoneWord);
            Button translateButton = FindViewById<Button>(Resource.Id.ButtonTranslate);
            Button callButton = FindViewById<Button>(Resource.Id.ButtonCall);

            locationText = FindViewById<EditText>(Resource.Id.locationTextView);

            buttonGetLocation = FindViewById<Button>(Resource.Id.ButtonGetLocation);

            buttonGetLocation.Click += async (sender, e) =>
            {
                await TryGetLocationAsync();
            };

            // Disable the "Call" button
            callButton.Enabled = false;

            // Add code to translate number
            string translatedNumber = string.Empty;

            translateButton.Click += (object sender, EventArgs e) =>
            {
                // Translate user's alphanumeric phone number to numeric
                translatedNumber = Ryde_Shared.PhonewordTranslator.ToNumber(phoneNumberText.Text);
                if (String.IsNullOrWhiteSpace(translatedNumber))
                {
                    callButton.Text = "Call";
                    callButton.Enabled = false;
                }
                else
                {
                    callButton.Text = "Call " + translatedNumber;
                    callButton.Enabled = true;
                }
            };


            callButton.Click += (object sender, EventArgs e) =>
            {
                // On "Call" button click, try to dial phone number.
                var callDialog = new AlertDialog.Builder(this);
                callDialog.SetMessage("Call " + translatedNumber + "?");
                callDialog.SetNeutralButton("Call", delegate
                {
                    // Create intent to dial phone
                    var callIntent = new Intent(Intent.ActionDial);
                    callIntent.SetData(Android.Net.Uri.Parse("tel:" + translatedNumber));
                    StartActivity(callIntent);

                });
                callDialog.SetNegativeButton("Cancel", delegate { });
                // Show the alert dialog to the user and wait for response.
                callDialog.Show();
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }





        async Task TryGetLocationAsync()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                await GetLocationAsync();
                return;
            }

            await GetLocationPermissionAsync();
        }
        async Task GetLocationPermissionAsync()
        {
            //Check to see if any permission in our group is available, if one, then all are
            const string permission = Manifest.Permission.AccessFineLocation;
            if (CheckSelfPermission(permission) == (int)Permission.Granted)
            {
                await GetLocationAsync();
                return;
            }

            //need to request permission
            if (ShouldShowRequestPermissionRationale(permission))
            {
                //Explain to the user why we need to read the contacts
                Snackbar.Make(layout, "Location access is required to show coffee shops nearby.", Snackbar.LengthIndefinite)
                        .SetAction("OK", v => RequestPermissions(PermissionsLocation, RequestLocationId))
                        .Show();
                return;
            }
            //Finally request permissions with the list of permissions and Id
            RequestPermissions(PermissionsLocation, RequestLocationId);
        }
        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            //Permission granted
                            var snack = Snackbar.Make(layout, "Location permission is available, getting lat/long.", Snackbar.LengthShort);
                            snack.Show();

                            await GetLocationAsync();
                        }
                        else
                        {
                            //Permission Denied :(
                            //Disabling location functionality
                            var snack = Snackbar.Make(layout, "Location permission is denied.", Snackbar.LengthShort);
                            snack.Show();
                        }
                    }
                    break;
            }
        }
        async Task GetLocationAsync()
        {
            locationText.Text = "Getting Location";
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;
                var position = await locator.GetPositionAsync(20000);

                locationText.Text = string.Format("Lat: {0}  Long: {1}", position.Latitude, position.Longitude);
            }
            catch (Exception ex)
            {
                locationText.Text = "Unable to get location: " + ex.ToString();
            }
        }
        async Task GetLocationCompatAsync()
        {
            const string permission = Manifest.Permission.AccessFineLocation;

            if (ContextCompat.CheckSelfPermission(this, permission) == (int)Permission.Granted)
            {
                await GetLocationAsync();
                return;
            }

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, permission))
            {
                //Explain to the user why we need to read the contacts
                Snackbar.Make(layout, "Location access is required to show coffee shops nearby.",
                    Snackbar.LengthIndefinite)
                    .SetAction("OK", v => RequestPermissions(PermissionsLocation, RequestLocationId))
                    .Show();

                return;
            }

            RequestPermissions(PermissionsLocation, RequestLocationId);
        }
    }
}