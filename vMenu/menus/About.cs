using System;

using MenuAPI;

using vMenuShared;

namespace vMenuClient.menus
{
    // Token: 0x0200001E RID: 30
    public class About
    {
        // Token: 0x06000202 RID: 514
        private void CreateMenu()
        {
            this.menu = new Menu("vMenu", "关于 vMenu");
            MenuItem version = new MenuItem("vMenu 汉化版本 - By.Mete0r", "此服务器正在使用 vMenu - 汉化By.Mete0r ~b~~h~" + MainMenu.Version + "~h~~s~.")
            {
                Label = "~h~" + MainMenu.Version + "~h~"
            };
            MenuItem credits = new MenuItem("关于 vMenu / 致谢", "vMenu 由 ~b~Vespura~s~ 制作。欲了解更多信息，请访问 ~b~www.vespura.com/vmenu~s~。感谢以下各位的贡献：Deltanic, Brigliar, IllusiveTea, Shayan Doust, zr0iq 和 Golden！");
            string serverInfoMessage = ConfigManager.GetSettingsString(ConfigManager.Setting.vmenu_server_info_message, null);
            if (!string.IsNullOrEmpty(serverInfoMessage))
            {
                MenuItem serverInfo = new MenuItem("服务器信息", serverInfoMessage);
                string siteUrl = ConfigManager.GetSettingsString(ConfigManager.Setting.vmenu_server_info_website_url, null);
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    serverInfo.Label = (siteUrl ?? "");
                }
                this.menu.AddMenuItem(serverInfo);
            }
            this.menu.AddMenuItem(version);
            this.menu.AddMenuItem(credits);
        }

        // Token: 0x06000203 RID: 515
        public Menu GetMenu()
        {
            if (this.menu == null)
            {
                this.CreateMenu();
            }
            return this.menu;
        }

        // Token: 0x0400007E RID: 126
        private Menu menu;
    }
}
