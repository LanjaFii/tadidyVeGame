using System;
using ReactiveUI;
using System.Reactive;
using TadidyVeGame.Models;

namespace TadidyVeGame.ViewModels;

public class GameViewModel : ViewModelBase
{
    private readonly GameEngine _engine;
    private readonly MainViewModel _mainNav;
    private readonly bool _isCompetition;
    private string _levelText = "Niveau 1";
    private string _timerText = "5s";
    private DateTime _gameStartTime;

    public string LevelText { get => _levelText; set => this.RaiseAndSetIfChanged(ref _levelText, value); }
    public string TimerText { get => _timerText; set => this.RaiseAndSetIfChanged(ref _timerText, value); }

    public ReactiveCommand<string, Unit> SquareClickCommand { get; }

    public GameViewModel(MainViewModel mainNav, bool isCompetition)
    {
        _mainNav = mainNav;
        _isCompetition = isCompetition;
        _engine = new GameEngine();
        _gameStartTime = DateTime.Now;
        
        _engine.OnLevelUp += () => {
            LevelText = $"Niveau {_engine.CurrentLevel}";
            _engine.StartNewLevel();
        };

        _engine.OnGameOver += () => {
            // Logique de fin de jeu (Envoi score si compétition)
            var score = _engine.CurrentLevel - 1;
            var duration = DateTime.Now - _gameStartTime;
            
            _mainNav.NavigateToScore(score, _engine.CurrentLevel - 1, duration, _isCompetition);
        };

        SquareClickCommand = ReactiveCommand.Create<string>(id => {
            _engine.ProcessInput(int.Parse(id));
        });

        _engine.StartNewLevel();
    }
}