# 安装与配置

WPF UI Skills（仓库目录 `wpf-styles`）本身就是一个 skill：入口是 `SKILL.md`，附带主题、文档、检查工具和测试模板。
`install.ps1` 会把整套内容安装到主流 AI 编码客户端，并为不支持 skill 的客户端写入规则文件或
`AGENTS.md` 标记块。

---

## 1. 快速开始

```powershell
# 查看本机检测到哪些客户端
pwsh ./install.ps1 -Detect

# 查看所有支持的客户端和路径
pwsh ./install.ps1 -List

# 安装到本机已检测到的客户端（用户级）
pwsh ./install.ps1

# 安装到所有客户端（用户级，会创建目录）
pwsh ./install.ps1 -Clients all

# 只安装到指定客户端
pwsh ./install.ps1 -Clients zcode,cursor,copilot,claude,workbuddy,trae-cn

# 安装到当前项目（项目级，可提交给团队）
pwsh ./install.ps1 -Scope Project

# 安装到指定项目
pwsh ./install.ps1 -Scope Project -Root "D:\workspace\inventor\OpenCADTranslate" -Clients all

# 同时安装用户级和项目级
pwsh ./install.ps1 -Scope Both -Clients zcode,cursor,copilot,codex,workbuddy

# 只看会写哪些文件，不实际写入
pwsh ./install.ps1 -Scope Both -Clients all -DryRun

# 覆盖已有安装（会先备份）
pwsh ./install.ps1 -Scope Global -Clients all -Force
```

安装后重启客户端，或重新打开窗口，让客户端重新扫描 skill/规则。

---

## 2. 参数

| 参数 | 默认值 | 说明 |
|---|---|---|
| `-Scope` | `Global` | `Global` 用户级；`Project` 项目级；`Both` 两者 |
| `-Clients` | `auto` | `auto` 只装检测到的客户端；`all` 全部；也可逗号分隔 |
| `-Root` | 当前目录 | `Project`/`Both` 的项目根目录 |
| `-SkillName` | `wpf-ui-style` | 安装后的技能目录名 |
| `-Source` | 脚本所在目录 | 技能源目录 |
| `-Force` | 关闭 | 覆盖已有技能/规则文件；共享文件会先备份 |
| `-DryRun` | 关闭 | 只打印动作，不写文件 |
| `-Detect` | — | 列出本机检测到的客户端后退出 |
| `-List` | — | 列出所有支持的客户端和路径后退出 |

`auto` 的检测依据是各客户端的配置目录（如 `~/.zcode`、`~/.cursor`、`~/.workbuddy`）。
如果一个都没检测到，会回退到通用 `agents`（`~/.agents/skills`）。

---

## 3. 支持的客户端

“最佳努力”表示该路径来自社区/本机观察，客户端版本升级后可能变化；安装后请在客户端里确认一次。

| 客户端 | 用户级 skill | 项目级 skill | 备注 |
|---|---|---|---|
| Generic Agent Skills | `~/.agents/skills` | `.agents/skills` | 通用标准，Copilot/Claude/Codex 等兼容 |
| ZCode | `~/.zcode/skills` | `.zcode/skills` | — |
| Claude Code | `~/.claude/skills` | `.claude/skills` | — |
| GitHub Copilot / VS Code | `~/.copilot/skills`、`~/.agents/skills` | `.github/skills`、`.agents/skills` | 另写 `.github/instructions/*.instructions.md` 和 `AGENTS.md` |
| Cursor | `~/.cursor/skills`（最佳努力） | `.cursor/skills` | 另写 `.cursor/rules/*.mdc` 和 `AGENTS.md` |
| Windsurf / Codeium | `~/.codeium/windsurf/memories/global_rules.md`、`~/.windsurf/rules`（最佳努力） | `.windsurf/rules` | 规则文件 |
| Cline | `~/Documents/Cline/Rules` | `.clinerules` | 规则文件 |
| Roo Code | `~/.roo/rules` | `.roo/rules` | 规则文件 |
| Kilo Code | `~/.kilocode/rules` | `.kilocode/rules` | 规则文件 |
| Continue | `~/.continue/rules` | `.continue/rules` | 规则文件 |
| OpenAI Codex | `~/.codex/AGENTS.md` | `AGENTS.md` | 标记块 |
| Gemini CLI | `~/.gemini/config/skills` | `.gemini/skills` | — |
| Qwen Code | `~/.qwen/skills` | `.qwen/skills` | — |
| Grok CLI | `~/.grok/skills`、`~/.grok/rules` | `.grok/skills`、`.grok/rules` | — |
| Kiro | `~/.kiro/skills`、`~/.kiro/steering` | `.kiro/skills`、`.kiro/steering` | — |
| Trae | `~/.trae/skills` | `.trae/skills` | — |
| Trae CN | `~/.trae-cn/skills` | `.trae-cn/skills` | 本机已确认 |
| WorkBuddy | `~/.workbuddy/skills` | `.workbuddy/skills` | 本机已确认 |
| CodeBuddy | `~/.codebuddy/skills` | `.codebuddy/skills` | — |
| CodeBuddy CN | `~/.codebuddycn/skills` | `.codebuddycn/skills` | — |
| DSH (DeepSeek Harness) | `~/.dsh/skills`（最佳努力）、`~/.dsh/AGENTS.md` | `AGENTS.md` | 以 `AGENTS.md` 为主 |
| Antigravity | `~/.antigravity-ide/skills`（最佳努力）、`~/.antigravity-ide/AGENTS.md` | `AGENTS.md` | — |
| MarsCode | `~/.marscode/skills`（最佳努力）、`~/.marscode/rules` | `.marscode/skills`、`.marscode/rules` | — |
| Zed | `~/.config/zed/skills`（最佳努力）、`~/.config/zed/AGENTS.md` | `.zed/skills`、`AGENTS.md` | — |
| Aider | `~/.aider/CONVENTIONS.md` | `CONVENTIONS.md` | 标记块 |
| OpenHands | `~/.openhands/skills`（最佳努力） | `.openhands/skills`、`AGENTS.md` | — |
| JetBrains AI Assistant | `~/.aiassistant/rules` | `.aiassistant/rules` | 规则文件 |

