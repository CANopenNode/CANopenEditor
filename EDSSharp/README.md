EDSSharp
=============

A C# CanOpen EDS (Electronic Data Sheet) library, CLI convertor and GUI editor.

This application is designed to load/save/edit and create EDS/DCF/XDC file for CANopen and also to generate the object dictionary for CANopenNode (V1.3 and newer) to aid development of CANopenNode devices.

EDS (Electronic Data Sheet) files are text files that define CANopen devices.
DCF (Device Configuration File) files are text files that define configured CANopen devices.
XDD files are an XML version of EDS files.

EDS/DCF are fully defined in the DSP306 standard by the CANopen standards body: CiA.

The EDS editor on its own is useful without the CANopenNode specific export and, as of the 0.6-XDD-alpha version, the editor can also load/save XDD files.
The GUI also shows PDO mappings and can generate reports of multiple devices that are loaded into the software.

The core library can be used without the GUI to implement eds/xdd loading/saving and parsing etc in other projects.

Please consider this code experimental and beta quality.
It is a work in progress and is rapidly changing.

Every attempt has been made to comply with the relevant DSP306 and other standards and EDS files from multiple sources have been tested for loading/saving and as been (at times) validated for errors using EDS conformance tools.

[Available exporters' list can be found here](https://github.com/CANopenNode/CANopenEditor?tab=readme-ov-file#available-formats).
