# First Plugin

### Preamble
This one will teach you how to create your first plugin by using the computer language Lua.
Of course, we think you have learned about Lua a lot.

### How to be like a plugin
Firstly, you should make you plugin organised by the plugin system.
<br>
To be a plugin, you must make your plugin directory like this:
<br>
~~~
.
├── ExamplePlugin
│   └── main.lua
│   └── plugin.yml
~~~
<br>

#### About *plugin.yml*
A plugin.yml must include these projects:
~~~yaml
name: PluginName
# The name of the plugin

author: Author
# The author of the plugin

version: 1.0
# The version of the plugin

main: main.lua
# The name of the start file of the plugin
~~~
<span style='font-size:10px;'>**WARNING: NO ONE CAN BE OMITTED.**</span>

#### About *main.lua*
A plugin must have 2 functions which are named onEnable and onDisable.
Here is an example.
~~~lua
function onEnable()
    --- codes here
end 

function onDisable()
    --- codes here
end
~~~
**onEnable** will be executed when the plugin starts to be LOADED.<br>
**onDisable** will be executed when the plugin starts to be UNLOADED.