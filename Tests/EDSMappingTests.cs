using Google.Protobuf.WellKnownTypes;
using LibCanOpen;
using libEDSsharp;
using System;
using System.Globalization;
using Xunit;

namespace Tests
{
    public class EDSMappingTests
    {
        [Fact]
        public void Test_ToProtobufferAssertConfig()
        {
            var eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };
            var od = new ODentry
            {
                objecttype = ObjectType.VAR,
                parameter_name = "Test REC",
                Index = 0x2000
            };

            var sub = new ODentry
            {
                parameter_name = "some value",
                datatype = DataType.UNSIGNED8,
                parent = od,
                accesstype = EDSsharp.AccessType.ro,
                defaultvalue = "1",
                PDOtype = PDOMappingType.no,
                objecttype = ObjectType.VAR
            };

            od.subobjects.Add(0x00, sub);
            eds.ods.Add(0x2000, od);

            //Assert is called inside the map function
            MappingEDS.MapToProtobuffer(eds);
        }
        [Fact]
        public void Test_ToProtobufferFileInfo()
        {
            var eds = new EDSsharp
            {
                fi = new FileInfo
                {
                    CreatedBy = "CreatedBy",
                    CreationDate = "01-20-2000",
                    CreationTime = "12:20am",
                    Description = "Description",
                    FileRevision = (byte)'A',
                    FileVersion = "1.0.0",
                    ModificationDate = "02-10-1000",
                    ModificationTime = "12:20pm",
                    ModifiedBy = "ModifiedBy"
                }
            };

            var creationDateTime = DateTime.ParseExact($"{eds.fi.CreationTime} {eds.fi.CreationDate}", "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
            var modificationDateTime = DateTime.ParseExact($"{eds.fi.ModificationTime} {eds.fi.ModificationDate}", "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
            var creationTimestamp = Timestamp.FromDateTime(creationDateTime.ToUniversalTime());
            var modificationTimestamp = Timestamp.FromDateTime(modificationDateTime.ToUniversalTime());

            var tmp = MappingEDS.MapToProtobuffer(eds);
            Assert.Equal(eds.fi.CreatedBy, tmp.FileInfo.CreatedBy);
            Assert.Equal(creationTimestamp, tmp.FileInfo.CreationTime);
            Assert.Equal(eds.fi.Description, tmp.FileInfo.Description);
            Assert.Equal(eds.fi.FileVersion, tmp.FileInfo.FileVersion);
            Assert.Equal(eds.fi.ModifiedBy, tmp.FileInfo.ModifiedBy);
            Assert.Equal(modificationTimestamp, tmp.FileInfo.ModificationTime);
        }
        [Fact]
        public void Test_ToProtobufferDeviceInfo()
        {
            var eds = new EDSsharp
            {
                di = new DeviceInfo
                {
                    BaudRate_10 = true,
                    BaudRate_20 = false,
                    BaudRate_50 = true,
                    BaudRate_125 = false,
                    BaudRate_250 = true,
                    BaudRate_500 = false,
                    BaudRate_800 = true,
                    BaudRate_1000 = false,
                    BaudRate_auto = true,
                    LSS_Master = false,
                    LSS_Supported = true,
                    VendorName = "VendorName",
                    ProductName = "ProductName"
                }
            };

            var tmp = MappingEDS.MapToProtobuffer(eds);
            Assert.Equal(eds.di.BaudRate_10, tmp.DeviceInfo.BaudRate10);
            Assert.Equal(eds.di.BaudRate_20, tmp.DeviceInfo.BaudRate20);
            Assert.Equal(eds.di.BaudRate_50, tmp.DeviceInfo.BaudRate50);
            Assert.Equal(eds.di.BaudRate_125, tmp.DeviceInfo.BaudRate125);
            Assert.Equal(eds.di.BaudRate_250, tmp.DeviceInfo.BaudRate250);
            Assert.Equal(eds.di.BaudRate_500, tmp.DeviceInfo.BaudRate500);
            Assert.Equal(eds.di.BaudRate_800, tmp.DeviceInfo.BaudRate800);
            Assert.Equal(eds.di.BaudRate_1000, tmp.DeviceInfo.BaudRate1000);
            Assert.Equal(eds.di.BaudRate_auto, tmp.DeviceInfo.BaudRateAuto);
            Assert.Equal(eds.di.LSS_Master, tmp.DeviceInfo.LssMaster);
            Assert.Equal(eds.di.LSS_Supported, tmp.DeviceInfo.LssSlave);
            Assert.Equal(eds.di.VendorName, tmp.DeviceInfo.VendorName);
            Assert.Equal(eds.di.ProductName, tmp.DeviceInfo.ProductName);
        }
        [Fact]
        public void Test_ToProtobufferDeviceCommissioning()
        {
            var eds = new EDSsharp
            {
                dc = new DeviceCommissioning
                {
                    NodeID = 123,
                    NodeName = "NodeName",
                    Baudrate = 456,
                }
            };

            var tmp = MappingEDS.MapToProtobuffer(eds);
            Assert.Equal(eds.dc.NodeID, tmp.DeviceCommissioning.NodeId);
            Assert.Equal(eds.dc.NodeName, tmp.DeviceCommissioning.NodeName);
            Assert.Equal(eds.dc.Baudrate, tmp.DeviceCommissioning.Baudrate);
        }
        [Theory]
        [InlineData(OdObject.Types.ObjectType.Array, ObjectType.ARRAY)]
        [InlineData(OdObject.Types.ObjectType.Record, ObjectType.RECORD)]
        [InlineData(OdObject.Types.ObjectType.Var, ObjectType.VAR)]
        [InlineData(OdObject.Types.ObjectType.Unspecified, ObjectType.DEFSTRUCT)]
        [InlineData(OdObject.Types.ObjectType.Unspecified, ObjectType.DEFTYPE)]
        [InlineData(OdObject.Types.ObjectType.Unspecified, ObjectType.DOMAIN)]
        [InlineData(OdObject.Types.ObjectType.Unspecified, ObjectType.NULL)]
        [InlineData(OdObject.Types.ObjectType.Unspecified, ObjectType.UNKNOWN)]
        public void Test_ToProtobufferODObject(OdObject.Types.ObjectType objTypeProto, ObjectType objTypeEDS)
        {
            var eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };
            var od = new ODentry
            {
                objecttype = objTypeEDS,
                parameter_name = "parameter name",
                Index = 0x2000,
                denotation = "denotation",
            };
            eds.ods.Add(od.Index, od);
            var tmp = MappingEDS.MapToProtobuffer(eds);
            Assert.Equal(objTypeProto, tmp.Objects[od.Index.ToString()].ObjectType);
            Assert.Equal(od.denotation, tmp.Objects[od.Index.ToString()].Alias);
        }

        [Theory]
        [InlineData(OdSubObject.Types.DataType.Unspecified, DataType.UNKNOWN)]
        [InlineData(OdSubObject.Types.DataType.Boolean, DataType.BOOLEAN)]
        [InlineData(OdSubObject.Types.DataType.Integer8, DataType.INTEGER8)]
        [InlineData(OdSubObject.Types.DataType.Integer16, DataType.INTEGER16)]
        [InlineData(OdSubObject.Types.DataType.Integer32, DataType.INTEGER32)]
        [InlineData(OdSubObject.Types.DataType.Unsigned8, DataType.UNSIGNED8)]
        [InlineData(OdSubObject.Types.DataType.Unsigned16, DataType.UNSIGNED16)]
        [InlineData(OdSubObject.Types.DataType.Unsigned32, DataType.UNSIGNED32)]
        [InlineData(OdSubObject.Types.DataType.Real32, DataType.REAL32)]
        [InlineData(OdSubObject.Types.DataType.VisibleString, DataType.VISIBLE_STRING)]
        [InlineData(OdSubObject.Types.DataType.OctetString, DataType.OCTET_STRING)]
        [InlineData(OdSubObject.Types.DataType.UnicodeString, DataType.UNICODE_STRING)]
        [InlineData(OdSubObject.Types.DataType.TimeOfDay, DataType.TIME_OF_DAY)]
        [InlineData(OdSubObject.Types.DataType.TimeDifference, DataType.TIME_DIFFERENCE)]
        [InlineData(OdSubObject.Types.DataType.Domain, DataType.DOMAIN)]
        [InlineData(OdSubObject.Types.DataType.Integer24, DataType.INTEGER24)]
        [InlineData(OdSubObject.Types.DataType.Real64, DataType.REAL64)]
        [InlineData(OdSubObject.Types.DataType.Integer40, DataType.INTEGER40)]
        [InlineData(OdSubObject.Types.DataType.Integer48, DataType.INTEGER48)]
        [InlineData(OdSubObject.Types.DataType.Integer56, DataType.INTEGER56)]
        [InlineData(OdSubObject.Types.DataType.Integer64, DataType.INTEGER64)]
        [InlineData(OdSubObject.Types.DataType.Unsigned24, DataType.UNSIGNED24)]
        [InlineData(OdSubObject.Types.DataType.Unsigned40, DataType.UNSIGNED40)]
        [InlineData(OdSubObject.Types.DataType.Unsigned48, DataType.UNSIGNED48)]
        [InlineData(OdSubObject.Types.DataType.Unsigned56, DataType.UNSIGNED56)]
        [InlineData(OdSubObject.Types.DataType.Unsigned64, DataType.UNSIGNED64)]
        public void Test_ToProtobufferSubODObjectDatatype(OdSubObject.Types.DataType datatypeProto, DataType datatypeEDS)
        {
            var eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };
            var od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                Index = 0x2000
            };
            var sub = new ODentry
            {
                datatype = datatypeEDS,
                parent = od,
            };

