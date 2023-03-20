namespace DeviceAdapter.Abstraction.Devices
{
    public interface IDevicesAdapter
    {
        /// <summary>
        /// Pokud chcete připojit do systému jednu kameru bez reinicializace ostatních kamer, můžete použít tuto operaci.
        /// Typicky ji využijete při připojení dodatečné kamery nebo pokud chcete znova připojit kameru, kterou jste před tím odpojili operací odpojení.Operace je asynchronní,
        /// proto může v odpovědi reportovat kameru jako stále offline. Kamera se připojí ihned jak to bude možné. Informaci o připojení obdržíte v rámci logu.
        /// </summary>
        /// <param name="camId">
        /// 0000,0815-0001 (string, required) - ID kamery (nebo kamer - v tomto případě se jedná o seznam ID oddělený čárkou),
        /// ze kterých bude orbázek sejmut. Example: 0815.</param>
        void Connect(string camid);

        /// <summary>
        /// Provede odpojení jedné kamery ze systému. Kameru je poté možno znova připojit pomocí operace připojení.Operace je asynchronní,
        /// proto může v odpovědi reportovat kameru jako stále online. Kamera se připojí ihned jak to bude možné. Informaci o odpojení obdržíte v rámci logu.
        /// </summary>
        /// <param name="camId">
        /// 0000,0815-0001 (string, required) - ID kamery (nebo kamer - v tomto případě se jedná o seznam ID oddělený čárkou),
        /// ze kterých bude orbázek sejmut. Example: 0815.</param>
        void Disconnect(string camid);
    }
}