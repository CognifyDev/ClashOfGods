# 内置函数变量

## 概要
我们为开发者准备了一些函数与变量，以帮助您进行开发。

## 变量列表

### `logger`
类型： `PluginLogger`

#### 公开成员

##### 函数：
|名称|参数|返回|作用|
|---|---|---|---|
|`LogDebug`|`msg`|无|在控制台输出调试信息|
|`LogInfo`|`msg`|无|在控制台输出普通信息|
|`LogWarning`|`msg`|无|在控制台输出警告|
|`LogFatal`|`msg`|无|在控制台输出严重错误|
|`LogError`|`msg`|无|在控制台输出错误|
|`LogMessage`|`msg`|无|在控制台输出消息|

##### 变量
无

## 函数列表

## `getResources(path)`

返回值类型：数组

### 参数
- `path`: 一个指向目标资源的相对路径的字符串。

### 说明
该函数会返回一个代表着指定位置的资源的字节数组，需要通过其它方式转换为您所需的类型（如字符串等）。
> [!NOTE]
> 参数 `path` 指向的文件基于您的插件根目录，而非 `resources` 目录。
> 如 `resources\logo.png` `myFolder\myResource.json` 。 

