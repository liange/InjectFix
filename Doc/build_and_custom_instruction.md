# InjectFix 构建流程与 Instruction.cs 定制说明

本文基于 `Source/VSProj` 下的构建脚本与源码，说明 InjectFix 的整体使用流程，以及"每个项目一份私有补丁格式"的核心——`Instruction.cs` 的定制机制。

---

## 一、构建产物与三者关系

运行 `build_for_unity.bat`（Windows）/ `build_for_unity.sh`（macOS）/ `build_for_unity.py`（跨平台，功能最全）会产出三个关键文件，它们必须**配套使用**：

| 产物 | 输出位置 | 运行端 | 作用 |
| :--- | :--- | :--- | :--- |
| `IFix.Core.dll` | `UnityProj/Assets/Plugins/IFix.Core.dll` | 游戏运行时 | 虚拟机，负责加载并执行补丁 |
| `IFix.exe` | `UnityProj/IFixToolKit/IFix.exe` | 编辑器（生成补丁） | 把修复后的 C# 代码翻译成补丁指令流 |
| `Instruction.cs` | `Source/VSProj/Instruction.cs` | —— | 由 `ShuffleInstruction.exe` 生成的"指令表"，**同时编进上面两个产物** |

> 关键点：`Instruction.cs` 是同一份文件，被同时编译进 `IFix.Core.dll`（`build_for_unity.bat:12-23`）和 `IFix.exe`（`build_for_unity.bat:26`）。两端必须共享**相同的指令编号顺序**和**相同的 `INSTRUCTION_FORMAT_MAGIC`**，补丁才能被正确解析与执行。

---

## 二、完整使用流程

### 1. 编译核心库与工具

修改构建脚本中的 Unity 安装路径：

- `.bat`：修改第 1 行 `UNITY_HOME`
- `.sh`：修改第 2 行 `UNITY_HOME`
- `.py`：修改 `config.json` 的 `UnityHome` 字段（`config.json:2`）

然后运行对应脚本。脚本按顺序完成 4 步（以 `build_for_unity.py` 为例）：

1. 编译 `ShuffleInstruction.cs` → `ShuffleInstruction.exe`（`build_for_unity.py:33-34`）
2. 运行 `ShuffleInstruction.exe`，由模板 `Src/Core/Instruction.cs` 生成打乱后的 `Instruction.cs`（`build_for_unity.py:37-38`）
3. 编译 `IFix.Core.dll`（`build_for_unity.py:41-53`）
4. 复制 `Mono.Cecil*` 并编译 `IFix.exe` 到 `IFixToolKit`（`build_for_unity.py:56-67`）

### 2. 复制到 Unity 工程

- `IFixToolKit/` 整个目录 → 拷到 Unity 项目的 `Assets` 同级目录
- `Assets/IFix/`（Editor 菜单与配置标签定义）→ 拷到 Unity 项目的 `Assets` 下
- `Assets/Plugins/IFix.Core.dll` → 拷到 Unity 项目的 `Assets/Plugins` 下

### 3. 配置注入清单（Editor 目录）

在 `Assets/.../Editor/` 下写一个 `[Configure]` 类，用 `[IFix]` 静态属性列出"将来可能需要修复的类"：

```csharp
[Configure]
public class HelloworldCfg
{
    [IFix]
    static IEnumerable<Type> hotfix
    {
        get
        {
            return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                    where type.Namespace == "XXXX"
                    select type).ToList();
        }
    }
}
```

要点：配置类必须放在 `Editor` 目录，`[IFix]` 属性必须为 `static`。详见 `user_manual.md`。

### 4. 注入（Inject，发包时一次）

对 `[IFix]` 清单中的类做静态代码预处理，只有注入过的类后续才能加载补丁：

- 自动：打手机包时 InjectFix 会自动预处理；
- 手动：调用 `IFix.Editor.IFixEditor.InjectAllAssemblys`，或菜单 `InjectFix/Inject`（`ILFixEditor.cs:180`）。

