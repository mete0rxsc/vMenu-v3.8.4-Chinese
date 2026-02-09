using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using vMenuShared;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class WeatherOptions
    {
        // Variables
        private Menu menu;
        public MenuCheckboxItem dynamicWeatherEnabled;
        public MenuCheckboxItem blackout;
        public MenuCheckboxItem vehicleBlackout;
        public MenuCheckboxItem snowEnabled;
        public static readonly List<string> weatherTypes = new()
        {
            "EXTRASUNNY",
            "CLEAR",
            "NEUTRAL",
            "SMOG",
            "FOGGY",
            "CLOUDS",
            "OVERCAST",
            "CLEARING",
            "RAIN",
            "THUNDER",
            "BLIZZARD",
            "SNOW",
            "SNOWLIGHT",
            "XMAS",
            "HALLOWEEN"
        };

        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "天气选项");

            dynamicWeatherEnabled = new MenuCheckboxItem("切换动态天气", "启用或禁用动态天气变化。", EventManager.DynamicWeatherEnabled);
            blackout = new MenuCheckboxItem("切换停电", "禁用或启用整个地图上的所有灯光。", EventManager.IsBlackoutEnabled);
            vehicleBlackout = new MenuCheckboxItem("切换车辆灯光停电", "禁用或启用整个地图上的所有车辆灯光。", !EventManager.IsVehicleLightsEnabled);
            snowEnabled = new MenuCheckboxItem("启用雪效果", "这将强制雪出现在地面上，并为角色和车辆启用雪粒子效果。与X-MAS或轻雪天气结合使用效果最佳。", ConfigManager.GetSettingsBool(ConfigManager.Setting.vmenu_enable_snow));

            var extrasunny = new MenuItem("特别晴朗", "设置天气为~y~特别晴朗~s~！") { ItemData = "EXTRASUNNY" };
            var clear = new MenuItem("晴朗", "设置天气为~y~晴朗~s~！") { ItemData = "CLEAR" };
            var neutral = new MenuItem("中性", "设置天气为~y~中性~s~！") { ItemData = "NEUTRAL" };
            var smog = new MenuItem("烟雾", "设置天气为~y~烟雾~s~！") { ItemData = "SMOG" };
            var foggy = new MenuItem("雾气", "设置天气为~y~雾气~s~！") { ItemData = "FOGGY" };
            var clouds = new MenuItem("多云", "设置天气为~y~多云~s~！") { ItemData = "CLOUDS" };
            var overcast = new MenuItem("阴天", "设置天气为~y~阴天~s~！") { ItemData = "OVERCAST" };
            var clearing = new MenuItem("转晴", "设置天气为~y~转晴~s~！") { ItemData = "CLEARING" };
            var rain = new MenuItem("下雨", "设置天气为~y~下雨~s~！") { ItemData = "RAIN" };
            var thunder = new MenuItem("雷雨", "设置天气为~y~雷雨~s~！") { ItemData = "THUNDER" };
            var blizzard = new MenuItem("暴风雪", "设置天气为~y~暴风雪~s~！") { ItemData = "BLIZZARD" };
            var snow = new MenuItem("下雪", "设置天气为~y~下雪~s~！") { ItemData = "SNOW" };
            var snowlight = new MenuItem("轻雪", "设置天气为~y~轻雪~s~！") { ItemData = "SNOWLIGHT" };
            var xmas = new MenuItem("圣诞雪", "设置天气为~y~圣诞雪~s~！") { ItemData = "XMAS" };
            var halloween = new MenuItem("万圣节", "设置天气为~y~万圣节~s~！") { ItemData = "HALLOWEEN" };
            var removeclouds = new MenuItem("移除所有云朵", "从天空中移除所有云朵！");
            var randomizeclouds = new MenuItem("随机云朵", "向天空添加随机云朵！");

            if (IsAllowed(Permission.WODynamic))
            {
                menu.AddMenuItem(dynamicWeatherEnabled);
            }
            if (IsAllowed(Permission.WOBlackout))
            {
                menu.AddMenuItem(blackout);
            }
            if (IsAllowed(Permission.WOVehBlackout))
            {
                menu.AddMenuItem(vehicleBlackout);
            }
            if (IsAllowed(Permission.WOSetWeather))
            {
                menu.AddMenuItem(snowEnabled);
                menu.AddMenuItem(extrasunny);
                menu.AddMenuItem(clear);
                menu.AddMenuItem(neutral);
                menu.AddMenuItem(smog);
                menu.AddMenuItem(foggy);
                menu.AddMenuItem(clouds);
                menu.AddMenuItem(overcast);
                menu.AddMenuItem(clearing);
                menu.AddMenuItem(rain);
                menu.AddMenuItem(thunder);
                menu.AddMenuItem(blizzard);
                menu.AddMenuItem(snow);
                menu.AddMenuItem(snowlight);
                menu.AddMenuItem(xmas);
                menu.AddMenuItem(halloween);
            }
            if (IsAllowed(Permission.WORandomizeClouds))
            {
                menu.AddMenuItem(randomizeclouds);
            }

            if (IsAllowed(Permission.WORemoveClouds))
            {
                menu.AddMenuItem(removeclouds);
            }

            menu.OnItemSelect += (sender, item, index2) =>
            {
                if (item == removeclouds)
                {
                    ModifyClouds(true);
                }
                else if (item == randomizeclouds)
                {
                    ModifyClouds(false);
                }
                else if (item.ItemData is string weatherType)
                {
                    Notify.Custom($"天气将被更改为~y~{item.Text}~s~。这将需要{EventManager.WeatherChangeTime}秒。");
                    UpdateServerWeather(weatherType, EventManager.DynamicWeatherEnabled, EventManager.IsSnowEnabled);
                }
            };

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == dynamicWeatherEnabled)
                {
                    Notify.Custom($"动态天气变化现在已{(_checked ? "~g~启用" : "~r~禁用")}~s~。");
                    UpdateServerWeather(EventManager.GetServerWeather, _checked, EventManager.IsSnowEnabled);
                }
                else if (item == blackout)
                {
                    Notify.Custom($"停电模式现在已{(_checked ? "~g~启用" : "~r~禁用")}~s~。");
                    UpdateServerBlackout(_checked);
                }
                else if (item == vehicleBlackout)
                {
                    Notify.Custom($"车辆灯光停电模式现在已{(_checked ? "~g~启用" : "~r~禁用")}~s~。");
                    UpdateServerVehicleBlackout(!_checked);
                }
                else if (item == snowEnabled)
                {
                    if (EventManager.GetServerWeather is "XMAS" or "SNOWLIGHT" or "SNOW" or "BLIZZARD")
                    {
                        Notify.Custom($"当天气为~y~{EventManager.GetServerWeather}~s~时，无法禁用雪效果。");
                        return;
                    }

                    Notify.Custom($"雪效果现在将被强制{(_checked ? "~g~启用" : "~r~禁用")}~s~。");
                    UpdateServerWeather(EventManager.GetServerWeather, EventManager.DynamicWeatherEnabled, _checked);
                }
            };
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
    }
}