using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace TadidyVeGame.Services;

public class ApiService
{
    private HttpClient _http;
    public string? Token { get; set; }

    public ApiService(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public void SetToken(string token)
    {
        Token = token;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint) => await _http.GetFromJsonAsync<T>(endpoint);
    
    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data) => 
        await _http.PostAsJsonAsync(endpoint, data);

    public void UpdateBaseUrl(string newUrl)
    {
        // On recrée le client car l'adresse de base est immuable après la première requête
        _http = new HttpClient { BaseAddress = new Uri(newUrl) };
        if (!string.IsNullOrEmpty(Token))
        {
            SetToken(Token);
        }
    }
}