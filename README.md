# CounterstrikeSharp - CS2-IdleManager

## Description

This CounterstrikeSharp plugin automatically changes the server map after a configurable delay when the server is empty (idle). Beyond facilitating an automatic return to a preferred map, it also addresses the following common issues:

 -   **Initial Map Selection for Workshop Mapgroups:** Workshop mapgroups typically don't allow direct selection of a starting map. This plugin enables an initial map change to your desired map immediately after server startup. Refer to the `ChangeInitial` and `WorkshopCollection` parameters for details.

-   **Mitigating Server Lag:** Extended periods of server idleness (1-3 days?!) can lead to significant lag indicated by 'buffer bloat' messages in the server console. Regularly changing the map effectively mitigates this issue.


## Configuration

The configuration file is located at `counterstrikesharp/configs/plugins/IdleManager/` and is automatically generated upon the plugin's initial startup.

### Delay

The time in seconds after which the server changes the map if it is empty.

### DefaultMap

Specifies the map to switch to once the idle delay has expired or after server startup (if `ChangeInitial` is enabled). For workshop maps, prepend `ws:` followed by the workshop map ID. E.g., `ws:123456789`.

### ChangeInitial

When enabled, triggers an immediate map change to the `DefaultMap` after the server has started.

### WorkshopCollection

Set this parameter to `True` if you use a workshop collection.

**Explanation:**

If a workshop map or collection is used, the server firstly starts with a default map and immediately changes to the defined workshop map or one workshop map out of the given workshop collection.
The *ChangeInitial* parameter decides if the initial map change is triggerd after the first mapchange (starting the server is a mapchange) or the second mapchange (default map, then workshop map).
Currently it seems there is no automatic way to detect if a workshop collection is used.

### Debug

Enable debug outputs.


## Commands

The plugin supports the *im \<cmd\>* command to get more or less useful informations.

Just typing *im*, you get information about the plugin.

Add *@im/admin* permission to get access to the command.

Following a (currently) very small list of commands (\<cmd\>)

### status

Shows information about player count, map change count and config.
