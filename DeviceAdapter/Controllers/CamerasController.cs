using Microsoft.AspNetCore.Mvc;
using DeviceAdapter.Abstraction.Devices.Cameras;

namespace DeviceAdapter.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class CamerasController : ControllerBase, ICamerasAdapter
    {
        private readonly ICamerasAdapter _adapter;

        public CamerasController(ICamerasAdapter adapter)
        {
            _adapter = adapter;
        }

        [HttpGet]
        [Route("cameras/{camid}/connect")]
        public void Connect(string camid)
        {
            //TODO TRACE
            //camid = "0000"; //TODO (0000, 0001) (VirtualDevice-0000, VirtualDevice-0001)

            _adapter.Connect(camid);
        }

        [HttpGet]
        [Route("cameras/{camid}/disconnect")]
        public void Disconnect(string camid)
        {
            //TODO TRACE
            //camid = "0000"; //TODO (0000, 0001) (VirtualDevice-0000, VirtualDevice-0001)

            _adapter.Disconnect(camid);
        }

        [HttpGet]
        [Route("capture/single/{format}/{cam_id}")]
        public CamerasResponse CaptureSingle(string camId, string format)
        {
            //TODO TRACE
            //Base64 to Image: https://codebeautify.org/base64-to-image-converter

            //cam_id = "0000"; //TODO (0000, 0001) (VirtualDevice-0000, VirtualDevice-0001)
            //format = "Jpeg"; //TODO Jpeg, Png

            return _adapter.CaptureSingle(camId, format);
        }

        [HttpGet]
        [Route("capture/stream-batch/{format}/{cam_id}/{num_images}/{fps}")]
        public CamerasResponse CaptureStreamBatch(string camId, string format, int numImages, int fps)
        {
            //TODO TRACE
            //Base64 to Image: https://codebeautify.org/base64-to-image-converter

            //cam_id = "0000"; //TODO (0000, 0001) (VirtualDevice-0000, VirtualDevice-0001)
            //format = "Jpeg"; //TODO Jpeg, Png
            //num_images = 2;
            //fps = 10;

            return _adapter.CaptureStreamBatch(camId, format, numImages, fps);
        }
    }
}