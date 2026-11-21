using System;
using ADOFAI.Editor;
using ADOFAI.Editor.Actions;
using HarmonyLib;
using UnityEngine;

namespace ADOFAI.SpeedModeAutoLenient
{
    /// <summary>
    /// Ctrl + Shift + Space 对应的编辑器动作：先把判定改成配置值，再直接调原版 Play()。
    /// 速度不归本 Mod 管 —— Ctrl 按住，原版会自己用「快捷播放速度」跑。
    /// </summary>
    internal sealed class PlayWithAutoLenientEditorAction : EditorAction
    {
        // None：不往游戏内「快捷键帮助」列表里插条目（那里没有对应本地化文案）
        public override EditorTabKey sectionKey => EditorTabKey.None;

        public override string descriptionKey => "SpeedModeAutoLenient";

        public override void Execute(scnEditor editor)
        {
            SpeedModeCore.Trigger(editor);
        }
    }

    [HarmonyPatch(typeof(scnEditor), "RegisterKeybinds")]
    internal static class Patch_RegisterKeybinds
    {
        [HarmonyPostfix]
        private static void Postfix(scnEditor __instance)
        {
            try
            {
                EditorKeybindManager manager =
                    AccessTools.Field(typeof(scnEditor), "keybindManager")?.GetValue(__instance) as EditorKeybindManager;

                if (manager == null)
                {
                    Main.Entry?.Logger.Warning("未找到 keybindManager，Ctrl+Shift+Space 快捷键注册失败。");
                    return;
                }

                EditorKeybind keybind = new EditorKeybind(KeyModifier.Control | KeyModifier.Shift, KeyCode.Space);
                if (manager.dictionary != null && manager.dictionary.ContainsKey(keybind))
                {
                    return; // 已注册过（编辑器重进时 RegisterKeybinds 会再调一次）
                }

                manager.RegisterKeybind(keybind, new PlayWithAutoLenientEditorAction());
            }
            catch (Exception ex)
            {
                Main.Entry?.Logger.LogException("注册 Ctrl+Shift+Space 快捷键失败", ex);
            }
        }
    }

    [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SwitchToEditMode))]
    internal static class Patch_SwitchToEditMode
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            SpeedModeCore.OnSwitchToEditModePrefix();
        }
    }
}
