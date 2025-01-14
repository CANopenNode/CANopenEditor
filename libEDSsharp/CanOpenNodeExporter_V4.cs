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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Reflection;

namespace libEDSsharp
{
    /// <summary>
    /// Exporter for CanOpenNode_V4
    /// </summary>
    public class CanOpenNodeExporter_V4 : IExporter, IFileExporter
    {
        private string odname;

        private List<string> ODStorageGroups;
        private Dictionary<string, List<string>> ODStorage_t;
        private Dictionary<string, List<string>> ODStorage;

        private List<string> ODObjs_t;
        private List<string> ODObjs;
        private List<string> ODList;
        private List<string> ODDefines;
        private List<string> ODDefinesLong;
        private Dictionary<string, UInt16> ODCnt;
        private Dictionary<string, int> ODArrSize;

        /// <summary>
        /// Fetches all the different fileexporter types the class supports
        /// </summary>
        /// <returns>List of the different exporters the class supports</returns>
        public ExporterDescriptor[] GetExporters()
        {
            return new ExporterDescriptor[]
            {
                new ExporterDescriptor("CanOpenNodeV4", new string[] { ".h", ".c" }, ExporterDescriptor.ExporterFlags.CanOpenNode, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new CanOpenNodeExporter_V4();
                    e.export(filepath, edss[0]);
                })
            };
        }

        /// <summary>
        /// export the current data set in the CanOpen Node format V4
        /// </summary>
        /// <param name="filepath">filepath, .c and .h will be added to this to make the mulitiple files</param>
        /// <param name="eds"></param>
        public void export(string filepath, EDSsharp eds)
        {
            string filename = Path.GetFileNameWithoutExtension(filepath);
            string folderpath = Path.GetDirectoryName(filepath);
            this.odname = filename;

            Prepare(eds);

            var versionAttributes = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                as AssemblyInformationalVersionAttribute[];

            string gitVersion = versionAttributes[0].InformationalVersion;
            Export_h(folderpath, filename, gitVersion, eds);
            Export_c(folderpath, filename, gitVersion, eds);
        }

        #region Prepare
        /// <summary>
        /// Generate ODStorage, ODObjs, ODList, ODDefines and ODCnt entries
        /// </summary>
        /// <param name="eds">EDS object</param>
        private void Prepare(EDSsharp eds)
        {
            ODStorageGroups = new List<string>();
            ODStorage_t = new Dictionary<string, List<string>>();
            ODStorage = new Dictionary<string, List<string>>();

            ODObjs_t = new List<string>();
            ODObjs = new List<string>();
            ODList = new List<string>();
            ODDefines = new List<string>();
            ODDefinesLong = new List<string>();
            ODCnt = new Dictionary<string, UInt16>();
            ODArrSize = new Dictionary<string, int>();

            List<string> mappingErrors = eds.VerifyPDOMapping();
            if (mappingErrors.Count > 0)
                Warnings.AddWarning($"Errors in PDO mappings:\r\n    " + string.Join("\r\n    ", mappingErrors), Warnings.warning_class.WARNING_BUILD);
            foreach (ODentry od in eds.ods.Values)
            {
                if (od.prop.CO_disabled == true)
                    continue;

                string indexH = $"{od.Index:X4}";
                string cName = Make_cname(od.parameter_name);
                string varName = $"{indexH}_{cName}";

                // storage group
                if (ODStorageGroups.IndexOf(od.prop.CO_storageGroup) == -1)
                {
                    ODStorageGroups.Add(od.prop.CO_storageGroup);
                    ODStorage_t.Add(od.prop.CO_storageGroup, new List<string>());
                    ODStorage.Add(od.prop.CO_storageGroup, new List<string>());
                }

                string odObjectType = "";
                int subEntriesCount = 0;

                // ODStorage and ODObjs - object type specific
                switch (od.objecttype)
                {
                    case ObjectType.VAR:
                        odObjectType = "VAR";
                        subEntriesCount = Prepare_var(od, indexH, varName, od.prop.CO_storageGroup);
                        break;

                    case ObjectType.ARRAY:
                        odObjectType = "ARR";
                        subEntriesCount = Prepare_arr(od, indexH, varName, od.prop.CO_storageGroup);
                        break;

                    case ObjectType.RECORD:
                        odObjectType = "REC";
                        subEntriesCount = Prepare_rec(od, indexH, varName, od.prop.CO_storageGroup);
                        break;
                }

                if (subEntriesCount < 1)
                    continue;

                // defines
                ODDefines.Add($"#define {odname}_ENTRY_H{indexH} &{odname}->list[{ODList.Count}]");
                ODDefinesLong.Add($"#define {odname}_ENTRY_H{varName} &{odname}->list[{ODList.Count}]");

                // object dictionary
                ODList.Add($"{{0x{indexH}, 0x{subEntriesCount:X2}, ODT_{odObjectType}, &{odname}Objs.o_{varName}, NULL}}");

                // count labels
                if (od.prop.CO_countLabel != null && od.prop.CO_countLabel != "")
                {
                    if (ODCnt.ContainsKey(od.prop.CO_countLabel))
                        ODCnt[od.prop.CO_countLabel]++;
                    else
                        ODCnt.Add(od.prop.CO_countLabel, 1);
                }

                // Verify objects, if they have set correct "CO_countLabel", according to Object Dictionary Requirements By CANopenNode V4.
                // https://github.com/CANopenNode/CANopenNode/blob/master/doc/objectDictionary.md
                VerifyCountLabel(od, 0x1000, 0x1000, "NMT");
                VerifyCountLabel(od, 0x1001, 0x1001, "EM");
                VerifyCountLabel(od, 0x1005, 0x1005, "SYNC");
                VerifyCountLabel(od, 0x1006, 0x1006, "SYNC_PROD");
                VerifyCountLabel(od, 0x1010, 0x1010, "STORAGE");
                VerifyCountLabel(od, 0x1012, 0x1012, "TIME");
                VerifyCountLabel(od, 0x1014, 0x1014, "EM_PROD");
                VerifyCountLabel(od, 0x1016, 0x1016, "HB_CONS");
                VerifyCountLabel(od, 0x1017, 0x1017, "HB_PROD");
                VerifyCountLabel(od, 0x1200, 0x127F, "SDO_SRV");
                VerifyCountLabel(od, 0x1280, 0x12FF, "SDO_CLI");
                VerifyCountLabel(od, 0x1300, 0x1300, "GFC");
                VerifyCountLabel(od, 0x1301, 0x1340, "SRDO");
                VerifyCountLabel(od, 0x1400, 0x15FF, "RPDO");
                VerifyCountLabel(od, 0x1800, 0x19FF, "TPDO");
            }
        }

