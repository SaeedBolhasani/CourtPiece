namespace CourtPiece.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("register", typeof(Register));
            Routing.RegisterRoute("login", typeof(Login));
        }
    }
}
