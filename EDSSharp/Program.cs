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
                Dictionary<string, string> argskvp = ParseArgs(args);

                if (argskvp.ContainsKey("--export-project"))
                {
                    ExportProject(argskvp);
                    return;
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

                    eds = LoadProject(infile);
                    if (eds != null)
                    {
                        Export(outfile, outtype);
                        Console.WriteLine("Successful conversion");
                    }
                    else
                    {
                        Program.WriteError("Invalid INFILE.");
                        PrintHelpText();
                    }
                }
                else
                {
                    Program.WriteError("INFILE or OUTFILE missing.");
                    PrintHelpText();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("");
                Program.WriteError("Invalid EDS INFILE.");
                PrintHelpText();
            }
        }

        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<string, string> argskvp = new Dictionary<string, string>();

            for (int argv = 0; argv < args.Length; argv++)
            {
                if (!args[argv].StartsWith("--"))
                {
                    continue;
                }

                if (args[argv] == "--export-project")
                {
                    argskvp.Add("--export-project", "true");
                    continue;
                }

                if (argv + 1 >= args.Length || args[argv + 1].StartsWith("--"))
                {
                    continue;
                }

                argskvp.Add(args[argv], args[++argv]);
            }

            return argskvp;
        }

        private static void ExportProject(Dictionary<string, string> argskvp)
        {
            if (!argskvp.ContainsKey("--infile") || !argskvp.ContainsKey("--outdir"))
            {
                Program.WriteError("INFILE or OUTDIR missing.");
                PrintExportProjectHelpText();
                return;
            }

            string infile = Path.GetFullPath(argskvp["--infile"]);
            string outdir = Path.GetFullPath(argskvp["--outdir"]);
            Directory.CreateDirectory(outdir);

            eds = LoadProject(infile);
            if (eds == null)
            {
                Program.WriteError("Invalid project INFILE.");
                PrintExportProjectHelpText();
                return;
            }

            eds.fi.exportFolder = outdir;

            string basename = argskvp.ContainsKey("--od")
                ? argskvp["--od"]
                : Path.GetFileNameWithoutExtension(infile);

            string canopennodeVersion = "v4";
            if (argskvp.ContainsKey("--canopennode"))
            {
                canopennodeVersion = argskvp["--canopennode"].ToLower();
            }

            if (canopennodeVersion != "v4" && canopennodeVersion != "legacy")
            {
                Program.WriteError("Invalid --canopennode value. Use 'v4' or 'legacy'.");
                PrintExportProjectHelpText();
                return;
            }

            ExporterFactory.Exporter cnType = canopennodeVersion == "legacy"
                ? ExporterFactory.Exporter.CANOPENNODE_LEGACY
                : ExporterFactory.Exporter.CANOPENNODE_V4;

            string odBasePath = Path.Combine(outdir, basename);
            string jsonPath = argskvp.ContainsKey("--json")
                ? Path.GetFullPath(argskvp["--json"])
                : Path.Combine(outdir, basename + ".json");

            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));

            Warnings.warning_list.Clear();

            ExporterFactory.getExporter(cnType).export(odBasePath, eds);
            Export(jsonPath, "CanOpenNodeProtobuf(json)");

            Console.WriteLine("Successful export");
        }

        private static EDSsharp LoadProject(string infile)
        {
            switch (Path.GetExtension(infile).ToLower())
            {
                case ".xdd":
                case ".xdc":
                case ".xpd":
                    return openXDDfile(infile);

                case ".eds":
                case ".dcf":
                    return openEDSfile(infile);

                case ".json":
                    return openProtobuffile(infile, true);

                case ".binpb":
                    return openProtobuffile(infile, false);

                default:
                    Program.WriteError("Invalid INFILE extension.");
                    return null;
            }
        }

        private static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ResetColor();
            Console.WriteLine("");
        }

        private static EDSsharp openEDSfile(string infile)
        {
            var loadedEds = new EDSsharp();
            loadedEds.Loadfile(infile);
            loadedEds.projectFilename = infile;
            return loadedEds;
        }

        private static EDSsharp openXDDfile(string path)
        {
            CanOpenXDD_1_1 coxml_1_1 = new CanOpenXDD_1_1();
            EDSsharp loadedEds = coxml_1_1.ReadXML(path);

            if (loadedEds == null)
            {
                CanOpenXDD coxml = new CanOpenXDD();
                loadedEds = coxml.readXML(path);

                if (loadedEds == null)
                    return null;
            }

            loadedEds.projectFilename = path;
            return loadedEds;
        }

        private static EDSsharp openProtobuffile(string path, bool json)
        {
            CanOpenXDD_1_1 coxml_1_1 = new CanOpenXDD_1_1();
            EDSsharp loadedEds = coxml_1_1.ReadProtobuf(path, json);

            if (loadedEds == null)
                return null;

            loadedEds.projectFilename = path;
            return loadedEds;
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
            string name = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            Console.WriteLine($"Usage: {name} --infile FILE1 --outfile FILE2 [--type EXPORTER]");
            Console.WriteLine($"       {name} --export-project --infile FILE --outdir DIR [--od BASENAME] [--json FILE.json] [--canopennode v4|legacy]");
            Console.WriteLine("Converts a given XDD or EDS file to many other available types.");
            Console.WriteLine($"Example: {name} --infile project.xdd --outfile map.md --type NetworkPDOReport");
            Console.WriteLine($"Example: {name} --export-project --infile project.xdd --outdir ./out");
            Console.WriteLine("");
            Console.WriteLine("FILE1 shall be a .xdd, .eds, .json, or .binpb file.");
            Console.WriteLine("FILE2 shall have the extension of one of the supported exporters below.");
            Console.WriteLine("EXPORTER shall be one of the listed exporters below IF AND ONLY IF multiple of them support your output file extension.");
            Console.WriteLine("");
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

        static void PrintExportProjectHelpText()
        {
            string name = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            Console.WriteLine($"Usage: {name} --export-project --infile FILE --outdir DIR [--od BASENAME] [--json FILE.json] [--canopennode v4|legacy]");
            Console.WriteLine("Exports a project file to CanOpenNode .c/.h and CanOpenNode protobuf JSON.");
            Console.WriteLine($"Example: {name} --export-project --infile project.xdd --outdir ./out");
            Console.WriteLine("");
            Console.WriteLine("FILE shall be a .xdd, .xdc, .xpd, .eds, .dcf, .json, or .binpb file.");
            Console.WriteLine("DIR is the output directory.");
            Console.WriteLine("BASENAME is the base name for the generated .c and .h files (default: input file name without extension).");
            Console.WriteLine("FILE.json is the output JSON path (default: DIR/BASENAME.json).");
            Console.WriteLine("--canopennode defaults to v4.");
        }
    }
}