        /// <summary>
        /// Verify "Count Label" of the object and raise warning if uncorrect.
        /// </summary>
        /// <param name="od"></param>
        /// <param name="indexL"></param>
        /// <param name="indexH"></param>
        /// <param name="countLabel"></param>
        /// <returns></returns>
        private void VerifyCountLabel(ODentry od, ushort indexL, ushort indexH, string countLabel)
        {
            if (od.Index >= indexL && od.Index <= indexH && od.prop.CO_countLabel != countLabel)
            {
                Warnings.AddWarning($"Error in 0x{od.Index:X4}: 'Count Label' must be '{countLabel}'", Warnings.warning_class.WARNING_BUILD);
            }
        }

        /// <summary>
        /// Generate ODStorage and ODObjs entries for VAR
        /// </summary>
        /// <param name="od"></param>
        /// <param name="indexH"></param>
        /// <param name="varName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private int Prepare_var(ODentry od, string indexH, string varName, string group)
        {
            DataProperties data = Get_dataProperties(od.datatype, od.defaultvalue, od.prop.CO_stringLengthMin, indexH);
            string attr = Get_attributes(od, data.cTypeMultibyte, data.cTypeString);

            // data storage
            string dataPtr = "NULL";
            if (data.cValue != null)
            {
                ODStorage_t[group].Add($"{data.cType} x{varName}{data.cTypeArray};");
                ODStorage[group].Add($".x{varName} = {data.cValue}");
                dataPtr = $"&{odname}_{group}.x{varName}{data.cTypeArray0}";
            }

            // objects
            ODObjs_t.Add($"OD_obj_var_t o_{varName};");
            ODObjs.Add($"    .o_{varName} = {{");
            ODObjs.Add($"        .dataOrig = {dataPtr},");
            ODObjs.Add($"        .attribute = {attr},");
            ODObjs.Add($"        .dataLength = {data.length}");
            ODObjs.Add($"    }},");

            return 1;
        }

