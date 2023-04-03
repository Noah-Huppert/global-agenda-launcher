using GlobalAgendaLauncher.Controllers;
using System.Diagnostics;

namespace GlobalAgendaLauncher;

public partial class MainPage : ContentPage
{
    public MainPage()
	{
		InitializeComponent();

		this.Loaded += this.OnLoaded;
	}

	/// <summary>
	/// Retrieves and populates any stored username or password values from secure storage.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private async void OnLoaded(object? sender, EventArgs e)
	{
		await AppSettings.Instance.Load();

		// Get remembered login values
		var storedUsername = await AppSettings.Instance.Username.GetValue();
		var storedPassword = await AppSettings.Instance.Password.GetValue();

		if (storedUsername is not null)
		{
			Username.Text = storedUsername;
		}

		if (storedPassword is not null)
		{
			Password.Text = storedPassword;
		}

		// Get remembered Global Agenda binary location
		var storedGlobalAgendaBinaryLocation = await AppSettings.Instance.GABinaryPath.GetValue();
		if (storedGlobalAgendaBinaryLocation is not null)
		{
			GlobalAgendaBinaryLocation.Text = storedGlobalAgendaBinaryLocation;
		}
    }

	/// <summary>
	/// Run when the launch button is clicked. Validates the form, saves values, then launches the game.
	/// The username will always be saved. The password will only be saved if the user enables the save password checkbox.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private async void OnLaunchClicked(object sender, EventArgs e)
	{
		// Check required form values
		var missingFields = new List<string>();
		if (Username.Text is null || Username.Text.Length == 0)
		{
			missingFields.Add("Username");
		}

		if (Password.Text is null || Password.Text.Length == 0)
		{
			missingFields.Add("Password");
		}

		if (missingFields.Count > 0)
		{
			await DisplayAlert("Missing Hi-Rez Login Information", String.Format("You must enter your: {0}", string.Join(", ", missingFields)), "Okay");
			return;
		}

		// Save login form values
		AppSettings.Instance.Username.Value = Username.Text;
		await AppSettings.Instance.Username.SaveValue();

		if (SavePassword.IsChecked)
		{
			AppSettings.Instance.Password.Value = Password.Text;
			await AppSettings.Instance.Password.SaveValue();
		}
	}

	/// <summary>
	/// Run when the label for the save password checkbox is clicked. Toggled the checkbox.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
    private void SavePasswordCheckBoxLabel_Tapped(object sender, TappedEventArgs e)
    {
		SavePassword.IsChecked = !SavePassword.IsChecked;
    }

	/// <summary>
	/// Run when the select file button is clicked next to the GA binary path input.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
    private async void SelectGobalAgendaBinaryLocation_Clicked(object sender, EventArgs e)
    {
		PickOptions pickOpts = new()
		{
			PickerTitle = "Select your Global Agenda Binary",
			FileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".exe" } }, // file extension
                }),
    };
		var file = await FilePicker.Default.PickAsync(pickOpts);
		if (file is null) {
			// File picker canceled
			return;
		}
		GlobalAgendaBinaryLocation.Text = file.FullPath;

		AppSettings.Instance.GABinaryPath.Value = file.FullPath;
		await AppSettings.Instance.GABinaryPath.SaveValue();
    }
}

