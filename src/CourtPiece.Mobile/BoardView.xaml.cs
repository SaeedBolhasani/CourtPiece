using CourtPiece.Common.Model;

namespace CourtPiece.Mobile;

public partial class BoardView : ContentView, IDisposable
{
    private readonly IServiceScope scope;
    private readonly PlayerService playerService;



    public BoardView()
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

        playerService.OnMyTurn += (s, e) =>
        {
            Dispatcher.Dispatch(() =>
            {
                MyTurn.IsVisible = true;
            });
        };
    }

    private void DisplayCards(Card[] cards)
    {
        foreach (var card in cards.OrderBy(i => i.Type).ThenBy(i => i.Value))
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

    private async void OnJoinToRandomRoom(object sender, EventArgs e)
    {
        await playerService.Join(NameEntry.Text);
    }

    private void GuidButton_Clicked(object sender, EventArgs e)
    {
        NameEntry.Text = Guid.NewGuid().ToString();
        SemanticScreenReader.Announce(NameEntry.Text);
    }

    ImageButton selectedCard = null;
    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender;
        Card card = button.StyleId;

        if (selectedCard == null)
        {
            button.ScaleTo(1.2);
            selectedCard = button;
        }
        else if (button != selectedCard)
        {
            selectedCard.ScaleTo(1);
            selectedCard = button;
            button.ScaleTo(1.2);
        }
        else
        {

            //button.ScaleTo(1.2);
            //SemanticScreenReader.Announce(button);
            await playerService.Action(card);
            Dispatcher.Dispatch(() =>
            {
                MyTurn.IsVisible = false;
            });

        }
    }

    public void Dispose()
    {
        scope.Dispose();
    }
}