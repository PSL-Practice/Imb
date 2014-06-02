using System.Collections.Generic;
using System.Configuration;

namespace Imb
{
    /// <summary>
    /// This class contains the user settings that should be saved for the simple damage meter app.
    /// </summary>
    public class ImbSettings
    {
        [UserScopedSetting]
        public string LibraryPath { get; set; }

        [UserScopedSetting]
        public List<WindowPositionData> WindowPositions { get; set; }
    }
}