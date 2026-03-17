using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TadidyVeGame.Models;

public class GameEngine
{
    private List<int> _sequence = new();
    private int _userIndex = 0;
    public int CurrentLevel { get; private set; } = 1;
    
    public event Action<int>? OnShowSquare; // Pour l'UI
    public event Action? OnGameOver;
    public event Action? OnLevelUp;

    public void StartNewLevel()
    {
        _userIndex = 0;
        // Ajouter un carré aléatoire (0-8 pour une grille 3x3)
        _sequence.Add(Random.Shared.Next(0, 9));
        PlaySequence();
    }

    private async void PlaySequence()
    {
        foreach (var id in _sequence)
        {
            OnShowSquare?.Invoke(id);
            await Task.Delay(800); // Temps d'allumage
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

    public int CalculateTimeLimit() => 5 + (CurrentLevel - 1) * 3;
}