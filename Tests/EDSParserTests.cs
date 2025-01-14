using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using libEDSsharp;

namespace Tests
{
    public class EDSParserTests : libEDSsharp.EDSsharp
    {

        [Fact]
        public void Test_parser()
        {

            string[] teststrings =
                {   "ParameterName=Error register",
                    "ParameterName =Error register",
                    "ParameterName = Error register",
                    "ParameterName =Error register",
                    "ParameterName = Error register ",
            };


            foreach (string teststring in teststrings)
            {
                eds.Clear();
                sectionname = "Tests";
                Parseline(teststring,0);

                if (!eds["Tests"].ContainsKey("ParameterName"))
                    throw (new Exception("Parser key detection error on string \"" + teststring + "\""));

                if (eds["Tests"]["ParameterName"] != "Error register")
                    throw (new Exception("Parser value detection error on string \"" + teststring + "\""));
            }

        }

        void injectobject(string testobject)
        {
            eds.Clear();


            string[] lines = testobject.Split('\n');

            foreach (string line in lines)
                Parseline(line,0);

            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in eds)
            {
                ParseEDSentry(kvp);
            }

        }

        [Fact]
        public void Test_CompactSubObj()
        {


            string testobject = @"[1003]
ParameterName=Pre-defined error field
ObjectType=0x8
DataType=0x0007
AccessType=rw
CompactSubObj=9
";

            injectobject(testobject);

            if (!ods.ContainsKey(0x1003))
                throw (new Exception("parseEDSentry() failed no object"));

            ODentry od = ods[0x1003];

            if (od.subobjects.Count != 10)
                throw (new Exception("parseEDSentry() CompactSubObj failed to generate children"));

            ODentry sub = od.Getsubobject(0);

            if (sub.parameter_name != "NrOfObjects")
                throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

            if (sub.datatype != DataType.UNSIGNED8)
                throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

            if (sub.accesstype != AccessType.ro)
                throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

            UInt16 defaultvalue = EDSsharp.ConvertToByte(sub.defaultvalue);

            if (defaultvalue != 9)
                throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

            if (sub.PDOtype != PDOMappingType.no)
                throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

            for (UInt16 x = 1; x < 10; x++)
            {
                if (!od.Containssubindex(x))
                    throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

                sub = od.Getsubobject(x);

                string name = string.Format("{0}{1:00}", od.parameter_name, x);
                if (sub.parameter_name != name)
                    throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

                if (sub.datatype != DataType.UNSIGNED32)
                    throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

                if (sub.accesstype != AccessType.rw)
                    throw (new Exception("parseEDSentry() CompactSubObj incorrect generation"));

            }


        }

