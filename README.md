# Speed Mode Auto Lenient

《A Dance of Fire and Ice》关卡编辑器内的辅助 Mod：按 `Ctrl + Shift + Space` 以原版 Ctrl 倍速开始铺面，
同时把判定**临时切换到宽松**（可在 Mod 配置里改 标准 / 严格 / 不改变）。回到编辑器时自动还原。

## 安装

前置：游戏已安装 [UnityModManager](https://www.nexusmods.com/site/mods/21)。

下载 Release 里的 zip 并通过 UnityModManager 安装。

## 从源码构建

需要 .NET SDK（`net48` 目标，`Microsoft.NETFramework.ReferenceAssemblies` 由 NuGet 自动还原）。

```bash
cd src
copy local.props.example local.props   # 编辑 local.props，填入本机游戏 Managed 目录
dotnet build -c Release
```

产物输出到 `dist/ADOFAI.SpeedModeAutoLenient/`（dll + Info.json）。
也可以不建 `local.props`，直接传路径：

```bash
dotnet build -c Release -p:GameManaged="<游戏目录>_Data\Managed"
```

`tools/verify.ps1` 是静态自检脚本：加载产物，核对补丁目标与游戏侧成员签名是否仍然存在。

## 工作原理

两个 Harmony 补丁，均挂在 `scnEditor` 上：

1. `RegisterKeybinds`（后置）—— 往 `keybindManager` 里多注册一个 `Control|Shift + Space` 键位。
   修饰键为精确匹配，因此不与原版 `Space` / `Ctrl+Space` / `Shift+Space` 冲突。
2. 键位动作 —— 记下当前 `GCS.difficulty`，设为配置值，再调原版 `editor.Play()`。
3. `SwitchToEditMode`（前置）—— 把 `GCS.difficulty` 还原为进试玩前的值。

速度与音高没有任何改动：快捷键本身按住 Ctrl，原版 `Play()` 会按 `Persistence.shortcutPlaySpeed`
自行计算 `playbackSpeed` 与 conductor 音高，所以行为与 `Ctrl + Space` 完全一致，
包括同样置位 `editorQuickPitchedPlaying`（这类试玩不计入成绩 / 成就）。

## 兼容性

- 开发与验证环境：《A Dance of Fire and Ice》Steam 版（Unity 6000.3.25f1）、UnityModManager 0.33.3、Harmony 2.3.6
- 补丁点依赖 `scnEditor.RegisterKeybinds` / `scnEditor.SwitchToEditMode` / `GCS.difficulty`；
  游戏大版本更新后若签名变化，`Load` 会拒绝启用并在 UMM 日志中报错，而不是留下半可用状态
- 其它游戏版本未测试

## 许可

[MIT](LICENSE)
