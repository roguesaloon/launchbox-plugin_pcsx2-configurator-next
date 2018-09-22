# PCSX2 Configurator Next for LaunchBox
PCSX2 Configuartor Next is a plugin for LaunchBox that allows users to easily configure PCSX2 on a per-game basis. The plugin also allows for pre-optimised configs to be downloaded from right within LaunchBox for a whole host of popular PS2 games, from [Zombeaver's excellent PS2 Configuration Project](https://github.com/Zombeaver/PCSX2-Configs).
# Installation
The plugin is easy to install and simply needs to be downloaded and extracted. The archive contains a folder called "PCSX2 Configurator Next", this should be placed in the root of the "Plugins" in the user's LaunchBox directory. Any previous versions of the plugin should be removed before attempting to install this version.
# General Usage
The plugin is relatively straight forward to use, but has been changed significantly from previous versions. You will first need to have PCSX2 setup as an emulator in LaunchBox. Then right click on a PS2 Game and choose "PCSX2 Configurator" from near the bottom of the context menu. This will present a menu of options:

**"Create Config"** - Creates a new config for the game in question based on the users current PCSX2 settings, If a config already exists this will overwrite that config with a new one.

**"Download Config"** - This will download one of Zombeaver's Config and set it up to be ready to play for the selected game, this will be greyed out if now config is available, and will change to "Update Config" if a new update is available for the downloaded config.

**"Remove Config"** - This will remove the current config for the selected game, and will be greyed out if there is no current config.

**"Configure With PCSX2"** - This will open PCSX2 with the config for the selected game loaded, allowing you to customise your config for the selected game (Downloaded Config's can also be tweaked). Once a game has a created config this can also be done using LaunchBox's built in "Configure" from the games context menu. Both these options will be greyed out if there is no config for the selected game.

There is also a "settings.ini" file created in the plugins folder ("%LaunchBoxDir%/Plugins/PCSX2 Configurator Next"), this can be tweaked manually but most settings don't need to be touched in most cases. Some people may wish to change the "GameConfigsDir" which is where the created configs are stored, this deafults to the users PCSX2 "inis" directory. There is currently no inteface to modify these settings, but that is planned in a future version.
# Version Info and The Next in The Name
This version of the plugin is the successor to the original PCSX2 Configurator plugin, with a completely re-worked codebase from that version, and full compatibility with LaunchBox.Next (which is now the current version of LaunchBox). The main reason "Next" was added to the name is to initially seperate it from that (now discontinued) verison of the plugin. This release is currently in Beta and the "Next" from the name will probably be dropped eventually for a stable release with more features.
# Credit and Support
I have personally put a lot of time and effort into all versions of this plugin, with it essentially being my free-time pet project for the last year and a half. Though I know the plugin would not be what it is today without Zombeaver who really helped me to bring this plugin to the next level, by allow me to integrate his PS2 configurations into it as well as creating artwork and helping me test the plugin. I would also like to say a big thank you to spectral, neil9000, and kmoney for helping me out with testing, and of course Jason Carr for making LaunchBox and implementing some of my requested changes into the plugin engine. I intend to keep working on this plugin, and fix any reported bugs as well as update the plugin with new features. So if you have any problem or feature request's hop over to [the plugins thread on the forums]() and let me know.

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=5ZLK8P6TYQTTC)