        void test_object_coverage(List<string> AlwaysSet, List<string> MandatorySet, List<string> OptionalSet)
        {
            int MandatoryBitMask = (int)Math.Pow(2, MandatorySet.Count);

            int x;
            for (x = 1; x < MandatoryBitMask; x++)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[2000]");

                foreach (String always in AlwaysSet)
                {
                    sb.AppendLine(always);
                }

                for (int y = 0; y < MandatorySet.Count; y++)
                {
                    if (((1 << y) & x) != 0)
                    {
                        sb.AppendLine(MandatorySet[y]);
                    }
                }

                string teststring = sb.ToString();

                bool ok = false;

                try
                {
                    injectobject(teststring);
                    if (x == (MandatoryBitMask - 1))
                    {
                        ok = true;
                    }
                }
                catch (ParameterException)
                {
                    ok = true;

                    if (x == (MandatoryBitMask - 1))
                    {
                        throw new Exception("Test_array_object_loader() failed this did fire exception when it should not:\n\r" + sb.ToString());
                    }
                }

                if (ok == false)
                    throw new Exception("Test_array_object_loader() failed this did not fire exception when it should :\n\r" + sb.ToString());
            }

        }

        [Fact]
        public void Test_var_object_loader()
        {

            List<string> AlwaysSet = new List<string>();
            List<string> MandatorySet = new List<string>();
            List<string> OptionalSet = new List<string>();

            MandatorySet.Add("ParameterName=My Test Object");
            MandatorySet.Add("DataType=0x0007");
            MandatorySet.Add("AccessType=rw");

            test_object_coverage(AlwaysSet, MandatorySet, OptionalSet);


            if (!ods.ContainsKey(0x2000))
                throw (new Exception("parseEDSentry() failed no object"));

            ODentry od = ods[0x2000];

            if (od.objecttype != ObjectType.VAR)
                throw (new Exception("Default object not VAR"));

            if (od.PDOMapping != false)
                throw (new Exception("Default PDO not false"));

            if (od.ObjFlags != 0)
                throw (new Exception("Default objectflags not 0"));


        }

        [Fact]
        public void Test_array_object_loader()
        {

            List<string> AlwaysSet = new List<string>();
            List<string> MandatorySet = new List<string>();
            List<string> OptionalSet = new List<string>();

            MandatorySet.Add("ParameterName=My Test Object");
            MandatorySet.Add("ObjectType=0x0008");
            MandatorySet.Add("SubNumber=5");

            test_object_coverage(AlwaysSet, MandatorySet, OptionalSet);

            if (!ods.ContainsKey(0x2000))
                throw (new Exception("parseEDSentry() failed no object"));

            ODentry od = ods[0x2000];

            if (od.ObjFlags != 0)
                throw (new Exception("Default objectflags not 0"));

        }

        [Fact]
        public void Test_compact_array_object_loader()
        {

            List<string> AlwaysSet = new List<string>();
            List<string> MandatorySet = new List<string>();
            List<string> OptionalSet = new List<string>();

            AlwaysSet.Add("CompactSubObj=5");

            MandatorySet.Add("ParameterName=My Test Object");
            MandatorySet.Add("ObjectType=0x0008");
            MandatorySet.Add("DataType=0x0007");
            MandatorySet.Add("AccessType=rw");

            test_object_coverage(AlwaysSet, MandatorySet, OptionalSet);

            if (!ods.ContainsKey(0x2000))
                throw (new Exception("parseEDSentry() failed no object"));

            ODentry od = ods[0x2000];

            if (od.PDOMapping != false)
                throw (new Exception("Default PDO not false"));

            if (od.ObjFlags != 0)
                throw (new Exception("Default objectflags not 0"));


        }


        [Fact]
        public void Test_implicit_PDOS()
        {


            string testobject = @"[DeviceInfo]
NrOfRXPDO=5
NrOfTXPDO=7
";

            injectobject(testobject);

            di = new DeviceInfo(eds["DeviceInfo"]);

            //Grab explicit PDOs

            UInt16 noexplicitrxpdos = di.NrOfRXPDO;
            UInt16 noexplicittxpdos = di.NrOfTXPDO;

            ApplyimplicitPDO();

            UpdatePDOcount();

            if (noexplicitrxpdos != di.NrOfRXPDO)
                throw (new Exception("Implicit RX PDO incorrect"));

            if (noexplicittxpdos != di.NrOfTXPDO)
                throw (new Exception("Implicit RT PDO incorrect"));


        }


        [Fact]
        public void Test_keyname_case()
        {

            string testobject = @"[DeviceInfo]
VendorName=test1
productname=test2
VENDORNUMBER=test3
ProDucTNumbeR=test4
";

            eds.Clear();

            string[] lines = testobject.Split('\n');

            foreach (string line in lines)
                Parseline(line,0);
            DeviceInfo di = new DeviceInfo(eds["DeviceInfo"]);

        }


        [Fact]
        public void Test_datetimeparse()
        {

            FileInfo fi = new FileInfo();
            Dictionary<string, string> section = new Dictionary<string, string>();
            section.Add("CreationTime", "9:03AM");
            section.Add("CreationDate", "04-27-2017");
            fi.Parse(section,"FileInfo");

            fi = new FileInfo();
            section = new Dictionary<string, string>();
            section.Add("CreationTime", "10:15 AM");
            section.Add("CreationDate", "10-08-2013");

            fi.Parse(section,"FileInfo");

        }

        [Fact]
        public void Test_accesstype()
        {
            {
                Dictionary<string, Dictionary<string, string>> section = new Dictionary<string, Dictionary<string, string>>();
                section.Add("[1234]", new Dictionary<string, string>());
                section["[1234]"].Add("AccessType", "ro");
                KeyValuePair<string, Dictionary<string, string>> kvp = section.Single();
                this.ParseEDSentry(kvp);
            }

            {
                Dictionary<string, Dictionary<string, string>> section = new Dictionary<string, Dictionary<string, string>>();
                section.Add("[1234]", new Dictionary<string, string>());
                section["[1234]"].Add("AccessType", "RO");
                KeyValuePair<string, Dictionary<string, string>> kvp = section.Single();
                this.ParseEDSentry(kvp);
            }


        }

    }
}
