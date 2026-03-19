using System;
using System.Collections.Generic;
using ReactiveUI;
using TadidyVeGame.Services;
using TadidyVeGame.Utils;

namespace TadidyVeGame.ViewModels;

public class MainViewModel : ViewModelBase
{
    // Pile de navigation pour retour contextuel
    private readonly Stack<ViewModelBase> _navigationStack = new();
    private ViewModelBase _currentPage;
    private readonly ApiService _api;
    private readonly AuthService _auth;
    private readonly ScoreService _score;

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    private void CleanupCurrentPage()
    {
        if (_currentPage is GameViewModel gameVM)
        {
            gameVM.Cleanup();
        }
    }

    public void GoBack()
    {
        CleanupCurrentPage();
        if (_navigationStack.Count > 0)
        {
            _currentPage = _navigationStack.Pop();
            this.RaisePropertyChanged(nameof(CurrentPage));
        }
        else
        {
            NavigateToMainMenu();
        }
    }

    public MainViewModel(ApiService api, AuthService auth, ScoreService score)
    {
        _api = api;
        _auth = auth;
        _score = score;
        
        // Au démarrage, on affiche le menu principal
        _currentPage = new MainMenuViewModel(this);
    }

    public void NavigateToCompetitionMenu()
    {
        _navigationStack.Push(_currentPage);
        CurrentPage = new CompetitionModeViewModel(this);
    }

    public void NavigateToLogin(bool isOffline)
    {
        _navigationStack.Push(_currentPage);
        _api.UpdateBaseUrl(ConfigHelper.GetBaseUrl(isOffline));
        CurrentPage = new LoginViewModel(this, _auth);
    }

    public void NavigateToRegister()
    {
        _navigationStack.Push(_currentPage);
        CurrentPage = new RegisterViewModel(this, _api);
    }

    public void NavigateToGame(bool isCompetition)
    {
        _navigationStack.Push(_currentPage);
        CurrentPage = new GameViewModel(this, isCompetition);
    }

    public void NavigateToScore(int score, int level, TimeSpan duration, bool isCompetition)
    {
        CleanupCurrentPage();
        _navigationStack.Push(_currentPage);
        var vm = new ScoreViewModel(this);
        vm.SetGameOverData(score, level, duration);
        CurrentPage = vm;
    }

    public void NavigateToLeaderboard()
    {
        CleanupCurrentPage();
        _navigationStack.Push(_currentPage);
        CurrentPage = new LeaderboardViewModel(this, _score);
    }

    public void NavigateToProfile()
    {
        CleanupCurrentPage();
        _navigationStack.Push(_currentPage);
        CurrentPage = new ProfileViewModel(this, _auth);
    }

    public void NavigateToMyScores()
    {
        CleanupCurrentPage();
        _navigationStack.Push(_currentPage);
        CurrentPage = new MyScoresViewModel(this, _score, _auth);
    }

    public void NavigateToMainMenu()
    {
        CleanupCurrentPage();
        _navigationStack.Clear();
        CurrentPage = new MainMenuViewModel(this);
    }
}