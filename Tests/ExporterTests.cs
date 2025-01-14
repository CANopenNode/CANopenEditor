using System;
using Xunit;
using libEDSsharp;

namespace Tests
{
    
    public class ExporterTests : CanOpenNodeExporter
    {
        [Fact]
        public void Test_cname_conversion()
        {
            ODentry od = new ODentry();

            if (make_cname("axle 0 wheel right controlword",od) != "axle0WheelRightControlword")
                throw (new Exception("make_cname Conversion error"));

            if (make_cname("mapped object 4",od) != "mappedObject4")
                throw (new Exception("make_cname Conversion error"));

            if (make_cname("COB ID used by RPDO",od) != "COB_IDUsedByRPDO")
                throw (new Exception("make_cname Conversion error"));
        }

        [Fact]
        public void Test_record_objects()
        {

            ODentry od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                parameter_name = "Test Record",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000
            };

            ODentry subod = new ODentry("Test String 1", 0x2000, DataType.VISIBLE_STRING, "abcdefg", EDSsharp.AccessType.rw, PDOMappingType.optional, od);

            string test = export_one_record_type(subod, "");

            if (test != "           {(void*)&CO_OD_RAM.testRecord.testString1, 0x3E, 0x7 }," + Environment.NewLine)
                throw (new Exception("export_one_record_type() error test 1"));

            subod = new ODentry("Test String 2", 0x01, DataType.VISIBLE_STRING, new string('*', 255), EDSsharp.AccessType.ro, PDOMappingType.optional, od);
            test = export_one_record_type(subod, "");

            if (test != "           {(void*)&CO_OD_RAM.testRecord.testString2, 0x36, 0xFF }," + Environment.NewLine)
                throw (new Exception("export_one_record_type() error test 2"));

        }

        [Fact]
        public void TestArrays()
        {

            ODentry od = new ODentry
            {
                objecttype = ObjectType.ARRAY,
                datatype = DataType.VISIBLE_STRING,
                parameter_name = "Test Array",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000
            };

            eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };

            eds.ods.Add(0x2000, od);

            prewalkArrays();
            od.subobjects.Add(0x00, new ODentry("No Entries", 0x00, DataType.UNSIGNED8, "4", EDSsharp.AccessType.ro, PDOMappingType.no));
            od.subobjects.Add(0x01, new ODentry("LINE1", 0x01, DataType.VISIBLE_STRING, new string('*', 1), EDSsharp.AccessType.ro, PDOMappingType.optional));
            od.subobjects.Add(0x02, new ODentry("LINE1", 0x02, DataType.VISIBLE_STRING, new string('*', 10), EDSsharp.AccessType.ro, PDOMappingType.optional));
            od.subobjects.Add(0x03, new ODentry("LINE1", 0x03, DataType.VISIBLE_STRING, new string('*', 16), EDSsharp.AccessType.ro, PDOMappingType.optional));
            od.subobjects.Add(0x04, new ODentry("LINE1", 0x04, DataType.VISIBLE_STRING, new string('*', 32), EDSsharp.AccessType.ro, PDOMappingType.optional));

            string test = print_h_entry(od);