        /// <summary>
        /// Generate ODStorage and ODObjs entries for ARRAY
        /// </summary>
        /// <param name="od"></param>
        /// <param name="indexH"></param>
        /// <param name="varName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private int Prepare_arr(ODentry od, string indexH, string varName, string group)
        {
            int subEntriesCount = od.subobjects.Count;
            if (subEntriesCount < 2)
            {
                Warnings.AddWarning($"Error in 0x{indexH}: ARRAY must have minimum two sub entries, not {subEntriesCount}!", Warnings.warning_class.WARNING_BUILD);
                return 0;
            }

            // add size of array
            ODArrSize.Add(indexH, subEntriesCount - 1);

            // prepare and verify each sub element
            string cValue0 = "";
            DataProperties dataElem = new DataProperties();
            string attrElem0 = "";
            string attrElem = "";
            List<string> ODStorageValues = new List<string>();
            UInt16 i = 0;
            foreach (ODentry sub in od.subobjects.Values)
            {
                // If sub datatype is not known, use the od datatype
                DataType dataType = (sub.datatype != DataType.UNKNOWN) ? sub.datatype : od.datatype;

                DataProperties data = Get_dataProperties(dataType, sub.defaultvalue, sub.prop.CO_stringLengthMin, indexH);
                string attr = Get_attributes(sub, data.cTypeMultibyte, data.cTypeString);

                if (sub.Subindex != i)
                    Warnings.AddWarning($"Error in 0x{indexH}: SubIndexes in ARRAY must be in sequence!", Warnings.warning_class.WARNING_BUILD);

                if (i == 0)
                {
                    if (data.cType != "uint8_t" || data.length != 1)
                        Warnings.AddWarning($"Error in 0x{indexH}: Data type in ARRAY in subIndex 0 must be UNSIGNED8, not {sub.datatype}!", Warnings.warning_class.WARNING_BUILD);

                    cValue0 = data.cValue;
                    attrElem0 = attr;
                }
                else
                {
                    if (i == 1)
                    {
                        // First array element. Other array elements must match this elements in data type and attributes
                        dataElem = data;
                        attrElem = attr;
                    }
                    else
                    {// Following checks are requirement of CANopenNode. Arrays must be C arrays.
                        if (data.cType != dataElem.cType || data.length != dataElem.length)
                            Warnings.AddWarning($"Error in 0x{indexH}: Data type of elements in ARRAY must be equal!", Warnings.warning_class.WARNING_BUILD);
                        if ((data.cValue == null && dataElem.cValue != null) || (data.cValue != null && dataElem.cValue == null))
                            Warnings.AddWarning($"Error in 0x{indexH}: Default value must be defined on all ARRAY elements or must be undefined on all ARRAY elements!", Warnings.warning_class.WARNING_BUILD);
                        if (attr != attrElem)
                            Warnings.AddWarning($"Error in 0x{indexH}: Attributes of elements in ARRAY must be equal", Warnings.warning_class.WARNING_BUILD);
                    }
                    ODStorageValues.Add($"{data.cValue}");
                }
                i++;
            }
            string dataPtr0 = "NULL";
            string dataPtr = "NULL";
            if (cValue0 != null)
            {
                ODStorage_t[group].Add($"uint8_t x{varName}_sub0;");
                ODStorage[group].Add($".x{varName}_sub0 = {cValue0}");
                dataPtr0 = $"&{odname}_{group}.x{varName}_sub0";
            }
            if (dataElem.cValue != null)
            {
                ODStorage_t[group].Add($"{dataElem.cType} x{varName}[{odname}_CNT_ARR_{indexH}]{dataElem.cTypeArray};");
                ODStorage[group].Add($".x{varName} = {{{string.Join(", ", ODStorageValues)}}}");
                dataPtr = $"&{odname}_{group}.x{varName}[0]{dataElem.cTypeArray0}";
            }

            // sizeof data element inside the array
            string dataElementSizeof = dataElem.cType == "not specified" && dataElem.length == 0
                ? "0" : $"sizeof({dataElem.cType}{dataElem.cTypeArray})";

            // objects
            ODObjs_t.Add($"OD_obj_array_t o_{varName};");
            ODObjs.Add($"    .o_{varName} = {{");
            ODObjs.Add($"        .dataOrig0 = {dataPtr0},");
            ODObjs.Add($"        .dataOrig = {dataPtr},");
            ODObjs.Add($"        .attribute0 = {attrElem0},");
            ODObjs.Add($"        .attribute = {attrElem},");
            ODObjs.Add($"        .dataElementLength = {dataElem.length},");
            ODObjs.Add($"        .dataElementSizeof = {dataElementSizeof}");
            ODObjs.Add($"    }},");

            return subEntriesCount;
        }

        /// <summary>
        /// Generate ODStorage and ODObjs entries for RECORD
        /// </summary>
        /// <param name="od"></param>
        /// <param name="indexH"></param>
        /// <param name="varName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private int Prepare_rec(ODentry od, string indexH, string varName, string group)
        {
            int subEntriesCount = od.subobjects.Count;
            if (subEntriesCount < 2)
            {
                Warnings.AddWarning($"Error in 0x{indexH}: RECORD must have minimum two sub entries, not {subEntriesCount}!", Warnings.warning_class.WARNING_BUILD);
                return 0;
            }

            List<string> subODStorage_t = new List<string>();
            List<string> subODStorage = new List<string>();

            ODObjs_t.Add($"OD_obj_record_t o_{varName}[{subEntriesCount}];");
            ODObjs.Add($"    .o_{varName} = {{");

            foreach (ODentry sub in od.subobjects.Values)
            {
                DataProperties data = Get_dataProperties(sub.datatype, sub.defaultvalue, sub.prop.CO_stringLengthMin, indexH);
                string attr = Get_attributes(sub, data.cTypeMultibyte, data.cTypeString);

                if (sub.Subindex == 0 && (data.cType != "uint8_t" || data.length != 1))
                    Warnings.AddWarning($"Error in 0x{indexH}: Data type in RECORD, subIndex 0 must be UNSIGNED8, not {sub.datatype}!", Warnings.warning_class.WARNING_BUILD);

                string subcName = Make_cname(sub.parameter_name);
                string dataPtr = "NULL";
                if (data.cValue != null)
                {
                    subODStorage_t.Add($"{data.cType} {subcName}{data.cTypeArray};");
                    subODStorage.Add($".{subcName} = {data.cValue}");
                dataPtr = $"&{odname}_{group}.x{varName}.{subcName}{data.cTypeArray0}";
                }
                ODObjs.Add($"        {{");
                ODObjs.Add($"            .dataOrig = {dataPtr},");
                ODObjs.Add($"            .subIndex = {sub.Subindex},");
                ODObjs.Add($"            .attribute = {attr},");
                ODObjs.Add($"            .dataLength = {data.length}");
                ODObjs.Add($"        }},");

            }
            // remove last ',' and add closing bracket
            string s = ODObjs[ODObjs.Count - 1];
            ODObjs[ODObjs.Count - 1] = s.Remove(s.Length - 1);
            ODObjs.Add($"    }},");

            if (subODStorage_t.Count > 0)
            {
                ODStorage_t[group].Add($"struct {{\n        {string.Join("\n        ", subODStorage_t)}\n    }} x{varName};");
                ODStorage[group].Add($".x{varName} = {{\n        {string.Join(",\n        ", subODStorage)}\n    }}");
            }

            return subEntriesCount;
        }
        #endregion

        #region Exporters

