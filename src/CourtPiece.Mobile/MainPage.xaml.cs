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

        private void Button_Clicked(object sender, EventArgs e)
        {
            foreach (MainView item in Stack.Children)
            {
                item.Dispose();
            }
            this.Stack.Children.Clear();
            this.Stack.Children.Add(new MainView());
            this.Stack.Children.Add(new MainView());
            this.Stack.Children.Add(new MainView());
            this.Stack.Children.Add(new MainView());
        }
    }

}
