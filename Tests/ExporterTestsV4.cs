using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using libEDSsharp;

namespace Tests
{
    public class ExporterTestsV4 : CanOpenNodeExporter_V4
    {
        readonly EDSsharp _eds;
        public ExporterTestsV4()
        {
            _eds = new EDSsharp();
        }

        void GetExportResult(EDSsharp eds, out IEnumerable<String> cfile, out IEnumerable<String> hfile, out string odname)
        {
            var fullPath = Path.GetTempFileName();
            var tempfile = Path.GetFileName(fullPath);
            var path = Path.GetTempPath();

            var cfilePath = fullPath + ".c";
            var hfilePath = fullPath + ".h";
            export(cfilePath, eds);
            cfile = File.ReadLines(cfilePath);
            hfile = File.ReadLines(hfilePath);
            odname = tempfile;
        }

        bool FindExpectedLines(IEnumerable<String> lines, List<String> expectedLines)
        {
            var expectedMatchCount = expectedLines.Count;
            var matchedCounter = 0;

            foreach (var line in lines)
            {
                if (line == expectedLines[matchedCounter])
                {
                    matchedCounter++;

                    if (expectedMatchCount == matchedCounter)
                        return true;
                }
            }
            return false;
        }

        [Fact]
        public void Test_Make_cname_conversion()
        {
            if (Make_cname("axle 0 wheel right controlword") != "axle0WheelRightControlword")
                throw (new Exception("Make_cname Conversion error"));

            if (Make_cname("mapped object 4") != "mappedObject4")
                throw (new Exception("Make_cname Conversion error"));

            if (Make_cname("COB ID used by RPDO") != "COB_IDUsedByRPDO")
                throw (new Exception("Make_cname Conversion error"));

            if (Make_cname("A/D unit offset value (filtered)") != "A_DUnitOffsetValueFiltered")
                throw (new Exception("Make_cname Conversion error"));

            if (Make_cname("80 test string") != "_80TestString")
                throw (new Exception("Make_cname Conversion error"));

            if (Make_cname("Eighty test string") != "eightyTestString")
                throw (new Exception("Make_cname Conversion error"));

            if (Make_cname("A") != "a")
                throw (new Exception("Make_cname Conversion error"));

        }

        [Fact]
        public void TestVariableStringZeroData()
        {
            ODentry od = new ODentry
            {
                objecttype = ObjectType.VAR,
                datatype = DataType.VISIBLE_STRING,
                parameter_name = "test string",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000
            };

            _eds.ods.Add(0x2000, od);

            GetExportResult(_eds, out var cfile, out var hfile, out var odname);
            Assert.True(FindExpectedLines(hfile, new List<string> { $"#define {odname}_ENTRY_H2000 &{odname}->list[0]" }));
            Assert.True(FindExpectedLines(hfile, new List<string> { $"#define {odname}_ENTRY_H2000_testString &{odname}->list[0]" }));

            Assert.True(FindExpectedLines(cfile, new List<string> {
               $"static CO_PROGMEM {odname}Objs_t {odname}Objs = {{",
                "    .o_2000_testString = {",
                "        .dataOrig = NULL,",
                "        .attribute = ODA_SDO_R | ODA_STR,",
                "        .dataLength = 0",
                "    }",
                "};"}));

            Assert.True(FindExpectedLines(cfile, new List<string> {
               $"static {odname}_ATTR_OD OD_entry_t {odname}List[] = {{",
               $"    {{0x2000, 0x01, ODT_VAR, &{odname}Objs.o_2000_testString, NULL}},",
                "    {0x0000, 0x00, 0, NULL, NULL}",
                "};"}));
        }

        [Theory]
        [InlineData("12345", 0, 5, "'1', '2', '3', '4', '5', 0")]
        [InlineData("12345", 10, 10, "'1', '2', '3', '4', '5', 0, 0, 0, 0, 0, 0")]
        public void TestVariableString(string defaultValue, uint stringLengthMin, int expectedLength, string expectedInitialValues)
        {
            ODentry od = new ODentry
            {
                objecttype = ObjectType.VAR,
                datatype = DataType.VISIBLE_STRING,
                parameter_name = "test string",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000,
                defaultvalue = defaultValue
            };
            od.prop.CO_stringLengthMin = stringLengthMin;

            _eds.ods.Add(0x2000, od);

            GetExportResult(_eds, out var cfile, out var _, out var odname);

            Assert.True(FindExpectedLines(cfile, new List<string> {
               $"{odname}_ATTR_RAM {odname}_RAM_t {odname}_RAM = {{",
               $"    .x2000_testString = {{{expectedInitialValues}}}",
                "};"}));

            Assert.True(FindExpectedLines(cfile, new List<string> {
               $"static CO_PROGMEM {odname}Objs_t {odname}Objs = {{",
                "    .o_2000_testString = {",
               $"        .dataOrig = &{odname}_RAM.x2000_testString[0],",
                "        .attribute = ODA_SDO_R | ODA_STR,",
               $"        .dataLength = {expectedLength}",
                "    }",
                "};"}));

            Assert.True(FindExpectedLines(cfile, new List<string> {
               $"static {odname}_ATTR_OD OD_entry_t {odname}List[] = {{",
               $"    {{0x2000, 0x01, ODT_VAR, &{odname}Objs.o_2000_testString, NULL}},",
                "    {0x0000, 0x00, 0, NULL, NULL}",
                "};"}));
        }

