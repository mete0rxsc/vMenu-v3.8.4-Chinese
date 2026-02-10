using CitizenFX.Core;

using MenuAPI;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;

namespace vMenuClient.menus
{
    public class Recording
    {
        // Variables
        private Menu menu;

        private void CreateMenu()
        {
            AddTextEntryByHash(0x86F10CE6, "Upload To Cfx.re Forum"); // Replace the "Upload To Social Club" button in gallery
            AddTextEntry("ERROR_UPLOAD", "Are you sure you want to upload this photo to Cfx.re forum?"); // Replace the warning message text for uploading

            // Create the menu.
            menu = new Menu("录制", "录制选项");

            var takePic = new MenuItem("拍照(客户端会崩溃)", "拍照，别用，有Bug会崩溃你的客户端");
            var openPmGallery = new MenuItem("打开相册", "打开你的暂停菜单相册");
            var startRec = new MenuItem("开始录制", "使用GTA V的内置录制功能开始新的游戏录制.");
            var stopRec = new MenuItem("停止录制", "停止并保存当前录制.");
            var openEditor = new MenuItem("R星编辑器", "打开rockstar编辑器，注意在执行此操作之前，您可能希望先退出会话，以防止出现一些问题");

            menu.AddMenuItem(takePic);
            menu.AddMenuItem(openPmGallery);
            menu.AddMenuItem(startRec);
            menu.AddMenuItem(stopRec);
            menu.AddMenuItem(openEditor);

            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == startRec)
                {
                    if (IsRecording())
                    {
                        Notify.Alert("您已经在录制剪辑，需要先停止录制，然后才能重新开始录制！");
                    }
                    else
                    {
                        StartRecording(1);
                    }
                }
                else if (item == openPmGallery)
                {
                    ActivateFrontendMenu((uint)GetHashKey("FE_MENU_VERSION_MP_PAUSE"), true, 3);
                }
                else if (item == takePic)
                {
                    BeginTakeHighQualityPhoto();
                    SaveHighQualityPhoto(-1);
                    FreeMemoryForHighQualityPhoto();
                }
                else if (item == stopRec)
                {
                    if (!IsRecording())
                    {
                        Notify.Alert("您当前没有录制剪辑，需要先开始录制，然后才能停止并保存剪辑。");
                    }
                    else
                    {
                        StopRecordingAndSaveClip();
                    }
                }
                else if (item == openEditor)
                {
                    if (GetSettingsBool(Setting.vmenu_quit_session_in_rockstar_editor))
                    {
                        QuitSession();
                    }
                    ActivateRockstarEditor();
                    // wait for the editor to be closed again.
                    while (IsPauseMenuActive())
                    {
                        await BaseScript.Delay(0);
                    }
                    // then fade in the screen.
                    DoScreenFadeIn(1);
                    Notify.Alert("您在进入Rockstar编辑器之前离开了上一个会话。重新启动游戏，以便能够重新加入服务器的主会话。", true, true);
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
