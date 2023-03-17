using System;
using System.Collections.Generic;
using System.IO;

namespace DeviceAdapter.Devices.Cameras.Adapters.Fake
{
    public class FakeAdapter : ICamerasAdapter
    {
        private readonly string _rootPath =
            Path.Combine(Environment.CurrentDirectory, "Devices\\Cameras\\Adapters\\Fake");

        public void Connect(string camid)
        {
        }

        public void Disconnect(string camid)
        {
        }

        public CamerasResponse CaptureSingle(string cam_id, string format)
        {
            return new CamerasResponse
            {
                Success = true,
                Images = new List<CamerasImage>
                {
                    new CamerasImage {CameraID = "CamF001", Image = CaptureJpg()}
                }
            };
        }

        public CamerasResponse CaptureStreamBatch(string cam_id, string format, int num_images, int fps)
        {
            return new CamerasResponse
            {
                Success = true,
                Images = new List<CamerasImage>
                {
                    new CamerasImage {CameraID = "CamF001", Image = CaptureJpg()},
                    new CamerasImage {CameraID = "CamF002", Image = CapturePng()}
                }
            };
        }

        private string CapturePng()
        {
            var path = Path.Combine(_rootPath, "logo_24_vision_system.png");
            return GetStaticFileContent(path);
        }

        private string CaptureJpg()
        {
            var path = Path.Combine(_rootPath, "logo_24_vision_system.jpg");
            return GetStaticFileContent(path);
        }

        private string GetStaticFileContent(string path)
        {
            using var fs = File.OpenRead(path);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}