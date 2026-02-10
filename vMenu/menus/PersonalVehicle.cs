using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class PersonalVehicle
    {
        // Variables
        private Menu menu;
        public bool EnableVehicleBlip { get; private set; } = UserDefaults.PVEnableVehicleBlip;

        // Empty constructor
        public PersonalVehicle() { }

        public Vehicle CurrentPersonalVehicle { get; internal set; } = null;

        public Menu VehicleDoorsMenu { get; internal set; } = null;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Menu
            menu = new Menu(GetSafePlayerName(Game.Player.Name), "个人车辆选项");

            // menu items
            var setVehice = new MenuItem("设置车辆", "将当前车辆设置为个人车辆。如果您已经设置了个人车辆，那么这将覆盖您的选择.") { Label = "当前车辆：无" };
            var toggleEngine = new MenuItem("切换发动机", "即使您不在车内，也可以打开或关闭发动机。如果其他人当前正在使用您的车辆，则此功能不起作用。");
            var toggleLights = new MenuListItem("设置车辆灯", new List<string>() { "Force On", "Force Off", "Reset" }, 0, "这将启用或禁用您的车辆前灯，您的车辆发动机需要运行才能正常工作.");
            var toggleStance = new MenuListItem("车辆姿态", new List<string>() { "Default", "Lowered" }, 0, "为您的个人车辆选择姿态.");
            var kickAllPassengers = new MenuItem("踢乘客", "这将使所有乘客离开您的私人车辆。");
            //MenuItem
            var lockDoors = new MenuItem("锁车门", "这将为所有玩家锁定所有车门。任何已经在车内的人都可以离开车辆，即使车门是锁着的。");
            var unlockDoors = new MenuItem("解锁车门", "这将为所有玩家解锁您的所有车门.");
            var doorsMenuBtn = new MenuItem("车门", "在此处打开、关闭、拆卸和恢复车门。")
            {
                Label = "→→→"
            };
            var soundHorn = new MenuItem("音响喇叭", "车辆的喇叭响了");
            var toggleAlarm = new MenuItem("开启关闭报警声音", "打开或关闭车辆报警声音。这不会设置报警。它仅切换报警的当前声音状态。");
            var enableBlip = new MenuCheckboxItem("为个人车辆添加Blip", "启用或禁用将车辆标记为个人车辆时添加的光点.", EnableVehicleBlip) { Style = MenuCheckboxItem.CheckboxStyle.Cross };
            var exclusiveDriver = new MenuCheckboxItem("专属司机", "如果启用，则您将是唯一可以进入驾驶员座椅的人。其他玩家将无法驾驶这辆车。他们仍然可以是乘客。", false) { Style = MenuCheckboxItem.CheckboxStyle.Cross };
            //submenu
            VehicleDoorsMenu = new Menu("车门管理", "车门管理(车门焊死走喽~)");
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);

            // This is always allowed if this submenu is created/allowed.
            menu.AddMenuItem(setVehice);

            // Add conditional features.

            // Toggle engine.
            if (IsAllowed(Permission.PVToggleEngine))
            {
                menu.AddMenuItem(toggleEngine);
            }

            // Toggle lights
            if (IsAllowed(Permission.PVToggleLights))
            {
                menu.AddMenuItem(toggleLights);
            }

            // Toggle stance
            if (IsAllowed(Permission.PVToggleStance))
            {
                menu.AddMenuItem(toggleStance);
            }

            // Kick vehicle passengers
            if (IsAllowed(Permission.PVKickPassengers))
            {
                menu.AddMenuItem(kickAllPassengers);
            }

            // Lock and unlock vehicle doors
            if (IsAllowed(Permission.PVLockDoors))
            {
                menu.AddMenuItem(lockDoors);
                menu.AddMenuItem(unlockDoors);
            }

            if (IsAllowed(Permission.PVDoors))
            {
                menu.AddMenuItem(doorsMenuBtn);
            }

            // Sound horn
            if (IsAllowed(Permission.PVSoundHorn))
            {
                menu.AddMenuItem(soundHorn);
            }

            // Toggle alarm sound
            if (IsAllowed(Permission.PVToggleAlarm))
            {
                menu.AddMenuItem(toggleAlarm);
            }

            // Enable blip for personal vehicle
            if (IsAllowed(Permission.PVAddBlip))
            {
                menu.AddMenuItem(enableBlip);
            }

            if (IsAllowed(Permission.PVExclusiveDriver))
            {
                menu.AddMenuItem(exclusiveDriver);
            }


            // Handle list presses
            menu.OnListItemSelect += (sender, item, itemIndex, index) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("您目前无法控制此车辆。现在有人开你的车吗？请确保其他玩家没有控制您的车辆后重试.");
                            return;
                        }
                    }

                    if (item == toggleLights)
                    {
                        PressKeyFob(CurrentPersonalVehicle);
                        if (itemIndex == 0)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 3);
                        }
                        else if (itemIndex == 1)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 1);
                        }
                        else
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 0);
                        }
                    }
                    else if (item == toggleStance)
                    {
                        PressKeyFob(CurrentPersonalVehicle);
                        if (itemIndex == 0)
                        {
                            SetReduceDriftVehicleSuspension(CurrentPersonalVehicle.Handle, false);
                        }
                        else if (itemIndex == 1)
                        {
                            SetReduceDriftVehicleSuspension(CurrentPersonalVehicle.Handle, true);
                        }
                    }

                }
                else
                {
                    Notify.Error("您尚未选择个人车辆，或者您的车辆已被删除。在使用这些选项之前，请先设置一辆私人车辆.");
                }
            };

            // Handle checkbox changes
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == enableBlip)
                {
                    EnableVehicleBlip = _checked;
                    if (EnableVehicleBlip)
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                        {
                            if (CurrentPersonalVehicle.AttachedBlip == null || !CurrentPersonalVehicle.AttachedBlip.Exists())
                            {
                                CurrentPersonalVehicle.AttachBlip();
                            }
                            CurrentPersonalVehicle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                            CurrentPersonalVehicle.AttachedBlip.Name = "个人车辆";
                        }
                        else
                        {
                            Notify.Error("您尚未选择个人车辆，或者您的车辆已被删除。在使用这些选项之前，请先设置一辆私人车辆.");
                        }

                    }
                    else
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists() && CurrentPersonalVehicle.AttachedBlip != null && CurrentPersonalVehicle.AttachedBlip.Exists())
                        {
                            CurrentPersonalVehicle.AttachedBlip.Delete();
                        }
                    }
                }
                else if (item == exclusiveDriver)
                {
                    if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                    {
                        if (NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (_checked)
                            {
                                // SetVehicleExclusiveDriver, but the current version is broken in C# so we manually execute it.
                                CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0x41062318F23ED854, CurrentPersonalVehicle, true);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, Game.PlayerPed.Handle, 1);
                            }
                            else
                            {
                                // SetVehicleExclusiveDriver, but the current version is broken in C# so we manually execute it.
                                CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0x41062318F23ED854, CurrentPersonalVehicle, false);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, 0, 1);
                            }
                        }
                        else
                        {
                            item.Checked = !_checked;
                            Notify.Error("您尚未选择个人车辆，或者您的车辆已被删除。在使用这些选项之前，请先设置一辆私人车辆.");
                        }
                    }
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == setVehice)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh != null && veh.Exists())
                        {
                            if (Game.PlayerPed == veh.Driver)
                            {
                                CurrentPersonalVehicle = veh;
                                veh.PreviouslyOwnedByPlayer = true;
                                veh.IsPersistent = true;
                                if (EnableVehicleBlip && IsAllowed(Permission.PVAddBlip))
                                {
                                    if (veh.AttachedBlip == null || !veh.AttachedBlip.Exists())
                                    {
                                        veh.AttachBlip();
                                    }
                                    veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    veh.AttachedBlip.Name = "Personal Vehicle";
                                }
                                var name = GetLabelText(veh.DisplayName);
                                if (string.IsNullOrEmpty(name) || name.ToLower() == "null")
                                {
                                    name = veh.DisplayName;
                                }
                                item.Label = $"Current Vehicle: {name}";
                            }
                            else
                            {
                                Notify.Error(CommonErrors.NeedToBeTheDriver);
                            }
                        }
                        else
                        {
                            Notify.Error(CommonErrors.NoVehicle);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                else if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                {
                    if (item == kickAllPassengers)
                    {
                        Ped[] occupants = CurrentPersonalVehicle.Occupants;

                        if (occupants.Count() > 0 && occupants.Any(p => p != Game.PlayerPed && p.IsPlayer))
                        {
                            TriggerServerEvent("vMenu:GetOutOfCar", CurrentPersonalVehicle.NetworkId);
                        }
                        else
                        {
                            Notify.Info("你的车里没有其他玩家需要被踢出去");
                        }
                    }
                    else
                    {
                        if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                            {
                                Notify.Error("您目前无法控制此车辆。现在有人开你的车吗？请确保其他玩家没有控制您的车辆后重试");
                                return;
                            }
                        }

                        if (item == toggleEngine)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            SetVehicleEngineOn(CurrentPersonalVehicle.Handle, !CurrentPersonalVehicle.IsEngineRunning, true, true);
                        }

                        else if (item == lockDoors || item == unlockDoors)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            var _lock = item == lockDoors;
                            LockOrUnlockDoors(CurrentPersonalVehicle, _lock);
                        }

                        else if (item == soundHorn)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            SoundHorn(CurrentPersonalVehicle);
                        }

                        else if (item == toggleAlarm)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            ToggleVehicleAlarm(CurrentPersonalVehicle);
                        }
                    }
                }
                else
                {
                    Notify.Error("您尚未选择个人车辆，或者您的车辆已被删除。在使用这些选项之前，请先设置一辆私人车辆.");
                }
            };

            #region Doors submenu 
            var openAll = new MenuItem("打开所有门", "Open all vehicle doors.");
            var closeAll = new MenuItem("关闭所有车门", "Close all vehicle doors.");
            var LF = new MenuItem("左前门", "Open/close the left front door.");
            var RF = new MenuItem("右前门", "Open/close the right front door.");
            var LR = new MenuItem("左后门", "Open/close the left rear door.");
            var RR = new MenuItem("右后门", "Open/close the right rear door.");
            var HD = new MenuItem("机器盖子", "Open/close the hood.");
            var TR = new MenuItem("后备箱", "Open/close the trunk.");
            var E1 = new MenuItem("附加 1", "Open/close the extra door (#1). Note this door is not present on most vehicles.");
            var E2 = new MenuItem("附加 2", "Open/close the extra door (#2). Note this door is not present on most vehicles.");
            var BB = new MenuItem("炸弹舱", "Open/close the bomb bay. 只在飞机上可用.");
            var doors = new List<string>() { "Front Left", "Front Right", "Rear Left", "Rear Right", "Hood", "Trunk", "Extra 1", "Extra 2", "Bomb Bay" };
            var removeDoorList = new MenuListItem("没门~", doors, 0, "移除车门.");
            var deleteDoors = new MenuCheckboxItem("删除已拆下的门", "启用后，使用上面的列表删除的门将从世界中删除。如果被禁用，那么门就会掉到地上.", false);

            VehicleDoorsMenu.AddMenuItem(LF);
            VehicleDoorsMenu.AddMenuItem(RF);
            VehicleDoorsMenu.AddMenuItem(LR);
            VehicleDoorsMenu.AddMenuItem(RR);
            VehicleDoorsMenu.AddMenuItem(HD);
            VehicleDoorsMenu.AddMenuItem(TR);
            VehicleDoorsMenu.AddMenuItem(E1);
            VehicleDoorsMenu.AddMenuItem(E2);
            VehicleDoorsMenu.AddMenuItem(BB);
            VehicleDoorsMenu.AddMenuItem(openAll);
            VehicleDoorsMenu.AddMenuItem(closeAll);
            VehicleDoorsMenu.AddMenuItem(removeDoorList);
            VehicleDoorsMenu.AddMenuItem(deleteDoors);

            VehicleDoorsMenu.OnListItemSelect += (sender, item, index, itemIndex) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("您目前无法控制此车辆。现在有人开你的车吗？请确保其他玩家没有控制您的车辆后重试");
                            return;
                        }
                    }

                    if (item == removeDoorList)
                    {
                        PressKeyFob(veh);
                        SetVehicleDoorBroken(veh.Handle, index, deleteDoors.Checked);
                    }
                }
            };

            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("您目前无法控制此车辆。现在有人开你的车吗？请确保其他玩家没有控制您的车辆后重试.");
                            return;
                        }
                    }

                    if (index < 8)
                    {
                        var open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f;
                        PressKeyFob(veh);
                        if (open)
                        {
                            SetVehicleDoorShut(veh.Handle, index, false);
                        }
                        else
                        {
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    }
                    else if (item == openAll)
                    {
                        PressKeyFob(veh);
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                    }
                    else if (item == closeAll)
                    {
                        PressKeyFob(veh);
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorShut(veh.Handle, door, false);
                        }
                    }
                    else if (item == BB && veh.HasBombBay)
                    {
                        PressKeyFob(veh);
                        var bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                        if (bombBayOpen)
                        {
                            veh.CloseBombBay();
                        }
                        else
                        {
                            veh.OpenBombBay();
                        }
                    }
                    else
                    {
                        Notify.Error("您目前无法控制此车辆。现在有人开你的车吗？请确保其他玩家没有控制您的车辆后重试.");
                    }
                }
            };
            #endregion
        }



        private async void SoundHorn(Vehicle veh)
        {
            if (veh != null && veh.Exists())
            {
                var timer = GetGameTimer();
                while (GetGameTimer() - timer < 1000)
                {
                    SoundVehicleHornThisFrame(veh.Handle);
                    await Delay(0);
                }
            }
        }

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
