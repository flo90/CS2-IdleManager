using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;


public class IdleManagerConfig : BasePluginConfig
{
    [JsonPropertyName("Delay")] public float Delay { get; set; } = 3600.0f;
    [JsonPropertyName("DefaultMap")] public string DefaultMap { get; set; } = "de_dust2";
    [JsonPropertyName("ChangeInitial")] public bool ChangeInitial { get; set; } = false;
    [JsonPropertyName("WorkshopCollection")] public bool WorkshopCollection { get; set; } = false;
    [JsonPropertyName("Debug")] public bool Debug { get; set; } = false;
}