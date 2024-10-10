using Xunit;
using libEDSsharp;
using LibCanOpen;
using System.Linq;

namespace Tests
{
    public class EDSMappingTests
    {
        [Fact]
        public void Test_ToProtobufferAssertConfig()
        {
            var eds = new EDSsharp();
            eds.ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>();
            ODentry od = new ODentry
            {
                objecttype = ObjectType.VAR,
                parameter_name = "Test REC",
                Index = 0x2000
            };

            ODentry sub = new ODentry();
            sub.parameter_name = "some value";
            sub.datatype = DataType.UNSIGNED8;
            sub.parent = od;
            sub.accesstype = EDSsharp.AccessType.ro;
            sub.defaultvalue = "1";
            sub.PDOtype = PDOMappingType.no;
            sub.objecttype = ObjectType.VAR;

            od.subobjects.Add(0x00, sub);
            eds.ods.Add(0x2000, od);

            //Assert is called inside the map function
            var tmp = MappingEDS.MapToProtobuffer(eds);
        }
        [Fact]
        public void Test_FromProtobufferAssertConfig()
        {
            var d = new CanOpenDevice();
            d.FileInfo = new CanOpen_FileInfo();
            d.DeviceInfo = new CanOpen_DeviceInfo();
            d.DeviceCommissioning = new CanOpen_DeviceCommissioning();

            //Assert is called inside the map function
            var tmp = MappingEDS.MapFromProtobuffer(d);

        }
    }
}
