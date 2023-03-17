using DeviceAdapter.Devices.Cameras;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeviceAdapter.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class CamerasController : ControllerBase, ICamerasAdapter
    {
        private readonly ICamerasAdapter _adapter;
        private readonly ILogger<CamerasController> _logger;

        public CamerasController(ICamerasAdapter adapter, ILogger<CamerasController> logger)
        {
            _adapter = adapter;
            _logger = logger;
        }

        [HttpGet]
        [Route("cameras/{camid}/connect")]
        public void Connect(string camid)
        {
            _adapter.Connect(camid);
        }

        [HttpGet]
        [Route("cameras/{camid}/disconnect")]
        public void Disconnect(string camid)
        {
            _adapter.Disconnect(camid);
        }

        [HttpGet]
        [Route("capture/single/{format}/{cam_id}")]
        public CamerasResponse CaptureSingle(string cam_id, string format)
        {
            return _adapter.CaptureSingle(cam_id, format);
        }

        [HttpGet]
        [Route("capture/stream-batch/{format}/{cam_id}/{num_images}/{fps}")]
        public CamerasResponse CaptureStreamBatch(string cam_id, string format, int num_images, int fps)
        {
            return _adapter.CaptureStreamBatch(cam_id, format, num_images, fps);
        }
    }
}