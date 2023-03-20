using System.Text.Json.Serialization;

namespace DeviceAdapter.Abstraction.Devices.Cameras
{
    /// <summary>
    ///     Obrázek z kamery.
    /// </summary>
    public class CamerasImage
    {
        /// <summary>
        ///     Sériové číslo kamery, která obrázek pořídila (např. 0815-0000).
        /// </summary>
        [JsonPropertyName("CameraID")]
        public string CameraId { get; set; }

        /// <summary>
        ///     Base64 binární reprezentace obrázku v požadovaném formátu (např. /9j/4AAQSkZJRg...).
        /// </summary>
        [JsonPropertyName("Image")]
        public string Image { get; set; }
    }
}