using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libEDSsharp;

namespace EDSSharp
{
    class Program
    {

        static libEDSsharp.EDSsharp eds = new EDSsharp();

        static void Main(string[] args)
        {
            try
            {

                Dictionary<string, string> argskvp = new Dictionary<string, string>();

                int argv = 0;

                for (argv = 0; argv < (args.Length - 1); argv++)
                {
                    if (args[argv] == "--infile")
                    {
                        argskvp.Add("--infile", args[argv + 1]);
                    }

                    if (args[argv] == "--outfile")
                    {
                        argskvp.Add("--outfile", args[argv + 1]);
                    }

                    if (args[argv] == "--type")
                    {
                        argskvp.Add("--type", args[argv + 1]);
                    }

                    argv++;
                }


                if (argskvp.ContainsKey("--infile") && argskvp.ContainsKey("--outfile"))
                {
                    string infile = argskvp["--infile"];
                    string outfile = argskvp["--outfile"];
                    string outtype = "";
                    if (argskvp.ContainsKey("--type"))
                    {
                        outtype = argskvp["--type"];
                    }


                    switch (Path.GetExtension(infile).ToLower())
                    {
                        case ".xdd":
                            openXDDfile(infile);
                            break;

                        case ".eds":
                            openEDSfile(infile);
                            break;


                        default:
                            return;

                    }
                    if(eds != null)
                    {
                        Export(outfile, outtype);
                    }
                }
                else
                {
                    PrintHelpText();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                PrintHelpText();
            }
        }

        private static void openEDSfile(string infile)
        {
          
            eds.Loadfile(infile);
        }

        private static void openXDDfile(string path)
        {
            CanOpenXDD_1_1 coxml_1_1 = new CanOpenXDD_1_1();
            eds = coxml_1_1.ReadXML(path);

            if (eds == null)
            {
                CanOpenXDD coxml = new CanOpenXDD();
                eds = coxml.readXML(path);

                if (eds == null)
                    return;
            }

            eds.projectFilename = path;
        }

        private static void Export(string outpath, string outType)
        {
            outpath = Path.GetFullPath(outpath);

            string savePath = Path.GetDirectoryName(outpath);

            eds.fi.exportFolder = savePath;

            Warnings.warning_list.Clear();

            var exporterDef = FindMatchingExporter(outpath, outType);

            if(exporterDef == null)
            {
                throw new Exception("Unable to find matching exporter)");
            }

            var edss = new List<EDSsharp> { eds };
            exporterDef.Func(outpath, edss);

            foreach(string warning in Warnings.warning_list)
            {
                Console.WriteLine("WARNING :" + warning);
            }

        }

        static ExporterDescriptor FindMatchingExporter(string outpath, string outType)
        {
            //Find exporter(s) matching the file extension
            var exporters = Filetypes.GetExporters();

            var outFiletype = Path.GetExtension(outpath);
            var exporterMatchingFiletype = new List<ExporterDescriptor>();
            foreach (var exporter in exporters)
            {
                foreach (var type in exporter.Filetypes)
                {
                    if (type == outFiletype)
                    {
                        exporterMatchingFiletype.Add(exporter);
                        break;
                    }
                }
            }

            if (exporterMatchingFiletype.Count == 1)
            {
                //If only one match we use that one.
                return exporterMatchingFiletype[0];
            }

            //If multiple or zero matches use type
            foreach (var exporter in exporters)
            {
                if (exporter.Description.Replace(" ", null) == outType)
                {
                    return exporter;
                }
            }
            return null;
        }

        static void PrintHelpText()
        {
            Console.WriteLine("Usage: EDSEditor --infile file.[xdd|eds] --outfile [valid output file] [OPTIONAL] --type [exporter type]");
            Console.WriteLine("The output file format depends on --outfile extension and --type");
            Console.WriteLine("If --outfile extension matcher one exporter then --type IS NOT needed");
            Console.WriteLine("If --outfile extension matcher multiple exporter then --type IS needed");
            Console.WriteLine("If --outfile has no extension --type IS needed");
            Console.WriteLine("Exporter types:");

            var exporters = Filetypes.GetExporters();
            foreach (var exporter in exporters)
            {
                string filetypes = "";
                for (int i = 0; i < exporter.Filetypes.Length; i++)
                {
                    filetypes += exporter.Filetypes[i];
                    //add seperator char if multiple filetypes
                    if(i +1 != exporter.Filetypes.Length)
                    {
                        filetypes += ',';
                    }
                }

                string description = $"  {exporter.Description.Replace(" ",null)} [{filetypes}]";
                Console.WriteLine(description);
            }
        }
    }
}
