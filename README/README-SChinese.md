# ClashOfGods
**一个划时代的Among Us模组，带有插件系统，使用简单的编程语言Python来制作你专属的模组**

> [English](README-English.md) | 简体中文

### 概述

Clash Of Gods 是一个功能强大的 Among Us 模组，为游戏引入了创新的玩法特性、自定义职业和可扩展的插件系统。基于 BepInEx 框架构建，为玩家和开发者提供了强大的自定义和扩展平台。

### 声明

该模组不隶属于 Among Us 或 Innersloth LLC，其中包含的内容未经 Innersloth LLC 认可或以其他方式赞助。此处包含的部分材料是 Innersloth LLC 的财产。

### 主要特性

- **自定义职业系统** - 为游戏添加具有特殊能力的独特职业
- **高级游戏选项** - 广泛的游戏规则和设置自定义选项
- **监听器/事件系统** - 使用自定义监听器挂接游戏事件
- **Python 插件系统** - 使用简单的 Python 编程扩展模组

### 关于模组

Clash Of Gods 在原版游戏的基础上添加了许多有趣的自定义职业和功能，提供全新的游戏体验并促进了围绕模组开发的创意社区发展。

### 插件系统

我们为开发者构建了一个强大的基于 Python 的插件系统。该系统已从原始的 JavaScript 实现演进为现在使用 **Python (IronPython)** 进行插件开发，提供了更高的灵活性和易用性。

#### 插件架构

- **插件管理器**: 管理插件的生命周期（加载、初始化、关闭）
- **插件处理器**: 执行 Python 脚本并管理插件实例
- **依赖系统**: 支持插件之间的硬依赖和软依赖
- **包装格式**: `.ca` 文件（包含 plugin.yml 元数据的 ZIP 压缩包）

#### 核心组件

| 组件 | 用途 |
|------|------|
| `PythonPluginManager` | 管理插件的加载、初始化和生命周期 |
| `PythonPluginHandler` | 使用 IronPython 引擎执行 Python 脚本 |
| `PluginDescription` | 从 `plugin.yml` 解析和存储插件元数据 |
| `IPluginManager` | 插件管理操作的接口 |
| `IPluginHandler` | 插件生命周期回调的接口 |

#### 插件生命周期

1. **加载阶段** - 插件文件被提取，元数据被解析
2. **初始化阶段** - Python 主脚本被加载，插件实例被创建
3. **运行阶段** - 插件执行 `on_initialize()` 回调并保持活跃
4. **关闭阶段** - 插件在卸载时执行 `on_shutdown()` 回调

### 插件开发快速入门

#### 插件目录结构

使用以下目录结构创建你的插件：

```
MyAwesomePlugin/
├── scripts/
│   ├── main.py
│   ├── utils.py
│   └── modules/
│       └── mymodule.zip
├── resources/
│   ├── icon.png
│   └── ...
├── plugin.yml
└── ...
```

#### plugin.yml 配置

```yaml
name: MyAwesomePlugin
version: 1.0.0
api-version: 1.0.0
main: main.MyAwesomePlugin
description: 一个很棒的 COG 插件
authors:
  - 你的名字
  - 其他贡献者
website: https://github.com/yourname/myawe soplugin

# 依赖
depend:
  - SomeRequiredPlugin
soft-depend:
  - OptionalPlugin

# 加载顺序
load-before:
  - LateLoadingPlugin

# 插件日志前缀
prefix: MAP
```

**必需字段：**
- `name` - 插件标识符
- `version` - 插件版本（语义化版本）
- `api-version` - 此插件构建的 COG API 版本
- `main` - 入口点，格式为 `module.ClassName`

**可选字段：**
- `description` - 插件描述
- `authors` - 作者列表
- `website` - 插件网站/仓库 URL
- `depend` - 硬依赖（缺失则插件加载失败）
- `soft-depend` - 软依赖（缺失时插件仍会加载）
- `load-before` - 应在此插件之后加载的插件
- `prefix` - 插件日志的自定义前缀

#### 基础 Python 插件示例

```python
class MyAwesomePlugin:
    def __init__(self):
        self.enabled = False
    
    def on_initialize(self):
        """当插件初始化时调用"""
        print("MyAwesomePlugin 已初始化！")
        self.enabled = True
    
    def on_shutdown(self):
        """当插件卸载时调用"""
        print("MyAwesomePlugin 正在关闭！")
        self.enabled = False
```

#### 插件安装

1. 按上述方式创建插件目录结构
2. 将整个插件目录压缩为 ZIP 文件
3. 将 `.zip` 文件的后缀名改为 `.ca` (CArabiner 格式)
4. 将 `.ca` 文件放在 `ClashOfGods_DATA/plugins/` 目录下
5. 启动游戏 - 你的插件将被自动发现并加载

> **注意：** `ClashOfGods_DATA` 目录位于你的 Among Us 安装目录中（与 `Among Us.exe` 所在的目录相同）。

### 插件 API 参考

#### 可用回调

```python
def on_initialize(self):
    """当插件初始化时调用"""
    pass

def on_shutdown(self):
    """当插件关闭时调用"""
    pass
```

#### 内置函数和对象

插件系统提供了各种内置函数用于日志记录、游戏交互等。完整的 API 文档请参考 [内置函数指南](Plugin/Builtins.md)。

### 依赖管理

COG 支持插件之间的硬依赖和软依赖：

- **硬依赖** (`depend`): 这里列出的插件必须存在，否则你的插件将无法加载
- **软依赖** (`soft-depend`): 这里列出的插件不必存在，但你的插件应该能够优雅地处理它们的缺失
- **先于加载** (`load-before`): 指定哪些插件应在你的插件之后加载

插件系统在加载前会根据依赖关系自动排序插件。

### 开发进度

该模组仍在积极开发中。如果你精通 Among Us 模组开发或 Python 开发，我们欢迎你的贡献！

### 资源

- **插件开发指南**: [第一个插件指南](Plugin/FirstPlugin.md)
- **内置 API 参考**: [内置函数](Plugin/Builtins.md)
- **插件系统概述**: [关于插件系统](Plugin/About.md)

### 社区

加入我们的社区讨论、分享和协作：
- **QQ 群**: 607761127
- **Discord**: [加入我们的 Discord 服务器](https://discord.gg/uWZGh4Chde)