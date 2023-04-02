using System.Diagnostics;

namespace GlobalAgendaLauncher;

public partial class MainPage : ContentPage
{
	/// <summary>
	/// The key in preference storage in which the user's login username will be stored.
	/// </summary>
    public const string PREFERENCES_LOGIN_USERNAME_KEY = "username";

	/// <summary>
	/// The key in secure storage in which the user's login password will be stored.
	/// </summary>
	public const string SECURE_STORAGE_LOGIN_PASSWORD_KEY = "password";

	/// <summary>
	/// The key in preference storage where the Global Agenda binary will be stored.
	/// </summary>
	public const string PREFERENCES_GLOBAL_AGENDA_BINARY_LOCATION = "global_agenda_binary_location";

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
        // Get remembered login values
        var storedUsername = Preferences.Default.Get<string?>(PREFERENCES_LOGIN_USERNAME_KEY, null);
		var storedPassword = await SecureStorage.Default.GetAsync(SECURE_STORAGE_LOGIN_PASSWORD_KEY);

		if (storedUsername is not null)
		{
			Username.Text = storedUsername;
		}

		if (storedPassword is not null)
		{
			Password.Text = storedPassword;
		}

		// Get remembered Global Agenda binary location
		var storedGlobalAgendaBinaryLocation = Preferences.Default.Get<string?>(PREFERENCES_GLOBAL_AGENDA_BINARY_LOCATION, null);
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
		Preferences.Default.Set(PREFERENCES_LOGIN_USERNAME_KEY, Username.Text);

		if (SavePassword.IsChecked)
		{
			await SecureStorage.Default.SetAsync(SECURE_STORAGE_LOGIN_PASSWORD_KEY, Password.Text);
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

		Preferences.Default.Set(PREFERENCES_GLOBAL_AGENDA_BINARY_LOCATION, file.FullPath);
    }
}