> “规则文件”是给不支持 skill 的客户端用的；内容包含强制规则和指向 `SKILL.md` 的路径。
> `AGENTS.md` / `CONVENTIONS.md` 采用 `<!-- BEGIN wpf-ui-style -->` 标记块，重复安装不会重复追加。

---

## 4. 安装后各客户端怎么验证

### ZCode / Trae / WorkBuddy / CodeBuddy / Kiro / Gemini / Qwen / Grok

1. 重启客户端。
2. 打开技能/规则面板，确认出现 `wpf-ui-style`。
3. 在对话里输入：

```text
按 wpf-ui-style 规范检查这个 WPF 窗口
```

### GitHub Copilot / VS Code

1. 重新加载 VS Code 窗口。
2. 确认 `.github/skills/wpf-ui-style/SKILL.md` 存在。
3. 确认 `.github/instructions/wpf-ui-style.instructions.md` 的 `applyTo` 是 `**/*.xaml`。
4. 在 Copilot Chat 中提问：

```text
Check this WPF window against the wpf-ui-style skill.
```

### Cursor

1. 重新加载窗口。
2. 确认 `.cursor/skills/wpf-ui-style/SKILL.md` 和 `.cursor/rules/wpf-ui-style.mdc` 存在。
3. 打开一个 `.xaml` 文件，在 Agent 中提问：

```text
按 .cursor/rules/wpf-ui-style.mdc 检查这个 XAML。
```

### Windsurf / Cline / Roo / Kilo / Continue / JetBrains

1. 重新加载窗口。
2. 确认对应规则文件存在（例如 `.windsurf/rules/wpf-ui-style.md`、`.clinerules/wpf-ui-style.md`）。
3. 在 Chat 中要求按 `wpf-ui-style` 规范检查。

### Codex / DSH / Antigravity / Aider

1. 确认 `AGENTS.md` / `CONVENTIONS.md` 中存在 `<!-- BEGIN wpf-ui-style -->` 标记块。
2. 新开一个会话，让客户端重新加载项目指令。
3. 提问时直接说“按 wpf-ui-style 规范检查”。

---

## 5. 安装内容

每个 skill 目录包含：

```text
wpf-ui-style/
├─ SKILL.md                 ← 技能入口
├─ README.md                ← 总览
├─ INSTALL.md               ← 本文件
├─ install.ps1              ← 安装器
├─ Theme/                   ← 可直接合并的 WPF 主题
├─ docs/                    ← 详细规范
├─ references/              ← 速查与 XAML 片段
├─ samples/                 ← 示例窗口
├─ tools/                   ← 对比度/布局检查
└─ tests/                   ← xUnit 契约测试模板
```

对于规则型客户端，还会写入：

- `wpf-ui-style.md` / `wpf-ui-style.mdc` / `wpf-ui-style.instructions.md`：精简规则 + 指向 skill 的路径。
- `AGENTS.md` / `CONVENTIONS.md`：`<!-- BEGIN wpf-ui-style -->` 标记块。
- `global_rules.md`（Windsurf/Codeium）：同样的标记块。

如果本次只选择了规则型客户端（例如只装 Windsurf + Cline），安装器会自动在
`~/.agents/skills`（用户级）或 `.agents/skills`（项目级）补装一份通用 skill，
这样规则文件里的 `SKILL.md` 路径始终有效。

---

## 6. 在项目里使用主题

安装 skill 只是让 AI 知道规范；业务项目仍需要合并主题：

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary
                Source="pack://application:,,,/YourAssembly;component/Theme/Theme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

把 `Theme/` 复制到你的 WPF 项目，或把 `wpf-styles/Theme` 作为共享项目链接。
插件项目在窗口构造时合并：

