
namespace libEDSsharp
{
    /// <summary>
    /// Factory for making different canopennode exporter  
    /// </summary>
    public static class ExporterFactory
    {
        /// <summary>
        /// CanOpenNode exporter types
        /// </summary>
        public enum Exporter
        {
            /// <summary>
            /// CanOpenNode exporter v4 (latest)
            /// </summary>
            CANOPENNODE_V4 = 0,
            /// <summary>
            /// CanOpenNode exporter for v1-3 (legacy)
            /// </summary>
            CANOPENNODE_LEGACY = 1
        }

        /// <summary>
        /// Returns exporter based on ex parameter
        /// </summary>
        /// <param name="ex">what exporter version you want. Default is CANOPENNODE_LEGACY</param>
        /// <returns>A exporter</returns>
        public static IExporter getExporter(Exporter ex = Exporter.CANOPENNODE_LEGACY)
        {
            IExporter exporter;

            switch (ex)
            {
                default:
                case Exporter.CANOPENNODE_V4:
                    exporter = new CanOpenNodeExporter_V4();
                    break;

                case Exporter.CANOPENNODE_LEGACY:
                    exporter = new CanOpenNodeExporter();
                    break;
            }


            return exporter;
        }
    }
}
