using System.IO;
using System.Text.Json;

namespace TadidyVeGame.Utils;

public static class ConfigHelper
{
    public static string GetBaseUrl(bool isOffline)
    {
        var json = File.ReadAllText("appsettings.json");
        using var doc = JsonDocument.Parse(json);
        var settings = doc.RootElement.GetProperty("ApiSettings");
        return isOffline 
            ? settings.GetProperty("OfflineUrl").GetString()! 
            : settings.GetProperty("OnlineUrl").GetString()!;
    }
}