        [Theory]
        [InlineData("", 10, 10, "0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0")] // only min length
        [InlineData("12345", 10, 10, "'1', '2', '3', '4', '5', 0, 0, 0, 0, 0, 0")] // default & min length
        [InlineData("12345", 0, 5, "'1', '2', '3', '4', '5', 0")] // default only
        public void TestRecordWithString(string defaultValue, uint stringLengthMin, int expectedLength, string expectedInitialValues)
        {
            ODentry od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                parameter_name = "test rec",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000
            };

            od.addsubobject(0, new ODentry("numElements", 0x2000, DataType.UNSIGNED8, "0", EDSsharp.AccessType.rw, PDOMappingType.optional));
            od.addsubobject(1, new ODentry("str", 0x2000, DataType.VISIBLE_STRING, defaultValue, EDSsharp.AccessType.rw, PDOMappingType.optional));
            od.subobjects[1].prop.CO_stringLengthMin = stringLengthMin;
            _eds.ods.Add(0x2000, od);

            GetExportResult(_eds, out var cfile, out var hfile, out var odname);
            Assert.True(FindExpectedLines(hfile, new List<string> {
                "typedef struct {",
                "    struct {",
                "        uint8_t numElements;",
               $"        char str[{expectedLength+1}];",
                "    } x2000_testRec;",
               $"}} {odname}_RAM_t;"}));

            Assert.True(FindExpectedLines(cfile, new List<string>
            {
               $"{odname}_ATTR_RAM {odname}_RAM_t {odname}_RAM = {{",
                "    .x2000_testRec = {",
                "        .numElements = 0x00,",
               $"        .str = {{{expectedInitialValues}}}",
                "    }",
                "};"}));
            
            Assert.True(FindExpectedLines(cfile, new List<string>
            {
                "typedef struct {",
                "    OD_obj_record_t o_2000_testRec[2];",
               $"}} {odname}Objs_t;"}));

            Assert.True(FindExpectedLines(cfile, new List<string>
            {
               $"static CO_PROGMEM {odname}Objs_t {odname}Objs = {{",
                "    .o_2000_testRec = {",
                "        {",
               $"            .dataOrig = &{odname}_RAM.x2000_testRec.numElements,",
                "            .subIndex = 0,",
                "            .attribute = ODA_SDO_RW | ODA_TRPDO,",
                "            .dataLength = 1",
                "        },",
                "        {",
               $"            .dataOrig = &{odname}_RAM.x2000_testRec.str[0],",
                "            .subIndex = 1,",
                "            .attribute = ODA_SDO_RW | ODA_TRPDO | ODA_STR,",
               $"            .dataLength = {expectedLength}",
                "        }",
                "    }",
                "};"}));

            Assert.True(FindExpectedLines(cfile, new List<string>
            {
               $"static {odname}_ATTR_OD OD_entry_t {odname}List[] = {{",
               $"    {{0x2000, 0x02, ODT_REC, &{odname}Objs.o_2000_testRec, NULL}},",
                "    {0x0000, 0x00, 0, NULL, NULL}",
                "};"}));
        }

        [Fact]
        public void TestRecordWithZeroLengthString()
        {
            ODentry od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                parameter_name = "test rec",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000
            };

            od.addsubobject(0, new ODentry("numElements", 0x2000, DataType.UNSIGNED8, "0", EDSsharp.AccessType.rw, PDOMappingType.optional));
            od.addsubobject(1, new ODentry("str", 0x2000, DataType.VISIBLE_STRING, "", EDSsharp.AccessType.rw, PDOMappingType.optional));
            _eds.ods.Add(0x2000, od);

            GetExportResult(_eds, out var cfile, out var hfile, out var odname);
            Assert.True(FindExpectedLines(hfile, new List<string> {
                "typedef struct {",
                "    struct {",
                "        uint8_t numElements;",
                "    } x2000_testRec;",
               $"}} {odname}_RAM_t;"}));

            Assert.True(FindExpectedLines(cfile, new List<string>
            {
               $"{odname}_ATTR_RAM {odname}_RAM_t {odname}_RAM = {{",
                "    .x2000_testRec = {",
                "        .numElements = 0x00",
                "    }",
                "};"}));

            Assert.True(FindExpectedLines(cfile, new List<string>
            {
                "typedef struct {",
                "    OD_obj_record_t o_2000_testRec[2];",
               $"}} {odname}Objs_t;"}));

            Assert.True(FindExpectedLines(cfile, new List<string>
            {
               $"static CO_PROGMEM {odname}Objs_t {odname}Objs = {{",
                "    .o_2000_testRec = {",
                "        {",
               $"            .dataOrig = &{odname}_RAM.x2000_testRec.numElements,",
                "            .subIndex = 0,",
                "            .attribute = ODA_SDO_RW | ODA_TRPDO,",
                "            .dataLength = 1",
                "        },",
                "        {",
                "            .dataOrig = NULL,",
                "            .subIndex = 1,",
                "            .attribute = ODA_SDO_RW | ODA_TRPDO | ODA_STR,",
                "            .dataLength = 0",
                "        }",
                "    }",
                "};"}));

            Assert.True(FindExpectedLines(cfile, new List<string>
            {
               $"static {odname}_ATTR_OD OD_entry_t {odname}List[] = {{",
               $"    {{0x2000, 0x02, ODT_REC, &{odname}Objs.o_2000_testRec, NULL}},",
                "    {0x0000, 0x00, 0, NULL, NULL}",
                "};"}));
        }
    }
}