### 5. 制作补丁（Fix）

给要修复的函数打上 `[IFix.Patch]` 并改写为正确逻辑，然后：

- 函数**不含**平台条件编译宏：菜单 `InjectFix/Fix`（`ILFixEditor.cs:941`）；
- 含 `#if UNITY_ANDROID` / `#if UNITY_IOS` 等宏：用 `InjectFix/Fix(Android)`（`ILFixEditor.cs:968`）或 `InjectFix/Fix(IOS)`（`ILFixEditor.cs:983`），处理方式见 `faq.md`。

成功后会在工程根目录生成 `{Dll Name}.patch.bytes`（如 `Assembly-CSharp.patch.bytes`）。

> 注意：修改代码与 Fix 之间不要执行 Inject，否则 IFix 会判定为"线上已注入版本"拒绝生成补丁（`faq.md`）。

### 6. 加载补丁（运行时）

```csharp
var patchPath = "./Assets/IFix/Resources/Assembly-CSharp.patch.bytes";
if (File.Exists(patchPath))
{
    PatchManager.Load(new FileStream(patchPath, FileMode.Open));
}
```

---

## 三、Instruction.cs 定制机制（核心）

### 1. 为什么要定制

InjectFix 的补丁不是直接的可执行代码，而是一串**自定义指令流**。补丁文件里存的是每条指令在 `enum Code` 中的**序号**，并在文件头部写入一个 `INSTRUCTION_FORMAT_MAGIC` 校验值。

- 生成补丁端（`IFix.exe`）：`CodeTranslator.cs:3855` 写入 MAGIC；
- 加载补丁端（`IFix.Core.dll`）：`FileVirtualMachineBuilder.cs:291-296` 读取并校验 MAGIC，不匹配直接报错拒绝加载。

因此，只要每个项目的 `Instruction.cs`（指令顺序 + MAGIC）不同：

- **指令编号不同** → 别人项目的补丁即使混进来也会执行错乱指令；
- **MAGIC 不同** → 加载时直接被拒绝。

这就是 README 所说的"每个游戏一份私有补丁格式，安全更有保障"。

### 2. ShuffleInstruction 工作原理

`ShuffleInstruction.cs` 读取模板 `Src/Core/Instruction.cs`，对 `public enum Code { ... }` 块做两件事：

1. **打乱指令顺序**：用 Fisher-Yates 洗牌重排 `enum Code` 内的每一行指令定义（`ShuffleInstruction.cs:22-37`）。指令在 enum 中的位置即其数值编号，顺序变了，编号也就变了。
2. **生成随机 MAGIC**：用两个 32 位随机数拼成 64 位 `INSTRUCTION_FORMAT_MAGIC`，正则替换写回文件（`ShuffleInstruction.cs:84-93`）。

> 模板 `Src/Core/Instruction.cs` 中的 `INSTRUCTION_FORMAT_MAGIC = 317431043901`（`Src/Core/Instruction.cs:204`）只是占位值，每次运行都会被随机覆盖，**不要手动修改模板里的 MAGIC**。

### 3. 三个脚本的定制能力差异

| 脚本 | 是否传 `ConfuseKey`（密钥） | 行为 |
| :--- | :--- | :--- |
| `build_for_unity.bat` | 否（`build_for_unity.bat:11`） | 用当前毫秒做随机种子 |
| `build_for_unity.sh` | 否（`build_for_unity.sh:12`） | 用当前毫秒做随机种子 |
| `build_for_unity.py` | 是，取 `config.json` 的 `ConfuseKey`（`build_for_unity.py:38`） | 密钥非空时用 `ConfuseKey.GetHashCode()` 做种子 |

`ConfuseKey` 的作用（`ShuffleInstruction.cs:24-25, 84-85`）：作为随机种子，使**同一密钥每次生成完全相同的打乱结果与 MAGIC**，便于团队多人各自构建得到一致的 `Instruction.cs`。

---

