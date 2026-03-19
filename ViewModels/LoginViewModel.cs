using System;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using TadidyVeGame.Services;

namespace TadidyVeGame.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _username = "";
    private string _password = "";
    private string _errorMessage = "";
    private string _successMessage = "";
    private bool _hasError = false;
    private bool _hasSuccess = false;
    private bool _isLoading = false;
    private readonly MainViewModel _mainNav;
    private readonly AuthService _auth;

    public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
    public string ErrorMessage { get => _errorMessage; set => this.RaiseAndSetIfChanged(ref _errorMessage, value); }
    public string SuccessMessage { get => _successMessage; set => this.RaiseAndSetIfChanged(ref _successMessage, value); }
    public bool HasError { get => _hasError; set => this.RaiseAndSetIfChanged(ref _hasError, value); }
    public bool HasSuccess { get => _hasSuccess; set => this.RaiseAndSetIfChanged(ref _hasSuccess, value); }
    public bool IsLoading { get => _isLoading; set => this.RaiseAndSetIfChanged(ref _isLoading, value); }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToRegisterCommand { get; }

    public LoginViewModel(MainViewModel mainNav, AuthService auth)
    {
        _mainNav = mainNav;
        _auth = auth;
        LoginCommand = ReactiveCommand.CreateFromTask(ExecuteLogin);
        NavigateToRegisterCommand = ReactiveCommand.Create(() => _mainNav.NavigateToRegister());
    }

    private async Task ExecuteLogin()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Veuillez remplir tous les champs");
            return;
        }

        IsLoading = true;
        HasError = false;
        HasSuccess = false;
        
        try
        {
            var result = await _auth.LoginAsync(Username, Password);
            if (result)
            {
                ShowSuccess("Connexion réussie!");
                await Task.Delay(500);
                _mainNav.NavigateToGame(false); // Go to game, not menu
            }
            else
            {
                ShowError("Identifiants invalides. Vérifiez votre nom et mot de passe.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur de connexion: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
        HasSuccess = false;
    }

    private void ShowSuccess(string message)
    {
        SuccessMessage = message;
        HasSuccess = true;
        HasError = false;
    }
}