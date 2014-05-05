using System;
using System.Collections;
using System.Collections.Generic;

namespace SOD
{
    namespace Nix
    {
        namespace Device
        {
            public class DeviceManager
            {
                public Dictionary<int, Device.CharacterDevice> DevicesById;
                public Dictionary<string, List<Device.CharacterDevice>> DevicesByType;
                public NixSystem MainSystem { get; private set; }
                private int _Counter = 0;
                public int Counter
                {
                    get
                    {
                        return _Counter++;
                    }
                }

                public DeviceManager(NixSystem system)
                {
                    DevicesById = new Dictionary<int, Device.CharacterDevice>();
                    DevicesByType = new Dictionary<string, List<Device.CharacterDevice>>();
                    MainSystem = system;
                }

                public List<Device.CharacterDevice> FindDevices(string type)
                {
                    if (DevicesByType.ContainsKey(type))
                    {
                        return DevicesByType[type];
                    }
                    return null;
                }
                public Device.CharacterDevice FindDevice(int id)
                {
                    if (DevicesById.ContainsKey(id))
                    {
                        return DevicesById[id];
                    }
                    return null;
                }

                public bool AddDevice(Device.CharacterDevice device)
                {
                    int id = device.Id;  
                    if (DevicesById.ContainsKey(id))
                    {
                        return false;
                    }
                    DevicesById[id] = device;

                    string type = device.DeviceType();
                    if (!DevicesByType.ContainsKey(type))
                    {
                        DevicesByType[type] = new List<Device.CharacterDevice>();
                    }
                    DevicesByType[type].Add(device);
                    return true;
                }
                public bool RemoveDevice(Device.CharacterDevice device)
                {
                    int id = device.Id;
                    if (!DevicesById.ContainsKey(id))
                    {
                        return false;
                    }
                    DevicesById.Remove(id);
                    string type = device.DeviceType();
                    DevicesByType[type].Remove(device);
                    if (DevicesByType[type].Count == 0)
                    {
                        DevicesByType.Remove(type);
                    }
                    return true;
                }
            }

        }
    }
}
