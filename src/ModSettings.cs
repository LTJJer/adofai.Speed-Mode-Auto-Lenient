using System;
using System.IO;
using UnityEngine;
using UnityModManagerNet;

namespace ADOFAI.SpeedModeAutoLenient
{
    public class ModSettings
    {
        // Lenient / Normal / Strict / None(不改变)
        public string difficulty = "Lenient";

        internal static ModSettings Current = new ModSettings();

        private static string filePath;

        internal static void Load(UnityModManager.ModEntry modEntry)
        {
            filePath = Path.Combine(modEntry.Path, "settings.json");
            try
            {
                if (File.Exists(filePath))
                {
                    ModSettings loaded = JsonUtility.FromJson<ModSettings>(File.ReadAllText(filePath));
                    if (loaded != null)
                    {
                        Current = loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                modEntry.Logger.LogException("读取 settings.json 失败，使用默认值", ex);
            }

            Current.Normalize();
            Save();
        }

        internal static void Save()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            try
            {
                File.WriteAllText(filePath, JsonUtility.ToJson(Current, prettyPrint: true));
            }
            catch (Exception ex)
            {
                if (Main.Entry != null)
                {
                    Main.Entry.Logger.LogException("写入 settings.json 失败", ex);
                }
            }
        }

        internal static void SaveGUI(UnityModManager.ModEntry modEntry)
        {
            Normalize(Current);
            Save();
        }

        internal static void DrawGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("判定模式", GUILayout.Width(64f));
            DrawDifficulty("宽松 (Lenient)", "Lenient");
            DrawDifficulty("标准 (Normal)", "Normal");
            DrawDifficulty("严格 (Strict)", "Strict");
            DrawDifficulty("不改变", "None");
            GUILayout.EndHorizontal();
        }

        private static void DrawDifficulty(string label, string value)
        {
            bool selected = Current.difficulty == value;
            bool pressed = GUILayout.Toggle(selected, label, GUI.skin.button, GUILayout.Width(118f));
            if (pressed && !selected)
            {
                Current.difficulty = value;
                Save();
            }
        }

        internal void Normalize()
        {
            difficulty = SpeedModeCore.NormalizeDifficultyName(difficulty);
        }

        internal static void Normalize(ModSettings cfg)
        {
            cfg.Normalize();
        }
    }
}
