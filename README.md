CANopenEditor
=============
CANopenEditor is a fork from [libedssharp, authored by Robin Cornelius](https://github.com/robincornelius/libedssharp).
CANopenEditor's homepage is https://github.com/CANopenNode/CANopenEditor.

CANopen Object Dictionary Editor:
 - Imports: CANopen electronic data sheets in EDS or XDD format.
 - Exports: CANopen electronic data sheets in EDS or XDD format, documentation, CANopenNode C source files and more.
 - Interfaces: GUI editor for CANopen Object Dictionary, Device information, etc. CLI client for simple conversions.

CANopen is the internationally standardized (EN 50325-4) ([CiA301](http://can-cia.org/standardization/technical-documents)) higher-layer protocol for embedded control system built on top of CAN. For more information on CANopen see http://www.can-cia.org/ .

[CANopenNode](https://github.com/CANopenNode/CANopenNode) is a free and open source CANopen Stack.

File structure
--------
The main files and directories you'll need to understand are:
- [setup.nsi](https://github.com/CANopenNode/CANopenEditor/blob/main/setup.nsi) is the Windows installer.
- [Makefile](https://github.com/CANopenNode/CANopenEditor/blob/main/Makefile) is the Linux installation and manipulation script.
- [EDSEditorGUI](https://github.com/CANopenNode/CANopenEditor/tree/main/EDSEditorGUI) directory is the old GUI. Fully functional but only works on Windows.
- [EDSEditorGUI2](https://github.com/CANopenNode/CANopenEditor/tree/main/EDSEditorGUI2) directory is the new GUI. It is not fully finished yet but is meant to work on any Windows, Mac or Linux OS.
- [EDSSharp](https://github.com/CANopenNode/CANopenEditor/tree/main/EDSSharp) directory is the CLI. It is only meant for simple conversions for now.
- [GUITests](https://github.com/CANopenNode/CANopenEditor/tree/main/GUITests) directory is the directory for all GUI unit tests. More tests, functional tests and tests for GUI2 may come here.
- [Images](https://github.com/CANopenNode/CANopenEditor/tree/main/Images) directory is the directory containing any and all of the documentation's images.
- [Tests](https://github.com/CANopenNode/CANopenEditor/tree/main/Tests) directory is the directory for all Lib unit tests. More tests, functional tests and tests for CLI may come here.
- [libEDSsharp](https://github.com/CANopenNode/CANopenEditor/tree/main/libEDSsharp) directory contains the libe from Robin Cornelius making all of this work.

BUGS
--------
If you find any, please open a bug report on github and attach any files you have created/opened etc... We need any help we can have and the main maintainers are quite active and will answer you fast.

Contributing
--------
If you want to help us out by contributing to this project, first of all thank you ! And please read our [Contributing Guidelines](https://github.com/CANopenNode/CANopenEditor/blob/docs/CONTRIBUTING.md). We are very beginner friendly so, even if you are not extremely experienced with contributing to open source projects, fear not and try !
