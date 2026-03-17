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
    private string _bio = "Nouveau joueur";
    private readonly MainViewModel _mainNav;
    private readonly ApiService _api;

    public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }

    public ReactiveCommand<Unit, Unit> RegisterCommand { get; }

    public RegisterViewModel(MainViewModel mainNav, ApiService api)
    {
        _mainNav = mainNav;
        _api = api;
        RegisterCommand = ReactiveCommand.CreateFromTask(Register);
    }

    private async Task Register()
    {
        var res = await _api.PostAsync("auth/register", new RegisterRequest(Username, Password, _bio, "default.png"));
        if (res.IsSuccessStatusCode) _mainNav.NavigateToLogin(false);
    }
}