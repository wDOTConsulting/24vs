namespace DeviceAdapter.Devices
{
    public interface IDevicesAdapter
    {
        void Connect(string camid);
        void Disconnect(string camid);
    }
}