using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace IdleManager
{
    public partial class IdleManager
    {
        /***************************************************
        **************** Command Handlers *******************
        ***************************************************/

        [ConsoleCommand("im", "Displays the current status of the Idle Manager plugin.")]
        [RequiresPermissions("@im/admin")]
        public void StatusCommand(CCSPlayerController? player, CommandInfo commandInfo)
        {
            var commandType = commandInfo.GetArg(1).ToLower();

            switch (commandType)
            {
                case "status":
                    ShowStatus(commandInfo);
                    break;
                default:
                    ShowPluginInfo(commandInfo);
                    break;
            }
        }

        private void ShowStatus(CommandInfo commandInfo)
        {
            var timerStarted = _idleTimeout != null ? "Yes" : "No";

            var response = $"""
            Idle Manager status:
            Map change count:            {_mapChangeCount}
            Current player count:        {_playerCount}
            Idle timeout delay:          {Config.Delay}s
            Default map:                 {Config.DefaultMap}
            Workshop collection enabled: {Config.WorkshopCollection}
            Debug mode:                  {Config.Debug}
            Timer started:               {timerStarted}
            """;

            Console.ForegroundColor = ConsoleColor.Green;
            commandInfo.ReplyToCommand(response);
            Console.ResetColor();
        }

        private void ShowPluginInfo(CommandInfo commandInfo)
        {
            var response = $"""
            Idle Manager Plugin Info:
            Module Name:    {ModuleName}
            Module Version: {ModuleVersion}
            """;

            commandInfo.ReplyToCommand(response);
        }
    }
}