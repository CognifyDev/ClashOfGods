# 第一个插件

### 概要
本章节将会教你如何使用计算机语言Lua创建属于你的插件。当然，我们默认您已经有足够的Lua语言基础。

### 如何像插件一样
首先，你应该让你的插件被插件系统识别。
<br>
要成为一个插件，您必须使您的插件目录如下：
<br>
~~~
.
├── ExamplePlugin
│   └── main.lua
│   └── plugin.yml
~~~
<br>

#### 关于 *plugin.yml*
一个 plugin.yml 必须包含下列项：
~~~yaml
name: PluginName
# 插件的名字

author: Author
# 插件的作者

version: 1.0
# 插件的版本

main: main.lua
# 插件的启动文件
~~~
<span style='font-size:10px;'>**注意： 所有项均不可省略。**</span>

#### 关于 *main.lua*
一个插件必须包含2个函数，分别名为onEnable和onDisable。
下面是一个例子：
~~~lua
function onEnable()
    --- 代码
end 

function onDisable()
    --- 代码
end
~~~
**onEnable** 将会在插件启动的时候调用<br>
**onDisable** 将会在插件被卸载的时候调用