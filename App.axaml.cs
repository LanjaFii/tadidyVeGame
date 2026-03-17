using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using TadidyVeGame.Services;
using TadidyVeGame.ViewModels;
using TadidyVeGame.Views;
using TadidyVeGame.Utils;

namespace TadidyVeGame;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure ReactiveUI scheduler FIRST, before creating any ViewModels
        RxApp.MainThreadScheduler = new AvaloniaThreadPoolScheduler();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 1. Initialisation des services de base
            string defaultUrl = ConfigHelper.GetBaseUrl(isOffline: false);
            
            var apiService = new ApiService(defaultUrl);
            var authService = new AuthService(apiService);
            var scoreService = new ScoreService(apiService);

            // 2. Injection des services dans le MainViewModel
            var mainViewModel = new MainViewModel(apiService, authService, scoreService);

            DisableAvaloniaDataAnnotationValidation();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Supprime les validateurs de données par défaut d'Avalonia 
        // pour laisser CommunityToolkit.Mvvm gérer les erreurs proprement.
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}

/// <summary>
/// Scheduler for ReactiveUI that ensures all operations run on Avalonia's UI thread.
/// </summary>
public class AvaloniaThreadPoolScheduler : IScheduler
{
    public DateTimeOffset Now => DateTimeOffset.Now;

    public IDisposable Schedule<TState>(
        TState state,
        Func<IScheduler, TState, IDisposable> action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return action(this, state);
        }

        var ret = Disposable.Empty;
        Dispatcher.UIThread.Post(() =>
        {
            ret = action(this, state);
        });

        return ret;
    }

    public IDisposable Schedule<TState>(
        TState state,
        TimeSpan dueTime,
        Func<IScheduler, TState, IDisposable> action)
    {
        if (dueTime.Ticks <= 0)
        {
            return Schedule(state, action);
        }

        var timer = new System.Timers.Timer(dueTime.TotalMilliseconds);
        timer.AutoReset = false;
        timer.Elapsed += (s, e) =>
        {
            Schedule(state, action);
            timer.Dispose();
        };
        timer.Start();

        return Disposable.Create(() => timer?.Dispose());
    }

    public IDisposable Schedule<TState>(
        TState state,
        DateTimeOffset dueTime,
        Func<IScheduler, TState, IDisposable> action)
    {
        var timeSpan = dueTime - Now;
        return Schedule(state, timeSpan, action);
    }
}