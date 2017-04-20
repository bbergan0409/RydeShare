using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Android.Content.PM;
using Android;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Plugin.Geolocator;


namespace Ryde_Android
{
    public class LocationProvider : Activity, ActivityCompat.IOnRequestPermissionsResultCallback
    {

        string locationLong = string.Empty;
        string locationLat = string.Empty;


        public async Task<string> getCurrentLocationAsync()
        {
            await GetLocationAsync();

            return locationLat + "," + locationLong;
        }


        async Task GetLocationAsync()
        {
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;
                var position = await locator.GetPositionAsync(20000);

                locationLat = position.Latitude.ToString();
                locationLong = position.Longitude.ToString();
            }
            catch (Exception ex)
            {

            }
        }
    }
}