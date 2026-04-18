using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PlantProduction.Desktop.Laboratory;

public partial class LoginWindow : Window
{
    private const string DefaultApiUrl = "http://localhost:5114";
    private readonly DesktopApiClient _apiClient = new();

    public LoginWindow()
    {
        InitializeComponent();
        _apiClient.SetBaseUrl(DefaultApiUrl);
    }

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var user = await _apiClient.LoginAsync(LoginTextBox.Text ?? string.Empty, PasswordTextBox.Text ?? string.Empty);

            if (!string.Equals(user.RoleCode, "LAB_TECHNICIAN", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(user.RoleCode, "ADMINISTRATOR", StringComparison.OrdinalIgnoreCase))
            {
                StatusTextBlock.Text = "Войдите под учетной записью лаборанта.";
                return;
            }

            var window = new MainWindow(_apiClient, user);
            window.Show();
            Close();
        }
        catch (Exception exception)
        {
            StatusTextBlock.Text = exception.Message;
        }
    }
}
