using Dalamud.Game.Command;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Globalization;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;
using PeepingTom.Resources;
using XivCommon;

namespace PeepingTom {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        internal static string Name => "Peeping Tom";

        [PluginService]
        internal static IPluginLog Log { get; private set; } = null!;

        [PluginService]
        internal DalamudPluginInterface Interface { get; init; } = null!;

        [PluginService]
        internal IChatGui ChatGui { get; init; } = null!;

        [PluginService]
        internal IClientState ClientState { get; init; } = null!;

        [PluginService]
        private ICommandManager CommandManager { get; init; } = null!;

        [PluginService]
        internal ICondition Condition { get; init; } = null!;

        [PluginService]
        internal IDataManager DataManager { get; init; } = null!;

        [PluginService]
        internal IFramework Framework { get; init; } = null!;

        [PluginService]
        internal IGameGui GameGui { get; init; } = null!;

        [PluginService]
        internal IObjectTable ObjectTable { get; init; } = null!;

        [PluginService]
        internal ITargetManager TargetManager { get; init; } = null!;

        [PluginService]
        internal IToastGui ToastGui { get; init; } = null!;

        internal Configuration Config { get; }
        internal PluginUi Ui { get; }
        internal TargetWatcher Watcher { get; }
        internal XivCommonBase Common { get; }
        internal IpcManager IpcManager { get; }

        internal bool InPvp { get; private set; }

        public Plugin() {
            this.Common = new XivCommonBase();
            this.Config = this.Interface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Config.Initialize(this.Interface);
            this.Watcher = new TargetWatcher(this);
            this.Ui = new PluginUi(this);
            this.IpcManager = new IpcManager(this);

            OnLanguageChange(this.Interface.UiLanguage);
            this.Interface.LanguageChanged += OnLanguageChange;

            this.CommandManager.AddHandler("/ppeepingtom", new CommandInfo(this.OnCommand) {
                HelpMessage = "Use with no arguments to show the list. Use with \"c\" or \"config\" to show the config",
            });
            this.CommandManager.AddHandler("/ptom", new CommandInfo(this.OnCommand) {
                HelpMessage = "Alias for /ppeepingtom",
            });
            this.CommandManager.AddHandler("/ppeep", new CommandInfo(this.OnCommand) {
                HelpMessage = "Alias for /ppeepingtom",
            });

            this.ClientState.Login += this.OnLogin;
            this.ClientState.Logout += this.OnLogout;
            this.ClientState.TerritoryChanged += this.OnTerritoryChange;
            this.Interface.UiBuilder.Draw += this.DrawUi;
            this.Interface.UiBuilder.OpenConfigUi += this.ConfigUi;
        }

        public void Dispose() {
            this.Interface.UiBuilder.OpenConfigUi -= this.ConfigUi;
            this.Interface.UiBuilder.Draw -= this.DrawUi;
            this.ClientState.TerritoryChanged -= this.OnTerritoryChange;
            this.ClientState.Logout -= this.OnLogout;
            this.ClientState.Login -= this.OnLogin;
            this.CommandManager.RemoveHandler("/ppeep");
            this.CommandManager.RemoveHandler("/ptom");
            this.CommandManager.RemoveHandler("/ppeepingtom");
            this.Interface.LanguageChanged -= OnLanguageChange;
            this.IpcManager.Dispose();
            this.Ui.Dispose();
            this.Watcher.Dispose();
            this.Common.Dispose();
        }

        private static void OnLanguageChange(string langCode) {
            Language.Culture = new CultureInfo(langCode);
        }

        private void OnTerritoryChange(ushort e) {
            try {
                var territory = this.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(e);
                this.InPvp = territory?.IsPvpZone == true;
            } catch (KeyNotFoundException) {
                Log.Warning("Could not get territory for current zone");
            }
        }

        private void OnCommand(string command, string args) {
            if (args is "config" or "c") {
                this.Ui.SettingsOpen = true;
            } else {
                this.Ui.WantsOpen = true;
            }
        }

        private void OnLogin() {
            if (!this.Config.OpenOnLogin) {
                return;
            }

            this.Ui.WantsOpen = true;
        }

        private void OnLogout() {
            this.Ui.WantsOpen = false;
            this.Watcher.ClearPrevious();
        }

        private void DrawUi() {
            this.Ui.Draw();
        }

        private void ConfigUi() {
            this.Ui.SettingsOpen = true;
        }
    }
}
