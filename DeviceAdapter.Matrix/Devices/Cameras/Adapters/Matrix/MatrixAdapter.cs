﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using mv.impact.acquire;
using mv.impact.acquire.examples.helper;
using DeviceAdapter.Abstraction.Devices.Cameras;

namespace DeviceAdapter.Matrix.Devices.Cameras.Adapters.Matrix
{
    public class MatrixAdapter : ICamerasAdapter
    {
        private readonly ILogger<MatrixAdapter> _logger;
        private string[] _errors;
        private Device _device;
        private FunctionInterface _fi;
        private Request _request;
        private string _cameraId;
        private ImageFormat _imageFormat;
        private IEnumerable<CamerasImage> _images;
        private Thread _thread;
        private int _imagesToSaveCount;
        private int _requestCount;

        public MatrixAdapter(ILogger<MatrixAdapter> logger)
        {
            _logger = logger;
        }

        public void Connect(string camid)
        {
            //The Device Needs To Be Initialized
            InitDevice(camid);
        }

        public void Disconnect(string camid)
        {
            //The Device Needs To Be Initialized
            CloseDevice(camid);
        }

        public CamerasResponse CaptureSingle(string camId, string format)
        {
            _cameraId = camId;
            SetImageFormat(format);

            //Step 1: The Device Needs To Be Initialized
            InitDevice(_cameraId);
            if (_device == null) return new CamerasResponse {Errors = _errors};

            //Step 2: Request The Acquisition Of An Image
            RequestAcquisitionOfImage();
            if (_fi == null) return new CamerasResponse {Errors = _errors};

            DeviceAccess.manuallyStartAcquisitionIfNeeded(_device, _fi);

            //Step 3: Wait Until The Image Has Been Captured
            ImageRequestWaitFor();
            if (_request == null)
            {
                DeviceAccess.manuallyStopAcquisitionIfNeeded(_device, _fi);
                return new CamerasResponse {Errors = _errors};
            }

            //Step 4: Get Image (Base64) using the request's bitmapData
            var image = GetImage();

            //Step 5: Unlock The Image Buffer Once The Image Has Been Processed
            ImageRequestUnlock(_request);

            DeviceAccess.manuallyStopAcquisitionIfNeeded(_device, _fi);

            //Step 6: Create and return cameras response
            return new CamerasResponse
            {
                Images = new List<CamerasImage>
                {
                    new CamerasImage {CameraId = _cameraId, Image = image}
                }
            };
        }

        public CamerasResponse CaptureStreamBatch(string camId, string format, int numImages, int fps)
        {
            _cameraId = camId;
            SetImageFormat(format);

            //Step 1: The Device Needs To Be Initialized
            InitDevice(_cameraId);
            if (_device == null) return new CamerasResponse {Errors = _errors};

            //Step 2: Terminate the capture thread if needed
            //TerminateCapture(_device);

            //Step 3: Request The Acquisition Of An Image list
            _imagesToSaveCount = numImages;
            _requestCount = Math.Max(numImages, 2);
            RequestAcquisitionOfImageList();
            if (_thread == null) return new CamerasResponse {Errors = _errors};

            //Step 4: Create and return cameras response
            return new CamerasResponse
            {
                Images = _images
            };
        }

        /// <summary>
        /// The Device Needs To Be Initialized (By Product And ID, e.g. 0815-0000, 0815-0001, VirtualDevice-0000)
        /// </summary>
        private void InitDevice(string camId)
        {
            _cameraId = camId;

            // This will add the folders containing unmanaged libraries to the PATH variable.
            LibraryPath.init();

            // get device by Id
            _device = GetDeviceById(camId) ??
                      // get device by product and Id
                      GetDeviceByProductAndId(camId);

            if (_device == null)
            {
                var msg = $"The device '{camId}' not found.";
                _logger.LogError(msg);
                _errors = new[] {msg};
                return;
            }

            _logger.LogInformation("Initialising the device. This might take some time...");

            // initialise device (this step is optional as this will be done automatically from
            // all other wrapper classes that accept a device pointer)
            try
            {
                _device.open();
            }
            catch (ImpactAcquireException e)
            {
                // this e.g. might happen if the same device is already opened in another process...

                var msg =
                    $"An error occurred while opening the device serial: '{_device.serial}' (error code: '{e.Message}')";
                _logger.LogError(msg);
                _errors = new[] {msg};
            }
        }

