using System;
using System.IO;
using System.Linq;
using Xunit;
using libEDSsharp;

namespace Tests
{
    // Warnings.warning_list is static, so the tests that read it must not run in parallel with each other
    [Collection("Warnings")]
    public class ArrayElementAttributeTests
    {
        private const string ArrayName = "Mixed array";
        private const string WarningText = "Attributes of elements in ARRAY must be equal";

        /// <summary>
        /// Build a model with one object 0x2000 and the given element accesses (sub-index 1, 2, ...).
        /// Sub-index 0 is the element count.
        /// </summary>
        private static EDSsharp BuildModel(ObjectType type, params EDSsharp.AccessType[] elementAccess)
        {
            var model = new EDSsharp();
            model.ods.Clear();
            var obj = new ODentry
            {
                Index = 0x2000,
                parameter_name = ArrayName,
                objecttype = type,
                datatype = DataType.UNSIGNED16
            };
            obj.subobjects.Add(0, new ODentry
            {
                parent = obj,
                parameter_name = "Number of elements",
                datatype = DataType.UNSIGNED8,
                accesstype = EDSsharp.AccessType.ro,
                defaultvalue = elementAccess.Length.ToString()
            });
            for (ushort i = 0; i < elementAccess.Length; i++)
            {
                obj.subobjects.Add((ushort)(i + 1), new ODentry
                {
                    parent = obj,
                    parameter_name = "Element " + (i + 1),
                    datatype = DataType.UNSIGNED16,
                    accesstype = elementAccess[i],
                    defaultvalue = "0"
                });
            }
            model.ods.Add(obj.Index, obj);
            return model;
        }

        private static EDSsharp BuildMixedArray()
        {
            return BuildModel(ObjectType.ARRAY, EDSsharp.AccessType.wo, EDSsharp.AccessType.ro, EDSsharp.AccessType.rw);
        }

        [Fact]
        public void UniformArrayHasNoWarning()
        {
            var model = BuildModel(ObjectType.ARRAY, EDSsharp.AccessType.rw, EDSsharp.AccessType.rw, EDSsharp.AccessType.rw);
            Assert.Empty(model.VerifyArrayElementAttributes());
        }

        [Fact]
        public void MixedArrayReportsOneMessageWithDetails()
        {
            var warnings = BuildMixedArray().VerifyArrayElementAttributes();

            string message = Assert.Single(warnings);
            Assert.Equal(
                "Error in 0x2000 \"Mixed array\": Attributes of elements in ARRAY must be equal, " +
                "but sub-indexes 0x02, 0x03 differ from sub-index 0x01. " +
                "CANopenNode stores one attribute for all elements of an ARRAY, so the attribute of sub-index 0x01 is used for all of them. " +
                "Make the access of all elements equal or change the object type to RECORD.",
                message);
            Assert.DoesNotContain("\n", message);
        }

        [Fact]
        public void MixedRecordHasNoWarning()
        {
            var model = BuildModel(ObjectType.RECORD, EDSsharp.AccessType.wo, EDSsharp.AccessType.ro, EDSsharp.AccessType.rw);
            Assert.Empty(model.VerifyArrayElementAttributes());
        }

        [Fact]
        public void DisabledMixedArrayHasNoWarning()
        {
            var model = BuildMixedArray();
            model.ods[0x2000].prop.CO_disabled = true;
            Assert.Empty(model.VerifyArrayElementAttributes());
        }

        [Fact]
        public void ReadOnlyAndConstElementsAreEqual()
        {
            // Both are exported as ODA_SDO_R
            var model = BuildModel(ObjectType.ARRAY, EDSsharp.AccessType.ro, EDSsharp.AccessType.@const, EDSsharp.AccessType.ro);
            Assert.Empty(model.VerifyArrayElementAttributes());
        }

        [Fact]
        public void DifferentPdoMappingIsReported()
        {
            var model = BuildModel(ObjectType.ARRAY, EDSsharp.AccessType.rw, EDSsharp.AccessType.rw, EDSsharp.AccessType.rw);
            model.ods[0x2000].subobjects[1].PDOtype = PDOMappingType.optional;
            model.ods[0x2000].subobjects[2].PDOtype = PDOMappingType.optional;
            model.ods[0x2000].subobjects[3].PDOtype = PDOMappingType.no;

            string message = Assert.Single(model.VerifyArrayElementAttributes());
            Assert.Contains("sub-index 0x03 differs from sub-index 0x01", message);
        }

        [Fact]
        public void DifferentSrdoAccessIsReported()
        {
            var model = BuildModel(ObjectType.ARRAY, EDSsharp.AccessType.rw, EDSsharp.AccessType.rw);
            model.ods[0x2000].subobjects[2].prop.CO_accessSRDO = AccessSRDO.rx;

            string message = Assert.Single(model.VerifyArrayElementAttributes());
            Assert.Contains("sub-index 0x02 differs from sub-index 0x01", message);
        }

        [Fact]
        public void V4ExporterReportsMixedArrayOnce()
        {
            string folder = Path.Combine(Path.GetTempPath(), "ArrayElementAttributeTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            try
            {
                Warnings.warning_list.Clear();
                new CanOpenNodeExporter_V4().export(Path.Combine(folder, "OD.c"), BuildMixedArray());

                // One warning for the whole ARRAY, not one per differing element.
                // Copy the list first: tests outside the collection may still add to it.
                var warnings = Warnings.warning_list.ToArray().Where(warning => warning.Contains(WarningText)).ToList();
                string message = Assert.Single(warnings);
                Assert.Contains(ArrayName, message);
                Assert.Contains("0x2000", message);
                Assert.Contains("sub-indexes 0x02, 0x03 differ from sub-index 0x01", message);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
