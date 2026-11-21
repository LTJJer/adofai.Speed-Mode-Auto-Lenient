using System;
using UnityEngine;

namespace ADOFAI.SpeedModeAutoLenient
{
    internal static class SpeedModeCore
    {
        private static bool difficultyOverridden;
        private static Difficulty originalDifficulty;

        /// <summary>
        /// 快捷键动作调用：先把判定改成配置值，再调原版 editor.Play()。
        /// 速度 / 音高完全交给原版：快捷键本身按住 Ctrl，原版 Play() 会用
        /// Persistence.shortcutPlaySpeed 算出 playbackSpeed 与 conductor 音高，本 Mod 一点都不碰。
        /// </summary>
        internal static void Trigger(scnEditor editor)
        {
            try
            {
                ApplyDifficulty(editor);
            }
            catch (Exception ex)
            {
                Log("应用判定失败", ex);
            }

            editor.Play();
        }

        private static void ApplyDifficulty(scnEditor editor)
        {
            if (difficultyOverridden)
            {
                return;
            }

            // Play() 里 floors.Count == 1 会直接 return，这种情况不改判定，免得卡在覆盖状态
            if (editor == null || editor.floors == null || editor.floors.Count <= 1)
            {
                return;
            }

            if (!TryParseDifficulty(ModSettings.Current.difficulty, out Difficulty difficulty))
            {
                return; // None：不改变当前判定
            }

            originalDifficulty = GCS.difficulty;
            GCS.difficulty = difficulty;
            difficultyOverridden = true;
        }

        /// <summary>scnEditor.SwitchToEditMode() 前置：还原判定，好让编辑器难度指示器显示回原值。</summary>
        internal static void OnSwitchToEditModePrefix()
        {
            if (!difficultyOverridden)
            {
                return;
            }

            GCS.difficulty = originalDifficulty;
            difficultyOverridden = false;
        }

        internal static string NormalizeDifficultyName(string name)
        {
            string key = (name ?? "").Trim().ToLowerInvariant();
            switch (key)
            {
                case "lenient":
                case "宽松":
                    return "Lenient";
                case "normal":
                case "标准":
                case "普通":
                    return "Normal";
                case "strict":
                case "严格":
                    return "Strict";
                case "none":
                case "off":
                case "不改变":
                    return "None";
                default:
                    return "Lenient";
            }
        }

        internal static bool TryParseDifficulty(string name, out Difficulty difficulty)
        {
            difficulty = Difficulty.Normal;
            switch (NormalizeDifficultyName(name))
            {
                case "Lenient":
                    difficulty = Difficulty.Lenient;
                    return true;
                case "Normal":
                    difficulty = Difficulty.Normal;
                    return true;
                case "Strict":
                    difficulty = Difficulty.Strict;
                    return true;
                default:
                    return false;
            }
        }

        private static void Log(string message, Exception ex)
        {
            if (Main.Entry != null)
            {
                Main.Entry.Logger.LogException(message, ex);
            }
        }
    }
}
