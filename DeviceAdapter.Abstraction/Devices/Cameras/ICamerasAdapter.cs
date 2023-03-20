namespace DeviceAdapter.Abstraction.Devices.Cameras
{
    public interface ICamerasAdapter : IDevicesAdapter
    {
        /// <summary>
        /// Provede sejmutí jednoho obrázku z jedné nebo více kamer. Pokud je sejmut obrázek pouze z jedné kamery,
        /// pak je odpovědí této metody rovnou tento obrázek s odpovídajícím mime typem.
        /// Pokud je požádáno o vícero obrázků, pak je odpovědí json popsaný v této operaci s binárními obrázky zakódovanými jako base64.
        /// Pokud je požadováno sejmutí obrázku z více kamer,
        /// uveďte ID těchto kamer do URL odděleny čárkou (např. /v1/capture/single/png/0815-0000,0815-0001).
        /// </summary>
        /// <param name="camId">
        /// 0000,0815-0001 (string, required) - ID kamery (nebo kamer - v tomto případě se jedná o seznam ID oddělený čárkou),
        /// ze kterých bude orbázek sejmut. Example: 0815.</param>
        /// <param name="format">
        /// Formát, ve kterém bude obrázek poskytnut, aktuálně je možné požádat o formáty: jpeg, png a bmp. Example: png.
        /// </param>
        /// <returns> Odpověď adaptéru kamery.</returns>
        CamerasResponse CaptureSingle(string camId, string format);

        /// <summary>
        /// Provede sejmutí dávky obrázků - dávkou rozumíme sadu num_images obrázků sejmutých ve frekvenci fps obrázků za vteřinu.Pokud je sejmut pouze jeden obrázek z jedné kamery,
        /// pak je odpovědí této metody rovnou tento obrázek s odpovídajícím mime typem. Pokud je požádáno o vícero obrázků, pak je odpovědí json popsaný v této operaci s binárními obrázky zakódovanými jako base64.
        /// Pokud je požadováno sejmutí obrázku z více kamer, uveďte ID těchto kamer do URL odděleny čárkou (např. /v1/capture/stream-batch/png/0815-0000,0815-0001/20/10).
        /// </summary>
        /// <param name="camId">
        /// 0000,0815-0001 (string, required) - ID kamery (nebo kamer - v tomto případě se jedná o seznam ID oddělený čárkou),
        /// ze kterých bude orbázek sejmut. Example: 0815.</param>
        /// <param name="format">
        /// Formát, ve kterém bude obrázek poskytnut, aktuálně je možné požádat o formáty: jpeg, png a bmp. Example: png.
        /// </param>
        /// <param name="numImages">
        /// Počet obrázků v dávce - pro každou kameru. Example: 20.
        /// </param>
        /// <param name="fps">
        /// frekvence snímání (počet obrázků za vteřinu). Example: 10.
        /// </param>
        /// <returns> Odpověď adaptéru kamery.</returns>
        CamerasResponse CaptureStreamBatch(string camId, string format, int numImages, int fps); //
    }
}