        /// <summary>
        /// The Device Needs To Be Closesed (By Product And ID, e.g. 0815-0000, 0815-0001, VirtualDevice-0000)
        /// </summary>
        private void CloseDevice(string camId)
        {
            _cameraId = camId;

            // This will add the folders containing unmanaged libraries to the PATH variable.
            LibraryPath.init();

            // get device by Id
            _device = GetDeviceById(camId) ??
                      // get device by product and Id
                      GetDeviceByProductAndId(camId);

            if (_device == null)
            {
                var msg = $"The device '{camId}' not found.";
                _logger.LogError(msg);
                _errors = new[] {msg};
                return;
            }

            _logger.LogInformation("Clossing the device. This might take some time...");

            // Closes an opened device.
            try
            {
                _device.close();
            }
            catch (ImpactAcquireException e)
            {
                // this e.g. might happen if the same device is already opened in another process...

                var msg =
                    $"An error occurred while clossing the device serial: '{_device.serial}' (error code: '{e.Message}')";
                _logger.LogError(msg);
                _errors = new[] {msg};
            }
        }

        /// <summary>
        /// Request The Acquisition Of An Image
        /// </summary>
        private void RequestAcquisitionOfImage()
        {
            // create an instance of the function interface for this device
            // (this would also initialise a device if necessary)
            _fi = new FunctionInterface(_device);

            // send a request to the default request queue of the device and wait for the result.
            var result = (TDMR_ERROR) _fi.imageRequestSingle();
            if (result == TDMR_ERROR.DMR_NO_ERROR) return;
            var msg =
                $"'FunctionInterface.imageRequestSingle' returned with an unexpected result: {result}({ImpactAcquireException.getErrorCodeAsString(result)})";
            _logger.LogError(msg);
            _errors = new[] {msg};
        }

        /// <summary>
        /// Request The Acquisition Of An Image list
        /// </summary>
        private void RequestAcquisitionOfImageList()
        {
            // Read the given request count.
            if (_requestCount < 2)
            {
                var msg =
                    $"Invalid request count '{_requestCount}'. Please re-enter the request count(an integer ranged from 2 to 32768).";
                _logger.LogError(msg);
                _errors = new[] {msg};
                return;
            }

            // Read the given number of images to save.
            if (_imagesToSaveCount < 1)
            {
                var msg =
                    $"Invalid number of images '{_imagesToSaveCount}' to be saved. At least one image has to be captured. Please re-enter the value.";
                _logger.LogError(msg);
                _errors = new[] {msg};
                return;
            }

            // Start capture thread.
            _thread = new Thread(delegate()
            {
                // establish access to the statistic properties
                var statistics = new Statistics(_device);
                // create an interface to the device found
                var fi = new FunctionInterface(_device);

                // Send all requests to the capture queue. There can be more than 1 queue for some devices, but for this sample
                // we will work with the default capture queue. If a device supports more than one capture or result
                // queue, this will be stated in the manual. If nothing is mentioned about it, the device supports one
                // queue only. This loop will send all requests currently available to the driver. To modify the number of requests
                // use the property mv.impact.acquire.SystemSettings.requestCount at runtime or the property
                // mv.impact.acquire.Device.defaultRequestCount BEFORE opening the device.
                TDMR_ERROR result;
                while ((result = (TDMR_ERROR) fi.imageRequestSingle()) == TDMR_ERROR.DMR_NO_ERROR)
                {
                }

                if (result != TDMR_ERROR.DEV_NO_FREE_REQUEST_AVAILABLE)
                {
                    _logger.LogInformation(
                        "'FunctionInterface.imageRequestSingle' returned with an unexpected result: {0}({1})", result,
                        ImpactAcquireException.getErrorCodeAsString(result));
                }

                DeviceAccess.manuallyStartAcquisitionIfNeeded(_device, fi);
                // run thread loop
                // we always have to keep at least 2 images as the display module might want to repaint the image, thus we
                // cannot free it unless we have a assigned the display to a new buffer.
                Request pPreviousRequest = null;
                const int timeoutMs = 500;
                var cnt = 0;
                var images = new List<CamerasImage>();
                _images = images;
                while (cnt < _imagesToSaveCount)
                {
                    // wait for results from the default capture queue
                    var requestNr = fi.imageRequestWaitFor(timeoutMs);
                    _request = fi.isRequestNrValid(requestNr) ? fi.getRequest(requestNr) : null;
                    if (_request != null)
                    {
                        if (_request.isOK)
                        {
                            ++cnt;
                            // here we can log some statistical information every 5th image
                            if (cnt % 5 == 0)
                            {
                                Console.WriteLine("Info from {0}: {1}: {2}, {3}: {4}, {5}: {6}", _device.serial.read(),
                                    statistics.framesPerSecond.name, statistics.framesPerSecond.readS(),
                                    statistics.errorCount.name, statistics.errorCount.readS(),
                                    statistics.captureTime_s.name, statistics.captureTime_s.readS());
                            }

                            // Save the current image with the given format.
                            var image = GetImage();
                            var cImage = new CamerasImage {CameraId = _cameraId, Image = image};
                            images.Add(cImage);
                        }
                        else
                        {
                            _logger.LogError("Error: {0}", _request.requestResult.readS());
                        }

                        // this image has been displayed thus the buffer is no longer needed...
                        pPreviousRequest?.unlock();

                        pPreviousRequest = _request;
                        // send a new image request into the capture queue
                        fi.imageRequestSingle();
                    }
                }

                DeviceAccess.manuallyStopAcquisitionIfNeeded(_device, fi);

                // free the last potentially locked request
                if (_request != null)
                {
                    _request.unlock();
                }

                // clear all queues
                fi.imageRequestReset(0, 0);
            });

            _thread.Start();
            _thread.Join();
        }

