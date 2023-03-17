using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeviceAdapter.Devices.Cameras
{
    /// <summary>
    ///     Odpověď adaptéru kamery.
    /// </summary>
    public class CamerasResponse
    {
        /// <summary>
        /// Informace o úspěchu operace.
        /// </summary>
        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        /// <summary>
        /// Seznam obrázků.
        /// </summary>

        [JsonPropertyName("Images")]
        public IEnumerable<CamerasImage> Images { get; set; } = new List<CamerasImage>();

        /// <summary>
        /// Seznam chyb, ke kterým při sejmutí obrázků došlo.
        /// </summary>
        [JsonPropertyName("Errors")]
        public string[] Errors { get; set; } = new string[0];
    }
}