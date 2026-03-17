using System;
using ReactiveUI;
using TadidyVeGame.Services;
using TadidyVeGame.Utils;

namespace TadidyVeGame.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;
    private readonly ApiService _api;
    private readonly AuthService _auth;
    private readonly ScoreService _score;

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public MainViewModel(ApiService api, AuthService auth, ScoreService score)
    {
        _api = api;
        _auth = auth;
        _score = score;
        
        // Au démarrage, on affiche le menu principal
        _currentPage = new MainMenuViewModel(this);
    }

    public void NavigateToCompetitionMenu() => CurrentPage = new CompetitionModeViewModel(this);
    
    public void NavigateToLogin(bool isOffline)
    {
        // On change l'URL de l'API dynamiquement avant d'aller au login
        _api.UpdateBaseUrl(ConfigHelper.GetBaseUrl(isOffline));
        CurrentPage = new LoginViewModel(this, _auth);
    }

    public void NavigateToGame(bool isCompetition) => CurrentPage = new GameViewModel(this, isCompetition);

    public void NavigateToScore(int score, int level, TimeSpan duration, bool isCompetition)
    {
        var vm = new ScoreViewModel(this);
        vm.SetGameOverData(score, level, duration);
        CurrentPage = vm;
    }

    public void NavigateToLeaderboard() => CurrentPage = new LeaderboardViewModel(this, _score);

    public void NavigateToProfile() => CurrentPage = new ProfileViewModel(this, _auth);

    public void NavigateToMyScores() => CurrentPage = new MyScoresViewModel(this, _score, _auth);

    public void NavigateToMainMenu() => CurrentPage = new MainMenuViewModel(this);
}