        /// <summary>
        /// Wait Until The Image Has Been Captured
        /// </summary>
        private void ImageRequestWaitFor()
        {
            // Wait for results from the default capture queue by passing a timeout (The maximum time allowed
            // for the application to wait for a Result). Infinity value: -1, positive value: The time to wait in milliseconds.
            // Please note that slow systems or interface technologies in combination with high resolution sensors
            // might need more time to transmit an image than the timeout value.
            // Once the device is configured for triggered image acquisition and the timeout elapsed before
            // the device has been triggered this might happen as well.
            // If waiting with an infinite timeout(-1) it will be necessary to call 'imageRequestReset' from another thread
            // to force 'imageRequestWaitFor' to return when no data is coming from the device/can be captured.
            const int timeoutMs = 10000;
            // wait for results from the default capture queue
            var requestNr = _fi.imageRequestWaitFor(timeoutMs);
            _request = _fi.isRequestNrValid(requestNr) ? _fi.getRequest(requestNr) : null;
            if (_request == null)
            {
                // If the error code is -2119(DEV_WAIT_FOR_REQUEST_FAILED), the documentation will provide
                // additional information under TDMR_ERROR in the interface reference
                var msg = "imageRequestWaitFor failed maybe the timeout value has been too small?";
                _logger.LogError(msg);
                _errors = new[] {msg};

                // unlock the buffer to let the driver know that you no longer need this buffer.
                _fi.imageRequestUnlock(requestNr);

                return;
            }

            if (!_request.isOK)
            {
                var msg = $"Error: {_request.requestResult.readS()}";
                _logger.LogError(msg);
                _errors = new[] {msg};
                // if the application wouldn't terminate at this point this buffer HAS TO be unlocked before
                // it can be used again as currently it is under control of the user. However terminating the application
                // will free the resources anyway thus the call
                // pRequest.unlock();
                // could be omitted here.

                // unlock the buffer to let the driver know that you no longer need this buffer.
                _request.unlock();

                return;
            }

            _logger.LogInformation("Image captured: {0}({1}x{2})", _request.imagePixelFormat.readS(),
                _request.imageWidth.read(), _request.imageHeight.read());
        }


        /// <summary>
        /// Get Image (Base64) using the request's bitmapData
        /// </summary>
        private string GetImage()
        {
            using var data = _request.bitmapData;
            // Building an instance of System.Drawing.Bitmap using the request's bitmapData
            var bImage = data.bitmap;

            // Storing the image
            var ms = new MemoryStream();
            bImage.Save(ms, _imageFormat);
            var byteImage = ms.ToArray();
            var base64Image = Convert.ToBase64String(byteImage); // Get Base64

            _logger.LogInformation($"Storing the image as '{_imageFormat}'.");

            return base64Image;
        }

        /// <summary>
        /// Unlock The Image Buffer Once The Image Has Been Processed
        /// </summary>
        private static void ImageRequestUnlock(Request pRequest)
        {
            // unlock the buffer to let the driver know that you no longer need this buffer.
            pRequest.unlock();
        }


        private void SetImageFormat(string format)
        {
            try
            {
                _imageFormat = (ImageFormat) typeof(ImageFormat)
                    .GetProperty(format, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                    ?.GetValue(null);
            }
            catch (Exception ex)
            {
                // The conversion failed.
                var msg = $"'Set image format '{format}' failed: " + ex.Message;
                _logger.LogError(msg);
                _errors = new[] {msg};
                // Set default value.
                _imageFormat = ImageFormat.Png;
            }
        }


        private static Device GetDeviceById(string camId)
        {
            return int.TryParse(camId, out var devId)
                ? DeviceManager.deviceList.FirstOrDefault(w => w.deviceID.read() == devId)
                : null;
        }

        private static Device GetDeviceByProductAndId(string camId)
        {
            var productAndId = camId.Split('-');
            var product = productAndId[0];
            return productAndId.Length > 1 && int.TryParse(productAndId[1], out var devId)
                ? DeviceManager.getDeviceByProductAndID(product, devId)
                : null;
        }
    }
}