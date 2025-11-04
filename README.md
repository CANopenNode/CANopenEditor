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


EDSSharp
--------

A C# CanOpen EDS (Electronic Data Sheet) library, CLI convertor and GUI editor.

This application is designed to load/save/edit and create EDS/DCF/XDC file for CANopen and also to generate the object dictionary for CANopenNode (CO_OD.c and CO_OD.h) to aid development of CANopenNode devices.

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

With many thanks to the following contributors for spotting my mistakes and improving the code:
   * s-fuchs
   * martinwag
   * trojanobelix
   * many others...

Library
-------

* Read EDS/DCF/XDC file and parse contents to appropriate classes
* Dump EDS/DCF classes via ToString()
* Save EDS/DCF classes back to EDS file
* Export C and H files in CANopenNode format CO_OD.c and CO_OD.h
* EDS/DCF supports modules
* EDS/DCF supports compactPDO (read only) *¹
* EDS/DCF supports implict PDO (read only) *¹
* EDS/DCF supports CompactSubOb (read only) *¹
* Supports loading/saving of all EDS/DCF module information

*¹ Read only, in this context, means the EDS/DCF is fully expanded but the compact
   forms is not written back, only the expanded form will be saved.

CLI
---
TODO

GUI
---
* Open multiple devices
* Open EDS/DCF/XDC files
* Save EDS/DCF/XDC files
* View OD Entries and explore the Object Dictionary
* Add new OD entries
* Delete exisiting OD entries
* Create new Devices
* Add default profiles
* Create profiles that can be added to any project (just save the device xml file to the profiles/ directory, only include the minimum number of objects that you want to auto insert); This will auto add to insert menu
* Edit Device and File Info sections
* Set RX/TX PDO mappings easily from dropdown lists of available objects
* Add and remove new PDO entries (communication paramaters and mapping) with a single button's push
* Save groups of EDS/XML files as a network object with ability to set concrete node IDs
* View report of all configured PDOs across the network
* View modules and module details present within EDS files
* View/edit actual object values for device configuring/DCF files
* Support for loading XDD files (CanOpen offical XML)
* Support for saving XDD files (CanOpen offical XML)
* Some module info is displayed in GUI showing available modules (eds) and configured modules (dcf) and what OD entries they reference.
  Full details such as subobj extension and fixed subobj are not currently displayed and unless there is demand probably will not ever be.

TODO
----

* Ensure and validate all XDD is loading/save correctly (Looking good so far)
* Add extra GUI fields for accessing extra XDD paramaters not in EDS
  (all common ones are done, a few special/edge cases remain)
* Look at XDC files and see if we can save config changes and allow editing and
  network setup here in the app, partial support is implemented by supporting
  DCF files


BUGS
----

If you find any, please open a bug report on github and attach any files you
have created/opened etc.

Pictures
--------

![Device info section of the GUI](Images/pic2.jpg)
![OD section of the GUI](Images/pic1.jpg)
![TX PDO Mapping section of the GUI](Images/pic3.jpg)
![A specific PDO view in the GUI](Images/pic4.jpg)
