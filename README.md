# WinContextTweaker
[![GitHub downloads](https://img.shields.io/github/downloads/zWolfrost/WinContextTweaker/total)](https://github.com/zWolfrost/WinContextTweaker/releases/latest)
[![license](https://img.shields.io/github/license/zWolfrost/WinContextTweaker)](LICENSE)

Free and Open-source Right Click Context Menu Editor for Windows.
You can download it in the releases tab.

If you are using Windows 7 or 8.1, remember to install the ".NET Framework 6.0" and its [dependencies](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60#additional-deps).

## Features
- Creation and deletion of context menu items, along with their scripts.
- Adding scripts only for files, folders and/or specific file types.
- Support for registry options (Shift only, Add icon...).

## Tips
- Hover options to show a tooltip with more information.

- There are multiple command line variables. The ones that are most useful (and consistent) are:
  - "%V" – The selected item (file or folder).
  - "%W" – The working directory. Usually, this is the directory of the right-click, but not necessarily the selected item's parent folder (for example, when right-clicking shortcuts, "%V" returns the path of the actual item, while "%W" returns the shortcut's folder).

- When selecting multiple files, the specified command will run *individually* for each file selected.

- If you are making changes using the actual Registry, you can update the scripts in the program by pressing "F5".

&nbsp;
## Screenshots (as of v1.3.0)
<img src="assets/screenshot1.png" alt="Script Creation" height="400"/>

&nbsp;

<img src="assets/screenshot2.png" alt="Script Editing" height="400"/>

&nbsp;

<img src="assets/screenshot3.png" alt="Results" height="400"/>

&nbsp;
## Changelog
_Note that any version might include a number of stylistic changes, which are often not documented to avoid cluttering the changelog_

- **v1.0.0**:
<br>- First release.

- **v1.1.0**:
<br>- Added "Icon" (with icon selection) and "No Working Directory" options.
<br>- Added "Text", "Document", "Image", "Video", "Audio" and "by extension" files context menus.
<br>- Fixed crashes and bugs.

- **v1.2.0**:
<br>- Added an "Hello World" default command.
<br>- Added informative tooltips.
<br>- Changed the elements padding to be more organized.

- **v1.3.0**:
<br>- Added a "No scripts was found" text.
<br>- Added an "F5" "Update" keybinding, for debugging purposes.
<br>- Changed the way a file extension gets selected.
<br>- Fixed bug where creating or renaming a script with an invalid name would crash the program.
<br>- Fixed bug where creating a script for a file extension that had no registry entry would not work.

- **v1.4.0**:
<br>- Added a "Classify as Player" option.
<br>- Changed the command's font to a monospace one.