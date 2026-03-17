using System;
using ReactiveUI;
using System.Reactive;
using TadidyVeGame.Services;

namespace TadidyVeGame.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    private string _username = "";
    private string _bio = "";
    private int _bestScore = 0;
    private DateTime _createdAt = DateTime.Now;
    private readonly AuthService _authService;
    private readonly MainViewModel _mainNav;

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    public string Bio
    {
        get => _bio;
        set => this.RaiseAndSetIfChanged(ref _bio, value);
    }

    public int BestScore
    {
        get => _bestScore;
        set => this.RaiseAndSetIfChanged(ref _bestScore, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => this.RaiseAndSetIfChanged(ref _createdAt, value);
    }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    public ProfileViewModel(MainViewModel mainNav, AuthService authService)
    {
        _mainNav = mainNav;
        _authService = authService;
        BackCommand = ReactiveCommand.Create(() => _mainNav.NavigateToMainMenu());
        LogoutCommand = ReactiveCommand.Create(() =>
        {
            _authService.CurrentPlayer = null;
            _mainNav.NavigateToMainMenu();
        });

        // Charger les infos du profil actuel
        if (_authService.CurrentPlayer != null)
        {
            Username = _authService.CurrentPlayer.Username;
            Bio = _authService.CurrentPlayer.Bio;
            BestScore = _authService.CurrentPlayer.BestScore;
            CreatedAt = _authService.CurrentPlayer.CreatedAt;
        }
    }
}
