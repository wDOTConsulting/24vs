﻿using System;
using System.Collections.Generic;
using System.IO;
using DeviceAdapter.Abstraction.Devices.Cameras;

namespace DeviceAdapter.Fake.Devices.Cameras.Adapters
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

        public CamerasResponse CaptureSingle(string camId, string format)
        {
            return new CamerasResponse
            {
                Success = true,
                Images = new List<CamerasImage>
                {
                    new CamerasImage {CameraId = "CamF001", Image = CaptureJpg()}
                }
            };
        }

        public CamerasResponse CaptureStreamBatch(string camId, string format, int numImages, int fps)
        {
            return new CamerasResponse
            {
                Success = true,
                Images = new List<CamerasImage>
                {
                    new CamerasImage {CameraId = "CamF001", Image = CaptureJpg()},
                    new CamerasImage {CameraId = "CamF002", Image = CapturePng()}
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