using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TadidyVeGame.Models;

namespace TadidyVeGame.Services;

public class ScoreService
{
    private readonly ApiService _api;
    public ScoreService(ApiService api) => _api = api;

    public async Task<bool> SubmitScoreAsync(int playerId, int value)
    {
        var response = await _api.PostAsync("scores", new ScoreRequest(playerId, value));
        return response.IsSuccessStatusCode;
    }

    public async Task<List<PlayerResponse>?> GetLeaderboardAsync() => 
        await _api.GetAsync<List<PlayerResponse>>("scores/top?limit=10");
}