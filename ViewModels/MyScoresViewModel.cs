using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive;
using TadidyVeGame.Models;
using TadidyVeGame.Services;

namespace TadidyVeGame.ViewModels;

public class MyScoresViewModel : ViewModelBase
{
    private ObservableCollection<ScoreResponse> _scores = new();
    private readonly ScoreService _scoreService;
    private readonly AuthService _authService;
    private readonly MainViewModel _mainNav;
    private int _playerId;

    public ObservableCollection<ScoreResponse> Scores
    {
        get => _scores;
        set => this.RaiseAndSetIfChanged(ref _scores, value);
    }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    public MyScoresViewModel(MainViewModel mainNav, ScoreService scoreService, AuthService authService)
    {
        _mainNav = mainNav;
        _scoreService = scoreService;
        _authService = authService;
        BackCommand = ReactiveCommand.Create(() => _mainNav.NavigateToMainMenu());

        if (_authService.CurrentPlayer != null)
        {
            _playerId = _authService.CurrentPlayer.Id;
            LoadScores();
        }
    }

    private void LoadScores()
    {
        // À implémenter avec un endpoint GET /scores/{playerId}
        // var scores = await _scoreService.GetPlayerScoresAsync(_playerId);
        // if (scores != null)
        // {
        //     Scores = new ObservableCollection<ScoreResponse>(scores);
        // }
    }
}
