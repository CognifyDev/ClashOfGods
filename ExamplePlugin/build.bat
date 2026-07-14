@echo off
echo Building ExamplePlugin...
if exist "ExamplePlugin.zip" del "ExamplePlugin.zip"
powershell -Command "Compress-Archive -Path 'metainfo.json','Scripts','Languages','Resources' -DestinationPath 'ExamplePlugin.zip' -Force"
echo Done. Copy ExamplePlugin.zip to AmongUs/ClashOfGods_DATA/Resources/
