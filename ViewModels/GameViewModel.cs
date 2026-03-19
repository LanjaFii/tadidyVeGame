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
    
    // Mots de feedback
    private static readonly string[] PosWords = { "Cool!", "OK", "Yes!", "Haha", "Top!" };
    private static readonly string[] NegWords = { "Nope", "Oups", "Bruh", "Aïe" };

    private IBrush[] _squareColors = new IBrush[9];
    private double[] _opacities = new double[9] { 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3 };
    private string[] _squareTexts = new string[9] { "", "", "", "", "", "", "", "", "" };

    public GamePhase CurrentPhase { get => _currentPhase; set { this.RaiseAndSetIfChanged(ref _currentPhase, value); this.RaisePropertyChanged(nameof(IsPlayerPhase)); } }
    public string LevelText { get => _levelText; set => this.RaiseAndSetIfChanged(ref _levelText, value); }
    public string TimerText { get => _timerText; set => this.RaiseAndSetIfChanged(ref _timerText, value); }
    
    public string StatusMessage 
    { 
        get => _statusMessage; 
        set 
        { 
            this.RaiseAndSetIfChanged(ref _statusMessage, value); 
            this.RaisePropertyChanged(nameof(StatusColor));
            this.RaisePropertyChanged(nameof(StatusBackgroundColor));
        } 
    }
    
    public string FooterMessage { get => _footerMessage; set => this.RaiseAndSetIfChanged(ref _footerMessage, value); }
    public string CurrentPhaseText { get => _currentPhaseText; set => this.RaiseAndSetIfChanged(ref _currentPhaseText, value); }

    public bool IsMemorizingPhase => CurrentPhase == GamePhase.Memorizing;
    public bool IsPlayerPhase => CurrentPhase == GamePhase.Playing;
    public bool ShowNavigationMenus => _isCompetition;

    // Propriétés de Couleurs
    public IBrush Square0Color { get => _squareColors[0]; set { _squareColors[0] = value; this.RaisePropertyChanged(); } }
    public IBrush Square1Color { get => _squareColors[1]; set { _squareColors[1] = value; this.RaisePropertyChanged(); } }
    public IBrush Square2Color { get => _squareColors[2]; set { _squareColors[2] = value; this.RaisePropertyChanged(); } }
    public IBrush Square3Color { get => _squareColors[3]; set { _squareColors[3] = value; this.RaisePropertyChanged(); } }
    public IBrush Square4Color { get => _squareColors[4]; set { _squareColors[4] = value; this.RaisePropertyChanged(); } }
    public IBrush Square5Color { get => _squareColors[5]; set { _squareColors[5] = value; this.RaisePropertyChanged(); } }
    public IBrush Square6Color { get => _squareColors[6]; set { _squareColors[6] = value; this.RaisePropertyChanged(); } }
    public IBrush Square7Color { get => _squareColors[7]; set { _squareColors[7] = value; this.RaisePropertyChanged(); } }
    public IBrush Square8Color { get => _squareColors[8]; set { _squareColors[8] = value; this.RaisePropertyChanged(); } }

    // Propriétés d'Opacité
    public double Square0Opacity { get => _opacities[0]; set { _opacities[0] = value; this.RaisePropertyChanged(); } }
    public double Square1Opacity { get => _opacities[1]; set { _opacities[1] = value; this.RaisePropertyChanged(); } }
    public double Square2Opacity { get => _opacities[2]; set { _opacities[2] = value; this.RaisePropertyChanged(); } }
    public double Square3Opacity { get => _opacities[3]; set { _opacities[3] = value; this.RaisePropertyChanged(); } }
    public double Square4Opacity { get => _opacities[4]; set { _opacities[4] = value; this.RaisePropertyChanged(); } }
    public double Square5Opacity { get => _opacities[5]; set { _opacities[5] = value; this.RaisePropertyChanged(); } }
    public double Square6Opacity { get => _opacities[6]; set { _opacities[6] = value; this.RaisePropertyChanged(); } }
    public double Square7Opacity { get => _opacities[7]; set { _opacities[7] = value; this.RaisePropertyChanged(); } }
    public double Square8Opacity { get => _opacities[8]; set { _opacities[8] = value; this.RaisePropertyChanged(); } }

    // Propriétés de Textes
    public string Square0Text { get => _squareTexts[0]; set { _squareTexts[0] = value; this.RaisePropertyChanged(); } }
    public string Square1Text { get => _squareTexts[1]; set { _squareTexts[1] = value; this.RaisePropertyChanged(); } }
    public string Square2Text { get => _squareTexts[2]; set { _squareTexts[2] = value; this.RaisePropertyChanged(); } }
    public string Square3Text { get => _squareTexts[3]; set { _squareTexts[3] = value; this.RaisePropertyChanged(); } }
    public string Square4Text { get => _squareTexts[4]; set { _squareTexts[4] = value; this.RaisePropertyChanged(); } }
    public string Square5Text { get => _squareTexts[5]; set { _squareTexts[5] = value; this.RaisePropertyChanged(); } }
    public string Square6Text { get => _squareTexts[6]; set { _squareTexts[6] = value; this.RaisePropertyChanged(); } }
    public string Square7Text { get => _squareTexts[7]; set { _squareTexts[7] = value; this.RaisePropertyChanged(); } }
    public string Square8Text { get => _squareTexts[8]; set { _squareTexts[8] = value; this.RaisePropertyChanged(); } }

    public IBrush TimerColor => _timeRemaining <= 3 
        ? new SolidColorBrush(Color.Parse("#ff7675"))
        : new SolidColorBrush(Color.Parse("#2ecc71"));
    
    public IBrush StatusColor => _statusMessage.Contains("✗") || _statusMessage.Contains("Erreur") || _statusMessage.Contains("écoulé")
        ? new SolidColorBrush(Color.Parse("#ff7675"))
        : new SolidColorBrush(Color.Parse("#2ecc71"));

    // Fond dynamique pour le message de statut
    public IBrush StatusBackgroundColor
    {
        get
        {
            if (_statusMessage.Contains("Erreur") || _statusMessage.Contains("écoulé"))
                return new SolidColorBrush(Color.Parse("#33ff7675")); // Rouge transparent
            if (_statusMessage.Contains("Observation"))
                return new SolidColorBrush(Color.Parse("#33f1c40f")); // Jaune transparent
            if (_statusMessage.Contains("Refais"))
                return new SolidColorBrush(Color.Parse("#333498db")); // Bleu transparent
            if (_statusMessage.Contains("Bravo"))
                return new SolidColorBrush(Color.Parse("#332ecc71")); // Vert transparent
            return new SolidColorBrush(Color.Parse("#11FFFFFF")); // Par défaut discret
        }
    }

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
        
        InitOriginalColors();

        _engine.OnShowSquare += (squareId) => FlashSquare(squareId);
        _engine.OnCorrectSquare += HandleCorrectSquare;
        _engine.OnWrongSquare += HandleWrongSquare;
        _engine.OnLevelUp += HandleLevelUp;
        _engine.OnGameOver += HandleGameOver;

        StartGameCommand = ReactiveCommand.Create(() => BeginGame());
        
        SquareClickCommand = ReactiveCommand.Create<string>(id => 
        {
            // La grille ignore les clics si c'est pas IsPlayerPhase (grâce à IsHitTestVisible),
            // Donc ici on est sûr que c'est le tour du joueur.
            _engine.ProcessInput(int.Parse(id));
        });

        GoToLeaderboardCommand = ReactiveCommand.Create(() => { _gameCts.Cancel(); _mainNav.NavigateToLeaderboard(); });
        GoToProfileCommand = ReactiveCommand.Create(() => { _gameCts.Cancel(); _mainNav.NavigateToProfile(); });
        GoToMyScoresCommand = ReactiveCommand.Create(() => { _gameCts.Cancel(); _mainNav.NavigateToMyScores(); });

        CurrentPhase = GamePhase.Initial;
        CurrentPhaseText = "";
        StatusMessage = "Appuyez sur Commencer";
        FooterMessage = "Mémorisez la séquence, puis reproduisez-la";
    }

    private void InitOriginalColors()
    {
        Square0Color = new SolidColorBrush(Color.Parse("#e74c3c"));
        Square1Color = new SolidColorBrush(Color.Parse("#3498db"));
        Square2Color = new SolidColorBrush(Color.Parse("#2ecc71"));
        Square3Color = new SolidColorBrush(Color.Parse("#e67e22"));
        Square4Color = new SolidColorBrush(Color.Parse("#f1c40f"));
        Square5Color = new SolidColorBrush(Color.Parse("#1abc9c"));
        Square6Color = new SolidColorBrush(Color.Parse("#9b59b6"));
        Square7Color = new SolidColorBrush(Color.Parse("#e91e63"));
        Square8Color = new SolidColorBrush(Color.Parse("#95a5a6"));
    }

    private IBrush GetOriginalColor(int id)
    {
        return id switch
        {
            0 => new SolidColorBrush(Color.Parse("#e74c3c")),
            1 => new SolidColorBrush(Color.Parse("#3498db")),
            2 => new SolidColorBrush(Color.Parse("#2ecc71")),
            3 => new SolidColorBrush(Color.Parse("#e67e22")),
            4 => new SolidColorBrush(Color.Parse("#f1c40f")),
            5 => new SolidColorBrush(Color.Parse("#1abc9c")),
            6 => new SolidColorBrush(Color.Parse("#9b59b6")),
            7 => new SolidColorBrush(Color.Parse("#e91e63")),
            8 => new SolidColorBrush(Color.Parse("#95a5a6")),
            _ => new SolidColorBrush(Colors.White)
        };
    }

    private void ResetGrid()
    {
        for (int i = 0; i < 9; i++)
        {
            SetSquareColor(i, GetOriginalColor(i));
            SetSquareText(i, "");
            SetSquareOpacity(i, 0.3);
        }
    }

    private void BeginGame()
    {
        _engine.ResetGame();
        LevelText = "1";
        ResetGrid();
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

    private void HandleCorrectSquare(int id)
    {
        // Change en vert clair et ajoute le texte positif
        SetSquareColor(id, new SolidColorBrush(Color.Parse("#7bed9f"))); // Vert très clair
        SetSquareText(id, PosWords[Random.Shared.Next(PosWords.Length)]);
        SetSquareOpacity(id, 1.0);

        Task.Run(async () =>
        {
            await Task.Delay(400); // Temps du feedback
            // Restaure la case
            SetSquareColor(id, GetOriginalColor(id));
            SetSquareText(id, "");
            SetSquareOpacity(id, 0.3);
        });
    }

    private void HandleWrongSquare(int id)
    {
        // Change en rouge clair et ajoute le texte négatif (reste affiché)
        SetSquareColor(id, new SolidColorBrush(Color.Parse("#ff7675"))); // Rouge très clair
        SetSquareText(id, NegWords[Random.Shared.Next(NegWords.Length)]);
        SetSquareOpacity(id, 1.0);
    }

    private void FlashSquare(int squareId)
    {
        Task.Run(async () =>
        {
            SetSquareOpacity(squareId, 1.0);
            await Task.Delay(350);
            SetSquareOpacity(squareId, 0.3);
        });
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
            
            ResetGrid();
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

    private void SetSquareColor(int id, IBrush color)
    {
        switch (id)
        {
            case 0: Square0Color = color; break;
            case 1: Square1Color = color; break;
            case 2: Square2Color = color; break;
            case 3: Square3Color = color; break;
            case 4: Square4Color = color; break;
            case 5: Square5Color = color; break;
            case 6: Square6Color = color; break;
            case 7: Square7Color = color; break;
            case 8: Square8Color = color; break;
        }
    }

    private void SetSquareText(int id, string text)
    {
        switch (id)
        {
            case 0: Square0Text = text; break;
            case 1: Square1Text = text; break;
            case 2: Square2Text = text; break;
            case 3: Square3Text = text; break;
            case 4: Square4Text = text; break;
            case 5: Square5Text = text; break;
            case 6: Square6Text = text; break;
            case 7: Square7Text = text; break;
            case 8: Square8Text = text; break;
        }
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