```csharp
window.Resources.MergedDictionaries.Add(new ResourceDictionary
{
    Source = new Uri(
        "pack://application:,,,/YourAssembly;component/Theme/Theme.xaml",
        UriKind.Absolute)
});
```

---

## 7. 运行检查

安装到用户级后：

```powershell
# 对比度（.ps1 会自动探测 py -3 / python3 / python）
pwsh "$HOME/.agents/skills/wpf-ui-style/tools/ContrastCheck.ps1"

# 布局（多行文本撑满、Height="*" 等）
pwsh "$HOME/.agents/skills/wpf-ui-style/tools/LayoutCheck.ps1" -Root "D:\your\project"

# 也可以直接运行 Python（CI / Linux / macOS）
python "$HOME/.agents/skills/wpf-ui-style/tools/ContrastCheck.py"
python "$HOME/.agents/skills/wpf-ui-style/tools/LayoutCheck.py" --root "D:\your\project"

# 契约测试（复制到测试项目后）
dotnet test
```

> 校验脚本需要 Python 3（仅标准库，无需 `pip install`）。安装脚本 `install.ps1` 本身不依赖 Python。

项目级安装时把路径换成 `.zcode/skills/wpf-ui-style/...` 或对应客户端路径。

---

## 8. 自定义配置

| 想改什么 | 改哪里 |
|---|---|
| 颜色、字体、尺寸、圆角 | `Theme/Tokens.xaml` |
| 控件状态、模板 | `Theme/Controls.xaml`、`Theme/DataControls.xaml` |
| 强制规则、检查清单 | `SKILL.md` |
| 状态矩阵 | `docs/02-control-states.md` |
| 布局/多行文本规则 | `docs/03-layout.md` |
| 常见错误 | `docs/04-common-pitfalls.md` |
| 速查与代码片段 | `references/cheatsheet.md`、`references/xaml-snippets.md` |
| 对比度阈值 | `tools/ContrastCheck.py` 的 `PAIRS` |
| 布局检查规则 | `tools/LayoutCheck.py` |

修改后重新安装并加 `-Force`，让已安装的副本更新：

```powershell
pwsh ./install.ps1 -Scope Both -Clients all -Force
```

---

## 9. 卸载

### 删除 skill 目录

```powershell
Remove-Item "$HOME/.agents/skills/wpf-ui-style" -Recurse -Force
# 以及其他客户端目录，例如：
# Remove-Item "$HOME/.zcode/skills/wpf-ui-style" -Recurse -Force
# Remove-Item "$HOME/.workbuddy/skills/wpf-ui-style" -Recurse -Force
```

### 删除规则/指令文件

```powershell
Remove-Item ".cursor/rules/wpf-ui-style.mdc" -Force
Remove-Item ".github/instructions/wpf-ui-style.instructions.md" -Force
Remove-Item ".windsurf/rules/wpf-ui-style.md" -Force
Remove-Item ".clinerules/wpf-ui-style.md" -Force
```

### 从 AGENTS.md / CONVENTIONS.md 移除标记块

删除以下两行之间的内容：

```text
<!-- BEGIN wpf-ui-style -->
...
<!-- END wpf-ui-style -->
```

不要删除整个 `AGENTS.md`，它可能包含项目自己的指令。

---

## 10. 故障排查

| 问题 | 处理 |
|---|---|
| 客户端看不到 skill | 重启客户端；确认 `SKILL.md` 在 `<skills>/<name>/SKILL.md`；确认 frontmatter 的 `name`/`description` |
| `-Clients auto` 没装任何东西 | 客户端配置目录不在检测列表；用 `-Detect` 查看，或 `-Clients all` / 显式指定 |
| 规则文件没有生效 | 检查客户端是否读取该路径；部分客户端只读项目级规则，不读用户级规则 |
| 重复安装出现重复 frontmatter | 已修复；规则文件会整体替换。若旧文件损坏，加 `-Force` 重装 |
| 不想覆盖已有文件 | 默认跳过；`-Force` 会先备份为 `.bak-<timestamp>` |
| 项目级安装到错误目录 | 使用 `-Root` 指定项目根目录，或先 `cd` 到项目根目录 |
| 路径中有空格 | PowerShell 参数用引号包住，例如 `-Root "D:\My Project"` |
| 公司网络/代理 | 安装器只写本地文件，不联网 |

---

## 11. 推荐组合

| 场景 | 命令 |
|---|---|
| 个人机器，一次装全 | `pwsh ./install.ps1 -Scope Global -Clients all` |
| 只给 ZCode 用 | `pwsh ./install.ps1 -Scope Global -Clients zcode` |
| 团队仓库统一规范 | `pwsh ./install.ps1 -Scope Project -Root <repo> -Clients copilot,cursor,claude,codex` |
| 先看会写什么 | `pwsh ./install.ps1 -Scope Both -Clients all -DryRun` |
| 更新已安装版本 | `pwsh ./install.ps1 -Scope Both -Clients all -Force` |
