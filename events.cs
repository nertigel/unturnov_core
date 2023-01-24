using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Locations;
using OpenMod.Unturned.Players.Connections.Events;
using OpenMod.Unturned.Players.Life.Events;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using unturnov_core.utils;

/*
    PlayerInventory.SLOTS // 2
*/

namespace unturnov_core.events
{
    public class UnturnedPlayerDeathEventListener : IEventListener<UnturnedPlayerDeathEvent>
    {
        private readonly unturnov_core m_Plugin;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly ILogger<UnturnedPlayerDeathEventListener> m_Logger;
        private readonly IPermissionChecker m_PermissionChecker;

        public UnturnedPlayerDeathEventListener(unturnov_core plugin,
            IUnturnedUserDirectory unturnedUserDirectory,
            ILogger<UnturnedPlayerDeathEventListener> logger, 
            IPermissionChecker permissionChecker)
        {
            m_Plugin = plugin;
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_Logger = logger;
            m_PermissionChecker = permissionChecker;
        }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public async Task HandleEventAsync(object sender, UnturnedPlayerDeathEvent @event)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            await UniTask.SwitchToMainThread();

            UnturnedUser unturnedUser = m_UnturnedUserDirectory.FindUser(@event.Player.SteamId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (unturnedUser != null) {
                DropDogtagOnDeath(unturnedUser);
                PerformInventoryScan(unturnedUser);
            }
        }

        void DropDogtagOnDeath(UnturnedUser unturnedUser)
        {
            Vector3 playerPosition = unturnedUser.Player.Transform.Position;
            UnityEngine.Vector3 pos; pos.x = playerPosition.X; pos.y = playerPosition.Y; pos.z = playerPosition.Z;
            var item = new Item(26019, true); // stg dog-tag
            ItemManager.dropItem(item, pos, false, true, false);
        }

        void PerformInventoryScan(UnturnedUser unturnedUser)
        {
            string copyright = "stop looking here you fucking faggot, this was made by Nertigel#5391"; copyright += " fuck pasters.";
            Vector3 pos = unturnedUser.Player.Transform.Position;
            UnityEngine.Vector3 playerPosition; playerPosition.x = pos.X; playerPosition.y = pos.Y; playerPosition.z = pos.Z;

            //save all items in hands slots
            List<Item> old_items = new List<Item>();
            if (unturnedUser.Player.Player.inventory.items[PlayerInventory.SLOTS] != null)
            {
                var count = unturnedUser.Player.Player.inventory.getItemCount(PlayerInventory.SLOTS);
                for (byte index = 0; index < count; index++) {
                    var _item = unturnedUser.Player.Player.inventory.getItem(PlayerInventory.SLOTS, index);
                    if (_item != null)
                    {
                        old_items.Add(_item.item);
                    }
                }
            }

            DequipAndDropClothes(unturnedUser);

            //remove all items
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (unturnedUser.Player.Player.inventory.items[page] != null && (page != PlayerInventory.AREA && page != PlayerInventory.STORAGE))
                {
                    var count = unturnedUser.Player.Player.inventory.getItemCount(page);
                    for (byte index = 0; index < count; index++) {
                        var _item = unturnedUser.Player.Player.inventory.getItem(page, index);
                        if (_item != null) {
                            if (page == PlayerInventory.SLOTS) {
                                unturnedUser.Player.Player.inventory.removeItem(page, index);
                            }
                            else {
#pragma warning disable CS0612 // Type or member is obsolete
                                unturnedUser.Player.Player.inventory.askDropItem(unturnedUser.Player.SteamId, page, _item.x, _item.y);
#pragma warning restore CS0612 // Type or member is obsolete
                            }
                        }
                    }
                }
            }

            //give old items from hands back
            foreach (Item item in old_items)
            {
                unturnedUser.Player.Player.inventory.forceAddItem(item, true);
            }
        }
        void DequipAndDropClothes(UnturnedUser unturnedUser) {
            List<ushort> clothes = new List<ushort>();
            clothes.Add(unturnedUser.Player.Player.clothing.backpack);
            clothes.Add(unturnedUser.Player.Player.clothing.glasses);
            clothes.Add(unturnedUser.Player.Player.clothing.hat);
            clothes.Add(unturnedUser.Player.Player.clothing.pants);
            clothes.Add(unturnedUser.Player.Player.clothing.mask);
            clothes.Add(unturnedUser.Player.Player.clothing.shirt);
            clothes.Add(unturnedUser.Player.Player.clothing.vest);

            unturnedUser.Player.Player.clothing.askWearBackpack(0, 0, new byte[0], true);
            unturnedUser.Player.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
            unturnedUser.Player.Player.clothing.askWearHat(0, 0, new byte[0], true);
            unturnedUser.Player.Player.clothing.askWearPants(0, 0, new byte[0], true);
            unturnedUser.Player.Player.clothing.askWearMask(0, 0, new byte[0], true);
            unturnedUser.Player.Player.clothing.askWearShirt(0, 0, new byte[0], true);
            unturnedUser.Player.Player.clothing.askWearVest(0, 0, new byte[0], true);

            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (unturnedUser.Player.Player.inventory.items[page] != null && (page != PlayerInventory.AREA && page != PlayerInventory.STORAGE))
                {
                    var count = unturnedUser.Player.Player.inventory.getItemCount(page);
                    for (byte index = 0; index < count; index++)
                    {
                        var _item = unturnedUser.Player.Player.inventory.getItem(page, index);
                        if (_item != null) {
                            if (clothes.Contains(_item.item.id)) {
#pragma warning disable CS0612 // Type or member is obsolete
                                unturnedUser.Player.Player.inventory.askDropItem(unturnedUser.Player.SteamId, page, _item.x, _item.y);
#pragma warning restore CS0612 // Type or member is obsolete
                                clothes.Remove(_item.item.id);
                            }
                        }
                    }
                }
            }
        }
    }
    public class UnturnedPlayerConnectedEventListener : IEventListener<UnturnedPlayerConnectedEvent>
    {
        private readonly unturnov_core m_Plugin;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly IPermissionChecker m_PermissionChecker;

        public UnturnedPlayerConnectedEventListener(unturnov_core plugin,
            IUnturnedUserDirectory unturnedUserDirectory,
            IPermissionChecker permissionChecker
            )
        {
            m_Plugin = plugin;
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_PermissionChecker = permissionChecker;
        }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public async Task HandleEventAsync(object sender, UnturnedPlayerConnectedEvent @event)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            await UniTask.SwitchToMainThread();

            UnturnedUser unturnedUser = m_UnturnedUserDirectory.FindUser(@event.Player.SteamId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (unturnedUser != null)
            {
                byte width = 3;
                byte height = 3;

                if (await m_PermissionChecker.CheckPermissionAsync(unturnedUser, "leftbehind") == PermissionGrantResult.Grant) {
                    width = 4; height = 3;
                } else if (await m_PermissionChecker.CheckPermissionAsync(unturnedUser, "prepareforescape") == PermissionGrantResult.Grant) {
                    width = 4; height = 4;
                } else if (await m_PermissionChecker.CheckPermissionAsync(unturnedUser, "edgeofdarkness") == PermissionGrantResult.Grant) {
                    width = 5; height = 4;
                }

                unturnedUser.Player.Player.inventory.ReceiveSize(PlayerInventory.SLOTS, width, height);
            }
        }
    }
}
