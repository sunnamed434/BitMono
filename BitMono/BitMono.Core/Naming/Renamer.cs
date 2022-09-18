using BitMono.API.Naming;
using dnlib.DotNet;
using System;

namespace BitMono.Core.Naming
{
    public class Renamer : IRenamer
    {
        private readonly string[] strings =
        {
            "HasPermission", "HasPermissions", "GetPermissions", "GetOpenWindows", "EnumWindows", "GetWindowText", "GetWindowTextLength",
            "IsWindowVisible", "GetShellWindow", "Awake", "FixedUpdate", "add_OnRockedInitialized", "remove_OnRockedInitialized",
            "Awake", "Initialize", "Translate", "Reload", "<Initialize>b__13_0", "Initialize", "FixedUpdate", "Start", "checkTimerRestart",
            "QueueOnMainThread", "QueueOnMainThread", "RunAsync", "RunAction", "Awake", "FixedUpdate", "IsUri", "GetTypes", "GetTypesFromParentClass",
            "GetTypesFromParentClass", "GetTypesFromInterface", "GetTypesFromInterface", "get_Timeout", "set_Timeout", "GetWebRequest", "get_SteamID64",
            "set_SteamID64", "get_SteamID", "set_SteamID", "get_OnlineState", "set_OnlineState", "get_StateMessage", "set_StateMessage",
            "get_PrivacyState", "set_PrivacyState", "get_VisibilityState", "set_VisibilityState", "get_AvatarIcon", "set_AvatarIcon",
            "get_AvatarMedium", "set_AvatarMedium", "get_AvatarFull", "set_AvatarFull", "get_IsVacBanned", "set_IsVacBanned", "get_TradeBanState",
            "set_TradeBanState", "get_IsLimitedAccount", "set_IsLimitedAccount", "get_CustomURL", "set_CustomURL", "get_MemberSince", "set_MemberSince",
            "get_HoursPlayedLastTwoWeeks", "set_HoursPlayedLastTwoWeeks", "get_Headline", "set_Headline", "get_Location", "set_Location", "get_RealName",
            "set_RealName", "get_Summary", "set_Summary", "get_MostPlayedGames", "set_MostPlayedGames", "get_Groups", "set_Groups", "Reload",
            "ParseString", "ParseDateTime", "ParseDouble", "ParseUInt16", "ParseUInt32", "ParseUInt64", "ParseBool", "ParseUri", "IsValidCSteamID",
            "LoadDefaults", "LoadDefaults", "get_Clients", "Awake", "handleConnection", "FixedUpdate", "Broadcast", "OnDestroy", "Read", "Send",
            "<Awake>b__8_0", "get_InstanceID", "set_InstanceID", "get_ConnectedTime", "set_ConnectedTime", "Send", "Read", "Close", "get_Address",
            "get_Instance", "set_Instance", "Save", "Load", "Unload", "Load", "Save", "Load", "get_Configuration", "LoadPlugin", "<.ctor>b__3_0",
            "<LoadPlugin>b__4_0", "add_OnPluginUnloading", "remove_OnPluginUnloading", "add_OnPluginLoading", "remove_OnPluginLoading",
            "get_Translations", "get_State", "get_Assembly", "set_Assembly", "get_Directory", "set_Directory", "get_Name", "set_Name",
            "get_DefaultTranslations", "IsDependencyLoaded", "ExecuteDependencyCode", "Translate", "ReloadPlugin", "LoadPlugin", "UnloadPlugin",
            "OnEnable", "OnDisable", "Load", "Unload", "TryAddComponent", "TryRemoveComponent", "add_OnPluginsLoaded", "remove_OnPluginsLoaded",
            "get_Plugins", "GetPlugins", "GetPlugin", "GetPlugin", "Awake", "Start", "GetMainTypeFromAssembly", "loadPlugins", "unloadPlugins",
            "Reload", "GetAssembliesFromDirectory", "LoadAssembliesFromDirectory", "<Awake>b__12_0", "GetGroupsByIds", "GetParentGroups",
            "HasPermission", "GetGroup", "RemovePlayerFromGroup", "AddPlayerToGroup", "DeleteGroup", "SaveGroup", "AddGroup", "GetGroups",
            "GetPermissions", "GetPermissions", "<GetGroups>b__11_3", "Start", "FixedUpdate", "Reload", "HasPermission", "GetGroups", "GetPermissions",
            "GetPermissions", "AddPlayerToGroup", "RemovePlayerFromGroup", "GetGroup", "SaveGroup", "AddGroup", "DeleteGroup", "DeleteGroup",
            "<FixedUpdate>b__4_0", "Enqueue", "_Logger_DoWork", "processLog", "Log", "Log", "var_dump", "LogWarning", "LogError", "LogError",
            "Log", "LogException", "ProcessInternalLog", "logRCON", "writeToConsole", "ProcessLog", "ExternalLog", "Invoke", "_invoke",
            "TryInvoke", "get_Aliases", "get_AllowedCaller", "get_Help", "get_Name", "get_Permissions", "get_Syntax", "Execute", "get_Aliases",
            "get_AllowedCaller", "get_Help", "get_Name", "get_Permissions", "get_Syntax", "Execute", "get_Aliases", "get_AllowedCaller", "get_Help",
            "get_Name", "get_Permissions", "get_Syntax", "Execute", "get_Name", "set_Name", "get_Name", "set_Name", "get_Name", "get_Help", "get_Syntax",
            "get_AllowedCaller", "get_Commands", "set_Commands", "add_OnExecuteCommand", "remove_OnExecuteCommand", "Reload", "Awake",
            "checkCommandMappings", "checkDuplicateCommandMappings", "Plugins_OnPluginsLoaded", "GetCommand", "GetCommand", "getCommandIdentity",
            "getCommandType", "Register", "Register", "Register", "DeregisterFromAssembly", "GetCooldown", "SetCooldown", "Execute",
            "RegisterFromAssembly", "Assembly.Load", "AppDomain.Current.Load", "ExternalLoad", "ExternalAssemblyLoad", "MethodInvokableCallback",
            "FireEventCallback", "RegisterAssemblyFromNative", "Memory", "Plugin", "External", "Logger.log", "Logging.log", "Logs"
        };

        private readonly Random random = new Random();

        public string Rename()
        {
            string randomStringOne = strings[random.Next(0, strings.Length - 1)] + " " + strings[random.Next(0, strings.Length - 1)];
            string randomStringTwo = strings[random.Next(0, strings.Length - 1)];
            string randomStringThree = strings[random.Next(0, strings.Length - 1)];
            return $"{randomStringTwo} {randomStringOne}.{randomStringThree}";
        }

        public void Rename(IFullName fullName)
        {
            fullName.Name = Rename();
        }

        public void Rename(params IFullName[] fullNames)
        {
            for (int i = 0; i < fullNames.Length; i++)
            {
                Rename(fullNames[i]);
            }
        }

        public void Rename(IVariable variable)
        {
            variable.Name = Rename();
        }
    }
}