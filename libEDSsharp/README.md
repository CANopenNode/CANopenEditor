libEDSsharp
=============
The library making it all work.

* Read EDS/DCF/XDC file and parse contents to appropriate classes
* Dump EDS/DCF classes via ToString()
* Save EDS/DCF classes back to EDS file
* Export C and H files in CANopenNode format (V1.3 and newer)
* EDS/DCF supports modules
* EDS/DCF supports compactPDO (read only) \*¹
* EDS/DCF supports implict PDO (read only) \*¹
* EDS/DCF supports CompactSubOb (read only) \*¹
* Supports loading/saving of all EDS/DCF module information

\*¹ Read only, in this context, means the EDS/DCF is fully expanded but the compact forms is not written back, only the expanded form will be saved.
<br>
[Available exporters' list can be found here](https://github.com/CANopenNode/CANopenEditor?tab=readme-ov-file#available-formats).
