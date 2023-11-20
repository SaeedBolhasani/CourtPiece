using CourtPiece.Common.Model;

namespace CourtPiece.Mobile;

public partial class MainView : ContentView, IDisposable
{
    private readonly IServiceScope scope;
    private readonly PlayerService playerService;

     

    public MainView()
    {
        InitializeComponent();

        scope = Application.Current.MainPage
                .Handler
                .MauiContext
                .Services
                .CreateScope();



        this.playerService = scope.ServiceProvider.GetService<PlayerService>();

        playerService.OnCardReceived += (s, cards) =>
        {
            Dispatcher.Dispatch(() =>
            {
                this.Images.Children.Clear();
            });

            DisplayCards(cards);
        };

        playerService.OnMessageReceived += (s, message) =>
        {
            Dispatcher.Dispatch(() =>
            {
                MessageLabel.Text = message;
                var d = this.Images.Children.Cast<ImageButton>().FirstOrDefault(i => i.StyleId == message);

                if (d != null)
                    this.Images.Children.Remove(d);
            });
        };
    }

    private void DisplayCards(Card[] cards)
    {
        foreach (var card in cards.OrderBy(i=>i.Type).ThenBy(i=>i.Value))
        {
            var i = new ImageButton();
            i.BatchBegin();
            i.Source = ImageSource.FromFile(card.Type.ToString().ToLower()[0].ToString() + card.Value + ".jpg");
            i.BatchCommit();
            i.StyleId = card;
            i.MaximumWidthRequest = 50;
            i.MaximumHeightRequest = 80;
            i.Clicked += ImageButton_Clicked;
            Dispatcher.Dispatch(() =>
            {
                this.Images.Children.Add(i);
            });
        }
    }
    private Guid GetRoomId()
    {
        return Guid.Parse(RoomEntry.Text);
    }
    private async void OnCounterClicked(object sender, EventArgs e)
    {
        await playerService.Join(int.Parse(NameEntry.Text));
    }

    private void GuidButton_Clicked(object sender, EventArgs e)
    {
        NameEntry.Text = Guid.NewGuid().ToString();
        SemanticScreenReader.Announce(NameEntry.Text);
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender;
        Card card = button.StyleId;
        //button.ScaleTo(1.2);
        //SemanticScreenReader.Announce(button);
        await playerService.Action(card);
    }

    public void Dispose()
    {
        scope.Dispose();
    }
}