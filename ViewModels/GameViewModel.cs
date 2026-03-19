using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using ReactiveUI;
using System.Reactive;
using TadidyVeGame.Models;

namespace TadidyVeGame.ViewModels;

public enum GamePhase
{
    Initial,
    Memorizing,
    Playing
}

public class GameViewModel : ViewModelBase
{
    public bool IsStartButtonVisible => CurrentPhase == GamePhase.Initial;
    public bool IsGameGridVisible => CurrentPhase != GamePhase.Initial;
    
    private readonly GameEngine _engine;
    private readonly MainViewModel _mainNav;
    private readonly bool _isCompetition;
    
    private GamePhase _currentPhase = GamePhase.Initial;
    private string _levelText = "1";
    private string _timerText = "0s";
    private int _timeRemaining = 0;
    private DateTime _gameStartTime;
    private CancellationTokenSource? _timerCts;
    private CancellationTokenSource _gameCts = new();
    private string _statusMessage = "";
    private string _footerMessage = "";
    private string _currentPhaseText = "";
    
    private IBrush _square0Color = new SolidColorBrush(Color.Parse("#e74c3c"));
    private IBrush _square1Color = new SolidColorBrush(Color.Parse("#3498db"));
    private IBrush _square2Color = new SolidColorBrush(Color.Parse("#2ecc71"));
    private IBrush _square3Color = new SolidColorBrush(Color.Parse("#e67e22"));
    private IBrush _square4Color = new SolidColorBrush(Color.Parse("#f1c40f"));
    private IBrush _square5Color = new SolidColorBrush(Color.Parse("#1abc9c"));
    private IBrush _square6Color = new SolidColorBrush(Color.Parse("#9b59b6"));
    private IBrush _square7Color = new SolidColorBrush(Color.Parse("#e91e63"));
    private IBrush _square8Color = new SolidColorBrush(Color.Parse("#95a5a6"));
    
    // Les cases sont sombres par défaut (0.3)
    private double[] _opacities = new double[9] { 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3 };

    public GamePhase CurrentPhase { get => _currentPhase; set => this.RaiseAndSetIfChanged(ref _currentPhase, value); }
    public string LevelText { get => _levelText; set => this.RaiseAndSetIfChanged(ref _levelText, value); }
    public string TimerText { get => _timerText; set => this.RaiseAndSetIfChanged(ref _timerText, value); }
    public string StatusMessage { get => _statusMessage; set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }
    public string FooterMessage { get => _footerMessage; set => this.RaiseAndSetIfChanged(ref _footerMessage, value); }
    public string CurrentPhaseText { get => _currentPhaseText; set => this.RaiseAndSetIfChanged(ref _currentPhaseText, value); }

    public bool IsMemorizingPhase => CurrentPhase == GamePhase.Memorizing;
    public bool IsPlayerPhase => CurrentPhase == GamePhase.Playing;
    public bool ShowNavigationMenus => _isCompetition;

    public IBrush Square0Color { get => _square0Color; set => this.RaiseAndSetIfChanged(ref _square0Color, value); }
    public IBrush Square1Color { get => _square1Color; set => this.RaiseAndSetIfChanged(ref _square1Color, value); }
    public IBrush Square2Color { get => _square2Color; set => this.RaiseAndSetIfChanged(ref _square2Color, value); }
    public IBrush Square3Color { get => _square3Color; set => this.RaiseAndSetIfChanged(ref _square3Color, value); }
    public IBrush Square4Color { get => _square4Color; set => this.RaiseAndSetIfChanged(ref _square4Color, value); }
    public IBrush Square5Color { get => _square5Color; set => this.RaiseAndSetIfChanged(ref _square5Color, value); }
    public IBrush Square6Color { get => _square6Color; set => this.RaiseAndSetIfChanged(ref _square6Color, value); }
    public IBrush Square7Color { get => _square7Color; set => this.RaiseAndSetIfChanged(ref _square7Color, value); }
    public IBrush Square8Color { get => _square8Color; set => this.RaiseAndSetIfChanged(ref _square8Color, value); }

