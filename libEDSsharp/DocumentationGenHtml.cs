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
    Copyright(c) 2020 Janez Paternoster
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace libEDSsharp
{
    /// <summary>
    /// Documentation generator
    /// </summary>
    public class DocumentationGenHtml : IFileExporter
    {
        StreamWriter file = null;

        /// <summary>
        /// Fetches all the different fileexporter types the class supports
        /// </summary>
        /// <returns>List of the different exporters the class supports</returns>
        public ExporterDescriptor[] GetExporters()
        {
            return new ExporterDescriptor[]
            {
                new ExporterDescriptor("Documentation HTML", new string[] { ".html" }, ExporterDescriptor.ExporterFlags.Documentation, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new DocumentationGenMarkup();
                    e.genmddoc(filepath, edss[0]);
                })
            };
        }
        /// <summary>
        /// Generate html documentation
        /// </summary>
        /// <param name="filepath">where the documentation should be saved</param>
        /// <param name="eds">data to generate the documentation from</param>
        public void genhtmldoc(string filepath, EDSsharp eds)
        {

            file = new StreamWriter(filepath, false);
            file.NewLine = "\n";

           file.Write("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\" /></head><body>");

           file.Write(string.Format("<h1> {0} Documentation </h1>",eds.di.ProductName));

           file.Write("<h2>Device Information</h2>");

           file.Write("<table id=\"deviceinfo\">");
           write2linetableheader("Product name", eds.di.ProductName);
           write2linetableheader("Product number", eds.di.ProductNumber);
           write2linetableheader("Revision number", eds.di.RevisionNumber);
           write2linetableheader("Vendor name", eds.di.VendorName);
           file.Write("</table>");

           file.Write("<h2>Mandatory objects</h2>");

           foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
           {
                ODentry od = kvp.Value;
                if (od.prop.CO_disabled == true)
                    continue;

                if (od.Index == 0x1000 || od.Index == 0x1001 || od.Index == 0x1018)
                {
                    writeODentryhtml(od);
                }
            }

            file.Write("<h2>Optional objects</h2>");

            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;
                if (od.prop.CO_disabled == true)
                    continue;

                if ((od.Index > 0x1001 && od.Index != 0x1018 && od.Index<0x2000) || od.Index>=0x6000)
                {
                    writeODentryhtml(od);
                }
            }

            file.Write("<h2>Manufacturer specific objects</h2>");

            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;
                if (od.prop.CO_disabled == true)
                    continue;

                if (od.Index >= 0x2000 && od.Index<0x6000)
                {
                    writeODentryhtml(od);
                }
            }


            file.Write("</body></html>");

           file.Close();


        }
        /// <summary>
        /// Write a object dictionary html entry to file
        /// </summary>
        /// <param name="od">Object dictionary entry</param>
        void writeODentryhtml(ODentry od)
        {
            if (od.parent == null)
            {
                file.Write("<hr/>");
                file.Write(String.Format("<h3>0x{0:x4} - {1}</h3>", od.Index, od.parameter_name));
            }
            else
            {
                file.Write(String.Format("<h3>0x{0:x4} sub 0x{2:x2} - {1}</h3>", od.Index, od.parameter_name,od.Subindex));
            }

            file.Write("<table id=\"odentry\">");
            write2linetableheader("Parameter", "Value");

            ObjectType ot = od.objecttype;
            if (ot == ObjectType.UNKNOWN && od.parent != null)
                ot = od.parent.objecttype;

            write2linetablerow("Object Type", ot.ToString());

            DataType dt = od.datatype;
            if (dt == DataType.UNKNOWN && od.parent != null)
                dt = od.parent.datatype;

            write2linetablerow("Data Type", dt.ToString());
            write2linetablerow("Default Value", od.defaultvalue);

            write2linetablerow("Location", od.prop.CO_storageGroup);
            write2linetablerow("Access type", od.accesstype.ToString());
            write2linetablerow("PDO mapping", od.PDOMapping);
            write2linetablerow("No Sub index", od.Nosubindexes);

            file.Write("</table>");

            string description = od.Description;
            file.Write(string.Format("<pre>{0}</pre>", description));

            foreach (KeyValuePair<UInt16,ODentry> sub in od.subobjects)
            {
                ODentry subod = sub.Value;
                writeODentryhtml(subod);
            }

        }
        /// <summary>
        /// Write a html table row with 2 elements to file
        /// </summary>
        /// <param name="a">element a</param>
        /// <param name="b">element b</param>
        void write2linetablerow(string a,object b)
        {
            if (b == null)
                b = "";
            file.Write("<tr><td>{0}</td><td>{1}</td></tr>", a, b.ToString());
        }
        /// <summary>
        /// Write a html table header with 2 elements to file
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void write2linetableheader(string a, object b)
        {
            file.Write("<tr><th>{0}</th><th>{1}</th></tr>",a,b.ToString());
        }
        /// <summary>
        /// Returns the datatype of a object dictionary
        /// </summary>
        /// <param name="od">the object dictionary entry</param>
        /// <returns>datatype of the OD entry</returns>
        string PrintDataType(ODentry od)
        {
            string dt = od.datatype.ToString();
            if ((od.datatype == DataType.VISIBLE_STRING || od.datatype == DataType.UNICODE_STRING)
                && od.prop.CO_stringLengthMin > od.defaultvalue.Length)
            {
                dt += $" (len={od.prop.CO_stringLengthMin})";
            }

            return dt;
        }
    }
}
