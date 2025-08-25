namespace SnapRead
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            SetTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        public void SetTheme()
        {
            if (Preferences.Get("DarkMode", false))
            {
                // Dark mode renkleri
                Resources["ModeColor"] = Color.FromArgb("#212121");
                Resources["TextColor"] = Colors.White;
            }
            else
            {
                // Light mode renkleri
                Resources["ModeColor"] = Colors.White;
                Resources["TextColor"] = Colors.Black;
            }
        }
    }
}