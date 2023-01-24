using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Commands;
using OpenMod.API.Commands;
using OpenMod.Core.Users;
using OpenMod.API.Users;
using Microsoft.Extensions.Logging;
using SDG.Unturned;

namespace unturnov_core.command_skills
{
    [Command("skills")] // perm: Nertigel.EfUCore:commands.skills
    [CommandAlias("maxskills")]
    [CommandDescription("grants max skills if they ever glitch/bug")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandSkills : UnturnedCommand
    {
        public CommandSkills(IServiceProvider serviceProvider) : base(serviceProvider) { }

        // use UniTask instead of Task if derivered from UnityEngineCommand or UnturnedCommand
        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser player = (UnturnedUser)Context.Actor;
            await UniTask.SwitchToMainThread();
            player.Player.Player.skills.ServerUnlockAllSkills();
            await Context.Actor.PrintMessageAsync("[EfU] Your skills have been set to the maximum level.", System.Drawing.Color.LawnGreen);
        }
    }
}

// credits: https://github.com/Kr4ken-9/NewEssentials/blob/master/NewEssentials/Commands/Messaging/CTell.cs
namespace unturnov_core.command_message
{
    [Command("message")] // perm: Nertigel.EfUCore:commands.message
    [CommandAlias("pm")]
    [CommandAlias("dm")]
    [CommandAlias("msg")]
    [CommandAlias("tell")]
    [CommandDescription("Send a private message to a player")]
    [CommandSyntax("<player> <message>")]
    public class CommandMessage : UnturnedCommand
    {
        public CommandMessage(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 2)
                throw new CommandWrongUsageException(Context);

            var recipient = await Context.Parameters.GetAsync<UnturnedUser>(0);

            if (recipient == null)
                throw new UserFriendlyException("[EfU] Could not find a player with that name!");

#pragma warning disable CS8602
            recipient.Session.SessionData["lastMessager"] = Context.Actor.Type == KnownActorTypes.Player
                ? Context.Actor.Id
                : Context.Actor.Type;
#pragma warning restore CS8602
            var message = string.Join(" ", Context.Parameters.Skip(1));
            
            await Context.Actor.PrintMessageAsync("[EfU] sent [" + recipient.DisplayName + "]: " + message, System.Drawing.Color.LawnGreen);
            await recipient.PrintMessageAsync("[EfU] from [" + Context.Actor.DisplayName + "]: " + message, System.Drawing.Color.DarkGreen);
        }
    }
}

// credits: https://github.com/Kr4ken-9/NewEssentials/blob/master/NewEssentials/Commands/Messaging/CReply.cs
namespace unturnov_core.command_reply
{
    [Command("reply")] // perm: Nertigel.EfUCore:commands.reply
    [CommandAlias("r")]
    [CommandDescription("Reply to the last person that private messaged you")]
    [CommandSyntax("<message>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandReply : UnturnedCommand
    {
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly ILogger<unturnov_core> m_Logger;

        public CommandReply(IServiceProvider serviceProvider,
                            IUnturnedUserDirectory unturnedUserDirectory,
                            ILogger<unturnov_core> logger) : base(serviceProvider) {
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_Logger = logger;
        }

        protected override async UniTask OnExecuteAsync() {
            if (Context.Parameters.Length < 1)
                throw new CommandWrongUsageException(Context);

            var uPlayer = Context.Actor as UnturnedUser;

#pragma warning disable CS8602
            if (!uPlayer.Session.SessionData.ContainsKey("lastMessager"))
                throw new UserFriendlyException("[EfU] Could not find a player to reply to.");
#pragma warning restore CS8602

#pragma warning disable CS8600
            string lastMessagerID = (string)uPlayer.Session.SessionData["lastMessager"];
#pragma warning restore CS8600
            bool consoleMessage = lastMessagerID == "console";
            var message = string.Join(" ", Context.Parameters);

            if (!consoleMessage)
            {
#pragma warning disable CS8604
                var lastMessager = m_UnturnedUserDirectory.FindUser(lastMessagerID, UserSearchMode.FindById);
#pragma warning restore CS8604
                if (lastMessager == null)
                    throw new UserFriendlyException("[EfU] The player has disconnected.");

#pragma warning disable CS8602
                lastMessager.Session.SessionData["lastMessager"] = uPlayer.Id;
#pragma warning restore CS8602

                await Context.Actor.PrintMessageAsync("[EfU] sent [" + lastMessager.DisplayName + "]: " + message, System.Drawing.Color.LawnGreen);
                await lastMessager.PrintMessageAsync("[EfU] from [" + Context.Actor.DisplayName + "]: " + message, System.Drawing.Color.DarkGreen);

                return;
            }

            await Context.Actor.PrintMessageAsync("[EfU] sent [CONSOLE]: " + message, System.Drawing.Color.DarkRed);

            m_Logger.LogInformation("[EfU] from [" + Context.Actor.DisplayName + "]: "+ message);
        }
    }
}

// credits: https://github.com/Kr4ken-9/NewEssentials/blob/master/NewEssentials/Commands/CHeal.cs
namespace unturnov_core.command_heal
{
    [Command("heal")] // perm: Nertigel.EfUCore:commands.heal
    [CommandDescription("Heal yourself or another player")]
    [CommandSyntax("[player]")]
    public class CommandHeal : UnturnedCommand
    {
        public CommandHeal(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            await UniTask.SwitchToMainThread();

            void Heal(PlayerLife life)
            {
                life.askHeal(100, true, true);
                life.serverModifyFood(100);
                life.serverModifyWater(100);
                life.serverModifyStamina(100);
                life.serverModifyVirus(100);
            }

            if (Context.Parameters.Length == 0)
            {
                var player = (UnturnedUser)Context.Actor;

                Heal(player.Player.Player.life);

                await player.PrintMessageAsync("[EfU] You were healed.", System.Drawing.Color.LawnGreen);
            }
            else
            {
                var player = await Context.Parameters.GetAsync<UnturnedUser>(0);
                if (player == null)
                    throw new UserFriendlyException("[EfU] Could not find a player with that name!");

                Heal(player.Player.Player.life);

                await Context.Actor.PrintMessageAsync("[EfU] You healed " + player.DisplayName + ".", System.Drawing.Color.DarkGreen);
                await player.PrintMessageAsync("[EfU] You were healed.", System.Drawing.Color.LawnGreen);
            }
        }
    }
}