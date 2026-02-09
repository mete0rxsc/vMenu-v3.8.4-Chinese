using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class PlayerOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Public variables (getters only), return the private variables.
        public bool PlayerGodMode { get; private set; } = UserDefaults.PlayerGodMode;
        public bool PlayerInvisible { get; private set; } = false;
        public bool PlayerStamina { get; private set; } = UserDefaults.UnlimitedStamina;
        public bool PlayerFastRun { get; private set; } = UserDefaults.FastRun;
        public bool PlayerFastSwim { get; private set; } = UserDefaults.FastSwim;
        public bool PlayerSuperJump { get; private set; } = UserDefaults.SuperJump;
        public bool PlayerNoRagdoll { get; private set; } = UserDefaults.NoRagdoll;
        public bool PlayerNeverWanted { get; private set; } = UserDefaults.NeverWanted;
        public bool PlayerIsIgnored { get; private set; } = UserDefaults.EveryoneIgnorePlayer;
        public bool PlayerStayInVehicle { get; private set; } = UserDefaults.PlayerStayInVehicle;
        public bool PlayerFrozen { get; private set; } = false;

        public int PlayerBlood { get; private set; } = 0;

        private readonly Menu CustomDrivingStyleMenu = new("Driving Style", "Custom Driving Style");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create menu and menu items
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Player Options");

            // Create all checkboxes.
            var playerGodModeCheckbox = new MenuCheckboxItem("你是上帝无敌的知道吧", "无敌是多么~多么寂寞~", PlayerGodMode);
            var invisibleCheckbox = new MenuCheckboxItem("隐身药水", "隐身还用我说吗，禁止去偷窥小姑娘洗澡", PlayerInvisible);
            var unlimitedStaminaCheckbox = new MenuCheckboxItem("大力王", "允许您永远奔跑，而不会减速或受到伤害", PlayerStamina);
            var fastRunCheckbox = new MenuCheckboxItem("加速跑(我靠外挂)", "获得~g~蜗牛~s~的力量，跑得很快！", PlayerFastRun);
            SetRunSprintMultiplierForPlayer(Game.Player.Handle, PlayerFastRun && IsAllowed(Permission.POFastRun) ? 1.49f : 1f);
            var fastSwimCheckbox = new MenuCheckboxItem("快速游泳", "没准快淹死了用一下呢", PlayerFastSwim);
            SetSwimMultiplierForPlayer(Game.Player.Handle, PlayerFastSwim && IsAllowed(Permission.POFastSwim) ? 1.49f : 1f);
            var superJumpCheckbox = new MenuCheckboxItem("我一个飞天大跳！", "获得~g~蜗牛3.0~s~的力量，像冠军一样跳跃！", PlayerSuperJump);
            var noRagdollCheckbox = new MenuCheckboxItem("无布娃娃", "禁用玩家布娃娃，让你不再从自行车上摔下来", PlayerNoRagdoll);
            var neverWantedCheckbox = new MenuCheckboxItem("从来不要", "禁用所有想要的级别(这个我没看懂，机器翻译的)", PlayerNeverWanted);
            var everyoneIgnoresPlayerCheckbox = new MenuCheckboxItem("所有人都忽略玩家", "每个人都会让你一个人呆着", PlayerIsIgnored);
            var playerStayInVehicleCheckbox = new MenuCheckboxItem("待在车里", "启用此功能后，如果NPC对你生气，他们将无法将你拖出车外。", PlayerStayInVehicle);
            var playerFrozenCheckbox = new MenuCheckboxItem("冻结玩家", "冻结你在一个地方，不过你没事冻结你自己干嘛", PlayerFrozen);

            // Wanted level options
            var wantedLevelList = new List<string> { "无需级别", "1", "2", "3", "4", "5" };
            var setWantedLevel = new MenuListItem("Set Wanted Level", wantedLevelList, GetPlayerWantedLevel(Game.Player.Handle), "Set your wanted level by selecting a value, and pressing enter.");
            var setArmorItem = new MenuListItem("Set Armor Type", new List<string> { "No Armor", GetLabelText("WT_BA_0"), GetLabelText("WT_BA_1"), GetLabelText("WT_BA_2"), GetLabelText("WT_BA_3"), GetLabelText("WT_BA_4"), }, 0, "Set the armor level/type for your player.");

            // Blood level options
            var clearBloodBtn = new MenuItem("清理血迹", "清理小哑巴身上的血迹");
            var bloodList = new List<string> { "BigHitByVehicle", "SCR_Torture", "SCR_TrevorTreeBang", "HOSPITAL_0", "HOSPITAL_1", "HOSPITAL_2", "HOSPITAL_3", "HOSPITAL_4", "HOSPITAL_5", "HOSPITAL_6", "HOSPITAL_7", "HOSPITAL_8", "HOSPITAL_9", "Explosion_Med", "Skin_Melee_0", "Explosion_Large", "Car_Crash_Light", "Car_Crash_Heavy", "Fall_Low", "Fall", "HitByVehicle", "BigRunOverByVehicle", "RunOverByVehicle", "TD_KNIFE_FRONT", "TD_KNIFE_FRONT_VA", "TD_KNIFE_FRONT_VB", "TD_KNIFE_REAR", "TD_KNIFE_REAR_VA", "TD_KNIFE_REAR_VB", "TD_KNIFE_STEALTH", "TD_MELEE_FRONT", "TD_MELEE_REAR", "TD_MELEE_STEALTH", "TD_MELEE_BATWAIST", "TD_melee_face_l", "MTD_melee_face_r", "MTD_melee_face_jaw", "TD_PISTOL_FRONT", "TD_PISTOL_FRONT_KILL", "TD_PISTOL_REAR", "TD_PISTOL_REAR_KILL", "TD_RIFLE_FRONT_KILL", "TD_RIFLE_NONLETHAL_FRONT", "TD_RIFLE_NONLETHAL_REAR", "TD_SHOTGUN_FRONT_KILL", "TD_SHOTGUN_REAR_KILL" };
            var setBloodLevel = new MenuListItem("设置你身上的血迹大小", bloodList, PlayerBlood, "Sets your players blood level.");

            var healPlayerBtn = new MenuItem("治疗你只鸡", "给你自己最大生命值");
            var cleanPlayerBtn = new MenuItem("清洁球员衣服", "清洁你的球员衣服");
            var dryPlayerBtn = new MenuItem("干爽小哑巴的衣服", "让你的小哑巴的衣服变干");
            var wetPlayerBtn = new MenuItem("浸湿小哑巴的衣服", "让你的小哑巴的衣服湿透(干啥了我不说)");
            var suicidePlayerBtn = new MenuItem("~r~自杀", "吃这药自杀。或者用手枪，如果你有的话");

            var vehicleAutoPilot = new Menu("特斯拉FSD", "FSD V14");

            MenuController.AddSubmenu(menu, vehicleAutoPilot);

            var vehicleAutoPilotBtn = new MenuItem("FSD菜单", "管理车辆自动驾驶选项")
            {
                Label = "→→→"
            };

            var drivingStyles = new List<string>() { "Normal", "Rushed", "Avoid highways", "Drive in reverse", "Custom" };
            var drivingStyle = new MenuListItem("驾驶模式", drivingStyles, 0, "设置用于“行驶到航点”和“随机行驶”功能的驾驶风格");

            // Scenarios (list can be found in the PedScenarios class)
            var playerScenarios = new MenuListItem("Player Scenarios", PedScenarios.Scenarios, 0, "选择一个场景并按enter键启动它。选择另一个场景将覆盖当前场景。如果您已经在播放所选场景，再次选择它将停止该场景");
            var stopScenario = new MenuItem("强制停止场景", "这将强制播放场景立即停止，而无需等待它完成“停止”动画");
            #endregion

            #region add items to menu based on permissions
            // Add all checkboxes to the menu. (keeping permissions in mind)
            if (IsAllowed(Permission.POGod))
            {
                menu.AddMenuItem(playerGodModeCheckbox);
            }
            if (IsAllowed(Permission.POInvisible))
            {
                menu.AddMenuItem(invisibleCheckbox);
            }
            if (IsAllowed(Permission.POUnlimitedStamina))
            {
                menu.AddMenuItem(unlimitedStaminaCheckbox);
            }
            if (IsAllowed(Permission.POFastRun))
            {
                menu.AddMenuItem(fastRunCheckbox);
            }
            if (IsAllowed(Permission.POFastSwim))
            {
                menu.AddMenuItem(fastSwimCheckbox);
            }
            if (IsAllowed(Permission.POSuperjump))
            {
                menu.AddMenuItem(superJumpCheckbox);
            }
            if (IsAllowed(Permission.PONoRagdoll))
            {
                menu.AddMenuItem(noRagdollCheckbox);
            }
            if (IsAllowed(Permission.PONeverWanted))
            {
                menu.AddMenuItem(neverWantedCheckbox);
            }
            if (IsAllowed(Permission.POSetWanted))
            {
                menu.AddMenuItem(setWantedLevel);
            }
            if (IsAllowed(Permission.POClearBlood))
            {
                menu.AddMenuItem(clearBloodBtn);
            }
            if (IsAllowed(Permission.POSetBlood))
            {
                menu.AddMenuItem(setBloodLevel);
            }
            if (IsAllowed(Permission.POIgnored))
            {
                menu.AddMenuItem(everyoneIgnoresPlayerCheckbox);
            }
            if (IsAllowed(Permission.POStayInVehicle))
            {
                menu.AddMenuItem(playerStayInVehicleCheckbox);
            }
            if (IsAllowed(Permission.POMaxHealth))
            {
                menu.AddMenuItem(healPlayerBtn);
            }
            if (IsAllowed(Permission.POMaxArmor))
            {
                menu.AddMenuItem(setArmorItem);
            }
            if (IsAllowed(Permission.POCleanPlayer))
            {
                menu.AddMenuItem(cleanPlayerBtn);
            }
            if (IsAllowed(Permission.PODryPlayer))
            {
                menu.AddMenuItem(dryPlayerBtn);
            }
            if (IsAllowed(Permission.POWetPlayer))
            {
                menu.AddMenuItem(wetPlayerBtn);
            }

            menu.AddMenuItem(suicidePlayerBtn);

            if (IsAllowed(Permission.POVehicleAutoPilotMenu))
            {
                menu.AddMenuItem(vehicleAutoPilotBtn);
                MenuController.BindMenuItem(menu, vehicleAutoPilot, vehicleAutoPilotBtn);

                vehicleAutoPilot.AddMenuItem(drivingStyle);

                var startDrivingWaypoint = new MenuItem("开车到导航点", "让你的玩家把你的车开到你的路点");
                var startDrivingRandomly = new MenuItem("随意开车", "让你的玩家在地图上随机驾驶你的车辆");
                var stopDriving = new MenuItem("停止驾驶", "玩家会找到一个合适的地方停车。一旦车辆到达合适的停止位置，任务将停止");
                var forceStopDriving = new MenuItem("强制停止驾驶", "这将立即停止驾驶任务，而不会找到合适的停车位置");
                var customDrivingStyle = new MenuItem("自定义驾驶风格", "选择自定义驾驶风格。确保通过在驾驶风格列表中选择“自定义”驾驶风格来启用它") { Label = "→→→" };
                MenuController.AddSubmenu(vehicleAutoPilot, CustomDrivingStyleMenu);
                vehicleAutoPilot.AddMenuItem(customDrivingStyle);
                MenuController.BindMenuItem(vehicleAutoPilot, CustomDrivingStyleMenu, customDrivingStyle);
                var knownNames = new Dictionary<int, string>()
                {
                    { 0, "停车等候车辆" },  // The driver will stop to avoid hitting vehicles.
                    { 1, "行人停车" },  // The driver will stop to avoid hitting pedestrians.
                    { 2, "绕过所有车辆" },  // The driver will swerve to avoid moving vehicles.
                    { 3, "绕过静止的车辆" },  // The driver will steer around parked or stationary vehicles.
                    { 4, "绕过行人" },  // The driver will steer to avoid pedestrians.
                    { 5, "绕过物体" },  // The driver will steer to avoid objects on the road.
                    { 6, "不要绕过玩家行人" },  // The driver will not avoid the player's character on foot.
                    { 7, "在红绿灯处停车" },  // The driver will obey traffic signals.
                    { 8, "避开时越野行驶" },  // The driver may go off-road to avoid obstacles.
                    { 9, "允许走错路" },  // The driver is allowed to drive against traffic.
                    { 10, "挂倒档" },  // The driver can reverse the vehicle.
                    { 11, "使用漫游回退而不是直线" },  // The driver uses wandering paths if straight paths fail.
                    { 12, "避开限制区域" },  // The driver avoids areas marked as restricted.
                    { 13, "防止后台寻路" },  // The driver will not perform background pathfinding.
                    { 14, "根据行驶速度调整巡航速度" },  // The driver adjusts speed to match road conditions.
                    { 18, "使用快捷链接（使用最短路径）" },  // The driver uses shortcuts to reach the destination faster.
                    { 19, "改变障碍物周围的车道" },  // The driver changes lanes to avoid obstructions.
                    { 21, "使用已关闭的节点" },  // The driver can use nodes that are typically disabled.
                    { 22, "更喜欢导航网路线" },  // The driver prefers routes defined in the navigation mesh.
                    { 23, "飞机滑行模式" },  // The driver operates as if taxiing an aircraft.
                    { 24, "司机试图直线行驶" },  // The driver attempts to drive in a straight line.
                    { 25, "司机使用拉线在交叉路口实现更平稳的转弯" },  // The driver uses string pulling for smoother turns at junctions.
                    { 29, "司机避免使用高速公路" },  // The driver avoids using highways.
                    { 30, "强行加入道路方向" },  // The driver joins roads in the correct direction.
                };
                for (var i = 0; i < 31; i++)
                {
                    var name = "~r~未知标志";
                    if (knownNames.ContainsKey(i))
                    {
                        name = knownNames[i];
                    }
                    var checkbox = new MenuCheckboxItem(name, "切换此驾驶风格标志", false);
                    CustomDrivingStyleMenu.AddMenuItem(checkbox);
                }
                CustomDrivingStyleMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                    CustomDrivingStyleMenu.MenuSubtitle = $"自定义风格: {style}";
                    if (drivingStyle.ListIndex == 4)
                    {
                        Notify.Custom("驾驶风格更新");
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                    }
                    else
                    {
                        Notify.Custom("驾驶风格未更新，因为您尚未在上一菜单中启用自定义驾驶风格");
                    }
                };

                vehicleAutoPilot.AddMenuItem(startDrivingWaypoint);
                vehicleAutoPilot.AddMenuItem(startDrivingRandomly);
                vehicleAutoPilot.AddMenuItem(stopDriving);
                vehicleAutoPilot.AddMenuItem(forceStopDriving);

                vehicleAutoPilot.RefreshIndex();

                vehicleAutoPilot.OnItemSelect += async (sender, item, index) =>
                {
                    if (Game.PlayerPed.IsInVehicle() && item != stopDriving && item != forceStopDriving)
                    {
                        if (Game.PlayerPed.CurrentVehicle != null && Game.PlayerPed.CurrentVehicle.Exists() && !Game.PlayerPed.CurrentVehicle.IsDead && Game.PlayerPed.CurrentVehicle.IsDriveable)
                        {
                            if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                            {
                                if (item == startDrivingWaypoint)
                                {
                                    if (IsWaypointActive())
                                    {
                                        var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                        DriveToWp(style);
                                        Notify.Info("您的玩家ped现在正在为您驾驶车辆。您可以随时按停止驾驶按钮取消。车辆到达目的地后将停止");
                                    }
                                    else
                                    {
                                        Notify.Error("你需要一个路点，然后才能开车去！");
                                    }

                                }
                                else if (item == startDrivingRandomly)
                                {
                                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                    DriveWander(style);
                                    Notify.Info("您的玩家ped现在正在为您驾驶车辆。您可以随时按停止驾驶按钮取消");
                                }
                            }
                            else
                            {
                                Notify.Error("你一定是这辆车的司机!");
                            }
                        }
                        else
                        {
                            Notify.Error("您的车辆损坏或不存在！");
                        }
                    }
                    else if (item != stopDriving && item != forceStopDriving)
                    {
                        Notify.Error("你需要先上车！");
                    }
                    if (item == stopDriving)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            var veh = GetVehicle();
                            if (veh != null && veh.Exists() && !veh.IsDead)
                            {
                                var outPos = new Vector3();
                                if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                                {
                                    Notify.Info("玩家ped将找到一个合适的地方停车，然后停止驾驶。请稍候");
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    TaskVehiclePark(Game.PlayerPed.Handle, veh.Handle, outPos.X, outPos.Y, outPos.Z, Game.PlayerPed.Heading, 3, 60f, true);
                                    while (Game.PlayerPed.Position.DistanceToSquared2D(outPos) > 3f)
                                    {
                                        await BaseScript.Delay(0);
                                    }
                                    SetVehicleHalt(veh.Handle, 3f, 0, false);
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    Notify.Info("那个运动员已经停止开车了");
                                }
                            }
                        }
                        else
                        {
                            ClearPedTasks(Game.PlayerPed.Handle);
                            Notify.Alert("你的自行车不在任何车里");
                        }
                    }
                    else if (item == forceStopDriving)
                    {
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Notify.Info("驾驶任务已取消");
                    }
                };

                vehicleAutoPilot.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
                {
                    if (item == drivingStyle)
                    {
                        var style = GetStyleFromIndex(listIndex);
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                        Notify.Info($"驾驶任务样式现在设置: ~r~{drivingStyles[listIndex]}~s~.");
                    }
                };
            }

            if (IsAllowed(Permission.POFreeze))
            {
                menu.AddMenuItem(playerFrozenCheckbox);
            }
            if (IsAllowed(Permission.POScenarios))
            {
                menu.AddMenuItem(playerScenarios);
                menu.AddMenuItem(stopScenario);
            }
            #endregion

            #region handle all events
            // Checkbox changes.
            menu.OnCheckboxChange += (sender, item, itemIndex, _checked) =>
            {
                // God Mode toggled.
                if (item == playerGodModeCheckbox)
                {
                    PlayerGodMode = _checked;
                }
                // Invisibility toggled.
                else if (item == invisibleCheckbox)
                {
                    PlayerInvisible = _checked;
                    SetEntityVisible(Game.PlayerPed.Handle, !PlayerInvisible, false);
                }
                // Unlimited Stamina toggled.
                else if (item == unlimitedStaminaCheckbox)
                {
                    PlayerStamina = _checked;
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), _checked ? 100 : 0, true);
                }
                // Fast run toggled.
                else if (item == fastRunCheckbox)
                {
                    PlayerFastRun = _checked;
                    SetRunSprintMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Fast swim toggled.
                else if (item == fastSwimCheckbox)
                {
                    PlayerFastSwim = _checked;
                    SetSwimMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Super jump toggled.
                else if (item == superJumpCheckbox)
                {
                    PlayerSuperJump = _checked;
                }
                // No ragdoll toggled.
                else if (item == noRagdollCheckbox)
                {
                    PlayerNoRagdoll = _checked;
                }
                // Never wanted toggled.
                else if (item == neverWantedCheckbox)
                {
                    PlayerNeverWanted = _checked;
                    if (!_checked)
                    {
                        SetMaxWantedLevel(5);
                    }
                    else
                    {
                        SetMaxWantedLevel(0);
                    }
                }
                // Everyone ignores player toggled.
                else if (item == everyoneIgnoresPlayerCheckbox)
                {
                    PlayerIsIgnored = _checked;

                    // Manage player is ignored by everyone.
                    SetEveryoneIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPoliceIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPlayerCanBeHassledByGangs(Game.Player.Handle, !PlayerIsIgnored);
                }
                else if (item == playerStayInVehicleCheckbox)
                {
                    PlayerStayInVehicle = _checked;
                }
                // Freeze player toggled.
                else if (item == playerFrozenCheckbox)
                {
                    PlayerFrozen = _checked;

                    if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                    else if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                }
            };

            // List selections
            menu.OnListItemSelect += (sender, listItem, listIndex, itemIndex) =>
            {
                // Set wanted Level
                if (listItem == setWantedLevel)
                {
                    SetPlayerWantedLevel(Game.Player.Handle, listIndex, false);
                    SetPlayerWantedLevelNow(Game.Player.Handle, false);
                }
                // Set blood level
                else if (listItem == setBloodLevel)
                {
                    ApplyPedDamagePack(Game.PlayerPed.Handle, bloodList[listIndex], 100, 100);
                }
                // Player Scenarios 
                else if (listItem == playerScenarios)
                {
                    PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[listIndex]]);
                }
                else if (listItem == setArmorItem)
                {
                    Game.PlayerPed.Armor = listItem.ListIndex * 20;
                }
            };

            // button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Force Stop Scenario button
                if (item == stopScenario)
                {
                    // Play a new scenario named "forcestop" (this scenario doesn't exist, but the "Play" function checks
                    // for the string "forcestop", if that's provided as th scenario name then it will forcefully clear the player task.
                    PlayScenario("forcestop");
                }
                else if (item == clearBloodBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Game.PlayerPed.ResetVisibleDamage();
                    // not ideal for removing visible bruises & scars, may have some sync issues but could not find an alternative method, anyone who does feel free to update

                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 0, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 1, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 2, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 3, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 4, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 5, "ALL");
                }
                else if (item == healPlayerBtn)
                {
                    Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;
                    Notify.Success("Player healed.");
                }
                else if (item == cleanPlayerBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Notify.Success("小哑巴的衣服已经洗干净了");
                }
                else if (item == dryPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 0f;
                    Notify.Success("小哑巴被你烘干了");
                }
                else if (item == wetPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 2f;
                    Notify.Success("小哑巴被你搞得好湿啊");
                }
                else if (item == suicidePlayerBtn)
                {
                    CommitSuicide();
                }
            };
            #endregion

        }

        private int GetCustomDrivingStyle()
        {
            var items = CustomDrivingStyleMenu.GetMenuItems();
            var flags = new int[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is MenuCheckboxItem checkbox)
                {
                    flags[i] = checkbox.Checked ? 1 : 0;
                }
            }
            var binaryString = "";
            var reverseFlags = flags.Reverse();
            foreach (var i in reverseFlags)
            {
                binaryString += i;
            }
            var binaryNumber = Convert.ToUInt32(binaryString, 2);
            return (int)binaryNumber;
        }

        private int GetStyleFromIndex(int index)
        {
            var style = index switch
            {
                0 => 443,// normal
                1 => 575,// rushed
                2 => 536871355,// Avoid highways
                3 => 1467,// Go in reverse
                4 => GetCustomDrivingStyle(),// custom driving style;
                _ => 0,// no style (impossible, but oh well)
            };
            return style;
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Player Options Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }

    }
}
