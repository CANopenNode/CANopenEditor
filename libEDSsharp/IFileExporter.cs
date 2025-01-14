using System;
using System.Collections.Generic;

namespace libEDSsharp
{
    /// <summary>
    /// Includes all info about a exporter that is needed to show user and call it
    /// </summary>
    public class ExporterDescriptor
    {
        /// <summary>
        /// Export eds(s) to file(s)
        /// </summary>
        /// <param name="filepath">path path that should indicate where and what name the outputed file(s) should have</param>
        /// <param name="eds">list of eds(s) not all support multiple edss, in that case use the first</param>
        public delegate void ExportFunc(string filepath, List<EDSsharp> edss);
        [Flags]
        public enum ExporterFlags
        {
            /// <summary>
            /// True if exporter will expect multiple edss
            /// </summary>
            MultipleNodeSupport = 1,
            /// <summary>
            /// Documentation related
            /// </summary>
            Documentation = 2,
            /// <summary>
            /// CanOpenNode related
            /// </summary>
            CanOpenNode = 3,
        }
        /// <summary>
        /// short human readable description
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// What file extension the exported file(s) will have
        /// </summary>
        public string[] Filetypes { get; }
        /// <summary>
        /// Used to indicated different types of exporters
        /// </summary>
        public ExporterFlags Flags { get; }
        /// <summary>
        /// The function that is exporting to file
        /// </summary>
        public ExportFunc Func { get; }
        /// <summary>
        /// constructor that sets all the values
        /// </summary>
        /// <param name="description">short human readable description</param>
        /// <param name="filetypes">What file extension the exported file(s) will have</param>
        /// <param name="flags">Used to indicated different types of exporters</param>
        /// <param name="func">The function that is exporting to file</param>
        public ExporterDescriptor(string description, string[] filetypes, ExporterFlags flags, ExportFunc func)
        {
            Description = description;
            Filetypes = filetypes;
            Flags = flags;
            Func = func;
        }
    }
    /// <summary>
    /// Interface for exporters
    /// </summary>
    public interface IFileExporter
    {
        /// <summary>
        /// Fetches all the different fileexporter types the class supports
        /// </summary>
        /// <returns>List of the different exporters the class supports</returns>
        ExporterDescriptor[] GetExporters();
    }
}
