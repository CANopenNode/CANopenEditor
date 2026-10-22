using libEDSsharp;
using Xunit;

namespace Tests
{
    public class CompoundSubIndexTests
    {
        [Fact]
        public void AddingFirstRecordFieldCreatesOneFieldAndUpdatesCount()
        {
            var record = new ODentry { Index = 0x2000, objecttype = ObjectType.RECORD };
            record.AddSubEntry();
            Assert.Equal(2, record.subobjects.Count);
            Assert.Equal((ushort)1, EDSsharp.ConvertToUInt16(record.subobjects[0].defaultvalue));
            Assert.NotSame(record.subobjects[0], record.subobjects[1]);

            record.AddSubEntry();
            Assert.Equal(3, record.subobjects.Count);
            Assert.Equal((ushort)2, EDSsharp.ConvertToUInt16(record.subobjects[0].defaultvalue));
        }

        [Theory]
        [InlineData(ObjectType.RECORD)]
        [InlineData(ObjectType.ARRAY)]
        public void SubIndexZeroMustBeACount(ObjectType type)
        {
            var model = new EDSsharp();
            model.ods.Clear();
            var record = new ODentry { Index = 0x2000, objecttype = type };
            model.ods.Add(record.Index, record);
            Assert.Single(model.VerifyCompoundSubIndexZero());

            var count = new ODentry
            {
                parent = record,
                datatype = DataType.UNSIGNED8,
                accesstype = EDSsharp.AccessType.rw,
                defaultvalue = "0"
            };
            record.subobjects.Add(0, count);
            if (type == ObjectType.RECORD)
            {
                Assert.Single(model.VerifyCompoundSubIndexZero());
            }
            else
            {
                Assert.Empty(model.VerifyCompoundSubIndexZero());
            }
            Assert.Equal("0", count.defaultvalue); // Validation never renumbers a device's wire layout.

            count.accesstype = EDSsharp.AccessType.ro;
            Assert.Empty(model.VerifyCompoundSubIndexZero());
            count.datatype = DataType.UNSIGNED32;
            Assert.Single(model.VerifyCompoundSubIndexZero());
        }

        [Theory]
        [InlineData(0x1600)]
        [InlineData(0x1A00)]
        public void PdoMappingCountMayBeWritable(int index)
        {
            var model = new EDSsharp();
            model.ods.Clear();
            var record = new ODentry { Index = (ushort)index, objecttype = ObjectType.RECORD };
            record.subobjects.Add(0, new ODentry
            {
                parent = record,
                datatype = DataType.UNSIGNED8,
                accesstype = EDSsharp.AccessType.rw,
                defaultvalue = "0"
            });
            model.ods.Add(record.Index, record);
            Assert.Empty(model.VerifyCompoundSubIndexZero());
        }
    }
}
