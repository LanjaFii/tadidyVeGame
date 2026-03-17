using System.Threading.Tasks;
using TadidyVeGame.Models;

namespace TadidyVeGame.Services;

public class PlayerService
{
    private readonly ApiService _api;
    public PlayerService(ApiService api) => _api = api;

    public async Task<PlayerResponse?> GetProfileAsync(int id) => 
        await _api.GetAsync<PlayerResponse>($"players/{id}");
}