using System;
using System.Collections;
using System.Collections.Generic;

namespace SOD
{
    namespace Nix
    {

        public class DeviceManager
        {
            public Dictionary<int, Bin.CharacterDevice> DevicesById;
            public Dictionary<string, List<Bin.CharacterDevice>> DevicesByType;
            public NixSystem MainSystem { get; private set; }

            public DeviceManager(NixSystem system)
            {
                DevicesById = new Dictionary<int, Bin.CharacterDevice>();
                DevicesByType = new Dictionary<string, List<Bin.CharacterDevice>>();
                MainSystem = system;
            }

            public List<Bin.CharacterDevice> FindDevices(string type)
            {
                if (DevicesByType.ContainsKey(type))
                {
                    return DevicesByType[type];
                }
                return null;
            }
            public Bin.CharacterDevice FindDevice(int id)
            {
                if (DevicesById.ContainsKey(id))
                {
                    return DevicesById[id];
                }
                return null;
            }

            public bool AddDevice(Bin.CharacterDevice device)
            {
                int id = device.GetId();  
                if (DevicesById.ContainsKey(id))
                {
                    return false;
                }
                DevicesById[id] = device;

                string type = device.DeviceType();
                if (!DevicesByType.ContainsKey(type))
                {
                    DevicesByType[type] = new List<Bin.CharacterDevice>();
                }
                DevicesByType[type].Add(device);
                return true;
            }
            public bool RemoveDevice(Bin.CharacterDevice device)
            {
                int id = device.GetId();
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
