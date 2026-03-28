# 第一个插件

### 概述

COG 插件系统已演进为使用 **Python (IronPython)** 进行插件开发，替代了早期的 JavaScript 实现。本指南涵盖了最新的插件开发实践和 API。

### 插件架构

插件系统由几个关键组件组成：

#### 1. **PythonPluginManager**
- 管理插件发现、加载和生命周期
- 处理依赖解析和插件排序
- 管理从 `.ca` 包文件提取插件
- 负责调用插件生命周期回调

**关键方法：**
```csharp
void LoadAllPlugins()        // 从插件文件夹发现并加载所有插件
void LoadPlugin(string path) // 从指定路径加载单个插件
void UnloadPlugin(Plugin plugin) // 卸载特定插件
Plugin[] GetPlugins()        // 获取所有已加载的插件
Plugin? GetPlugin(string name) // 按名称获取插件
void DisableAllPlugins()     // 卸载所有插件
```

#### 2. **PythonPluginHandler**
- 使用 IronPython 管理 Python 脚本执行
- 创建和管理插件实例
- 调用生命周期回调 (`on_initialize`, `on_shutdown`)
- 向插件作用域注入 API 和工具

**关键方法：**
```csharp
void LoadMainScript()   // 加载并实例化插件
void OnInitialize()     // 调用插件的 on_initialize() 方法
void OnShutdown()       // 调用插件的 on_shutdown() 方法
dynamic? GetPythonInstance() // 获取插件实例
```

#### 3. **PluginDescription**
表示来自 `plugin.yml` 的插件元数据：
```csharp
public record PluginDescription(
    string Name,              // 插件名称
    string Version,           // 插件版本
    string? Description,      // 插件描述
    string ApiVersion,        // 所需的 COG API 版本
    string Main,              // 主类引用 (例如: "main.MyPlugin")
    string[]? Authors,        // 插件作者
    string[]? Depends,        // 硬依赖
    string[]? SoftDepends,    // 软依赖
    string? Website,          // 插件网站/仓库
    string Prefix,            // 日志前缀
    string[]? LoadBefore)     // 应在此之后加载的插件
```

### 插件包格式 (.ca 文件)

COG 插件以 `.ca` 文件的形式分发，这是包含以下内容的标准 ZIP 压缩包：

```
plugin.ca
├── plugin.yml          # 插件元数据（必需）
├── scripts/            # Python 脚本文件夹（必需）
│   ├── main.py        # 主插件文件
│   ├── utils.py       # 辅助模块
│   └── modules/       # 外部依赖
│       └── module.zip
├── resources/         # 资源文件（可选）
│   ├── icon.png
│   └── data.json
└── docs/             # 文档（可选）
```

### plugin.yml 规范

#### 必需字段

```yaml
name: PluginName
version: 1.0.0
api-version: 1.0.0
main: main.PluginClass
```

- **name** (字符串): 唯一的插件标识符
  - 必须是字母数字和下划线
  - 用于依赖引用和日志记录
  
- **version** (字符串): 语义化版本 (例如: "1.0.0", "2.1.3-beta")
  - 用于版本检查
  
- **api-version** (字符串): COG API 版本要求
  - 与模组版本比较以确定兼容性
  - 如果插件需要更新的 API，加载将被拒绝
  
- **main** (字符串): 入口点，格式为 `module.ClassName`
  - 例如: `main.MyAwesomePlugin`
  - 系统将导入模块并实例化该类

#### 可选字段

```yaml
description: 插件描述文本
authors:
  - 作者1
  - 作者2
website: https://github.com/author/plugin
prefix: PLUG  # 日志前缀（默认为插件名称）

# 依赖管理
depend:
  - RequiredPlugin1
  - RequiredPlugin2
soft-depend:
  - OptionalPlugin1

# 加载顺序
load-before:
  - PluginThatLoadsAfterThis
```

- **description** (字符串): 插件功能的简短描述
- **authors** (列表): 插件作者列表
- **website** (字符串): 插件仓库或网站的 URL
- **prefix** (字符串): 插件日志前缀（默认为插件名称）
- **depend** (列表): 硬依赖 - 如果缺失，插件将无法加载
- **soft-depend** (列表): 软依赖 - 即使缺失，插件仍会加载
- **load-before** (列表): 应在此插件之后加载的插件

### Python 插件开发

#### 基本插件结构

