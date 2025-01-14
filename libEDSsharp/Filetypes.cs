using System;
using System.Collections.Generic;
using System.Linq;
using static libEDSsharp.ExporterDescriptor;

namespace libEDSsharp
{
    /// <summary>
    /// Unified interface to all filetypes supported by the library
    /// </summary>
    public class Filetypes
    {
        /// <summary>
        /// Returns description of all the different filetypes that can be exported to
        /// </summary>
        /// <param name="flags">optional filter to filter out different types of exporters</param>
        /// <returns>list of file exporter that matches the filter</returns>
        public static ExporterDescriptor[] GetExporters(ExporterFlags flags = 0)
        {
            var exporters = new List<ExporterDescriptor>();
            foreach (Type mytype in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(mytype => mytype.GetInterfaces().Contains(typeof(IFileExporter))))
            {
                var exporterClass = (IFileExporter)Activator.CreateInstance(mytype);
                var classExporters = exporterClass.GetExporters();
                foreach (var exporter in classExporters)
                {
                    if (((exporter.Flags & flags) > 0) || flags == 0)
                    {
                        exporters.Add(exporter);
                    }

                }
            }
            return exporters.ToArray();
        }
    }
}
