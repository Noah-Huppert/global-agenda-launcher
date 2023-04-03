using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalAgendaLauncher.Controllers
{
    public class GABinary
    {
        /// <summary>
        /// Location where GA could be installed in a standard computer on Steam.
        /// </summary>
        public const string GLOBAL_AGENDA_BINARY_STEAM_PATH = "\"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Global Agenda Live\\Binaries\\GlobalAgenda.exe\"";

        /// <summary>
        /// The location of the game binary.
        /// </summary>
        private string? path;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="path"></param>
        GABinary(string? path)
        {
            this.path = path;
        }

        /// <summary>
        /// Try to find the GA binary based on already known locations and set the path property.
        /// </summary>
        /// <returns>
        /// True if the binary was found and recorded, false if not.
        /// </returns>
        public bool GuessPath()
        {
            // Check if the GA binary is in any known locations
            if (File.Exists(GLOBAL_AGENDA_BINARY_STEAM_PATH))
            {
                this.path = GLOBAL_AGENDA_BINARY_STEAM_PATH;
                return true;
            }

            return false;
        }
    }
}