```python
class MyAwesomePlugin:
    """你的插件类"""
    
    def __init__(self):
        """构造函数 - 插件实例创建时调用"""
        self.name = "MyAwesomePlugin"
        self.is_enabled = False
        print(f"插件 {self.name} 已构造")
    
    def on_initialize(self):
        """当插件初始化时调用"""
        print(f"插件 {self.name} 已初始化")
        self.is_enabled = True
        # 在这里初始化你的插件
    
    def on_shutdown(self):
        """当插件关闭时调用"""
        print(f"插件 {self.name} 正在关闭")
        self.is_enabled = False
        # 在这里进行清理
```

#### 插件生命周期

1. **构造**: 插件类通过 `PluginClass()` 实例化
2. **初始化**: 调用 `on_initialize()`
3. **运行**: 插件运行并处理事件/逻辑
4. **关闭**: 卸载时调用 `on_shutdown()`

#### 内置 API

插件系统提供对各种内置函数和对象的访问：

```python
# 日志
logger.info("消息")
logger.warning("警告")
logger.error("错误")

# 游戏工具
game.get_players()
game.current_round
# ... 更多 API（见 Builtins.md）
```

### 依赖管理

#### 硬依赖

如果你的插件需要其他插件才能正常运行，请在 `depend` 中列出：

```yaml
depend:
  - BasePlugin
  - UtilityLibrary
```

没有这些插件，你的插件将 **无法加载**。

#### 软依赖

对于增强功能但不必需的可选插件：

```yaml
soft-depend:
  - EnhancementPlugin
  - OptionalExtension
```

即使缺少这些插件，你的插件仍会加载。请优雅地处理缺失情况：

```python
def on_initialize(self):
    # 检查可选插件是否可用
    if self.is_plugin_loaded("EnhancementPlugin"):
        self.use_enhancement_plugin()
    else:
        print("增强插件不可用，使用基础模式")
```

#### 加载顺序

控制你的插件相对于其他插件何时加载：

```yaml
load-before:
  - LateLoadingPlugin
  - AnotherLatePlugin
```

这确保你的插件在指定的插件之前初始化。

### 开发工作流

#### 步骤 1: 创建插件结构

```bash
mkdir MyAwesomePlugin
cd MyAwesomePlugin
mkdir scripts resources
```

#### 步骤 2: 编写 plugin.yml

创建 `plugin.yml`：
```yaml
name: MyAwesomePlugin
version: 1.0.0
api-version: 1.0.0
main: main.MyAwesomePlugin
description: 一个很棒的插件
authors:
  - 你的名字
```

#### 步骤 3: 编写主插件

创建 `scripts/main.py`：
```python
class MyAwesomePlugin:
    def __init__(self):
        self.enabled = False
    
    def on_initialize(self):
        print("MyAwesomePlugin 已初始化！")
        self.enabled = True
    
    def on_shutdown(self):
        print("MyAwesomePlugin 正在关闭！")
        self.enabled = False
```

#### 步骤 4: 打包插件

```bash
# 压缩所有内容（排除 .git, venv 等）
zip -r MyAwesomePlugin.zip MyAwesomePlugin/

# 重命名为 .ca
mv MyAwesomePlugin.zip MyAwesomePlugin.ca
```

#### 步骤 5: 安装和测试

```bash
# 复制到 COG 插件目录
cp MyAwesomePlugin.ca "路径/Among Us/ClashOfGods_DATA/plugins/"

# 启动游戏并检查日志
```

### 错误处理

插件系统为常见问题提供错误消息：

| 错误 | 原因 | 解决方案 |
|------|------|--------|
| "plugin.yml missing" | 缺少元数据文件 | 将 `plugin.yml` 添加到插件根目录 |
| "Main must be in format 'Module.ClassName'" | 无效的 main 引用 | 修复格式，例如 `main.MyClass` |
| "Failed to load python script" | Python 语法错误 | 检查 Python 代码是否有错误 |
| "Missing dependency 'X'" | 找不到所需插件 | 安装所需插件 |
| "Plugin X is invented for a higher version" | API 版本不兼容 | 更新 COG 或降级插件 |

### 最佳实践

1. **错误处理**: 始终在 try-except 块中包装操作
2. **日志记录**: 使用 logger 进行调试和信息消息
3. **资源清理**: 始终在 `on_shutdown()` 中清理资源
4. **版本管理**: 使用语义化版本控制
5. **文档**: 包括 README 和内联代码注释
6. **测试**: 在存在和不存在依赖插件的情况下测试

## 本章结束
另请参阅：
[内置函数变量](Builtins.md)