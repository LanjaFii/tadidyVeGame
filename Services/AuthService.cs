using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TadidyVeGame.Models;

namespace TadidyVeGame.Services;

public class AuthService
{
    private readonly ApiService _api;
    public PlayerResponse? CurrentPlayer { get; set; }

    public AuthService(ApiService api) => _api = api;

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _api.PostAsync("auth/login", new LoginRequest(username, password));
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            _api.SetToken(result!["token"]);
            // Optionnel : Récupérer les infos du profil juste après
            return true;
        }
        return false;
    }
}