            if (test != "/*2000      */ VISIBLE_STRING  testArray[4][32];" + Environment.NewLine)
                throw (new Exception("TestArrays() test 1 failed"));


        }


        /// <summary>
        /// Check we can override maxsub index correctly
        /// </summary>
        [Fact]
        public void TestArrayNoEntries()
        {
            eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };

            ODentry od = new ODentry
            {
                objecttype = ObjectType.ARRAY,
                datatype = DataType.VISIBLE_STRING,
                parameter_name = "Test Array",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x1011
            };

            eds.ods.Add(0x1011, od);

            od.subobjects.Add(0x00, new ODentry("No Entries", 0x00, DataType.UNSIGNED8, "0x7f", EDSsharp.AccessType.ro, PDOMappingType.no));
            od.subobjects.Add(0x01, new ODentry("LINE1", 0x01, DataType.UNSIGNED32, "0x01", EDSsharp.AccessType.ro, PDOMappingType.optional));
          
            string test = write_od_line(od);
            if(test != "{0x1011, 0x7F, 0x06,  0, (void*)&CO_OD_RAM.testArray[0]}," + Environment.NewLine)
                throw (new Exception("TestArrayNoEntries() failed"));


            od = new ODentry
            {
                objecttype = ObjectType.ARRAY,
                datatype = DataType.VISIBLE_STRING,
                parameter_name = "Test Array",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000
            };

            eds.ods.Add(0x2000, od);

            od.subobjects.Add(0x00, new ODentry("No Entries", 0x00, DataType.UNSIGNED8, "0x7f", EDSsharp.AccessType.ro, PDOMappingType.no));
            od.subobjects.Add(0x01, new ODentry("LINE1", 0x01, DataType.UNSIGNED32, "0x01", EDSsharp.AccessType.ro, PDOMappingType.optional));

            test = write_od_line(od);
            if (test != "{0x2000, 0x7F, 0x06,  0, (void*)&CO_OD_RAM.testArray[0]}," + Environment.NewLine)
                throw (new Exception("TestArrayNoEntries() failed"));


            od = new ODentry
            {
                objecttype = ObjectType.ARRAY,
                datatype = DataType.VISIBLE_STRING,
                parameter_name = "Test Array",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x1003
            };

            eds.ods.Add(0x1003, od);

            od.subobjects.Add(0x00, new ODentry("No Entries", 0x00, DataType.UNSIGNED8, "0x00", EDSsharp.AccessType.ro, PDOMappingType.no));
            od.subobjects.Add(0x01, new ODentry("LINE1", 0x01, DataType.UNSIGNED32, "0x01", EDSsharp.AccessType.ro, PDOMappingType.optional));
            od.subobjects.Add(0x02, new ODentry("LINE1", 0x02, DataType.UNSIGNED32, "0x01", EDSsharp.AccessType.ro, PDOMappingType.optional));
            od.subobjects.Add(0x03, new ODentry("LINE1", 0x03, DataType.UNSIGNED32, "0x01", EDSsharp.AccessType.ro, PDOMappingType.optional));

            test = write_od_line(od);
            if (test != "{0x1003, 0x03, 0x0E,  0, (void*)&CO_OD_RAM.testArray[0]}," + Environment.NewLine)
                throw (new Exception("TestArrayNoEntries() failed"));



        }

        /// <summary>
        /// Check the var string is exportet correctly
        /// </summary>
        [Theory]
        [InlineData(1, "")]
        [InlineData(5, "12345")]
        void TestVarString(int expectedLength, string defaultvalue)
        {
            var od = new ODentry
            {
                objecttype = ObjectType.VAR,
                datatype = DataType.VISIBLE_STRING,
                parameter_name = "test string",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000
            };
            od.defaultvalue = defaultvalue;
            var test = print_h_entry(od);
            Assert.Equal(test, $"/*{od.Index:X}      */ VISIBLE_STRING testString[{expectedLength}];" + Environment.NewLine);
        }

        /// <summary>
        /// Check size of objects is correct
        /// </summary>
        [Fact]
        public void TestExportSizes()
        {

            ODentry od = new ODentry
            {
                objecttype = ObjectType.VAR,
                datatype = DataType.INTEGER32,
                parameter_name = "Test INT32",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2000
            };

            
            ODentry od2 = new ODentry
            {
                objecttype = ObjectType.VAR,
                datatype = DataType.UNSIGNED8,
                parameter_name = "Test UINT8",
                accesstype = EDSsharp.AccessType.ro,
                Index = 0x2001
            };

            eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };

            eds.ods.Add(0x2000, od);
            eds.ods.Add(0x2001, od2);

            prewalkArrays();

            string test = write_od_line(od);
            if (test != "{0x2000, 0x00, 0x86,  4, (void*)&CO_OD_RAM.testINT32}," + Environment.NewLine)
                throw (new Exception("write_od_line() returning wrong data length"));

           if((getflags(od2) & 0x80) == 0x80)
           {
                throw (new Exception("Multi byte flag set for single byte"));
           }

        }

        [Fact]
        public void TestImportExportVar()
        {
            eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };

            ODentry od = new ODentry
            {
                objecttype = ObjectType.VAR,
                datatype = DataType.UNSIGNED8,
                parameter_name = "Test VAR",
                accesstype = EDSsharp.AccessType.ro,
                PDOtype = PDOMappingType.optional,
                Index = 0x2000
            };

            eds.ods.Add(0x2000, od);

            string tempfile = System.IO.Path.GetTempFileName();
            eds.Savefile(tempfile, InfoSection.Filetype.File_EDS);

            eds = new EDSsharp();
            eds.Loadfile(tempfile);

            od = eds.ods[0x2000];

            Assert.True(od.PDOtype == PDOMappingType.optional, "TPDOMappingType.optional not set in EDS for VAR");
        }

        /// </summary>
        [Fact]
        public void TestImportExportRecord()
        {
            eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };

            ODentry od = new ODentry
            {
                objecttype = ObjectType.RECORD,
                parameter_name = "Test REC",
                Index = 0x2000
            };

            ODentry sub = new ODentry
            {
                parameter_name = "max sub-index",
                datatype = DataType.UNSIGNED8,
                parent = od,
                accesstype = EDSsharp.AccessType.ro,
                defaultvalue = "1",
                PDOtype = PDOMappingType.no,
                objecttype = ObjectType.VAR
            };

            od.subobjects.Add(0x00, sub);

            sub = new ODentry
            {
                parameter_name = "entry 1",
                datatype = DataType.UNSIGNED8,
                parent = od,
                accesstype = EDSsharp.AccessType.rw,
                defaultvalue = "0",
                PDOtype = PDOMappingType.optional,
                objecttype = ObjectType.VAR
            };

            od.subobjects.Add(0x01, sub);

            eds.ods.Add(0x2000, od);

            string tempfile = System.IO.Path.GetTempFileName();
            eds.Savefile(tempfile, InfoSection.Filetype.File_EDS);

            eds = new EDSsharp();
            eds.Loadfile(tempfile);

            od = eds.ods[0x2000];

            Assert.True(od.subobjects[1].PDOtype == PDOMappingType.optional, "TPDOMappingType.optional not set in EDS for REC");
        }

        [Fact]
        public void TestImportExportArray()
        {

            // NOTE although can opennode does not support per array entry flags, they are supported in EDS
            // so the  PDOtype and TPDODetectCos flags are set per array entry (every VAR sub object) but 
            // they all must be the same
            // and they should not exist on the parent object.

            eds = new EDSsharp
            {
                ods = new System.Collections.Generic.SortedDictionary<ushort, ODentry>()
            };

            ODentry od = new ODentry
            {
                objecttype = ObjectType.ARRAY,
                datatype = DataType.UNSIGNED32,
                parameter_name = "Test Array",
                accesstype = EDSsharp.AccessType.rw,
                Index = 0x2000
            };

            ODentry sub = new ODentry
            {
                parameter_name = "max sub-index",
                datatype = DataType.UNSIGNED8,
                parent = od,
                accesstype = EDSsharp.AccessType.ro,
                PDOtype = PDOMappingType.no,
                defaultvalue = "2",
                objecttype = ObjectType.VAR
            };

            od.subobjects.Add(0x00, sub);

            sub = new ODentry
            {
                parameter_name = "entry 1",
                datatype = DataType.UNSIGNED32,
                parent = od,
                accesstype = EDSsharp.AccessType.rw,
                defaultvalue = "0",
                objecttype = ObjectType.VAR,
                PDOtype = PDOMappingType.optional
            };

            od.subobjects.Add(0x01, sub);

            sub = new ODentry
            {
                parameter_name = "entry 2",
                datatype = DataType.UNSIGNED32,
                parent = od,
                accesstype = EDSsharp.AccessType.rw,
                defaultvalue = "0",
                objecttype = ObjectType.VAR,
                PDOtype = PDOMappingType.optional
            };

            od.subobjects.Add(0x02, sub);

            eds.ods.Add(0x2000, od);

            string tempfile = System.IO.Path.GetTempFileName();
            eds.Savefile(tempfile, InfoSection.Filetype.File_EDS);

            eds = new EDSsharp();
            eds.Loadfile(tempfile);

            od = eds.ods[0x2000];

            Assert.True(od.subobjects[1].PDOtype == PDOMappingType.optional, "TPDOMappingType.optional not set in EDS for ARRAY");
        }
    }
}