    public double Square0Opacity { get => _opacities[0]; set { _opacities[0] = value; this.RaisePropertyChanged(); } }
    public double Square1Opacity { get => _opacities[1]; set { _opacities[1] = value; this.RaisePropertyChanged(); } }
    public double Square2Opacity { get => _opacities[2]; set { _opacities[2] = value; this.RaisePropertyChanged(); } }
    public double Square3Opacity { get => _opacities[3]; set { _opacities[3] = value; this.RaisePropertyChanged(); } }
    public double Square4Opacity { get => _opacities[4]; set { _opacities[4] = value; this.RaisePropertyChanged(); } }
    public double Square5Opacity { get => _opacities[5]; set { _opacities[5] = value; this.RaisePropertyChanged(); } }
    public double Square6Opacity { get => _opacities[6]; set { _opacities[6] = value; this.RaisePropertyChanged(); } }
    public double Square7Opacity { get => _opacities[7]; set { _opacities[7] = value; this.RaisePropertyChanged(); } }
    public double Square8Opacity { get => _opacities[8]; set { _opacities[8] = value; this.RaisePropertyChanged(); } }

    public IBrush TimerColor => _timeRemaining <= 3 
        ? new SolidColorBrush(Color.Parse("#e74c3c"))
        : new SolidColorBrush(Color.Parse("#2ecc71"));
    
    public IBrush StatusColor => _statusMessage.Contains("✗") || _statusMessage.Contains("Erreur")
        ? new SolidColorBrush(Color.Parse("#e74c3c"))
        : new SolidColorBrush(Color.Parse("#2ecc71"));

    public ReactiveCommand<Unit, Unit> StartGameCommand { get; }
    public ReactiveCommand<string, Unit> SquareClickCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToLeaderboardCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToMyScoresCommand { get; }

    public GameViewModel(MainViewModel mainNav, bool isCompetition)
    {
        _mainNav = mainNav;
        _isCompetition = isCompetition;
        _engine = new GameEngine();
        _gameStartTime = DateTime.Now;
        
        _engine.OnShowSquare += (squareId) => FlashSquare(squareId);
        _engine.OnLevelUp += HandleLevelUp;
        _engine.OnGameOver += HandleGameOver;

        StartGameCommand = ReactiveCommand.Create(() => BeginGame());
        
        // On fait clignoter la case quand le joueur clique pour du feedback
        SquareClickCommand = ReactiveCommand.Create<string>(id => 
        {
            int parsedId = int.Parse(id);
            FlashSquare(parsedId);
            _engine.ProcessInput(parsedId);
        });

        GoToLeaderboardCommand = ReactiveCommand.Create(() => { _gameCts.Cancel(); _mainNav.NavigateToLeaderboard(); });
        GoToProfileCommand = ReactiveCommand.Create(() => { _gameCts.Cancel(); _mainNav.NavigateToProfile(); });
        GoToMyScoresCommand = ReactiveCommand.Create(() => { _gameCts.Cancel(); _mainNav.NavigateToMyScores(); });

        CurrentPhase = GamePhase.Initial;
        CurrentPhaseText = "";
        StatusMessage = "Appuyez sur Commencer pour débuter";
        FooterMessage = "Mémorisez la séquence, puis reproduisez-la";
    }

    private void BeginGame()
    {
        _engine.ResetGame(); // On remet au niveau 1
        LevelText = "1";
        _engine.StartNewLevel();
        _ = ShowMemorizingPhaseAsync();
    }

