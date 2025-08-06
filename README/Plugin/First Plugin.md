# 第一个插件

## 概要
本章节将会介绍如何使用计算机语言JavaScript创建属于你的插件。当然，我们默认您已经有足够的JavaScript语言基础。

## 如何像插件一样
首先，您应该让您的插件能够被插件系统识别。

要成为一个插件，您必须使您的插件目录如下：

~~~
.
├── ExamplePlugin
│   ├── resources
│       ├── example.png
│       └── ...
│   ├── scripts
│       ├── main.js
│       ├── modules
│           ├── myModule.zip
│       └── ...
│   └── plugin.yml
~~~

随后，将 `ExamplePlugin`（实际为您的插件目录）下的所有文件及文件夹以 `.zip` 文件的形式进行压缩，随后将压缩包文件的后缀名修改为 `.cog` ，放置于 `\ClashOfGods_DATA\plugins\` 目录下。

> [!NOTE]
> `ClashOfGods_DATA` 目录位于您的 Among Us 游戏根目录，即放有 Among Us 游戏可执行文件 `Among Us.exe` 的目录。

这样，启动游戏后，您的插件就能够被识别并加载。


### 关于 *plugin.yml*
一个 plugin.yml 必须包含下列项：
~~~yaml
name: PluginName
# 插件的名字

authors: [ Author1 ]
# 插件的作者（可以包含多位作者，使用半角逗号分隔）
# 如 [ "Author1", "Author2" ]

version: 1.0.0
# 插件的版本

main: main.js
# 插件的启动文件（插件系统将在 scripts 文件夹下查找该文件）

modules: [ 'modules\myModule.zip|MyModule' ]
# 插件使用的外部模块（可以包含多项，同样可以使用半角逗号分隔，必须为 .zip 文件，插件系统将在 scripts 文件夹下查找该文件）
# 每一个模块信息包含两部分：路径与名称，两部分需使用竖杠分隔
~~~

注意：若插件系统识别时遇到问题，请自行搜寻学习 Yaml 的基础语法（如字符串）后再检查是否对特殊符号进行处理。

> [!CAUTION]
> 除了项 `modules` `authors` ，其它项为必需。

### 关于 *main.js*
一个插件必须包含3个函数，分别为 `onLoad`，`onEnable` 与 `onDisable`。
下面是一个例子：
~~~js
function onLoad() {
    logger.info("1")
}

function onEnable() {
    logger.info("2")
}

function onDisable() {
    logger.info("3")
}
~~~
`onLoad` 将会在插件加载的时候调用；

`onEnable` 将会在插件启动的时候调用，比 `onLoad` 稍晚；

`onDisable` 将会在插件被卸载的时候调用。

若您正确安装了包含上述代码的插件，打开 `\BepInEx\LogOutput.log` 后，您将会发现输出顺序为 `"1"` `"2"`，但不一定会出现 `"3"`。要使插件输出 `"3"` ，请在游戏主界面单击设置 > COG选项 > 点击“临时禁用模组”，即可发现输出了 `"3"` 。

## 本章结束
另请参阅：
[内置函数变量](Builtins.md)