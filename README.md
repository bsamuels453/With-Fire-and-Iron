Airship combat simulator I'm working on in my spare time. Most of the code is based on one of my older sim projects, Forge.

-------------

There's a lot of components in this project, so here's a quick overview of what's been implemented.

-OpenCL terrain generation

-OpenCL terrain quantization (quadtree)

-Airship hull modeling using bezier curves/surfaces

-Partially completed object/airship internals editor

-Airship firing/cannonball collision mechanics.

-User interface defined by external template files

-------------

File Guide:

Forge.Framework - Framework built on top of, and extends XNA. Introduces a handful of utility classes along with not-crap sprite, buffer, and UI classes. Also implements an input handling system that can handle externally loaded keybindings, and handle issues such as a class wanting to obtain exclusive access to an input device.

Forge.Core - Contains core gameplay code. Airship logic, terrain generation, airship editor components - they're all in here.

Forge.Content - Contains all of the game's asset files. These aren't on github.

Scripts - Contains all game scripts. For now this is only opencl scripts.

Config - Configuration files for tidbits such as shaders, keybindings, and terrain generator settings.

Data - Used to contain non-config data such as airship definition files and save files.

UiTemplates - Contains configuration files for user interface objects. This will probably be moved into Config in the future.

MonoGameUtility - Contains monogame data classes. This is just a placeholder until we can implement the custom monogame build.

Tools - Contains test projects, debug projects, and any tools related to the main project.

-------------

For now, this code is licenced under the Mozilla Public Licence.
http://www.mozilla.org/MPL/2.0/

[![githalytics.com alpha](https://cruel-carlota.pagodabox.com/0ac64015708f0a4b47a68145827c6fae "githalytics.com")](http://githalytics.com/bsamuels453/Gondola)