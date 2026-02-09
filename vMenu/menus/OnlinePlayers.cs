using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class OnlinePlayers
    {
        public List<int> PlayersWaypointList = new();
        public Dictionary<int, int> PlayerCoordWaypoints = new();

        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        readonly Menu playerMenu = new("在线玩家", "玩家：");
        IPlayer currentPlayer = new NativePlayer(Game.Player);


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "在线玩家")
            {
                CounterPreText = "玩家："
            };

            MenuController.AddSubmenu(menu, playerMenu);

            var sendMessage = new MenuItem("发送私聊", "向该玩家发送私聊。~r~注意：工作人员可能能够看到所有私聊。");
            var teleport = new MenuItem("传送到玩家", "传送到该玩家。");
            var teleportVeh = new MenuItem("传送进玩家的车辆", "传送进该玩家的车辆。");
            var summon = new MenuItem("召唤玩家", "将该玩家传送到你身边。");
            var toggleGPS = new MenuItem("切换GPS", "启用或禁用你雷达上指向该玩家的GPS路线。");
            var spectate = new MenuItem("观察玩家", "观察该玩家。再次点击此按钮可停止观察。");
            var printIdentifiers = new MenuItem("打印标识符", "这将打印玩家的标识符到客户端控制台(F8)。也会保存到CitizenFX.log文件中。");
            var kill = new MenuItem("~r~击杀玩家", "击杀该玩家，请注意他们将收到一条你击杀了他们的通知。这也会被记录在工作人员行为日志中。");
            var kick = new MenuItem("~r~踢出玩家", "从服务器踢出该玩家。");
            var ban = new MenuItem("~r~永久封禁玩家", "从服务器永久封禁该玩家。你确定要这样做吗？点击此按钮后你可以指定封禁原因。");
            var tempban = new MenuItem("~r~临时封禁玩家", "给该玩家一个最多30天的临时封禁。点击此按钮后你可以指定封禁时长和原因。");

            if (IsAllowed(Permission.OPSendMessage))
            {
                playerMenu.AddMenuItem(sendMessage);
            }
            if (IsAllowed(Permission.OPTeleport))
            {
                playerMenu.AddMenuItem(teleport);
                playerMenu.AddMenuItem(teleportVeh);
            }
            if (IsAllowed(Permission.OPSummon))
            {
                playerMenu.AddMenuItem(summon);
            }
            if (IsAllowed(Permission.OPSpectate))
            {
                playerMenu.AddMenuItem(spectate);
            }
            if (IsAllowed(Permission.OPWaypoint))
            {
                playerMenu.AddMenuItem(toggleGPS);
            }
            if (IsAllowed(Permission.OPIdentifiers))
            {
                playerMenu.AddMenuItem(printIdentifiers);
            }
            if (IsAllowed(Permission.OPKill))
            {
                playerMenu.AddMenuItem(kill);
            }
            if (IsAllowed(Permission.OPKick))
            {
                playerMenu.AddMenuItem(kick);
            }
            if (IsAllowed(Permission.OPTempBan))
            {
                playerMenu.AddMenuItem(tempban);
            }
            if (IsAllowed(Permission.OPPermBan))
            {
                playerMenu.AddMenuItem(ban);
                ban.LeftIcon = MenuItem.Icon.WARNING;
            }

            playerMenu.OnMenuClose += (sender) =>
            {
                playerMenu.RefreshIndex();
                ban.Label = "";
            };

            playerMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                ban.Label = "";
            };

            // handle button presses for the specific player's menu.
            playerMenu.OnItemSelect += async (sender, item, index) =>
            {
                // send message
                if (item == sendMessage)
                {
                    if (currentPlayer.Handle == Game.Player.Handle)
                    {
                        Notify.Error("你不能给自己发送消息！");
                        return;
                    }

                    if (MainMenu.MiscSettingsMenu != null && !MainMenu.MiscSettingsMenu.MiscDisablePrivateMessages)
                    {
                        var message = await GetUserInput($"私聊给 {currentPlayer.Name}", 200);
                        if (string.IsNullOrEmpty(message))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }
                        else
                        {
                            TriggerServerEvent("vMenu:SendMessageToPlayer", currentPlayer.ServerId, message);
                            PrivateMessage(currentPlayer.ServerId.ToString(), message, true);
                        }
                    }
                    else
                    {
                        Notify.Error("如果你自己禁用了私聊，就不能发送私聊。请在杂项设置菜单中启用它，然后重试。");
                    }

                }
                // teleport (in vehicle) button
                else if (item == teleport || item == teleportVeh)
                {
                    if (!currentPlayer.IsLocal)
                    {
                        _ = TeleportToPlayer(currentPlayer, item == teleportVeh); // teleport to the player. optionally in the player's vehicle if that button was pressed.
                    }
                    else
                    {
                        Notify.Error("你不能传送到自己！");
                    }
                }
                // summon button
                else if (item == summon)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                    {
                        SummonPlayer(currentPlayer);
                    }
                    else
                    {
                        Notify.Error("你不能召唤自己。");
                    }
                }
                // spectating
                else if (item == spectate)
                {
                    SpectatePlayer(currentPlayer);
                }
                // kill button
                else if (item == kill)
                {
                    KillPlayer(currentPlayer);
                }
                // manage the gps route being clicked.
                else if (item == toggleGPS)
                {
                    var selectedPedRouteAlreadyActive = false;
                    if (PlayersWaypointList.Count > 0)
                    {
                        if (PlayersWaypointList.Contains(currentPlayer.ServerId))
                        {
                            selectedPedRouteAlreadyActive = true;
                        }
                        foreach (var serverId in PlayersWaypointList)
                        {
                            // remove any coord blip
                            if (PlayerCoordWaypoints.TryGetValue(serverId, out var wp))
                            {
                                SetBlipRoute(wp, false);
                                RemoveBlip(ref wp);

                                PlayerCoordWaypoints.Remove(serverId);
                            }

                            // remove any entity blip
                            var playerId = GetPlayerFromServerId(serverId);

                            if (playerId < 0)
                            {
                                continue;
                            }

                            var playerPed = GetPlayerPed(playerId);
                            if (DoesEntityExist(playerPed) && DoesBlipExist(GetBlipFromEntity(playerPed)))
                            {
                                var oldBlip = GetBlipFromEntity(playerPed);
                                SetBlipRoute(oldBlip, false);
                                RemoveBlip(ref oldBlip);
                                Notify.Custom($"~g~到~s~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~的GPS路线已禁用。");
                            }
                        }
                        PlayersWaypointList.Clear();
                    }

                    if (!selectedPedRouteAlreadyActive)
                    {
                        if (currentPlayer.ServerId != Game.Player.ServerId)
                        {
                            int blip;

                            if (currentPlayer.IsActive && currentPlayer.Character != null)
                            {
                                var ped = GetPlayerPed(currentPlayer.Handle);
                                blip = GetBlipFromEntity(ped);
                                if (!DoesBlipExist(blip))
                                {
                                    blip = AddBlipForEntity(ped);
                                }
                            }
                            else
                            {
                                if (!PlayerCoordWaypoints.TryGetValue(currentPlayer.ServerId, out blip))
                                {
                                    var coords = await MainMenu.RequestPlayerCoordinates(currentPlayer.ServerId);
                                    blip = AddBlipForCoord(coords.X, coords.Y, coords.Z);
                                    PlayerCoordWaypoints[currentPlayer.ServerId] = blip;
                                }
                            }

                            SetBlipColour(blip, 58);
                            SetBlipRouteColour(blip, 58);
                            SetBlipRoute(blip, true);

                            PlayersWaypointList.Add(currentPlayer.ServerId);
                            Notify.Custom($"~g~到~s~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~的GPS路线已激活，再次按~s~切换GPS路线~g~按钮以禁用路线。");
                        }
                        else
                        {
                            Notify.Error("你不能给自己设置路点。");
                        }
                    }
                }
                else if (item == printIdentifiers)
                {
                    // TODO: Replace callback function
                    Func<string, string> CallbackFunction = (data) =>
                    {
                        Debug.WriteLine(data);
                        var ids = "~s~";
                        foreach (var s in JsonConvert.DeserializeObject<string[]>(data))
                        {
                            ids += "~n~" + s;
                        }
                        Notify.Custom($"~y~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~的标识符：{ids}", false);
                        return data;
                    };
                    BaseScript.TriggerServerEvent("vMenu:GetPlayerIdentifiers", currentPlayer.ServerId, CallbackFunction);
                }
                // kick button
                else if (item == kick)
                {
                    if (currentPlayer.Handle != Game.Player.Handle)
                    {
                        KickPlayer(currentPlayer, true);
                    }
                    else
                    {
                        Notify.Error("你不能踢出自己！");
                    }
                }
                // temp ban
                else if (item == tempban)
                {
                    BanPlayer(currentPlayer, false);
                }
                // perm ban
                else if (item == ban)
                {
                    if (ban.Label == "确定吗？")
                    {
                        ban.Label = "";
                        _ = UpdatePlayerlist();
                        playerMenu.GoBack();
                        BanPlayer(currentPlayer, true);
                    }
                    else
                    {
                        ban.Label = "确定吗？";
                    }
                }
            };

            // handle button presses in the player list.
            menu.OnItemSelect += (sender, item, index) =>
            {
                var baseId = int.Parse(item.Label.Replace(" →→→", "").Replace("服务器 #", ""));
                var player = MainMenu.PlayersList.FirstOrDefault(p => p.ServerId == baseId);

                if (player != null)
                {
                    currentPlayer = player;
                    playerMenu.MenuSubtitle = $"~s~玩家：~y~{GetSafePlayerName(currentPlayer.Name)}";
                    playerMenu.CounterPreText = $"[服务器ID：~y~{currentPlayer.ServerId}~s~] ";
                }
                else
                {
                    playerMenu.GoBack();
                }
            };
        }

        /// <summary>
        /// Updates the player items.
        /// </summary>
        public async Task UpdatePlayerlist()
        {
            void UpdateStuff()
            {
                menu.ClearMenuItems();

                foreach (var p in MainMenu.PlayersList.OrderBy(a => a.Name))
                {
                    var pItem = new MenuItem($"{GetSafePlayerName(p.Name)}", $"点击查看此玩家的选项。服务器ID：{p.ServerId}。本地ID：{p.Handle}。")
                    {
                        Label = $"服务器 #{p.ServerId} →→→"
                    };
                    menu.AddMenuItem(pItem);
                    MenuController.BindMenuItem(menu, playerMenu, pItem);
                }

                menu.RefreshIndex();
                //menu.UpdateScaleform();
                playerMenu.RefreshIndex();
                //playerMenu.UpdateScaleform();
            }

            // First, update *before* waiting - so we get all local players.
            UpdateStuff();
            await MainMenu.PlayersList.WaitRequested();

            // Update after waiting too so we have all remote players.
            UpdateStuff();
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Online Players Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
                return menu;
            }
            else
            {
                _ = UpdatePlayerlist();
                return menu;
            }
        }
    }
}