            od.subobjects.Add(0x00, sub);
            eds.ods.Add(od.Index, od);
            var tmp = MappingEDS.MapToProtobuffer(eds);
            Assert.Equal(datatypeProto, tmp.Objects[od.Index.ToString()].SubObjects["0"].DataType);
        }

        [Theory]
        [InlineData(OdSubObject.Types.AccessPDO.Tr, OdSubObject.Types.AccessSDO.Rw, EDSsharp.AccessType.rw)]
        [InlineData(OdSubObject.Types.AccessPDO.No, OdSubObject.Types.AccessSDO.Ro, EDSsharp.AccessType.ro)]
        [InlineData(OdSubObject.Types.AccessPDO.No, OdSubObject.Types.AccessSDO.Wo, EDSsharp.AccessType.wo)]
        [InlineData(OdSubObject.Types.AccessPDO.T, OdSubObject.Types.AccessSDO.Rw, EDSsharp.AccessType.rwr)]
        [InlineData(OdSubObject.Types.AccessPDO.R, OdSubObject.Types.AccessSDO.Rw, EDSsharp.AccessType.rww)]
        [InlineData(OdSubObject.Types.AccessPDO.R, OdSubObject.Types.AccessSDO.Ro, EDSsharp.AccessType.@const)]
        [InlineData(OdSubObject.Types.AccessPDO.No, OdSubObject.Types.AccessSDO.No, EDSsharp.AccessType.UNKNOWN)]
        public void Test_ToProtobufferSubODObjectAccesstype(OdSubObject.Types.AccessPDO accessPDOProto, OdSubObject.Types.AccessSDO accessSDOProto, EDSsharp.AccessType datatypeEDS)
        {
            var eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };
            var od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                Index = 0x2000
            };
            var sub = new ODentry
            {
                parent = od,
                accesstype = datatypeEDS,
                PDOtype = PDOMappingType.no,
            };

            od.subobjects.Add(0x00, sub);
            eds.ods.Add(od.Index, od);
            var tmp = MappingEDS.MapToProtobuffer(eds);
            Assert.Equal(accessPDOProto, tmp.Objects[od.Index.ToString()].SubObjects["0"].Pdo);
            Assert.Equal(accessSDOProto, tmp.Objects[od.Index.ToString()].SubObjects["0"].Sdo);
        }
        [Fact]
        public void Test_ToProtobufferSubODObjectMembers()
        {
            var eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };
            var od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                Index = 0x2000
            };
            var sub = new ODentry
            {
                parent = od,
                actualvalue = "123",
                parameter_name = "parameter_name",
                HighLimit = "HighLimit",
                LowLimit = "LowLimit",
                defaultvalue = "defaultvalue",
            };

            od.subobjects.Add(0x00, sub);
            eds.ods.Add(od.Index, od);
            var tmp = MappingEDS.MapToProtobuffer(eds);
            Assert.Equal(sub.actualvalue, tmp.Objects[od.Index.ToString()].SubObjects["0"].ActualValue);
            Assert.Equal(sub.parameter_name, tmp.Objects[od.Index.ToString()].SubObjects["0"].Name);
            Assert.Equal(sub.HighLimit, tmp.Objects[od.Index.ToString()].SubObjects["0"].HighLimit);
            Assert.Equal(sub.LowLimit, tmp.Objects[od.Index.ToString()].SubObjects["0"].LowLimit);
            Assert.Equal(sub.defaultvalue, tmp.Objects[od.Index.ToString()].SubObjects["0"].DefaultValue);
        }
        [Fact]
        public void Test_ToProtobufferODObject_CustomProperties()
        {
            var eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };
            var od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                Index = 0x2000,
            };
            od.prop.CO_disabled = true;
            od.prop.CO_countLabel = "CO_countLabel";
            od.prop.CO_storageGroup = "CO_storageGroup";
            od.prop.CO_flagsPDO = true;

            eds.ods.Add(od.Index, od);
            var tmp = MappingEDS.MapToProtobuffer(eds);

            Assert.Equal(od.prop.CO_disabled, tmp.Objects[od.Index.ToString()].Disabled);
            Assert.Equal(od.prop.CO_countLabel, tmp.Objects[od.Index.ToString()].CountLabel);
            Assert.Equal(od.prop.CO_storageGroup, tmp.Objects[od.Index.ToString()].StorageGroup);
            Assert.Equal(od.prop.CO_flagsPDO, tmp.Objects[od.Index.ToString()].FlagsPDO);
        }
        [Theory]
        [InlineData(OdSubObject.Types.AccessSRDO.No, AccessSRDO.no)]
        [InlineData(OdSubObject.Types.AccessSRDO.Rx, AccessSRDO.rx)]
        [InlineData(OdSubObject.Types.AccessSRDO.Trx, AccessSRDO.trx)]
        [InlineData(OdSubObject.Types.AccessSRDO.Tx, AccessSRDO.tx)]
        public void Test_ToProtobufferSubODObject_CustomProperties(OdSubObject.Types.AccessSRDO accessSRDO, AccessSRDO co_prop)
        {
            var eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };
            var od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                Index = 0x2000
            };
            var sub = new ODentry
            {
                parent = od,
            };
            sub.prop.CO_accessSRDO = co_prop;
            sub.prop.CO_stringLengthMin = 123;

            od.subobjects.Add(0x00, sub);
            eds.ods.Add(od.Index, od);
            var tmp = MappingEDS.MapToProtobuffer(eds);

            Assert.Equal(accessSRDO, tmp.Objects[od.Index.ToString()].SubObjects["0"].Srdo);
            Assert.Equal(sub.prop.CO_stringLengthMin, tmp.Objects[od.Index.ToString()].SubObjects["0"].StringLengthMin);
        }

        [Fact]
        public void Test_FromProtobufferAssertConfig()
        {
            var d = new CanOpenDevice { };

            var od = new OdObject
            {
                ObjectType = OdObject.Types.ObjectType.Var,
                Name = "Test VAR",
            };

            var sub = new OdSubObject
            {
                Name = "some value",
                DataType = OdSubObject.Types.DataType.Unsigned8,
                Pdo = OdSubObject.Types.AccessPDO.T,
                Sdo = OdSubObject.Types.AccessSDO.Ro,
                DefaultValue = "1",
            };
            od.SubObjects.Add("0", sub);
            d.Objects.Add("0x2000", od);

            //Assert is called inside the map function
            MappingEDS.MapFromProtobuffer(d);
        }
        [Fact]
        public void Test_FromProtobufferFileInfo()
        {
            var d = new CanOpenDevice
            {
                FileInfo = new CanOpen_FileInfo()
            };
            var CreationTime = DateTime.ParseExact($"11:22AM 11-22-1234", "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
            var ModificationTime = DateTime.ParseExact($"11:22AM 11-22-1234", "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);

            d.FileInfo.FileVersion = "FileVersion";
            d.FileInfo.Description = "Description";
            d.FileInfo.CreationTime = Timestamp.FromDateTime(CreationTime.ToUniversalTime());
            d.FileInfo.ModificationTime = Timestamp.FromDateTime(ModificationTime.ToUniversalTime());
            d.FileInfo.ModifiedBy = "ModifiedBy";

            var tmp = MappingEDS.MapFromProtobuffer(d);
            Assert.Equal(d.FileInfo.CreatedBy, tmp.fi.CreatedBy);
            Assert.Equal(d.FileInfo.CreationTime.ToDateTime().ToString("h:mmtt"), tmp.fi.CreationTime);
            Assert.Equal(d.FileInfo.CreationTime.ToDateTime().ToString("MM-dd-yyyy"), tmp.fi.CreationDate);
            Assert.Equal(d.FileInfo.Description, tmp.fi.Description);
            Assert.Equal(d.FileInfo.FileVersion, tmp.fi.FileVersion);
            Assert.Equal(d.FileInfo.ModifiedBy, tmp.fi.ModifiedBy);
            Assert.Equal(d.FileInfo.ModificationTime.ToDateTime().ToString("h:mmtt"), tmp.fi.ModificationTime);
            Assert.Equal(d.FileInfo.ModificationTime.ToDateTime().ToString("MM-dd-yyyy"), tmp.fi.ModificationDate);

            //Assert is called inside the map function
            MappingEDS.MapFromProtobuffer(d);
        }
        [Fact]
        public void Test_FromProtobufferDeviceInfo()
        {
            var d = new CanOpenDevice
            {
                DeviceInfo = new CanOpen_DeviceInfo()
                {
                    BaudRate10 = true,
                    BaudRate20 = false,
                    BaudRate50 = true,
                    BaudRate125 = false,
                    BaudRate250 = true,
                    BaudRate500 = false,
                    BaudRate800 = true,
                    BaudRate1000 = false,
                    BaudRateAuto = true,
                    LssMaster = false,
                    LssSlave = true,
                    VendorName = "VendorName",
                    ProductName = "ProductName"
                }
            };

            var tmp = MappingEDS.MapFromProtobuffer(d);
            Assert.Equal(d.DeviceInfo.BaudRate10, tmp.di.BaudRate_10);
            Assert.Equal(d.DeviceInfo.BaudRate20, tmp.di.BaudRate_20);
            Assert.Equal(d.DeviceInfo.BaudRate50, tmp.di.BaudRate_50);
            Assert.Equal(d.DeviceInfo.BaudRate125, tmp.di.BaudRate_125);
            Assert.Equal(d.DeviceInfo.BaudRate250, tmp.di.BaudRate_250);
            Assert.Equal(d.DeviceInfo.BaudRate500, tmp.di.BaudRate_500);
            Assert.Equal(d.DeviceInfo.BaudRate800, tmp.di.BaudRate_800);
            Assert.Equal(d.DeviceInfo.BaudRate1000, tmp.di.BaudRate_1000);
            Assert.Equal(d.DeviceInfo.BaudRateAuto, tmp.di.BaudRate_auto);
            Assert.Equal(d.DeviceInfo.LssMaster, tmp.di.LSS_Master);
            Assert.Equal(d.DeviceInfo.LssSlave, tmp.di.LSS_Supported);
            Assert.Equal(d.DeviceInfo.VendorName, tmp.di.VendorName);
            Assert.Equal(d.DeviceInfo.ProductName, tmp.di.ProductName);
        }

        [Fact]
        public void Test_FromProtobufferDeviceCommissioning()
        {
            var d = new CanOpenDevice
            {
                DeviceCommissioning = new CanOpen_DeviceCommissioning
                {
                    Baudrate = 456,
                    NodeId = 123,
                    NodeName = "NodeName"

                }
            };

            var tmp = MappingEDS.MapFromProtobuffer(d);
            Assert.Equal(d.DeviceCommissioning.NodeId, tmp.dc.NodeID);
            Assert.Equal(d.DeviceCommissioning.NodeName, tmp.dc.NodeName);
            Assert.Equal(d.DeviceCommissioning.Baudrate, tmp.dc.Baudrate);
        }

        [Theory]
        [InlineData(OdObject.Types.ObjectType.Array, ObjectType.ARRAY)]
        [InlineData(OdObject.Types.ObjectType.Record, ObjectType.RECORD)]
        [InlineData(OdObject.Types.ObjectType.Var, ObjectType.VAR)]
        [InlineData(OdObject.Types.ObjectType.Unspecified, ObjectType.UNKNOWN)]
        public void Test_FromProtobufferODObject(OdObject.Types.ObjectType objTypeProto, ObjectType objTypeEDS)
        {
            ushort index = 0x2000;
            var d = new CanOpenDevice();
            var od = new OdObject
            {
                ObjectType = objTypeProto,
                Name = "Name",
                Alias = "alias",
            };
            d.Objects.Add(index.ToString(), od);
            var tmp = MappingEDS.MapFromProtobuffer(d);
            Assert.Equal(index, tmp.ods[index].Index);
            Assert.Equal(objTypeEDS, tmp.ods[index].objecttype);
            Assert.Equal(od.Name, tmp.ods[index].parameter_name);
            Assert.Equal(od.Alias, tmp.ods[index].denotation);
        }

        [Theory]
        [InlineData(OdSubObject.Types.DataType.Unspecified, DataType.UNKNOWN)]
        [InlineData(OdSubObject.Types.DataType.Boolean, DataType.BOOLEAN)]
        [InlineData(OdSubObject.Types.DataType.Integer8, DataType.INTEGER8)]
        [InlineData(OdSubObject.Types.DataType.Integer16, DataType.INTEGER16)]
        [InlineData(OdSubObject.Types.DataType.Integer32, DataType.INTEGER32)]
        [InlineData(OdSubObject.Types.DataType.Unsigned8, DataType.UNSIGNED8)]
        [InlineData(OdSubObject.Types.DataType.Unsigned16, DataType.UNSIGNED16)]
        [InlineData(OdSubObject.Types.DataType.Unsigned32, DataType.UNSIGNED32)]
        [InlineData(OdSubObject.Types.DataType.Real32, DataType.REAL32)]
        [InlineData(OdSubObject.Types.DataType.VisibleString, DataType.VISIBLE_STRING)]
        [InlineData(OdSubObject.Types.DataType.OctetString, DataType.OCTET_STRING)]
        [InlineData(OdSubObject.Types.DataType.UnicodeString, DataType.UNICODE_STRING)]
        [InlineData(OdSubObject.Types.DataType.TimeOfDay, DataType.TIME_OF_DAY)]
        [InlineData(OdSubObject.Types.DataType.TimeDifference, DataType.TIME_DIFFERENCE)]
        [InlineData(OdSubObject.Types.DataType.Domain, DataType.DOMAIN)]
        [InlineData(OdSubObject.Types.DataType.Integer24, DataType.INTEGER24)]
        [InlineData(OdSubObject.Types.DataType.Real64, DataType.REAL64)]
        [InlineData(OdSubObject.Types.DataType.Integer40, DataType.INTEGER40)]
        [InlineData(OdSubObject.Types.DataType.Integer48, DataType.INTEGER48)]
        [InlineData(OdSubObject.Types.DataType.Integer56, DataType.INTEGER56)]
        [InlineData(OdSubObject.Types.DataType.Integer64, DataType.INTEGER64)]
        [InlineData(OdSubObject.Types.DataType.Unsigned24, DataType.UNSIGNED24)]
        [InlineData(OdSubObject.Types.DataType.Unsigned40, DataType.UNSIGNED40)]
        [InlineData(OdSubObject.Types.DataType.Unsigned48, DataType.UNSIGNED48)]
        [InlineData(OdSubObject.Types.DataType.Unsigned56, DataType.UNSIGNED56)]
        [InlineData(OdSubObject.Types.DataType.Unsigned64, DataType.UNSIGNED64)]
        public void Test_FromProtobufferSubODObjectDatatype(OdSubObject.Types.DataType datatypeProto, DataType datatypeEDS)
        {
            ushort index = 0x2000;
            ushort subindex = 0x1;
            var d = new CanOpenDevice();
            var od = new OdObject
            {
                ObjectType = OdObject.Types.ObjectType.Record
            };
            var sub = new OdSubObject
            {
                DataType = datatypeProto,
            };

            od.SubObjects.Add(subindex.ToString(), sub);
            d.Objects.Add(index.ToString(), od);

            var tmp = MappingEDS.MapFromProtobuffer(d);

            Assert.Equal(datatypeEDS, tmp.ods[index].subobjects[subindex].datatype);
        }

        [Theory]
        [InlineData(OdSubObject.Types.AccessPDO.Tr, OdSubObject.Types.AccessSDO.Rw, EDSsharp.AccessType.rw)]
        [InlineData(OdSubObject.Types.AccessPDO.No, OdSubObject.Types.AccessSDO.Ro, EDSsharp.AccessType.ro)]
        [InlineData(OdSubObject.Types.AccessPDO.No, OdSubObject.Types.AccessSDO.Wo, EDSsharp.AccessType.wo)]
        [InlineData(OdSubObject.Types.AccessPDO.T, OdSubObject.Types.AccessSDO.Rw, EDSsharp.AccessType.rwr)]
        [InlineData(OdSubObject.Types.AccessPDO.R, OdSubObject.Types.AccessSDO.Rw, EDSsharp.AccessType.rww)]
        [InlineData(OdSubObject.Types.AccessPDO.R, OdSubObject.Types.AccessSDO.Ro, EDSsharp.AccessType.@const)]
        [InlineData(OdSubObject.Types.AccessPDO.No, OdSubObject.Types.AccessSDO.No, EDSsharp.AccessType.UNKNOWN)]
        public void Test_FromProtobufferSubODObjectAccesstype(OdSubObject.Types.AccessPDO accessPDOProto, OdSubObject.Types.AccessSDO accessSDOProto, EDSsharp.AccessType datatypeEDS)
        {
            ushort index = 0x2000;
            ushort subindex = 0x1;
            var d = new CanOpenDevice();
            var od = new OdObject
            {
                ObjectType = OdObject.Types.ObjectType.Record
            };
            var sub = new OdSubObject
            {
                Sdo = accessSDOProto,
                Pdo = accessPDOProto,
            };

            od.SubObjects.Add(subindex.ToString(), sub);
            d.Objects.Add(index.ToString(), od);

            var tmp = MappingEDS.MapFromProtobuffer(d);

            Assert.Equal(datatypeEDS, tmp.ods[index].subobjects[subindex].accesstype);
        }

        [Fact]
        public void Test_FromProtobufferSubODObjectMembers()
        {
            ushort index = 0x2000;
            ushort subindex = 0x1;
            var d = new CanOpenDevice();
            var od = new OdObject
            {
                ObjectType = OdObject.Types.ObjectType.Record
            };
            var sub = new OdSubObject
            {
                ActualValue = "123",
                Name = "some value",
                HighLimit = "HighLimit",
                LowLimit = "LowLimit",
                DefaultValue = "defaultvalue",
                Alias = "alias",
            };

            od.SubObjects.Add(subindex.ToString(), sub);
            d.Objects.Add(index.ToString(), od);
            var tmp = MappingEDS.MapFromProtobuffer(d);

            Assert.Equal(sub.ActualValue, tmp.ods[index].subobjects[subindex].actualvalue);
            Assert.Equal(sub.Name, tmp.ods[index].subobjects[subindex].parameter_name);
            Assert.Equal(sub.HighLimit, tmp.ods[index].subobjects[subindex].HighLimit);
            Assert.Equal(sub.LowLimit, tmp.ods[index].subobjects[subindex].LowLimit);
            Assert.Equal(sub.DefaultValue, tmp.ods[index].subobjects[subindex].defaultvalue);
            Assert.Equal(index, tmp.ods[index].subobjects[subindex].Index);
            Assert.Equal(subindex, tmp.ods[index].subobjects[subindex].Subindex);
            Assert.Equal(sub.Alias, tmp.ods[index].subobjects[subindex].denotation);
        }
        [Fact]
        public void Test_FromProtobufferODObject_CustomProperties()
        {
            ushort index = 0x2000;
            var d = new CanOpenDevice();
            var od = new OdObject
            {
                ObjectType = OdObject.Types.ObjectType.Record,
                Disabled = true,
                CountLabel = "CountLabel",
                StorageGroup = "StorageGroup",
                FlagsPDO = true,
            };

            d.Objects.Add(index.ToString(), od);
            var tmp = MappingEDS.MapFromProtobuffer(d);

            Assert.Equal(od.Disabled, tmp.ods[index].prop.CO_disabled);
            Assert.Equal(od.CountLabel, tmp.ods[index].prop.CO_countLabel);
            Assert.Equal(od.StorageGroup, tmp.ods[index].prop.CO_storageGroup);
            Assert.Equal(od.FlagsPDO, tmp.ods[index].prop.CO_flagsPDO);
        }
        [Theory]
        [InlineData(OdSubObject.Types.AccessSRDO.No, AccessSRDO.no)]
        [InlineData(OdSubObject.Types.AccessSRDO.Rx, AccessSRDO.rx)]
        [InlineData(OdSubObject.Types.AccessSRDO.Trx, AccessSRDO.trx)]
        [InlineData(OdSubObject.Types.AccessSRDO.Tx, AccessSRDO.tx)]
        public void Test_FromProtobufferSubODObject_CustomProperties(OdSubObject.Types.AccessSRDO accessSRDO, AccessSRDO co_prop)
        {
            ushort index = 0x2000;
            ushort subindex = 0x1;
            var d = new CanOpenDevice();
            var od = new OdObject
            {
                ObjectType = OdObject.Types.ObjectType.Record
            };
            var sub = new OdSubObject
            {
                Srdo = accessSRDO,
                StringLengthMin = 123,
            };

            od.SubObjects.Add(subindex.ToString(), sub);
            d.Objects.Add(index.ToString(), od);
            var tmp = MappingEDS.MapFromProtobuffer(d);

            Assert.Equal(co_prop, tmp.ods[index].subobjects[subindex].prop.CO_accessSRDO);
            //Assert.Equal(sub.ActualValue, tmp.ods[index].subobjects[subindex].prop.CO_flagsPDO);
            Assert.Equal(sub.StringLengthMin, tmp.ods[index].subobjects[subindex].prop.CO_stringLengthMin);
        }
    }
}
