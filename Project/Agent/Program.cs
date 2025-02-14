class Program
{
    static void Main()
    {
        var mib = new MIB
        {
            Device = new DeviceGroup
            {
                Id = "Device1",
                Type = "Domotics",
                BeaconRate = 60,
                UpTime = TimeSpan.Zero, // This will be updated later
                LastTimeUpdated = DateTime.UtcNow,
                OperationalStatus = 1, // 1 for normal operation
                Reset = 0, // No reset required initially
                DateAndTime = DateTime.UtcNow,
                NSensors = 3, // The number of sensors in the system
                NActuators = 3 // The number of actuators in the system
            },

            Sensors = new SensorsTable
            {
                Sensors = new List<Sensor>
                {
                    new Sensor
                    {
                        Id = "Light_Sala",
                        Type = "Light",
                        Status = 80,
                        MinValue = 0,
                        MaxValue = 100,
                        LastSamplingTime = DateTime.UtcNow
                    },
                    new Sensor
                    {
                        Id = "Light_Cozinha",
                        Type = "Light",
                        Status = 50,
                        MinValue = 0,
                        MaxValue = 100,
                        LastSamplingTime = DateTime.UtcNow
                    },
                    new Sensor
                    {
                        Id = "AC_Quarto",
                        Type = "Temperature",
                        Status = 22,
                        MinValue = 16,
                        MaxValue = 30,
                        LastSamplingTime = DateTime.UtcNow
                    }
                }
            },

            Actuators = new ActuatorsTable
            {
                Actuators = new List<Actuator>
                {
                    new Actuator
                    {
                        Id = "Light_Sala",
                        Type = "Light",
                        Status = 0,
                        MinValue = 0,
                        MaxValue = 100,
                        LastControlTime = DateTime.UtcNow
                    },
                    new Actuator
                    {
                        Id = "Light_Cozinha",
                        Type = "Light",
                        Status = 0,
                        MinValue = 0,
                        MaxValue = 100,
                        LastControlTime = DateTime.UtcNow
                    },
                    new Actuator
                    {
                        Id = "AC_Quarto",
                        Type = "Temperature",
                        Status = 0,
                        MinValue = 16,
                        MaxValue = 30,
                        LastControlTime = DateTime.UtcNow
                    }
                }
            }
        };


        Console.WriteLine("🔹 [Agent] Starting L-SNMPvS Agent...");
        Agent agent = new Agent(mib);

        agent.Start();

        Console.WriteLine("✅ [Agent] Ready and listening for requests.");
        Console.WriteLine("Pressione ENTER para sair...");
        Console.ReadLine();
    }
}