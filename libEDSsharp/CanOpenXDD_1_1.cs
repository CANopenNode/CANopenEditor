/*
    This file is part of libEDSsharp.

    libEDSsharp is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    libEDSsharp is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with libEDSsharp.  If not, see <http://www.gnu.org/licenses/>.

    Copyright(c) 2016 - 2020 Robin Cornelius <robin.cornelius@gmail.com>
    Copyright(c) 2020 Janez Paternoster
*/


using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CanOpenXSD_1_1;
using LibCanOpen;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Reflection;

namespace libEDSsharp
{
    /// <summary>
    /// Convert to/from EDSsharp and CanOpenXDD v1.1, it uses the generated source file CanOpenXSD_1_1
    /// </summary>
    /// <seealso cref="CanOpenXSD_1_1"/>
    public class CanOpenXDD_1_1 : IFileExporter
    {
        /// <summary>
        /// Fetches all the different fileexporter types the class supports
        /// </summary>
        /// <returns>List of the different exporters the class supports</returns>
        public ExporterDescriptor[] GetExporters()
        {
            return new ExporterDescriptor[] {
                new ExporterDescriptor("CanOpen XDD v1.1", new string[] { ".xdd" }, 0, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD_1_1();
                    e.WriteXML(filepath,edss[0],false,false);
                }),
                new ExporterDescriptor("CanOpen XDD v1.1 stripped", new string[] { ".xdd" }, 0, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD_1_1();
                    e.WriteXML(filepath,edss[0],false,true);
                }),
                new ExporterDescriptor("CanOpen XDC v1.1", new string[] { ".xdc" }, 0, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD_1_1();
                    e.WriteXML(filepath,edss[0],true,true);
                }),
                new ExporterDescriptor("CanOpen Network XDD v1.1", new string[] { ".nxdd" }, ExporterDescriptor.ExporterFlags.MultipleNodeSupport, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD_1_1();
                    e.WriteMultiXML(filepath,edss,false);
                }),
                new ExporterDescriptor("CanOpen Network XDC v1.1", new string[] { ".nxdc" }, ExporterDescriptor.ExporterFlags.MultipleNodeSupport, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD_1_1();
                    e.WriteMultiXML(filepath,edss,true);
                }),
                new ExporterDescriptor("CanOpenNode Protobuf (json)", new string[] { ".json" }, 0, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD_1_1();
                    e.WriteProtobuf(filepath,edss[0],true);
                }),
                new ExporterDescriptor("CanOpenNode Protobuf (binary)", new string[] { ".binpb" }, 0, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenXDD_1_1();
                    e.WriteProtobuf(filepath,edss[0],false);
                })
            };
        }
        /// <summary>
        /// Read XDD file into EDSsharp object
        /// </summary>
        /// <param name="file">Name of the xdd file</param>
        /// <returns>EDSsharp object</returns>
        public EDSsharp ReadXML(string file)
        {
            ISO15745ProfileContainer dev;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ISO15745ProfileContainer));
                StreamReader reader = new StreamReader(file);
                dev = (ISO15745ProfileContainer)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception e)
            {
                if (e is System.InvalidOperationException)
                {
                    Warnings.warning_list.Add(String.Format("{0} {1} Action aborted!", e.Message, e.InnerException.Message));
                }
                else
                {
                    Warnings.warning_list.Add(String.Format("{0} Action aborted!", e.ToString()));
                }
                return null;
            }

            return Convert(dev);
        }

        /// <summary>
        /// Read custom multi xdd file (multiple standard xdd files inside one xml container)
        /// </summary>
        /// <param name="file">Name of the multi xdd file</param>
        /// <returns>List of EDSsharp objects</returns>
        public List<EDSsharp> ReadMultiXML(string file )
        {
            List<EDSsharp> edss = new List<EDSsharp>();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CanOpenProject_1_1));
                StreamReader reader = new StreamReader(file);
                CanOpenProject_1_1 oep = (CanOpenProject_1_1)serializer.Deserialize(reader);

                foreach(ISO15745ProfileContainer cont in oep.ISO15745ProfileContainer)
                {
                    edss.Add(Convert(cont));
                }

                reader.Close();

                return edss;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Write custom multi xdd file (multiple standard xdd files inside one xml container)
        /// </summary>
        /// <param name="file">Name of the multi xdd file</param>
        /// <param name="edss">List of EDSsharp objects</param>
        /// <param name="deviceCommissioning">If true, device commisioning, denotations and actual values will be included</param>
        public void WriteMultiXML(string file, List<EDSsharp> edss, bool deviceCommissioning)
        {
            var versionAttributes = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                as AssemblyInformationalVersionAttribute[];

            string gitVersion = versionAttributes[0].InformationalVersion;
            List<ISO15745ProfileContainer> devs = new List<ISO15745ProfileContainer>();

            foreach (EDSsharp eds in edss)
            {
                ISO15745ProfileContainer dev = Convert(eds, Path.GetFileName(file), deviceCommissioning, false);
                devs.Add(dev);
            }

            CanOpenProject_1_1 oep = new CanOpenProject_1_1
            {
                Version = "1.1",
                ISO15745ProfileContainer = devs
            };

            XmlSerializer serializer = new XmlSerializer(typeof(CanOpenProject_1_1));

            StreamWriter stream = new StreamWriter(file);
            stream.NewLine = "\n";
            XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, NewLineChars = "\n" });
            writer.WriteStartDocument();
            writer.WriteComment(string.Format("CANopen Project file in custom format. It contains multiple standard CANopen device description files.", gitVersion));
            writer.WriteComment(string.Format("File is generated by CANopenEditor {0}, URL: https://github.com/CANopenNode/CANopenEditor", gitVersion));
            writer.WriteComment("File includes additional custom properties for CANopenNode, CANopen protocol stack, URL: https://github.com/CANopenNode/CANopenNode");
            serializer.Serialize(writer, oep);
            writer.WriteEndDocument();
            writer.Close();
            stream.Close();
        }

        /// <summary>
        /// Write XDD file from EDSsharp object
        /// </summary>
        /// <param name="file">Name of the xdd file</param>
        /// <param name="eds">EDSsharp object</param>
        /// <param name="deviceCommissioning">If true, device commisioning, denotations and actual values will be included</param>
        /// <param name="stripped">If true, then all CANopenNode specific parameters and all disabled objects will be stripped</param>
        public void WriteXML(string file, EDSsharp eds, bool deviceCommissioning, bool stripped)
        {
            ISO15745ProfileContainer dev = Convert(eds, Path.GetFileName(file), deviceCommissioning, stripped);
            XmlSerializer serializer = new XmlSerializer(typeof(ISO15745ProfileContainer));

            StreamWriter stream = new StreamWriter(file);
            stream.NewLine = "\n";
            XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, NewLineChars = "\n" });
            writer.WriteStartDocument();

            var versionAttributes = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                as AssemblyInformationalVersionAttribute[];

            string gitVersion = versionAttributes[0].InformationalVersion;

            writer.WriteComment(string.Format("File is generated by CANopenEditor {0}, URL: https://github.com/CANopenNode/CANopenEditor", gitVersion));
            if (!stripped)
                writer.WriteComment("File includes additional custom properties for CANopenNode, CANopen protocol stack, URL: https://github.com/CANopenNode/CANopenNode");
            serializer.Serialize(writer, dev);
            writer.WriteEndDocument();
            writer.Close();
            stream.Close();
        }

        /// <summary>
        /// Read protobuffer file into EDSsharp object
        /// </summary>
        /// <param name="file">Name of the protobuffer file</param>
        /// <param name="json">read as JSON string or binary wireformat</param>
        /// <returns>EDSsharp object</returns>
        public EDSsharp ReadProtobuf(string file, bool json)
        {
            CanOpenDevice devCanOpen;

            // read the protobuffer message in json format or binary wireformat
            if (json)
            {
                var parserConfig = new JsonParser.Settings(100);
                var parser = new JsonParser(parserConfig);
                devCanOpen = parser.Parse<CanOpenDevice>(System.IO.File.ReadAllText(file));
            }
            else
            {
                using (var input = System.IO.File.OpenRead(file))
                {
                    devCanOpen = CanOpenDevice.Parser.ParseFrom(input);
                }
            }

            /* first convert to XDD, then to EDSsharp (for now) */
            ISO15745ProfileContainer devXdd = ConvertFromProtobuf(devCanOpen, file, true, false);

            return Convert(devXdd);
        }

        /// <summary>
        /// Write protobuffer file from EDSsharp object
        /// </summary>
        /// <param name="file">Name of the protobuffer file</param>
        /// <param name="eds">EDSsharp object</param>
        /// <param name="json">write as JSON string or binary wireformat</param>
        public void WriteProtobuf(string file, EDSsharp eds, bool json)
        {
            /* first convert to XDD, then to protobuffer (for now) */
            ISO15745ProfileContainer devXdd = Convert(eds, Path.GetFileName(file), true, false);
            CanOpenDevice dev = ConvertToProtobuf(devXdd);

            // write the protobuffer message in json format or binary wireformat
            if (json)
            {
                var formatterConfig = new JsonFormatter.Settings(true).WithIndentation().WithFormatDefaultValues(true);
                var formatter = new JsonFormatter(formatterConfig);
                var rawJsonString = formatter.Format(dev);
                System.IO.File.WriteAllText(file, rawJsonString);
            }
            else
            {
                using (var output = System.IO.File.Create(file))
                {
                    dev.WriteTo(output);
                }
            }
        }

        private parameterTemplateAccess ConvertAccessType(EDSsharp.AccessType edsAccessType)
        {
            switch (edsAccessType)
            {
                case EDSsharp.AccessType.@const: return parameterTemplateAccess.@const;
                case EDSsharp.AccessType.ro:     return parameterTemplateAccess.read;
                case EDSsharp.AccessType.rw:     return parameterTemplateAccess.readWrite;
                case EDSsharp.AccessType.rwr:    return parameterTemplateAccess.readWriteInput;
                case EDSsharp.AccessType.rww:    return parameterTemplateAccess.readWriteOutput;
                case EDSsharp.AccessType.wo:     return parameterTemplateAccess.write;
                default:                         return parameterTemplateAccess.noAccess;
            }
        }

        private parameterTemplateAccess ConvertAccessType(OdSubObject subEntry)
        {
            switch (subEntry.Sdo)
            {
                case LibCanOpen.OdSubObject.Types.AccessSDO.Ro: return parameterTemplateAccess.read;
                case LibCanOpen.OdSubObject.Types.AccessSDO.Wo: return parameterTemplateAccess.write;
                case LibCanOpen.OdSubObject.Types.AccessSDO.Rw:
                    switch (subEntry.Pdo)
                    {
                        case LibCanOpen.OdSubObject.Types.AccessPDO.R: return parameterTemplateAccess.readWriteInput;
                        case LibCanOpen.OdSubObject.Types.AccessPDO.T: return parameterTemplateAccess.readWriteOutput;
                        default: return parameterTemplateAccess.readWrite;
                    }
                default: return parameterTemplateAccess.noAccess;
            }
        }

        private EDSsharp.AccessType ConvertAccessType(parameterTemplateAccess xddAccessType)
        {
            switch (xddAccessType)
            {
                case parameterTemplateAccess.@const:          return EDSsharp.AccessType.@const;
                case parameterTemplateAccess.read:            return EDSsharp.AccessType.ro;
                case parameterTemplateAccess.readWrite:       return EDSsharp.AccessType.rw;
                case parameterTemplateAccess.readWriteInput:  return EDSsharp.AccessType.rwr;
                case parameterTemplateAccess.readWriteOutput: return EDSsharp.AccessType.rww;
                case parameterTemplateAccess.write:           return EDSsharp.AccessType.wo;
                default:                                      return EDSsharp.AccessType.UNKNOWN;
            }
        }

        private void ConvertAccessType(parameterTemplateAccess xddAccessType, OdSubObject subEntry)
        {
            switch (xddAccessType)
            {
                case parameterTemplateAccess.@const:
                case parameterTemplateAccess.read:
                    subEntry.Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Ro;
                    break;
                case parameterTemplateAccess.readWrite:
                    subEntry.Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw;
                    break;
                case parameterTemplateAccess.readWriteInput:
                    subEntry.Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw;
                    subEntry.Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.T;
                    break;
                case parameterTemplateAccess.readWriteOutput:
                    subEntry.Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw;
                    subEntry.Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.R;
                    break;
                case parameterTemplateAccess.write:
                    subEntry.Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Wo;
                    break;
                default:
                    subEntry.Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.No;
                    break;
            }
        }

        private Items1ChoiceType ConvertDataType (ODentry od)
        {
            UInt32 byteLength;
            bool signed = false;
            var dt = od.datatype;
            if (dt == DataType.UNKNOWN && od.parent != null)
                dt = od.parent.datatype;

            switch (dt)
            {
                case DataType.BOOLEAN:        return Items1ChoiceType.BOOL;
                case DataType.INTEGER8:       return Items1ChoiceType.SINT;
                case DataType.INTEGER16:      return Items1ChoiceType.INT;
                case DataType.INTEGER32:      return Items1ChoiceType.DINT;
                case DataType.INTEGER64:      return Items1ChoiceType.LINT;
                case DataType.UNSIGNED8:      return Items1ChoiceType.USINT;
                case DataType.UNSIGNED16:     return Items1ChoiceType.UINT;
                case DataType.UNSIGNED32:     return Items1ChoiceType.UDINT;
                case DataType.UNSIGNED64:     return Items1ChoiceType.ULINT;
                case DataType.REAL32:         return Items1ChoiceType.REAL;
                case DataType.REAL64:         return Items1ChoiceType.LREAL;
                case DataType.VISIBLE_STRING: return Items1ChoiceType.STRING;
                case DataType.OCTET_STRING:   return Items1ChoiceType.BITSTRING;
                case DataType.UNICODE_STRING: return Items1ChoiceType.WSTRING;

                case DataType.DOMAIN:
                    od.defaultvalue = "";
                    return Items1ChoiceType.BITSTRING;

                default:
                    od.datatype = DataType.INTEGER32;
                    return Items1ChoiceType.DINT;

                // transform other non standard values to OCTET_STRING
                case DataType.INTEGER24:       byteLength = 3; signed = true; break;
                case DataType.INTEGER40:       byteLength = 5; signed = true; break;
                case DataType.INTEGER48:       byteLength = 6; signed = true; break;
                case DataType.INTEGER56:       byteLength = 7; signed = true; break;
                case DataType.UNSIGNED24:      byteLength = 3; break;
                case DataType.UNSIGNED40:      byteLength = 5; break;
                case DataType.UNSIGNED48:
                case DataType.TIME_OF_DAY:
                case DataType.TIME_DIFFERENCE: byteLength = 6; break;
                case DataType.UNSIGNED56:      byteLength = 7; break;
            }

            // set datatype OCTET_STRING and write default value as a sequence of bytes, little endian, like "56 34 12"
            UInt64 value;
            try
            {
                value = signed ? (UInt64)((Int64)new System.ComponentModel.Int64Converter().ConvertFromString(od.defaultvalue))
                               : (UInt64) new System.ComponentModel.UInt64Converter().ConvertFromString(od.defaultvalue);
            }
            catch (Exception)
            {
                value = 0;
            }

            List<string> bytes = new List<string>();
            for (UInt32 i = 0; i < byteLength; i++)
            {
                bytes.Add(String.Format("{0:X2}", value & 0xFF));
                value >>= 8;
            }
            od.datatype = DataType.OCTET_STRING;
            od.defaultvalue = string.Join(" ", bytes);

            return Items1ChoiceType.BITSTRING;
        }

        private DataType ConvertDataType(Items1ChoiceType choiceType, string defaultValue)
        {
            switch (choiceType)
            {
                case Items1ChoiceType.BOOL: return DataType.BOOLEAN;
                case Items1ChoiceType.CHAR:
                case Items1ChoiceType.SINT: return DataType.INTEGER8;
                case Items1ChoiceType.INT: return DataType.INTEGER16;
                case Items1ChoiceType.DINT: return DataType.INTEGER32;
                case Items1ChoiceType.LINT: return DataType.INTEGER64;
                case Items1ChoiceType.BYTE:
                case Items1ChoiceType.USINT: return DataType.UNSIGNED8;
                case Items1ChoiceType.WORD:
                case Items1ChoiceType.UINT: return DataType.UNSIGNED16;
                case Items1ChoiceType.DWORD:
                case Items1ChoiceType.UDINT: return DataType.UNSIGNED32;
                case Items1ChoiceType.LWORD:
                case Items1ChoiceType.ULINT: return DataType.UNSIGNED64;
                case Items1ChoiceType.REAL: return DataType.REAL32;
                case Items1ChoiceType.LREAL: return DataType.REAL64;
                case Items1ChoiceType.STRING: return DataType.VISIBLE_STRING;
                case Items1ChoiceType.WSTRING: return DataType.UNICODE_STRING;
                case Items1ChoiceType.BITSTRING:
                    return defaultValue == "" ? DataType.DOMAIN : DataType.OCTET_STRING;
                default:
                    return DataType.INTEGER32;
            }
        }

        private Items1ChoiceType ConvertDataType(OdSubObject subEntry)
        {
            UInt32 byteLength;
            bool signed = false;
            var dt = subEntry.DataType;

            switch (dt)
            {
                case LibCanOpen.OdSubObject.Types.DataType.Boolean: return Items1ChoiceType.BOOL;
                case LibCanOpen.OdSubObject.Types.DataType.Integer8: return Items1ChoiceType.SINT;
                case LibCanOpen.OdSubObject.Types.DataType.Integer16: return Items1ChoiceType.INT;
                case LibCanOpen.OdSubObject.Types.DataType.Integer32: return Items1ChoiceType.DINT;
                case LibCanOpen.OdSubObject.Types.DataType.Integer64: return Items1ChoiceType.LINT;
                case LibCanOpen.OdSubObject.Types.DataType.Unsigned8: return Items1ChoiceType.USINT;
                case LibCanOpen.OdSubObject.Types.DataType.Unsigned16: return Items1ChoiceType.UINT;
                case LibCanOpen.OdSubObject.Types.DataType.Unsigned32: return Items1ChoiceType.UDINT;
                case LibCanOpen.OdSubObject.Types.DataType.Unsigned64: return Items1ChoiceType.ULINT;
                case LibCanOpen.OdSubObject.Types.DataType.Real32: return Items1ChoiceType.REAL;
                case LibCanOpen.OdSubObject.Types.DataType.Real64: return Items1ChoiceType.LREAL;
                case LibCanOpen.OdSubObject.Types.DataType.VisibleString: return Items1ChoiceType.STRING;
                case LibCanOpen.OdSubObject.Types.DataType.OctetString: return Items1ChoiceType.BITSTRING;
                case LibCanOpen.OdSubObject.Types.DataType.UnicodeString: return Items1ChoiceType.WSTRING;

                case LibCanOpen.OdSubObject.Types.DataType.Domain:
                    subEntry.DefaultValue = "";
                    return Items1ChoiceType.BITSTRING;

                default:
                    subEntry.DataType = LibCanOpen.OdSubObject.Types.DataType.Integer32;
                    return Items1ChoiceType.DINT;

                // transform other non standard values to OCTET_STRING
                case LibCanOpen.OdSubObject.Types.DataType.Integer24: byteLength = 3; signed = true; break;
                case LibCanOpen.OdSubObject.Types.DataType.Integer40: byteLength = 5; signed = true; break;
                case LibCanOpen.OdSubObject.Types.DataType.Integer48: byteLength = 6; signed = true; break;
                case LibCanOpen.OdSubObject.Types.DataType.Integer56: byteLength = 7; signed = true; break;
                case LibCanOpen.OdSubObject.Types.DataType.Unsigned24: byteLength = 3; break;
                case LibCanOpen.OdSubObject.Types.DataType.Unsigned40: byteLength = 5; break;
                case LibCanOpen.OdSubObject.Types.DataType.Unsigned48:
                case LibCanOpen.OdSubObject.Types.DataType.TimeOfDay:
                case LibCanOpen.OdSubObject.Types.DataType.TimeDifference: byteLength = 6; break;
                case LibCanOpen.OdSubObject.Types.DataType.Unsigned56: byteLength = 7; break;
            }

            // set datatype OCTET_STRING and write default value as a sequence of bytes, little endian, like "56 34 12"
            UInt64 value;
            try
            {
                value = signed ? (UInt64)((Int64)new System.ComponentModel.Int64Converter().ConvertFromString(subEntry.DefaultValue))
                               : (UInt64)new System.ComponentModel.UInt64Converter().ConvertFromString(subEntry.DefaultValue);
            }
            catch (Exception)
            {
                value = 0;
            }

            List<string> bytes = new List<string>();
            for (UInt32 i = 0; i < byteLength; i++)
            {
                bytes.Add(String.Format("{0:X2}", value & 0xFF));
                value >>= 8;
            }
            subEntry.DataType = LibCanOpen.OdSubObject.Types.DataType.OctetString;
            subEntry.DefaultValue = string.Join(" ", bytes);

            return Items1ChoiceType.BITSTRING;
        }

        private void ConvertDataType(Items1ChoiceType choiceType, OdSubObject OdSubObject)
        {
            switch (choiceType)
            {
                case Items1ChoiceType.BOOL:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Boolean;
                    break;
                case Items1ChoiceType.CHAR:
                case Items1ChoiceType.SINT:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Integer8;
                    break;
                case Items1ChoiceType.INT:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Integer16;
                    break;
                case Items1ChoiceType.DINT:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Integer32;
                    break;
                case Items1ChoiceType.LINT:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Integer64;
                    break;
                case Items1ChoiceType.BYTE:
                case Items1ChoiceType.USINT:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Unsigned8;
                    break;
                case Items1ChoiceType.WORD:
                case Items1ChoiceType.UINT:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Unsigned16;
                    break;
                case Items1ChoiceType.DWORD:
                case Items1ChoiceType.UDINT:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Unsigned32;
                    break;
                case Items1ChoiceType.LWORD:
                case Items1ChoiceType.ULINT:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Unsigned64;
                    break;
                case Items1ChoiceType.REAL:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Real32;
                    break;
                case Items1ChoiceType.LREAL:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Real64;
                    break;
                case Items1ChoiceType.STRING:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.VisibleString;
                    break;
                case Items1ChoiceType.WSTRING:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.UnicodeString;
                    break;
                case Items1ChoiceType.BITSTRING:
                    OdSubObject.DataType = OdSubObject.DefaultValue == "" ? LibCanOpen.OdSubObject.Types.DataType.Domain : LibCanOpen.OdSubObject.Types.DataType.OctetString;
                    break;
                default:
                    OdSubObject.DataType = LibCanOpen.OdSubObject.Types.DataType.Integer32;
                    break;
            }
        }

        private void WriteVar(parameter devPar, ODentry od)
        {
            if (od.accesstype == EDSsharp.AccessType.UNKNOWN && od.parent != null && od.parent.objecttype == ObjectType.ARRAY)
                od.accesstype = od.parent.accesstype;

            devPar.access = ConvertAccessType(od.accesstype);

            devPar.Items1 = new object[] { new object() };
            devPar.Items1ElementName = new Items1ChoiceType[] { ConvertDataType(od) };

            if (od.defaultvalue != null && od.defaultvalue != "")
                    devPar.defaultValue = new defaultValue { value = od.defaultvalue };

            if (od.LowLimit != null && od.LowLimit != "" && od.HighLimit != null && od.HighLimit != "")
            {
                devPar.allowedValues = new allowedValues
                {
                    range = new range[]
                    {
                        new range
                        {
                            minValue = new rangeMinValue { value = od.LowLimit },
                            maxValue = new rangeMaxValue { value = od.HighLimit }
                        }
                    }
                };
            }
        }

        private void WriteVar(parameter devPar, OdSubObject subEntry)
        {
            devPar.access = ConvertAccessType(subEntry);

            devPar.Items1 = new object[] { new object() };
            devPar.Items1ElementName = new Items1ChoiceType[] { ConvertDataType(subEntry) };

            if (subEntry.DefaultValue != null && subEntry.DefaultValue != "")
                devPar.defaultValue = new defaultValue { value = subEntry.DefaultValue };

            if (subEntry.LowLimit != null && subEntry.LowLimit != "" && subEntry.HighLimit != null && subEntry.HighLimit != "")
            {
                devPar.allowedValues = new allowedValues
                {
                    range = new range[]
                    {
                        new range
                        {
                            minValue = new rangeMinValue { value = subEntry.LowLimit },
                            maxValue = new rangeMaxValue { value = subEntry.HighLimit }
                        }
                    }
                };
            }
        }

        private void ConvertXddProperties(property[] properties, OdObject entry, OdSubObject subEntry)
        {
            if (properties != null)
            {
                foreach (property prop in properties)
                {
                    switch (prop.name)
                    {
                        case "CO_disabled": entry.Disabled = prop.value == "true"; break;
                        case "CO_countLabel": entry.CountLabel = prop.value ?? ""; break;
                        case "CO_storageGroup": entry.StorageGroup = prop.value ?? ""; break;
                        case "CO_flagsPDO": entry.FlagsPDO = prop.value == "true"; break;
                        case "CO_accessSRDO":
                            if (prop.value != null)
                                switch (prop.value)
                                {
                                    case "rx": subEntry.Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.Rx; break;
                                    case "tx": subEntry.Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.Tx; break;
                                    case "trx": subEntry.Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.Trx; break;
                                    case "no": subEntry.Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No; break;
                                }
                            break;
                        case "CO_stringLengthMin":
                            try { subEntry.StringLengthMin = System.Convert.ToUInt16(prop.value); }
                            catch (Exception) { subEntry.StringLengthMin = 0; }
                            break;
                    }
                }
            }
        }

        private property[] ConvertXddProperties(OdObject entry)
        {
            var props = new List<property>();

            if (entry.Disabled)
                props.Add(new property { name = "CO_disabled", value = "true" });
            if (entry.CountLabel != "")
                props.Add(new property { name = "CO_countLabel", value = entry.CountLabel });
            if (entry.StorageGroup != "RAM" && entry.StorageGroup != "")
                props.Add(new property { name = "CO_storageGroup", value = entry.StorageGroup });
            if (entry.FlagsPDO)
                props.Add(new property { name = "CO_flagsPDO", value = "true" });

            return props.ToArray();
        }

        private property[] ConvertXddProperties(OdSubObject subEntry)
        {
            var props = new List<property>();
            string val;

            switch (subEntry.Srdo)
            {
                case LibCanOpen.OdSubObject.Types.AccessSRDO.Rx: val = "rx"; break;
                case LibCanOpen.OdSubObject.Types.AccessSRDO.Tx: val = "tx"; break;
                case LibCanOpen.OdSubObject.Types.AccessSRDO.Trx: val = "trx"; break;
                default: val = "no"; break;
            }
            props.Add(new property { name = "CO_accessSRDO", value = val });

            if (subEntry.StringLengthMin != 0)
                props.Add(new property { name = "CO_stringLengthMin", value = subEntry.StringLengthMin.ToString() });

            return props.ToArray();
        }

        private ISO15745ProfileContainer Convert(EDSsharp eds, string fileName, bool deviceCommissioning, bool stripped)
        {
            // Get xdd template from eds (if memorized on xdd file open)
            ISO15745ProfileContainer container = eds.xddTemplate;

            ProfileBody_Device_CANopen body_device = null;
            ProfileBody_CommunicationNetwork_CANopen body_network = null;

            List<string> mappingErrors = eds.VerifyPDOMapping();
            if (mappingErrors.Count > 0)
                Warnings.AddWarning($"Errors in PDO mappings:\r\n    " + string.Join("\r\n    ", mappingErrors), Warnings.warning_class.WARNING_BUILD);

            eds.fi.ModificationDateTime = DateTime.Now;

            #region base_elements
            // create required xml objects, where necessay
            if (container == null)
                container = new ISO15745ProfileContainer();
            if (container.ISO15745Profile == null
                || container.ISO15745Profile.Length < 2
                || container.ISO15745Profile[0] == null
                || container.ISO15745Profile[1] == null)
            {
                container.ISO15745Profile = new ISO15745Profile[]
                {
                    new ISO15745Profile(),
                    new ISO15745Profile(),
                };
            }

            // get headers and bodies
            if (container.ISO15745Profile[0].ProfileHeader != null
                && container.ISO15745Profile[0].ProfileBody != null
                && container.ISO15745Profile[1].ProfileHeader != null
                && container.ISO15745Profile[1].ProfileBody != null)
            {
                foreach (ISO15745Profile item in container.ISO15745Profile)
                {
                    if (item.ProfileBody.GetType() == typeof(ProfileBody_Device_CANopen))
                        body_device = (ProfileBody_Device_CANopen)item.ProfileBody;
                    else if (item.ProfileBody.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopen))
                        body_network = (ProfileBody_CommunicationNetwork_CANopen)item.ProfileBody;
                }
            }
            if (body_network == null || body_device == null)
            {
                container.ISO15745Profile[0].ProfileHeader = new ProfileHeader_DataType
                {
                    ProfileIdentification = "CANopen device profile",
                    ProfileRevision = "1.1",
                    ProfileName = "",
                    ProfileSource = "",
                    ProfileClassID = ProfileClassID_DataType.Device,
                    ISO15745Reference = new ISO15745Reference_DataType
                    {
                        ISO15745Part = "1",
                        ISO15745Edition = "1",
                        ProfileTechnology = "CANopen"
                    }
                };
                container.ISO15745Profile[0].ProfileBody = new ProfileBody_Device_CANopen();
                body_device = (ProfileBody_Device_CANopen)container.ISO15745Profile[0].ProfileBody;

                container.ISO15745Profile[1].ProfileHeader = new ProfileHeader_DataType
                {
                    ProfileIdentification = "CANopen communication network profile",
                    ProfileRevision = "1.1",
                    ProfileName = "",
                    ProfileSource = "",
                    ProfileClassID = ProfileClassID_DataType.CommunicationNetwork,
                    ISO15745Reference = new ISO15745Reference_DataType
                    {
                        ISO15745Part = "1",
                        ISO15745Edition = "1",
                        ProfileTechnology = "CANopen"
                    }
                };
                container.ISO15745Profile[1].ProfileBody = new ProfileBody_CommunicationNetwork_CANopen();
                body_network = (ProfileBody_CommunicationNetwork_CANopen)container.ISO15745Profile[1].ProfileBody;
            }
            #endregion

            #region ObjectDictionary
            var body_network_objectList = new List<CANopenObjectListCANopenObject>();
            var body_device_parameterList = new List<parameter>();
            var body_device_arrayList = new List<array>();
            var body_device_structList = new List<@struct>();

            foreach (ODentry od in eds.ods.Values)
            {
                if (stripped && od.prop.CO_disabled == true)
                    continue;

                string uid = string.Format("{0:X4}", od.Index);

                var netObj = new CANopenObjectListCANopenObject
                {
                    index = new byte[] { (byte)(od.Index >> 8), (byte)(od.Index & 0xFF) },
                    name = od.parameter_name,
                    objectType = (byte)od.objecttype,
                    uniqueIDRef = "UID_OBJ_" + uid
                };
                body_network_objectList.Add(netObj);

                var devPar = new parameter { uniqueID = "UID_OBJ_" + uid };
                if (od.Description != null && od.Description != "")
                {
                    devPar.Items = new object[] { new vendorTextDescription { lang = "en", Value = od.Description } };
                }
                else
                {
                    // Add at least label made from parameter name, because g_labels is required by schema
                    devPar.Items = new object[] { new vendorTextLabel { lang = "en", Value = od.parameter_name } };
                }
                if (deviceCommissioning && od.denotation != null && od.denotation != "")
                {
                    devPar.denotation = new denotation
                    {
                        Items = new object[] { new vendorTextLabel { lang = "en", Value = od.denotation } }
                    };
                }
                body_device_parameterList.Add(devPar);

                if (od.objecttype == ObjectType.VAR)
                {
                    try { netObj.PDOmapping = (CANopenObjectListCANopenObjectPDOmapping)System.Enum.Parse(typeof(CANopenObjectListCANopenObjectPDOmapping), od.PDOtype.ToString()); }
                    catch (Exception) { netObj.PDOmapping = CANopenObjectListCANopenObjectPDOmapping.no; }

                    netObj.PDOmappingSpecified = true;

                    if (!stripped)
                    {
                        var propOd = od.prop.OdeXdd();
                        var propSub = od.prop.SubOdeXdd();
                        devPar.property = new property[propOd.Length + propSub.Length];
                        propOd.CopyTo(devPar.property, 0);
                        propSub.CopyTo(devPar.property, propOd.Length);
                    }

                    WriteVar(devPar, od);
                    if (deviceCommissioning && od.actualvalue != null && od.actualvalue != "")
                        devPar.actualValue = new actualValue { value = od.actualvalue };
                }
                else if ((od.objecttype == ObjectType.ARRAY || od.objecttype == ObjectType.RECORD) && od.subobjects != null && od.subobjects.Count > 0)
                {
                    netObj.subNumber = (byte)od.subobjects.Count;
                    netObj.subNumberSpecified = true;

                    if (!stripped)
                        devPar.property = od.prop.OdeXdd();

                    var netSubObjList = new List<CANopenObjectListCANopenObjectCANopenSubObject>();
                    var devStructSubList = new List<varDeclaration>();

                    foreach (KeyValuePair<UInt16, ODentry> kvp in od.subobjects)
                    {
                        ODentry subod = kvp.Value;
                        UInt16 subindex = kvp.Key;
                        string subUid = string.Format("{0:X4}{1:X2}", od.Index, subindex);

                        var netSubObj = new CANopenObjectListCANopenObjectCANopenSubObject
                        {
                            subIndex = new byte[] { (byte)(subindex & 0xFF) },
                            name = subod.parameter_name,
                            objectType = (byte)ObjectType.VAR,
                            PDOmappingSpecified = true,
                            uniqueIDRef = "UID_SUB_" + subUid
                        };
                        try { netSubObj.PDOmapping = (CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping)System.Enum.Parse(typeof(CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping), subod.PDOtype.ToString()); }
                        catch (Exception) { netSubObj.PDOmapping = CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping.no; }

                        var devSubPar = new parameter {
                            uniqueID = "UID_SUB_" + subUid
                        };
                        if (subod.Description != null && subod.Description != "")
                        {
                            devSubPar.Items = new object[] { new vendorTextDescription { lang = "en", Value = subod.Description } };
                        }
                        else
                        {
                            // Add at least label made from parameter name, because g_labels is required by schema
                            devSubPar.Items = new object[] { new vendorTextLabel { lang = "en", Value = subod.parameter_name } };
                        }
                        if (!stripped)
                            devSubPar.property = subod.prop.SubOdeXdd();
                        if (deviceCommissioning && subod.denotation != null && subod.denotation != "")
                        {
                            devPar.denotation = new denotation
                            {
                                Items = new object[] { new vendorTextLabel { lang = "en", Value = subod.denotation } }
                            };
                        }
                        WriteVar(devSubPar, subod);
                        if (deviceCommissioning && subod.actualvalue != null && subod.actualvalue != "")
                            devPar.actualValue = new actualValue { value = subod.actualvalue };

                        if (od.objecttype == ObjectType.RECORD)
                        {
                            devStructSubList.Add(new varDeclaration
                            {
                                name = subod.parameter_name,
                                uniqueID = "UID_RECSUB_" + subUid,
                                Item = new object(),
                                ItemElementName = (ItemChoiceType1)ConvertDataType(subod)
                            });
                        }

                        body_device_parameterList.Add(devSubPar);
                        netSubObjList.Add(netSubObj);
                    }

                    // add refference to data type definition and definition of array or struct data type (schema requirement)
                    if (od.objecttype == ObjectType.ARRAY)
                    {
                        devPar.Items1 = new object[] { new dataTypeIDRef { uniqueIDRef = "UID_ARR_" + uid } };
                        devPar.Items1ElementName = new Items1ChoiceType[] { Items1ChoiceType.dataTypeIDRef };
                        body_device_arrayList.Add(new array
                        {
                            uniqueID = "UID_ARR_" + uid,
                            name = od.parameter_name,
                            Item = new object(),
                            ItemElementName = (ItemChoiceType)ConvertDataType(od),
                            subrange = new subrange[] { new subrange { lowerLimit = 0, upperLimit = od.subobjects.Count - 1 } }
                        });
                    }
                    else
                    {
                        devPar.Items1 = new object[] { new dataTypeIDRef { uniqueIDRef = "UID_REC_" + uid } };
                        devPar.Items1ElementName = new Items1ChoiceType[] { Items1ChoiceType.dataTypeIDRef };
                        body_device_structList.Add(new @struct
                        {
                            uniqueID = "UID_REC_" + uid,
                            name = od.parameter_name,
                            varDeclaration = devStructSubList.ToArray()
                        });
                    }

                    netObj.CANopenSubObject = netSubObjList.ToArray();
                }
            }
            #endregion

            #region body_device
            body_device.fileName = fileName;
            body_device.fileCreator = eds.fi.CreatedBy;
            body_device.fileCreationDate = eds.fi.CreationDateTime;
            body_device.fileCreationTime = eds.fi.CreationDateTime;
            body_device.fileCreationTimeSpecified = true;
            body_device.fileVersion = eds.fi.FileVersion;
            body_device.fileModifiedBy = eds.fi.ModifiedBy;
            body_device.fileModificationDate = eds.fi.ModificationDateTime;
            body_device.fileModificationTime = eds.fi.ModificationDateTime;
            body_device.fileModificationDateSpecified = true;
            body_device.fileModificationTimeSpecified = true;
            body_device.supportedLanguages = "en";

            // Device identity
            if (body_device.DeviceIdentity == null)
                body_device.DeviceIdentity = new DeviceIdentity();
            body_device.DeviceIdentity.vendorName = new vendorName { Value = eds.di.VendorName };
            body_device.DeviceIdentity.vendorID = new vendorID { Value = eds.di.VendorNumber };
            body_device.DeviceIdentity.productName = new productName { Value = eds.di.ProductName };
            body_device.DeviceIdentity.productID = new productID { Value = eds.di.ProductNumber };
            if (eds.fi.Description != null && eds.fi.Description != "")
            {
                body_device.DeviceIdentity.productText = new productText
                {
                    Items = new object[]
                    {
                        new vendorTextDescription { lang = "en", Value = eds.fi.Description }
                    }
                };
            }

            // version is optional element, make a template if empty
            if (body_device.DeviceIdentity.version == null)
            {
                body_device.DeviceIdentity.version = new version[]
                {
                    new version { versionType = versionVersionType.SW, Value = "0" },
                    new version { versionType = versionVersionType.FW, Value = "0" },
                    new version { versionType = versionVersionType.HW, Value = "0" }
                };
            }

            // DeviceFunction is required by schema, make a template if empty.
            if (body_device.DeviceFunction == null)
            {
                // This is just a template for somehow complex xml structure. Users can edit the
                // xdd file directly to write characteristics of own devices or use generic xml
                // editing tool. External editing will be preserved anyway, if xdd file will be
                // later opened and saved back in EDSEditor.
                // EDSEditor curerently does not have interface for editing the characteristics.
                body_device.DeviceFunction = new DeviceFunction[]
                {
                    new DeviceFunction
                    {
                        capabilities = new capabilities
                        {
                            characteristicsList = new characteristicsList[]
                            {
                                new characteristicsList
                                {
                                    characteristic = new characteristic[]
                                    {
                                        new characteristic
                                        {
                                            characteristicName = new characteristicName
                                            {
                                                Items = new object[]
                                                {
                                                    new vendorTextLabel { lang = "en", Value = "SW library" }
                                                }
                                            },
                                            characteristicContent = new characteristicContent[]
                                            {
                                                new characteristicContent {
                                                    Items = new object[]
                                                    {
                                                        new vendorTextLabel { lang = "en", Value = "libedssharp" }
                                                    }
                                                },
                                                new characteristicContent {
                                                    Items = new object[]
                                                    {
                                                        new vendorTextLabel { lang = "en", Value = "CANopenNode" }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }

            // ApplicationProcess (insert object dictionary)
            if (body_device.ApplicationProcess == null)
                body_device.ApplicationProcess = new ApplicationProcess[1];
            if (body_device.ApplicationProcess[0] == null)
                body_device.ApplicationProcess[0] = new ApplicationProcess();
            body_device.ApplicationProcess[0].dataTypeList = new dataTypeList
            {
                array = body_device_arrayList.ToArray(),
                @struct = body_device_structList.ToArray()
            };
            body_device.ApplicationProcess[0].parameterList = body_device_parameterList.ToArray();
            #endregion

            #region body_network
            body_network.fileName = fileName;
            body_network.fileCreator = eds.fi.CreatedBy;
            body_network.fileCreationDate = eds.fi.CreationDateTime;
            body_network.fileCreationTime = eds.fi.CreationDateTime;
            body_network.fileCreationTimeSpecified = true;
            body_network.fileVersion = eds.fi.FileVersion;
            body_network.fileModificationDate = eds.fi.ModificationDateTime;
            body_network.fileModificationTime = eds.fi.ModificationDateTime;
            body_network.fileModificationDateSpecified = true;
            body_network.fileModificationTimeSpecified = true;
            body_network.supportedLanguages = "en";

            // base elements
            ProfileBody_CommunicationNetwork_CANopenApplicationLayers ApplicationLayers = null;
            ProfileBody_CommunicationNetwork_CANopenTransportLayers TransportLayers = null;
            ProfileBody_CommunicationNetwork_CANopenNetworkManagement NetworkManagement = null;
            if (body_network.Items != null && body_network.Items.Length >= 3)
            {
                foreach (object item in body_network.Items)
                {
                    if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        ApplicationLayers = (ProfileBody_CommunicationNetwork_CANopenApplicationLayers)item;
                    else if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        TransportLayers = (ProfileBody_CommunicationNetwork_CANopenTransportLayers)item;
                    else if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        NetworkManagement = (ProfileBody_CommunicationNetwork_CANopenNetworkManagement)item;
                }
            }
            if (ApplicationLayers == null || TransportLayers == null || NetworkManagement == null)
            {
                body_network.Items = new object[3];
                body_network.Items[0] = new ProfileBody_CommunicationNetwork_CANopenApplicationLayers();
                ApplicationLayers = (ProfileBody_CommunicationNetwork_CANopenApplicationLayers)body_network.Items[0];
                body_network.Items[1] = new ProfileBody_CommunicationNetwork_CANopenTransportLayers();
                TransportLayers = (ProfileBody_CommunicationNetwork_CANopenTransportLayers)body_network.Items[1];
                body_network.Items[2] = new ProfileBody_CommunicationNetwork_CANopenNetworkManagement();
                NetworkManagement = (ProfileBody_CommunicationNetwork_CANopenNetworkManagement)body_network.Items[2];
            }

            // ApplicationLayers -> dummyUsage
            ApplicationLayers.dummyUsage = new ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummy[7];
            for (int x = 0; x < 7; x++)
                ApplicationLayers.dummyUsage[x] = new ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummy();

            ApplicationLayers.dummyUsage[0].entry = eds.du.Dummy0001
                ? ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00011
                : ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00010;
            ApplicationLayers.dummyUsage[1].entry = eds.du.Dummy0002
                ? ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00021
                : ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00020;
            ApplicationLayers.dummyUsage[2].entry = eds.du.Dummy0003
                ? ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00031
                : ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00030;
            ApplicationLayers.dummyUsage[3].entry = eds.du.Dummy0004
                ? ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00041
                : ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00040;
            ApplicationLayers.dummyUsage[4].entry = eds.du.Dummy0005
                ? ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00051
                : ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00050;
            ApplicationLayers.dummyUsage[5].entry = eds.du.Dummy0006
                ? ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00061
                : ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00060;
            ApplicationLayers.dummyUsage[6].entry = eds.du.Dummy0007
                ? ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00071
                : ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00070;

            // ApplicationLayers -> CANopenObjectList (insert object dictionary)
            ApplicationLayers.CANopenObjectList = new CANopenObjectList
            {
                CANopenObject = body_network_objectList.ToArray()
            };

            // TransportLayers -> supportedBaudRate
            List<ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue> bauds;
            bauds = new List<ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue>();
            if (eds.di.BaudRate_10)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item10Kbps);
            if (eds.di.BaudRate_20)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item20Kbps);
            if (eds.di.BaudRate_50)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item50Kbps);
            if (eds.di.BaudRate_125)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item125Kbps);
            if (eds.di.BaudRate_250)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item250Kbps);
            if (eds.di.BaudRate_500)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item500Kbps);
            if (eds.di.BaudRate_800)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item800Kbps);
            if (eds.di.BaudRate_1000)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item1000Kbps);
            if (eds.di.BaudRate_auto)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.autobaudRate);
            TransportLayers.PhysicalLayer = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayer
            {
                baudRate = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRate
                {
                    supportedBaudRate = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate[bauds.Count]
                }
            };
            for (int i = 0; i < bauds.Count; i++)
            {
                TransportLayers.PhysicalLayer.baudRate.supportedBaudRate[i] = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate
                {
                    value = bauds[i]
                };
            }

            // NetworkManagement
            if (NetworkManagement.CANopenGeneralFeatures == null)
                NetworkManagement.CANopenGeneralFeatures = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementCANopenGeneralFeatures();
            NetworkManagement.CANopenGeneralFeatures.granularity = eds.di.Granularity; // required parameter
            NetworkManagement.CANopenGeneralFeatures.nrOfRxPDO = eds.di.NrOfRXPDO;
            NetworkManagement.CANopenGeneralFeatures.nrOfTxPDO = eds.di.NrOfTXPDO;

            NetworkManagement.CANopenGeneralFeatures.ngSlave = eds.di.NG_Slave;
            NetworkManagement.CANopenGeneralFeatures.ngMaster = eds.di.NG_Master;
            NetworkManagement.CANopenGeneralFeatures.NrOfNG_MonitoredNodes = eds.di.NrOfNG_MonitoredNodes;

            NetworkManagement.CANopenGeneralFeatures.layerSettingServiceSlave = eds.di.LSS_Supported;
            // not handled by GUI
            NetworkManagement.CANopenGeneralFeatures.groupMessaging = eds.di.GroupMessaging;
            if (eds.di.DynamicChannelsSupported)
                NetworkManagement.CANopenGeneralFeatures.dynamicChannels = 1;
            NetworkManagement.CANopenGeneralFeatures.bootUpSlave = eds.di.SimpleBootUpSlave;

            if (NetworkManagement.CANopenMasterFeatures == null)
                NetworkManagement.CANopenMasterFeatures = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementCANopenMasterFeatures();
            NetworkManagement.CANopenMasterFeatures.layerSettingServiceMaster = eds.di.LSS_Master;
            // not handled by GUI
            NetworkManagement.CANopenMasterFeatures.bootUpMaster = eds.di.SimpleBootUpMaster;

            if (deviceCommissioning)
            {
                NetworkManagement.deviceCommissioning = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementDeviceCommissioning
                {
                    NodeID = eds.dc.NodeID,
                    nodeName = eds.dc.NodeName,
                    actualBaudRate = eds.dc.Baudrate.ToString(),
                    networkNumber = eds.dc.NetNumber,
                    networkName = eds.dc.NetworkName,
                    CANopenManager = eds.dc.CANopenManager
                };
            }
            else
            {
                NetworkManagement.deviceCommissioning = null;
            }
            #endregion

            return container;
        }

        private string G_label_getDescription(object[] items) {
            if (items != null)
            {
                foreach (object o in items)
                {
                    if (o.GetType() == typeof(vendorTextDescription))
                    {
                        return ((vendorTextDescription)o).Value ?? "";
                    }
                }
            }
            return "";
        }

        private EDSsharp Convert(ISO15745ProfileContainer container)
        {
            EDSsharp eds = new EDSsharp();

            ProfileBody_Device_CANopen body_device = null;
            ProfileBody_CommunicationNetwork_CANopen body_network = null;
            ProfileBody_CommunicationNetwork_CANopenApplicationLayers ApplicationLayers = null;
            var parameters = new Dictionary<string, parameter>();

            foreach (ISO15745Profile item in container.ISO15745Profile)
            {
                if (item.ProfileBody.GetType() == typeof(ProfileBody_Device_CANopen))
                    body_device = (ProfileBody_Device_CANopen)item.ProfileBody;
                else if (item.ProfileBody.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopen))
                    body_network = (ProfileBody_CommunicationNetwork_CANopen)item.ProfileBody;
            }

            if (body_device != null)
            {
                eds.fi.FileName = body_device.fileName ?? "";
                eds.fi.FileVersion = body_device.fileVersion ?? "";
                eds.fi.CreatedBy = body_device.fileCreator ?? "";
                eds.fi.ModifiedBy = body_device.fileModifiedBy ?? "";

                if (body_device.fileCreationTimeSpecified)
                {
                    eds.fi.CreationDateTime = body_device.fileCreationDate.Add(body_device.fileCreationTime.TimeOfDay);
                    eds.fi.CreationDate = eds.fi.CreationDateTime.ToString("MM-dd-yyyy");
                    eds.fi.CreationTime = eds.fi.CreationDateTime.ToString("h:mmtt");

                }
                if (body_device.fileModificationDateSpecified)
                {
                    eds.fi.ModificationDateTime = body_device.fileModificationDate.Add(body_device.fileModificationTime.TimeOfDay);
                    eds.fi.ModificationDate = eds.fi.ModificationDateTime.ToString("MM-dd-yyyy");
                    eds.fi.ModificationTime = eds.fi.ModificationDateTime.ToString("h:mmtt");
                }

                if (body_device.DeviceIdentity != null)
                {
                    if (body_device.DeviceIdentity.vendorName != null)
                        eds.di.VendorName = body_device.DeviceIdentity.vendorName.Value ?? "";
                    if (body_device.DeviceIdentity.vendorID != null)
                        eds.di.VendorNumber = body_device.DeviceIdentity.vendorID.Value ?? "";
                    if (body_device.DeviceIdentity.productName != null)
                        eds.di.ProductName = body_device.DeviceIdentity.productName.Value ?? "";
                    if (body_device.DeviceIdentity.productID != null)
                        eds.di.ProductNumber = body_device.DeviceIdentity.productID.Value ?? "";
                    if (body_device.DeviceIdentity.productText != null)
                        eds.fi.Description = G_label_getDescription(body_device.DeviceIdentity.productText.Items);
                }

                if (body_device.ApplicationProcess != null
                    && body_device.ApplicationProcess[0] != null
                    && body_device.ApplicationProcess[0].parameterList != null)
                {
                    foreach (parameter param in body_device.ApplicationProcess[0].parameterList)
                    {
                        if (!parameters.ContainsKey(param.uniqueID))
                            parameters.Add(param.uniqueID, param);
                    }
                }
            }

            if (body_network != null)
            {
                ProfileBody_CommunicationNetwork_CANopenTransportLayers TransportLayers = null;
                ProfileBody_CommunicationNetwork_CANopenNetworkManagement NetworkManagement = null;

                foreach (object item in body_network.Items)
                {
                    if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        ApplicationLayers = (ProfileBody_CommunicationNetwork_CANopenApplicationLayers)item;
                    else if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenTransportLayers))
                        TransportLayers = (ProfileBody_CommunicationNetwork_CANopenTransportLayers)item;
                    else if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenNetworkManagement))
                        NetworkManagement = (ProfileBody_CommunicationNetwork_CANopenNetworkManagement)item;
                }

                if (TransportLayers != null && TransportLayers.PhysicalLayer != null
                    && TransportLayers.PhysicalLayer.baudRate != null && TransportLayers.PhysicalLayer.baudRate.supportedBaudRate != null)
                {
                    foreach (ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate baud in TransportLayers.PhysicalLayer.baudRate.supportedBaudRate)
                    {
                        switch (baud.value.ToString())
                        {
                            case "Item10Kbps": eds.di.BaudRate_10 = true; break;
                            case "Item20Kbps": eds.di.BaudRate_20 = true; break;
                            case "Item50Kbps": eds.di.BaudRate_50 = true; break;
                            case "Item125Kbps": eds.di.BaudRate_125 = true; break;
                            case "Item250Kbps": eds.di.BaudRate_250 = true; break;
                            case "Item500Kbps": eds.di.BaudRate_500 = true; break;
                            case "Item800Kbps": eds.di.BaudRate_800 = true; break;
                            case "Item1000Kbps": eds.di.BaudRate_1000 = true; break;
                            case "autobaudRate": eds.di.BaudRate_auto = true; break;
                        }
                    }
                }

                if (NetworkManagement != null)
                {
                    if (NetworkManagement.CANopenGeneralFeatures != null)
                    {
                        eds.di.Granularity = NetworkManagement.CANopenGeneralFeatures.granularity;
                        eds.di.NrOfRXPDO = NetworkManagement.CANopenGeneralFeatures.nrOfRxPDO;
                        eds.di.NrOfTXPDO = NetworkManagement.CANopenGeneralFeatures.nrOfTxPDO;
                        eds.di.LSS_Supported = NetworkManagement.CANopenGeneralFeatures.layerSettingServiceSlave;
                        eds.di.NG_Slave = NetworkManagement.CANopenGeneralFeatures.ngSlave;
                        eds.di.NG_Master = NetworkManagement.CANopenGeneralFeatures.ngMaster;
                        eds.di.NrOfNG_MonitoredNodes = NetworkManagement.CANopenGeneralFeatures.NrOfNG_MonitoredNodes;
                        // not handled by GUI
                        eds.di.GroupMessaging = NetworkManagement.CANopenGeneralFeatures.groupMessaging;
                        eds.di.DynamicChannelsSupported = NetworkManagement.CANopenGeneralFeatures.dynamicChannels > 0;
                        eds.di.SimpleBootUpSlave = NetworkManagement.CANopenGeneralFeatures.bootUpSlave;

                    }

                    if (NetworkManagement.CANopenMasterFeatures != null)
                    {
                        eds.di.LSS_Master = NetworkManagement.CANopenMasterFeatures.layerSettingServiceMaster;
                        // not handled by GUI
                        eds.di.SimpleBootUpMaster = NetworkManagement.CANopenMasterFeatures.bootUpMaster;
                    }

                    if (NetworkManagement.deviceCommissioning != null)
                    {
                        eds.dc.NodeID = NetworkManagement.deviceCommissioning.NodeID;
                        try { eds.dc.Baudrate = System.Convert.ToUInt16(NetworkManagement.deviceCommissioning.actualBaudRate); }
                        catch (Exception) { eds.dc.Baudrate = 0; }
                        eds.dc.CANopenManager = NetworkManagement.deviceCommissioning.CANopenManager;
                        eds.dc.NetworkName = NetworkManagement.deviceCommissioning.networkName;
                        try { eds.dc.NetNumber = System.Convert.ToUInt16(NetworkManagement.deviceCommissioning.networkNumber); }
                        catch (Exception) { eds.dc.NetNumber = 0; }
                        eds.dc.NodeName = NetworkManagement.deviceCommissioning.nodeName;

                    }
                }

                if (ApplicationLayers != null)
                {
                    if (ApplicationLayers.dummyUsage != null)
                    {
                        foreach (ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummy dummy in ApplicationLayers.dummyUsage)
                        {
                            string pat = @"Dummy([0-9]{4})([0-1])";
                            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                            Match m = r.Match(dummy.entry.ToString());

                            if (m.Success)
                            {
                                int index = int.Parse(m.Groups[1].Value);
                                bool used = int.Parse(m.Groups[2].Value) == 1;

                                switch (index)
                                {
                                    case 1: eds.du.Dummy0001 = used; break;
                                    case 2: eds.du.Dummy0002 = used; break;
                                    case 3: eds.du.Dummy0003 = used; break;
                                    case 4: eds.du.Dummy0004 = used; break;
                                    case 5: eds.du.Dummy0005 = used; break;
                                    case 6: eds.du.Dummy0006 = used; break;
                                    case 7: eds.du.Dummy0007 = used; break;
                                }
                            }
                        }
                    }

                    if (ApplicationLayers.CANopenObjectList != null && ApplicationLayers.CANopenObjectList.CANopenObject != null)
                    {
                        foreach (CANopenObjectListCANopenObject netObj in ApplicationLayers.CANopenObjectList.CANopenObject)
                        {
                            if (netObj.index == null || netObj.index.Length != 2)
                                continue;

                            UInt16 index = (UInt16)((netObj.index[0] << 8) | netObj.index[1]);

                            EDSsharp.AccessType accessType;
                            if (netObj.accessTypeSpecified)
                            {
                                try { accessType = (EDSsharp.AccessType)System.Enum.Parse(typeof(EDSsharp.AccessType), netObj.accessType.ToString()); }
                                catch (Exception) { accessType = EDSsharp.AccessType.ro; }
                            }
                            else {
                                accessType = EDSsharp.AccessType.ro;
                            }

                            PDOMappingType PDOtype;
                            if (netObj.PDOmappingSpecified)
                            {
                                try { PDOtype = (PDOMappingType)System.Enum.Parse(typeof(PDOMappingType), netObj.PDOmapping.ToString()); }
                                catch (Exception) { PDOtype = PDOMappingType.no; }
                            }
                            else
                            {
                                PDOtype = PDOMappingType.no;
                            }

                            if (accessType == EDSsharp.AccessType.rw)
                            {
                                if (PDOtype == PDOMappingType.RPDO)
                                    accessType = EDSsharp.AccessType.rww;
                                else if (PDOtype == PDOMappingType.TPDO)
                                    accessType = EDSsharp.AccessType.rwr;
                            }

                            ODentry od = new ODentry
                            {
                                Index = index,
                                parameter_name = netObj.name ?? "",
                                objecttype = (ObjectType)netObj.objectType,
                                PDOtype = PDOtype,
                                // following values are optional and may be overriden by parameters from body_device
                                accesstype = accessType,
                                datatype = netObj.dataType != null && netObj.dataType.Length == 1 ? (DataType)netObj.dataType[0] : DataType.UNKNOWN,
                                defaultvalue = netObj.defaultValue ?? "",
                                actualvalue = netObj.actualValue ?? "",
                                denotation = netObj.denotation ?? "",
                                LowLimit = netObj.lowLimit ?? "",
                                HighLimit = netObj.highLimit ?? "",
                                ObjFlags = netObj.objFlags != null && netObj.objFlags.Length == 2 ? netObj.objFlags[1] : (byte)0,
                                uniqueID = netObj.uniqueIDRef ?? ""
                            };

                            if (netObj.uniqueIDRef != null && netObj.uniqueIDRef != "" && parameters.ContainsKey(netObj.uniqueIDRef))
                            {
                                parameter devPar = parameters[netObj.uniqueIDRef];

                                od.Description = G_label_getDescription(devPar.Items);

                                od.accesstype = ConvertAccessType(devPar.access);

                                if (devPar.defaultValue != null && devPar.defaultValue.value != null)
                                    od.defaultvalue = devPar.defaultValue.value;

                                if (devPar.Items1 != null && devPar.Items1ElementName != null)
                                    od.datatype = ConvertDataType(devPar.Items1ElementName[0], od.defaultvalue);

                                if (devPar.actualValue != null && devPar.actualValue.value != null)
                                    od.actualvalue = devPar.actualValue.value;

                                if (devPar.denotation != null)
                                    od.denotation = G_label_getDescription(devPar.denotation.Items);

                                if (devPar.allowedValues != null && devPar.allowedValues.range != null && devPar.allowedValues.range[0] != null)
                                {
                                    range r = devPar.allowedValues.range[0];
                                    if (r.minValue != null) od.LowLimit = r.minValue.value ?? "";
                                    if (r.maxValue != null) od.HighLimit = r.maxValue.value ?? "";
                                }

                                od.prop.OdeXdd(devPar.property);
                            }

                            if (netObj.CANopenSubObject != null)
                            {
                                foreach (CANopenObjectListCANopenObjectCANopenSubObject netSubObj in netObj.CANopenSubObject)
                                {
                                    if (netSubObj.subIndex == null || netSubObj.subIndex.Length != 1)
                                        continue;

                                    UInt16 subIndex = (UInt16)netSubObj.subIndex[0];

                                    EDSsharp.AccessType subAccessType;
                                    if (netSubObj.accessTypeSpecified)
                                    {
                                        try { subAccessType = (EDSsharp.AccessType)System.Enum.Parse(typeof(EDSsharp.AccessType), netSubObj.accessType.ToString()); }
                                        catch (Exception) { subAccessType = EDSsharp.AccessType.ro; }
                                    }
                                    else
                                    {
                                        subAccessType = EDSsharp.AccessType.ro;
                                    }

                                    PDOMappingType subPDOtype;
                                    if (netSubObj.PDOmappingSpecified)
                                    {
                                        try { subPDOtype = (PDOMappingType)System.Enum.Parse(typeof(PDOMappingType), netSubObj.PDOmapping.ToString()); }
                                        catch (Exception) { subPDOtype = PDOMappingType.no; }
                                    }
                                    else
                                    {
                                        subPDOtype = PDOMappingType.no;
                                    }

                                    if (subAccessType == EDSsharp.AccessType.rw)
                                    {
                                        if (subPDOtype == PDOMappingType.RPDO)
                                            subAccessType = EDSsharp.AccessType.rww;
                                        else if (subPDOtype == PDOMappingType.TPDO)
                                            subAccessType = EDSsharp.AccessType.rwr;
                                    }

                                    ODentry subod = new ODentry
                                    {
                                        parent = od,
                                        parameter_name = netSubObj.name ?? "",
                                        objecttype = (ObjectType)netSubObj.objectType,
                                        PDOtype = subPDOtype,
                                        // following values are optional and may be overriden by parameters from body_device
                                        accesstype = subAccessType,
                                        datatype = netSubObj.dataType != null && netSubObj.dataType.Length == 1 ? (DataType)netSubObj.dataType[0] : DataType.UNKNOWN,
                                        defaultvalue = netSubObj.defaultValue ?? "",
                                        actualvalue = netSubObj.actualValue ?? "",
                                        denotation = netSubObj.denotation ?? "",
                                        LowLimit = netSubObj.lowLimit ?? "",
                                        HighLimit = netSubObj.highLimit ?? "",
                                        ObjFlags = netSubObj.objFlags != null && netSubObj.objFlags.Length == 2 ? netSubObj.objFlags[1] : (byte)0,
                                        uniqueID = netSubObj.uniqueIDRef ?? ""
                                    };

                                    if (netSubObj.uniqueIDRef != null && netSubObj.uniqueIDRef != "" && parameters.ContainsKey(netSubObj.uniqueIDRef))
                                    {
                                        parameter devSubPar = parameters[netSubObj.uniqueIDRef];

                                        subod.Description = G_label_getDescription(devSubPar.Items);

                                        subod.accesstype = ConvertAccessType(devSubPar.access);

                                        if (devSubPar.defaultValue != null && devSubPar.defaultValue.value != null)
                                            subod.defaultvalue = devSubPar.defaultValue.value;

                                        if (devSubPar.Items1 != null && devSubPar.Items1ElementName != null)
                                            subod.datatype = ConvertDataType(devSubPar.Items1ElementName[0], subod.defaultvalue);

                                        if (devSubPar.actualValue != null && devSubPar.actualValue.value != null)
                                            subod.actualvalue = devSubPar.actualValue.value;

                                        if (devSubPar.denotation != null)
                                            subod.denotation = G_label_getDescription(devSubPar.denotation.Items);

                                        if (devSubPar.allowedValues != null && devSubPar.allowedValues.range != null && devSubPar.allowedValues.range[0] != null)
                                        {
                                            range r = devSubPar.allowedValues.range[0];
                                            if (r.minValue != null) subod.LowLimit = r.minValue.value ?? "";
                                            if (r.maxValue != null) subod.HighLimit = r.maxValue.value ?? "";
                                        }

                                        subod.prop.OdeXdd(devSubPar.property);
                                    }

                                    if (od.objecttype == ObjectType.ARRAY)
                                    {
                                        od.datatype = subod.datatype;
                                        od.accesstype = subod.accesstype;
                                        od.PDOtype = subod.PDOtype;
                                        od.prop.CO_accessSRDO = subod.prop.CO_accessSRDO;
                                    }

                                    if (!od.subobjects.ContainsKey(subIndex))
                                        od.subobjects.Add(subIndex, subod);
                                }
                            }
                            if (!eds.ods.ContainsKey(index))
                                eds.ods.Add(index, od);
                        }
                    }
                }
            }

            // Remove OD from the container and store container into eds to preserve unhandled objects
            if (body_device != null && body_device.ApplicationProcess != null && body_device.ApplicationProcess[0] != null)
            {
                body_device.ApplicationProcess[0].dataTypeList = null;
                body_device.ApplicationProcess[0].parameterList = null;
            }
            if (ApplicationLayers != null)
                ApplicationLayers.CANopenObjectList = null;
            eds.xddTemplate = container;

            return eds;
        }

        private CanOpenDevice ConvertToProtobuf(ISO15745ProfileContainer container)
        {
            CanOpenDevice dev = new CanOpenDevice();

            dev.FileInfo = new CanOpen_FileInfo();
            dev.DeviceInfo = new CanOpen_DeviceInfo();
            dev.DeviceCommissioning = new CanOpen_DeviceCommissioning();

            ProfileBody_Device_CANopen body_device = null;
            ProfileBody_CommunicationNetwork_CANopen body_network = null;
            ProfileBody_CommunicationNetwork_CANopenApplicationLayers ApplicationLayers = null;
            var parameters = new Dictionary<string, parameter>();

            foreach (ISO15745Profile item in container.ISO15745Profile)
            {
                if (item.ProfileBody.GetType() == typeof(ProfileBody_Device_CANopen))
                    body_device = (ProfileBody_Device_CANopen)item.ProfileBody;
                else if (item.ProfileBody.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopen))
                    body_network = (ProfileBody_CommunicationNetwork_CANopen)item.ProfileBody;
            }

            if (body_device != null)
            {
                //eds.fi.FileName = body_device.fileName ?? "";
                dev.FileInfo.FileVersion = body_device.fileVersion ?? "";
                dev.FileInfo.CreatedBy = body_device.fileCreator ?? "";
                dev.FileInfo.ModifiedBy = body_device.fileModifiedBy ?? "";

                if (body_device.fileCreationTimeSpecified)
                {
                    dev.FileInfo.CreationTime = Timestamp.FromDateTime(body_device.fileCreationDate.Add(body_device.fileCreationTime.TimeOfDay).ToUniversalTime());

                }
                if (body_device.fileModificationDateSpecified)
                {
                    dev.FileInfo.ModificationTime = Timestamp.FromDateTime(body_device.fileModificationDate.Add(body_device.fileModificationTime.TimeOfDay).ToUniversalTime());
                }

                if (body_device.DeviceIdentity != null)
                {
                    if (body_device.DeviceIdentity.productText != null)
                        dev.FileInfo.Description = G_label_getDescription(body_device.DeviceIdentity.productText.Items);
                    if (body_device.DeviceIdentity.vendorName != null)
                        dev.DeviceInfo.VendorName = body_device.DeviceIdentity.vendorName.Value ?? "";
                    if (body_device.DeviceIdentity.productName != null)
                        dev.DeviceInfo.ProductName = body_device.DeviceIdentity.productName.Value ?? "";
                }

                if (body_device.ApplicationProcess != null
                    && body_device.ApplicationProcess[0] != null
                    && body_device.ApplicationProcess[0].parameterList != null)
                {
                    foreach (parameter param in body_device.ApplicationProcess[0].parameterList)
                    {
                        if (!parameters.ContainsKey(param.uniqueID))
                            parameters.Add(param.uniqueID, param);
                    }
                }
            }

            if (body_network != null)
            {
                ProfileBody_CommunicationNetwork_CANopenTransportLayers TransportLayers = null;
                ProfileBody_CommunicationNetwork_CANopenNetworkManagement NetworkManagement = null;

                foreach (object item in body_network.Items)
                {
                    if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        ApplicationLayers = (ProfileBody_CommunicationNetwork_CANopenApplicationLayers)item;
                    else if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenTransportLayers))
                        TransportLayers = (ProfileBody_CommunicationNetwork_CANopenTransportLayers)item;
                    else if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenNetworkManagement))
                        NetworkManagement = (ProfileBody_CommunicationNetwork_CANopenNetworkManagement)item;
                }

                if (TransportLayers != null && TransportLayers.PhysicalLayer != null
                    && TransportLayers.PhysicalLayer.baudRate != null && TransportLayers.PhysicalLayer.baudRate.supportedBaudRate != null)
                {
                    foreach (ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate baud in TransportLayers.PhysicalLayer.baudRate.supportedBaudRate)
                    {
                        switch (baud.value.ToString())
                        {
                            case "Item10Kbps": dev.DeviceInfo.BaudRate10 = true; break;
                            case "Item20Kbps": dev.DeviceInfo.BaudRate20 = true; break;
                            case "Item50Kbps": dev.DeviceInfo.BaudRate50 = true; break;
                            case "Item125Kbps": dev.DeviceInfo.BaudRate125 = true; break;
                            case "Item250Kbps": dev.DeviceInfo.BaudRate250 = true; break;
                            case "Item500Kbps": dev.DeviceInfo.BaudRate500 = true; break;
                            case "Item800Kbps": dev.DeviceInfo.BaudRate800 = true; break;
                            case "Item1000Kbps": dev.DeviceInfo.BaudRate1000 = true; break;
                            case "autobaudRate": dev.DeviceInfo.BaudRateAuto = true; break;
                        }
                    }
                }

                if (NetworkManagement != null)
                {
                    if (NetworkManagement.CANopenGeneralFeatures != null)
                    {
                        dev.DeviceInfo.LssSlave = NetworkManagement.CANopenGeneralFeatures.layerSettingServiceSlave;
                    }

                    if (NetworkManagement.CANopenMasterFeatures != null)
                    {
                        dev.DeviceInfo.LssMaster = NetworkManagement.CANopenMasterFeatures.layerSettingServiceMaster;
                    }

                    if (NetworkManagement.deviceCommissioning != null)
                    {
                        dev.DeviceCommissioning.NodeId = NetworkManagement.deviceCommissioning.NodeID;
                        dev.DeviceCommissioning.NodeName = NetworkManagement.deviceCommissioning.nodeName;
                        try { dev.DeviceCommissioning.Baudrate = System.Convert.ToUInt32(NetworkManagement.deviceCommissioning.actualBaudRate); }
                        catch (Exception) { dev.DeviceCommissioning.Baudrate = 0; }
                    }
                }

                if (ApplicationLayers != null)
                {
                    if (ApplicationLayers.CANopenObjectList != null && ApplicationLayers.CANopenObjectList.CANopenObject != null)
                    {
                        foreach (CANopenObjectListCANopenObject netObj in ApplicationLayers.CANopenObjectList.CANopenObject)
                        {
                            if (netObj.index == null || netObj.index.Length != 2)
                                continue;

                            string index = netObj.index[0].ToString("X2") + netObj.index[1].ToString("X2");

                            // some properties for object, set to default, changed by netObj
                            LibCanOpen.OdObject.Types.ObjectType objectType = LibCanOpen.OdObject.Types.ObjectType.Unspecified;

                            // some properties for sub objects, set to default, changed by netObj or netSubObj
                            LibCanOpen.OdSubObject.Types.DataType dataType = LibCanOpen.OdSubObject.Types.DataType.Unspecified;
                            LibCanOpen.OdSubObject.Types.AccessSDO accessSDO = LibCanOpen.OdSubObject.Types.AccessSDO.No;
                            LibCanOpen.OdSubObject.Types.AccessPDO accessPDO = LibCanOpen.OdSubObject.Types.AccessPDO.No;

                            if (System.Enum.IsDefined(typeof(LibCanOpen.OdObject.Types.ObjectType), (Int32)netObj.objectType))
                            {
                                objectType = (LibCanOpen.OdObject.Types.ObjectType)netObj.objectType;
                            }

                            if (netObj.dataType != null && netObj.dataType.Length == 1)
                            {
                                if (System.Enum.IsDefined(typeof(LibCanOpen.OdSubObject.Types.DataType), netObj.dataType[0]))
                                {
                                    dataType = (LibCanOpen.OdSubObject.Types.DataType)netObj.dataType[0];
                                }
                            }

                            // accessType in xdd may be specified by netObj.accessType or inside netObj.uniqueIDRef ??
                            if (netObj.accessTypeSpecified)
                            {
                                switch (netObj.accessType)
                                {
                                    case CANopenObjectListCANopenObjectAccessType.@const:
                                    case CANopenObjectListCANopenObjectAccessType.ro: accessSDO = LibCanOpen.OdSubObject.Types.AccessSDO.Ro; break;
                                    case CANopenObjectListCANopenObjectAccessType.wo: accessSDO = LibCanOpen.OdSubObject.Types.AccessSDO.Wo; break;
                                    case CANopenObjectListCANopenObjectAccessType.rw: accessSDO = LibCanOpen.OdSubObject.Types.AccessSDO.Rw; break;
                                }
                            }

                            if (netObj.PDOmappingSpecified)
                            {
                                switch (netObj.PDOmapping)
                                {
                                    case CANopenObjectListCANopenObjectPDOmapping.RPDO: accessPDO = LibCanOpen.OdSubObject.Types.AccessPDO.R; break;
                                    case CANopenObjectListCANopenObjectPDOmapping.TPDO: accessPDO = LibCanOpen.OdSubObject.Types.AccessPDO.T; break;
                                    case CANopenObjectListCANopenObjectPDOmapping.optional: accessPDO = LibCanOpen.OdSubObject.Types.AccessPDO.Tr; break;
                                }
                            }

                            OdObject entry = new OdObject()
                            {
                                Name = netObj.name ?? "",
                                Alias = netObj.denotation ?? "",
                                ObjectType = objectType
                            };

                            OdSubObject subEntry0 = new OdSubObject()
                            {
                                DataType = dataType,
                                Sdo = accessSDO,
                                Pdo = accessPDO,
                                DefaultValue = netObj.defaultValue ?? "",
                                ActualValue = netObj.actualValue ?? "",
                                LowLimit = netObj.lowLimit ?? "",
                                HighLimit = netObj.highLimit ?? ""
                            };

                            if (netObj.uniqueIDRef != null && netObj.uniqueIDRef != "" && parameters.ContainsKey(netObj.uniqueIDRef))
                            {
                                parameter devPar = parameters[netObj.uniqueIDRef];

                                entry.Description = G_label_getDescription(devPar.Items);

                                ConvertAccessType(devPar.access, subEntry0);

                                if (devPar.defaultValue != null && devPar.defaultValue.value != null)
                                    subEntry0.DefaultValue = devPar.defaultValue.value;

                                if (devPar.Items1 != null && devPar.Items1ElementName != null)
                                    ConvertDataType(devPar.Items1ElementName[0], subEntry0);

                                if (devPar.actualValue != null && devPar.actualValue.value != null)
                                    subEntry0.ActualValue = devPar.actualValue.value;

                                if (devPar.denotation != null)
                                    subEntry0.Alias = G_label_getDescription(devPar.denotation.Items);

                                if (devPar.allowedValues != null && devPar.allowedValues.range != null && devPar.allowedValues.range[0] != null)
                                {
                                    range r = devPar.allowedValues.range[0];
                                    if (r.minValue != null) subEntry0.LowLimit = r.minValue.value ?? "";
                                    if (r.maxValue != null) subEntry0.HighLimit = r.maxValue.value ?? "";
                                }

                                ConvertXddProperties(devPar.property, entry, subEntry0);
                            }

                            if (netObj.CANopenSubObject == null)
                            {
                                entry.SubObjects.Add("00", subEntry0);
                            }
                            else
                            {
                                foreach (CANopenObjectListCANopenObjectCANopenSubObject netSubObj in netObj.CANopenSubObject)
                                {
                                    if (netSubObj.subIndex == null || netSubObj.subIndex.Length != 1)
                                        continue;

                                    string subIndex = netSubObj.subIndex[0].ToString("X2");
                                    LibCanOpen.OdSubObject.Types.DataType subDataType = LibCanOpen.OdSubObject.Types.DataType.Unspecified;
                                    LibCanOpen.OdSubObject.Types.AccessSDO subAccessSDO = LibCanOpen.OdSubObject.Types.AccessSDO.No;
                                    LibCanOpen.OdSubObject.Types.AccessPDO subAccessPDO = LibCanOpen.OdSubObject.Types.AccessPDO.No;

                                    if (netSubObj.dataType != null && netSubObj.dataType.Length == 1)
                                    {
                                        if (System.Enum.IsDefined(typeof(LibCanOpen.OdSubObject.Types.DataType), netSubObj.dataType[0]))
                                        {
                                            subDataType = (LibCanOpen.OdSubObject.Types.DataType)netSubObj.dataType[0];
                                        }
                                    }

                                    if (netSubObj.accessTypeSpecified)
                                    {
                                        switch (netSubObj.accessType)
                                        {
                                            case CANopenObjectListCANopenObjectCANopenSubObjectAccessType.@const:
                                            case CANopenObjectListCANopenObjectCANopenSubObjectAccessType.ro: subAccessSDO = LibCanOpen.OdSubObject.Types.AccessSDO.Ro; break;
                                            case CANopenObjectListCANopenObjectCANopenSubObjectAccessType.wo: subAccessSDO = LibCanOpen.OdSubObject.Types.AccessSDO.Wo; break;
                                            case CANopenObjectListCANopenObjectCANopenSubObjectAccessType.rw: subAccessSDO = LibCanOpen.OdSubObject.Types.AccessSDO.Rw; break;
                                        }
                                    }

                                    if (netSubObj.PDOmappingSpecified)
                                    {
                                        switch (netSubObj.PDOmapping)
                                        {
                                            case CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping.RPDO: subAccessPDO = LibCanOpen.OdSubObject.Types.AccessPDO.R; break;
                                            case CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping.TPDO: subAccessPDO = LibCanOpen.OdSubObject.Types.AccessPDO.T; break;
                                            case CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping.optional: subAccessPDO = LibCanOpen.OdSubObject.Types.AccessPDO.Tr; break;
                                        }
                                    }

                                    OdSubObject subEntry = new OdSubObject()
                                    {
                                        Name = netSubObj.name ?? "",
                                        Alias = netSubObj.denotation ?? "",
                                        DataType = subDataType,
                                        Sdo = subAccessSDO,
                                        Pdo = subAccessPDO,
                                        DefaultValue = netSubObj.defaultValue ?? "",
                                        ActualValue = netSubObj.actualValue ?? "",
                                        LowLimit = netSubObj.lowLimit ?? "",
                                        HighLimit = netSubObj.highLimit ?? ""
                                    };

                                    if (netSubObj.uniqueIDRef != null && netSubObj.uniqueIDRef != "" && parameters.ContainsKey(netSubObj.uniqueIDRef))
                                    {
                                        parameter devSubPar = parameters[netSubObj.uniqueIDRef];

                                        entry.Description += G_label_getDescription(devSubPar.Items);

                                        ConvertAccessType(devSubPar.access, subEntry);

                                        if (devSubPar.defaultValue != null && devSubPar.defaultValue.value != null)
                                            subEntry.DefaultValue = devSubPar.defaultValue.value;

                                        if (devSubPar.Items1 != null && devSubPar.Items1ElementName != null)
                                            ConvertDataType(devSubPar.Items1ElementName[0], subEntry);

                                        if (devSubPar.actualValue != null && devSubPar.actualValue.value != null)
                                            subEntry.ActualValue = devSubPar.actualValue.value;

                                        if (devSubPar.denotation != null)
                                            subEntry.Alias = G_label_getDescription(devSubPar.denotation.Items);

                                        if (devSubPar.allowedValues != null && devSubPar.allowedValues.range != null && devSubPar.allowedValues.range[0] != null)
                                        {
                                            range r = devSubPar.allowedValues.range[0];
                                            if (r.minValue != null) subEntry.LowLimit = r.minValue.value ?? "";
                                            if (r.maxValue != null) subEntry.HighLimit = r.maxValue.value ?? "";
                                        }

                                        ConvertXddProperties(devSubPar.property, entry, subEntry);
                                    }

                                    if (!entry.SubObjects.ContainsKey(subIndex))
                                        entry.SubObjects.Add(subIndex, subEntry);
                                }
                            }

                            if (!dev.Objects.ContainsKey(index))
                                dev.Objects.Add(index, entry);
                        }
                    }
                }
            }

            return dev;
        }

        private ISO15745ProfileContainer ConvertFromProtobuf(CanOpenDevice dev, string fileName, bool deviceCommissioning, bool stripped)
        {
            #region base_elements
            ISO15745ProfileContainer container = new ISO15745ProfileContainer();

            container.ISO15745Profile = new ISO15745Profile[]
            {
                new ISO15745Profile(),
                new ISO15745Profile(),
            };

            container.ISO15745Profile[0].ProfileHeader = new ProfileHeader_DataType
            {
                ProfileIdentification = "CANopen device profile",
                ProfileRevision = "1.1",
                ProfileName = "",
                ProfileSource = "",
                ProfileClassID = ProfileClassID_DataType.Device,
                ISO15745Reference = new ISO15745Reference_DataType
                {
                    ISO15745Part = "1",
                    ISO15745Edition = "1",
                    ProfileTechnology = "CANopen"
                }
            };
            container.ISO15745Profile[0].ProfileBody = new ProfileBody_Device_CANopen();
            var body_device = (ProfileBody_Device_CANopen)container.ISO15745Profile[0].ProfileBody;

            container.ISO15745Profile[1].ProfileHeader = new ProfileHeader_DataType
            {
                ProfileIdentification = "CANopen communication network profile",
                ProfileRevision = "1.1",
                ProfileName = "",
                ProfileSource = "",
                ProfileClassID = ProfileClassID_DataType.CommunicationNetwork,
                ISO15745Reference = new ISO15745Reference_DataType
                {
                    ISO15745Part = "1",
                    ISO15745Edition = "1",
                    ProfileTechnology = "CANopen"
                }
            };
            container.ISO15745Profile[1].ProfileBody = new ProfileBody_CommunicationNetwork_CANopen();
            var body_network = (ProfileBody_CommunicationNetwork_CANopen)container.ISO15745Profile[1].ProfileBody;

            #endregion

            #region ObjectDictionary
            var body_network_objectList = new List<CANopenObjectListCANopenObject>();
            var body_device_parameterList = new List<parameter>();
            var body_device_arrayList = new List<array>();
            var body_device_structList = new List<@struct>();

            foreach (var kvp in dev.Objects)
            {
                string index = kvp.Key;
                Int16 indexInt;
                OdObject entry = kvp.Value;


                try
                {
                    indexInt = System.Convert.ToInt16(index, 16);
                }
                catch (Exception)
                {
                    Warnings.AddWarning($"Error in Object ({index}), wrong index", Warnings.warning_class.WARNING_GENERIC);
                    continue;
                }

                if (stripped && entry.Disabled)
                    continue;

                var netObj = new CANopenObjectListCANopenObject
                {
                    index = new byte[] { (byte)(indexInt >> 8), (byte)(indexInt & 0xFF) },
                    name = entry.Name,
                    objectType = (byte)entry.ObjectType,
                    uniqueIDRef = "UID_OBJ_" + index
                };
                body_network_objectList.Add(netObj);

                var devPar = new parameter { uniqueID = "UID_OBJ_" + index };
                if (entry.Description != null && entry.Description != "")
                {
                    devPar.Items = new object[] { new vendorTextDescription { lang = "en", Value = entry.Description } };
                }
                else
                {
                    // Add at least label made from parameter name, because g_labels is required by schema
                    devPar.Items = new object[] { new vendorTextLabel { lang = "en", Value = entry.Name } };
                }
                if (deviceCommissioning && entry.Alias != null && entry.Alias != "")
                {
                    devPar.denotation = new denotation
                    {
                        Items = new object[] { new vendorTextLabel { lang = "en", Value = entry.Alias } }
                    };
                }
                body_device_parameterList.Add(devPar);

                if (entry.ObjectType == LibCanOpen.OdObject.Types.ObjectType.Var)
                {
                    if (entry.SubObjects.Count != 1)
                    {
                        Warnings.AddWarning($"Error in Object ({index}), VAR must have one subobject", Warnings.warning_class.WARNING_GENERIC);
                        continue;
                    }
                    var subEntry0 = entry.SubObjects[entry.SubObjects.Keys.First()];
                    switch (subEntry0.Pdo)
                    {
                        case LibCanOpen.OdSubObject.Types.AccessPDO.R: netObj.PDOmapping = CANopenObjectListCANopenObjectPDOmapping.RPDO; break;
                        case LibCanOpen.OdSubObject.Types.AccessPDO.T: netObj.PDOmapping = CANopenObjectListCANopenObjectPDOmapping.TPDO; break;
                        case LibCanOpen.OdSubObject.Types.AccessPDO.Tr: netObj.PDOmapping = CANopenObjectListCANopenObjectPDOmapping.optional; break;
                        default: netObj.PDOmapping = CANopenObjectListCANopenObjectPDOmapping.no; break;
                    }

                    netObj.PDOmappingSpecified = true;

                    if (!stripped)
                    {
                        var propOd = ConvertXddProperties(entry);
                        var propSub = ConvertXddProperties(subEntry0);
                        devPar.property = new property[propOd.Length + propSub.Length];
                        propOd.CopyTo(devPar.property, 0);
                        propSub.CopyTo(devPar.property, propOd.Length);
                    }

                    WriteVar(devPar, subEntry0);
                    if (deviceCommissioning && subEntry0.ActualValue != null && subEntry0.ActualValue != "")
                        devPar.actualValue = new actualValue { value = subEntry0.ActualValue };
                }
                else if ((entry.ObjectType == LibCanOpen.OdObject.Types.ObjectType.Array || entry.ObjectType == LibCanOpen.OdObject.Types.ObjectType.Record) && entry.SubObjects != null && entry.SubObjects.Count > 0)
                {
                    netObj.subNumber = (byte)entry.SubObjects.Count;
                    netObj.subNumberSpecified = true;

                    if (!stripped)
                        devPar.property = ConvertXddProperties(entry);

                    var netSubObjList = new List<CANopenObjectListCANopenObjectCANopenSubObject>();
                    var devStructSubList = new List<varDeclaration>();
                    ItemChoiceType ArrayItemElementName = ItemChoiceType.SINT;

                    foreach (var subKvp in entry.SubObjects)
                    {
                        string subIndex = subKvp.Key;
                        byte subIndexInt;
                        OdSubObject subEntry = subKvp.Value;

                        try
                        {
                            subIndexInt = System.Convert.ToByte(subIndex, 16);
                        }
                        catch (Exception)
                        {
                            Warnings.AddWarning($"Error in Object ({index}), wrong SubIndex", Warnings.warning_class.WARNING_GENERIC);
                            continue;
                        }

                        var netSubObj = new CANopenObjectListCANopenObjectCANopenSubObject
                        {
                            subIndex = new byte[] { subIndexInt },
                            name = subEntry.Name,
                            objectType = (byte)LibCanOpen.OdObject.Types.ObjectType.Var,
                            PDOmappingSpecified = true,
                            uniqueIDRef = "UID_SUB_" + index + subIndex
                        };
                        switch (subEntry.Pdo)
                        {
                            case LibCanOpen.OdSubObject.Types.AccessPDO.R: netSubObj.PDOmapping = CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping.RPDO; break;
                            case LibCanOpen.OdSubObject.Types.AccessPDO.T: netSubObj.PDOmapping = CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping.TPDO; break;
                            case LibCanOpen.OdSubObject.Types.AccessPDO.Tr: netSubObj.PDOmapping = CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping.optional; break;
                            default: netSubObj.PDOmapping = CANopenObjectListCANopenObjectCANopenSubObjectPDOmapping.no; break;
                        }

                        var devSubPar = new parameter
                        {
                            uniqueID = "UID_SUB_" + index + subIndex
                        };
                        // Add at least label made from parameter name, because g_labels is required by schema
                        devSubPar.Items = new object[] { new vendorTextLabel { lang = "en", Value = subEntry.Name } };

                        if (!stripped)
                            devSubPar.property = ConvertXddProperties(subEntry);

                        if (deviceCommissioning && subEntry.Alias != null && subEntry.Alias != "")
                        {
                            devPar.denotation = new denotation
                            {
                                Items = new object[] { new vendorTextLabel { lang = "en", Value = subEntry.Alias } }
                            };
                        }
                        WriteVar(devSubPar, subEntry);

                        if (deviceCommissioning && subEntry.ActualValue != null && subEntry.ActualValue != "")
                            devPar.actualValue = new actualValue { value = subEntry.ActualValue };

                        if (entry.ObjectType == LibCanOpen.OdObject.Types.ObjectType.Record)
                        {
                            devStructSubList.Add(new varDeclaration
                            {
                                name = subEntry.Name,
                                uniqueID = "UID_RECSUB_" + index + subIndex,
                                Item = new object(),
                                ItemElementName = (ItemChoiceType1)ConvertDataType(subEntry)
                            });
                        }
                        else
                        {
                            ArrayItemElementName = (ItemChoiceType)ConvertDataType(subEntry);
                        }

                        body_device_parameterList.Add(devSubPar);
                        netSubObjList.Add(netSubObj);
                    }

                    // add refference to data type definition and definition of array or struct data type (schema requirement)
                    if (entry.ObjectType == LibCanOpen.OdObject.Types.ObjectType.Array)
                    {
                        devPar.Items1 = new object[] { new dataTypeIDRef { uniqueIDRef = "UID_ARR_" + index } };
                        devPar.Items1ElementName = new Items1ChoiceType[] { Items1ChoiceType.dataTypeIDRef };
                        body_device_arrayList.Add(new array
                        {
                            uniqueID = "UID_ARR_" + index,
                            name = entry.Name,
                            Item = new object(),
                            ItemElementName = ArrayItemElementName,
                            subrange = new subrange[] { new subrange { lowerLimit = 0, upperLimit = entry.SubObjects.Count - 1 } }
                        });
                    }
                    else
                    {
                        devPar.Items1 = new object[] { new dataTypeIDRef { uniqueIDRef = "UID_REC_" + index } };
                        devPar.Items1ElementName = new Items1ChoiceType[] { Items1ChoiceType.dataTypeIDRef };
                        body_device_structList.Add(new @struct
                        {
                            uniqueID = "UID_REC_" + index,
                            name = entry.Name,
                            varDeclaration = devStructSubList.ToArray()
                        });
                    }

                    netObj.CANopenSubObject = netSubObjList.ToArray();
                }
            }
            #endregion

            #region body_device
            body_device.fileName = fileName;
            body_device.fileCreator = dev.FileInfo.CreatedBy;
            body_device.fileCreationDate = dev.FileInfo.CreationTime.ToDateTime();
            body_device.fileCreationTime = dev.FileInfo.CreationTime.ToDateTime();
            body_device.fileCreationTimeSpecified = true;
            body_device.fileVersion = dev.FileInfo.FileVersion;
            body_device.fileModifiedBy = dev.FileInfo.ModifiedBy;
            body_device.fileModificationDate = dev.FileInfo.ModificationTime.ToDateTime();
            body_device.fileModificationTime = dev.FileInfo.ModificationTime.ToDateTime();
            body_device.fileModificationDateSpecified = true;
            body_device.fileModificationTimeSpecified = true;
            body_device.supportedLanguages = "en";

            // Device identity
            if (body_device.DeviceIdentity == null)
                body_device.DeviceIdentity = new DeviceIdentity();
            body_device.DeviceIdentity.vendorName = new vendorName { Value = dev.DeviceInfo.VendorName };
            body_device.DeviceIdentity.vendorID = new vendorID { Value = "0" };
            body_device.DeviceIdentity.productName = new productName { Value = dev.DeviceInfo.ProductName };
            body_device.DeviceIdentity.productID = new productID { Value = "0" };
            if (dev.FileInfo.Description != null && dev.FileInfo.Description != "")
            {
                body_device.DeviceIdentity.productText = new productText
                {
                    Items = new object[]
                    {
                        new vendorTextDescription { lang = "en", Value = dev.FileInfo.Description }
                    }
                };
            }

            // version is optional element, make a template if empty
            if (body_device.DeviceIdentity.version == null)
            {
                body_device.DeviceIdentity.version = new version[]
                {
                    new version { versionType = versionVersionType.SW, Value = "0" },
                    new version { versionType = versionVersionType.FW, Value = "0" },
                    new version { versionType = versionVersionType.HW, Value = "0" }
                };
            }

            // DeviceFunction is required by schema, make a template if empty.
            if (body_device.DeviceFunction == null)
            {
                // This is just a template for somehow complex xml structure. Users can edit the
                // xdd file directly to write characteristics of own devices or use generic xml
                // editing tool. External editing will be preserved anyway, if xdd file will be
                // later opened and saved back in EDSEditor.
                // EDSEditor curerently does not have interface for editing the characteristics.
                body_device.DeviceFunction = new DeviceFunction[]
                {
                    new DeviceFunction
                    {
                        capabilities = new capabilities
                        {
                            characteristicsList = new characteristicsList[]
                            {
                                new characteristicsList
                                {
                                    characteristic = new characteristic[]
                                    {
                                        new characteristic
                                        {
                                            characteristicName = new characteristicName
                                            {
                                                Items = new object[]
                                                {
                                                    new vendorTextLabel { lang = "en", Value = "SW library" }
                                                }
                                            },
                                            characteristicContent = new characteristicContent[]
                                            {
                                                new characteristicContent {
                                                    Items = new object[]
                                                    {
                                                        new vendorTextLabel { lang = "en", Value = "libedssharp" }
                                                    }
                                                },
                                                new characteristicContent {
                                                    Items = new object[]
                                                    {
                                                        new vendorTextLabel { lang = "en", Value = "CANopenNode" }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }

            // ApplicationProcess (insert object dictionary)
            if (body_device.ApplicationProcess == null)
                body_device.ApplicationProcess = new ApplicationProcess[1];
            if (body_device.ApplicationProcess[0] == null)
                body_device.ApplicationProcess[0] = new ApplicationProcess();
            body_device.ApplicationProcess[0].dataTypeList = new dataTypeList
            {
                array = body_device_arrayList.ToArray(),
                @struct = body_device_structList.ToArray()
            };
            body_device.ApplicationProcess[0].parameterList = body_device_parameterList.ToArray();
            #endregion

            #region body_network
            body_network.fileName = fileName;
            body_network.fileCreator = dev.FileInfo.CreatedBy;
            body_network.fileCreationDate = dev.FileInfo.CreationTime.ToDateTime();
            body_network.fileCreationTime = dev.FileInfo.CreationTime.ToDateTime();
            body_network.fileCreationTimeSpecified = true;
            body_network.fileVersion = dev.FileInfo.FileVersion;
            body_network.fileModificationDate = dev.FileInfo.ModificationTime.ToDateTime();
            body_network.fileModificationTime = dev.FileInfo.ModificationTime.ToDateTime();
            body_network.fileModificationDateSpecified = true;
            body_network.fileModificationTimeSpecified = true;
            body_network.supportedLanguages = "en";

            // base elements
            ProfileBody_CommunicationNetwork_CANopenApplicationLayers ApplicationLayers = null;
            ProfileBody_CommunicationNetwork_CANopenTransportLayers TransportLayers = null;
            ProfileBody_CommunicationNetwork_CANopenNetworkManagement NetworkManagement = null;
            if (body_network.Items != null && body_network.Items.Length >= 3)
            {
                foreach (object item in body_network.Items)
                {
                    if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        ApplicationLayers = (ProfileBody_CommunicationNetwork_CANopenApplicationLayers)item;
                    else if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        TransportLayers = (ProfileBody_CommunicationNetwork_CANopenTransportLayers)item;
                    else if (item.GetType() == typeof(ProfileBody_CommunicationNetwork_CANopenApplicationLayers))
                        NetworkManagement = (ProfileBody_CommunicationNetwork_CANopenNetworkManagement)item;
                }
            }
            if (ApplicationLayers == null || TransportLayers == null || NetworkManagement == null)
            {
                body_network.Items = new object[3];
                body_network.Items[0] = new ProfileBody_CommunicationNetwork_CANopenApplicationLayers();
                ApplicationLayers = (ProfileBody_CommunicationNetwork_CANopenApplicationLayers)body_network.Items[0];
                body_network.Items[1] = new ProfileBody_CommunicationNetwork_CANopenTransportLayers();
                TransportLayers = (ProfileBody_CommunicationNetwork_CANopenTransportLayers)body_network.Items[1];
                body_network.Items[2] = new ProfileBody_CommunicationNetwork_CANopenNetworkManagement();
                NetworkManagement = (ProfileBody_CommunicationNetwork_CANopenNetworkManagement)body_network.Items[2];
            }

            // ApplicationLayers -> dummyUsage
            ApplicationLayers.dummyUsage = new ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummy[7];
            for (int x = 0; x < 7; x++)
                ApplicationLayers.dummyUsage[x] = new ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummy();

            ApplicationLayers.dummyUsage[0].entry = ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00010;
            ApplicationLayers.dummyUsage[1].entry = ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00021;
            ApplicationLayers.dummyUsage[2].entry = ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00031;
            ApplicationLayers.dummyUsage[3].entry = ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00041;
            ApplicationLayers.dummyUsage[4].entry = ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00051;
            ApplicationLayers.dummyUsage[5].entry = ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00061;
            ApplicationLayers.dummyUsage[6].entry = ProfileBody_CommunicationNetwork_CANopenApplicationLayersDummyEntry.Dummy00071;

            // ApplicationLayers -> CANopenObjectList (insert object dictionary)
            ApplicationLayers.CANopenObjectList = new CANopenObjectList
            {
                CANopenObject = body_network_objectList.ToArray()
            };

            // TransportLayers -> supportedBaudRate
            List<ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue> bauds;
            bauds = new List<ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue>();
            if (dev.DeviceInfo.BaudRate10)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item10Kbps);
            if (dev.DeviceInfo.BaudRate20)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item20Kbps);
            if (dev.DeviceInfo.BaudRate50)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item50Kbps);
            if (dev.DeviceInfo.BaudRate125)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item125Kbps);
            if (dev.DeviceInfo.BaudRate250)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item250Kbps);
            if (dev.DeviceInfo.BaudRate500)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item500Kbps);
            if (dev.DeviceInfo.BaudRate800)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item800Kbps);
            if (dev.DeviceInfo.BaudRate1000)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.Item1000Kbps);
            if (dev.DeviceInfo.BaudRateAuto)
                bauds.Add(ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRateValue.autobaudRate);
            TransportLayers.PhysicalLayer = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayer
            {
                baudRate = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRate
                {
                    supportedBaudRate = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate[bauds.Count]
                }
            };
            for (int i = 0; i < bauds.Count; i++)
            {
                TransportLayers.PhysicalLayer.baudRate.supportedBaudRate[i] = new ProfileBody_CommunicationNetwork_CANopenTransportLayersPhysicalLayerBaudRateSupportedBaudRate
                {
                    value = bauds[i]
                };
            }

            // NetworkManagement
            if (NetworkManagement.CANopenGeneralFeatures == null)
                NetworkManagement.CANopenGeneralFeatures = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementCANopenGeneralFeatures();
            NetworkManagement.CANopenGeneralFeatures.granularity = 8; // required parameter
            NetworkManagement.CANopenGeneralFeatures.nrOfRxPDO = 4; //extract from OD
            NetworkManagement.CANopenGeneralFeatures.nrOfTxPDO = 4; //extract from OD

            NetworkManagement.CANopenGeneralFeatures.ngSlave = false;
            NetworkManagement.CANopenGeneralFeatures.ngMaster = false;
            NetworkManagement.CANopenGeneralFeatures.NrOfNG_MonitoredNodes = 0;

            NetworkManagement.CANopenGeneralFeatures.layerSettingServiceSlave = dev.DeviceInfo.LssSlave;
            NetworkManagement.CANopenGeneralFeatures.groupMessaging = false;
            NetworkManagement.CANopenGeneralFeatures.dynamicChannels = 0;

            if (NetworkManagement.CANopenMasterFeatures == null)
                NetworkManagement.CANopenMasterFeatures = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementCANopenMasterFeatures();
            NetworkManagement.CANopenMasterFeatures.layerSettingServiceMaster = dev.DeviceInfo.LssMaster;
            NetworkManagement.CANopenMasterFeatures.bootUpMaster = false;

            if (deviceCommissioning)
            {
                NetworkManagement.deviceCommissioning = new ProfileBody_CommunicationNetwork_CANopenNetworkManagementDeviceCommissioning
                {
                    NodeID = (byte)dev.DeviceCommissioning.NodeId,
                    nodeName = dev.DeviceCommissioning.NodeName,
                    actualBaudRate = dev.DeviceCommissioning.Baudrate.ToString()
                };
            }
            else
            {
                NetworkManagement.deviceCommissioning = null;
            }
            #endregion

            return container;
        }

    }
}

[XmlRoot(ElementName = "CanOpenProject_1_1")]
public class CanOpenProject_1_1
{
    [XmlElement(ElementName = "ISO15745ProfileContainer", Namespace = "http://www.canopen.org/xml/1.1")]
    public List<ISO15745ProfileContainer> ISO15745ProfileContainer { get; set; }
    /// <summary>
    /// XDD version
    /// </summary>
    [XmlAttribute(AttributeName = "version")]
    public string Version { get; set; }
}
