using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using Microsoft.Extensions.Logging;


namespace IdleManager;

public partial class IdleManager : BasePlugin, IPluginConfig<IdleManagerConfig>
{
    public override string ModuleName => "Idle Manager";

    public required IdleManagerConfig Config { get; set; }
 
    private Timer? _idleTimeout;
   
    // Lock object for _playerCount
    private readonly object _playerCountLock = new();
    // Tracks the number of human players currently connected to the server.
    private int _playerCount;

    // Tracks the number of map changes since the server started, if plugin is loaded.
    // Note: First map change happens after the server starts.
    private uint _mapChangeCount = 0;



    /***************************************************
    ***************** Plugin control *******************
    ***************************************************/
    public override void Load(bool hotReload)
    {
        //Print name and version of the plugin
        Logger.LogInformation($"{ModuleName} v{ModuleVersion} loaded");

        string? workshopCollectionId = ConVar.Find("host_workshop_collection")?.StringValue;
        DebugLog($"Workshop Collection ID: {workshopCollectionId ?? "None"}");

        // Register event handlers and listeners
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    public void OnConfigParsed(IdleManagerConfig config)
    {
        Config = config;

        //Return early if debug mode is off
        if (!Config.Debug)
        {
            return;
        }

        var configStr = $"""
            Config loaded: Delay {Config.Delay}
            DefaultMap {Config.DefaultMap}
            WorkshopCollection {Config.WorkshopCollection}
            Debug {Config.Debug}
            """;

        DebugLog(configStr);
    }



    /***************************************************
    ********** Event handlers & listeners **************
    ***************************************************/
    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!IsHumanPlayer(player))
        {
            return HookResult.Continue;
        }


        ModifyPlayerCount(1);
        StopIdleTimeout();

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!IsHumanPlayer(player))
        {
            return HookResult.Continue;
        }


        ModifyPlayerCount(-1);

        // Start timer if server is empty
        if (IsServerEmpty())
        {
            StartIdleTimeout(Config.Delay);
        }

        return HookResult.Continue;
    }

    private void OnMapStart(string mapName)
    {
        // Increment map change count - pretty sure it will never overflow ;)
        _mapChangeCount++;

        if (Config.ChangeInitial && Config.WorkshopCollection)
        {
            var mapChangeTriggered = SetDefaultMap();

            if (mapChangeTriggered)
            {
                // Return early
                return;
            }
        }
        
        // Reset and recalculate player count
        RefreshPlayerCount();

        if (IsServerEmpty())
        {
            HandleEmptyServerOnMapStart();
        }

        DebugLog($"Map started: {mapName}. Map change count: {_mapChangeCount}");
    }

    private void RefreshPlayerCount()
    {
        lock (_playerCountLock)
        {
            _playerCount = Utilities.GetPlayers().Count(IsHumanPlayer);
        }
    }

    private void HandleEmptyServerOnMapStart()
    {
        if (_mapChangeCount >= 3)
        {
            StartIdleTimeout(Config.Delay);
            DebugLog("No players connected. Starting idle timeout.");
        }
    }

    private bool SetDefaultMap()
    {
        // Depending on if a workshop collection/map is used or not, the server will change a second time.
        // Thus, for default maps, trigger a map change if _mapChangeCount = 1, and for workshop maps if _mapChangeCount = 2.
        var triggerMapChange = Config.WorkshopCollection ? _mapChangeCount == 2 : _mapChangeCount == 1;

        if (triggerMapChange)
        {
            // As immediate map change crashes the server, a short idle timeout is started.
            StartIdleTimeout(3);
            return true;
        }

        return false;
    }



    /***************************************************
    **************** Timer methods *********************
    ***************************************************/
    private void StartIdleTimeout(float delay)
    {
        if (_idleTimeout != null)
            return;

        DebugLog($"Starting idle timeout... Delay: {delay}");
        _idleTimeout = AddTimer(delay, MapChange);
    }

    private void StopIdleTimeout()
    {
        if (_idleTimeout == null)
            return;

        DebugLog("Stopping idle timeout...");
        _idleTimeout.Kill();
        _idleTimeout = null;
    }



    /***************************************************
    **************** Server control ********************
    ***************************************************/
    private void MapChange()
    {
        StopIdleTimeout();
        Console.WriteLine("Map change or idle timeout occurred.");
        ChangeMap(Config.DefaultMap);
    }

    private void ChangeMap(string mapName)
    {
        var isWorkshopMap = mapName.StartsWith("ws:");
        var map = isWorkshopMap ? mapName[3..] : mapName;
        var command = isWorkshopMap ? $"host_workshop_map {map}" : $"map {map}";

        DebugLog($"Changing map to {(isWorkshopMap ? "workshop" : "regular")} map: {map}");
        Server.ExecuteCommand(command);
    }



    /***************************************************
    *************** Utility methods ********************
    ***************************************************/
    private void ModifyPlayerCount(int delta)
    {
        lock (_playerCountLock)
        {
            _playerCount = Math.Max(0, _playerCount + delta);
        }

        var action = delta > 0 ? "incremented" : "decremented";
        DebugLog($"Player count {action}. Current player count: {_playerCount}");
    }

    private bool IsServerEmpty()
    {
        return  0 == _playerCount;
    }

    // Check if the player is human
    private static bool IsHumanPlayer(CCSPlayerController? player)
    {
        return player?.IsValid == true && !player.IsBot && !player.IsHLTV;
    }


    private void DebugLog(string message, ConsoleColor color = ConsoleColor.DarkYellow)
    {
        if (!Config.Debug)
            return;

        Console.ForegroundColor = color;
        Console.WriteLine($"IM DEBUG: {message}");
        Console.ResetColor();
    }
}