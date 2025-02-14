using System;
using System.Collections.Generic;

public class Program
{
    public static void Main(string[] args)
    {
        Manager manager = new Manager();

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("🌐 Domotics Manager");
        DisplayDeviceTable();

        while (true)
        {
            Console.WriteLine("\n📋 **Main Menu**");
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine("1️⃣  Send Get Request");
            Console.WriteLine("2️⃣  Send Set Request");
            Console.WriteLine("3️⃣  View Help (IID Rules)");
            Console.WriteLine("4️⃣  Exit");
            Console.WriteLine("══════════════════════════════════════════");
            Console.Write("Please choose an option (1-4): ");

            string choice = Console.ReadLine();

            Console.WriteLine("");

            switch (choice)
            {
                case "1":
                    HandleGetRequest(manager);
                    break;
                case "2":
                    HandleSetRequest(manager);
                    break;
                case "3":
                    DisplayHelp2();
                    break;
                case "4":
                    Console.WriteLine("👋 Exiting...");
                    return;
                default:
                    Console.WriteLine("❌ Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static void DisplayDeviceTable()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ L-SNMPvS Device, Sensors, and Actuators Information                                          ║");
        Console.WriteLine("╠═══════╦═══════════════════════════╦══════════╦═══╦═══════════════════════════════════════════╣");
        Console.WriteLine("║ IID   ║ Name                      ║ Type     ║A  ║ Description                               ║");
        Console.WriteLine("╠═══════╬═══════════════════════════╬══════════╬═══╬═══════════════════════════════════════════╣");
        
        // Device Group
        PrintRow("1.1", "device.id", "String", "R", "Device Identifier (MAC Address)");
        PrintRow("1.2", "device.type", "String", "R", "Device Type");
        PrintRow("1.3", "device.beaconRate", "Integer", "RW", "Beacon Message Frequency");
        PrintRow("1.4", "device.nSensors", "Integer", "R", "Number of Sensors");
        PrintRow("1.5", "device.nActuators", "Integer", "R", "Number of Actuators");
        PrintRow("1.6", "device.dateAndTime", "Timestamp", "RW", "System Date and Time");
        PrintRow("1.7", "device.upTime", "Timestamp", "R", "elapsed time since boot");
        PrintRow("1.8", "device.lastTimeUpdated", "Timestamp", "R", "time of last object update");
        PrintRow("1.9", "device.operationalStatus", "Integer", "R", "0-standby,1-normal,2-fault");
        PrintRow("1.10", "device.reset", "Integer", "RW", "0-default,1-reset");
        
        // Sensors
        Console.WriteLine("╠═══════╩═══════════════════════════╩══════════╩═══╩═══════════════════════════════════════════╣");
        Console.WriteLine("║ Sensors Table (IID 2)                                                                        ║");
        Console.WriteLine("╠═══════╦═══════════════════════════╦══════════╦═══╦═══════════════════════════════════════════╣");
        // PrintRow("2.1.1", "Light_Sala", "Light", "R", "Light Sensor (Sala)");
        // PrintRow("2.1.2", "Light_Cozinha", "Light", "R", "Light Sensor (Cozinha)");
        // PrintRow("2.1.3", "AC_Quarto", "Temp", "R", "Temperature Sensor (Quarto)");
        PrintRow("2.1", "sensors.id", "String", "R", "Sensor Identifier");
        PrintRow("2.2", "sensors.type", "String", "R", "Type of Sensor (e.g., Light, Temperature)");
        PrintRow("2.3", "sensors.status", "Integer", "R", "Last sampled value (%)");
        PrintRow("2.4", "sensors.minValue", "Integer", "R", "Minimum sensor value");
        PrintRow("2.5", "sensors.maxValue", "Integer", "R", "Maximum sensor value");
        PrintRow("2.6", "sensors.lastSamplingTime", "Timestamp", "R", "Time of last sample");
        
        // Actuators
        Console.WriteLine("╠═══════╩═══════════════════════════╩══════════╩═══╩═══════════════════════════════════════════╣");
        Console.WriteLine("║ Actuators Table (IID 3)                                                                      ║");
        Console.WriteLine("╠═══════╦═══════════════════════════╦══════════╦═══╦═══════════════════════════════════════════╣");
        // PrintRow("3.1.1", "Light_Sala", "Light", "RW", "Light Actuator (Sala)");
        // PrintRow("3.1.2", "Light_Cozinha", "Light", "RW", "Light Actuator (Cozinha)");
        // PrintRow("3.1.3", "AC_Quarto", "Temp", "RW", "Temperature Actuator (Quarto)");
        PrintRow("3.1", "actuators.id", "String", "R", "Actuator Identifier");
        PrintRow("3.2", "actuators.type", "String", "R", "Type of Actuator (e.g., Temperature)");
        PrintRow("3.3", "actuators.status", "Integer", "RW", "Configured value (must be between min/max)");
        PrintRow("3.4", "actuators.minValue", "Integer", "R", "Minimum actuator value");
        PrintRow("3.5", "actuators.maxValue", "Integer", "R", "Maximum actuator value");
        PrintRow("3.6", "actuators.lastControlTime", "Timestamp", "R", "Last control operation timestamp");
        Console.WriteLine("╚═══════╩═══════════════════════════╩══════════╩═══╩═══════════════════════════════════════════╝\n");

    }

    private static void PrintRow(string iid, string name, string type, string access, string description)
    {
        Console.WriteLine($"║ {iid,-6}║ {name,-26}║ {type,-9}║ {access,-2}║ {description,-42}║");
    }

    private static void HandleGetRequest(Manager manager)
    {
        Console.Write("Enter IIDs (comma-separated, e.g., 1.1,2.1.1,3.1.1): ");
        string[] iids = Console.ReadLine().Split(',');
        Console.Write("");
        manager.SendGetRequest(iids);

        Console.WriteLine("✅ GetRequest sent successfully\n");
    }

    private static void HandleSetRequest(Manager manager)
    {
        Console.Write("Enter IIDs (comma-separated, e.g., 3.1.1,3.1.2): ");
        string[] iids = Console.ReadLine().Split(',');
        
        Console.Write("Enter values (comma-separated, e.g., 3.1.1.75,3.1.2.50): ");
        string[] values = Console.ReadLine().Split(',');

        if (iids.Length != values.Length)
        {
            Console.WriteLine("❌ Number of IIDs and values must be the same.");
            return;
        }

        var dictionary = new Dictionary<string, string>();
        for (int i = 0; i < iids.Length; i++)
        {
            dictionary[iids[i]] = values[i];
        }
        manager.SendSetRequest(dictionary);
        Console.WriteLine("✅ SetRequest sent successfully.\n");
    }

    private static void DisplayHelp()
    {
        Console.WriteLine("");
        Console.WriteLine("📖 IID Rules in L-SNMPvS:");
        Console.WriteLine("══════════════════════════════════════════════════════════════════════════════════════════");
        Console.WriteLine("1️⃣ Format: Structure.Object.1ºindex?.2ºindex?");
        Console.WriteLine("   - Structure: Identifies Group (1) or Table (2=Sensors, 3=Actuators)");
        Console.WriteLine("   - Object: Attribute of the sensor/actuator");
        Console.WriteLine("   - 1º index (optional): Identifies specific sensor/actuator in the table");
        Console.WriteLine("   - 2º index (optional): Identifies a range of sensors/actuators (from the 1ºindex to 2ºindex)");
        Console.WriteLine();

        Console.WriteLine("2️⃣ Groups (1.X)");
        Console.WriteLine("   - Direct attributes without indexed instances");
        Console.WriteLine("   - Example: 1.3 → Beacon Rate");
        Console.WriteLine();

        Console.WriteLine("3️⃣ Tables (2.N.X or 3.N.X)");
        Console.WriteLine("   - N represents the attribute of that sensor/actuator");
        Console.WriteLine("   - X represents the specific sensor/actuator");
        Console.WriteLine("   - Example: 2.1.3 → Attribute 1 of Sensor 3");
        Console.WriteLine();

        Console.WriteLine("4️⃣ Special Cases");
        Console.WriteLine("   - Object = 0 → Returns the number of attributes in group/table");
        Console.WriteLine("   - N = 0 → Returns the total number of sensors/actuators");
        Console.WriteLine("══════════════════════════════════════════════════════════════════════════════════════════\n");
    }

    private static void DisplayHelp2()
    {
        Console.WriteLine("📖 **IID Rules in L-SNMPvS**");
        Console.WriteLine("╔══════════════════════════════════════════════╦════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Description                                  ║ Details                                                            ║");
        Console.WriteLine("╠══════════════════════════════════════════════╬════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ 1 Format: Structure.Object.1ºindex?.2ºindex? ║- Structure: Identifies Group (1) or Table (2=Sensors, 3=Actuators) ║");
        Console.WriteLine("║                                              ║ - Object: Attribute of the sensor/actuator                         ║");
        Console.WriteLine("║                                              ║ - 1º index (optional): Identifies specific sensor/actuator         ║");
        Console.WriteLine("║                                              ║ - 2º index (optional): Identifies a range of sensors/actuators     ║");
        Console.WriteLine("╠══════════════════════════════════════════════╬════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ 2 Groups (1.X)                               ║ - Direct attributes without indexed instances                      ║");
        Console.WriteLine("║                                              ║ - Example: 1.3 → Beacon Rate                                       ║");
        Console.WriteLine("╠══════════════════════════════════════════════╬════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ 3 Tables (2.N.X or 3.N.X)                    ║ - N represents the attribute of that sensor/actuator               ║");
        Console.WriteLine("║                                              ║ - X represents the specific sensor/actuator                        ║");
        Console.WriteLine("║                                              ║ - Example: 2.1.3 → Attribute 1 of Sensor 3                         ║");
        Console.WriteLine("╠══════════════════════════════════════════════╬════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ 4 Special Cases                              ║ - Object = 0 → Returns the number of attributes in group/table     ║");
        Console.WriteLine("║                                              ║ - N = 0 → Returns the total number of sensors/actuators            ║");
        Console.WriteLine("╚══════════════════════════════════════════════╩════════════════════════════════════════════════════════════════════╝\n");
    }

}
