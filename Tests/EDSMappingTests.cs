using Xunit;
using libEDSsharp;
using LibCanOpen;
using System.Globalization;
using System;
using Google.Protobuf.WellKnownTypes;

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
                Index = 0x2000
            };
            eds.ods.Add(od.Index, od);
            var tmp = MappingEDS.MapToProtobuffer(eds);
            Assert.Equal(objTypeProto, tmp.Objects[od.Index.ToString()].ObjectType);
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
        public void Test_FromProtobufferAssertConfig()
        {
            var d = new CanOpenDevice
            {
                FileInfo = new CanOpen_FileInfo(),
                DeviceInfo = new CanOpen_DeviceInfo(),
                DeviceCommissioning = new CanOpen_DeviceCommissioning()
            };

            //Assert is called inside the map function
            MappingEDS.MapFromProtobuffer(d);
        }
    }
}
