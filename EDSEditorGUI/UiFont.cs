using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ODEditor
{
    /// <summary>
    /// Mitgelieferte, skalierbare Oberflaechenschrift fuer den net8-Build:
    /// Noto Sans, fuer Freitextfelder Noto Sans Mono (Dateien in Fonts/).
    ///
    /// Die WinForms-Standardschrift Segoe UI gibt es unter Wine nicht; der
    /// Ersatz dort ist oft eine schlecht lesbare Bitmap-Schrift. Die Schriften
    /// werden nur fuer diesen Prozess registriert, installiert wird nichts.
    /// Im net481-Build (ohne Application.SetDefaultFont) bleibt alles beim Alten.
    /// </summary>
    internal static class UiFont
    {
        /// <summary>Standardgroesse in Punkt, auf die sich die Skalierung bezieht.</summary>
        public const float DefaultSize = 9f;

        private static readonly PrivateFontCollection collection = new PrivateFontCollection();
        private static FontFamily sans;
        private static FontFamily mono;

        /// <summary>Gewaehlte Schriftgroesse in Punkt.</summary>
        public static float PointSize { get; private set; } = DefaultSize;

        /// <summary>True, wenn die mitgelieferte Schrift aktiv ist.</summary>
        public static bool IsActive => sans != null;

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern int AddFontResourceEx(string name, uint flags, IntPtr reserved);
        private const uint FR_PRIVATE = 0x10;

        /// <summary>
        /// Registriert die Schriften und setzt sie als Standardschrift der
        /// Anwendung. Muss vor dem ersten Fenster aufgerufen werden.
        /// </summary>
        /// <param name="pointSize">gewuenschte Groesse in Punkt (6..24)</param>
        public static void Apply(float pointSize)
        {
#if NET6_0_OR_GREATER
            try
            {
                string dir = Path.Combine(AppContext.BaseDirectory, "Fonts");
                if (!Directory.Exists(dir))
                    return;

                foreach (string file in Directory.GetFiles(dir, "*.ttf"))
                {
                    // GDI (Textausgabe der Controls) und GDI+ (Font-Objekte)
                    // fuehren getrennte Schriftlisten - beide brauchen die Datei.
                    AddFontResourceEx(file, FR_PRIVATE, IntPtr.Zero);
                    collection.AddFontFile(file);
                }

                FontFamily family = collection.Families.FirstOrDefault(f => f.Name == "Noto Sans");
                if (family == null)
                    return;

                PointSize = Math.Max(6f, Math.Min(24f, pointSize));
                Application.SetDefaultFont(new Font(family, PointSize));
                sans = family;
                mono = collection.Families.FirstOrDefault(f => f.Name == "Noto Sans Mono");
            }
            catch (Exception)
            {
                // Ohne mitgelieferte Schrift einfach mit der Standardschrift weiter.
                sans = null;
                mono = null;
            }
#endif
        }

        /// <summary>
        /// Schrift der Oberflaechenfamilie relativ zur gewaehlten Groesse.
        /// </summary>
        /// <param name="style">Schriftstil (Regular oder Bold)</param>
        /// <param name="factor">Faktor auf die gewaehlte Groesse</param>
        /// <returns>die Schrift, oder null, wenn die mitgelieferte Schrift nicht aktiv ist</returns>
        public static Font Create(FontStyle style, float factor)
        {
            return IsActive ? new Font(sans, PointSize * factor, style) : null;
        }

        /// <summary>
        /// Setzt auf einer Menue-, Kontextmenue- oder Werkzeugleiste die
        /// Oberflaechenschrift. WinForms nimmt fuer Leisten sonst die
        /// System-Menueschrift statt der Standardschrift; aufklappende
        /// Untermenues uebernehmen die Schrift ihrer Leiste.
        /// </summary>
        /// <param name="strip">Leiste, z.B. MenuStrip oder ContextMenuStrip</param>
        public static void UseFor(ToolStrip strip)
        {
            if (IsActive)
                strip.Font = new Font(sans, PointSize);
        }

        /// <summary>
        /// Setzt auf einem Freitextfeld die mitgelieferte Mono-Schrift.
        /// </summary>
        /// <param name="control">Textfeld mit fest eingestellter Courier-Schrift</param>
        public static void UseMono(Control control)
        {
            if (IsActive && mono != null)
                control.Font = new Font(mono, PointSize);
        }

        /// <summary>
        /// Skaliert eine fest eingestellte Pixelgroesse mit der gewaehlten Schriftgroesse.
        /// </summary>
        /// <param name="size">Groesse bei Standardschriftgroesse</param>
        /// <returns>skalierte Groesse (unveraendert, wenn die Schrift nicht aktiv ist)</returns>
        public static Size Scale(Size size)
        {
            if (!IsActive)
                return size;

            float f = PointSize / DefaultSize;
            return new Size((int)Math.Round(size.Width * f), (int)Math.Round(size.Height * f));
        }
    }
}
