using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace libEDSsharp
{
    /// <summary>Loads either supported XDD schema without speculative deserialization.</summary>
    public static class XddLoader
    {
        private const string Namespace10 = "http://www.canopen.org/xml/1.0";
        private const string Namespace11 = "http://www.canopen.org/xml/1.1";

        public static EDSsharp Load(string filename)
        {
            using (XmlReader reader = CreateReader(filename))
            {
                reader.MoveToContent();
                if (reader.LocalName != "ISO15745ProfileContainer")
                {
                    throw new InvalidDataException("Expected a CANopen XDD device description.");
                }

                // ProfileBody.formatVersion can be 1.0 even in an XDD 1.1 file.
                // The expanded root name identifies the actual serialization schema;
                // namespace prefixes, file extensions and generator versions do not.
                bool legacy = IsLegacyNamespace(reader.NamespaceURI);
                EDSsharp result = legacy
                    ? Convert(Deserialize<CanOpenXSD_1_0.ISO15745ProfileContainer>(reader))
                    : Convert(Deserialize<CanOpenXSD_1_1.ISO15745ProfileContainer>(reader));
                if (legacy)
                {
                    result.xddfilename_1_0 = filename;
                }
                else
                {
                    result.xddfilename_1_1 = filename;
                }
                return result;
            }
        }

        public static List<EDSsharp> LoadNetwork(string filename)
        {
            XDocument document = ReadDocument(filename);
            XElement root = document.Root;
            bool legacy = root.Name == XName.Get("OpenEDSProject");
            if (!legacy && root.Name != XName.Get("CanOpenProject_1_1"))
            {
                throw new InvalidDataException("Expected a supported CANopen XDD network project.");
            }
            string version = (string)root.Attribute("version");
            if (!string.IsNullOrEmpty(version) && version != (legacy ? "1.0" : "1.1"))
            {
                throw new InvalidDataException("The network project version conflicts with its container format.");
            }
            XName deviceName = XName.Get("ISO15745ProfileContainer", legacy ? Namespace10 : Namespace11);
            if (root.Elements().Any(element => element.Name != deviceName))
            {
                throw new InvalidDataException("The network project contains an unsupported or mixed XDD schema.");
            }
            if (legacy)
            {
                var project = Deserialize<OpenEDSProject>(document);
                return (project.ISO15745ProfileContainer ?? new List<CanOpenXSD_1_0.ISO15745ProfileContainer>())
                    .Select(container => Convert(container)).ToList();
            }
            var current = Deserialize<CanOpenProject_1_1>(document);
            return (current.ISO15745ProfileContainer ?? new List<CanOpenXSD_1_1.ISO15745ProfileContainer>())
                .Select(container => Convert(container)).ToList();
        }

        private static EDSsharp Convert(CanOpenXSD_1_0.ISO15745ProfileContainer container)
        {
            if (container.ISO15745Profile == null || container.ISO15745Profile.Length == 0
                || container.ISO15745Profile.Any(profile => profile == null || profile.ProfileBody == null))
            {
                throw new InvalidDataException("The XDD 1.0 description is missing a profile body.");
            }
            return new CanOpenXDD().convert(container);
        }

        private static EDSsharp Convert(CanOpenXSD_1_1.ISO15745ProfileContainer container)
        {
            if (container.ISO15745Profile == null || container.ISO15745Profile.Length == 0
                || container.ISO15745Profile.Any(profile => profile == null || profile.ProfileBody == null))
            {
                throw new InvalidDataException("The XDD 1.1 description is missing a profile body.");
            }
            return new CanOpenXDD_1_1().Convert(container);
        }

        private static bool IsLegacyNamespace(string value)
        {
            if (value == Namespace10)
            {
                return true;
            }
            if (value == Namespace11)
            {
                return false;
            }
            throw new InvalidDataException("Unsupported CANopen XDD schema namespace: " + value);
        }

        private static XmlReader CreateReader(string filename)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            return XmlReader.Create(filename, settings);
        }

        private static XDocument ReadDocument(string filename)
        {
            using (XmlReader reader = CreateReader(filename))
            {
                return XDocument.Load(reader);
            }
        }

        private static T Deserialize<T>(XDocument document)
        {
            using (XmlReader reader = document.CreateReader())
            {
                return Deserialize<T>(reader);
            }
        }

        private static T Deserialize<T>(XmlReader reader)
        {
            try
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
            }
            catch (InvalidOperationException error)
            {
                throw new InvalidDataException("Invalid CANopen XDD document for the declared schema.", error);
            }
        }
    }
}
