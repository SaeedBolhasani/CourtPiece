namespace CourtPiece.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                //MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Error");
                MainPage = new AppShell();

            };
            MainPage = new AppShell();
        }
      
    }
}
