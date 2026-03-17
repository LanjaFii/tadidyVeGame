using System;
using ReactiveUI;
using System.Reactive;
using Avalonia.Media;

namespace TadidyVeGame.ViewModels;

public class ScoreViewModel : ViewModelBase
{
    private int _finalScore = 0;
    private int _levelReached = 1;
    private string _playDuration = "0s";
    private string _gameOverTitle = "GAME OVER";
    private IBrush _titleColor = Brushes.Red;
    private readonly MainViewModel _mainNav;

    public int FinalScore
    {
        get => _finalScore;
        set => this.RaiseAndSetIfChanged(ref _finalScore, value);
    }

    public int LevelReached
    {
        get => _levelReached;
        set => this.RaiseAndSetIfChanged(ref _levelReached, value);
    }

    public string PlayDuration
    {
        get => _playDuration;
        set => this.RaiseAndSetIfChanged(ref _playDuration, value);
    }

    public string GameOverTitle
    {
        get => _gameOverTitle;
        set => this.RaiseAndSetIfChanged(ref _gameOverTitle, value);
    }

    public IBrush TitleColor
    {
        get => _titleColor;
        set => this.RaiseAndSetIfChanged(ref _titleColor, value);
    }

    public ReactiveCommand<Unit, Unit> PlayAgainCommand { get; }
    public ReactiveCommand<Unit, Unit> BackToMenuCommand { get; }

    public ScoreViewModel(MainViewModel mainNav)
    {
        _mainNav = mainNav;
        PlayAgainCommand = ReactiveCommand.Create(() => _mainNav.NavigateToGame(false));
        BackToMenuCommand = ReactiveCommand.Create(() => _mainNav.NavigateToMainMenu());
    }

    public void SetGameOverData(int score, int level, TimeSpan duration, bool isWin = false)
    {
        FinalScore = score;
        LevelReached = level;
        PlayDuration = duration.ToString(@"m\:ss");
        GameOverTitle = isWin ? "VICTOIRE! 🎉" : "GAME OVER ❌";
        TitleColor = isWin ? Brushes.LimeGreen : Brushes.Red;
    }
}
