using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using libEDSsharp;
using Xunit;

namespace Tests
{
    public sealed class XddLoaderTests : IDisposable
    {
        private readonly string folder = Path.Combine(Path.GetTempPath(), "xdd-loader-" + Guid.NewGuid().ToString("N"));

        public XddLoaderTests()
        {
            Directory.CreateDirectory(folder);
        }

        public void Dispose()
        {
            Directory.Delete(folder, true);
        }

        private static EDSsharp Model(string name)
        {
            var model = new EDSsharp();
            model.ods.Clear();
            model.di.ProductName = name;
            model.ods.Add(0x2000, new ODentry("Mapped value", 0x2000, DataType.UNSIGNED32, "42",
                EDSsharp.AccessType.rw, PDOMappingType.optional));
            return model;
        }

        private string Write(bool legacy, bool network)
        {
            string path = Path.Combine(folder, "description.xdd");
            var models = new List<EDSsharp> { Model("First device"), Model("Second device") };
            if (legacy)
            {
                var writer = new CanOpenXDD();
                if (network)
                {
                    writer.writeMultiXML(path, models);
                }
                else
                {
                    writer.writeXML(path, models[0]);
                }
            }
            else
            {
                var writer = new CanOpenXDD_1_1();
                if (network)
                {
                    writer.WriteMultiXML(path, models, false);
                }
                else
                {
                    writer.WriteXML(path, models[0], false, false);
                }
            }
            return path;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CommandLineImporterAcceptsBothSchemas(bool legacy)
        {
            string source = Write(legacy, false);
            string output = Path.Combine(folder, "converted.eds");
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EDSSharp.exe"),
                    Arguments = "--infile \"" + source + "\" --outfile \"" + output + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.Start();
                Assert.True(process.WaitForExit(10000), "CLI import timed out");
                Assert.Equal(0, process.ExitCode);
            }
            var loaded = new EDSsharp();
            loaded.Loadfile(output);
            Assert.Equal("42", loaded.ods[0x2000].defaultvalue);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void LoadsDeclaredSchemaRegardlessOfPrefixOrBodyFormat(bool legacy, bool prefixed)
        {
            string path = Write(legacy, false);
            XDocument document = XDocument.Load(path);
            // Both schemas can declare ProfileBody formatVersion=1.0.
            foreach (XElement body in document.Descendants().Where(element => element.Name.LocalName == "ProfileBody"))
            {
                body.SetAttributeValue("formatVersion", "1.0");
            }
            if (prefixed)
            {
                document.Root.Attribute("xmlns")?.Remove();
                document.Root.SetAttributeValue(XNamespace.Xmlns + "alternate", document.Root.Name.NamespaceName);
                foreach (XAttribute type in document.Descendants().Attributes(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance")))
                {
                    if (!type.Value.Contains(":"))
                    {
                        type.Value = "alternate:" + type.Value;
                    }
                }
            }
            document.AddFirst(new XComment("An XML declaration, BOM and comment must not affect schema selection."));
            document.Save(path);
            EDSsharp loaded = XddLoader.Load(path);
            Assert.Equal("First device", loaded.di.ProductName);
            Assert.Equal("42", loaded.ods[0x2000].defaultvalue);
            Assert.Equal(PDOMappingType.optional, loaded.ods[0x2000].PDOtype);
            Assert.Equal(path, legacy ? loaded.xddfilename_1_0 : loaded.xddfilename_1_1);
            using (var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.True(exclusive.Length > 0);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LoadsBothNetworkFormatsWithoutLosingDevices(bool legacy)
        {
            string path = Write(legacy, true);
            List<EDSsharp> loaded = XddLoader.LoadNetwork(path);
            Assert.Equal(new[] { "First device", "Second device" }, loaded.Select(model => model.di.ProductName));
            Assert.All(loaded, model => Assert.Equal("42", model.ods[0x2000].defaultvalue));
            Assert.Throws<InvalidDataException>(() => XddLoader.Load(path));
        }

        [Theory]
        [InlineData("")]
        [InlineData("http://www.canopen.org/xml/2.0")]
        [InlineData("http://example.com/1.1")]
        public void RejectsUnsupportedNamespacesExplicitly(string schema)
        {
            string path = Path.Combine(folder, "unknown.xdd");
            new XDocument(new XElement(XName.Get("ISO15745ProfileContainer", schema))).Save(path);
            Assert.Contains("namespace", Assert.Throws<InvalidDataException>(() => XddLoader.Load(path)).Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RejectsMixedNetworkSchemasAndConflictingVersions(bool legacy)
        {
            string path = Write(legacy, true);
            XDocument document = XDocument.Load(path);
            XElement child = document.Root.Elements().First();
            child.Name = XName.Get("ISO15745ProfileContainer", legacy
                ? "http://www.canopen.org/xml/1.1" : "http://www.canopen.org/xml/1.0");
            child.Attribute("xmlns")?.Remove();
            document.Save(path);
            Assert.Throws<InvalidDataException>(() => XddLoader.LoadNetwork(path));
            path = Write(legacy, true);
            document = XDocument.Load(path);
            document.Root.SetAttributeValue("version", legacy ? "1.1" : "1.0");
            document.Save(path);
            Assert.Throws<InvalidDataException>(() => XddLoader.LoadNetwork(path));
        }

        [Theory]
        [InlineData("http://www.canopen.org/xml/1.0")]
        [InlineData("http://www.canopen.org/xml/1.1")]
        public void RecognizedNamespaceWithoutProfilesGivesAnExplicitError(string schema)
        {
            string path = Path.Combine(folder, "empty.xdd");
            new XDocument(new XElement(XName.Get("ISO15745ProfileContainer", schema))).Save(path);
            Assert.Contains("profile body", Assert.Throws<InvalidDataException>(() => XddLoader.Load(path)).Message);
        }

        [Fact]
        public void MalformedXmlAndDtdDoNotTriggerAnotherParserOrKeepFileOpen()
        {
            string path = Path.Combine(folder, "bad.xdd");
            foreach (string content in new[] { "<broken", "<!DOCTYPE a [<!ENTITY x 'value'>]><a>&x;</a>" })
            {
                File.WriteAllText(path, content);
                Assert.Throws<XmlException>(() => XddLoader.Load(path));
                using (var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    Assert.True(exclusive.Length > 0);
                }
            }
        }
    }
}
