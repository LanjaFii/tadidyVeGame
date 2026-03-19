using System;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using TadidyVeGame.Services;
using TadidyVeGame.Models;

namespace TadidyVeGame.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private string _username = "";
    private string _password = "";
    private string _confirmPassword = "";
    private string _errorMessage = "";
    private string _successMessage = "";
    private bool _hasError = false;
    private bool _hasSuccess = false;
    private bool _isLoading = false;
    private readonly MainViewModel _mainNav;
    private readonly ApiService _api;

    public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
    public string ConfirmPassword { get => _confirmPassword; set => this.RaiseAndSetIfChanged(ref _confirmPassword, value); }
    public string ErrorMessage { get => _errorMessage; set => this.RaiseAndSetIfChanged(ref _errorMessage, value); }
    public string SuccessMessage { get => _successMessage; set => this.RaiseAndSetIfChanged(ref _successMessage, value); }
    public bool HasError { get => _hasError; set => this.RaiseAndSetIfChanged(ref _hasError, value); }
    public bool HasSuccess { get => _hasSuccess; set => this.RaiseAndSetIfChanged(ref _hasSuccess, value); }
    public bool IsLoading { get => _isLoading; set => this.RaiseAndSetIfChanged(ref _isLoading, value); }

    public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> BackToLoginCommand { get; }

    public RegisterViewModel(MainViewModel mainNav, ApiService api)
    {
        _mainNav = mainNav;
        _api = api;
        RegisterCommand = ReactiveCommand.CreateFromTask(ExecuteRegister);
        BackToLoginCommand = ReactiveCommand.Create(() => _mainNav.NavigateToLogin(false));
    }

    private async Task ExecuteRegister()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ShowError("Veuillez remplir tous les champs");
            return;
        }

        if (Password != ConfirmPassword)
        {
            ShowError("Les mots de passe ne correspondent pas");
            return;
        }

        if (Password.Length < 6)
        {
            ShowError("Le mot de passe doit avoir au moins 6 caractères");
            return;
        }

        IsLoading = true;
        HasError = false;
        HasSuccess = false;

        try
        {
            var registerRequest = new RegisterRequest(Username, Password, "Nouveau joueur", "default.png");
            var res = await _api.PostAsync("auth/register", registerRequest);
            
            if (res.IsSuccessStatusCode)
            {
                ShowSuccess("Compte créé avec succès!");
                await Task.Delay(500);
                _mainNav.NavigateToLogin(false);
            }
            else if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ShowError("Ce nom d'utilisateur existe déjà");
            }
            else
            {
                ShowError("Erreur lors de l'inscription. Veuillez réessayer.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur d'inscription: {ex.Message}");
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