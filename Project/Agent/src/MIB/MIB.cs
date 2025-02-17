public class MIB
{
    public DeviceGroup Device { get; set; } = new();
    public SensorsTable Sensors { get; set; } = new();
    public ActuatorsTable Actuators { get; set; } = new();
}

public class DeviceGroup
{
    public string Id { get; set; }
    public string Type { get; set; }
    public int BeaconRate { get; set; }
    public int NSensors { get; set; }
    public int NActuators { get; set; }
    public DateTime DateAndTime { get; set; }
    public TimeSpan UpTime { get; set; }
    public DateTime LastTimeUpdated { get; set; }
    public int OperationalStatus { get; set; } // 0 - standby  , 1 - normal ,  2 - erro
    public int Reset { get; set; }// 0 - no reset , 1 - reset must be done
}

public class SensorsTable
{
    public List<Sensor> Sensors { get; set; } = new();
}

public class ActuatorsTable
{
    public List<Actuator> Actuators { get; set; } = new();
}

public class Sensor
{
    public string Id { get; set; }
    public string Type { get; set; }
    public int Status { get; set; }
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public DateTime LastSamplingTime { get; set; }
}

public class Actuator
{
    public string Id { get; set; }
    public string Type { get; set; }
    public int Status { get; set; }
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public DateTime LastControlTime { get; set; }
}