## 四、每个项目如何定制 Instruction.cs

### 方式一：首次随机生成（默认，最简单）

每个项目独立拷贝一份 `Source/VSProj`，首次运行构建脚本即可：

1. 直接运行 `build_for_unity.bat` / `build_for_unity.sh`；
2. `ShuffleInstruction.exe` 基于当前毫秒随机生成项目专属的 `Instruction.cs`（随机指令顺序 + 随机 MAGIC）；
3. 将生成的 `Source/VSProj/Instruction.cs`、`IFix.Core.dll`、`IFix.exe` 一并**纳入版本控制**，团队共享同一份。

> ⚠️ **重要**：`ShuffleInstruction.cs:47` 有判断 `if (File.Exists(des))` —— 只要 `Instruction.cs` 已存在就直接跳过、不重新生成。所以首次生成后，后续重复运行脚本不会再改动它。这也意味着：**不要误删 `Instruction.cs`**，一旦删除会触发重新生成，导致新指令表与历史补丁全部不兼容。

### 方式二：ConfuseKey 密钥定制（可复现，推荐团队协作）

适合希望"凭密钥即可重建完全一致指令表"的场景：

1. 编辑 `config.json`，填入项目专属密钥：
   ```json
   {
       "UnityHome": "/Applications/Unity2017/Unity.app",
       "DllOutput": "../UnityProj/Assets/Plugins/IFix.Core.dll",
       "ToolKitOutput": "../UnityProj/IFixToolKit",
       "ConfuseKey": "my-project-secret-key-2026"
   }
   ```
2. **删除已存在的 `Source/VSProj/Instruction.cs`**（否则因 `File.Exists` 判断而被跳过）；
3. 运行 `build_for_unity.py`，按密钥重新生成 `Instruction.cs`；
4. 将 `Instruction.cs`、`config.json`（注意密钥保密，可不入库或加密管理）纳入版本控制。

这样任何成员只要拿到同一 `ConfuseKey`，删掉旧 `Instruction.cs` 后重新构建，即可得到字节一致的指令表与 MAGIC。

> 注意：只有 `.py` 脚本支持 `ConfuseKey`；`.bat` / `.sh` 即便配了密钥也不会使用。

### 不同项目间的隔离原则

- 每个项目使用**各自独立**的 `Instruction.cs`，项目 A 的补丁**不能**在项目 B 运行（MAGIC / 指令编号均不同）；
- 项目 A 的 `IFix.Core.dll` 与 `IFix.exe` **必须配套**（同一份 `Instruction.cs` 编出），不可跨项目混用；
- 升级 InjectFix 版本、或修改了 `Src/Core/Instruction.cs` 模板的指令集后，需要删除产物 `Instruction.cs` 重新生成，并重新构建两端，旧补丁随之失效。

---

## 五、注意事项汇总

1. **手动改 `IFix.Core` 源码后必须用 `build_for_unity.bat` 重新构建**，否则 IL2CPP 会报 `EvaluationStackOperation::ToObject` 之类错误（`faq.md`）。不要用其他方式单独编译 `IFix.Core.dll`。
2. **不要手动编辑 `Source/VSProj/Instruction.cs`**（产物）和 `Src/Core/Instruction.cs`（模板的指令顺序）。需要调整指令集应改模板，然后删产物重新生成。
3. **`Instruction.cs` 一旦确定就长期不变**：它决定历史所有补丁的兼容性。重生成 = 历史补丁全部失效。
4. **两端配套**：游戏端 `IFix.Core.dll` 与工具端 `IFix.exe` 必须来自同一次构建（同一 `Instruction.cs`），否则补丁编号错位。
5. **密钥保密**：`ConfuseKey` 决定项目补丁格式，泄露后他人可仿制兼容补丁，应作为敏感配置管理。
6. **生成 Patch 报 `must not be inject`**：目标 dll 已被注入，在 Unity 工程根目录右键 Reimport 后重试（`faq.md`）。
