using ReactiveUI;
using System.Reactive;

namespace TadidyVeGame.ViewModels;

public class MainMenuViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> StartTrainingCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToCompetitionCommand { get; }

    public MainMenuViewModel(MainViewModel mainNav)
    {
        // On demande au MainViewModel de changer de page
        StartTrainingCommand = ReactiveCommand.Create(() => mainNav.NavigateToGame(false));
        GoToCompetitionCommand = ReactiveCommand.Create(() => mainNav.NavigateToCompetitionMenu());
    }
}