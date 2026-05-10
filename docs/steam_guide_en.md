# Cortex-Command Steam ModManager 🛠️

Are you having trouble getting your Steam Workshop mods to work in Cortex Command? The built-in mod system can be buggy, and sometimes downloaded `.rte` mods just refuse to show up or activate correctly.

**Cortex-Command Steam ModManager** is a lightweight, open-source WPF application designed specifically to bypass the broken Workshop integration. It automatically finds the mods you've subscribed to on Steam, identifies your game folder, and securely installs them right where they belong!

## 🚀 Features
- **Auto-Detection:** Automatically locates your Steam installation folder and your Cortex Command game directory.
- **Deep Workshop Scanning:** Looks through Steam's `userdata` directory to find all downloaded `.rte.*` archives.
- **Smart Extraction:** Safely extracts the contents of the Workshop `.zip` files into a correct `.rte` folder structure inside the game directory. 
- **One-Click Enable/Disable:** View all found mods with their names and folder names, and use checkboxes to install or uninstall them seamlessly.
- **Multi-Language Support:** Available in English, Russian, Arabic, Chinese, and Japanese.
- **Manual Settings:** If the manager can't find your directories automatically, you can always set the paths manually via the settings menu.

## 📥 How to Use
1. Download the latest release from our GitHub/GitLab repository.
2. Run the `CortexCommandModManager.exe` executable.
3. The app will automatically scan for the game and mods.
4. Check the box next to a mod to install it, or uncheck to remove it from the game folder.
5. Launch Cortex Command and enjoy your mods!

## ⚙️ Settings
Click the "Gear" icon in the top right corner to open the Settings menu. Here you can:
- Manually configure the path to `Steam` and `Cortex Command`.
- Reset paths to the auto-detected defaults.
- Clear the mod manager's internal configuration file.
- Change the interface language via the Globe icon next to the gear.
