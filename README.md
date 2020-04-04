# MediaInfo.NET

MediaInfo.NET is a modern MediaInfo GUI for Windows.

![](Main.png)

## Features

- High DPI support
- Search and highlight feature
- Tab bar showing each track in a dedicated tab
- Move to the next and previous file in the folder
- Raw view to show parameters as used in the MediaInfo API
- Customizable light and dark color theme
- Many settings to change the appearance
- Special presentation for encoding settings
- Compact summary
- Folder View powered by [PowerShell Out-GridView](https://github.com/stax76/Get-MediaInfo#examples)
- Update check and PowerShell based update feature

## Installation

**[Requires .NET Core 3.1 32bit.](https://dotnet.microsoft.com/download/dotnet-core/current/runtime)**

Run MediaInfo.NET and right-click to show the context menu, choose Install to register file associations, File Explorer will then show a MediaInfo menu item in the context menu when a media file is right-clicked.

For real power users there is [Open with++](https://github.com/stax76/OpenWithPlusPlus) for Windows File Explorer integration.

## Usage

Open media files with the context menu in File Explorer or via drag & drop or from the app context menu.

The theme colors are not hard coded but defined in Settings.xml.

Developers can enable raw view to show parameter names as they are used in the MediaInfo API.

MediaInfo.NET registers itself in the registry at:

`HKCU\Software\Microsoft\Windows\CurrentVersion\App Paths\`

This enables third party apps to find and start MediaInfo.NET.

## Related Projects

**MediaInfo** is the library which MediaInfo.NET is based on.  
https://mediaarea.net/en/MediaInfo

**Get-MediaInfo** is a PowerShell advanced function to access MediaInfo properties from media files via PowerShell.  
https://github.com/stax76/Get-MediaInfo

**StaxRip** is a encoding GUI that integrates MediaInfo.NET.  
https://github.com/staxrip/staxrip

**mpv** and **mpv.net** are media players that allow to integrate MediaInfo.NET.  
https://mpv.io/
https://github.com/stax76/mpv.net

**Open with++** is a shell extension that can integrate MediaInfo.NET into File Explorer.  
https://github.com/stax76/OpenWithPlusPlus