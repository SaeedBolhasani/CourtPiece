namespace CourtPiece.Mobile;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
	}

    private void LoginButton_Clicked(object sender, EventArgs e)
    {

    }

    private async void RegisterButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("register");
    }
}