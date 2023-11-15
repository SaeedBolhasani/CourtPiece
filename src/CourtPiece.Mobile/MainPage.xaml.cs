namespace CourtPiece.Mobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.Stack.Children.Add(new MainView());
            this.Stack.Children.Add(new MainView());
            this.Stack.Children.Add(new MainView());
            this.Stack.Children.Add(new MainView());
        }

       
    }

}
