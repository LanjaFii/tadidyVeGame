using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TadidyVeGame.Models;

public class GameEngine
{
    private List<int> _sequence = new();
    private int _userIndex = 0;
    public int CurrentLevel { get; private set; } = 1;
    
    public event Action<int>? OnShowSquare;
    public event Action? OnGameOver;
    public event Action? OnLevelUp;

    public void ResetGame()
    {
        CurrentLevel = 1;
        _sequence.Clear();
        _userIndex = 0;
    }

    public void StartNewLevel()
    {
        _userIndex = 0;
        _sequence.Clear(); // On vide l'ancienne séquence
        
        // On génère une toute nouvelle séquence de la taille du niveau actuel
        for (int i = 0; i < CurrentLevel; i++)
        {
            _sequence.Add(Random.Shared.Next(0, 9));
        }
    }

    // Devenu asynchrone pour que le ViewModel puisse l'attendre
    public async Task PlaySequenceAsync()
    {
        foreach (var id in _sequence)
        {
            OnShowSquare?.Invoke(id);
            await Task.Delay(700); // Temps allumé
            await Task.Delay(200); // Petite pause entre 2 cases identiques
        }
    }

    public void ProcessInput(int squareId)
    {
        if (_sequence[_userIndex] == squareId)
        {
            _userIndex++;
            if (_userIndex >= _sequence.Count)
            {
                CurrentLevel++;
                OnLevelUp?.Invoke();
            }
        }
        else
        {
            OnGameOver?.Invoke();
        }
    }

    public void TimeoutGame()
    {
        OnGameOver?.Invoke();
    }

    public int CalculateTimeLimit() => 10 + (CurrentLevel - 1) * 3;
}