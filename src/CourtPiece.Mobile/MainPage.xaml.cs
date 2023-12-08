namespace CourtPiece.Mobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.Stack.Children.Add(new BoardView());
            this.Stack.Children.Add(new BoardView());
            this.Stack.Children.Add(new BoardView());
            this.Stack.Children.Add(new BoardView());
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
         
            foreach (BoardView item in Stack.Children)
            {
                item.Dispose();
            }
            this.Stack.Children.Clear();
            this.Stack.Children.Add(new BoardView());
            this.Stack.Children.Add(new BoardView());
            this.Stack.Children.Add(new BoardView());
            this.Stack.Children.Add(new BoardView());
        }
    }

}
