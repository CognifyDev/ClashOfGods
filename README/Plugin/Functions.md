# Functions

### Preamble
We prepared some functions for developers to help you develop.

### Function List
| Function Name    | Arguments                   | Description                                                                    |
| ---------------- | --------------------------- |--------------------------------------------------------------------------------|
| logInfo(info)    | message(string)             | to log a message by the INFO method                                            |
| logError         | message(string)             | to log a message by the ERROR method                                           |
| logWarning       | message(string)             | to log a message by the WARNING method                                         |
| logDebug         | message(string)             | to log a message by the DEBUG method                                           |
| getAuthor        | pluginName(string)          | to get the author of a plugin                                                  |
| getVersion       | pluginName(string)          | to get the version of a plugin                                                 |
| getMainClass     | pluginName(string)          | to get the main class path of a plugin                                         |
| readFileAsBytes  | path(string)                | to get a file's bytes by passing the path                                      |
| writeFileByBytes | path(string), bytes(string) | to write a file of texts by passing bytes(form like this: "0,2,4,6,72,44,122") |