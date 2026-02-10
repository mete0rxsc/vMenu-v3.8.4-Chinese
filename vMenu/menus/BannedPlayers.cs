using System;
using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class BannedPlayers
    {
        // Variables
        private Menu menu;

        /// <summary>
        /// Struct used to store bans.
        /// </summary>
        public struct BanRecord
        {
            public string playerName;
            public List<string> identifiers;
            public DateTime bannedUntil;
            public string banReason;
            public string bannedBy;
            public string uuid;
        }

        BanRecord currentRecord = new();

        public List<BanRecord> banlist = new();

        readonly Menu bannedPlayer = new("封禁玩家", "Ban Record: ");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, "封禁玩家管理");

            menu.InstructionalButtons.Add(Control.Jump, "过滤选项");
            menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>(async (a, b) =>
            {
                if (banlist.Count > 1)
                {
                    var filterText = await GetUserInput("筛选器用户名或禁止id（留空以重置筛选器)");
                    if (string.IsNullOrEmpty(filterText))
                    {
                        Subtitle.Custom("过滤器已清除.");
                        menu.ResetFilter();
                        UpdateBans();
                    }
                    else
                    {
                        menu.FilterMenuItems(item => item.ItemData is BanRecord br && (br.playerName.ToLower().Contains(filterText.ToLower()) || br.uuid.ToLower().Contains(filterText.ToLower())));
                        Subtitle.Custom("过滤器已应用.");
                    }
                }
                else
                {
                    Notify.Error("至少需要禁止2名玩家才能使用过滤功能.");
                }

                Log($"Button pressed: {a} {b}");
            }), true));

            bannedPlayer.AddMenuItem(new MenuItem("玩家名称"));
            bannedPlayer.AddMenuItem(new MenuItem("封禁处理者"));
            bannedPlayer.AddMenuItem(new MenuItem("封禁直到"));
            bannedPlayer.AddMenuItem(new MenuItem("玩家标识符"));
            bannedPlayer.AddMenuItem(new MenuItem("封禁原因"));
            bannedPlayer.AddMenuItem(new MenuItem("~r~取消封禁", "~r~警告，取消暂停玩家无法撤消。在他们重新加入服务器之前，您将无法再次禁止他们。你确定要解除这个玩家的禁令吗？~s~提示：如果被封禁的玩家在封禁日期到期后登录服务器，他们将自动取消禁封禁"));

            // should be enough for now to cover all possible identifiers.
            var colors = new List<string>() { "~r~", "~g~", "~b~", "~o~", "~y~", "~p~", "~s~", "~t~", };

            bannedPlayer.OnMenuClose += (sender) =>
            {
                BaseScript.TriggerServerEvent("vMenu:RequestBanList", Game.Player.Handle);
                bannedPlayer.GetMenuItems()[5].Label = "";
                UpdateBans();
            };

            bannedPlayer.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                bannedPlayer.GetMenuItems()[5].Label = "";
            };

            bannedPlayer.OnItemSelect += (sender, item, index) =>
            {
                if (index == 5 && IsAllowed(Permission.OPUnban))
                {
                    if (item.Label == "你确定？")
                    {
                        if (banlist.Contains(currentRecord))
                        {
                            UnbanPlayer(banlist.IndexOf(currentRecord));
                            bannedPlayer.GetMenuItems()[5].Label = "";
                            bannedPlayer.GoBack();
                        }
                        else
                        {
                            Notify.Error("不知怎么的，你设法点击了取消禁令按钮，但你正在查看的这条禁令记录似乎根本不存在。奇怪的..");
                        }
                    }
                    else
                    {
                        item.Label = "你确定？";
                    }
                }
                else
                {
                    bannedPlayer.GetMenuItems()[5].Label = "";
                }

            };

            menu.OnItemSelect += (sender, item, index) =>
            {
                currentRecord = item.ItemData;

                bannedPlayer.MenuSubtitle = "封禁原因: ~y~" + currentRecord.playerName;
                var nameItem = bannedPlayer.GetMenuItems()[0];
                var bannedByItem = bannedPlayer.GetMenuItems()[1];
                var bannedUntilItem = bannedPlayer.GetMenuItems()[2];
                var playerIdentifiersItem = bannedPlayer.GetMenuItems()[3];
                var banReasonItem = bannedPlayer.GetMenuItems()[4];
                nameItem.Label = currentRecord.playerName;
                nameItem.Description = "玩家名称: ~y~" + currentRecord.playerName;
                bannedByItem.Label = currentRecord.bannedBy;
                bannedByItem.Description = "此玩家封禁执行者: ~y~" + currentRecord.bannedBy;
                if (currentRecord.bannedUntil.Date.Year == 3000)
                {
                    bannedUntilItem.Label = "永远(真的很远)";
                }
                else
                {
                    bannedUntilItem.Label = currentRecord.bannedUntil.Date.ToString();
                }

                bannedUntilItem.Description = "这名玩家被封禁直到: " + currentRecord.bannedUntil.Date.ToString();
                playerIdentifiersItem.Description = "";

                var i = 0;
                foreach (var id in currentRecord.identifiers)
                {
                    // only (admins) people that can unban players are allowed to view IP's.
                    // this is just a slight 'safety' feature in case someone who doesn't know what they're doing
                    // gave builtin.everyone access to view the banlist.
                    if (id.StartsWith("ip:") && !IsAllowed(Permission.OPUnban))
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}ip: (hidden) ";
                    }
                    else
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}{id.Replace(":", ": ")} ";
                    }
                    i++;
                }
                banReasonItem.Description = "Banned for: " + currentRecord.banReason;

                var unbanPlayerBtn = bannedPlayer.GetMenuItems()[5];
                unbanPlayerBtn.Label = "";
                if (!IsAllowed(Permission.OPUnban))
                {
                    unbanPlayerBtn.Enabled = false;
                    unbanPlayerBtn.Description = "你不允许解除小哑巴的禁令。您只允许查看他们的禁令记录.";
                    unbanPlayerBtn.LeftIcon = MenuItem.Icon.LOCK;
                }

                bannedPlayer.RefreshIndex();
            };
            MenuController.AddMenu(bannedPlayer);

        }

        /// <summary>
        /// Updates the ban list menu.
        /// </summary>
        public void UpdateBans()
        {
            menu.ResetFilter();
            menu.ClearMenuItems();

            foreach (var ban in banlist)
            {
                var recordBtn = new MenuItem(ban.playerName, $"~y~{ban.playerName}~s~ 被 ~y~{ban.bannedBy}~s~ 封禁直到 ~y~{ban.bannedUntil}~s~ 封禁原因 ~y~{ban.banReason}~s~.")
                {
                    Label = "→→→",
                    ItemData = ban
                };
                menu.AddMenuItem(recordBtn);
                MenuController.BindMenuItem(menu, bannedPlayer, recordBtn);
            }
            menu.RefreshIndex();
        }

        /// <summary>
        /// Updates the list of ban records.
        /// </summary>
        /// <param name="banJsonString"></param>
        public void UpdateBanList(string banJsonString)
        {
            banlist.Clear();
            banlist = JsonConvert.DeserializeObject<List<BanRecord>>(banJsonString);
            UpdateBans();
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }

            return menu;
        }

        /// <summary>
        /// Sends an event to the server requesting the player to be unbanned.
        /// We'll just assume that worked fine, so remove the item from our local list, we'll re-sync once the menu is re-opened.
        /// </summary>
        /// <param name="index"></param>
        private void UnbanPlayer(int index)
        {
            var record = banlist[index];
            banlist.Remove(record);
            BaseScript.TriggerServerEvent("vMenu:RequestPlayerUnban", record.uuid);
        }

        /// <summary>
        /// Converts the ban record (json object) into a BanRecord struct.
        /// </summary>
        /// <param name="banRecordJsonObject"></param>
        /// <returns></returns>
        public static BanRecord JsonToBanRecord(dynamic banRecordJsonObject)
        {
            var newBr = new BanRecord();
            foreach (Newtonsoft.Json.Linq.JProperty brValue in banRecordJsonObject)
            {
                var key = brValue.Name.ToString();
                var value = brValue.Value;
                if (key == "playerName")
                {
                    newBr.playerName = value.ToString();
                }
                else if (key == "identifiers")
                {
                    var tmpList = new List<string>();
                    foreach (var identifier in value)
                    {
                        tmpList.Add(identifier.ToString());
                    }
                    newBr.identifiers = tmpList;
                }
                else if (key == "bannedUntil")
                {
                    newBr.bannedUntil = DateTime.Parse(value.ToString());
                }
                else if (key == "banReason")
                {
                    newBr.banReason = value.ToString();
                }
                else if (key == "bannedBy")
                {
                    newBr.bannedBy = value.ToString();
                }
            }
            return newBr;
        }
    }
}
