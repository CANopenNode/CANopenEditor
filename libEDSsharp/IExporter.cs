using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEDSsharp
{
    /// <summary>
    /// Interface for exporting CanOpenNode OD files
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Export file(s)
        /// </summary>
        /// <param name="filepath">filepath, .c and .h will be added to this to make the mulitiple files</param>
        /// <param name="eds">The eds that will be exported</param>
        void export(string filepath, EDSsharp eds);
    }
}
