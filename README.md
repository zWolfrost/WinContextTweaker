# WinContextTweaker
Free and Open-source Right Click Context Menu Editor for Windows.
You can download it in the releases tab.

If you are using Windows 7 or 8.1, remember to install the ".NET Framework 6.0" and its [dependencies](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60#additional-deps).

## Features
- Background, folders and file types context menu editing.
- Creation, renaming and deletion of context menu items, along with their scripts.
- Support for registry options (Shift only, Icon...).

## Tips
- Hover options to show a tooltip with more information.

- There are multiple command line variables. The ones that are most useful (and consistent) are:
  - "%V" – The selected item (file or folder).
  - "%W" – The working directory. Usually, this is the directory of the right-click, but not necessarily the selected item's parent folder (for example, when right-clicking shortcuts, "%V" returns the path of the actual item, while "%W" returns the shortcut's folder).


&nbsp;

<img src="assets/1.png" alt="Script Creation" width="600"/>

&nbsp;

<img src="assets/2.png" alt="Script Editing" width="600"/>

&nbsp;

<img src="assets/3.png" alt="Results" width="300"/>

&nbsp;
## Changelog
- **v1.0.0**:
<br>- First release.

- **v1.1.0**:
<br>- Added "Icon" (with icon selection) and "No Working Directory" options.
<br>- Added "Text", "Document", "Image", "Video", "Audio" and "by extension" files context menus.
<br>- Fixed crashes and bugs.

- **v1.2.0**:
<br>- Added an "Hello World" default command.
<br>- Added Tooltips on hover.
<br>- Made the elements padding more organized.
