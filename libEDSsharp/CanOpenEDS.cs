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
 
    Copyright(c) 2016 - 2019 Robin Cornelius <robin.cornelius@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace libEDSsharp
{
    public partial class InfoSection
    {
        public virtual void Parse(Dictionary<string, string> section, string sectionname)
        {
            this.section = section;

            FieldInfo[] fields = this.GetType().GetFields();

            foreach (FieldInfo f in fields)
            {
                if (Attribute.IsDefined(f, typeof(EdsExport)))
                    GetField(f.Name, f.Name);

                if (Attribute.IsDefined(f, typeof(DcfExport)))
                    GetField(f.Name, f.Name);
            }
        }

        /// <summary>
        /// Write object to stream
        /// </summary>
        /// <param name="writer">stream to write the data to</param>
        /// <param name="ft">file type</param>
        public void Write(StreamWriter writer, Filetype ft)
        {
            writer.WriteLine("[" + edssection + "]");
            Type tx = this.GetType();
            FieldInfo[] fields = this.GetType().GetFields();

            foreach (FieldInfo f in fields)
            {
                if ((ft == Filetype.File_EDS) && (!Attribute.IsDefined(f, typeof(EdsExport))))
                    continue;

                if ((ft == Filetype.File_DCF) && (!(Attribute.IsDefined(f, typeof(DcfExport)) || Attribute.IsDefined(f, typeof(EdsExport)))))
                    continue;

                if (f.GetValue(this) == null)
                    continue;

                EdsExport ex = (EdsExport)f.GetCustomAttribute(typeof(EdsExport));

                bool comment = ex.IsReadOnly();

                if (f.FieldType.Name == "Boolean")
                {
                    writer.WriteLine(string.Format("{2}{0}={1}", f.Name, ((bool)f.GetValue(this)) == true ? 1 : 0, comment == true ? ";" : ""));
                }
                else
                {
                    writer.WriteLine(string.Format("{2}{0}={1}", f.Name, f.GetValue(this).ToString(), comment == true ? ";" : ""));
                }
            }

            writer.WriteLine("");
        }
    }

    public partial class MandatoryObjects : SupportedObjects
    {
        public MandatoryObjects(Dictionary<string, string> section)
        : this()
        {
            Parse(section);
        }
    }

    public partial class OptionalObjects : SupportedObjects
    {
        public OptionalObjects(Dictionary<string, string> section)
            : this()
        {
            Parse(section);
        }
    }

    public partial class ManufacturerObjects : SupportedObjects
    {
        public ManufacturerObjects(Dictionary<string, string> section)
            : this()
        {
            Parse(section);
        }
    }

    public partial class TypeDefinitions : SupportedObjects
    {
        public TypeDefinitions(Dictionary<string, string> section)
        {
            Parse(section);
        }
    }

    public partial class SupportedObjects
    {
        public virtual void Parse(Dictionary<string, string> section)
        {
            objectlist = new Dictionary<int, int>();
            foreach (KeyValuePair<string, string> kvp in section)
            {
                if (kvp.Key.ToLower() == "supportedobjects")
                    continue;

                if (kvp.Key.ToLower() == "nrofentries")
                    continue;

                int count = Convert.ToInt16(kvp.Key, EDSsharp.Getbase(kvp.Key));
                int target = Convert.ToInt16(kvp.Value, EDSsharp.Getbase(kvp.Value));
                objectlist.Add(count, target);
            }
        }
        /// <summary>
        /// Write object to stream
        /// </summary>
        /// <param name="writer">stream to write the data to</param>
        public void Write(StreamWriter writer)
        {
            writer.WriteLine("[" + edssection + "]");
            writer.WriteLine(string.Format("{0}={1}", countmsg, objectlist.Count));
            foreach (KeyValuePair<int, int> kvp in objectlist)
            {
                writer.WriteLine(string.Format("{0}=0x{1:X4}", kvp.Key, kvp.Value));
            }
            writer.WriteLine("");
        }
    }

    public partial class Comments
    {
        public Comments(Dictionary<string, string> section)
        {
            Parse(section);
        }

        public virtual void Parse(Dictionary<string, string> section)
        {
            comments = new List<string>();
            foreach (KeyValuePair<string, string> kvp in section)
            {
                if (kvp.Key == "Lines")
                    continue;

                comments.Add(kvp.Value);
            }
        }
        /// <summary>
        /// Write object to stream
        /// </summary>
        /// <param name="writer">stream to write the data to</param>
        public void Write(StreamWriter writer)
        {
            if (comments == null)
            {
                comments = new List<string>();
            }

            writer.WriteLine("[" + edssection + "]");

            writer.WriteLine(string.Format("Lines={0}", comments.Count));

            int count = 1;
            foreach (string s in comments)
            {
                writer.WriteLine(string.Format("Line{0}={1}", count, s));
                count++;
            }

            writer.WriteLine("");
        }
    }
    public partial class Dummyusage : InfoSection
    {
        public Dummyusage(Dictionary<string, string> section) : this()
        {
            Parse(section, edssection);
        }
    }

    /// <summary>
    /// FileInfo section as described in CiA 306
    /// </summary>
    public partial class FileInfo : InfoSection
    {
        public FileInfo(Dictionary<string, string> section) : this()
        {
            Parse(section, edssection);
        }

        override public void Parse(Dictionary<string, string> section, string sectionname)
        {
            base.Parse(section, edssection);

            string dtcombined = "";
            try
            {
                if (section.ContainsKey("CreationTime") && section.ContainsKey("CreationDate"))
                {
                    dtcombined = section["CreationTime"].Replace(" ", "") + " " + section["CreationDate"];
                    CreationDateTime = DateTime.ParseExact(dtcombined, "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception e)
            {
                if (e is System.FormatException)
                {
                    Warnings.warning_list.Add(String.Format("EDS Error: Section [{1}] Unable to parse DateTime {0} for CreationTime, not in DS306 format", dtcombined, sectionname));
                }
            }

            try
            {
                if (section.ContainsKey("ModificationTime") && section.ContainsKey("ModificationTime"))
                {
                    dtcombined = section["ModificationTime"].Replace(" ", "") + " " + section["ModificationDate"];
                    ModificationDateTime = DateTime.ParseExact(dtcombined, "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception e)
            {
                if (e is System.FormatException)
                {
                    Warnings.warning_list.Add(String.Format("EDS Error: Section [{1}] Unable to parse DateTime {0} for ModificationTime, not in DS306 format", dtcombined, sectionname));
                }
            }


            try
            {
                if (section.ContainsKey("EDSVersion"))
                {
                    string[] bits = section["EDSVersion"].Split('.');
                    if (bits.Length >= 1)
                        EDSVersionMajor = Convert.ToByte(bits[0]);
                    if (bits.Length >= 2)
                        EDSVersionMinor = Convert.ToByte(bits[1]);
                    //EDSVersion = String.Format("{0}.{1}", EDSVersionMajor, EDSVersionMinor);
                }
            }
            catch
            {
                Warnings.warning_list.Add(String.Format("Unable to parse EDS version {0}", section["EDSVersion"]));
            }
        }
    }
    public partial class DeviceInfo : InfoSection
    {
        public DeviceInfo(Dictionary<string, string> section) : this()
        {
            Parse(section, edssection);
        }
    }

    public partial class DeviceCommissioning : InfoSection
    {
        public DeviceCommissioning(Dictionary<string, string> section) : this()
        {
            Parse(section, edssection);
        }

    }
    public partial class SupportedModules : InfoSection
    {
        public SupportedModules(Dictionary<string, string> section) : this()
        {
            Parse(section, edssection);
        }
    }

    public partial class ConnectedModules : SupportedObjects
    {
        public ConnectedModules(Dictionary<string, string> section) : this()
        {
            Parse(section);

            foreach (KeyValuePair<int, int> kvp in this.objectlist)
            {
                UInt16 K = (UInt16)kvp.Value;
                UInt16 V = (UInt16)kvp.Key;

                connectedmodulelist.Add(K, V);
            }
        }
    }
    public partial class MxFixedObjects : SupportedObjects
    {
        public MxFixedObjects(Dictionary<string, string> section, UInt16 modindex) : this(modindex)
        {
            Parse(section);

            foreach (KeyValuePair<int, int> kvp in this.objectlist)
            {
                connectedmodulelist.Add((UInt16)kvp.Value, (UInt16)kvp.Key);
            }
        }
    }

    public partial class ModuleInfo : InfoSection
    {
        public ModuleInfo(Dictionary<string, string> section, UInt16 moduleindex) : this(moduleindex)
        {
            Parse(section, edssection);
        }
    }

    public partial class ModuleComments : Comments
    {
        public ModuleComments(Dictionary<string, string> section, UInt16 moduleindex) : this(moduleindex)
        {
            Parse(section);
        }
    }

    public partial class ModuleSubExtends : SupportedObjects
    {
        public ModuleSubExtends(Dictionary<string, string> section, UInt16 moduleindex)
              : this(moduleindex)
        {
            Parse(section);
        }
    }

    public partial class ODentry
    {
        /// <summary>
        /// Write out this Object dictionary entry to an EDS/DCF file using correct formatting
        /// </summary>
        /// <param name="writer">Handle to the stream writer to write to</param>
        /// <param name="ft">File type being written</param>
        /// <param name="odt">OD type to write</param>
        /// <param name="module">module</param>
        public void Write(StreamWriter writer, InfoSection.Filetype ft, Odtype odt = Odtype.NORMAL, int module = 0)
        {
            string fixedmodheader = "";

            if (odt == Odtype.FIXED)
            {
                fixedmodheader = string.Format("M{0}Fixed", module);
            }

            if (odt == Odtype.SUBEXT)
            {
                fixedmodheader = string.Format("M{0}SubExt", module);
            }

            if (parent != null)
            {
                writer.WriteLine(string.Format("[{0}{1:X}sub{2:X}]", fixedmodheader, Index, Subindex));
            }
            else
            {
                writer.WriteLine(string.Format("[{0}{1:X}]", fixedmodheader, Index));
            }

            writer.WriteLine(string.Format("ParameterName={0}", parameter_name));

            if (ft == InfoSection.Filetype.File_DCF)
            {
                writer.WriteLine(string.Format("Denotation={0}", denotation));
            }

            writer.WriteLine(string.Format("ObjectType=0x{0:X}", (int)objecttype));
            writer.WriteLine(string.Format(";StorageLocation={0}", prop.CO_storageGroup));

            if (objecttype == ObjectType.ARRAY)
            {
                writer.WriteLine(string.Format("SubNumber=0x{0:X}", Nosubindexes));
            }

            if (objecttype == ObjectType.RECORD)
            {
                writer.WriteLine(string.Format("SubNumber=0x{0:X}", Nosubindexes));
            }

            if (objecttype == ObjectType.VAR)
            {
                DataType dt = datatype;
                if (dt == DataType.UNKNOWN && this.parent != null)
                    dt = parent.datatype;
                writer.WriteLine(string.Format("DataType=0x{0:X4}", (int)dt));
                writer.WriteLine(string.Format("AccessType={0}", accesstype.ToString()));

                if (HighLimit != null && HighLimit != "")
                {
                    writer.WriteLine(string.Format("HighLimit={0}", Formatoctetstring(HighLimit)));
                }

                if (LowLimit != null && LowLimit != "")
                {
                    writer.WriteLine(string.Format("LowLimit={0}", Formatoctetstring(LowLimit)));
                }

                writer.WriteLine(string.Format("DefaultValue={0}", Formatoctetstring(defaultvalue)));

                //TODO If the ObjectType is domain (0x2) the value of the object may be stored in a file,UploadFile and DownloadFile
                if (ft == InfoSection.Filetype.File_DCF)
                {
                    writer.WriteLine(string.Format("ParameterValue={0}", Formatoctetstring(actualvalue)));
                }

                writer.WriteLine(string.Format("PDOMapping={0}", PDOMapping == true ? 1 : 0));

                if (prop.CO_flagsPDO == true)
                {
                    writer.WriteLine(";TPDODetectCos=1");
                }
            }

            //Count is for modules in the [MxSubExtxxxx]
            //Should we export this on EDS only, or DCF or both?
            if (odt == Odtype.SUBEXT)
            {
                writer.WriteLine(string.Format("Count={0}", count));
                writer.WriteLine(string.Format("ObjExtend={0}", ObjExtend));
            }

            //ObjectFlags is always optional (Page 15, DSP306) and used for DCF writing to nodes
            //also recommended not to write if it is already 0
            if (ObjFlags != 0)
            {
                writer.WriteLine(string.Format("ObjFlags={0}", ObjFlags));
            }

            writer.WriteLine("");
        }
    }
    public partial class EDSsharp
    {
        public void Parseline(string linex, int no)
        {
            string key = "";
            string value = "";

            string line = linex.TrimStart(';');
            bool custom_extension = false;

            if (linex == null || linex == "")
                return;

            if (linex[0] == ';')
                custom_extension = true;

            //extract sections
            {
                string pat = @"^\[([a-z0-9]+)\]";

                Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                Match m = r.Match(line);
                if (m.Success)
                {
                    Group g = m.Groups[1];
                    sectionname = g.ToString();

                    if (!eds.ContainsKey(sectionname))
                    {
                        eds.Add(sectionname, new Dictionary<string, string>());
                    }
                    else
                    {
                        Warnings.warning_list.Add(string.Format("EDS Error on Line {0} : Duplicate section [{1}] ", no, sectionname));
                    }
                }
            }

            //extract keyvalues
            {
                //Bug #70 Eat whitespace!
                string pat = @"^([a-z0-9_]+)[ ]*=[ ]*(.*)";

                Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                Match m = r.Match(line);
                if (m.Success)
                {
                    key = m.Groups[1].ToString();
                    value = m.Groups[2].ToString();
                    value = value.TrimEnd(' ', '\t', '\n', '\r');

                    //not sure how we actually get here with out a section being in the dictionary already..
                    //suspect this is dead code.
                    if (!eds.ContainsKey(sectionname))
                    {
                        eds.Add(sectionname, new Dictionary<string, string>());
                    }

                    if (custom_extension == false)
                    {
                        try
                        {
                            eds[sectionname].Add(key, value);
                        }
                        catch (Exception)
                        {
                            Warnings.warning_list.Add(string.Format("EDS Error on Line {3} : Duplicate key \"{0}\" value \"{1}\" in section [{2}]", key, value, sectionname, no));
                        }
                    }
                    else
                    //Only allow our own extensions to populate the key/value pair
                    {
                        if (key == "StorageLocation" || key == "TPDODetectCos")
                        {
                            try
                            {
                                eds[sectionname].Add(key, value);
                            }
                            catch (Exception)
                            {
                                Warnings.warning_list.Add(string.Format("EDS Error on Line {3} : Duplicate custom key \"{0}\" value \"{1}\" in section [{2}]", key, value, sectionname, no));
                            }
                        }
                    }
                }
            }
        }

        public void ParseEDSentry(KeyValuePair<string, Dictionary<string, string>> kvp)
        {
            string section = kvp.Key;

            string pat = @"^(M[0-9a-fA-F]+(Fixed|SubExt))?([a-fA-F0-9]+)(sub)?([0-9a-fA-F]*)$";

            Regex r = new Regex(pat);
            Match m = r.Match(section);
            if (m.Success)
            {
                SortedDictionary<UInt16, ODentry> target = this.ods;

                //** MODULE DCF SUPPORT

                string pat2 = @"^M([0-9a-fA-F]+)(Fixed|SubExt)([0-9a-fA-F]+)";
                Regex r2 = new Regex(pat2, RegexOptions.IgnoreCase);
                Match m2 = r2.Match(m.Groups[0].ToString());

                if (m2.Success)
                {
                    UInt16 modindex = 0, odindex = 0;

                    try { modindex = Convert.ToUInt16(m2.Groups[1].Value); }
                    catch (Exception) { Console.WriteLine("** ALL GONE WRONG **" + m2.Groups[1].Value); }
                    //Indexes in the EDS are always in hex format without the pre 0x
                    try { odindex = Convert.ToUInt16(m2.Groups[3].Value, 16); }
                    catch (Exception) { Console.WriteLine("** ALL GONE WRONG **" + m2.Groups[3].Value); }


                    if (!modules.ContainsKey(modindex))
                        modules.Add(modindex, new Module(modindex));

                    if (m2.Groups[2].ToString() == "SubExt")
                    {
                        target = modules[modindex].modulesubext;
                    }
                    else
                    {
                        target = modules[modindex].modulefixedobjects;
                    }
                }

                ODentry od = new ODentry
                {
                    //Indexes in the EDS are always in hex format without the pre 0x
                    Index = Convert.ToUInt16(m.Groups[3].ToString(), 16)
                };

                //Parameter name, mandatory always
                if (!kvp.Value.ContainsKey("ParameterName"))
                    throw new ParameterException("Missing required field ParameterName on" + section);
                od.parameter_name = kvp.Value["ParameterName"];

                //Object type, assumed to be VAR unless specified
                if (kvp.Value.ContainsKey("ObjectType"))
                {
                    int type = Convert.ToInt16(kvp.Value["ObjectType"], Getbase(kvp.Value["ObjectType"]));
                    od.objecttype = (ObjectType)type;
                }
                else
                {
                    od.objecttype = ObjectType.VAR;
                }

                if (kvp.Value.ContainsKey("CompactSubObj"))
                {
                    od.CompactSubObj = Convert.ToByte(kvp.Value["CompactSubObj"], Getbase(kvp.Value["CompactSubObj"]));
                }

                if (kvp.Value.ContainsKey("ObjFlags"))
                {
                    od.ObjFlags = Convert.ToUInt32(kvp.Value["ObjFlags"], Getbase(kvp.Value["ObjFlags"]));
                }
                else
                {
                    od.ObjFlags = 0;
                }

                //Access Type
                if (kvp.Value.ContainsKey("StorageLocation"))
                {
                    od.prop.CO_storageGroup = kvp.Value["StorageLocation"];
                }

                if (kvp.Value.ContainsKey("TPDODetectCos"))
                {
                    string test = kvp.Value["TPDODetectCos"].ToLower();
                    if (test == "1" || test == "true")
                    {
                        od.prop.CO_flagsPDO = true;
                    }
                    else
                        od.prop.CO_flagsPDO = false;
                }

                if (kvp.Value.ContainsKey("Count"))
                {
                    /*  FIXME: The format of  "Count" is Unsigned8[; Unsigned8] according DS306
                     *  Count:
                        Number of extended Sub-Indexes with this description that are created per module. The format is Unsigned8 [; Unsigned8].
                        If one or more Sub - Indexes are created per attached module to build a new sub- index, then Count is that
                        Number. In example 32 bit module creates 4 Sub - Indexes each having 8 Bit: Count = 4
                        If several modules are gathered to form a new Sub- Index, then the number is 0, followed by semicolon and the
                        number of bits that are created per module to build a new Sub-Index.In example 2 bit modules with 8 bit objects: The
                        first Sub - Index is built upon modules 1 - 4, the next upon modules 5 - 8 etc.: Count = 0; 2.The objects are created,
                        when a new byte begins: Module 1 creates the Sub - Index 1; modules 2 - 4 fill it up; module 5 creates Sub-Index 2 and
                        so forth.
                    */
                    pat2 = @"\s*([0-9a-fA-F]+)\s*;\s*([0-9a-fA-F]+)";
                    r2 = new Regex(pat2, RegexOptions.IgnoreCase);
                    m2 = r2.Match(kvp.Value["Count"]);

                    if (m2.Success)
                    {
                        Console.WriteLine("** FIXME Count format not supported ** Count: " + kvp.Value["Count"]);
                        int found = kvp.Value["Count"].IndexOf(";");
                        string s = kvp.Value["Count"].Substring(found + 1);
                        try { od.count = Convert.ToByte(s, Getbase(s)); }
                        catch (Exception) { Console.WriteLine("** ALL GONE WRONG ** Count" + kvp.Value["Count"]); }
                    }
                    else
                    {
                        try { od.count = Convert.ToByte(kvp.Value["Count"], Getbase(kvp.Value["Count"])); }
                        catch (Exception) { Console.WriteLine("** ALL GONE WRONG ** Count" + kvp.Value["Count"]); }
                    }
                }

                if (kvp.Value.ContainsKey("ObjExtend"))
                {
                    try { od.ObjExtend = Convert.ToByte(kvp.Value["ObjExtend"]); }
                    catch (Exception) { Console.WriteLine("** ALL GONE WRONG ** ObjExtend:" + kvp.Value["ObjExtend"]); }
                }


                if (od.objecttype == ObjectType.VAR)
                {
                    if (kvp.Value.ContainsKey("CompactSubObj"))
                        throw new ParameterException("CompactSubObj not valid for a VAR Object, section: " + section);

                    if (kvp.Value.ContainsKey("ParameterValue"))
                    {
                        try { od.actualvalue = kvp.Value["ParameterValue"]; }
                        catch (Exception) { Console.WriteLine("** ALL GONE WRONG ** ParameterValue:" + kvp.Value["ParameterValue"]); }
                    }

                    if (kvp.Value.ContainsKey("HighLimit"))
                    {
                        try { od.HighLimit = kvp.Value["HighLimit"]; }
                        catch (Exception) { Console.WriteLine("** ALL GONE WRONG ** HighLimit:" + kvp.Value["HighLimit"]); }
                    }

                    if (kvp.Value.ContainsKey("LowLimit"))
                    {
                        try { od.LowLimit = kvp.Value["LowLimit"]; }
                        catch (Exception) { Console.WriteLine("** ALL GONE WRONG ** LowLimit:" + kvp.Value["LowLimit"]); }
                    }

                    if (kvp.Value.ContainsKey("Denotation"))
                    {
                        try { od.denotation = kvp.Value["Denotation"]; }
                        catch (Exception) { Console.WriteLine("** ALL GONE WRONG ** Denotation:" + kvp.Value["Denotation"]); }
                    }

                    if (m.Groups[5].Length != 0)
                    {
                        //FIXME are subindexes in hex always?
                        UInt16 subindex = Convert.ToUInt16(m.Groups[5].ToString(), 16);
                        od.parent = target[od.Index];
                        target[od.Index].subobjects.Add(subindex, od);
                    }

                    if (!kvp.Value.ContainsKey("DataType"))
                        throw new ParameterException("Missing required field DataType on" + section);
                    od.datatype = (DataType)Convert.ToInt16(kvp.Value["DataType"], Getbase(kvp.Value["DataType"]));

                    if (!kvp.Value.ContainsKey("AccessType"))
                        throw new ParameterException("Missing required AccessType on" + section);

                    string accesstype = kvp.Value["AccessType"].ToLower();

                    if (Enum.IsDefined(typeof(AccessType), accesstype))
                    {
                        od.accesstype = (AccessType)Enum.Parse(typeof(AccessType), accesstype);
                    }
                    else
                    {
                        throw new ParameterException("Unknown AccessType on" + section);
                    }

                    if (kvp.Value.ContainsKey("DefaultValue"))
                        od.defaultvalue = kvp.Value["DefaultValue"];

                    od.PDOtype = PDOMappingType.no;
                    if (kvp.Value.ContainsKey("PDOMapping"))
                    {
                        bool pdo = Convert.ToInt16(kvp.Value["PDOMapping"], Getbase(kvp.Value["PDOMapping"])) == 1;
                        if (pdo == true)
                            od.PDOtype = PDOMappingType.optional;
                    }
                }

                if (od.objecttype == ObjectType.RECORD || od.objecttype == ObjectType.ARRAY || od.objecttype == ObjectType.DEFSTRUCT)
                {
                    if (od.CompactSubObj != 0)
                    {
                        if (!kvp.Value.ContainsKey("DataType"))
                            throw new ParameterException("Missing required field DataType on" + section);
                        od.datatype = (DataType)Convert.ToInt16(kvp.Value["DataType"], Getbase(kvp.Value["DataType"]));

                        if (!kvp.Value.ContainsKey("AccessType"))
                            throw new ParameterException("Missing required AccessType on" + section);
                        string accesstype = kvp.Value["AccessType"];
                        if (Enum.IsDefined(typeof(AccessType), accesstype))
                        {
                            od.accesstype = (AccessType)Enum.Parse(typeof(AccessType), accesstype);
                        }
                        else
                        {
                            throw new ParameterException("Unknown AccessType on" + section);
                        }

                        //now we generate CompactSubObj number of var objects below this parent

                        if (od.CompactSubObj >= 0xfe)
                        {
                            od.CompactSubObj = 0xfe;
                        }

                        ODentry subi = new ODentry("NrOfObjects", od.Index, DataType.UNSIGNED8, String.Format("0x{0:x2}", od.CompactSubObj), AccessType.ro, PDOMappingType.no, od);
                        od.subobjects.Add(0x00, subi);

                        for (int x = 1; x <= od.CompactSubObj; x++)
                        {
                            string parameter_name = string.Format("{0}{1:x2}", od.parameter_name, x);
                            ODentry sub = new ODentry(parameter_name, od.Index, od.datatype, od.defaultvalue, od.accesstype, od.PDOtype, od);

                            if (kvp.Value.ContainsKey("HighLimit"))
                                sub.HighLimit = kvp.Value["HighLimit"];

                            if (kvp.Value.ContainsKey("LowLimit"))
                                sub.HighLimit = kvp.Value["LowLimit"];

                            od.subobjects.Add((ushort)(x), sub);
                        }
                    }
                    else
                    {
                        if (!kvp.Value.ContainsKey("SubNumber"))
                            throw new ParameterException("Missing SubNumber on Array for" + section);
                    }
                }

                if (od.objecttype == ObjectType.DOMAIN)
                {
                    od.datatype = DataType.DOMAIN;
                    od.accesstype = AccessType.rw;

                    if (kvp.Value.ContainsKey("DefaultValue"))
                        od.defaultvalue = kvp.Value["DefaultValue"];
                }

                //Only add top level to this list
                if (m.Groups[5].Length == 0)
                {
                    target.Add(od.Index, od);
                }
            }
        }

        public void Loadfile(string filename)
        {
            projectFilename = filename;

            if (Path.GetExtension(filename).ToLower() == ".eds")
            {
                edsfilename = filename;
            }

            if (Path.GetExtension(filename).ToLower() == ".dcf")
            {
                dcffilename = filename;
            }

            int lineno = 1;
            foreach (string linex in System.IO.File.ReadLines(filename))
            {
                Parseline(linex, lineno);
                lineno++;
            }

            di = new DeviceInfo(eds["DeviceInfo"]);

            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in eds)
            {
                try { ParseEDSentry(kvp); }
                catch (Exception) { Console.WriteLine("** ALL GONE WRONG **" + kvp); }
            }

            fi = new FileInfo(eds["FileInfo"]);
            if (eds.ContainsKey("DummyUsage"))
                du = new Dummyusage(eds["DummyUsage"]);

            md = new MandatoryObjects(eds["MandatoryObjects"]);

            if (eds.ContainsKey("OptionalObjects"))
                oo = new OptionalObjects(eds["OptionalObjects"]);

            if (eds.ContainsKey("ManufacturerObjects"))
                mo = new ManufacturerObjects(eds["ManufacturerObjects"]);

            if (eds.ContainsKey("TypeDefinitions"))
                td = new TypeDefinitions(eds["TypeDefinitions"]);

            //Only DCF not EDS files
            dc = new DeviceCommissioning();
            string strSection = "";
            if (eds.ContainsKey("DeviceCommissioning"))     // wrong section name as defined in the DSP302, but right spelling (for compabiltiy to some tools)
                strSection = "DeviceCommissioning";
            else if (eds.ContainsKey("DeviceComissioning")) // right section name as defined in the DSP302, (wrong spelling)
                strSection = "DeviceComissioning";

            if (strSection != "")
            {
                dc.Parse(eds[strSection], "DeviceCommissioning");
                edsfilename = fi.LastEDS;
            }

            c = new Comments();

            if (eds.ContainsKey("Comments"))
                c.Parse(eds["Comments"]);

            //Modules

            //FIXME
            //we don't parse or support [MxFixedObjects] with MxFixedxxxx and MxFixedxxxxsubx

            if (eds.ContainsKey("SupportedModules"))
            {
                sm = new SupportedModules(eds["SupportedModules"]);

                //find MxModuleInfo

                foreach (string s in eds.Keys)
                {
                    String pat = @"M([0-9]+)ModuleInfo";
                    Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                    Match m = r.Match(s);

                    if (m.Success)
                    {
                        UInt16 modindex = Convert.ToUInt16(m.Groups[1].Value);
                        ModuleInfo mi = new ModuleInfo(eds[s], modindex);

                        if (!modules.ContainsKey(modindex))
                            modules.Add(modindex, new Module(modindex));

                        modules[modindex].mi = mi;
                    }

                    pat = @"M([0-9]+)Comments";
                    r = new Regex(pat, RegexOptions.IgnoreCase);
                    m = r.Match(s);

                    if (m.Success)
                    {
                        UInt16 modindex = Convert.ToUInt16(m.Groups[1].Value);
                        ModuleComments mc = new ModuleComments(eds[s], modindex);

                        if (!modules.ContainsKey(modindex))
                            modules.Add(modindex, new Module(modindex));

                        modules[modindex].mc = mc;
                    }
                    pat = @"M([0-9]+)SubExtends";
                    r = new Regex(pat, RegexOptions.IgnoreCase);
                    m = r.Match(s);

                    if (m.Success)
                    {
                        UInt16 modindex = Convert.ToUInt16(m.Groups[1].Value);
                        ModuleSubExtends mse = new ModuleSubExtends(eds[s], modindex);

                        if (!modules.ContainsKey(modindex))
                            modules.Add(modindex, new Module(modindex));

                        modules[modindex].mse = mse;
                    }

                    //DCF only

                    pat = @"M([0-9]+)FixedObjects";
                    r = new Regex(pat, RegexOptions.IgnoreCase);
                    m = r.Match(s);

                    if (m.Success)
                    {
                        UInt16 modindex = Convert.ToUInt16(m.Groups[1].Value);
                        MxFixedObjects mxf = new MxFixedObjects(eds[s], modindex);

                        if (!modules.ContainsKey(modindex))
                            modules.Add(modindex, new Module(modindex));

                        modules[modindex].mxfo = mxf;
                    }
                }

                if (eds.ContainsKey("ConnectedModules"))
                {
                    cm = new ConnectedModules(eds["ConnectedModules"]);
                }

                //COMPACT PDO

                if (di.CompactPDO != 0)
                {
                    for (UInt16 index = 0x1400; index < 0x1600; index++)
                    {
                        ApplycompactPDO(index);
                    }

                    for (UInt16 index = 0x1800; index < 0x1A00; index++)
                    {
                        ApplycompactPDO(index);
                    }
                }

                ApplyimplicitPDO();
            }
            // catch(Exception e)
            //{
            //  Console.WriteLine("** ALL GONE WRONG **" + e.ToString());
            // }
        }

        void ApplycompactPDO(UInt16 index)
        {
            if (ods.ContainsKey(index))
            {
                if ((!ods[index].Containssubindex(1)) && ((this.di.CompactPDO & 0x01) == 0))
                {
                    //Fill in cob ID
                    //FIX ME i'm really sure this is not correct, what default values should be used???
                    string cob = string.Format("$NODEID + 0x180");
                    ODentry subod = new ODentry("COB-ID", index, DataType.UNSIGNED32, cob, AccessType.rw, PDOMappingType.no, ods[index]);
                    ods[index].subobjects.Add(0x05, subod);
                }

                if ((!ods[index].Containssubindex(2)) && ((this.di.CompactPDO & 0x02) == 0))
                {
                    //Fill in type
                    ODentry subod = new ODentry("Type", index, DataType.UNSIGNED8, "0xff", AccessType.rw, PDOMappingType.no, ods[index]);
                    ods[index].subobjects.Add(0x02, subod);
                }

                if ((!ods[index].Containssubindex(3)) && ((this.di.CompactPDO & 0x04) == 0))
                {
                    //Fill in inhibit
                    ODentry subod = new ODentry("Inhibit time", index, DataType.UNSIGNED16, "0", AccessType.rw, PDOMappingType.no, ods[index]);
                    ods[index].subobjects.Add(0x03, subod);
                }

                //NOT FOR RX PDO
                if (index < 0x1800)
                    return;

                if ((!ods[index].Containssubindex(4)) && ((this.di.CompactPDO & 0x08) == 0))
                {
                    //Fill in compatibility entry
                    ODentry subod = new ODentry("Compatibility entry", index, DataType.UNSIGNED8, "0", AccessType.ro, PDOMappingType.no, ods[index]);
                    ods[index].subobjects.Add(0x04, subod);
                }

                if ((!ods[index].Containssubindex(5)) && ((this.di.CompactPDO & 0x10) == 0))
                {
                    //Fill in event timer
                    ODentry subod = new ODentry("Event Timer", index, DataType.UNSIGNED16, "0", AccessType.rw, PDOMappingType.no, ods[index]);
                    ods[index].subobjects.Add(0x05, subod);
                }
            }
        }

        /// <summary>
        /// This function scans the PDO list and compares it to NrOfRXPDO and NrOfTXPDO
        /// if these do not match in count then implicit PDOs are present and they are
        /// filled in with default values from the lowest possible index
        /// </summary>
        protected void ApplyimplicitPDO()
        {
            UInt16 totalnorxpdos = di.NrOfRXPDO;
            UInt16 totalnotxpdos = di.NrOfTXPDO;

            UpdatePDOcount();

            UInt16 noexplicitrxpdos = di.NrOfRXPDO;
            UInt16 noexplicittxpdos = di.NrOfTXPDO;

            //this is how many PDOS need generating on the fly
            UInt16 noimplictrxpdos = (UInt16)(totalnorxpdos - noexplicitrxpdos);
            UInt16 noimplicttxpdos = (UInt16)(totalnotxpdos - noexplicittxpdos);

            for (UInt16 index = 0x1400; (index < 0x1600) && (noimplictrxpdos > 0); index++)
            {
                if (!ods.ContainsKey(index))
                {
                    CreateRXPDO(index);
                    noimplictrxpdos--;
                }
            }

            for (UInt16 index = 0x1800; (index < 0x1A00) && (noimplicttxpdos > 0); index++)
            {
                if (!ods.ContainsKey(index))
                {
                    CreateTXPDO(index);
                    noimplicttxpdos--;
                }
            }
            UpdatePDOcount();
        }

        public void Savefile(string filename, InfoSection.Filetype ft)
        {
            if (ft == InfoSection.Filetype.File_EDS)
                this.edsfilename = filename;

            if (ft == InfoSection.Filetype.File_DCF)
            {
                this.dcffilename = filename;
                fi.LastEDS = edsfilename;
            }

            UpdatePDOcount();

            //generate date times in DS306 format; h:mmtt MM-dd-yyyy

            fi.CreationDate = fi.CreationDateTime.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            fi.CreationTime = fi.CreationDateTime.ToString("h:mmtt", CultureInfo.InvariantCulture);

            fi.ModificationDate = fi.ModificationDateTime.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            fi.ModificationTime = fi.ModificationDateTime.ToString("h:mmtt", CultureInfo.InvariantCulture);

            fi.FileName = Path.GetFileName(filename);

            fi.EDSVersion = "4.0";
            fi.EDSVersionMajor = 4;
            fi.EDSVersionMinor = 0;

            StreamWriter writer = System.IO.File.CreateText(filename);
            writer.NewLine = "\n";
            fi.Write(writer, ft);
            di.Write(writer, ft);
            du.Write(writer, ft);
            c.Write(writer);

            if (ft == InfoSection.Filetype.File_DCF)
            {
                dc.Write(writer, ft);
            }

            //regenerate the object lists
            md.objectlist.Clear();
            mo.objectlist.Clear();
            oo.objectlist.Clear();

            foreach (KeyValuePair<UInt16, ODentry> kvp in ods)
            {
                ODentry entry = kvp.Value;

                if (entry.prop.CO_disabled == true)
                    continue;

                if (entry.Index == 0x1000 || entry.Index == 0x1001 || entry.Index == 0x1018)
                {
                    md.objectlist.Add(md.objectlist.Count + 1, entry.Index);
                }
                else
                if (entry.Index >= 0x2000 && entry.Index < 0x6000)
                {
                    mo.objectlist.Add(mo.objectlist.Count + 1, entry.Index);
                }
                else
                {
                    oo.objectlist.Add(oo.objectlist.Count + 1, entry.Index);
                }
            }

            md.Write(writer);

            foreach (KeyValuePair<UInt16, ODentry> kvp in ods)
            {
                ODentry od = kvp.Value;
                if (md.objectlist.ContainsValue(od.Index))
                {
                    od.Write(writer, ft);
                    foreach (KeyValuePair<UInt16, ODentry> kvp2 in od.subobjects)
                    {
                        ODentry od2 = kvp2.Value;
                        od2.Write(writer, ft);
                    }
                }
            }

            oo.Write(writer);

            foreach (KeyValuePair<UInt16, ODentry> kvp in ods)
            {
                ODentry od = kvp.Value;
                if (oo.objectlist.ContainsValue(od.Index))
                {
                    od.Write(writer, ft);
                    foreach (KeyValuePair<UInt16, ODentry> kvp2 in od.subobjects)
                    {
                        ODentry od2 = kvp2.Value;
                        od2.Write(writer, ft);
                    }
                }
            }

            mo.Write(writer);

            foreach (KeyValuePair<UInt16, ODentry> kvp in ods)
            {
                ODentry od = kvp.Value;
                if (mo.objectlist.ContainsValue(od.Index))
                {
                    od.Write(writer, ft);
                    foreach (KeyValuePair<UInt16, ODentry> kvp2 in od.subobjects)
                    {
                        ODentry od2 = kvp2.Value;
                        od2.Write(writer, ft);
                    }
                }
            }

            //modules

            if (sm.NrOfEntries > 0)
            {
                sm.Write(writer, ft);

                for (UInt16 moduleid = 1; moduleid <= sm.NrOfEntries; moduleid++)
                {
                    modules[moduleid].mi.Write(writer, ft);

                    modules[moduleid].mc.Write(writer);

                    modules[moduleid].mse.Write(writer);

                    foreach (KeyValuePair<UInt16, ODentry> kvp2 in modules[moduleid].modulesubext)
                    {
                        ODentry od = kvp2.Value;
                        od.Write(writer, ft, ODentry.Odtype.SUBEXT, moduleid);
                    }

                    modules[moduleid].mxfo.Write(writer);

                    foreach (KeyValuePair<UInt16, ODentry> kvp3 in modules[moduleid].modulefixedobjects)
                    {
                        ODentry od = kvp3.Value;
                        od.Write(writer, ft, ODentry.Odtype.SUBEXT, moduleid);

                        foreach (KeyValuePair<UInt16, ODentry> kvp4 in od.subobjects)
                        {
                            ODentry subod = kvp4.Value;
                            subod.Write(writer, ft, ODentry.Odtype.FIXED, moduleid);
                        }
                    }
                }
            }

            if (ft == InfoSection.Filetype.File_DCF)
            {
                if (cm.NrOfEntries > 0)
                {
                    cm.Write(writer);
                }
            }
            writer.Close();
        }

        //RX COM 0x1400
        //RX Map 0x1600
        //TX COM 0x1800
        //TX MAP 0x1a00

        //call this with the comm param index not the mapping
        bool CreatePDO(bool rx, UInt16 index)
        {
            //check if we are creating an RX PDO it is a valid index
            if (rx && (index < 0x1400 || index >= 0x1600))
                return false;

            //check if we are creating an PDO TX it is a valid index
            if (!rx & (index < 0x1800 || index >= 0x1A00))
                return false;

            //Check it does not already exist
            if (ods.ContainsKey(index))
                return false;

            //check the associated mapping index does not exist
            if (ods.ContainsKey((UInt16)(index + 0x200)))
                return false;

            ODentry od_comparam;
            ODentry od_mapping;

            if (rx)
            {
                od_comparam = new ODentry("RPDO communication parameter", index, 0)
                {
                    Description = @"0x1400 - 0x15FF RPDO communication parameter
max sub-index

COB - ID
 bit  0 - 10: COB - ID for PDO, to change it bit 31 must be set
 bit 11 - 29: set to 0 for 11 bit COB - ID
 bit 30:    0(1) - rtr are allowed(are NOT allowed) for PDO
 bit 31:    0(1) - node uses(does NOT use) PDO

Transmission type
 value = 0 - 240:   receiving is synchronous, process after next reception of SYNC object
 value = 241 - 253: not used
 value = 254:     manufacturer specific
 value = 255:     asynchronous"
                };

                od_mapping = new ODentry("RPDO mapping parameter", (UInt16)(index + 0x200), 0)
                {
                    Description = @"0x1600 - 0x17FF RPDO mapping parameter (To change mapping, 'Number of mapped objects' must be set to 0)
Number of mapped objects

mapped object  (subindex 1...8)
 bit  0 - 7:  data length in bits
 bit 8 - 15:  subindex from OD
 bit 16 - 31: index from OD"
                };
            }
            else
            {
                od_comparam = new ODentry("TPDO communication parameter", index, 0)
                {
                    Description = @"0x1800 - 0x19FF TPDO communication parameter
max sub-index

COB - ID
 bit  0 - 10: COB - ID for PDO, to change it bit 31 must be set
 bit 11 - 29: set to 0 for 11 bit COB - ID
 bit 30:    0(1) - rtr are allowed(are NOT allowed) for PDO
 bit 31:    0(1) - node uses(does NOT use) PDO

Transmission type
 value = 0:       transmitting is synchronous, specification in device profile
 value = 1 - 240:   transmitting is synchronous after every N - th SYNC object
 value = 241 - 251: not used
 value = 252 - 253: Transmitted only on reception of Remote Transmission Request
 value = 254:     manufacturer specific
 value = 255:     asynchronous, specification in device profile

inhibit time
 bit 0 - 15:  Minimum time between transmissions of the PDO in 100µs.Zero disables functionality.

event timer
 bit 0-15:  Time between periodic transmissions of the PDO in ms.Zero disables functionality.

SYNC start value
 value = 0:       Counter of the SYNC message shall not be processed.
 value = 1-240:   The SYNC message with the counter value equal to this value shall be regarded as the first received SYNC message."
                };

                od_mapping = new ODentry("TPDO mapping parameter", (UInt16)(index + 0x200), 0)
                {
                    Description = @"0x1A00 - 0x1BFF TPDO mapping parameter. (To change mapping, 'Number of mapped objects' must be set to 0).
Number of mapped objects

mapped object  (subindex 1...8)
 bit   0 - 7: data length in bits
 bit  8 - 15: subindex from OD
 bit 16 - 31: index from OD"
                };
            }

            od_comparam.objecttype = ObjectType.RECORD;
            od_comparam.prop.CO_storageGroup = "ROM";
            od_comparam.accesstype = AccessType.ro;
            od_comparam.PDOtype = PDOMappingType.no;

            ODentry sub;

            if (rx)
            {
                sub = new ODentry("max sub-index", index, DataType.UNSIGNED8, "2", AccessType.ro, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(0, sub);
                sub = new ODentry("COB-ID used by RPDO", index, DataType.UNSIGNED32, "$NODEID+0x200", AccessType.rw, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(1, sub);
                sub = new ODentry("transmission type", index, DataType.UNSIGNED8, "254", AccessType.rw, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(2, sub);
            }
            else
            {
                sub = new ODentry("max sub-index", index, DataType.UNSIGNED8, "6", AccessType.ro, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(0, sub);
                sub = new ODentry("COB-ID used by TPDO", index, DataType.UNSIGNED32, "$NODEID+0x180", AccessType.rw, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(1, sub);
                sub = new ODentry("transmission type", index, DataType.UNSIGNED8, "254", AccessType.rw, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(2, sub);
                sub = new ODentry("inhibit time", index, DataType.UNSIGNED16, "0", AccessType.rw, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(3, sub);
                //sub = new ODentry("compatibility entry", index, DataType.UNSIGNED8, "0", AccessType.rw, PDOMappingType.no, od_comparam);
                //od_comparam.subobjects.Add(4, sub);
                sub = new ODentry("event timer", index, DataType.UNSIGNED16, "0", AccessType.rw, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(5, sub);
                sub = new ODentry("SYNC start value", index, DataType.UNSIGNED8, "0", AccessType.rw, PDOMappingType.no, od_comparam);
                od_comparam.subobjects.Add(6, sub);
            }

            od_mapping.objecttype = ObjectType.RECORD;
            od_mapping.prop.CO_storageGroup = "ROM";
            od_mapping.accesstype = AccessType.rw; //Same as default but inconsistent with ROM above
            od_mapping.PDOtype = PDOMappingType.no;

            sub = new ODentry("Number of mapped objects", (UInt16)(index + 0x200), DataType.UNSIGNED8, "0", AccessType.ro, PDOMappingType.no, od_mapping);
            od_mapping.subobjects.Add(0, sub);

            for (int p = 1; p <= 8; p++)
            {
                sub = new ODentry(string.Format("mapped object {0}", p), (UInt16)(index + 0x200), DataType.UNSIGNED32, "0x00000000", AccessType.ro, PDOMappingType.no, od_mapping);
                od_mapping.subobjects.Add((byte)p, sub);
            }

            ods.Add(index, od_comparam);
            ods.Add((UInt16)(index + 0x200), od_mapping);

            return true;
        }

        bool CreateTXPDO(UInt16 index)
        {
            return CreatePDO(false, index);
        }

        bool CreateRXPDO(UInt16 index)
        {
            return CreatePDO(true, index);
        }
    }
}
