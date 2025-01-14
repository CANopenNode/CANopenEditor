using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEDSsharp
{
    /// <summary>
    /// Represent a PDO slot (mapping + communication index)
    /// </summary>
    public class PDOSlot
    {

        private UInt16 _MappingIndex;
        private UInt16 _ConfigurationIndex;
        /// <summary>
        /// Indicate that $NODEID is present and the COB-ID should added to the node id when deployed
        /// </summary>
        public bool nodeidpresent;
        /// <summary>
        /// The OD index of the PDO configuration (aka. communication parameter)
        /// </summary>
        public ushort ConfigurationIndex
        {
            get { return _ConfigurationIndex; }
            set {

                if(value==0)
                {
                    _ConfigurationIndex = 0;
                    _MappingIndex = 0;
                    return;
                }

                if ( ((value >= 0x1400) && (value < 0x1600)) || ((value >=0x1800) && (value <0x1a00)) )
                {
                    _ConfigurationIndex = value; _MappingIndex = (UInt16)(_ConfigurationIndex + (UInt16)0x200);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Configuration Index", "Must be between 0x1400 and 0x17FF ");
                }
                   
                   
            }
        }
        /// <summary>
        /// The OD index of the PDO mapping
        /// </summary>
        public ushort MappingIndex
        {
            get { return _MappingIndex; }
        }
        /// <summary>
        /// PDO Mapping access
        /// </summary>
        public EDSsharp.AccessType mappingAccessType;
        /// <summary>
        /// PDO Configuration (aka. communication parameter) access
        /// </summary>
        public EDSsharp.AccessType configAccessType;
        /// <summary>
        /// PDO mapping CanOpenNode storage group
        /// </summary>
        public string mappingloc;
        /// <summary>
        /// PDO config CanOpenNode storage group
        /// </summary>
        public string configloc;
        /// <summary>
        /// PDO COB-ID
        /// </summary>
        public uint COB;
        /// <summary>
        /// Returns if true the PDO is a TxPDO (aka TPDO)
        /// </summary>
        /// <returns>true if TXPDO</returns>
        public bool isTXPDO()
        {
            return ConfigurationIndex >= 0x1800;
        }
        /// <summary>
        /// Returns if true the PDO is a RxPDO (aka RPDO)
        /// </summary>
        /// <returns>true if RxPDO</returns>
        public bool isRXPDO()
        {
            return ConfigurationIndex < 0x1800;
        }
        /// <summary>
        /// PDO invalid bit value
        /// </summary>
        public bool invalid
        {
            get
            {
                return (COB & 0x80000000) != 0;
            }
            set
            {
        
                if (value == true)
                    COB = COB | 0x80000000;
                else
                    COB = COB & 0x7FFFFFFF;
            }
        }
        /// <summary>
        /// PDO mapping
        /// </summary>
        public List<ODentry> Mapping = new List<ODentry>();
        /// <summary>
        /// PDO inhibit time,multiple of 100us
        /// </summary>
        public UInt16 inhibit;
        /// <summary>
        /// PDO event time,multiple of 1ms
        /// </summary>
        public UInt16 eventtimer;
        /// <summary>
        /// PDO sync start value
        /// </summary>
        public byte syncstart;
        /// <summary>
        /// PDO transmission type
        /// </summary>
        public byte transmissiontype;
        /// <summary>
        /// Description of PDO communication index (aka configuration)
        /// </summary>
        public string DescriptionComm;
        /// <summary>
        /// Description of PDO mapping index
        /// </summary>
        public string DescriptionMap;
        /// <summary>
        /// default constructor
        /// </summary>
        public PDOSlot()
        {
            configloc = "PERSIST_COMM";
            mappingloc = "PERSIST_COMM";
            transmissiontype = 254;
            Mapping = new List<ODentry>();
            DescriptionComm = "";
            DescriptionMap = "";
        }
        /// <summary>
        /// Returns name of a OD entry (including dummy)
        /// </summary>
        /// <param name="od">object dictionary entry</param>
        /// <returns>name of entry with index and subindex prefixed, or blank string if not found</returns>
        public string getTargetName(ODentry od)
        {
            string target = "";

            if (od.Index >= 0x0002 && od.Index <= 0x007)
            {
                //the dummy objects
                switch (od.Index)
                {
                    case 0x002:
                        target = "0x0002/00/Dummy Int8";
                        break;
                    case 0x003:
                        target = "0x0003/00/Dummy Int16";
                        break;
                    case 0x004:
                        target = "0x0004/00/Dummy Int32";
                        break;
                    case 0x005:
                        target = "0x0005/00/Dummy UInt8";
                        break;
                    case 0x006:
                        target = "0x0006/00/Dummy UInt16";
                        break;
                    case 0x007:
                        target = "0x0007/00/Dummy UInt32";
                        break;
                }

            }
            else
            {
                target = String.Format("0x{0:X4}/{1:X2}/", od.Index, od.Subindex) + od.parameter_name;
            }

            return target;

        }
        /// <summary>
        /// Insert a OD entry into the mapping table
        /// </summary>
        /// <param name="ordinal">The zero-based index at which item should be inserted</param>
        /// <param name="entry">OD entry to be mapped</param>
        public void insertMapping(int ordinal, ODentry entry)
        {
            int size = 0;
            foreach(ODentry e in Mapping)
            {
                size += e.Sizeofdatatype();
            }

            if (size + entry.Sizeofdatatype() > 64)
                return;

            Mapping.Insert(ordinal,entry);
        }

    }

    /// <summary>
    /// PDO helper class, control all TPDO and RPDO in a node
    /// </summary>
    public class PDOHelper
    {

        EDSsharp eds;
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="eds">eds data to interact with</param>
        public PDOHelper(EDSsharp eds)
        {
            this.eds = eds;
        }
        /// <summary>
        /// List of all T/R PDO
        /// </summary>
        public List<PDOSlot> pdoslots = new List<PDOSlot>();
        /// <summary>
        /// Why is this not called from constructor?
        /// </summary>
        public void build_PDOlists()
        {
            //List<ODentry> odl = new List<ODentry>();
            build_PDOlist(0x1800,pdoslots);
            build_PDOlist(0x1400,pdoslots);

        }
        /// <summary>
        /// Look through the OD and register PDO
        /// </summary>
        /// <param name="startcob">OD index to to start looking from, it will stop after 0x1ff indexes</param>
        /// <param name="slots">list to add found pdo into</param>
        void build_PDOlist(UInt16 startcob, List<PDOSlot> slots)
        {
            for (UInt16 idx = startcob; idx < startcob + 0x01ff; idx++)
            {
                if (eds.ods.ContainsKey(idx))
                {
                    ODentry od = eds.ods[idx];
                    if (od.prop.CO_disabled == true)
                        continue;

                    //protect against not completed new CommunicationParamater sections
                    //we probably could do better and do more checking but as long as
                    //we protect against the subobjects[1] read in a few lines all else is
                    //good
                    if (od.subobjects.Count <= 1)
                        continue;

                    PDOSlot slot = new PDOSlot();

                    slot.COB = eds.GetNodeID(od.subobjects[1].defaultvalue, out slot.nodeidpresent);

                    if (od.Containssubindex(2))
                        slot.transmissiontype = EDSsharp.ConvertToByte(od.Getsubobject(2).defaultvalue);
                    
                    if (od.Containssubindex(3))
                        slot.inhibit = EDSsharp.ConvertToUInt16(od.Getsubobject(3).defaultvalue);

                    if (od.Containssubindex(5))
                        slot.eventtimer = EDSsharp.ConvertToUInt16(od.Getsubobject(5).defaultvalue);

                    if (od.Containssubindex(6))                  
                        slot.syncstart = EDSsharp.ConvertToByte(od.Getsubobject(6).defaultvalue);

                    slot.ConfigurationIndex = idx;

                    slot.configAccessType = od.accesstype;
                    slot.configloc = od.prop.CO_storageGroup;
                    slot.DescriptionComm = od.Description;


                    Console.WriteLine(String.Format("Found PDO Entry {0:X4} {1:X3}", idx, slot.COB));

                    //Look at mappings

                    ODentry mapping = eds.Getobject((ushort)(idx + 0x200));

                    if (mapping != null) {
                        slot.DescriptionMap = mapping.Description; 
                    }else
                    {
                        Console.WriteLine(string.Format("No mapping for index 0x{0:X4} should be at 0x{1:X4}", idx, idx + 0x200));
                        continue;
                    }

                    uint totalsize = 0;

                    slot.mappingAccessType = od.accesstype;
                    slot.mappingloc = od.prop.CO_storageGroup;

                    for (ushort subindex= 1; subindex<= mapping.Getmaxsubindex();subindex++)
                    {
                        ODentry sub = mapping.Getsubobject(subindex);
                        if (sub == null)
                            continue;

                        //Decode the mapping

                        UInt32 data = 0;

                        if (sub.defaultvalue.Length < 10)
                            continue;

                        if (sub.defaultvalue != "")
                            data = Convert.ToUInt32(sub.defaultvalue, EDSsharp.Getbase(sub.defaultvalue));

                        if (data == 0)
                            continue;

                        byte datasize = (byte)(data & 0x000000FF);
                        UInt16 pdoindex = (UInt16)((data >> 16) & 0x0000FFFF);
                        byte pdosub = (byte)((data >> 8) & 0x000000FF);

                        totalsize += datasize;

                        Console.WriteLine(string.Format("Mapping 0x{0:X4}/{1:X2} size {2}", pdoindex, pdosub, datasize));

                        //validate this against what is in the actual object mapped
                        try
                        {
                            ODentry maptarget;
                            if (pdosub == 0)
                            {
                                if (eds.tryGetODEntry(pdoindex, out maptarget) == false)
                                {
                                    Console.WriteLine("MAPPING FAILED");
                                    //Critical PDO error
                                    return;
                                }
                            }
                            else
                                maptarget = eds.ods[pdoindex].Getsubobject(pdosub);

                            if (maptarget.prop.CO_disabled == false && datasize == (maptarget.Sizeofdatatype()))
                            {
                                //mappingfail = false;
                            }
                            else
                            {
                                Console.WriteLine(String.Format("MAPPING FAILED {0} != {1}", datasize, maptarget.Sizeofdatatype()));
                            }

                            slot.Mapping.Add(maptarget);
                        }
                        catch (Exception) { }
                    }

                    Console.WriteLine(String.Format("Total PDO Size {0}\n", totalsize));

                    slots.Add(slot);
                }
            }

        }

        /// <summary>
        /// Rebuild the communication and mapping paramaters from the
        /// lists the PDOhelper currently has. These live in the list pdoslots
        /// </summary>
        public void buildmappingsfromlists(bool isCANopenNode_V4)
        {
            for(ushort x=0x1400;x<0x1c00;x++)
            {
                if (eds.ods.ContainsKey(x))
                    eds.ods.Remove(x);
            }

            foreach(PDOSlot slot in pdoslots)
            {

                ODentry config = new ODentry();
                config.Index = slot.ConfigurationIndex;
                config.datatype = DataType.PDO_COMMUNICATION_PARAMETER;
                config.objecttype = ObjectType.RECORD;
                config.accesstype = slot.configAccessType;
                config.prop.CO_storageGroup = slot.configloc;
                config.Description = slot.DescriptionComm;

                ODentry sub;

                if (slot.isTXPDO())
                {
                    config.parameter_name = "TPDO communication parameter";
                    config.prop.CO_countLabel = "TPDO";

                    sub = new ODentry("Highest sub-index supported", (ushort)slot.ConfigurationIndex, 0);
                    sub.defaultvalue = "0x06";
                    sub.datatype = DataType.UNSIGNED8;
                    sub.accesstype = EDSsharp.AccessType.ro;
                    config.addsubobject(0x00, sub);

                    sub = new ODentry("COB-ID used by TPDO", (ushort)slot.ConfigurationIndex, 1);
                    sub.datatype = DataType.UNSIGNED32;
                    if (slot.nodeidpresent)
                        sub.defaultvalue = "$NODEID+"; // DSP306: "The $NODEID must appear at the beginning of the expression. Otherwise the line is interpreted as without a formula. 
                    sub.defaultvalue += String.Format("0x{0:X}", slot.COB);
                    sub.accesstype = EDSsharp.AccessType.rw;
                    config.addsubobject(0x01, sub);

                    sub = new ODentry("Transmission type", (ushort)slot.ConfigurationIndex, 2);
                    sub.datatype = DataType.UNSIGNED8;
                    sub.defaultvalue = slot.transmissiontype.ToString();
                    sub.accesstype = EDSsharp.AccessType.rw;
                    config.addsubobject(0x02, sub);

                    sub = new ODentry("Inhibit time", (ushort)slot.ConfigurationIndex, 3);
                    sub.datatype = DataType.UNSIGNED16;
                    sub.defaultvalue = slot.inhibit.ToString();
                    sub.accesstype = EDSsharp.AccessType.rw;
                    config.addsubobject(0x03, sub);

                    if (!isCANopenNode_V4)
                    {
                        sub = new ODentry("compatibility entry", (ushort)slot.ConfigurationIndex, 4);
                        sub.datatype = DataType.UNSIGNED8;
                        sub.defaultvalue = "0";
                        sub.accesstype = EDSsharp.AccessType.rw;
                        config.addsubobject(0x04, sub);
                    }

                    sub = new ODentry("Event timer", (ushort)slot.ConfigurationIndex, 5);
                    sub.datatype = DataType.UNSIGNED16;
                    sub.defaultvalue = slot.eventtimer.ToString();
                    sub.accesstype = EDSsharp.AccessType.rw;
                    config.addsubobject(0x05, sub);

                    sub = new ODentry("SYNC start value", (ushort)slot.ConfigurationIndex, 6);
                    sub.datatype = DataType.UNSIGNED8;
                    sub.defaultvalue = slot.syncstart.ToString(); ;
                    sub.accesstype = EDSsharp.AccessType.rw;
                    config.addsubobject(0x06, sub);

                }
                else
                {
                    config.parameter_name = "RPDO communication parameter";
                    config.prop.CO_countLabel = "RPDO";

                    sub = new ODentry("Highest sub-index supported", (ushort)slot.ConfigurationIndex, 0);
                    sub.defaultvalue = isCANopenNode_V4 ? "0x05" : "0x02";
                    sub.datatype = DataType.UNSIGNED8;
                    sub.accesstype = EDSsharp.AccessType.ro;
                    config.addsubobject(0x00, sub);

                    sub = new ODentry("COB-ID used by RPDO", (ushort)slot.ConfigurationIndex, 1);
                    sub.datatype = DataType.UNSIGNED32;
                    if (slot.nodeidpresent)
                        sub.defaultvalue = "$NODEID+"; // DSP306: "The $NODEID must appear at the beginning of the expression. Otherwise the line is interpreted as without a formula. 
                    sub.defaultvalue += String.Format("0x{0:X}", slot.COB);
                    sub.accesstype = EDSsharp.AccessType.rw;
                    config.addsubobject(0x01, sub);

                    sub = new ODentry("Transmission type", (ushort)slot.ConfigurationIndex, 2);
                    sub.datatype = DataType.UNSIGNED8;
                    sub.defaultvalue = slot.transmissiontype.ToString();
                    sub.accesstype = EDSsharp.AccessType.rw;
                    config.addsubobject(0x02, sub);

                    if (isCANopenNode_V4)
                    {
                        sub = new ODentry("Event timer", (ushort)slot.ConfigurationIndex, 5);
                        sub.datatype = DataType.UNSIGNED16;
                        sub.defaultvalue = slot.eventtimer.ToString();
                        sub.accesstype = EDSsharp.AccessType.rw;
                        config.addsubobject(0x05, sub);
                    }
                }

                eds.ods.Add(slot.ConfigurationIndex,config);

                ODentry mapping = new ODentry();
                mapping.Index = slot.MappingIndex;
                mapping.datatype = DataType.PDO_MAPPING;
                mapping.objecttype = ObjectType.RECORD;

                if(slot.isTXPDO())
                    mapping.parameter_name = "TPDO mapping parameter";
                else
                    mapping.parameter_name = "RPDO mapping parameter";

                mapping.prop.CO_storageGroup = slot.mappingloc;
                mapping.accesstype = slot.mappingAccessType;
                mapping.Description = slot.DescriptionMap;

                sub = new ODentry("Number of mapped application objects in PDO", (ushort)slot.MappingIndex, 0);
                sub.datatype = DataType.UNSIGNED8;
                sub.defaultvalue = slot.Mapping.Count().ToString();
                sub.accesstype = EDSsharp.AccessType.rw;
                mapping.addsubobject(0x00, sub);

                byte mappingcount = 1;
                foreach (ODentry mapslot in slot.Mapping)
                {
                    sub = new ODentry(String.Format("Application object {0:X}", mappingcount), (ushort)slot.MappingIndex, mappingcount);
                    sub.datatype = DataType.UNSIGNED32;
                    sub.defaultvalue = string.Format("0x{0:X4}{1:X2}{2:X2}", mapslot.Index, mapslot.Subindex, mapslot.Sizeofdatatype());
                    sub.accesstype = EDSsharp.AccessType.rw;
                    mapping.addsubobject(mappingcount, sub);

                    mappingcount++;

                }

                for (; mappingcount <= 8; mappingcount++)
                {
                    sub = new ODentry(String.Format("Application object {0:X}", mappingcount), (ushort)slot.MappingIndex, mappingcount);
                    sub.datatype = DataType.UNSIGNED32;
                    sub.defaultvalue = "0x00000000";
                    sub.accesstype = EDSsharp.AccessType.rw;
                    mapping.addsubobject(mappingcount, sub);
                }
                eds.ods.Add(slot.MappingIndex,mapping);
               
            }
        }

        /// <summary>
        /// Add a PDO slot as set by index
        /// </summary>
        /// <param name="configindex"></param>
        public void addPDOslot(UInt16 configindex)
        {

            //quick range check, it must be a config index for an RXPDO or a TXPDO
            if( (configindex<0x1400) || (configindex >= 0x1a00)  || ((configindex>=0x1600) && (configindex<0x1800)))

                return;

            foreach(PDOSlot slot in pdoslots)
            {
                if (slot.ConfigurationIndex == configindex)
                    return;
            }

            bool isTXPDO = configindex >= 0x1800;

            PDOSlot newslot = new PDOSlot();
            newslot.ConfigurationIndex = configindex;

            switch (configindex)
            {
                case 0x1400:
                    newslot.COB = 0x80000200;
                    newslot.nodeidpresent = true;
                    break;
                case 0x1401:
                    newslot.COB = 0x80000300;
                    newslot.nodeidpresent = true;
                    break;
                case 0x1402:
                    newslot.COB = 0x80000400;
                    newslot.nodeidpresent = true;
                    break;
                case 0x1403:
                    newslot.COB = 0x80000500;
                    newslot.nodeidpresent = true;
                    break;
                case 0x1800:
                    newslot.COB = 0xC0000180;
                    newslot.nodeidpresent = true;
                    break;
                case 0x1801:
                    newslot.COB = 0xC0000280;
                    newslot.nodeidpresent = true;
                    break;
                case 0x1802:
                    newslot.COB = 0xC0000380;
                    newslot.nodeidpresent = true;
                    break;
                case 0x1803:
                    newslot.COB = 0xC0000480;
                    newslot.nodeidpresent = true;
                    break;
                default:
                    newslot.COB = 0xC0000000;
                    break;
            }
            
            newslot.configloc = "PERSIST_COMM";
            newslot.mappingloc = "PERSIST_COMM";

            pdoslots.Add(newslot);

        }

        /// <summary>
        /// This finds a gap in the PDO slots
        /// </summary>
        public UInt16 findPDOslotgap(bool isTXPDO)
        {
            //firstly find the first gap and place it there

            UInt16 startindex = 0x1400;

            if (isTXPDO)
                startindex = 0x1800;

            for(UInt16 index = startindex; index<(startindex+0x200);index++)
            {
                bool found = false;
                foreach(PDOSlot slot in pdoslots)
                {
                    if (slot.ConfigurationIndex == index)
                    {
                        found = true;
                        break;
                    }
                }

                if(found==false)
                {
                    return index;
                }
            }

            //no gaps
            return 0x0000;
        }

        /// <summary>
        /// Remove existing PDO slot as set by index
        /// </summary>
        /// <param name="configindex"></param>
        public void removePDOslot(UInt16 configindex)
        {
            foreach (PDOSlot slot in pdoslots)
            {
                if (slot.ConfigurationIndex == configindex)
                {
                    pdoslots.Remove(slot);
                    break;
                }
            }
        }
    }
}
