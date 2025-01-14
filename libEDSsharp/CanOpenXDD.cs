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
    based heavily on the files CO_OD.h and CO_OD.c from CANopenNode which are
    Copyright(c) 2010 - 2016 Janez Paternoster
*/


using System;
using System.Xml.Serialization;
using System.IO;
using CanOpenXSD_1_0;
using System.Text.RegularExpressions; //and nope this is not anywhere near the xml parsing
using System.Collections.Generic;

namespace libEDSsharp
{
    public class CanOpenXDD : IFileExporter
    {
        public ISO15745ProfileContainer dev;

        /// <summary>
        /// Fetches all the different fileexporter types the class supports
        /// </summary>
        /// <returns>List of the different exporters the class supports</returns>
        public ExporterDescriptor[] GetExporters()
        {
            return new ExporterDescriptor[] { 
                new ExporterDescriptor("CanOpen XDD v1.0", new string[] { ".xdd" }, 0, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD();
                    e.writeXML(filepath,edss[0]);
                }),
                new ExporterDescriptor("CanOpen Network v1.0", new string[] { ".nxdd" }, ExporterDescriptor.ExporterFlags.MultipleNodeSupport, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD();
                    e.writeMultiXML(filepath,edss);
                })
            };
        }

        public EDSsharp readXML(string file)
        {

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ISO15745ProfileContainer));
                StreamReader reader = new StreamReader(file);
                dev = (ISO15745ProfileContainer)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception)
            {
                return null;
            }

            return convert(dev);

        }

        public List<EDSsharp> readMultiXML(string file )
        {

            List<EDSsharp> edss = new List<EDSsharp>();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(OpenEDSProject));
                StreamReader reader = new StreamReader(file);
                OpenEDSProject oep = (OpenEDSProject)serializer.Deserialize(reader);

                foreach(ISO15745ProfileContainer cont in oep.ISO15745ProfileContainer)
                {
                    edss.Add(convert(cont));
                }

                reader.Close();

                return edss;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }    
        }

        public void writeMultiXML(string file, List<EDSsharp> edss)
        {

            List<ISO15745ProfileContainer> devs = new List<ISO15745ProfileContainer>();

            foreach (EDSsharp eds in edss)
            {
                ISO15745ProfileContainer dev = convert(eds);
                devs.Add(dev);
            }

            OpenEDSProject oep = new OpenEDSProject();
            oep.Version = "1.0";

            oep.ISO15745ProfileContainer = devs;

            XmlSerializer serializer = new XmlSerializer(typeof(OpenEDSProject));
            StreamWriter writer = new StreamWriter(file);
            serializer.Serialize(writer, oep);
            writer.Close();
        }

        public void writeXML(string file, EDSsharp eds)
        {
            dev = convert(eds);
            XmlSerializer serializer = new XmlSerializer(typeof(ISO15745ProfileContainer));
            StreamWriter writer = new StreamWriter(file);
            serializer.Serialize(writer, dev);
            writer.Close();
        }

        public void fillparamater(parameter p, ODentry od)
        {

            if (od.parent == null)
            {
                p.uniqueID = string.Format("UID_PARAM_{0:x4}", od.Index);
            }
            else
            {
                p.uniqueID = string.Format("UID_PARAM_{0:x4}{1:x2}", od.parent.Index, od.Subindex);
            }

            switch (od.accesstype)
            {
                case EDSsharp.AccessType.rw:
                    p.access = parameterTemplateAccess.readWrite;
                    break;

                case EDSsharp.AccessType.ro:
                    p.access = parameterTemplateAccess.read;
                    break;

                case EDSsharp.AccessType.wo:
                    p.access = parameterTemplateAccess.write;
                    break;

                case EDSsharp.AccessType.rwr:
                    p.access = parameterTemplateAccess.readWriteInput;
                    break;

                case EDSsharp.AccessType.rww:
                    p.access = parameterTemplateAccess.readWriteOutput;
                    break;

                case EDSsharp.AccessType.@const:
                    p.access = parameterTemplateAccess.@const;
                    break;

                //fixme no access not handled
                default:
                    p.access = parameterTemplateAccess.noAccess;
                    break;

            }

            p.Items = new object[2];

            vendorTextLabel lab = new vendorTextLabel();
            lab.lang = "en";
            lab.Value = od.parameter_name;
            p.Items[0] = lab;


            //FIXME we are currently writing the denotation value to both the object and the parameterList section
            //i'm not sure why two exist

            denotation denot = new denotation();
            vendorTextLabel lab2 = new vendorTextLabel();
            lab2.lang = "en";
            lab2.Value = od.denotation;
            denot.Items = new object[1];
            denot.Items[0] = lab2;
            p.denotation = denot;
            

            vendorTextDescription desc = new vendorTextDescription();
            desc.lang = "en"; //fixme we could and should do better than just English
            desc.Value = od.Description;
            p.Items[1] = desc;

            p.defaultValue = new defaultValue();
            p.defaultValue.value = od.defaultvalue;

        }


        public ISO15745ProfileContainer convert(EDSsharp eds)
        {
            dev = new ISO15745ProfileContainer();
            
            dev.ISO15745Profile = new ISO15745Profile[2];



            //Profile 0 ProfileBody_Device_CANopen
            dev.ISO15745Profile[0] = new ISO15745Profile();
            dev.ISO15745Profile[0].ProfileHeader = new ProfileHeader_DataType();

            dev.ISO15745Profile[0].ProfileHeader.ProfileIdentification = "CAN device profile";
            dev.ISO15745Profile[0].ProfileHeader.ProfileRevision = "1";
            dev.ISO15745Profile[0].ProfileHeader.ProfileClassID = ProfileClassID_DataType.Device;
            dev.ISO15745Profile[0].ProfileHeader.ProfileName = "";
            dev.ISO15745Profile[0].ProfileHeader.ProfileSource = "";

            dev.ISO15745Profile[0].ProfileHeader.ISO15745Reference = new ISO15745Reference_DataType();
            dev.ISO15745Profile[0].ProfileHeader.ISO15745Reference.ISO15745Part = "1";
            dev.ISO15745Profile[0].ProfileHeader.ISO15745Reference.ISO15745Edition = "1";
            dev.ISO15745Profile[0].ProfileHeader.ISO15745Reference.ProfileTechnology = "CANopen";

            dev.ISO15745Profile[0].ProfileBody = new ProfileBody_Device_CANopen();

            ProfileBody_Device_CANopen device = (ProfileBody_Device_CANopen)dev.ISO15745Profile[0].ProfileBody;

            device.DeviceIdentity = new DeviceIdentity();

            device.DeviceIdentity.vendorName = new vendorName();
            device.DeviceIdentity.vendorName.Value = eds.di.VendorName;
            device.DeviceIdentity.vendorName.readOnly = true;

            device.DeviceIdentity.vendorID = new vendorID();
            device.DeviceIdentity.vendorID.Value = eds.di.VendorNumber;
            device.DeviceIdentity.vendorID.readOnly = true;

            device.DeviceIdentity.deviceFamily = new deviceFamily();

            device.DeviceIdentity.productFamily = new productFamily();

            //device.DeviceIdentity.orderNumber = 

            device.fileCreationDate = eds.fi.CreationDateTime;
            device.fileCreationTime = eds.fi.CreationDateTime;
            device.fileCreationTimeSpecified = true;
            
            device.fileModificationDate = eds.fi.ModificationDateTime;
            device.fileModificationTime = eds.fi.ModificationDateTime;
            device.fileModificationDateSpecified = true;
            device.fileModificationTimeSpecified = true;

            device.fileCreator = eds.fi.CreatedBy;
            device.fileModifiedBy = eds.fi.ModifiedBy;

            device.supportedLanguages = "en";

            device.fileVersion = eds.fi.FileVersion;

            device.fileName = Path.GetFileName(eds.projectFilename);
            

            //device.DeviceIdentity.vendorText
            //device.DeviceIdentity.deviceFamily
            //device.DeviceIdentity.productFamily;

            device.DeviceIdentity.productName = new productName();
            device.DeviceIdentity.productName.Value = eds.di.ProductName;
            device.DeviceIdentity.productName.readOnly = true;

            device.DeviceIdentity.productID = new productID();
            device.DeviceIdentity.productID.Value = eds.di.ProductNumber;
            device.DeviceIdentity.productID.readOnly = true;

            device.DeviceIdentity.productText = new productText();
            device.DeviceIdentity.productText.Items = new object[1];


            vendorTextDescription des = new vendorTextDescription();
            des.lang = "en";

            des.Value = String.Format("FileDescription={0}|EdsVersion={1}|FileRevision={2}|RevisionNum={3}",eds.fi.Description,eds.fi.EDSVersion,eds.fi.FileVersion,eds.fi.FileRevision);

            device.DeviceIdentity.productText.Items[0] = des;
            device.DeviceIdentity.productText.readOnly = true;

            //device.DeviceIdentity.orderNumber

            device.DeviceIdentity.version = new version[3];

            device.DeviceIdentity.version[0] = new version();
            device.DeviceIdentity.version[0].readOnly = true;
            device.DeviceIdentity.version[0].versionType = versionVersionType.SW;

            device.DeviceIdentity.version[1] = new version();
            device.DeviceIdentity.version[1].readOnly = true;
            device.DeviceIdentity.version[1].versionType = versionVersionType.FW;

            device.DeviceIdentity.version[2] = new version();
            device.DeviceIdentity.version[2].readOnly = true;
            device.DeviceIdentity.version[2].versionType = versionVersionType.HW;

            //device.DeviceIdentity.specificationRevision.value = 
            device.DeviceIdentity.specificationRevision = new specificationRevision();
            device.DeviceIdentity.specificationRevision.readOnly = true;

            device.DeviceIdentity.instanceName = new instanceName();
            //device.DeviceIdentity.instanceName.Value = ;
            device.DeviceIdentity.instanceName.readOnly = true;


            device.DeviceManager = new DeviceManager();
            device.DeviceManager.indicatorList = new indicatorList();
            device.DeviceManager.indicatorList.LEDList = new LEDList();
            device.DeviceManager.indicatorList.LEDList.LED = new LED[1]; //fixme
            device.DeviceManager.indicatorList.LEDList.LED[0] = new LED();
            device.DeviceManager.indicatorList.LEDList.LED[0].LEDcolors = LEDLEDcolors.monocolor;
            device.DeviceManager.indicatorList.LEDList.LED[0].LEDtype = LEDLEDtype.device;
            device.DeviceManager.indicatorList.LEDList.LED[0].Items = new object[1];

            // LEDstate ls = new LEDstate();
            // ls.uniqueID = "LED_State_1";
            // ls.state = LEDstateState.off;
            // ls.LEDcolor = LEDstateLEDcolor.green;

            // device.DeviceManager.indicatorList.LEDList.LED[0].Items[0] = ls;

            device.DeviceFunction = new DeviceFunction[1];
            //fix me fll this in


            device.ApplicationProcess = new ApplicationProcess[1];
            device.ApplicationProcess[0] = new ApplicationProcess();

            device.ApplicationProcess[0].parameterList = new parameter[eds.GetNoEnabledObjects(true)];

            int ordinal = 0;

            foreach (ODentry od in eds.ods.Values)
            {

                if (od.prop.CO_disabled)
                    continue;

                parameter p = new parameter();

                fillparamater(p, od);

                device.ApplicationProcess[0].parameterList[ordinal] = p;
                ordinal++;

                foreach (ODentry sub in od.subobjects.Values)
                {
                    p = new parameter();

                    fillparamater(p, sub);

                    device.ApplicationProcess[0].parameterList[ordinal] = p;
                    ordinal++;
                }

            }


            //Profile 1 ProfileClassID_DataType.CommunicationNetwork
            dev.ISO15745Profile[1] = new ISO15745Profile();

            dev.ISO15745Profile[1].ProfileHeader = new ProfileHeader_DataType();

            dev.ISO15745Profile[1].ProfileHeader.ProfileIdentification = "CAN comm net profile";
            dev.ISO15745Profile[1].ProfileHeader.ProfileRevision = "1";
            dev.ISO15745Profile[1].ProfileHeader.ProfileClassID = ProfileClassID_DataType.CommunicationNetwork;
            dev.ISO15745Profile[1].ProfileHeader.ProfileName = "";
            dev.ISO15745Profile[1].ProfileHeader.ProfileSource = "";

            dev.ISO15745Profile[1].ProfileHeader.ISO15745Reference = new ISO15745Reference_DataType();
            dev.ISO15745Profile[1].ProfileHeader.ISO15745Reference.ISO15745Part = "1";
            dev.ISO15745Profile[1].ProfileHeader.ISO15745Reference.ISO15745Edition = "1";
            dev.ISO15745Profile[1].ProfileHeader.ISO15745Reference.ProfileTechnology = "CANopen";

            dev.ISO15745Profile[1].ProfileBody = new ProfileBody_CommunicationNetwork_CANopen();
            ProfileBody_CommunicationNetwork_CANopen comnet = (ProfileBody_CommunicationNetwork_CANopen)dev.ISO15745Profile[1].ProfileBody;
            comnet.Items = new object[3];

            comnet.fileName = Path.GetFileName(eds.projectFilename);

            comnet.fileCreator = eds.fi.CreatedBy; //etc
            comnet.fileCreationDate = eds.fi.CreationDateTime;
            comnet.fileCreationTime = eds.fi.CreationDateTime;
            comnet.fileCreationTimeSpecified = true;

            comnet.fileModificationDate = eds.fi.ModificationDateTime;
            comnet.fileModificationTime = eds.fi.ModificationDateTime;
            comnet.fileModificationDateSpecified = true;

            comnet.fileVersion = eds.fi.FileVersion;

            comnet.supportedLanguages = "en";

            comnet.Items[0] = new ProfileBody_CommunicationNetwork_CANopenApplicationLayers();
            ProfileBody_CommunicationNetwork_CANopenApplicationLayers AppLayer = (ProfileBody_CommunicationNetwork_CANopenApplicationLayers)comnet.Items[0];

            comnet.Items[1] = new ProfileBody_CommunicationNetwork_CANopenTransportLayers();
            ProfileBody_CommunicationNetwork_CANopenTransportLayers TransportLayer = (ProfileBody_CommunicationNetwork_CANopenTransportLayers)comnet.Items[1];

            comnet.Items[2] = new ProfileBody_CommunicationNetwork_CANopenNetworkManagement();
            ProfileBody_CommunicationNetwork_CANopenNetworkManagement NetworkManagement = (ProfileBody_CommunicationNetwork_CANopenNetworkManagement)comnet.Items[2];

            AppLayer.CANopenObjectList = new CANopenObjectList();


            AppLayer.CANopenObjectList.CANopenObject = new CANopenObjectListCANopenObject[eds.GetNoEnabledObjects()];
            
            int count = 0;
            foreach (KeyValuePair<UInt16,ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;
                UInt16 subindex = kvp.Key;

                if (od.prop.CO_disabled)
                    continue;

                AppLayer.CANopenObjectList.CANopenObject[count] = new CANopenObjectListCANopenObject();

                byte[] bytes = BitConverter.GetBytes((UInt16)od.Index);
                Array.Reverse(bytes);

                AppLayer.CANopenObjectList.CANopenObject[count].index = bytes;
                AppLayer.CANopenObjectList.CANopenObject[count].name = od.parameter_name;
                AppLayer.CANopenObjectList.CANopenObject[count].objectType = (byte)od.objecttype;

                bytes = BitConverter.GetBytes((UInt16)od.datatype);
                Array.Reverse(bytes);

                // hack - special handling for rrw / rww access type 
                // https://github.com/robincornelius/libedssharp/issues/128
                EDSsharp.AccessType accesstype = od.accesstype;
                PDOMappingType PDOtype = od.PDOtype;
                if (accesstype == EDSsharp.AccessType.rww) {
                    accesstype = EDSsharp.AccessType.rw;

                    // when optional, set it to the corresponding type
                    if (PDOtype == PDOMappingType.optional) {
                        PDOtype = PDOMappingType.RPDO;
                    }
                }
                if (accesstype == EDSsharp.AccessType.rwr) {
                    accesstype = EDSsharp.AccessType.rw;

                    // when optional, set it to the corresponding type
                    if (PDOtype == PDOMappingType.optional) {
                        PDOtype = PDOMappingType.TPDO;
                    }
                }


                if (od.objecttype != ObjectType.ARRAY && od.objecttype != ObjectType.RECORD)
                {
                    //#209 don't set data type for array or rec objects, the subobjects hold 
                    //the data type
                    AppLayer.CANopenObjectList.CANopenObject[count].dataType = bytes;
                    AppLayer.CANopenObjectList.CANopenObject[count].accessType = (CANopenObjectListCANopenObjectAccessType)Enum.Parse(typeof(CANopenObjectListCANopenObjectAccessType), accesstype.ToString());
                    AppLayer.CANopenObjectList.CANopenObject[count].accessTypeSpecified = true;

                }
                else
                {
                    AppLayer.CANopenObjectList.CANopenObject[count].accessTypeSpecified = false;
                }

                AppLayer.CANopenObjectList.CANopenObject[count].PDOmapping = (CANopenObjectListCANopenObjectPDOmapping)Enum.Parse(typeof(CANopenObjectListCANopenObjectPDOmapping),PDOtype.ToString());
                AppLayer.CANopenObjectList.CANopenObject[count].PDOmappingSpecified = true;

                AppLayer.CANopenObjectList.CANopenObject[count].uniqueIDRef = String.Format("UID_PARAM_{0:x4}", od.Index);

                AppLayer.CANopenObjectList.CANopenObject[count].denotation = od.denotation;
                AppLayer.CANopenObjectList.CANopenObject[count].edseditor_extenstion_storagelocation = od.prop.CO_storageGroup;

                AppLayer.CANopenObjectList.CANopenObject[count].edseditor_extension_notifyonchange = od.prop.CO_flagsPDO;

                AppLayer.CANopenObjectList.CANopenObject[count].highLimit = od.HighLimit;
                AppLayer.CANopenObjectList.CANopenObject[count].lowLimit = od.LowLimit;
                AppLayer.CANopenObjectList.CANopenObject[count].actualValue = od.actualvalue;

                if (od.subobjects != null && od.subobjects.Count > 0)
                {
                    AppLayer.CANopenObjectList.CANopenObject[count].subNumber = (byte)od.subobjects.Count;
                    AppLayer.CANopenObjectList.CANopenObject[count].subNumberSpecified = true;

                    AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject = new CANopenObjectListCANopenObjectCANopenSubObject[od.subobjects.Count];

                    int subcount = 0;

                    foreach ( KeyValuePair<UInt16,ODentry> kvp2 in od.subobjects)
                    {
                        ODentry subod = kvp2.Value;
                        UInt16 subindex2 = kvp2.Key;

                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount] = new CANopenObjectListCANopenObjectCANopenSubObject();

                        bytes = BitConverter.GetBytes((UInt16)kvp2.Key);
                        Array.Reverse(bytes);

                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].subIndex = bytes;
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].name = subod.parameter_name;
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].objectType = (byte)subod.objecttype;

                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].denotation = subod.denotation;

                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].edseditor_extension_notifyonchange = subod.prop.CO_flagsPDO;

                        bytes = (subod.datatype == DataType.UNKNOWN && od.parent != null) ? BitConverter.GetBytes((UInt16)od.parent.datatype) : BitConverter.GetBytes((UInt16)subod.datatype);
                      
                        Array.Reverse(bytes);

                        // hack - special handling for rrw / rww access type
                        // https://github.com/robincornelius/libedssharp/issues/128
                        accesstype = subod.accesstype;
                        PDOtype = subod.PDOtype;
                        if (accesstype == EDSsharp.AccessType.rww) {
                            accesstype = EDSsharp.AccessType.rw;

                            // when optional is set, 
                            if (PDOtype == PDOMappingType.optional) {
                                PDOtype = PDOMappingType.RPDO;
                            }
                        }
                        if (accesstype == EDSsharp.AccessType.rwr) {
                            accesstype = EDSsharp.AccessType.rw;

                            // when optional is set, 
                            if (PDOtype == PDOMappingType.optional) {
                                PDOtype = PDOMappingType.TPDO;
                            }
                        }
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].dataType = bytes;
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].PDOmapping = (CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping)Enum.Parse(typeof(CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping),PDOtype.ToString());
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].PDOmappingSpecified = true;
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].uniqueIDRef = String.Format("UID_PARAM_{0:x4}{1:x2}", od.Index, subindex2);
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].accessType = (CANopenObjectListCANopenObjectCANopenSubObjectAccessType)Enum.Parse(typeof(CANopenObjectListCANopenObjectCANopenSubObjectAccessType), accesstype.ToString());
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].accessTypeSpecified = true;

                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].highLimit = subod.HighLimit;
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].lowLimit = subod.LowLimit;
                        AppLayer.CANopenObjectList.CANopenObject[count].CANopenSubObject[subcount].actualValue = subod.actualvalue;


                        subcount++;
                    }
                }


                count++;
            }

            AppLayer.dummyUsage = new ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummy[7];

            for (int x = 0; x < 7; x++)
            {
                AppLayer.dummyUsage[x] = new ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummy();
            }

            //FIX ME this is terrible
            AppLayer.dummyUsage[0].entry = (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry)Enum.Parse(typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry), string.Format("Dummy0001{0}", eds.du.Dummy0001 == true ? "1" : "0"));
            AppLayer.dummyUsage[1].entry = (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry)Enum.Parse(typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry), string.Format("Dummy0002{0}", eds.du.Dummy0002 == true ? "1" : "0"));
            AppLayer.dummyUsage[2].entry = (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry)Enum.Parse(typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry), string.Format("Dummy0003{0}", eds.du.Dummy0003 == true ? "1" : "0"));
            AppLayer.dummyUsage[3].entry = (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry)Enum.Parse(typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry), string.Format("Dummy0004{0}", eds.du.Dummy0004 == true ? "1" : "0"));
            AppLayer.dummyUsage[4].entry = (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry)Enum.Parse(typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry), string.Format("Dummy0005{0}", eds.du.Dummy0005 == true ? "1" : "0"));
            AppLayer.dummyUsage[5].entry = (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry)Enum.Parse(typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry), string.Format("Dummy0006{0}", eds.du.Dummy0006 == true ? "1" : "0"));
            AppLayer.dummyUsage[6].entry = (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry)Enum.Parse(typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry), string.Format("Dummy0007{0}", eds.du.Dummy0007 == true ? "1" : "0"));

            TransportLayer.PhysicalLayer = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayer();

            //FIX me this is worse than above

            int bauds = 0;

            if (eds.di.BaudRate_10 == true)
                bauds++;
            if (eds.di.BaudRate_20 == true)
                bauds++;
            if (eds.di.BaudRate_50 == true)
                bauds++;
            if (eds.di.BaudRate_125 == true)
                bauds++;
            if (eds.di.BaudRate_250 == true)
                bauds++;
            if (eds.di.BaudRate_500 == true)
                bauds++;
            if (eds.di.BaudRate_800 == true)
                bauds++;
            if (eds.di.BaudRate_1000 == true)
                bauds++;

            //Fixme auto baudrate needs adding to system
            TransportLayer.PhysicalLayer.baudRate = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRate();
            TransportLayer.PhysicalLayer.baudRate.supportedBaudRate = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate[bauds];

            for (int x = 0; x < bauds; x++)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[x] = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate();
            }

            bauds = 0;

            if (eds.di.BaudRate_10 == true)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[bauds].value = ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item10Kbps;
                bauds++;
            }
            if (eds.di.BaudRate_20 == true)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[bauds].value = ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item20Kbps;
                bauds++;
            }
            if (eds.di.BaudRate_50 == true)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[bauds].value = ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item50Kbps;
                bauds++;
            }
            if (eds.di.BaudRate_125 == true)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[bauds].value = ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item125Kbps;
                bauds++;
            }
            if (eds.di.BaudRate_250 == true)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[bauds].value = ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item250Kbps;
                bauds++;
            }
            if (eds.di.BaudRate_500 == true)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[bauds].value = ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item500Kbps;
                bauds++;
            }
            if (eds.di.BaudRate_800 == true)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[bauds].value = ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item800Kbps;
                bauds++;
            }
            if (eds.di.BaudRate_1000 == true)
            {
                TransportLayer.PhysicalLayer.baudRate.supportedBaudRate[bauds].value = ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item1000Kbps;
                bauds++;
            }


            NetworkManagement.CANopenGeneralFeatures = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementCANopenGeneralFeatures();

            NetworkManagement.CANopenGeneralFeatures.bootUpSlave = eds.di.SimpleBootUpSlave;
            //NetworkManagment.CANopenGeneralFeatures.dynamicChannels = eds.di.DynamicChannelsSupported;   //fix me count of dynamic channels not handled yet eds only has bool
            NetworkManagement.CANopenGeneralFeatures.granularity = eds.di.Granularity;
            NetworkManagement.CANopenGeneralFeatures.groupMessaging = eds.di.GroupMessaging;

            NetworkManagement.CANopenGeneralFeatures.ngMaster = eds.di.NG_Master;
            NetworkManagement.CANopenGeneralFeatures.ngSlave = eds.di.NG_Slave;
            NetworkManagement.CANopenGeneralFeatures.NrOfNG_MonitoredNodes = eds.di.NrOfNG_MonitoredNodes;

            NetworkManagement.CANopenGeneralFeatures.layerSettingServiceSlave = eds.di.LSS_Supported;
            NetworkManagement.CANopenGeneralFeatures.nrOfRxPDO = eds.di.NrOfRXPDO;
            NetworkManagement.CANopenGeneralFeatures.nrOfTxPDO = eds.di.NrOfTXPDO;
            //extra items

            //NetworkManagment.CANopenGeneralFeatures.SDORequestingDevice;
            //NetworkManagment.CANopenGeneralFeatures.selfStartingDevice;

            NetworkManagement.CANopenMasterFeatures = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementCANopenMasterFeatures();

            NetworkManagement.CANopenMasterFeatures.bootUpMaster = eds.di.SimpleBootUpMaster;

            //Extra items
            //NetworkManagment.CANopenMasterFeatures.configurationManager;
            //NetworkManagment.CANopenMasterFeatures.flyingMaster;
            NetworkManagement.CANopenMasterFeatures.layerSettingServiceMaster = eds.di.LSS_Master;
            //NetworkManagment.CANopenMasterFeatures.SDOManager;


            NetworkManagement.deviceCommissioning = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementDeviceCommissioning();
            NetworkManagement.deviceCommissioning.actualBaudRate = eds.dc.Baudrate.ToString();
            NetworkManagement.deviceCommissioning.NodeID = eds.dc.NodeID;
            NetworkManagement.deviceCommissioning.networkName = eds.dc.NetworkName;
            NetworkManagement.deviceCommissioning.networkNumber = eds.dc.NetNumber;
            NetworkManagement.deviceCommissioning.CANopenManager = eds.dc.CANopenManager;

            //fixme unconnected
            //eds.dc.LSS_SerialNumber;



            return dev;
        }



        public EDSsharp convert(ISO15745ProfileContainer container)
        {
            EDSsharp eds = new EDSsharp();

            //Find Object Dictionary entries

           //fixme??
           // ProfileBody_DataType dt;


            ProfileBody_CommunicationNetwork_CANopen body_network = null;
            ProfileBody_Device_CANopen body_device = null;


            foreach (ISO15745Profile dev in container.ISO15745Profile)
            {
                if (dev.ProfileBody.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopen))
                {
                    body_network = (ProfileBody_CommunicationNetwork_CANopen)dev.ProfileBody;
                }

                if (dev.ProfileBody.GetType() == typeof(ProfileBody_Device_CANopen))
                {
                    body_device = (ProfileBody_Device_CANopen)dev.ProfileBody;

                }

            }

            //ProfileBody_CommunicationNetwork_CANopen
            if (body_network != null)
            {
                ProfileBody_CommunicationNetwork_CANopen obj = body_network;

                ProfileBody_CommunicationNetwork_CANopenApplicationLayers ApplicationLayers = null;
                ProfileBody_CommunicationNetwork_CANopenTransportLayers TransportLayers = null;
                ProfileBody_CommunicationNetwork_CANopenNetworkManagement NetworkManagment = null;

                foreach (object obj2 in obj.Items)
                {

                    if (obj2.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        ApplicationLayers = (ProfileBody_CommunicationNetwork_CANopenApplicationLayers)obj2;

                    if (obj2.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenTransportLayers))
                        TransportLayers = (ProfileBody_CommunicationNetwork_CANopenTransportLayers)obj2;

                    if (obj2.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenNetworkManagement))
                        NetworkManagment = (ProfileBody_CommunicationNetwork_CANopenNetworkManagement)obj2;

                }

                if (ApplicationLayers != null)
                {

                    string vendorID = "";
                    //fixme
                    //string deviceFamily = "";
                    string productID = "";
                    //fixme
                    //string version = "";
                    DateTime buildDate;
                    string specificationRevision = "";

                    if (ApplicationLayers.identity != null)
                    {
                        if (ApplicationLayers.identity.vendorID != null)
                        {
                            vendorID = ApplicationLayers.identity.vendorID.Value;
                        }

                        if (ApplicationLayers.identity.deviceFamily != null)
                        {
                            //deviceFamily = ApplicationLayers.identity.deviceFamily.Items[]
                            //not really sure how to handle this. its a list of g_labels
                            //these contain label, description, language, URi etc could do with a simple class
                            //to wrap these in as they are used in a number of places
                        }

                        if (ApplicationLayers.identity.productID != null)
                        {
                            productID = ApplicationLayers.identity.productID.Value;
                        }

                        if (ApplicationLayers.identity.buildDate != default)
                        {
                            buildDate = ApplicationLayers.identity.buildDate;
                        }

                        if (ApplicationLayers.identity.specificationRevision != null)
                        {
                            specificationRevision = ApplicationLayers.identity.specificationRevision.Value;
                        }

                    }

                    if (ApplicationLayers.dummyUsage != null)
                    {
                        foreach (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummy dummy in ApplicationLayers.dummyUsage)
                        {
                            string pat = @"Dummy([0-9]{4})([0-1])";
                            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                            Match m = r.Match(dummy.entry.ToString());


                            if (m.Success)
                            {


                                int index = int.Parse(m.Groups[1].Value);

                                switch (index)
                                {
                                    case 1:
                                        eds.du.Dummy0001 = int.Parse(m.Groups[2].Value) == 1;
                                        break;
                                    case 2:
                                        eds.du.Dummy0002 = int.Parse(m.Groups[2].Value) == 1;
                                        break;
                                    case 3:
                                        eds.du.Dummy0003 = int.Parse(m.Groups[2].Value) == 1;
                                        break;
                                    case 4:
                                        eds.du.Dummy0004 = int.Parse(m.Groups[2].Value) == 1;
                                        break;
                                    case 5:
                                        eds.du.Dummy0005 = int.Parse(m.Groups[2].Value) == 1;
                                        break;
                                    case 6:
                                        eds.du.Dummy0006 = int.Parse(m.Groups[2].Value) == 1;
                                        break;
                                    case 7:
                                        eds.du.Dummy0007 = int.Parse(m.Groups[2].Value) == 1;
                                        break;

                                }

                            }

                        }

                    } //dummyusage != null

                    if (ApplicationLayers.dynamicChannels != null)
                    {

                    }

                    if (ApplicationLayers.conformanceClass != null)
                    {

                    }

                    if (ApplicationLayers.communicationEntityType != null)
                    {

                    }

                } //application layer

                if (TransportLayers != null)
                {
                    if(TransportLayers.PhysicalLayer!=null)
                    {
                        if (TransportLayers.PhysicalLayer.baudRate != null)
                        {
                            if (TransportLayers.PhysicalLayer.baudRate.supportedBaudRate != null)
                            {
                                foreach (ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate baud in TransportLayers.PhysicalLayer.baudRate.supportedBaudRate)
                                {

                                    if (baud.value.ToString() == "Item10Kbps")
                                        eds.di.BaudRate_10 = true;
                                    if (baud.value.ToString() == "Item20Kbps")
                                        eds.di.BaudRate_20 = true;
                                    if (baud.value.ToString() == "Item50Kbps")
                                        eds.di.BaudRate_50 = true;
                                    if (baud.value.ToString() == "Item125Kbps")
                                        eds.di.BaudRate_125 = true;
                                    if (baud.value.ToString() == "Item250Kbps")
                                        eds.di.BaudRate_250 = true;
                                    if (baud.value.ToString() == "Item500Kbps")
                                        eds.di.BaudRate_500 = true;
                                    if (baud.value.ToString() == "Item800Kbps")
                                        eds.di.BaudRate_800 = true;
                                    if (baud.value.ToString() == "Item1000Kbps")
                                        eds.di.BaudRate_1000 = true;

                                    //fixme "auto-baudRate" is a valid identifier here as well


                                }
                            }
                        }
                    }


                } //Transport layer

                eds.di.LSS_Supported = false;
                eds.di.LSS_Master = false;

                if (NetworkManagment != null)
                {
                    if (NetworkManagment.CANopenMasterFeatures != null)
                    {
                        eds.di.SimpleBootUpMaster = NetworkManagment.CANopenMasterFeatures.bootUpMaster;

                        //fixme Extra items
                        //NetworkManagment.CANopenMasterFeatures.configurationManager;
                        //NetworkManagment.CANopenMasterFeatures.flyingMaster;

                        //Fix me if Client and Server are set in XDD i can't deal with this and will default to Server
                        if (NetworkManagment.CANopenMasterFeatures.layerSettingServiceMaster)
                        {
                            eds.di.LSS_Master = true;
                        }

                        //NetworkManagment.CANopenMasterFeatures.SDOManager;
                    }

                    if (NetworkManagment.CANopenGeneralFeatures != null)
                    {
                        eds.di.SimpleBootUpSlave = NetworkManagment.CANopenGeneralFeatures.bootUpSlave;
                        eds.di.DynamicChannelsSupported = NetworkManagment.CANopenGeneralFeatures.dynamicChannels > 0;
                        //fix me count of dynamic channels not handled yet eds only has bool

                        eds.di.Granularity = NetworkManagment.CANopenGeneralFeatures.granularity;
                        eds.di.GroupMessaging = NetworkManagment.CANopenGeneralFeatures.groupMessaging;

                        //Fix me if Client and Server are set in XDD i can't deal with this and will default to Server
                        if (NetworkManagment.CANopenGeneralFeatures.layerSettingServiceSlave)
                        {
                            eds.di.LSS_Supported = true;
                        }

                        eds.di.NG_Master = NetworkManagment.CANopenGeneralFeatures.ngMaster;
                        eds.di.NG_Slave = NetworkManagment.CANopenGeneralFeatures.ngSlave;
                        eds.di.NrOfNG_MonitoredNodes = NetworkManagment.CANopenGeneralFeatures.NrOfNG_MonitoredNodes;

                        eds.di.NrOfRXPDO = NetworkManagment.CANopenGeneralFeatures.nrOfRxPDO;
                        eds.di.NrOfTXPDO = NetworkManagment.CANopenGeneralFeatures.nrOfTxPDO;

                        //fixme extra items
                        //NetworkManagment.CANopenGeneralFeatures.SDORequestingDevice;
                        //NetworkManagment.CANopenGeneralFeatures.selfStartingDevice;

                    }

                    if (NetworkManagment.deviceCommissioning != null)
                    {
                        eds.dc.NodeID = NetworkManagment.deviceCommissioning.NodeID;
                        eds.dc.Baudrate = Convert.ToUInt16(NetworkManagment.deviceCommissioning.actualBaudRate);
                        eds.dc.CANopenManager = NetworkManagment.deviceCommissioning.CANopenManager;
                        eds.dc.NetworkName = NetworkManagment.deviceCommissioning.networkName;
                        eds.dc.NetNumber = Convert.ToUInt16(NetworkManagment.deviceCommissioning.networkNumber);
                        eds.dc.NodeName = NetworkManagment.deviceCommissioning.nodeName;

                    }
                }

                if (ApplicationLayers.CANopenObjectList.CANopenObject != null)
                    foreach (CanOpenXSD_1_0.CANopenObjectListCANopenObject obj3 in ApplicationLayers.CANopenObjectList.CANopenObject)
                    {
                        ODentry entry = new ODentry();

                        UInt16 index;

                        if (obj3.index != null)
                        {
                            index = (UInt16)EDSsharp.ConvertToUInt16(obj3.index);
                            entry.Index = index;
                        }
                        else
                            continue; //unparseable

                        if (obj3.name != null)
                            entry.parameter_name = obj3.name;

                        entry.objecttype = (ObjectType)obj3.objectType;

                        if (obj3.dataType != null)
                            entry.datatype = (DataType)EDSsharp.ConvertToUInt16(obj3.dataType);

                        if (obj3.defaultValue != null)
                            entry.defaultvalue = obj3.defaultValue;

                        if (obj3.highLimit != null)
                            entry.HighLimit = obj3.highLimit;

                        if (obj3.lowLimit != null)
                            entry.LowLimit = obj3.lowLimit;

                        if (obj3.actualValue != null)
                            entry.actualvalue = obj3.actualValue;

                        if (obj3.denotation != null)
                            entry.denotation = obj3.denotation;

                        if (obj3.edseditor_extenstion_storagelocation != null)
                        {
                            string sl = obj3.edseditor_extenstion_storagelocation;
                            entry.prop.CO_storageGroup = sl;
                            eds.CO_storageGroups.Add(sl);
                        }

                        if (obj3.edseditor_extension_notifyonchange)
                            entry.prop.CO_flagsPDO = obj3.edseditor_extension_notifyonchange;

                            

                        //FIXME im not sure this is correct
                        if (obj3.objFlags != null)
                            entry.ObjFlags = obj3.objFlags[0];

                        entry.uniqueID = obj3.uniqueIDRef;

                        // https://github.com/robincornelius/libedssharp/issues/128
                        // Mapping of accesstype and pdo mappings have changed between EDS and XDD
                        // in EDS we have rw,wo, r and const which are the same in both standards, but EDS also
                        // has rww and rwr which are rw objects that can also be mapped to RPDOs (rww) or
                        // TPDOs (rwr) 

                        if (obj3.accessTypeSpecified)
                        {
                            entry.accesstype = (EDSsharp.AccessType)Enum.Parse(typeof(EDSsharp.AccessType), obj3.accessType.ToString());
                        }
                        else
                        {
                            entry.accesstype = EDSsharp.AccessType.ro; //fixme sensible default required here??    
                        }

                        if(obj3.PDOmappingSpecified)
                        {

                            entry.PDOtype = (PDOMappingType)Enum.Parse(typeof(PDOMappingType), obj3.PDOmapping.ToString());

                            if (entry.accesstype == EDSsharp.AccessType.rw && entry.PDOtype == PDOMappingType.RPDO)
                            {
                                entry.accesstype = EDSsharp.AccessType.rww;
                            }

                            if (entry.accesstype == EDSsharp.AccessType.rw && entry.PDOtype == PDOMappingType.TPDO)
                            {
                                entry.accesstype = EDSsharp.AccessType.rwr;
                            }
                        }
                        else
                        {
                            entry.PDOtype = PDOMappingType.no; //fixme should this be @default??
                        }

                        eds.ods.Add(index, entry);

                        if (obj3.CANopenSubObject != null)
                        {
                            foreach (CanOpenXSD_1_0.CANopenObjectListCANopenObjectCANopenSubObject subobj in obj3.CANopenSubObject)
                            {

                                DataType datatype;
                                EDSsharp.AccessType accesstype = EDSsharp.AccessType.ro; //fixme sensible default?
                                PDOMappingType pdotype = PDOMappingType.no;

                                if (subobj.dataType != null)
                                {
                                    datatype = (DataType)EDSsharp.ConvertToUInt16(subobj.dataType);
                                }
                                else
                                {
                                    datatype = entry.datatype;
                                }


                       
                                // https://github.com/robincornelius/libedssharp/issues/128
                                // Mapping of accesstype and pdo mappings have changed between EDS and XDD
                                // in EDS we have rw,wo, r and const which are the same in both standards, but EDS also
                                // has rww and rwr which are rw objects that can also be mapped to RPDOs (rww) or
                                // TPDOs (rwr) 


                                if (subobj.accessTypeSpecified == true)
                                {
                                    accesstype = (EDSsharp.AccessType)Enum.Parse(typeof(EDSsharp.AccessType), subobj.accessType.ToString());
                                }
                                else
                                {
                                    accesstype = entry.accesstype;
                                }
                           
                                if (subobj.PDOmappingSpecified == true)
                                {

                                    pdotype = (PDOMappingType)Enum.Parse(typeof(PDOMappingType), subobj.PDOmapping.ToString());

                                    if(accesstype == EDSsharp.AccessType.rw && pdotype == PDOMappingType.RPDO)
                                    {
                                        accesstype = EDSsharp.AccessType.rww;
                                    }

                                    if (accesstype == EDSsharp.AccessType.rw && pdotype == PDOMappingType.TPDO)
                                    {
                                        accesstype = EDSsharp.AccessType.rwr;
                                    }

                                }
                                else
                                {
                                    pdotype = entry.PDOtype;
                                }


                                ODentry subentry = new ODentry(subobj.name, index, datatype, subobj.defaultValue, accesstype, pdotype, entry);


                                //extra items

                                subentry.prop.CO_flagsPDO = subobj.edseditor_extension_notifyonchange;

                                if (subobj.lowLimit!=null)
                                    subentry.LowLimit = subobj.lowLimit;

                                if(subobj.highLimit!=null)
                                    subentry.HighLimit = subobj.highLimit;

                                if(subobj.actualValue!=null)
                                    subentry.actualvalue = subobj.actualValue;

                                if(subobj.denotation!=null)
                                    subentry.denotation = subobj.denotation;

                                if(subobj.objFlags!=null)
                                    subentry.ObjFlags = subobj.objFlags[0];


                                subentry.uniqueID = subobj.uniqueIDRef;

                                entry.subobjects.Add(subobj.subIndex[subobj.subIndex.Length - 1], subentry);

                            }
                        }


                }

            }

            //Process Device after network so we already have the ODEntries populated then can match bu uniqueID

            //ProfileBody_Device_CANopen
            if (body_device != null)
            {
                ProfileBody_Device_CANopen obj = body_device;

                if (obj.DeviceIdentity != null)
                {
                    eds.di.ProductName = obj.DeviceIdentity.productName.Value;
                    eds.di.ProductNumber = obj.DeviceIdentity.productID.Value;
                    eds.di.VendorName = obj.DeviceIdentity.vendorName.Value;
                    eds.di.VendorNumber = obj.DeviceIdentity.vendorID.Value;

                    foreach (object o in obj.DeviceIdentity.productText.Items)
                    {
                        //this is another g_label affair

                        if (o.GetType() == typeof(vendorTextDescription))
                        {
                            String desc = ((vendorTextDescription)o).Value;
                            string[] bits = desc.Split('|');

                            foreach(string bit in bits)
                            {
                                string[] keyvalue = bit.Split('=');
                                if(keyvalue.Length==2)
                                {
                                    switch(keyvalue[0])
                                    {
                                        case "FileDescription":
                                            eds.fi.Description = keyvalue[1];
                                            break;
                                        case "EdsVersion":
                                            eds.fi.EDSVersion = keyvalue[1];
                                            break;
                                        case "FileRevision":
                                            eds.fi.FileVersion = keyvalue[1];
                                            break;
                                        case "RevisionNum":
                                            byte.TryParse(keyvalue[1], out eds.fi.FileRevision);                                            break;

                                                
                                    }
                                }
                            }
                        }

                        if (o.GetType() == typeof(vendorTextDescriptionRef))
                        {
                        }
                        if (o.GetType() == typeof(vendorTextLabel))
                        {
                        }
                        if (o.GetType() == typeof(vendorTextLabelRef))
                        {
                        }
                    }

                    //fixme i think date should be tested in a separate way
                    //as dates are supported without times
                    if (obj.fileCreationTimeSpecified)
                    {
                        eds.fi.CreationDateTime = obj.fileCreationDate.Add(obj.fileCreationTime.TimeOfDay);
                        eds.fi.CreationDate = eds.fi.CreationDateTime.ToString("MM-dd-yyyy");
                        eds.fi.CreationTime = eds.fi.CreationDateTime.ToString("h:mmtt");

                    }

                    if (obj.fileModificationDateSpecified)
                    {
                        eds.fi.ModificationDateTime = obj.fileModificationDate.Add(obj.fileCreationTime.TimeOfDay);
                        eds.fi.ModificationDate = eds.fi.ModificationDateTime.ToString("MM-dd-yyyy");
                        eds.fi.ModificationTime = eds.fi.ModificationDateTime.ToString("h:mmtt");

                    }

                    eds.fi.ModifiedBy = obj.fileModifiedBy;
                    eds.fi.CreatedBy = obj.fileCreator;
                }

                if (obj.DeviceManager != null)
                {

                }

                if (obj.DeviceFunction != null)
                {

                }

                if (obj.ApplicationProcess != null)
                {

                    if (obj.ApplicationProcess[0] != null)
                    {
                        foreach (parameter param in obj.ApplicationProcess[0].parameterList)
                        {

                            //match unique ID


                            ODentry od = eds.Getobject(param.uniqueID);

                            if (od == null)
                                continue;

                            //fix me defaultValue contains other stuff we might want
                            if (param.defaultValue != null)
                                od.defaultvalue = param.defaultValue.value;

                            //fix me, if more than one vendorTextDescription is present, eg
                            //multi language this will result in the last one being used
                            if (param.Items!=null && param.Items.Length>0)
                            {
                                foreach(object item in param.Items)
                                {
                                    if(item.GetType() == typeof(vendorTextDescription))
                                    {
                                        vendorTextDescription vtd = (vendorTextDescription)item;
                                        od.Description = vtd.Value;
                                    }

                                }

                            }

                            //FIXME: if we have a denotation set for an object in the <parameterList> section but it is not set on the object
                            //use the <parameterList> one. We may discover that this is used for something else and can be removed??
                            if ((od.denotation==null || od.denotation=="") && param.denotation!=null && param.denotation.Items.Length>0)
                            {
                                foreach (object item in param.denotation.Items)
                                {
                                    if (item.GetType() == typeof(vendorTextLabel))
                                    {
                                        vendorTextLabel vtd = (vendorTextLabel)item;
                                        od.denotation = vtd.Value;
                                    }
                                }
                            }





                        }

                    }





                }

            }

            return eds;

        }

    }

}

[XmlRoot(ElementName = "OpenEDSProject")]
public class OpenEDSProject
{
    [XmlElement(ElementName = "ISO15745ProfileContainer", Namespace = "http://www.canopen.org/xml/1.0")]
    public List<ISO15745ProfileContainer> ISO15745ProfileContainer { get; set; }
    [XmlAttribute(AttributeName = "version")]
    public string Version { get; set; }

}
