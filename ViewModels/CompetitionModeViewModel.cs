using ReactiveUI;
using System.Reactive;

namespace TadidyVeGame.ViewModels;

public class CompetitionModeViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> SelectOnlineCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectOfflineCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    public CompetitionModeViewModel(MainViewModel mainNav)
    {
        SelectOnlineCommand = ReactiveCommand.Create(() => mainNav.NavigateToLogin(false));
        SelectOfflineCommand = ReactiveCommand.Create(() => mainNav.NavigateToLogin(true));
        BackCommand = ReactiveCommand.Create(() => mainNav.NavigateToMainMenu());
    }
}