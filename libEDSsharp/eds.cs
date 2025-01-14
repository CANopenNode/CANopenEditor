/*
    This file is part of libEDSsharp.

    libEDSsharp is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    libEDSsharp is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with libEDSsharp.  If not, see <http://www.gnu.org/licenses/>.
 
    Copyright(c) 2016 - 2019 Robin Cornelius <robin.cornelius@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using CanOpenXSD_1_1;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace libEDSsharp
{

    /// <summary>
    /// Object dictionary data types from CiA 301
    /// </summary>
    public enum DataType
    {
        UNKNOWN = 0,
        BOOLEAN = 1,
        INTEGER8 = 2,
        INTEGER16 = 3,
        INTEGER32 = 4,
        UNSIGNED8 = 5,
        UNSIGNED16 = 6,
        UNSIGNED32 = 7,
        REAL32 = 8,
        VISIBLE_STRING = 9,
        OCTET_STRING = 0x0A,
        UNICODE_STRING = 0x0B,
        TIME_OF_DAY = 0x0C,
        TIME_DIFFERENCE = 0x0D,
        DOMAIN = 0x0F,
        INTEGER24 = 0x10,
        REAL64 = 0x11,
        INTEGER40 = 0x12,
        INTEGER48 = 0x13,
        INTEGER56 = 0x14,
        INTEGER64 = 0x15,
        UNSIGNED24 = 0x16,
        UNSIGNED40 = 0x18,
        UNSIGNED48 = 0x19,
        UNSIGNED56 = 0x1A,
        UNSIGNED64 = 0x1B,

        PDO_COMMUNICATION_PARAMETER = 0x20,  //PDO_CommPar
        PDO_MAPPING  = 0x21, //PDO_Mapping
        SDO_PARAMETER = 0x22,
        IDENTITY = 0x23,

    }
    /// <summary>
    /// Object Dictionary object definitions from CiA 301
    /// </summary>
    public enum ObjectType
    {
        UNKNOWN = -1,
        /// <summary>
        /// An object with no data fields
        /// </summary>
        NULL = 0,
        /// <summary>
        /// Large variable amount of data e.g. executable program code
        /// </summary>
        DOMAIN =2,
        /// <summary>
        /// Denotes a type definition such as a BOOLEAN, UNSIGNED16, FLOAT and so on
        /// </summary>
        DEFTYPE=5,
        /// <summary>
        /// Defines a new record type e.g. the PDO mapping structure at 21h
        /// </summary>
        DEFSTRUCT=6,
        /// <summary>
        /// A single value such as an UNSIGNED8, BOOLEAN, FLOAT, INTEGER16, VISIBLE STRING etc.
        /// </summary>
        VAR = 7,
        /// <summary>
        /// A multiple data field object where each data field is a 
        /// simple variable of the SAME basic data type e.g. array of UNSIGNED16 etc.
        /// Sub-index 0 is of UNSIGNED8 and therefore not part of the ARRAY data
        /// </summary>
        ARRAY = 8,
        /// <summary>
        /// A multiple data field object where the data fields may be any combination of
        /// simple variables. Sub-index 0 is of UNSIGNED8 and sub-index 255 is of UNSIGNED32 and
        /// therefore not part of the RECORD data
        /// </summary>
        RECORD = 9,
    }

    public enum PDOMappingType
    {
        no=0,
        optional=1,
        RPDO=2,
        TPDO=3,
        @default=4,
    }

    /// <summary>
    /// Defines how the object can be changed from SDO
    /// </summary>
    public enum AccessSDO
    {
        /// <summary>
        /// no access
        /// </summary>
        no,
        /// <summary>
        /// read only access
        /// </summary>
        ro,
        /// <summary>
        /// write only access
        /// </summary>
        wo,
        /// <summary>
        /// read and write access
        /// </summary>
        rw
    }

    /// <summary>
    /// Defines how the object can be changed from PDO
    /// </summary>
    public enum AccessPDO
    {
        /// <summary>
        /// no access
        /// </summary>
        no,
        /// <summary>
        /// TPDO access
        /// </summary>
        t,
        /// <summary>
        /// RPDO access
        /// </summary>
        r,
        /// <summary>
        /// TPDO and RPDO access
        /// </summary>
        tr
    }

    public enum AccessSRDO
    {
        no = 0,
        tx = 1,
        rx = 2,
        trx = 3
    }

    /// <summary>
    /// Custom properties for OD entry or sub-entry, which are saved into xdd file v1.1
    /// </summary>
    public class CustomProperties
    {
        /// <summary>
        /// If true, object is completelly skipped by CANopenNode exporters, etc.
        /// </summary>
        public bool CO_disabled = false;
        public string CO_countLabel = "";
        /// <summary>
        /// CanOpenNode storage group
        /// </summary>
        public string CO_storageGroup = "RAM";
        public bool CO_flagsPDO = false;
        public AccessSRDO CO_accessSRDO = AccessSRDO.no;
        /// <summary>
        /// Minimum length of a string that can be stored   
        /// </summary>
        public UInt32 CO_stringLengthMin = 0;

        /// <summary>
        /// Deep clone
        /// </summary>
        /// <returns>a deep clone</returns>
        public CustomProperties Clone()
        {
            return new CustomProperties
            {
                CO_disabled = CO_disabled,
                CO_countLabel = CO_countLabel,
                CO_storageGroup = CO_storageGroup,
                CO_flagsPDO = CO_flagsPDO,
                CO_accessSRDO = CO_accessSRDO,
                CO_stringLengthMin = CO_stringLengthMin
            };
        }
        /// <summary>
        /// Convert from XSD to EDS
        /// </summary>
        /// <param name="properties">raw custom properties from XSD</param>
        public void OdeXdd(property[] properties)
        {
            if (properties != null)
            {
                foreach (property prop in properties)
                {
                    switch (prop.name)
                    {
                        case "CO_disabled": CO_disabled = prop.value == "true"; break;
                        case "CO_countLabel": CO_countLabel = prop.value ?? ""; break;
                        case "CO_storageGroup": CO_storageGroup = prop.value ?? ""; break;
                        case "CO_flagsPDO": CO_flagsPDO = prop.value == "true"; break;
                        case "CO_accessSRDO":
                            try { CO_accessSRDO = (AccessSRDO)Enum.Parse(typeof(AccessSRDO), prop.value); }
                            catch (Exception) { CO_accessSRDO = AccessSRDO.no; }
                            break;
                        case "CO_stringLengthMin":
                            try { CO_stringLengthMin = Convert.ToUInt16(prop.value); }
                            catch (Exception) { CO_stringLengthMin = 0; }
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Convert custom properties from EDS to XSD
        /// </summary>
        /// <returns>XSD properties ready to use</returns>
        public property[] OdeXdd()
        {
            var props = new List<property>();

            if (CO_disabled)
                props.Add(new property { name = "CO_disabled", value = "true" });
            if (CO_countLabel != "")
                props.Add(new property { name = "CO_countLabel", value = CO_countLabel });
            if (CO_storageGroup != "RAM")
                props.Add(new property { name = "CO_storageGroup", value = CO_storageGroup });
            if (CO_flagsPDO)
                props.Add(new property { name = "CO_flagsPDO", value = "true" });

            return props.ToArray();
        }

        public property[] SubOdeXdd()
        {
            var props = new List<property>();

            if (CO_accessSRDO != AccessSRDO.no)
                props.Add(new property { name = "CO_accessSRDO", value = CO_accessSRDO.ToString() });
            if (CO_stringLengthMin != 0)
                props.Add(new property { name = "CO_stringLengthMin", value = CO_stringLengthMin.ToString() });

            return props.ToArray();
        }
    }

    /// <summary>
    /// List of multiple CO_storageGroup strings available in project
    /// </summary>
    public class CO_storageGroups : List<string>
    {
        public CO_storageGroups()
        {
            Add("RAM"); // default value
        }

        public new void Add(string item)
        {
            if (!Contains(item))
            {
                base.Add(item);
            }
        }
    }
    /// <summary>
    /// Indicate that it should be exported in EDS files and may have some data about how
    /// </summary>
    public class EdsExport : Attribute
    {
        /// <summary>
        /// Max length of the string when exported
        /// </summary>
        public UInt16 maxlength;
        public bool commentonly=false;

        /// <summary>
        /// default constructor
        /// </summary>
        public EdsExport()
        {
        }
        /// <summary>
        /// contstructor with max string length
        /// </summary>
        /// <param name="maxlength">max length of the string when exported</param>
        public EdsExport(UInt16 maxlength)
        {
            this.maxlength = maxlength;
        }

        public bool IsReadOnly()
        {
            return commentonly;
        }

      
    }
    /// <summary>
    /// Indicate that it should be exported in DCF files
    /// </summary>
    public class DcfExport : EdsExport
    {
    }

    /// <summary>
    /// Section of info in EDS or DCF file
    /// </summary>
    public partial class InfoSection
    {
        protected Dictionary<string, string> section;

        protected string infoheader;
        protected string edssection;

        public enum Filetype
        {
            File_EDS,
            File_DCF
        }

        public bool GetField(string name, string varname)
        {
            FieldInfo f = null;

            try 
            {
                foreach (var element in section)
                {
                    if (String.Equals(element.Key, name, StringComparison.OrdinalIgnoreCase))
                    {

                        name = element.Key;
                        Type tx = this.GetType();

                        f = tx.GetField(varname);
                        object var = null;

                        switch (f.FieldType.Name)
                        {
                            case "String":
                                var = section[name];
                                break;

                            case "UInt32":
                                var = Convert.ToUInt32(section[name], EDSsharp.Getbase(section[name]));
                                break;

                            case "Int16":
                                var = Convert.ToInt16(section[name], EDSsharp.Getbase(section[name]));
                                break;

                            case "UInt16":
                                var = Convert.ToUInt16(section[name], EDSsharp.Getbase(section[name]));
                                break;

                            case "Byte":
                                var = Convert.ToByte(section[name], EDSsharp.Getbase(section[name]));
                                break;

                            case "Boolean":
                                var = section[name] == "1"; //because Convert is Awesome
                                break;

                            default:
                                Console.WriteLine(String.Format("Unhanded variable {0} for {1}", f.FieldType.Name, varname));
                                break;
                        }

                        if (var != null)
                        {
                            tx.GetField(varname).SetValue(this, var);
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e is OverflowException)
                {
                    Warnings.warning_list.Add(string.Format("Warning parsing {0} tried to fit {1} into {2}", name, section[name], f.FieldType.Name));
                }
            }

            return false;
        }
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            string msg;

            msg = $"*****************************************************{Environment.NewLine}";
            msg += $"*{String.Format("{0," + ((51 + infoheader.Length) / 2).ToString() + "}", infoheader),-51}*{Environment.NewLine}";
            msg += $"*****************************************************{Environment.NewLine}";

            Type tx = this.GetType();
            FieldInfo[] fields = this.GetType().GetFields();

            foreach (FieldInfo f in fields)
            {
                msg += $"{f.Name,-28}: {f.GetValue(this).ToString()}{Environment.NewLine}";
            }

            return msg;
        }
    }

 
    public partial class MandatoryObjects : SupportedObjects
    {
        public MandatoryObjects()
            : base()
         {
              infoheader = "Mandatory Objects";
              edssection = "MandatoryObjects";
         }
    }

    public partial class OptionalObjects : SupportedObjects
    {
        public OptionalObjects()
            : base()
        {
            infoheader = "Optional Objects";
            edssection = "OptionalObjects";
        }
    }

    public partial class ManufacturerObjects : SupportedObjects
    {
        public ManufacturerObjects() : base()
        {
            infoheader = "Manufacturer Objects";
            edssection = "ManufacturerObjects";
        }
    }

    public partial class TypeDefinitions : SupportedObjects
    {   
        public TypeDefinitions() : base()
        {
            infoheader = "Type Definitions";
            edssection = "TypeDefinitions";
        }
    }

    public partial class SupportedObjects
    {

        public Dictionary<int, int> objectlist;
        public string infoheader;
        public string edssection = "Supported Objects";
        public string countmsg = "SupportedObjects";

        public SupportedObjects()
        {
            objectlist = new Dictionary<int, int>();
        }
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            string msg;

            msg = $"*****************************************************{Environment.NewLine}";
            msg += $"*{String.Format("{0," + ((51 + infoheader.Length) / 2).ToString() + "}", infoheader),-51}*{Environment.NewLine}";
            msg += $"*****************************************************{Environment.NewLine}";
            msg += $"}}{Environment.NewLine}{countmsg} = {objectlist.Count}{Environment.NewLine}";
            foreach(KeyValuePair<int,int> kvp in objectlist)
            {
                msg += $"{kvp.Key,-5}: {kvp.Value:x4}{Environment.NewLine}";
            }

            return msg;

        }
    }

    public partial class Comments
    {

        public List<string> comments = new List<string>();
        public string infoheader = "Comments";
        public string edssection = "Comments";

        public Comments()
        {
           
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            string msg;

            msg = $"*****************************************************{Environment.NewLine}";
            msg += $"*{String.Format("{0," + ((51 + infoheader.Length) / 2).ToString() + "}", infoheader),-51}*{Environment.NewLine}";
            msg += $"*****************************************************{Environment.NewLine}";
            msg += $"{Environment.NewLine}Lines = {comments.Count}{Environment.NewLine}";
            foreach (string s in comments)
            {
                msg += $"{s}{Environment.NewLine}";
            }

            return msg;

        }
    }


    public partial class Dummyusage : InfoSection
    {
        [EdsExport]
        public bool Dummy0001;
        [EdsExport]
        public bool Dummy0002;
        [EdsExport]
        public bool Dummy0003;
        [EdsExport]
        public bool Dummy0004;
        [EdsExport]
        public bool Dummy0005;
        [EdsExport]
        public bool Dummy0006;
        [EdsExport]
        public bool Dummy0007;

 
        public Dummyusage()
        {
             infoheader = "CAN OPEN Dummy Usage";
             edssection = "DummyUsage";
        }
    }

    /// <summary>
    /// FileInfo section as described in CiA 306
    /// </summary>
    public partial class FileInfo : InfoSection
    {
        // Only for internal usage, use Path.GetFileName(eds.projectFilename) instead.
        /// <summary>
        /// indicate the file name (according to OS restrictions)
        /// </summary>
        [EdsExport]
        public string FileName="";
        /// <summary>
        /// indicate the actual file version (Unsigned8)
        /// </summary>
        [EdsExport]
        public string FileVersion="";
        /// <summary>
        /// indicate the actual file revision (Unsigned8)
        /// </summary>
        [EdsExport]
        public byte FileRevision;//=1

        [DcfExport]
        public string LastEDS = "";

        public byte EDSVersionMajor;//=4.0
        
        public byte EDSVersionMinor;//=4.0
        /// <summary>
        /// indicate the version of the specification (3 characters) in the format x.y
        /// </summary>
        [EdsExport]
        public string EDSVersion="";
        /// <summary>
        /// file description (max 243 characters)
        /// </summary>
        [EdsExport(maxlength=243)]
        public string Description="";//= //max 243 characters
        public DateTime CreationDateTime;//
        /// <summary>
        /// file creation time (characters in format hh:mm(AM|PM)),
        /// </summary>
        [EdsExport]
        public string CreationTime="";
        /// <summary>
        /// provide the date of file creation (characters in format mm-dd-yyyy)
        /// </summary>
        [EdsExport]
        public string CreationDate="";
        /// <summary>
        /// name or a description of the file creator (max. 245 characters)
        /// </summary>
        [EdsExport(maxlength = 245)]
        public string CreatedBy = "";//=CANFestival //max245
        
        public DateTime ModificationDateTime;//

        /// <summary>
        /// time of last modification (characters in format hh:mm(AM|PM))
        /// </summary>
        [EdsExport]
        public string ModificationTime="";
        /// <summary>
        ///  date of the last file modification (characters in format mm-dd-yyyy)
        /// </summary>
        [EdsExport]
        public string ModificationDate="";
        /// <summary>
        /// name or a description of the creator (max. 244 characters)
        /// </summary>
        [EdsExport(maxlength = 244)]
        public string ModifiedBy="";//=CANFestival //max244

        //Folder CO_OD.c and CO_OD.h will be exported into
        public string exportFolder = "";

        public FileInfo()
        {
            infoheader = "CAN OPEN FileInfo";
            edssection = "FileInfo";
        }
    }

    /// <summary>
    /// DeviceInfo section as described in CiA 306
    /// </summary>
    public partial class DeviceInfo : InfoSection
    {
        /// <summary>
        /// vendor name (max. 244 characters)
        /// </summary>
        [EdsExport]
        public string VendorName="";
        /// <summary>
        /// unique vendor ID according to identity object sub-index 01h (Unsigned32) 
        /// </summary>
        [EdsExport]
        public string VendorNumber="";
        /// <summary>
        /// product name (max. 243 characters)
        /// </summary>
        [EdsExport]
        public string ProductName="";
        /// <summary>
        /// product code according to identity object sub-index 02h (Unsigned32)
        /// </summary>
        [EdsExport]
        public string ProductNumber="";
        /// <summary>
        /// product revision number according to identity object sub-index 03h (Unsigned32) 
        /// </summary>
        [EdsExport]
        public UInt32 RevisionNumber;

        /// <summary>
        /// indicate the supported baud rates (Boolean, 0 = not supported, 1=supported)
        /// </summary>
        [EdsExport]
        public bool BaudRate_10 = false;
        /// <summary>
        /// indicate the supported baud rates (Boolean, 0 = not supported, 1=supported)
        /// </summary>
        [EdsExport]
        public bool BaudRate_20 = false;
        /// <summary>
        /// indicate the supported baud rates (Boolean, 0 = not supported, 1=supported)
        /// </summary>
        [EdsExport]
        public bool BaudRate_50 = false;
        /// <summary>
        /// indicate the supported baud rates (Boolean, 0 = not supported, 1=supported)
        /// </summary>
        [EdsExport]
        public bool BaudRate_125 = false;
        /// <summary>
        /// indicate the supported baud rates (Boolean, 0 = not supported, 1=supported)
        /// </summary>
        [EdsExport]
        public bool BaudRate_250 = false;
        /// <summary>
        /// indicate the supported baud rates (Boolean, 0 = not supported, 1=supported)
        /// </summary>
        [EdsExport]
        public bool BaudRate_500 = false;
        /// <summary>
        /// indicate the supported baud rates (Boolean, 0 = not supported, 1=supported)
        /// </summary>
        [EdsExport]
        public bool BaudRate_800 = false;
        /// <summary>
        /// indicate the supported baud rates (Boolean, 0 = not supported, 1=supported)
        /// </summary>
        [EdsExport]
        public bool BaudRate_1000 = false;

        public bool BaudRate_auto = false;

        /// <summary>
        /// indicate the simple boot-up master functionality (Boolean, 0 = not supported, 1 = supported), 
        /// </summary>
        [EdsExport]
        public bool SimpleBootUpMaster;
        /// <summary>
        /// indicate the simple boot-up slave functionality (Boolean, 0 = not supported, 1 = supported),
        /// </summary>
        [EdsExport]
        public bool SimpleBootUpSlave;
        /// <summary>
        /// granularity allowed for the mapping on this device - 
        /// most of the existing devices support a granularity of 8 (Unsigned8; 0 - mapping not modifiable, 1-64 granularity) 
        /// </summary>
        [EdsExport]
        public byte Granularity = 8;
        /// <summary>
        /// Indicate the facility of dynamic variable generation. If the value is unequal to 0, the additional section DynamicChannels exists (CiA302 and CiA405) 
        /// </summary>
        [EdsExport]
        public bool DynamicChannelsSupported;

        [EdsExport]
        public byte CompactPDO;
        /// <summary>
        /// indicate the facility of multiplexed PDOs. (Boolean, 0 = not supported, 1 = supported)
        /// </summary>
        [EdsExport]
        public bool GroupMessaging;
        /// <summary>
        /// indicate the number of supported receive PDOs. (Unsigned16)
        /// </summary>
        [EdsExport]
        public UInt16 NrOfRXPDO;
        /// <summary>
        /// indicate the number of supported transmit PDOs. (Unsigned16) 
        /// </summary>
        [EdsExport]
        public UInt16 NrOfTXPDO;
        /// <summary>
        /// indicate if LSS functionality is supported (Boolean, 0 = not supported, 1 = supported) 
        /// </summary>
        [EdsExport]
        public bool LSS_Supported;

        public bool LSS_Master;

        [EdsExport]
        public bool NG_Slave;

        public bool NG_Master;

        public UInt16 NrOfNG_MonitoredNodes;

        public DeviceInfo()
        {
            infoheader = "CAN OPEN DeviceInfo";
            edssection = "DeviceInfo";
        }
    }


    public partial class DeviceCommissioning : InfoSection
    {

        public DeviceCommissioning()
        {
            infoheader = "CAN OPEN DeviceCommissioning";
            edssection = "DeviceComissioning";  
        }

        [DcfExport]
        public byte NodeID = 0;

        [DcfExport(maxlength = 246)]
        public string NodeName = ""; //Max 246 characters

        [DcfExport]
        public UInt16 Baudrate;

        [DcfExport]
        public UInt32 NetNumber;

        [DcfExport(maxlength = 243)]
        public string NetworkName = ""; //Max 243 characters

        [DcfExport]
        public bool CANopenManager;  //1 = CANopen manager, 0 or missing = not the manager

        [DcfExport]
        public UInt32 LSS_SerialNumber;

    }

    public partial class SupportedModules : InfoSection
    {
        [EdsExport]
        public UInt16 NrOfEntries;

        public SupportedModules()
        {
            infoheader = "CAN OPEN Supported Modules";
            edssection = "SupportedModules";
        }
    }

    public partial class ConnectedModules : SupportedObjects
    {
        [EdsExport]
        public UInt16 NrOfEntries
        {
            get {  return (UInt16)connectedmodulelist.Count;  }
        }

        public Dictionary<int, int> connectedmodulelist;

        public ConnectedModules()
        {
            infoheader = "CAN OPEN Connected Modules";
            edssection = "ConnectedModules";
            countmsg = "NrOfEntries";
            connectedmodulelist = new Dictionary<int, int>();
        }
    }

    public partial class MxFixedObjects : SupportedObjects
    {
        [EdsExport]
        public UInt16 NrOfEntries
        {
            get { return (UInt16)connectedmodulelist.Count; }
        }

        public Dictionary<int, int> connectedmodulelist;

        private int _moduleindex;

        public int Moduleindex
        {
            get { return _moduleindex; }
            set { _moduleindex = value; edssection = String.Format("M{0}FixedObjects",value); }
        }

        public MxFixedObjects(UInt16 modindex)
        {
            infoheader = "CAN OPEN Module Fixed Objects";
            this.Moduleindex = modindex;
            countmsg = "NrOfEntries";
            connectedmodulelist = new Dictionary<int, int>();
        }
    }

    public partial class ModuleInfo : InfoSection
    {
        [EdsExport(maxlength = 248)]
        public string ProductName;

        [EdsExport]
        public byte ProductVersion;

        [EdsExport]
        public byte ProductRevision;

        [EdsExport]
        public string OrderCode;

        UInt16 moduleindex = 0;

        public ModuleInfo(UInt16 moduleindex)
        {
            this.moduleindex = moduleindex;
            infoheader = "CAN OPEN Module Info " + moduleindex.ToString();
            edssection = string.Format("M{0}{1}", moduleindex, "ModuleInfo");
        }
    }

    public partial class ModuleComments : Comments
    {

        UInt16 moduleindex;

        public ModuleComments(UInt16 moduleindex)
        {
            this.moduleindex = moduleindex;
            infoheader = "CAN OPEN Module Comments " + moduleindex.ToString();
            edssection = string.Format("M{0}{1}", moduleindex, "Comments");
        }
    }

    public partial class ModuleSubExtends : SupportedObjects
    {

        UInt16 moduleindex;

        public ModuleSubExtends(UInt16 moduleindex)
              : base()
        {
            this.moduleindex = moduleindex;
            infoheader = "CAN OPEN ModuleSubExtends "+moduleindex.ToString();
            edssection = string.Format("M{0}{1}", moduleindex, "SubExtends");
        }
    }
    /// <summary>
    /// Represent object dictionary index and subindex objects
    /// </summary>
    public partial class ODentry
    {
        private UInt16 _index;

        /// <summary>
        /// The index of the object in the Object Dictionary
        /// This cannot be set for child objects, if you read a child object you get the parents index
        /// </summary>
        [EdsExport]
        public UInt16 Index
        {
            get
            {
                if (parent != null)
                    return parent.Index;
                else
                    return _index;
            }
            set
            {
                if(value==0)
                {

                    //throw (new Exception("Object index must be set"));
                }

                if(parent == null)
                {
                    _index = value;
                }
                else
                {

                    //throw (new Exception("Typing to set index of a subobject"));
                }
               
            }
        }

        [EdsExport]
        public string parameter_name = "";

        [DcfExport]
        public string denotation = "";

        /// <summary>
        /// object type var,rec, array etc.
        /// </summary>
        [EdsExport]
        public ObjectType objecttype = ObjectType.UNKNOWN;
        /// <summary>
        /// data type bool, integer etc.
        /// </summary>
        [EdsExport]
        public DataType datatype = DataType.UNKNOWN;
        /// <summary>
        /// access type
        /// </summary>
        [EdsExport]
        public EDSsharp.AccessType accesstype = EDSsharp.AccessType.UNKNOWN;
        /// <summary>
        /// default value
        /// </summary>
        [EdsExport]
        public string defaultvalue = "";
        /// <summary>
        /// low numeric limit
        /// </summary>
        [EdsExport]
        public string LowLimit = "";
        /// <summary>
        /// high numeric limit
        /// </summary>
        [EdsExport]
        public string HighLimit = "";
        /// <summary>
        /// actual value
        /// </summary>
        [DcfExport]
        public string actualvalue = "";

        [EdsExport]
        public UInt32 ObjFlags = 0;

        [EdsExport]
        public byte CompactSubObj = 0;
        /// <summary>
        /// true if it is PDO mapping object
        /// </summary>
        [EdsExport]
        public bool PDOMapping
        {
            get
            {
                return PDOtype != PDOMappingType.no;
            }
        }

        //FIXME Count "If several modules are gathered to form a new Sub-Index,
        //then the number is 0, followed by semicolon and the
        //number of bits that are created per module to build a new
        //Sub-Index"

        [EdsExport]
        public byte count = 0;

        [EdsExport]
        public byte ObjExtend = 0;

        public PDOMappingType PDOtype = PDOMappingType.no;

        public string Label = "";
        public string Description = "";
        public SortedDictionary<UInt16, ODentry> subobjects = new SortedDictionary<UInt16, ODentry>();
        public ODentry parent = null;

        public CustomProperties prop = new CustomProperties();

        //XDD Extensions//
        public string uniqueID;

        /// <summary>
        /// Used when writing out objects to know if we are writing the normal or the module parts out
        /// Two module parts subext and fixed are available.
        /// </summary>
        public enum Odtype
        {
            NORMAL,
            SUBEXT,
            FIXED,
        }

        /// <summary>
        /// Empty object constructor
        /// </summary>
        public ODentry()
        {

        }

        /// <summary>
        /// ODentry constructor for a simple VAR type
        /// </summary>
        /// <param name="parameter_name">Name of Object Dictionary Entry</param>
        /// <param name="index">Index of object in object dictionary</param>
        /// <param name="datatype">Type of this objects data</param>
        /// <param name="defaultvalue">Default value (always set as a string)</param>
        /// <param name="accesstype">Allowed CANopen access permissions</param>
        /// <param name="PDOMapping">Allowed PDO mapping options</param>
        public ODentry(string parameter_name,UInt16 index, DataType datatype, string defaultvalue, EDSsharp.AccessType accesstype, PDOMappingType PDOMapping)
        {
            this.parameter_name = parameter_name;
            this.Index = index;
            this.objecttype = ObjectType.VAR;
            this.datatype = datatype;
            this.defaultvalue = defaultvalue;


            if (accesstype >= EDSsharp.AccessType_Min && accesstype <= EDSsharp.AccessType_Max)
                this.accesstype = accesstype;
            else
                throw new ParameterException("AccessType invalid");

            this.PDOtype = PDOMapping;

        }

         /// <summary>
         /// ODConstructor useful for subobjects
         /// </summary>
         /// <param name="parameter_name"></param>
         /// <param name="index">NOT USED</param>
         /// <param name="datatype"></param>
         /// <param name="defaultvalue"></param>
         /// <param name="accesstype"></param>
         /// <param name="PDOMapping"></param>
         /// <param name="parent"></param>
        public ODentry(string parameter_name, UInt16 index,  DataType datatype, string defaultvalue, EDSsharp.AccessType accesstype, PDOMappingType PDOMapping, ODentry parent)
        {
            this.parent = parent;
            this.parameter_name = parameter_name;
            this.objecttype = ObjectType.VAR;
            this.datatype = datatype;
            this.defaultvalue = defaultvalue;
            this.Index = index;

            if (accesstype >= EDSsharp.AccessType_Min && accesstype <= EDSsharp.AccessType_Max)
                this.accesstype = accesstype;
            else
                throw new ParameterException("AccessType invalid");

            this.PDOtype = PDOMapping;
        }
        

        /// <summary>
        /// ODEntry constructor for array subobjects
        /// </summary>
        /// <param name="parameter_name"></param>
        /// <param name="index"></param>
        /// <param name="nosubindex"></param>
        public ODentry(string parameter_name,UInt16 index, byte nosubindex)
        {
            this.parameter_name = parameter_name;
            this.objecttype = ObjectType.ARRAY;
            this.Index = index;
            //this.nosubindexes = nosubindex;
            this.objecttype = ObjectType.VAR;     
        }

        /// <summary>
        /// Make a deep clone of this ODentry
        /// </summary>
        /// <returns></returns>
        public ODentry Clone(ODentry newParent = null)
        {
            ODentry newOd = new ODentry
            {
                parent = newParent,
                parameter_name = parameter_name,
                denotation = denotation,
                objecttype = objecttype,
                datatype = datatype,
                accesstype = accesstype,
                PDOtype = PDOtype,
                defaultvalue = defaultvalue,
                LowLimit = LowLimit,
                HighLimit = HighLimit,
                actualvalue = actualvalue,
                Label = Label,
                Description = Description,
                subobjects = new SortedDictionary<UInt16, ODentry>(),
                prop = prop.Clone()
            };

            foreach (KeyValuePair<UInt16, ODentry> kvp in subobjects)
                newOd.subobjects.Add(kvp.Key, kvp.Value.Clone(newOd));

            return newOd;
        }

        /// <summary>
        /// Provide a simple string representation of the object, only parameters index, no subindexes/subindex parameter name and data type are included
        /// Useful for debug and also appears in debugger when you inspect this object
        /// </summary>
        /// <returns>string summary of object</returns>
        public override string ToString()
        {
            if (subobjects.Count > 0)
            {
                return String.Format("{0:x4}[{1}] : {2} : {3}", Index, subobjects.Count, parameter_name, datatype);
 
            }
            else
            {
                return String.Format("{0:x4}/{1} : {2} : {3}", Index, Subindex, parameter_name, datatype);
            }
        }

        /// <summary>
        /// Provide a simple string representation of the object type. Returns the string of the ENUM ObjectType.VAR if objecttype is not enumed  
        /// </summary>
        /// <returns>string representation of object type </returns>
        public string ObjectTypeString()
        {
                return Enum.IsDefined(typeof(ObjectType), objecttype) ? objecttype.ToString() : ObjectType.VAR.ToString();
        }

        public void ObjectTypeString(string objectType)
        {
            this.objecttype = Enum.IsDefined(typeof(ObjectType), objecttype) ? objecttype : ObjectType.VAR;
        }

        public AccessSDO AccessSDO()
        {
            EDSsharp.AccessType accType = accesstype;
            if (accType == EDSsharp.AccessType.UNKNOWN && parent != null && parent.objecttype == ObjectType.ARRAY)
                accType = parent.accesstype;

            switch (accType)
            {
                default:
                    return libEDSsharp.AccessSDO.no;
                case EDSsharp.AccessType.ro:
                case EDSsharp.AccessType.@const:
                    return libEDSsharp.AccessSDO.ro;
                case EDSsharp.AccessType.wo:
                    return libEDSsharp.AccessSDO.wo;
                case EDSsharp.AccessType.rw:
                case EDSsharp.AccessType.rwr:
                case EDSsharp.AccessType.rww:
                    return libEDSsharp.AccessSDO.rw;
            }
        }

        public void AccessSDO(AccessSDO accessSDO, AccessPDO accessPDO)
        {
            switch (accessSDO)
            {
                default:
                    accesstype = EDSsharp.AccessType.UNKNOWN;
                    break;
                case libEDSsharp.AccessSDO.ro:
                    accesstype = EDSsharp.AccessType.ro;
                    break;
                case libEDSsharp.AccessSDO.wo:
                    accesstype = EDSsharp.AccessType.wo;
                    break;
                case libEDSsharp.AccessSDO.rw:
                    if (accessPDO == libEDSsharp.AccessPDO.r)
                        accesstype = EDSsharp.AccessType.rww;
                    else if (accessPDO == libEDSsharp.AccessPDO.t)
                        accesstype = EDSsharp.AccessType.rwr;
                    else
                        accesstype = EDSsharp.AccessType.rw;
                    break;
            }
        }

        public AccessPDO AccessPDO()
        {
            EDSsharp.AccessType accType = accesstype;
            if (accType == EDSsharp.AccessType.UNKNOWN && parent != null && parent.objecttype == ObjectType.ARRAY)
                accType = parent.accesstype;

            if (PDOtype == PDOMappingType.RPDO || accType == EDSsharp.AccessType.rww)
                return libEDSsharp.AccessPDO.r;
            else if (PDOtype == PDOMappingType.TPDO || accType == EDSsharp.AccessType.rwr)
                return libEDSsharp.AccessPDO.t;
            if (PDOtype == PDOMappingType.optional || PDOtype == PDOMappingType.@default)
                return libEDSsharp.AccessPDO.tr;
            else
                return libEDSsharp.AccessPDO.no;
        }

        public void AccessPDO(AccessPDO accessPDO)
        {
            switch (accessPDO)
            {
                default:
                    PDOtype = PDOMappingType.no;
                    break;
                case libEDSsharp.AccessPDO.r:
                    PDOtype = PDOMappingType.RPDO;
                    break;
                case libEDSsharp.AccessPDO.t:
                    PDOtype = PDOMappingType.TPDO;
                    break;
                case libEDSsharp.AccessPDO.tr:
                    PDOtype = PDOMappingType.optional;
                    break;
            }
        }

        /// <summary>
        /// Duplicate current sub entry and add it to parent
        /// </summary>
        /// <returns>true on successfull addition</returns>
        public ODentry AddSubEntry()
        {
            ODentry baseObject = parent == null ? this : parent;

            if (baseObject.objecttype == ObjectType.VAR)
                return null;
            
            ODentry newOd;

            if ((baseObject.Nosubindexes == 0) && ((baseObject.objecttype == ObjectType.ARRAY) || (baseObject.objecttype == ObjectType.RECORD))) {
                baseObject.subobjects.Add(0, new ODentry
                {
                    parent = baseObject,
                    parameter_name = "Highest sub-index supported",
                    accesstype = EDSsharp.AccessType.ro,
                    objecttype = ObjectType.VAR,
                    datatype = DataType.UNSIGNED8,
                    defaultvalue = "0x01"
                });
            }

            ODentry lastSubOd = baseObject.subobjects.Values.Last();
            ODentry originalOd = null;
            UInt16 maxSubIndex = 1;
            UInt16 lastSubIndex = 1;

            // create new or clone existing sub od
            if (lastSubOd == null || lastSubOd.Subindex < 1)
            {
                newOd = new ODentry
                {
                    parent = baseObject,
                    parameter_name = "item",
                    objecttype = ObjectType.VAR,
                    datatype = DataType.UNSIGNED32
                };
            }
            else
            {
                originalOd = (parent != null && this.Subindex > 0) ? this : lastSubOd;
                newOd = originalOd.Clone(originalOd.parent);
                maxSubIndex = EDSsharp.ConvertToUInt16(baseObject.subobjects[0].defaultvalue);
                lastSubIndex = lastSubOd.Subindex;
            }

            // insert new sub od
            SortedDictionary<UInt16, ODentry> newSubObjects = new SortedDictionary<ushort, ODentry>();
            UInt16 newSubIndex = 0;
            foreach (ODentry subOd in baseObject.subobjects.Values)
            {
                if (subOd.Subindex > newSubIndex)
                    newSubIndex = subOd.Subindex;

                newSubObjects.Add(newSubIndex++, subOd);

                if (originalOd == subOd)
                    newSubObjects.Add(newSubIndex++, newOd);
            }
            if (originalOd == null)
                newSubObjects.Add(newSubIndex++, newOd);

            baseObject.subobjects = newSubObjects;

            // Write maxSubIndex to first sub index
            if (maxSubIndex > 0 && maxSubIndex == lastSubIndex && baseObject.subobjects.Count > 0)
            {
                baseObject.subobjects[0].defaultvalue = string.Format("0x{0:X2}", newSubIndex - 1);
            }

            return newOd;
        }

        /// <summary>
        /// Remove current sub entry
        /// </summary>
        /// <param name="renumber">Renumber subentries</param>
        /// <returns>true on successfull removal</returns>
        public bool RemoveSubEntry(bool renumber)
        {
            if (parent != null && (parent.objecttype == ObjectType.ARRAY || parent.objecttype == ObjectType.RECORD))
            {
                UInt16 maxSubIndex = EDSsharp.ConvertToUInt16(parent.subobjects[0].defaultvalue);
                UInt16 lastSubIndex = parent.subobjects.Values.Last().Subindex;

                parent.subobjects.Remove(Subindex);

                if (renumber)
                {
                    SortedDictionary<UInt16, ODentry> newSubObjects = new SortedDictionary<ushort, ODentry>();
                    UInt16 subIndex = 0;
                    foreach (ODentry subOd in parent.subobjects.Values)
                        newSubObjects.Add(subIndex++, subOd);
                    parent.subobjects = newSubObjects;
                }

                // Write maxSubIndex to first sub index
                if (maxSubIndex > 0 && maxSubIndex == lastSubIndex && parent.subobjects.Count > 0)
                {
                    parent.subobjects[0].defaultvalue = string.Format("0x{0:X2}", parent.subobjects.Values.Last().Subindex);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// If data type is an octet string we must remove all spaces when writing out to a EDS/DCF file
        /// </summary>
        /// <param name="value">Value to be processed</param>
        /// <returns>value if not octet string or value with spaces removed if octet string</returns>
        public string Formatoctetstring(string value)
        {
            DataType dt = datatype;
            if (dt == DataType.UNKNOWN && this.parent != null)
                dt = parent.datatype;

            string ret = value;

            if (dt == DataType.OCTET_STRING)
            {
                ret = value.Replace(" ", "");
            }

            return ret;
        }

        /// <summary>
        /// Returns a c compatible string that represents the name of the object, - is replaced with _
        /// words separated by a space are replaced with _ for a separator eg ONE TWO becomes ONE_TWO
        /// </summary>
        /// <returns></returns>
        public string Paramater_cname()
        {
            string cname = parameter_name.Replace("-", "_");

            cname =  Regex.Replace(cname, @"([A-Z]) ([A-Z])", "$1_$2");
            cname = cname.Replace(" ", "");

            return cname;
        }

        /// <summary>
        /// Return the size in bytes for the given CANopen datatype of this object, eg the size of what ever the datatype field is set to 
        /// </summary>
        /// <returns>no of bytes</returns>
        public int Sizeofdatatype()
        {
            DataType dt = datatype;

            if (dt == DataType.UNKNOWN && this.parent != null)
                dt = parent.datatype;
 
            switch (dt)
            {
                case DataType.BOOLEAN:
                    return 1;

                case DataType.UNSIGNED8:
                case DataType.INTEGER8:
                    return 8;

                case DataType.VISIBLE_STRING:
                case DataType.OCTET_STRING:
                    return Lengthofstring*8;

                case DataType.INTEGER16:
                case DataType.UNSIGNED16:
                case DataType.UNICODE_STRING:
                    return 16; //FIXME is this corret for UNICODE_STRING seems dodgy?

                case DataType.UNSIGNED24:
                case DataType.INTEGER24:
                    return 24;

                case DataType.INTEGER32:
                case DataType.UNSIGNED32:
                case DataType.REAL32:
                    return 32;

                case DataType.INTEGER40:
                case DataType.UNSIGNED40:
                    return 40;

                case DataType.INTEGER48:
                case DataType.UNSIGNED48:
                case DataType.TIME_DIFFERENCE:
                case DataType.TIME_OF_DAY:
                    return 48;

                case DataType.INTEGER56:
                case DataType.UNSIGNED56:
                    return 56;

                case DataType.INTEGER64:
                case DataType.UNSIGNED64:
                case DataType.REAL64:
                    return 64;

                case DataType.DOMAIN:
                    return 0;

                default: //FIXME
                    return 0;

            }
        }

        
        /// <summary>
        /// This is the no of subindexes present in the object, it is NOT the maximum subobject index
        /// </summary>
        [EdsExport]
        public int Nosubindexes
        {
            get
            {
                return subobjects.Count;
            }
        }
        
        //warning eds files with gaps in subobject lists have been seen in the wild
        //this function tries to get the array index based on sub number not array number
        //it may return null
        //This needs expanding to be used globally through the application ;-(
        public ODentry Getsubobject(UInt16 no)
        {
            if (subobjects.ContainsKey(no))
                return subobjects[no];
            return null;
        }
        /// <summary>
        /// Returns default value for a subindex
        /// </summary>
        /// <param name="no">subindex to get the default value for</param>
        /// <returns>default value for that subindex or "" if the subindex was not found</returns>
        public string Getsubobjectdefaultvalue(UInt16 no)
        {
            if (subobjects.ContainsKey(no))
                return subobjects[no].defaultvalue;
            else
                return "";
        }
        /// <summary>
        /// Returns true if the object contains a subindex
        /// </summary>
        /// <param name="no">the subindex to look for</param>
        /// <returns>true if it contains the subindex</returns>
        public bool Containssubindex(UInt16 no)
        {
            if (subobjects.ContainsKey(no))
                return true;

            return false;
        }

        /// <summary>
        /// Return max indicated subindex, or null if not array or record
        /// </summary>
        /// <returns></returns>
        public byte Getmaxsubindex()
        {
            //Although subindex 0 should contain the max subindex value
            //we don't enforce that anywhere in this lib, we should have a setter function
            //that sets it to the highest subobject found.
            if (objecttype == ObjectType.ARRAY || objecttype == ObjectType.RECORD)
                if (Containssubindex(0))
                {
                    return EDSsharp.ConvertToByte(Getsubobjectdefaultvalue(0));
                }

            return 0;
        }

        public int Lengthofstring
        {
            get
            {
                string defaultvalue = this.defaultvalue;
                if (defaultvalue == null)
                    return 0;

                switch (this.datatype)
                {
                    case DataType.VISIBLE_STRING:
                        {
                            return defaultvalue.Unescape().Length;
                        }

                    case DataType.OCTET_STRING:
                        {
                            return Regex.Replace(defaultvalue, @"\s", "").Length / 2;
                        }

                    case DataType.UNICODE_STRING:
                        {
                            return Regex.Replace(defaultvalue, @"\s", "").Length / 4;
                        }
                    default:
                        {
                            return 0;
                        }
                }
            }
        }
        /// <summary>
        /// Subindex of this object if it is a subindex object, 0 if not
        /// </summary>
        public UInt16 Subindex
        { 
            get
            {
                if(this.parent!=null)
                {
                    return parent.Findsubindex(this);
                }
                return 0;

            }
        }
        /// <summary>
        /// Look for a entry in the subindexs, return the index if found
        /// </summary>
        /// <param name="od">the OD entry to look for in the subindex objects</param>
        /// <returns>the subindex if found or 0 if not found</returns>
        public UInt16 Findsubindex(ODentry od)
        {
            foreach(KeyValuePair<UInt16,ODentry>kvp in subobjects )
            {
                if (kvp.Value == od)
                    return kvp.Key;
            }

            return 0;

        }

        /// <summary>
        /// Add an existing entry as a subobject of this OD
        /// </summary>
        /// <param name="sub"></param>
        /// <param name="index"></param>
        public void addsubobject(byte index, ODentry sub)
        {
            sub.parent = this;
            this.subobjects.Add(index, sub);
        }

    }

    public class Module
    {

        public ModuleInfo mi;
        public ModuleComments mc;
        public ModuleSubExtends mse;
        public MxFixedObjects mxfo;
        public SortedDictionary<UInt16, ODentry> modulefixedobjects;
        public SortedDictionary<UInt16, ODentry> modulesubext;

        public UInt16 moduleindex;

        public Module(UInt16 moduleindex)
        {

            this.moduleindex = moduleindex;

            mi = new ModuleInfo(moduleindex);
            mc = new ModuleComments(moduleindex);
            mse = new ModuleSubExtends(moduleindex);
            mxfo = new MxFixedObjects(moduleindex);
            modulefixedobjects = new SortedDictionary<ushort, ODentry>();
            modulesubext = new SortedDictionary<ushort, ODentry>();
        }



    }

    public partial class EDSsharp : IFileExporter
    {

        public enum AccessType
        {
            rw = 0,
            ro = 1,
            wo = 2,
            rwr = 3,
            rww = 4,
            @const = 5,
            UNKNOWN
        }

        public const AccessType AccessType_Min = AccessType.rw;
        public const AccessType AccessType_Max = AccessType.@const;


        // File name of the opened project. Multiple file types are possible
        // for opened project file, but project is always saved as xdd_v1.1
        // Filename within the FileInfo structure has only limited usage.
        public string projectFilename = "";
        /// <summary>
        /// File name, when project is opened in xdd_v1.1 or project is saved
        /// </summary>
        public string xddfilename_1_1 = "";
        // File names for exported files
        public string xddfilenameStripped = "";
        public string edsfilename = "";
        public string dcffilename = "";
        public string ODfilename = "";
        public string ODfileVersion = "";
        public string mdfilename = "";
        public string xmlfilename = ""; // old format
        public string xddfilename_1_0 = ""; // old format

        //This is memorized, when XDD v1.1 is opened. It keeps all elements
        //from original XDD file, which are not handled by libedesharp, so
        //they will be preserved, when the file will be saved.
        //Object dictionary parameters are not stored here.
        public ISO15745ProfileContainer xddTemplate = null;

        //property to indicate unsaved data;
        private bool _dirty;
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                _dirty = value;
                OnDataDirty?.Invoke(_dirty, this);
            }
        }

        protected Dictionary<string, Dictionary<string, string>> eds;
        protected Dictionary<string, int> sectionlinenos;
        public SortedDictionary<UInt16, ODentry> ods;
        public SortedDictionary<UInt16, ODentry> dummy_ods;

        public CO_storageGroups CO_storageGroups = new CO_storageGroups();

        public FileInfo fi;
        public DeviceInfo di;
        public MandatoryObjects md;
        public OptionalObjects oo;
        public ManufacturerObjects mo;
        public Comments c;
        public Dummyusage du;
        public DeviceCommissioning dc;

        public TypeDefinitions td;

        public SupportedModules sm;
        public ConnectedModules cm;

        // public Dictionary<UInt16, ModuleInfo> mi;
        // public Dictionary<UInt16, ModuleComments> mc;
        // public Dictionary<UInt16, ModuleSubExtends> mse;
        // public Dictionary<ushort, MxFixedObjects> mxfo;
        // public SortedDictionary<UInt16, SortedDictionary<UInt16, ODentry>> modulefixedobjects;
        // public SortedDictionary<UInt16, SortedDictionary<UInt16, ODentry>> modulesubext;

        public Dictionary<UInt16, Module> modules;

        public UInt16 NodeID = 0;

        public delegate void DataDirty(bool dirty, EDSsharp sender);
        public event DataDirty OnDataDirty;

        /// <summary>
        /// Fetches all the different fileexporter types the class supports
        /// </summary>
        /// <returns>List of the different exporters the class supports</returns>
        public ExporterDescriptor[] GetExporters()
        {
            return new ExporterDescriptor[] {
                new ExporterDescriptor("Electronic Data Sheet", new string[] { ".eds" }, 0, delegate (string filepath, List<EDSsharp> eds)
                {
                    eds[0].Savefile(filepath, InfoSection.Filetype.File_EDS);
                }),
                new ExporterDescriptor("Device Configuration File", new string[] { ".dcf" }, 0, delegate (string filepath, List<EDSsharp> eds)
                {
                    eds[0].Savefile(filepath, InfoSection.Filetype.File_DCF);
                })
            };
        }

        public EDSsharp()
        {


            eds = new Dictionary<string, Dictionary<string, string>>();
            ods = new SortedDictionary<UInt16, ODentry>();
            dummy_ods = new SortedDictionary<UInt16, ODentry>();

            fi = new FileInfo();
            di = new DeviceInfo();
            du = new Dummyusage();
            md = new MandatoryObjects();
            oo = new OptionalObjects();
            mo = new ManufacturerObjects();
            dc = new DeviceCommissioning();
            c = new Comments();
            sm = new SupportedModules();
            cm = new ConnectedModules();
            td = new TypeDefinitions();


            //mi = new Dictionary<ushort, ModuleInfo>();
            //mc = new Dictionary<ushort, ModuleComments>();
            //mse = new Dictionary<ushort, ModuleSubExtends>();
            //mxfo = new Dictionary <ushort, MxFixedObjects>();
            //modulefixedobjects = new SortedDictionary<ushort, SortedDictionary<ushort, ODentry>>();
            //modulesubext = new SortedDictionary<ushort, SortedDictionary<ushort, ODentry>>();

            modules = new Dictionary<UInt16, Module>();


            //FIXME no way for the Major/Minor to make it to EDSVersion
            fi.EDSVersionMajor = 4;
            fi.EDSVersionMinor = 0;

            fi.FileVersion = "1";
            fi.FileRevision = 1;

            fi.CreationDateTime = DateTime.Now;
            fi.ModificationDateTime = DateTime.Now;

            du.Dummy0001 = false;
            du.Dummy0002 = false;
            du.Dummy0003 = false;
            du.Dummy0004 = false;
            du.Dummy0005 = false;
            du.Dummy0006 = false;
            du.Dummy0007 = false;

            ODentry od = new ODentry();

            dummy_ods.Add(0x002, new ODentry("Dummy Int8", 0x002,  DataType.INTEGER8, "0", AccessType.ro, PDOMappingType.optional, null));
            dummy_ods.Add(0x003, new ODentry("Dummy Int16", 0x003, DataType.INTEGER16, "0", AccessType.ro, PDOMappingType.optional, null));
            dummy_ods.Add(0x004, new ODentry("Dummy Int32", 0x004, DataType.INTEGER32, "0", AccessType.ro, PDOMappingType.optional, null));
            dummy_ods.Add(0x005, new ODentry("Dummy UInt8", 0x005, DataType.UNSIGNED8, "0", AccessType.ro, PDOMappingType.optional, null));
            dummy_ods.Add(0x006, new ODentry("Dummy UInt16", 0x006, DataType.UNSIGNED16, "0", AccessType.ro, PDOMappingType.optional, null));
            dummy_ods.Add(0x007, new ODentry("Dummy UInt32", 0x007, DataType.UNSIGNED32, "0", AccessType.ro, PDOMappingType.optional, null));

        }

        protected string sectionname = "";

        /// <summary>
        /// Verify PDO mapping parameters in Object Dictionary. Every mapped OD entry must exist and mapping must be allowed
        /// </summary>
        /// <returns>List of error strings, empty if no errors found.</returns>
        public List<string> VerifyPDOMapping()
        {
            List<string> mappingErrors = new List<string>();

            foreach (KeyValuePair<UInt16, ODentry> kvp in ods)
            {
                int indexPdo = kvp.Key;
                if (!((indexPdo >= 0x1600 && indexPdo < 0x1800) || (indexPdo >= 0x1A00 && indexPdo < 0x1C00)))
                    continue;

                string PDO = indexPdo < 0x1800 ? "RPDO" : "TPDO";

                ODentry odPdo = kvp.Value;
                for (byte subIdxPdo = 1; subIdxPdo < odPdo.subobjects.Count; subIdxPdo++)
                {
                    UInt32 mapVal;
                    try { mapVal = (UInt32)new System.ComponentModel.UInt32Converter().ConvertFromString(odPdo.subobjects[subIdxPdo].defaultvalue); }
                    catch (Exception) { continue; }

                    UInt16 mapIdx = (UInt16)(mapVal >> 16);
                    UInt16 mapSub = (UInt16)((mapVal >> 8) & 0xFF);

                    if (mapIdx < 0x1000)
                        continue;

                    bool missing = true;
                    AccessPDO accessPDO = AccessPDO.no;
                    if (ods.ContainsKey(mapIdx))
                    {
                        ODentry od = ods[mapIdx];
                        if (!od.prop.CO_disabled)
                        {
                            if (od.objecttype == ObjectType.VAR)
                            {
                                missing = false;
                                accessPDO = od.AccessPDO();
                            }
                            else if (od.subobjects.ContainsKey(mapSub))
                            {
                                missing = false;
                                accessPDO = od.subobjects[mapSub].AccessPDO();
                            }
                        }
                    }
                    if (missing)
                        mappingErrors.Add($"{PDO} 0x{indexPdo:X4},0x{subIdxPdo:X2}: missing OD entry 0x{mapIdx:X4},0x{mapSub:X2}");
                    else if (accessPDO == AccessPDO.no || (PDO == "RPDO" && accessPDO == AccessPDO.t) || (PDO == "TPDO" && accessPDO == AccessPDO.r))
                        mappingErrors.Add($"{PDO} 0x{indexPdo:X4},0x{subIdxPdo:X2}: not mappable OD entry 0x{mapIdx:X4},0x{mapSub:X2}");
                }
            }

            return mappingErrors;
        }

        public DataType Getdatatype(ODentry od)
        {

            if (od.objecttype == ObjectType.VAR)
            {
                return od.datatype;
            }

            if (od.objecttype == ObjectType.ARRAY)
            {
                ODentry sub2 = ods[od.Index];

                //FIX ME !!! INCONSISTANT setup of the datatype for arrays when loading xml and eds!!

                DataType t = sub2.datatype;

                if (sub2.Getsubobject(1) != null)
                {
                    t = sub2.Getsubobject(1).datatype;
                    if (t == DataType.UNKNOWN)
                        t = sub2.datatype;
                }

                return t;
            }

            //Warning, REC types need to be handled else where as the specific
            //implementation of a REC type depends on the exporter being used

            return DataType.UNKNOWN;

        }


        static public byte ConvertToByte(string defaultvalue)
        {
            if (defaultvalue == null || defaultvalue == "")
                return 0;

            return (Convert.ToByte(defaultvalue, Getbase(defaultvalue)));
        }
        /// <summary>
        /// Convert two bytes into Uint16 (big endian)
        /// </summary>
        /// <param name="bytes">bytes to convert to Uint16, only the 2 first will be used</param>
        /// <returns>value of the 2 bytes combined (big endian)</returns>
        static public UInt16 ConvertToUInt16(byte [] bytes)
        {

            UInt16 value = 0;

            value = (UInt16) ((bytes[0] << 8) | bytes[1]);

            return value;

        }
        /// <summary>
        /// Try to convert a string to UInt16
        /// </summary>
        /// <param name="defaultvalue">string containing a number</param>
        /// <returns>the value or 0 if unable to read it</returns>
        static public UInt16 ConvertToUInt16(string defaultvalue)
        {
            if (defaultvalue == null || defaultvalue == "" )
                return 0;

            return (Convert.ToUInt16(defaultvalue, Getbase(defaultvalue)));
        }
        /// <summary>
        /// Try to convert a string to UInt32
        /// </summary>
        /// <param name="defaultvalue">string containing a number</param>
        /// <returns>the value or 0 if unable to read it</returns>
        static public UInt32 ConvertToUInt32(string defaultvalue)
        {
            if (defaultvalue == null || defaultvalue == "" )
                return 0;

            return (Convert.ToUInt32(defaultvalue, Getbase(defaultvalue)));
        }
        /// <summary>
        /// Return number base of a string (10 for desimal, 16 for hex and 8 for octal)
        /// </summary>
        /// <param name="defaultvalue">a string that will be read to try to find its base number</param>
        /// <returns>16 if hex, 8 if octal else 10</returns>
        static public int Getbase(string defaultvalue)
        {

            if (defaultvalue == null || defaultvalue == "")
                return 10;

            int nobase = 10;

            String pat = @"^\s*0[xX][0-9a-fA-F]+\s*$";

            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
            Match m = r.Match(defaultvalue);
            if (m.Success)
            {
                nobase = 16;
            }

            pat = @"^0[0-7]+";
            r = new Regex(pat, RegexOptions.IgnoreCase);
            m = r.Match(defaultvalue);
            if (m.Success)
            {
                nobase = 8;
            }


            return nobase;
        }

        public void UpdatePDOcount()
        {
            di.NrOfRXPDO = 0;
            di.NrOfTXPDO = 0;
            foreach(KeyValuePair<UInt16,ODentry> kvp in ods)
            {
                ODentry od = kvp.Value;
                if(od.prop.CO_disabled == false && od.Index >= 0x1400 && od.Index < 0x1600)
                    di.NrOfRXPDO++;

                if(od.prop.CO_disabled == false && od.Index >= 0x1800 && od.Index < 0x1A00)
                    di.NrOfTXPDO++;

            }

        }

        /// <summary>
        /// Split on + , replace $NODEID with concrete value and add together
        /// </summary>
        /// <param name="input">input string containing a number maybe prefixed by $NODEID+ </param>
        /// <param name="nodeidpresent">if $NODEID is in the string</param>
        /// <returns></returns>
        public UInt32 GetNodeID(string input, out bool nodeidpresent)
        {

            if (input == null || input == "")
            {
                nodeidpresent = false;
                return 0;
            }

    		input = input.ToUpper();

            if(input.Contains("$NODEID"))     
                nodeidpresent = true;
            else
                nodeidpresent = false;

            try
            {
                if (dc.NodeID == 0)
                {
                    input = input.Replace("$NODEID", "");
                    input = input.Replace("+", "");
                    input = input.Replace(" ", "");
                    return Convert.ToUInt32(input.Trim(), Getbase(input));
                }

                input = input.Replace("$NODEID", dc.NodeID.ToString()); // dc.NodeID is decimal
                string[] bits = Array.ConvertAll(input.Split('+'), p => p.Trim()); // Split and Trim the value
                if (bits.Length==1)
                {
                    //nothing to parse here just return the value
                    return Convert.ToUInt32(input, Getbase(input));
                }

                if (bits.Length != 2)
                {
                    throw new FormatException("cannot parse " + input + "\nExpecting N+$NODEID or $NODEID+N");
                }

                UInt32 b1 = Convert.ToUInt32(bits[0], Getbase(bits[0]));
                UInt32 b2 = Convert.ToUInt32(bits[1], Getbase(bits[1]));

                return (UInt32)(b1 + b2);
            }
            catch(Exception e)
            {
                Warnings.warning_list.Add(String.Format("Error parsing node id {0} nodes, {1}", input,e.ToString()));
            }

            return 0;
        }

        /// <summary>
        /// Try to get a OD entry
        /// </summary>
        /// <param name="index">the index</param>
        /// <param name="od">null if not found</param>
        /// <returns>true if found, false if not</returns>
        public bool tryGetODEntry(UInt16 index, out ODentry od)
        {
            od = null;
            if(ods.ContainsKey(index))
            {
                od = ods[index];
                return true;
            }

            if(dummy_ods.ContainsKey(index))
            {
                od = dummy_ods[index];
                return true;
            }

            return false;
        }

        public ODentry Getobject(UInt16 no)
        {

            if(no>=0x002 && no<=0x007)
            {
                return dummy_ods[no];
            }

            if (ods.ContainsKey(no))
            {
                return ods[no];
            }

            return null;

        }


        public ODentry Getobject(string uniqueID)
        {
            foreach(KeyValuePair<UInt16,ODentry> e in ods)
            {
                if (e.Value.uniqueID == uniqueID)
                    return e.Value;

                if(e.Value.subobjects!=null && e.Value.subobjects.Count>0)
                {
                    foreach(KeyValuePair<UInt16, ODentry> sube in e.Value.subobjects)
                    {
                        if (sube.Value.uniqueID == uniqueID)
                            return sube.Value;
                    }

                }
                

            }

            return null;
        }
        /// <summary>
        /// Return the number of enabled objects
        /// </summary>
        /// <param name="includesub">Include subindexes in the counting</param>
        /// <returns></returns>
        public int GetNoEnabledObjects(bool includesub=false)
        {
            int enabledcount = 0;
            foreach (ODentry od in ods.Values)
            {
                if (od.prop.CO_disabled == false)
                {
                    enabledcount++;

                    if(includesub)
                    {
                        foreach(ODentry sub in od.subobjects.Values)
                        {
                            if (od.prop.CO_disabled == false)
                            {
                                enabledcount++;
                            }
                        }

                    }
                }
            }

            return enabledcount;

        }



    }

        public class ParameterException : Exception
        {
            public ParameterException(String message)
                : base(message)
            {
        
            }
        }

      

 }
