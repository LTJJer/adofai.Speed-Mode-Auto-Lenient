using System;
using HarmonyLib;
using UnityModManagerNet;

namespace ADOFAI.SpeedModeAutoLenient
{
    public static class Main
    {
        internal static UnityModManager.ModEntry Entry;

        private static Harmony harmony;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Entry = modEntry;
            ModSettings.Load(modEntry);

            if (!ApplyPatches())
            {
                modEntry.Logger.Error("Harmony 补丁应用失败，Mod 未生效。");
                return false;
            }

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = ModSettings.DrawGUI;
            modEntry.OnSaveGUI = ModSettings.SaveGUI;

            modEntry.Logger.Log("已加载：编辑器内 Ctrl + Shift + Space -> 判定 " + ModSettings.Current.difficulty
                + "（速度沿用原版 Ctrl 倍速，本 Mod 不改速度）");
            return true;
        }

        private static bool ApplyPatches()
        {
            try
            {
                if (harmony == null)
                {
                    harmony = new Harmony(Entry.Info.Id);
                }
                harmony.PatchAll(typeof(Main).Assembly);
                return true;
            }
            catch (Exception ex)
            {
                Entry.Logger.LogException("PatchAll 失败", ex);
                return false;
            }
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool enabled)
        {
            if (enabled)
            {
                return ApplyPatches();
            }

            if (harmony != null)
            {
                harmony.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        }
    }
}
