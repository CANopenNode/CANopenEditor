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

Repository structure
--------
This repository is home to three projects:
- [LibEDSsharp](https://github.com/CANopenNode/CANopenEditor/blob/docs/libEDSsharp/README.md), a C# library for EDS files manipulation which went upstream and is now maintained in this repository.
- [A CLI](https://github.com/CANopenNode/CANopenEditor/blob/docs/EDSSharp/README.md), used for simple conversions across all supported formats.
- [A GUI](https://github.com/CANopenNode/CANopenEditor/blob/docs/EDSEditorGUI/README.md) for full manipulation of your CANopen files [which is being rewritten to be more multi platform](https://github.com/CANopenNode/CANopenEditor/blob/docs/EDSEditorGUI2/README.md).

How to use
--------
1. [Download the latest release's binary zip file](https://github.com/CANopenNode/CANopenEditor/releases). DO NOT DOWNLOAD SOURCE CODE.
2. Unzip it.
3. Go to net8.0-windows directory.
4. Execute the .exe .

Available formats
--------
Exhaustive list of the library's supported formats to date, sorted by category:<br>

### CAN in Automation official formats:
| Description                           | Exporter                                                   | Format |
|---------------------------------------|------------------------------------------------------------|--------|
| Electronic Data Sheet (CiA 306-1)     | ElectronicDataSheet                                        | .eds   |
| Device Configuration File (CiA 306-1) | DeviceConfigurationFile                                    | .dcf   |
| XML Device Description (CiA 311)      | CanOpenXDDv1.0<br>CanOpenXDDv1.1<br>CanOpenXDDv1.1stripped | .xdd   |
| XML Device Configuration (CiA 311)    | CanOpenXDCv1.1                                             | .xdc   |

### Extended formats:
| Description                      | Exporter                                    | Format |
|----------------------------------|---------------------------------------------|--------|
| Network XML Device Description   | CanOpenNetworkv1.0<br>CanOpenNetworkXDDv1.1 | .nxdd  |
| Network XML Device Configuration | CanOpenNetworkXDCv1.1                       | .nxdc  |
| XML Profile Description          | None                                        | .xpd   |

### CANopenNode specific formats:
| Description                              | Exporter                                                 | Format          |
|------------------------------------------|----------------------------------------------------------|-----------------|
| CanOpenNode Object Dictionary file pairs | CanOpenNode<br>CanOpenNodeV4                             | .h,.c           |
| PCanOpenNode Project file                | CanOpenNodeProtobuf(json)<br>CanOpenNodeProtobuf(binary) | .json<br>.binpb |

### Documentation formats:
| Exporter            | Format |
|---------------------|--------|
| DocumentationHTML   | .html  |
| DocumentationMarkup | .md    |
| NetworkPDOReport    | .md    |

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
- [libEDSsharp](https://github.com/CANopenNode/CANopenEditor/tree/main/libEDSsharp) directory contains the library from Robin Cornelius making all of this work.

BUGS
--------
If you find any, please open a bug report on github and attach any files you have created/opened etc... We need any help we can have and the main maintainers are quite active and will answer you fast.

You might want to check your EDS/XDD file with this free [EDSchecker](https://www.vector.com/de/de/support-downloads/download-center/#product=%5B%2274771%22%5D&tab=1&pageSize=15&sort=date&order=desc)

Contributing
--------
If you want to help us out by contributing to this project, first of all thank you ! And please read our [Contributing Guidelines](https://github.com/CANopenNode/CANopenEditor/blob/docs/CONTRIBUTING.md). We are very beginner friendly so, even if you are not extremely experienced with contributing to open source projects, fear not and try !

Collaborators
--------
<!-- readme: collaborators -start -->
<table>
	<tbody>
		<tr>
            <td align="center">
                <a href="https://github.com/reza0310">
                    <img src="https://avatars.githubusercontent.com/u/70545529?v=4" width="100;" alt="reza0310"/>
                    <br />
                    <sub><b>reza0310</b></sub>
                </a>
            </td>
		</tr>
	<tbody>
</table>
<!-- readme: collaborators -end -->

Contributors
--------
<!-- readme: contributors -start -->
<table>
	<tbody>
		<tr>
            <td align="center">
                <a href="https://github.com/robincornelius">
                    <img src="https://avatars.githubusercontent.com/u/159000?v=4" width="100;" alt="robincornelius"/>
                    <br />
                    <sub><b>robincornelius</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/trojanobelix">
                    <img src="https://avatars.githubusercontent.com/u/15106425?v=4" width="100;" alt="trojanobelix"/>
                    <br />
                    <sub><b>trojanobelix</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/nimrof">
                    <img src="https://avatars.githubusercontent.com/u/9848846?v=4" width="100;" alt="nimrof"/>
                    <br />
                    <sub><b>nimrof</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/CANopenNode">
                    <img src="https://avatars.githubusercontent.com/u/13575344?v=4" width="100;" alt="CANopenNode"/>
                    <br />
                    <sub><b>CANopenNode</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/martinwag">
                    <img src="https://avatars.githubusercontent.com/u/676672?v=4" width="100;" alt="martinwag"/>
                    <br />
                    <sub><b>martinwag</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/simon-fuchs-inmach">
                    <img src="https://avatars.githubusercontent.com/u/57712038?v=4" width="100;" alt="simon-fuchs-inmach"/>
                    <br />
                    <sub><b>simon-fuchs-inmach</b></sub>
                </a>
            </td>
		</tr>
		<tr>
            <td align="center">
                <a href="https://github.com/heliochronix">
                    <img src="https://avatars.githubusercontent.com/u/1733202?v=4" width="100;" alt="heliochronix"/>
                    <br />
                    <sub><b>heliochronix</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/Bartimaeus-">
                    <img src="https://avatars.githubusercontent.com/u/2954254?v=4" width="100;" alt="Bartimaeus-"/>
                    <br />
                    <sub><b>Bartimaeus-</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/cfr-mir">
                    <img src="https://avatars.githubusercontent.com/u/44053860?v=4" width="100;" alt="cfr-mir"/>
                    <br />
                    <sub><b>cfr-mir</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/JuPrgn">
                    <img src="https://avatars.githubusercontent.com/u/20264907?v=4" width="100;" alt="JuPrgn"/>
                    <br />
                    <sub><b>JuPrgn</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/gotocoffee1">
                    <img src="https://avatars.githubusercontent.com/u/26260677?v=4" width="100;" alt="gotocoffee1"/>
                    <br />
                    <sub><b>gotocoffee1</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/wilkinsw">
                    <img src="https://avatars.githubusercontent.com/u/10655771?v=4" width="100;" alt="wilkinsw"/>
                    <br />
                    <sub><b>wilkinsw</b></sub>
                </a>
            </td>
		</tr>
		<tr>
            <td align="center">
                <a href="https://github.com/pettaa123">
                    <img src="https://avatars.githubusercontent.com/u/31046837?v=4" width="100;" alt="pettaa123"/>
                    <br />
                    <sub><b>pettaa123</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/henrikbrixandersen">
                    <img src="https://avatars.githubusercontent.com/u/1076226?v=4" width="100;" alt="henrikbrixandersen"/>
                    <br />
                    <sub><b>henrikbrixandersen</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/ckhardin">
                    <img src="https://avatars.githubusercontent.com/u/1160137?v=4" width="100;" alt="ckhardin"/>
                    <br />
                    <sub><b>ckhardin</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/Regelink">
                    <img src="https://avatars.githubusercontent.com/u/1665817?v=4" width="100;" alt="Regelink"/>
                    <br />
                    <sub><b>Regelink</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/DylanRobertBennett">
                    <img src="https://avatars.githubusercontent.com/u/87715493?v=4" width="100;" alt="DylanRobertBennett"/>
                    <br />
                    <sub><b>DylanRobertBennett</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/rgruening">
                    <img src="https://avatars.githubusercontent.com/u/72022918?v=4" width="100;" alt="rgruening"/>
                    <br />
                    <sub><b>rgruening</b></sub>
                </a>
            </td>
		</tr>
		<tr>
            <td align="center">
                <a href="https://github.com/Barzello">
                    <img src="https://avatars.githubusercontent.com/u/52344726?v=4" width="100;" alt="Barzello"/>
                    <br />
                    <sub><b>Barzello</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/rcolatobe">
                    <img src="https://avatars.githubusercontent.com/u/86854948?v=4" width="100;" alt="rcolatobe"/>
                    <br />
                    <sub><b>rcolatobe</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/kekiefer">
                    <img src="https://avatars.githubusercontent.com/u/48104?v=4" width="100;" alt="kekiefer"/>
                    <br />
                    <sub><b>kekiefer</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/zhanglongqi">
                    <img src="https://avatars.githubusercontent.com/u/956693?v=4" width="100;" alt="zhanglongqi"/>
                    <br />
                    <sub><b>zhanglongqi</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/DaMutz">
                    <img src="https://avatars.githubusercontent.com/u/406081?v=4" width="100;" alt="DaMutz"/>
                    <br />
                    <sub><b>DaMutz</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/StormOli">
                    <img src="https://avatars.githubusercontent.com/u/4819887?v=4" width="100;" alt="StormOli"/>
                    <br />
                    <sub><b>StormOli</b></sub>
                </a>
            </td>
		</tr>
		<tr>
            <td align="center">
                <a href="https://github.com/possibly-not">
                    <img src="https://avatars.githubusercontent.com/u/12588174?v=4" width="100;" alt="possibly-not"/>
                    <br />
                    <sub><b>possibly-not</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/reza0310">
                    <img src="https://avatars.githubusercontent.com/u/70545529?v=4" width="100;" alt="reza0310"/>
                    <br />
                    <sub><b>reza0310</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/KwonTae-young">
                    <img src="https://avatars.githubusercontent.com/u/10510127?v=4" width="100;" alt="KwonTae-young"/>
                    <br />
                    <sub><b>KwonTae-young</b></sub>
                </a>
            </td>
		</tr>
	<tbody>
</table>
<!-- readme: contributors -end -->
