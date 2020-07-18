using Android.App;
using Android.Content.PM;

namespace EventRegistrar.OrganizerApp.Droid
{
    //const string CALLBACK_SCHEME = "eventregistrar";

    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Android.Content.Intent.ActionView },
                          Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
                          DataScheme = "com.eventregistrar.organizerapp")]
    public class WebAuthenticationCallbackActivity : Xamarin.Essentials.WebAuthenticatorCallbackActivity
    {
    }
}