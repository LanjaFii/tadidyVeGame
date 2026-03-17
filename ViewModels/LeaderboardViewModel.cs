using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive;
using TadidyVeGame.Models;
using TadidyVeGame.Services;

namespace TadidyVeGame.ViewModels;

public class LeaderboardViewModel : ViewModelBase
{
    private ObservableCollection<PlayerResponse> _topPlayers = new();
    private readonly ScoreService _scoreService;
    private readonly MainViewModel _mainNav;

    public ObservableCollection<PlayerResponse> TopPlayers
    {
        get => _topPlayers;
        set => this.RaiseAndSetIfChanged(ref _topPlayers, value);
    }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    public LeaderboardViewModel(MainViewModel mainNav, ScoreService scoreService)
    {
        _mainNav = mainNav;
        _scoreService = scoreService;
        BackCommand = ReactiveCommand.Create(() => _mainNav.NavigateToMainMenu());
        
        LoadLeaderboard();
    }

    private async void LoadLeaderboard()
    {
        var players = await _scoreService.GetLeaderboardAsync();
        if (players != null)
        {
            TopPlayers = new ObservableCollection<PlayerResponse>(players);
        }
    }
}
