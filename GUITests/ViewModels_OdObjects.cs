using EDSEditorGUI2.ViewModels;

namespace GUITests
{
    public class ViewModels_OdObjects
    {
        private OdObject sut;
        public ViewModels_OdObjects()
        {
            sut = new OdObject();
            sut.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0", new OdSubObject
            {
                Name = "Highest sub-index supported",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned8,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Ro,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0x01"
            }));
            sut.SubObjects.Add(new KeyValuePair<string, OdSubObject>("1", new OdSubObject()
            {
                Name = "Sub Object 1",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            }));
            sut.SubObjects.Add(new KeyValuePair<string, OdSubObject>("2", new OdSubObject()
            {
                Name = "Sub Object 2",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            }));
        }

        [Fact]
        public void AddSubEntry_VarType()
        {
            sut = new OdObject();
            sut.Type = LibCanOpen.OdObject.Types.ObjectType.Var;
            sut.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0", new OdSubObject
            {
                Name = "variableTest",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            }));
            sut.AddSubEntry(sut.SubObjects[0]);
            Assert.Single(sut.SubObjects);
        }

        [Fact]
        public void AddSubEntry_RecordType()
        {
            sut.Type = LibCanOpen.OdObject.Types.ObjectType.Record;
            sut.AddSubEntry(sut.SubObjects[1]);
            Assert.Equal(4, sut.SubObjects.Count);
            Assert.Equal("0x03", sut.SubObjects[0].Value.DefaultValue);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RemoveSubEntry_VarType(bool renumber)
        {
            sut = new OdObject();
            sut.Type = LibCanOpen.OdObject.Types.ObjectType.Var;
            sut.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x01", new OdSubObject
            {
                Name = "variableTest",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            }));

            var result = sut.RemoveSubEntry(sut.SubObjects[0], renumber);
            Assert.False(result);
        }
        [Fact]
        public void RemoveSubEntry_RecordType()
        {
            sut = new OdObject();
            sut.Type = LibCanOpen.OdObject.Types.ObjectType.Record;
            sut.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x00", new OdSubObject
            {
                Name = "variableTest0",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0x01"
            }));
            sut.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x01", new OdSubObject
            {
                Name = "variableTest1",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            }));
            sut.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x02", new OdSubObject
            {
                Name = "variableTest2",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            }));

            var result = sut.RemoveSubEntry(sut.SubObjects[1], false);
            Assert.False(result);
        }
    }
}