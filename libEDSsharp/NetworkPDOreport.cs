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
using System.IO;

namespace libEDSsharp
{
    /// <summary>
    /// Generate a PDO network report
    /// </summary>
    public class NetworkPDOreport : IFileExporter
    {

        public ExporterDescriptor[] GetExporters()
        {
            return new ExporterDescriptor[]
            {
                new ExporterDescriptor("Network PDO Report", new string[] { ".md" }, ExporterDescriptor.ExporterFlags.Documentation | ExporterDescriptor.ExporterFlags.MultipleNodeSupport, delegate (string filepath, List<EDSsharp> edss)
                {
                    var e = new NetworkPDOreport();
                    e.gennetpdodoc(filepath, edss);
                })
            };
        }
        StreamWriter file = null;
        /// <summary>
        /// Generate a PDO network report
        /// </summary>
        /// <param name="filepath">where the doc should be saved</param>
        /// <param name="network">Data from the different nodes in the network</param>
        public void gennetpdodoc(string filepath, List<EDSsharp> network)
        {

            file = new StreamWriter(filepath, false);
            file.NewLine = "\n";

            file.Write("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\" /> <title>Network PDO report</title></head><body>");

            file.Write(string.Format("<h1>PDO Network Documentation </h1>"));

            file.Write("<table id=\"nodelist\">");

            write2linetableheader("Node ID", "Name");

            foreach (EDSsharp eds in network)
            {
                write2linetablerow(eds.dc.NodeID.ToString(), eds.di.ProductName);
            }

            file.Write("</table>");


            file.Write(string.Format("<h1>PDO Map</h1>"));

            //now for each node find each TX PDO

            foreach (EDSsharp eds in network)
            {

                foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
                {
                    //find the com params for each TX pdo
                    if (kvp.Key >= 0x1800 && kvp.Key < 0x1a00)
                    {

                        //check the mapping also exists
                        if (eds.ods.ContainsKey((UInt16)(kvp.Key + 0x200)))
                        {
                            ODentry map = eds.ods[(UInt16)(kvp.Key + 0x200)];
                            ODentry compar = eds.ods[(UInt16)(kvp.Key)];

                            //Fix me we need some generic PDO access functions

                            UInt32 TXCOB = 0;
                            byte syncstart = 0;
                            UInt16 timer = 0;
                            UInt16 inhibit = 0;
                            byte type = 0;

                            bool nodeidpresent;

                            if (kvp.Value.Containssubindex(1))
                                TXCOB = eds.GetNodeID(kvp.Value.Getsubobject(1).defaultvalue, out nodeidpresent);

                            if (kvp.Value.Containssubindex(2))
                                type = EDSsharp.ConvertToByte(kvp.Value.Getsubobject(2).defaultvalue);

                            if (kvp.Value.Containssubindex(3))
                                inhibit = EDSsharp.ConvertToUInt16(kvp.Value.Getsubobject(3).defaultvalue);

                            if (kvp.Value.Containssubindex(5))
                                timer = EDSsharp.ConvertToUInt16(kvp.Value.Getsubobject(5).defaultvalue);

                            if (kvp.Value.Containssubindex(6))
                                syncstart = EDSsharp.ConvertToByte(kvp.Value.Getsubobject(6).defaultvalue);


                            byte totalsize = 0;
                            for (UInt16 sub = 1; sub <= map.Getmaxsubindex(); sub++)
                            {
                                if (!map.Containssubindex(sub))
                                    continue;

                                UInt32 mapping = EDSsharp.ConvertToUInt32(map.Getsubobject(sub).defaultvalue);
                                if (mapping == 0)
                                    continue;

                                Byte size = (byte)mapping;
                                totalsize += size;

                            }

                            if (totalsize != 0)
                            {
                                file.Write(string.Format("<h2> PDO 0x{0:x3} <h2>", TXCOB));
                                file.Write("<table><tr> <th>Parameter</th> <th>value</th> </tr>");
                                file.Write(string.Format("<tr><td>{0}</td><td>0x{1:x3}</td></tr>", "COB", TXCOB));
                                file.Write(string.Format("<tr><td>{0}</td><td>0x{1:x}</td></tr>", "Type", type));
                                file.Write(string.Format("<tr><td>{0}</td><td>0x{1:x} ({2})</td></tr>", "Inhibit", inhibit, inhibit));
                                file.Write(string.Format("<tr><td>{0}</td><td>0x{1:x} ({2})</td></tr>", "Event timer", timer, timer));
                                file.Write(string.Format("<tr><td>{0}</td><td>0x{1:x} ({2})</td></tr>", "Sync start", syncstart, syncstart));
                                file.Write(string.Format("<tr><td>{0}</td><td>0x{1:x}</td></tr>", "PDO Size (Bytes)", totalsize / 8));


                                file.Write("<table class=\"pdomap\">");
                                file.Write(string.Format("<tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th><th>{5}</th></tr>", "Node", "Dev name", "OD Index", "Name", "Size", "Receivers"));

                            }



                            byte offsetend = 0;
                            byte offsetstart = 0;
                            for (UInt16 sub = 1; sub <= map.Getmaxsubindex(); sub++)
                            {

                                if (!map.Containssubindex(sub))
                                    continue;

                                if (map.Getsubobjectdefaultvalue(sub) == "")
                                    continue;

                                UInt32 mapping = EDSsharp.ConvertToUInt32(map.Getsubobjectdefaultvalue(sub));
                                if (mapping == 0)
                                    continue;

                                //its real extract the OD and sub index
                                UInt16 index = (UInt16)(mapping >> 16);
                                UInt16 subindex = (UInt16)(0x00FF & (mapping >> 8));
                                Byte size = (byte)mapping;

                                String name;

                                if (!eds.ods.ContainsKey(index))
                                    break;

                                if (subindex == 0)
                                {
                                    name = eds.ods[index].parameter_name;

                                }
                                else
                                {
                                    name = eds.ods[index].Getsubobject(subindex).parameter_name;
                                }


                                file.Write(string.Format("<tr> <td>0x{0:x2}</td> <td>{1}</td> <td>0x{2:x4}/0x{3:x2}</td><td>{4}</td> <td>{5}</td><td>", eds.dc.NodeID, eds.di.ProductName, index, subindex, name, size));


                                //find all receivers here

                                file.Write("<table class=\"receivers\">");

                                file.Write(string.Format("<tr> <th>Node</th> <th>Dev Name</th> <th>OD Index</th> <th>Name</th> <th>Size</td></tr>"));

                                offsetend += (byte)(size / 8);

                                foreach (EDSsharp eds2 in network)
                                {

                                    if (eds == eds2)
                                        continue;
                                    else
                                    {

                                        foreach (KeyValuePair<UInt16, ODentry> kvp2 in eds2.ods)
                                        {
                                            if (kvp2.Key >= 0x1400 && kvp2.Key < 0x1600)
                                            {
                                                if (eds2.ods.ContainsKey((UInt16)(kvp2.Key + 0x200)))
                                                {
                                                    bool nodeidpresent2;
                                                    UInt32 RXCOB = eds2.GetNodeID(kvp2.Value.Getsubobjectdefaultvalue(1), out nodeidpresent2);
                                                    if (RXCOB == TXCOB)
                                                    {

                                                        ODentry map2 = eds2.ods[(UInt16)(kvp2.Key + 0x200)];

                                                        //Get the actual subindex to use

                                                        byte offsetstart2 = 0;
                                                        byte offsetend2 = 0;

                                                        Byte size2 = 0;
                                                        UInt32 mapping2 = 0;
                                                        UInt16 index2 = 0;
                                                        UInt16 subindex2 = 0;

                                                        byte totalsize2 = 0;

                                                        //Sanity check the total size

                                                        for (byte sub2 = 1; sub2 <= map2.Getmaxsubindex(); sub2++)
                                                        {
                                                            mapping2 = EDSsharp.ConvertToUInt32(map2.Getsubobjectdefaultvalue(sub2));
                                                            size2 = (byte)(mapping2);
                                                            totalsize2 += size2;
                                                        }
                                                        if (totalsize2 != totalsize)
                                                        {
                                                            file.WriteLine("<B> Critical error with network RX PDO size != TX PDO SIZE");
                                                        }

                                                        for (byte sub2 = 1; sub2 <= map2.Getmaxsubindex(); sub2++)
                                                        {
                                                            mapping2 = EDSsharp.ConvertToUInt32(map2.Getsubobjectdefaultvalue(sub2));
                                                            index2 = (UInt16)(mapping2 >> 16);
                                                            subindex2 = (UInt16)(0x00FF & (mapping2 >> 8));
                                                            size2 = (byte)mapping2;

                                                            if (mapping2 == 0)
                                                                continue;

                                                            offsetend2 += (byte)(size2 / 8);


                                                            // if(offsetstart == offsetstart2 && offsetend == offsetend2)
                                                            //we are all good equal data 1:1 mapping

                                                            if (offsetstart2 < offsetstart)
                                                            {
                                                                //more data needed to reach start
                                                                offsetstart2 += (byte)(size2 / 8);
                                                                continue;
                                                            }

                                                            if (offsetend2 > offsetend && offsetstart2 > offsetstart)
                                                                break; //we are done

                                                            offsetstart2 += (byte)(size2 / 8);

                                                            if (offsetend2 > offsetend)
                                                            {
                                                                //merge cell required on parent table
                                                                //meh difficult to do from here
                                                            }


                                                            String name2;

                                                            if (subindex2 == 0)
                                                            {
                                                                //fixme getobject could return null
                                                                name2 = eds2.Getobject(index2).parameter_name;

                                                            }
                                                            else
                                                            {
                                                                //fixme getobject could return null
                                                                name2 = eds2.Getobject(index2).Getsubobject(subindex2).parameter_name;
                                                            }

                                                            string sizemsg = "";

                                                            if (size != size2)
                                                            {
                                                                sizemsg = " <b>WARNING</b>";
                                                            }


                                                            file.Write(string.Format("<tr> <td>0x{0:x2}</td> <td>{1}</td> <td>0x{2:x4}/0x{3:x2}</td> <td>{4}</td><td>{5}{6}</td></tr>", eds2.dc.NodeID, eds2.di.ProductName, index2, subindex2, name2, size2, sizemsg));

                                                        }


                                                    }
                                                }
                                            }
                                        }

                                      
                                    }
                                }

                                offsetstart += (byte)(size / 8);

                                file.Write("</table>");


                                file.Write("</td>");

                                file.Write(string.Format("</tr>"));

                            }

                        }

                        file.Write("</table>");

                    }

                }


            }


            file.Close();

        }

        /// <summary>
        /// Write a html table row with 2 elements to file
        /// </summary>
        /// <param name="a">element a</param>
        /// <param name="b">element b</param>
        public void write2linetablerow(string a, object b)
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
            file.Write("<tr><th>{0}</th><th>{1}</th></tr>", a, b.ToString());
        }

    }
}