    private async Task ShowMemorizingPhaseAsync()
    {
        try
        {
            CurrentPhase = GamePhase.Memorizing;
            this.RaisePropertyChanged(nameof(IsStartButtonVisible));
            this.RaisePropertyChanged(nameof(IsGameGridVisible));
            CurrentPhaseText = "TADIDIO";
            StatusMessage = "Observation...";
            
            await Task.Delay(1000, _gameCts.Token);
            if (_gameCts.Token.IsCancellationRequested) return;
            
            // On attend VRAIMENT que la séquence soit finie de jouer !
            await _engine.PlaySequenceAsync();
            
            await Task.Delay(500, _gameCts.Token);
            if (_gameCts.Token.IsCancellationRequested) return;
            
            CurrentPhase = GamePhase.Playing;
            this.RaisePropertyChanged(nameof(IsStartButtonVisible));
            this.RaisePropertyChanged(nameof(IsGameGridVisible));
            CurrentPhaseText = "ANAO IZAO";
            StatusMessage = "Refais la séquence!";
            StartTimer();
        }
        catch (OperationCanceledException) { }
    }

    private void HandleLevelUp()
    {
        LevelText = _engine.CurrentLevel.ToString();
        StatusMessage = "Bravo! Niveau suivant...";
        _timerCts?.Cancel();
        _ = HandleLevelUpAsync();
    }

    private async Task HandleLevelUpAsync()
    {
        try
        {
            await Task.Delay(1000, _gameCts.Token);
            if (_gameCts.Token.IsCancellationRequested) return;
            
            _engine.StartNewLevel();
            await ShowMemorizingPhaseAsync();
        }
        catch (OperationCanceledException) { }
    }

    private void HandleGameOver()
    {
        StatusMessage = "Erreur! Jeu terminé";
        _timerCts?.Cancel();
        _ = HandleGameOverAsync();
    }

    private async Task HandleGameOverAsync()
    {
        try
        {
            await Task.Delay(2000, _gameCts.Token);
            if (_gameCts.Token.IsCancellationRequested) return;
            
            var score = _engine.CurrentLevel - 1;
            var duration = DateTime.Now - _gameStartTime;
            CurrentPhase = GamePhase.Initial;
            this.RaisePropertyChanged(nameof(IsStartButtonVisible));
            this.RaisePropertyChanged(nameof(IsGameGridVisible));
            _mainNav.NavigateToScore(score, _engine.CurrentLevel - 1, duration, _isCompetition);
        }
        catch (OperationCanceledException) { }
    }

    private void FlashSquare(int squareId)
    {
        Task.Run(async () =>
        {
            SetSquareOpacity(squareId, 1.0); // La case s'allume complètement !
            await Task.Delay(350);
            SetSquareOpacity(squareId, 0.3); // La case redevient sombre
        });
    }

    private void SetSquareOpacity(int squareId, double opacity)
    {
        if (squareId >= 0 && squareId < _opacities.Length)
        {
            _opacities[squareId] = opacity;
            this.RaisePropertyChanged($"Square{squareId}Opacity");
        }
    }

    private void StartTimer()
    {
        _timerCts = new CancellationTokenSource();
        int timeLimit = _engine.CalculateTimeLimit();
        _timeRemaining = timeLimit;
        UpdateTimerDisplay();
        
        Task.Run(async () =>
        {
            while (_timeRemaining > 0 && !_timerCts.Token.IsCancellationRequested)
            {
                await Task.Delay(1000);
                _timeRemaining--;
                UpdateTimerDisplay();
            }

            if (_timeRemaining <= 0 && !_timerCts.Token.IsCancellationRequested)
            {
                StatusMessage = "Temps écoulé!";
                _engine.TimeoutGame();
            }
        });
    }

    private void UpdateTimerDisplay()
    {
        TimerText = _timeRemaining > 0 ? $"{_timeRemaining}s" : "0s";
        this.RaisePropertyChanged(nameof(TimerColor));
    }

    public void Cleanup()
    {
        _gameCts?.Cancel(); _gameCts?.Dispose();
        _timerCts?.Cancel(); _timerCts?.Dispose();
    }

    ~GameViewModel()
    {
        _gameCts?.Cancel(); _gameCts?.Dispose();
        _timerCts?.Cancel(); _timerCts?.Dispose();
    }
}