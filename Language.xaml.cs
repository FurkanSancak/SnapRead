using System.Globalization;

namespace SnapRead;

public partial class Language : ContentPage
{
	public Language()
	{
		InitializeComponent();
    }

    private void AR(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ar-AR");

        Preferences.Set("AppLanguage", "ar-AR");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void DE(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("de-DE");

        Preferences.Set("AppLanguage", "de-DE");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void ES(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-ES");

        Preferences.Set("AppLanguage", "es-ES");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void FR(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("fr-FR");

        Preferences.Set("AppLanguage", "fr-FR");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void IT(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("it-IT");

        Preferences.Set("AppLanguage", "it-IT");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void KU(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ku-KU");

        Preferences.Set("AppLanguage", "ku-KU");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void PT(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-PT");

        Preferences.Set("AppLanguage", "pt-PT");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void EN(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("");

        Preferences.Set("AppLanguage", "");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void RU(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru-RU");

        Preferences.Set("AppLanguage", "ru-RU");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private void TR(object sender, EventArgs e)
    {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("tr-TR");

        Preferences.Set("AppLanguage", "tr-TR");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.Windows[0].Page = new AppShell();
        });
    }

    private async void Close(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}