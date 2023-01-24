using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using SDG.Unturned;
using unturnov_core.utils;
using System.Collections.Generic;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using OpenMod.API.Permissions;

// For more, visit https://openmod.github.io/openmod-docs/devdoc/guides/getting-started.html

[assembly: PluginMetadata("Nertigel.EfUCore", Author = "Nertigel", DisplayName = "EfU Core", Website = "https://steamcommunity.com/id/nertigel")]
namespace unturnov_core
{
    public class unturnov_core : OpenModUnturnedPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly ILogger<unturnov_core> m_Logger;
        private readonly IPermissionRegistry m_PermissionRegistry;

        public unturnov_core(
            IConfiguration configuration,
            ILogger<unturnov_core> logger,
            IPermissionRegistry permissionRegistry,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_Logger = logger;
            m_PermissionRegistry = permissionRegistry;
        }

        protected override async UniTask OnLoadAsync()
        {
            await UniTask.SwitchToMainThread();
            m_Logger.LogInformation("Nertigel's Unturnov Core loaded. Anti-N1G3R-Fag0t Club");

            m_PermissionRegistry.RegisterPermission(this, "leftbehind", "donator rank permissions");
            m_PermissionRegistry.RegisterPermission(this, "prepareforescape", "donator rank permissions");
            m_PermissionRegistry.RegisterPermission(this, "edgeofdarkness", "donator rank permissions");

            // await UniTask.SwitchToThreadPool();

            //return UniTask.CompletedTask;
        }

        protected override async UniTask OnUnloadAsync()
        {
            await UniTask.SwitchToMainThread();
            m_Logger.LogInformation("Nertigel's Unturnov Core un-loaded.");
        }

        public void FixedUpdate() // called every tick
        {

        }
    }
}
