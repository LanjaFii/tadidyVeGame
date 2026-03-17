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
    private readonly MainViewModel _mainNav;
    private readonly AuthService _auth;

    public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    public LoginViewModel(MainViewModel mainNav, AuthService auth)
    {
        _mainNav = mainNav;
        _auth = auth;
        LoginCommand = ReactiveCommand.CreateFromTask(ExecuteLogin);
    }

    private async Task ExecuteLogin()
    {
        if (await _auth.LoginAsync(Username, Password))
        {
            _mainNav.NavigateToGame(true);
        }
    }
}