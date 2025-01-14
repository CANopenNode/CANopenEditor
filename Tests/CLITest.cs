using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Tests
{
    public class CliTest : libEDSsharp.EDSsharp
    {
        string RunEDSSharp(string arguments)
        {
            File.Delete("Legacy.c");
            File.Delete("Legacy.h");
            File.Delete("V4.c");
            File.Delete("V4.h");
            File.Delete("file.eds");
            File.Delete("file.xpd");

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "EDSSharp.exe";
            p.StartInfo.Arguments = arguments;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output;
        }

        [Fact]
        public void XddToCanOpenNodeLegacy()
        {
            RunEDSSharp("--type CanOpenNode --infile minimal_project.xdd --outfile Legacy");
            string[] files = Directory.GetFiles(".", "Legacy.*");
            Assert.Equal(2, files.Length);
        }
        [Fact]
        public void XddToCanOpenNodeV4()
        {
            RunEDSSharp("--type CanOpenNodeV4 --infile minimal_project.xdd --outfile V4");
            string[] files = Directory.GetFiles(".", "V4.*");
            Assert.Equal(2, files.Length);
        }
        [Fact]
        public void OnlySingleExporterByExtensionPossible()
        {
            RunEDSSharp("--infile minimal_project.xdd --outfile file.eds");
            string[] files = Directory.GetFiles(".", "file.eds");
            Assert.Single(files);
        }
        [Fact]
        public void MultipleExporterByExtensionPossibleWithoutType()
        {
            //this should fail
            RunEDSSharp("--infile minimal_project.xdd --outfile file.xdd");
            string[] files = Directory.GetFiles(".", "file.xdd");
            Assert.Empty(files);
        }
        [Fact]
        public void MultipleExporterByExtensionPossibleWithType()
        {
            RunEDSSharp("--type CanOpenXDDv1.1 --infile minimal_project.xdd --outfile file.nxdd");
            string[] files = Directory.GetFiles(".", "file.nxdd");
            Assert.Single(files);
        }
    }
}