        /// <summary>
        /// Export the header file
        /// </summary>
        /// <param name="folderpath">path to folder that will contain the file</param>
        /// <param name="filename">filename</param>
        /// <param name="gitVersion">git version that will be added to file comment</param>
        /// <param name="eds">data that contain the data that will be exported</param>
        private void Export_h(string folderpath, string filename, string gitVersion, EDSsharp eds)
        {

            if (filename == "")
                filename = "OD";

            StreamWriter file = new StreamWriter(folderpath + Path.DirectorySeparatorChar + filename + ".h");
            file.NewLine = "\n";

            file.WriteLine(string.Format(
@"/*******************************************************************************
    CANopen Object Dictionary definition for CANopenNode V4

    This file was automatically generated by CANopenEditor {0}

    https://github.com/CANopenNode/CANopenNode
    https://github.com/CANopenNode/CANopenEditor

    DON'T EDIT THIS FILE MANUALLY !!!!
********************************************************************************

    File info:
        File Names:   {1}.h; {1}.c
        Project File: {2}
        File Version: {3}

        Created:      {4}
        Created By:   {5}
        Modified:     {6}
        Modified By:  {7}

    Device Info:
        Vendor Name:  {8}
        Vendor ID:    {9}
        Product Name: {10}
        Product ID:   {11}

        Description:  {12}
*******************************************************************************/",
            gitVersion, odname,
            Path.GetFileName(eds.projectFilename), eds.fi.FileVersion,
            eds.fi.CreationDateTime, eds.fi.CreatedBy, eds.fi.ModificationDateTime, eds.fi.ModifiedBy,
            eds.di.VendorName, eds.di.VendorNumber, eds.di.ProductName, eds.di.ProductNumber,
            eds.fi.Description));

            file.WriteLine(string.Format(@"
#ifndef {0}_H
#define {0}_H
/*******************************************************************************
    Counters of OD objects
*******************************************************************************/",
            odname));

            foreach (KeyValuePair<string, UInt16> kvp in ODCnt)
            {
                file.WriteLine($"#define {odname}_CNT_{kvp.Key} {kvp.Value}");
            }

            file.WriteLine(string.Format(@"

/*******************************************************************************
    Sizes of OD arrays
*******************************************************************************/"));
            foreach (KeyValuePair<string, int> kvp in ODArrSize)
            {
                file.WriteLine($"#define {odname}_CNT_ARR_{kvp.Key} {kvp.Value}");
            }

            file.WriteLine(@"

/*******************************************************************************
    OD data declaration of all groups
*******************************************************************************/");
            foreach (string group in ODStorageGroups)
            {
                if (ODStorage_t.Count > 0)
                {
                    file.WriteLine($"typedef struct {{");
                    file.WriteLine($"    {string.Join("\n    ", ODStorage_t[group])}");
                    file.WriteLine($"}} {odname}_{group}_t;\n");
                }
            }

            foreach (string group in ODStorageGroups)
            {
                if (ODStorage_t.Count > 0)
                {
                    file.WriteLine($"#ifndef {odname}_ATTR_{group}");
                    file.WriteLine($"#define {odname}_ATTR_{group}");
                    file.WriteLine($"#endif");
                    file.WriteLine($"extern {odname}_ATTR_{group} {odname}_{group}_t {odname}_{group};\n");
                }
            }
            file.WriteLine($"#ifndef {odname}_ATTR_OD");
            file.WriteLine($"#define {odname}_ATTR_OD");
            file.WriteLine($"#endif");
            file.WriteLine($"extern {odname}_ATTR_OD OD_t *{odname};");

            file.WriteLine(string.Format(@"

/*******************************************************************************
    Object dictionary entries - shortcuts
*******************************************************************************/
{0}", string.Join("\n", ODDefines)));

            file.WriteLine(string.Format(@"

/*******************************************************************************
    Object dictionary entries - shortcuts with names
*******************************************************************************/
{0}", string.Join("\n", ODDefinesLong)));

            // following code initializes CO_config_t structure as specified in file CANopenNode/CANopen.h
            file.WriteLine($@"

/*******************************************************************************
    OD config structure
*******************************************************************************/
#ifdef CO_MULTIPLE_OD
#define {odname}_INIT_CONFIG(config) {{\
    (config).CNT_NMT = {(ODCnt.ContainsKey("NMT") ? odname + "_CNT_NMT" : "0")};\
    (config).ENTRY_H1017 = {(eds.ods.ContainsKey(0x1017) ? odname + "_ENTRY_H1017" : "NULL")};\
    (config).CNT_HB_CONS = {(ODCnt.ContainsKey("HB_CONS") ? odname + "_CNT_HB_CONS" : "0")};\
    (config).CNT_ARR_1016 = {(eds.ods.ContainsKey(0x1016) ? odname + "_CNT_ARR_1016" : "0")};\
    (config).ENTRY_H1016 = {(eds.ods.ContainsKey(0x1016) ? odname + "_ENTRY_H1016" : "NULL")};\
    (config).CNT_EM = {(ODCnt.ContainsKey("EM") ? odname + "_CNT_EM" : "0")};\
    (config).ENTRY_H1001 = {(eds.ods.ContainsKey(0x1001) ? odname + "_ENTRY_H1001" : "NULL")};\
    (config).ENTRY_H1014 = {(eds.ods.ContainsKey(0x1014) ? odname + "_ENTRY_H1014" : "NULL")};\
    (config).ENTRY_H1015 = {(eds.ods.ContainsKey(0x1015) ? odname + "_ENTRY_H1015" : "NULL")};\
    (config).CNT_ARR_1003 = {(eds.ods.ContainsKey(0x1003) ? odname + "_CNT_ARR_1003" : "0")};\
    (config).ENTRY_H1003 = {(eds.ods.ContainsKey(0x1003) ? odname + "_ENTRY_H1003" : "NULL")};\
    (config).CNT_SDO_SRV = {(ODCnt.ContainsKey("SDO_SRV") ? odname + "_CNT_SDO_SRV" : "0")};\
    (config).ENTRY_H1200 = {(eds.ods.ContainsKey(0x1200) ? odname + "_ENTRY_H1200" : "NULL")};\
    (config).CNT_SDO_CLI = {(ODCnt.ContainsKey("SDO_CLI") ? odname + "_CNT_SDO_CLI" : "0")};\
    (config).ENTRY_H1280 = {(eds.ods.ContainsKey(0x1280) ? odname + "_ENTRY_H1280" : "NULL")};\
    (config).CNT_TIME = {(ODCnt.ContainsKey("TIME") ? odname + "_CNT_TIME" : "0")};\
    (config).ENTRY_H1012 = {(eds.ods.ContainsKey(0x1012) ? odname + "_ENTRY_H1012" : "NULL")};\
    (config).CNT_SYNC = {(ODCnt.ContainsKey("SYNC") ? odname + "_CNT_SYNC" : "0")};\
    (config).ENTRY_H1005 = {(eds.ods.ContainsKey(0x1005) ? odname + "_ENTRY_H1005" : "NULL")};\
    (config).ENTRY_H1006 = {(eds.ods.ContainsKey(0x1006) ? odname + "_ENTRY_H1006" : "NULL")};\
    (config).ENTRY_H1007 = {(eds.ods.ContainsKey(0x1007) ? odname + "_ENTRY_H1007" : "NULL")};\
    (config).ENTRY_H1019 = {(eds.ods.ContainsKey(0x1019) ? odname + "_ENTRY_H1019" : "NULL")};\
    (config).CNT_RPDO = {(ODCnt.ContainsKey("RPDO") ? odname + "_CNT_RPDO" : "0")};\
    (config).ENTRY_H1400 = {(eds.ods.ContainsKey(0x1400) ? odname + "_ENTRY_H1400" : "NULL")};\
    (config).ENTRY_H1600 = {(eds.ods.ContainsKey(0x1600) ? odname + "_ENTRY_H1600" : "NULL")};\
    (config).CNT_TPDO = {(ODCnt.ContainsKey("TPDO") ? odname + "_CNT_TPDO" : "0")};\
    (config).ENTRY_H1800 = {(eds.ods.ContainsKey(0x1800) ? odname + "_ENTRY_H1800" : "NULL")};\
    (config).ENTRY_H1A00 = {(eds.ods.ContainsKey(0x1A00) ? odname + "_ENTRY_H1A00" : "NULL")};\
    (config).CNT_LEDS = 0;\
    (config).CNT_GFC = {(ODCnt.ContainsKey("GFC") ? odname + "_CNT_GFC" : "0")};\
    (config).ENTRY_H1300 = {(eds.ods.ContainsKey(0x1300) ? odname + "_ENTRY_H1300" : "NULL")};\
    (config).CNT_SRDO = {(ODCnt.ContainsKey("SRDO") ? odname + "_CNT_SRDO" : "0")};\
    (config).ENTRY_H1301 = {(eds.ods.ContainsKey(0x1301) ? odname + "_ENTRY_H1301" : "NULL")};\
    (config).ENTRY_H1381 = {(eds.ods.ContainsKey(0x1381) ? odname + "_ENTRY_H1381" : "NULL")};\
    (config).ENTRY_H13FE = {(eds.ods.ContainsKey(0x13FE) ? odname + "_ENTRY_H13FE" : "NULL")};\
    (config).ENTRY_H13FF = {(eds.ods.ContainsKey(0x13FF) ? odname + "_ENTRY_H13FF" : "NULL")};\
    (config).CNT_LSS_SLV = 0;\
    (config).CNT_LSS_MST = 0;\
    (config).CNT_GTWA = 0;\
    (config).CNT_TRACE = 0;\
}}
#endif");

            file.WriteLine(string.Format(@"
#endif /* {0}_H */", odname));

            file.Close();
        }


        /// <summary>
        /// Export the c file
        /// </summary>
        /// <param name="folderpath">path to folder that will contain the file</param>
        /// <param name="filename">filename</param>
        /// <param name="gitVersion">git version that will be added to file comment</param>
        /// <param name="eds">data that contain the data that will be exported</param>
        private void Export_c(string folderpath, string filename, string gitVersion, EDSsharp eds)
            {

            if (filename == "")
                filename = "OD";

            StreamWriter file = new StreamWriter(folderpath + Path.DirectorySeparatorChar + filename + ".c");
            file.NewLine = "\n";

            file.WriteLine(string.Format(
@"/*******************************************************************************
    CANopen Object Dictionary definition for CANopenNode V4

    This file was automatically generated by CANopenEditor {0}

    https://github.com/CANopenNode/CANopenNode
    https://github.com/CANopenNode/CANopenEditor

    DON'T EDIT THIS FILE MANUALLY, UNLESS YOU KNOW WHAT YOU ARE DOING !!!!
*******************************************************************************/

#define OD_DEFINITION
#include ""301/CO_ODinterface.h""
#include ""{1}.h""

#if CO_VERSION_MAJOR < 4
#error This Object dictionary is compatible with CANopenNode V4.0 and above!
#endif", gitVersion, filename));

    file.WriteLine(@"
/*******************************************************************************
    OD data initialization of all groups
*******************************************************************************/");
            foreach (string group in ODStorageGroups)
            {
                if (ODStorage.Count > 0)
                {
                    file.WriteLine($"{odname}_ATTR_{group} {odname}_{group}_t {odname}_{group} = {{");
                    file.WriteLine($"    {string.Join(",\n    ", ODStorage[group])}");
                    file.WriteLine($"}};\n");
                }
            }

            // remove ',' from the last element
            string s = ODObjs[ODObjs.Count - 1];
            ODObjs[ODObjs.Count - 1] = s.Remove(s.Length - 1);

            file.WriteLine(string.Format(@"

/*******************************************************************************
    All OD objects (constant definitions)
*******************************************************************************/
typedef struct {{
    {1}
}} {0}Objs_t;

static CO_PROGMEM {0}Objs_t {0}Objs = {{
{2}
}};", odname, string.Join("\n    ", ODObjs_t), string.Join("\n", ODObjs)));

            file.WriteLine(string.Format(@"

/*******************************************************************************
    Object dictionary
*******************************************************************************/
static {0}_ATTR_OD OD_entry_t {0}List[] = {{
    {1},
    {{0x0000, 0x00, 0, NULL, NULL}}
}};

static OD_t _{0} = {{
    (sizeof({0}List) / sizeof({0}List[0])) - 1,
    &{0}List[0]
}};

OD_t *{0} = &_{0};", odname, string.Join(",\n    ", ODList)));

            file.Close();
        }
        #endregion

        #region helper_functions

        /// <summary>
        /// Take a paramater name from the object dictionary and make it acceptable
        /// for use in c variables/structs etc
        /// </summary>
        /// <param name="name">string, name to convert</param>
        /// <returns>string</returns>
        protected static string Make_cname(string name)
        {
            if (name == null || name == "")
                return "";

            // split string to tokens, separated by non-word characters. Remove any empty strings
            var tokens = Regex.Split(name.Replace('-', '_'), @"[\W]+").Where(s => s != String.Empty);

            string output = "";
            char prev = ' ';
            foreach (string tok in tokens)
            {
                char first = tok[0];

                if (Char.IsUpper(prev) && Char.IsUpper(first))
                {
                    // add underscore, if we have two upper-case words
                    output += "_";
                }

                if (tok.Length > 1 && Char.IsLetter(first))
                {
                    // all tokens except the first start with uppercase letter
                    output += Char.ToUpper(first) + tok.Substring(1);
                }
                else
                {
                    // use token as is and handle what the start of the output looks like outside of the loop 
                    output += tok;
                }

                prev = tok[tok.Length - 1];
            }

            if (Char.IsDigit(output[0]))
            {
                // output that starts with a digit needs a starting underscore 
                output = "_" + output;
            }
            else if (output.Length > 1)
            {
                // output that doesnt start with all-cap-words should have word start with a lower case character
                if (Char.IsLetter(output[0]) && Char.IsLower(output[1]))
                    output = Char.ToLowerInvariant(output[0]) + output.Substring(1);
            }
            else
            {
                // single character output
                output = output.ToLowerInvariant();
            }

            return output;
        }

        /// <summary>
        /// Return from Get_dataProperties
        /// </summary>
        private class DataProperties
        {
            public string cType = "not specified";
            public string cTypeArray = "";
            public string cTypeArray0 = "";
            public bool cTypeMultibyte = false;
            public bool cTypeString = false;
            public UInt32 length = 0;
            public string cValue = null;
        }

        /// <summary>
        /// Get the correct c data type, length and default value, based on CANopen data type
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="defaultvalue"></param>
        /// <param name="stringLength"></param>
        /// <param name="indexH"></param>
        /// <returns>Structure filled with data</returns>
        private DataProperties Get_dataProperties(DataType dataType, string defaultvalue, UInt32 stringLength, string indexH)
        {
            DataProperties data = new DataProperties();
            int nobase = 10;
            bool valueDefined = true;
            if (defaultvalue == null || defaultvalue == "")
                valueDefined = false;
            else if (dataType != DataType.VISIBLE_STRING && dataType != DataType.UNICODE_STRING && dataType != DataType.OCTET_STRING)
            {
                defaultvalue = defaultvalue.Trim();       

                if (defaultvalue.Contains("$NODEID",StringComparison.OrdinalIgnoreCase)) // fetch different case of "NODeID" (allowed according DS301)
                {
                    defaultvalue = defaultvalue.ToUpper().Replace("$NODEID", "");
                    defaultvalue = defaultvalue.Replace("+", "");
                    defaultvalue = defaultvalue.Trim();
                    if (defaultvalue == "")
                        defaultvalue = "0";
                }

                String pat = @"^0[xX][0-9a-fA-FUL]+";
                Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                Match m = r.Match(defaultvalue);
                if (m.Success)
                {
                    nobase = 16;
                    defaultvalue = defaultvalue.Replace("U", "");
                    defaultvalue = defaultvalue.Replace("L", "");
                }

                pat = @"^0[0-7]+";
                r = new Regex(pat, RegexOptions.IgnoreCase);
                m = r.Match(defaultvalue);
                if (m.Success)
                {
                    nobase = 8;
                }
            }

            try
            {
                bool signedNumber = false;
                bool unsignedNumber = false;

                switch (dataType)
                {
                    case DataType.BOOLEAN:
                        data.length = 1;
                        data.cType = "bool_t";
                        if (valueDefined)
                        {
                            data.cValue = (defaultvalue.ToLower() == "false" || defaultvalue == "0") ? "false" : "true";
                        }
                        break;
                    case DataType.INTEGER8:
                        data.length = 1;
                        data.cType = "int8_t";
                        if (valueDefined)
                        {
                            data.cValue = $"{Convert.ToSByte(defaultvalue, nobase)}";
                        }
                        break;
                    case DataType.INTEGER16:
                        data.length = 2;
                        data.cType = "int16_t";
                        data.cTypeMultibyte = true;
                        if (valueDefined)
                        {
                            data.cValue = $"{Convert.ToInt16(defaultvalue, nobase)}";
                        }
                        break;
                    case DataType.INTEGER32:
                        data.length = 4;
                        data.cType = "int32_t";
                        data.cTypeMultibyte = true;
                        if (valueDefined)
                        {
                            data.cValue = $"{Convert.ToInt32(defaultvalue, nobase)}";
                        }
                        break;
                    case DataType.INTEGER64:
                        data.length = 8;
                        data.cType = "int64_t";
                        data.cTypeMultibyte = true;
                        if (valueDefined)
                        {
                            data.cValue = $"{Convert.ToInt64(defaultvalue, nobase)}";
                        }
                        break;

                    case DataType.UNSIGNED8:
                        data.length = 1;
                        data.cType = "uint8_t";
                        if (valueDefined)
                        {
                            data.cValue = String.Format("0x{0:X2}", Convert.ToByte(defaultvalue, nobase));
                        }
                        break;
                    case DataType.UNSIGNED16:
                        data.length = 2;
                        data.cType = "uint16_t";
                        data.cTypeMultibyte = true;
                        if (valueDefined)
                        {
                            data.cValue = String.Format("0x{0:X4}", Convert.ToUInt16(defaultvalue, nobase));
                        }
                        break;
                    case DataType.UNSIGNED32:
                        data.length = 4;
                        data.cType = "uint32_t";
                        data.cTypeMultibyte = true;
                        if (valueDefined)
                        {
                            data.cValue = String.Format("0x{0:X8}", Convert.ToUInt32(defaultvalue, nobase));
                        }
                        break;
                    case DataType.UNSIGNED64:
                        data.length = 8;
                        data.cType = "uint64_t";
                        data.cTypeMultibyte = true;
                        if (valueDefined)
                        {
                            data.cValue = String.Format("0x{0:X16}", Convert.ToUInt64(defaultvalue, nobase));
                        }
                        break;

                    case DataType.REAL32:
                        data.length = 4;
                        data.cType = "float32_t";
                        data.cTypeMultibyte = true;
                        if (valueDefined)
                        {
                            data.cValue = defaultvalue;
                        }
                        break;
                    case DataType.REAL64:
                        data.length = 8;
                        data.cType = "float64_t";
                        data.cTypeMultibyte = true;
                        if (valueDefined)
                        {
                            data.cValue = defaultvalue;
                        }
                        break;

                    case DataType.DOMAIN:
                        // keep default values (0 and null)
                        break;

                    case DataType.VISIBLE_STRING:
                        data.cTypeString = true;
                        if (valueDefined || stringLength > 0)
                        {
                            List<string> chars = new List<string>();
                            UInt32 len = 0;

                            if (valueDefined)
                            {
                                UTF8Encoding utf8 = new UTF8Encoding();
                                Byte[] encodedBytes = utf8.GetBytes(defaultvalue);
                                foreach (Byte b in encodedBytes)
                                {
                                    if ((char)b == '\'')
                                        chars.Add("'\\''");
                                    else if (b >= 0x20 && b < 0x7F)
                                        chars.Add($"'{(char)b}'");
                                    else if (b <= 0x7F)
                                        chars.Add($"0x{b:X2}");
                                    else
                                        chars.Add($"(char)0x{b:X2}");
                                    len++;
                                }
                            }
                            /* fill unused bytes with nulls */
                            for (; len < stringLength; len++)
                            {
                                chars.Add("0");
                            }

                            // extra string terminator
                            chars.Add("0");

                            data.length = len;
                            data.cType = "char";
                            data.cTypeArray = $"[{len + 1}]";
                            data.cTypeArray0 = "[0]";
                            data.cValue = $"{{{string.Join(", ", chars)}}}";
                        }
                        break;

                    case DataType.OCTET_STRING:
                        defaultvalue = defaultvalue.Trim();
                        if (defaultvalue == "")
                            valueDefined = false;
                        if (valueDefined || stringLength > 0)
                        {
                            List<string> bytes = new List<string>();
                            UInt32 len = 0;

                            if (valueDefined)
                            {
                                string[] strBytes = defaultvalue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string s in strBytes)
                                {
                                    bytes.Add(String.Format("0x{0:X2}", Convert.ToByte(s, 16)));
                                    len++;
                                }
                            }
                            for (; len < stringLength; len++)
                            {
                                bytes.Add("0x00");
                            }

                            data.length = len;
                            data.cType = "uint8_t";
                            data.cTypeArray = $"[{len}]";
                            data.cTypeArray0 = "[0]";
                            data.cValue = $"{{{string.Join(", ", bytes)}}}";
                        }
                        break;
                    case DataType.UNICODE_STRING:
                        data.cTypeString = true;
                        if (valueDefined || stringLength > 0)
                        {
                            List<string> words = new List<string>();
                            UInt32 len = 0;

                            if (valueDefined)
                            {
                                Encoding unicode = Encoding.Unicode;
                                Byte[] encodedBytes = unicode.GetBytes(defaultvalue);
                                for (UInt32 i = 0; i < encodedBytes.Length; i += 2)
                                {
                                    UInt16 val = (ushort)(encodedBytes[i] | (UInt16)encodedBytes[i+1] << 8);
                                    words.Add(String.Format("0x{0:X4}", val));
                                    len++;
                                }
                            }
                            for (; len < stringLength; len++)
                            {
                                words.Add("0x0000");
                            }

                            // extra string terminator
                            words.Add("0x0000");

                            data.length = len * 2;
                            data.cType = "uint16_t";
                            data.cTypeArray = $"[{len + 1}]";
                            data.cTypeArray0 = "[0]";
                            data.cValue = $"{{{string.Join(", ", words)}}}";
                        }
                        break;

                    case DataType.INTEGER24:
                        data.length = 3;
                        signedNumber = true;
                        break;
                    case DataType.INTEGER40:
                        data.length = 5;
                        signedNumber = true;
                        break;
                    case DataType.INTEGER48:
                        data.length = 6;
                        signedNumber = true;
                        break;
                    case DataType.INTEGER56:
                        data.length = 7;
                        signedNumber = true;
                        break;

                    case DataType.UNSIGNED24:
                        data.length = 3;
                        unsignedNumber = true;
                        break;
                    case DataType.UNSIGNED40:
                        data.length = 5;
                        unsignedNumber = true;
                        break;
                    case DataType.UNSIGNED48:
                    case DataType.TIME_OF_DAY:
                    case DataType.TIME_DIFFERENCE:
                        data.length = 6;
                        unsignedNumber = true;
                        break;
                    case DataType.UNSIGNED56:
                        data.length = 7;
                        unsignedNumber = true;
                        break;

                    default:
                        Warnings.AddWarning($"Error in 0x{indexH}: Unknown dataType: {dataType}", Warnings.warning_class.WARNING_BUILD);
                        break;
                }

                if (valueDefined && (signedNumber || unsignedNumber))
                {
                    // write default value as a sequence of bytes, like "{0x56, 0x34, 0x12}"
                    ulong value = signedNumber ? (ulong)Convert.ToInt64(defaultvalue, nobase) : Convert.ToUInt64(defaultvalue, nobase);
                    List<string> bytes = new List<string>();
                    for (UInt32 i = 0; i < data.length; i++)
                    {
                        bytes.Add(String.Format("0x{0:X2}", value & 0xFF));
                        value >>= 8;
                    }
                    if (value > 0)
                        Warnings.AddWarning($"Error in 0x{indexH}: Overflow error in default value {defaultvalue} of type {dataType}", Warnings.warning_class.WARNING_BUILD);
                    else
                    {
                        data.cType = "uint8_t";
                        data.cTypeArray = $"[{data.length}]";
                        data.cTypeArray0 = "[0]";
                        data.cValue = $"{{{string.Join(", ", bytes)}}}";
                    }
                }
            }
            catch (Exception)
            {
                Warnings.AddWarning($"Error in 0x{indexH}: Error converting default value {defaultvalue} to type {dataType}", Warnings.warning_class.WARNING_BUILD);
            }

            return data;
        }

        /// <summary>
        /// Get attributes from OD entry or sub-entry
        /// </summary>
        /// <param name="od"></param>
        /// <param name="cTypeMultibyte"></param>
        /// <param name="cTypeString"></param>
        /// <returns></returns>
        private string Get_attributes(ODentry od, bool cTypeMultibyte, bool cTypeString)
        {
            List<string> attributes = new List<string>();

            switch (od.AccessSDO())
            {
                case AccessSDO.ro:
                    attributes.Add("ODA_SDO_R");
                    break;
                case AccessSDO.wo:
                    attributes.Add("ODA_SDO_W");
                    break;
                case AccessSDO.rw:
                    attributes.Add("ODA_SDO_RW");
                    break;
            }

            switch (od.AccessPDO())
            {
                case AccessPDO.r:
                    attributes.Add("ODA_RPDO");
                    break;
                case AccessPDO.t:
                    attributes.Add("ODA_TPDO");
                    break;
                case AccessPDO.tr:
                    attributes.Add("ODA_TRPDO");
                    break;
            }

            switch (od.prop.CO_accessSRDO)
            {
                case AccessSRDO.rx:
                    attributes.Add("ODA_RSRDO");
                    break;
                case AccessSRDO.tx:
                    attributes.Add("ODA_TSRDO");
                    break;
                case AccessSRDO.trx:
                    attributes.Add("ODA_TRSRDO");
                    break;
            }

            if (cTypeMultibyte)
                attributes.Add("ODA_MB");

            if (cTypeString)
                attributes.Add("ODA_STR");

            if (attributes.Count == 0) attributes.Add("0");

            return string.Join(" | ", attributes);
        }
        #endregion
    }
}
