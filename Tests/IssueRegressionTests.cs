using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using libEDSsharp;
using Xunit;

namespace Tests
{
    // Kept API-neutral so the same issue contracts can run against CANopenEditor upstream.
    public class IssueRegressionTests : CanOpenNodeExporter
    {
        private static EDSsharp Blank()
        {
            var model = new EDSsharp();
            model.ods.Clear();
            return model;
        }

        private static ODentry Scalar(ushort index, DataType type, string value, string name = "Test value")
        {
            return new ODentry(name, index, type, value, EDSsharp.AccessType.rw, PDOMappingType.no);
        }

        private static string Folder()
        {
            string path = Path.Combine(Path.GetTempPath(), "eds-issue-regressions-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static string Export(EDSsharp model, bool v4, string folder = null, [CallerMemberName] string testName = "")
        {
            string outputRoot = Environment.GetEnvironmentVariable("EDS_REGRESSION_OUTPUT_DIRECTORY");
            folder = folder ?? (string.IsNullOrEmpty(outputRoot) ? Folder()
                : Path.Combine(outputRoot, testName + "-" + Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(folder);
            object exporter = v4 ? (object)new CanOpenNodeExporter_V4() : new CanOpenNodeExporter();
            MethodInfo method = exporter.GetType().GetMethods().Single(m => m.Name == "export");
            object[] args = method.GetParameters().Length == 2
                ? new object[] { Path.Combine(folder, "OD"), model }
                : new object[] { folder, "OD", "issue regression", model, "OD" };
            method.Invoke(exporter, args);
            return folder;
        }

        private static EDSsharp XddRoundTrip(EDSsharp model, bool legacy)
        {
            string path = Path.Combine(Folder(), "device.xdd");
            if (legacy)
            {
                var converter = new CanOpenXDD();
                converter.writeXML(path, model);
                return converter.readXML(path);
            }
            object writer = new CanOpenXDD_1_1();
            MethodInfo method = writer.GetType().GetMethod("WriteXML");
            method.Invoke(writer, method.GetParameters().Length == 5
                ? new object[] { path, model, "regression", false, false }
                : new object[] { path, model, false, false });
            return (EDSsharp)writer.GetType().GetMethod("ReadXML").Invoke(writer, new object[] { path });
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void RejectedXddDoesNotKeepTheFileLocked(bool legacy, bool multi)
        {
            string path = Path.Combine(Folder(), "invalid.xdd");
            File.WriteAllText(path, "<unsupported/>");
            object converter = legacy ? (object)new CanOpenXDD() : new CanOpenXDD_1_1();
            string method = legacy ? (multi ? "readMultiXML" : "readXML") : (multi ? "ReadMultiXML" : "ReadXML");
            Assert.Null(converter.GetType().GetMethod(method).Invoke(converter, new object[] { path }));
            // Format probing deliberately rejects inputs. The caller must still
            // be able to replace/delete the file immediately after that probe.
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.True(stream.Length > 0);
            }
            File.Delete(path);
        }

        [Fact]
        public void Issue271MissingFileInfoImportsWithDefaults()
        {
            string file = Path.Combine(Folder(), "without-fileinfo.eds");
            var model = Blank();
            model.ods.Add(0x2000, Scalar(0x2000, DataType.UNSIGNED32, "42"));
            model.Savefile(file, InfoSection.Filetype.File_EDS);
            string text = Regex.Replace(File.ReadAllText(file), @"(?ms)^\[FileInfo\][^\[]*", "");
            File.WriteAllText(file, text);
            var loaded = Blank();
            loaded.Loadfile(file);
            Assert.Equal("42", loaded.ods[0x2000].defaultvalue);
            Assert.NotNull(loaded.fi);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Issue259PdoRebuildPreservesMetadata(bool v4)
        {
            var model = Blank();
            var helper = new PDOHelper(model);
            helper.addPDOslot(0x1800);
            helper.buildmappingsfromlists(v4);
            ODentry inhibit = model.ods[0x1800].subobjects[3];
            inhibit.accesstype = EDSsharp.AccessType.ro;
            inhibit.Description = "Minimum separation between transmissions";
            inhibit.LowLimit = "10";
            inhibit.HighLimit = "5000";
            inhibit.ObjFlags = 0x12345678;
            model.ods[0x1A00].prop.CO_countLabel = "CUSTOM_MAPPING";
            var rebuilt = new PDOHelper(model);
            rebuilt.build_PDOlists();
            rebuilt.pdoslots[0].inhibit = 200;
            rebuilt.buildmappingsfromlists(v4);
            ODentry actual = model.ods[0x1800].subobjects[3];
            Assert.Equal("200", actual.defaultvalue);
            Assert.Equal(EDSsharp.AccessType.ro, actual.accesstype);
            Assert.Equal(inhibit.Description, actual.Description);
            Assert.Equal("10", actual.LowLimit);
            Assert.Equal("5000", actual.HighLimit);
            Assert.Equal((uint)0x12345678, actual.ObjFlags);
            Assert.Equal("CUSTOM_MAPPING", model.ods[0x1A00].prop.CO_countLabel);
        }

        [Fact]
        public void Issue268InactiveMappingsStillDefineFullPhysicalType()
        {
            var model = Blank();
            var helper = new PDOHelper(model);
            helper.addPDOslot(0x1400);
            helper.buildmappingsfromlists(false);
            string directory = Export(model, false);
            string header = File.ReadAllText(Path.Combine(directory, "OD.h"));
            Assert.Contains("}              OD_RPDOMappingParameter_t;", header);
            Assert.Contains("applicationObject8", header);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Issue233V4MissingNumericDefaultsGenerateZeros(int definedSubindex)
        {
            var model = Blank();
            var record = new ODentry { Index = 0x1018, parameter_name = "Identity object", objecttype = ObjectType.ARRAY, datatype = DataType.UNSIGNED32 };
            record.addsubobject(0, new ODentry("Count", record.Index, DataType.UNSIGNED8, "4", EDSsharp.AccessType.ro, PDOMappingType.no, record));
            for (byte sub = 1; sub <= 4; sub++)
            {
                record.addsubobject(sub, new ODentry("Identity " + sub, record.Index, DataType.UNSIGNED32,
                    sub == definedSubindex ? "0xAB" : "", EDSsharp.AccessType.ro, PDOMappingType.no, record));
            }
            model.ods.Add(record.Index, record);
            string source = File.ReadAllText(Path.Combine(Export(model, true), "OD.c"));
            Assert.DoesNotMatch(@",\s*,", source);
            Assert.DoesNotMatch(@"=\s*,", source);
            Assert.Contains("0x000000AB", source);
        }

        [Theory]
        [InlineData("01a1053c45aabbccddeeff")]
        [InlineData("01 a1 05 3c 45 aa bb cc dd ee ff")]
        public void Issue174LegacyOctetsPreserveExactBytes(string value)
        {
            var model = Blank();
            model.ods.Add(0x2000, Scalar(0x2000, DataType.OCTET_STRING, value));
            string source = File.ReadAllText(Path.Combine(Export(model, false), "OD.c"));
            string initializer = Regex.Match(source, @"/\*2000\*/\s*(\{[^}]+\})").Groups[1].Value;
            Assert.NotEmpty(initializer);
            string compact = Regex.Replace(initializer, @"\s|L", "");
            Assert.Equal("{0x01,0xA1,0x05,0x3C,0x45,0xAA,0xBB,0xCC,0xDD,0xEE,0xFF}", compact);
        }

        [Fact]
        public void Issue164SharedRecordStringUsesLargestInstance()
        {
            var model = Blank();
            foreach (ushort index in new ushort[] { 0x2301, 0x2302 })
            {
                var record = new ODentry { Index = index, parameter_name = "Trace config", objecttype = ObjectType.RECORD };
                record.addsubobject(0, new ODentry("Count", index, DataType.UNSIGNED8, "1", EDSsharp.AccessType.ro, PDOMappingType.no, record));
                record.addsubobject(1, new ODentry("Color", index, DataType.VISIBLE_STRING, index == 0x2301 ? "red" : "green", EDSsharp.AccessType.rw, PDOMappingType.no, record));
                model.ods.Add(index, record);
            }
            string header = File.ReadAllText(Path.Combine(Export(model, false), "OD.h"));
            Assert.Contains("color[5]", header);
        }

        [Fact]
        public void Issue162OctetArrayHasMatchingStorageAndOdLength()
        {
            eds = Blank();
            var array = new ODentry { Index = 0x6100, parameter_name = "Octet array", objecttype = ObjectType.ARRAY, datatype = DataType.OCTET_STRING, accesstype = EDSsharp.AccessType.rw };
            array.addsubobject(0, new ODentry("Count", array.Index, DataType.UNSIGNED8, "4", EDSsharp.AccessType.ro, PDOMappingType.no, array));
            for (byte sub = 1; sub <= 4; sub++)
            {
                array.addsubobject(sub, new ODentry("Entry", array.Index, DataType.OCTET_STRING, new string('0', 32), EDSsharp.AccessType.rw, PDOMappingType.no, array));
            }
            eds.ods.Add(array.Index, array);
            prewalkArrays();
            Assert.Contains("[4][16]", print_h_entry(array));
            Assert.Matches(@"0x6100, 0x04, 0x[0-9A-F]+,\s*16,", write_od_line(array));
        }

        [Fact]
        public void Issue176PdoStorageGroupsProduceBalancedInitializers()
        {
            var model = Blank();
            var helper = new PDOHelper(model);
            helper.addPDOslot(0x1400);
            helper.addPDOslot(0x1401);
            helper.buildmappingsfromlists(false);
            model.ods[0x1400].prop.CO_storageGroup = "ROM";
            model.ods[0x1600].prop.CO_storageGroup = "ROM";
            model.ods[0x1401].prop.CO_storageGroup = "RAM";
            model.ods[0x1601].prop.CO_storageGroup = "RAM";
            string source = File.ReadAllText(Path.Combine(Export(model, false), "OD.c"));
            foreach (Match initializer in Regex.Matches(source, @"(?s)struct sCO_OD_\w+ CO_OD_\w+\s*=\s*\{(.*?)\n\};"))
            {
                string text = initializer.Groups[1].Value;
                Assert.Equal(text.Count(c => c == '{'), text.Count(c => c == '}'));
            }
        }

        [Fact]
        public void Issue230ClonePreservesIdentityFlagsAndExtensions()
        {
            ODentry original = Scalar(0x2345, DataType.UNSIGNED16, "123");
            original.ObjFlags = 0x12345678;
            original.CompactSubObj = 7;
            original.ObjExtend = 3;
            original.count = 9;
            original.uniqueID = "generic-object";
            ODentry clone = original.Clone();
            Assert.Equal(original.Index, clone.Index);
            Assert.Equal(original.ObjFlags, clone.ObjFlags);
            Assert.Equal(original.CompactSubObj, clone.CompactSubObj);
            Assert.Equal(original.ObjExtend, clone.ObjExtend);
            Assert.Equal(original.count, clone.count);
            Assert.Equal(original.uniqueID, clone.uniqueID);
            Assert.NotSame(original.prop, clone.prop);
        }

        [Fact]
        public void Issue259DisabledPdosAndLastSlotSurviveRebuild()
        {
            var model = Blank();
            var helper = new PDOHelper(model);
            helper.addPDOslot(0x1800);
            helper.addPDOslot(0x19FF);
            helper.buildmappingsfromlists(true);
            model.ods[0x1800].prop.CO_disabled = true;
            model.ods[0x1A00].prop.CO_disabled = true;
            var rebuilt = new PDOHelper(model);
            rebuilt.build_PDOlists();
            rebuilt.build_PDOlists();
            Assert.Single(rebuilt.pdoslots);
            Assert.Equal((ushort)0x19FF, rebuilt.pdoslots[0].ConfigurationIndex);
            rebuilt.buildmappingsfromlists(true);
            Assert.True(model.ods[0x1800].prop.CO_disabled);
            Assert.True(model.ods[0x1A00].prop.CO_disabled);
            Assert.True(model.ods.ContainsKey(0x1BFF));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Issue225XddPreservesAllObjFlagBytes(bool legacy)
        {
            var model = Blank();
            ODentry variable = Scalar(0x2000, DataType.UNSIGNED16, "12");
            variable.ObjFlags = 0x12345678;
            model.ods.Add(variable.Index, variable);
            ODentry record = new ODentry { Index = 0x2001, parameter_name = "Record", objecttype = ObjectType.RECORD };
            record.addsubobject(0, new ODentry("Count", record.Index, DataType.UNSIGNED8, "1", EDSsharp.AccessType.ro, PDOMappingType.no, record));
            record.addsubobject(1, new ODentry("Member", record.Index, DataType.UNSIGNED16, "3", EDSsharp.AccessType.rw, PDOMappingType.no, record));
            record.subobjects[1].ObjFlags = 0xABCDEF01;
            model.ods.Add(record.Index, record);
            EDSsharp loaded = XddRoundTrip(model, legacy);
            Assert.Equal(variable.ObjFlags, loaded.ods[variable.Index].ObjFlags);
            Assert.Equal(record.subobjects[1].ObjFlags, loaded.ods[record.Index].subobjects[1].ObjFlags);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Issue198UnchangedXddPreservesCreationAndModification(bool legacy)
        {
            var model = Blank();
            model.ods.Add(0x2000, Scalar(0x2000, DataType.UNSIGNED32, "0"));
            DateTime created = new DateTime(2020, 1, 2, 3, 4, 0);
            DateTime modified = new DateTime(2021, 2, 3, 14, 15, 0);
            model.fi.CreationDateTime = created;
            model.fi.ModificationDateTime = modified;
            model.Dirty = false;
            EDSsharp loaded = XddRoundTrip(model, legacy);
            Assert.Equal(created, loaded.fi.CreationDateTime);
            Assert.Equal(modified, loaded.fi.ModificationDateTime);
        }

        [Fact]
        public void Issue222DuplicateRecordMembersHaveDistinctDataPointers()
        {
            var model = Blank();
            ODentry record = new ODentry { Index = 0x2000, parameter_name = "Record", objecttype = ObjectType.RECORD };
            record.addsubobject(0, new ODentry("Count", record.Index, DataType.UNSIGNED8, "2", EDSsharp.AccessType.ro, PDOMappingType.no, record));
            for (byte sub = 1; sub <= 2; sub++)
            {
                record.addsubobject(sub, new ODentry("Value", record.Index, DataType.UNSIGNED32, sub.ToString(), EDSsharp.AccessType.rw, PDOMappingType.no, record));
            }
            model.ods.Add(record.Index, record);
            string folder = Export(model, false);
            Assert.Contains("value1;", File.ReadAllText(Path.Combine(folder, "OD.h")));
            Assert.Contains("CO_OD_RAM.record.value1,", File.ReadAllText(Path.Combine(folder, "OD.c")));
        }

        [Fact]
        public void Issue122LegacyEmptyStringAndMinimumCapacityAreValid()
        {
            var model = Blank();
            ODentry text = Scalar(0x2000, DataType.VISIBLE_STRING, "", "Empty string");
            text.prop.CO_stringLengthMin = 16;
            model.ods.Add(text.Index, text);
            string folder = Export(model, false);
            Assert.Contains("emptyString[16]", File.ReadAllText(Path.Combine(folder, "OD.h")));
            string source = File.ReadAllText(Path.Combine(folder, "OD.c"));
            Assert.Matches(@"/\*2000\*/\s*\{0\}", source);
            Assert.Matches(@"0x2000, 0x00, 0x[0-9A-F]+,\s*16,", source);
        }

        [Fact]
        public void Issue251DomainArrayDoesNotEmitInvalidSizeof()
        {
            var model = Blank();
            var array = new ODentry { Index = 0x2000, parameter_name = "Domain array", datatype = DataType.DOMAIN, objecttype = ObjectType.ARRAY, accesstype = EDSsharp.AccessType.rw };
            array.addsubobject(0, new ODentry("Count", array.Index, DataType.UNSIGNED8, "1", EDSsharp.AccessType.ro, PDOMappingType.no, array));
            array.addsubobject(1, new ODentry("Domain", array.Index, DataType.DOMAIN, "", EDSsharp.AccessType.rw, PDOMappingType.no, array));
            model.ods.Add(array.Index, array);
            string source = File.ReadAllText(Path.Combine(Export(model, true), "OD.c"));
            Assert.DoesNotContain("sizeof(not specified)", source);
            Assert.Contains(".dataElementSizeof = 0", source);
        }

        [Fact]
        public void Issue190LastEdsOnlyAppearsInDcf()
        {
            var model = Blank();
            string folder = Folder();
            string edsPath = Path.Combine(folder, "device.eds");
            string dcfPath = Path.Combine(folder, "device.dcf");
            model.Savefile(edsPath, InfoSection.Filetype.File_EDS);
            model.Savefile(dcfPath, InfoSection.Filetype.File_DCF);
            Assert.DoesNotContain("LastEDS=", File.ReadAllText(edsPath));
            Assert.Contains("LastEDS=", File.ReadAllText(dcfPath));
        }

        [Fact]
        public void Issue243CustomStorageGroupHasDefinitionAndDeclaration()
        {
            var model = Blank();
            ODentry variable = Scalar(0x2000, DataType.UNSIGNED32, "0");
            variable.prop.CO_storageGroup = "PERSIST_COMM";
            model.ods.Add(variable.Index, variable);
            string header = File.ReadAllText(Path.Combine(Export(model, false), "OD.h"));
            Assert.Contains("struct sCO_OD_PERSIST_COMM", header);
            Assert.Contains("extern struct sCO_OD_PERSIST_COMM CO_OD_PERSIST_COMM", header);
        }

        [Fact]
        public void Issue207CompactArrayPreservesBothNumericLimits()
        {
            string file = Path.Combine(Folder(), "compact.eds");
            Blank().Savefile(file, InfoSection.Filetype.File_EDS);
            File.AppendAllText(file, "\n[2000]\nParameterName=Compact array\nObjectType=0x8\nDataType=0x6\nAccessType=rw\nCompactSubObj=2\nDefaultValue=25\nLowLimit=10\nHighLimit=100\n");
            var model = Blank();
            model.Loadfile(file);
            Assert.Equal("10", model.ods[0x2000].subobjects[1].LowLimit);
            Assert.Equal("100", model.ods[0x2000].subobjects[1].HighLimit);
        }

        [Fact]
        public void Issue224PdoRenameCollisionDoesNotDestroyExistingRecords()
        {
            var model = Blank();
            var helper = new PDOHelper(model);
            helper.addPDOslot(0x1800);
            helper.addPDOslot(0x1801);
            helper.buildmappingsfromlists(true);
            ODentry first = model.ods[0x1800];
            ODentry second = model.ods[0x1801];
            helper.pdoslots[0].ConfigurationIndex = 0x1801;
            Assert.Throws<InvalidOperationException>(() => helper.buildmappingsfromlists(true));
            Assert.Same(first, model.ods[0x1800]);
            Assert.Same(second, model.ods[0x1801]);
        }

        [Fact]
        public void Issue232PdoGapSearchDoesNotOverwriteDisabledObjects()
        {
            var model = Blank();
            var helper = new PDOHelper(model);
            helper.addPDOslot(0x1800);
            helper.buildmappingsfromlists(true);
            model.ods[0x1800].prop.CO_disabled = true;
            var other = new PDOHelper(model);
            other.build_PDOlists();
            Assert.Equal((ushort)0x1801, other.findPDOslotgap(true));
        }

        [Fact]
        public void LegacyExporterCanBeReusedWithoutStaleFeatureCounters()
        {
            var exporter = new CanOpenNodeExporter();
            MethodInfo method = exporter.GetType().GetMethod("export");
            string folder = Folder();
            Action<EDSsharp> generate = model => method.Invoke(exporter, method.GetParameters().Length == 2
                ? new object[] { Path.Combine(folder, "OD"), model }
                : new object[] { folder, "OD", "regression", model, "OD" });
            var first = Blank();
            first.ods.Add(0x1012, Scalar(0x1012, DataType.UNSIGNED32, "0"));
            generate(first);
            var second = Blank();
            second.ods.Add(0x2000, Scalar(0x2000, DataType.UNSIGNED32, "0"));
            generate(second);
            Assert.Matches(@"#define CO_NO_TIME\s+0", File.ReadAllText(Path.Combine(folder, "OD.h")));
        }

        [Fact]
        public void Issue200LegacySyncConfigurationMatchesSupportedStack()
        {
            var model = Blank();
            foreach (ushort index in new ushort[] { 0x1005, 0x1006, 0x1019 })
            {
                model.ods.Add(index, Scalar(index, DataType.UNSIGNED32, "0"));
            }
            string header = File.ReadAllText(Path.Combine(Export(model, false), "OD.h"));
            Assert.Matches(@"#define CO_NO_SYNC\s+0", header);
            model.ods.Add(0x1007, Scalar(0x1007, DataType.UNSIGNED32, "0"));
            header = File.ReadAllText(Path.Combine(Export(model, false), "OD.h"));
            Assert.Matches(@"#define CO_NO_SYNC\s+1", header);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Issue193OrderCodeSurvivesXddRoundTrip(bool legacy)
        {
            var model = Blank();
            model.ods.Add(0x2000, Scalar(0x2000, DataType.UNSIGNED16, "1"));
            model.di.OrderCode = "STANDARD-DEVICE";
            Assert.Equal(model.di.OrderCode, XddRoundTrip(model, legacy).di.OrderCode);
        }

        [Fact]
        public void Issue233LegacyDuplicateScalarNamesHaveUniqueStorage()
        {
            var model = Blank();
            model.ods.Add(0x2000, Scalar(0x2000, DataType.UNSIGNED32, "1", "Same value"));
            model.ods.Add(0x2001, Scalar(0x2001, DataType.UNSIGNED32, "2", "Same value"));
            string folder = Export(model, false);
            Assert.Contains("sameValue_2001", File.ReadAllText(Path.Combine(folder, "OD.h")));
            Assert.Contains("CO_OD_RAM.sameValue_2001", File.ReadAllText(Path.Combine(folder, "OD.c")));
        }

        [Fact]
        public void Issue236VendorPdoMemberNamesMatchSharedTypedef()
        {
            var model = Blank();
            var helper = new PDOHelper(model);
            helper.addPDOslot(0x1400);
            helper.addPDOslot(0x1401);
            helper.buildmappingsfromlists(false);
            model.ods[0x1401].subobjects[1].parameter_name = "COB ID use by RPDO2";
            string source = File.ReadAllText(Path.Combine(Export(model, false), "OD.c"));
            Assert.Contains("RPDOCommunicationParameter[1].COB_IDUsedByRPDO", source);
            Assert.DoesNotContain(".COB_IDUseByRPDO2", source);
        }

        [Fact]
        public void Issue176InterleavedSharedRecordsRemainContiguousInStorage()
        {
            var model = Blank();
            foreach (ushort index in new ushort[] { 0x2300, 0x2302 })
            {
                var record = new ODentry { Index = index, parameter_name = "Trace record", objecttype = ObjectType.RECORD };
                record.addsubobject(0, new ODentry("Count", index, DataType.UNSIGNED8, "1", EDSsharp.AccessType.ro, PDOMappingType.no, record));
                record.addsubobject(1, new ODentry("Value", index, DataType.UNSIGNED32, "1", EDSsharp.AccessType.rw, PDOMappingType.no, record));
                model.ods.Add(index, record);
            }
            model.ods.Add(0x2301, Scalar(0x2301, DataType.UNSIGNED32, "5", "Other value"));
            string source = File.ReadAllText(Path.Combine(Export(model, false), "OD.c"));
            Assert.True(source.IndexOf("/*2302*/", StringComparison.Ordinal) < source.IndexOf("/*2301*/", StringComparison.Ordinal),
                "A scalar was initialized inside the array of shared record instances");
        }

        [Fact]
        public void DisabledRecordCannotDefineAnActiveRecordsLayout()
        {
            var model = Blank();
            foreach (ushort index in new ushort[] { 0x2300, 0x2301 })
            {
                var record = new ODentry { Index = index, parameter_name = "Shared record", objecttype = ObjectType.RECORD };
                record.prop.CO_disabled = index == 0x2300;
                record.addsubobject(0, new ODentry("Count", index, DataType.UNSIGNED8, "1", EDSsharp.AccessType.ro, PDOMappingType.no, record));
                record.addsubobject(1, new ODentry(index == 0x2300 ? "Inactive member" : "Active member", index,
                    DataType.UNSIGNED32, "0", EDSsharp.AccessType.rw, PDOMappingType.no, record));
                model.ods.Add(index, record);
            }
            string header = File.ReadAllText(Path.Combine(Export(model, false), "OD.h"));
            Assert.Contains("activeMember;", header);
            Assert.DoesNotContain("inactiveMember;", header);
        }
    }
}
