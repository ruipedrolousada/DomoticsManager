public class MIBManager
{
    private MIB _mib;
    private DateTime lastBootTime;

    public MIBManager(MIB mib)
    {
        _mib = mib;
        lastBootTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Obtém ou define um valor na MIB com base no IID fornecido.
    /// </summary>
    /// <param name="iidString">O IID no formato "Structure.Object.Index".</param>
    /// <param name="value">O valor a ser definido (opcional para operações de get).</param>
    /// <returns>O valor atual do objeto (após a operação de set, se aplicável).</returns>
    public string GetOrSetValue(string iidString, string value = null)
    {
        var iid = new IID(iidString);

        if (iid.Structure == 1) // Device Group
        {
            return HandleDeviceGroup(iid, value);
        }
        else if (iid.Structure == 2) // Sensors Table
        {
            return HandleSensorsTable(iid, value);
        }
        else if (iid.Structure == 3) // Actuators Table
        {
            return HandleActuatorsTable(iid, value);
        }
        else
        {
            throw new InvalidIIDException("[MIB Manager] Invalid IID structure.");
        }
    }

    private string HandleDeviceGroup(IID iid, string value)
    {
        if (iid.Object == 0) // Número de objetos no grupo
        {
            return "10"; // Device Group tem 10 objetos
        }

        if (iid.FirstIndex.HasValue || iid.SecondIndex.HasValue)
        {
            throw new InvalidIIDException("[MIB Manager] Invalid IID range for device group.");
        }

        switch (iid.Object)
        {
            case 1: // device.id
                if (value != null) throw new ReadOnlyException("[MIB Manager] device.id is read-only.");
                return _mib.Device.Id;

            case 2: // device.type
                if (value != null) throw new ReadOnlyException("[MIB Manager] device.type is read-only.");
                return _mib.Device.Type;

            case 3: // device.beaconRate
                if (value != null) _mib.Device.BeaconRate = int.Parse(value);
                return _mib.Device.BeaconRate.ToString();

            case 4: // device.nSensors
                if (value != null) throw new ReadOnlyException("[MIB Manager] device.nSensors is read-only.");
                return _mib.Device.NSensors.ToString();

            case 5: // device.nActuators
                if (value != null) throw new ReadOnlyException("[MIB Manager] device.nActuators is read-only.");
                return _mib.Device.NActuators.ToString();

            case 6: // device.dateAndTime
                if (value != null) _mib.Device.DateAndTime = DateTime.Parse(value);
                return _mib.Device.DateAndTime.ToString();

            case 7: // device.upTime
                if (value != null) throw new ReadOnlyException("[MIB Manager] device.upTime is read-only.");
                return _mib.Device.UpTime.ToString();

            case 8: // device.lastTimeUpdated
                if (value != null) throw new ReadOnlyException("[MIB Manager] device.lastTimeUpdated is read-only.");
                return _mib.Device.LastTimeUpdated.ToString();

            case 9: // device.operationalStatus
                if (value != null) throw new ReadOnlyException("[MIB Manager] device.operationalStatus is read-only.");
                return _mib.Device.OperationalStatus.ToString();

            case 10: // device.reset
                if (value != null && value == "1")
                {
                    _mib.Device.Reset = int.Parse(value);
                    HandleReset();
                } 
                return _mib.Device.Reset.ToString();

            default:
                throw new InvalidIIDException("[MIB Manager]$ Invalid IID object for device group.");
        }
    }

    private string HandleSensorsTable(IID iid, string value)
    {
        if (iid.Object == 0) // Número de objetos na tabela
        {
            return "6"; // Sensors Table tem 6 objetos por linha
        }

        if (iid.FirstIndex == 0 && iid.SecondIndex == 0) // Retorna todos os valores da coluna X (object)
        {
            var listValues = new List<string>();

            foreach (var sensor in _mib.Sensors.Sensors)
            {
                listValues.Add(ProcessSensorObject(sensor, iid.Object, value));
            }

            return string.Join(",", listValues);
        }

        if (iid.FirstIndex == 0 && !iid.SecondIndex.HasValue)
        {
            return _mib.Sensors.Sensors.Count.ToString(); // Retorna o número total de linhas (sensores)
        }

        // Verifica se a tabela de atuadores está vazia
        if (_mib.Sensors.Sensors.Count == 0)
        {
            throw new EmptySensorsTableException("[MIB Manager] Sensors table is empty."); //mudar para emptysensortable exception
        }

        // Se o FirstIndex não estiver presente, trata-se da primeira entrada da tabela
        int firstIndex = iid.FirstIndex ?? 1; // Defaults to 1 if FirstIndex is null

        // Verifica se o índice é válido
        if (firstIndex <= 0 || firstIndex > _mib.Sensors.Sensors.Count)
            throw new InvalidIIDException("[MIB Manager]$ Invalid sensor index.");

        // Se houver um segundo índice, trata como um range
        if (iid.SecondIndex.HasValue)
        {
            if (iid.SecondIndex < firstIndex || iid.SecondIndex > _mib.Sensors.Sensors.Count)
                throw new InvalidIIDException("[MIB Manager]$ Invalid second index.");

            // Processa todas as instâncias no range
            var listValues = new List<string>();

            for (int i = firstIndex; i <= iid.SecondIndex.Value; i++)
            {
                var sensor = _mib.Sensors.Sensors[i - 1];

                listValues.Add( ProcessSensorObject(sensor, iid.Object, value));
            }

            Console.WriteLine("✅ [MIB Manager] IIDs range processed successfully.\n");

            return string.Join(",", listValues);
        }
        else
        {
            // Processa uma única instância
            var sensor = _mib.Sensors.Sensors[firstIndex - 1];
            return ProcessSensorObject(sensor, iid.Object, value);
        }
    }

    private string ProcessSensorObject(Sensor sensor, int objectId, string value)
    {
        switch (objectId)
        {
            case 1: // sensors.id
                if (value != null) throw new ReadOnlyException("[MIB Manager] sensors.id is read-only.");
                return sensor.Id;

            case 2: // sensors.type
                if (value != null) throw new ReadOnlyException("[MIB Manager] sensors.type is read-only.");
                return sensor.Type;

            case 3: // sensors.status
                if (value != null)
                {
                    sensor.Status = int.Parse(value);
                    UpdateLastTimeUpdated();
                }
                    
                return sensor.Status.ToString();

            case 4: // sensors.minValue
                if (value != null) throw new ReadOnlyException("[MIB Manager] sensors.minValue is read-only.");
                return sensor.MinValue.ToString();

            case 5: // sensors.maxValue
                if (value != null) throw new ReadOnlyException("[MIB Manager] sensors.maxValue is read-only.");
                return sensor.MaxValue.ToString();

            case 6: // sensors.lastSamplingTime
                if (value != null) throw new ReadOnlyException("[MIB Manager] sensors.lastSamplingTime is read-only.");
                return sensor.LastSamplingTime.ToString();

            default:
                throw new InvalidIIDException("[MIB Manager] Invalid IID object for sensors table.");
        }
    }

    private string HandleActuatorsTable(IID iid, string value)
    {
        if (iid.Object == 0) // Número de objetos na tabela
        {
            return "6"; // Actuators Table tem 6 objetos por linha
        }

        if (iid.FirstIndex == 0 && iid.SecondIndex == 0) // Retorna todos os valores da coluna X (object)
        {
            var listValues = new List<string>();

            foreach (var sensor in _mib.Sensors.Sensors)
            {
                listValues.Add(ProcessSensorObject(sensor, iid.Object, value));
            }

            return string.Join(",", listValues);
        }

        if (iid.FirstIndex == 0 && !iid.SecondIndex.HasValue)
        {
            return _mib.Actuators.Actuators.Count.ToString(); // Retorna o número total de linhas (sensores)
        }

        // Verifica se a tabela de atuadores está vazia
        if (_mib.Actuators.Actuators.Count == 0)
        {
            throw new EmptyActuatorsTableException("[MIB Manager] Actuators table is empty.");
        }

        // Se o FirstIndex não estiver presente, trata-se da primeira entrada da tabela
        int firstIndex = iid.FirstIndex ?? 1; // Defaults to 1 if FirstIndex is null

        // Verifica se o índice é válido
        if (firstIndex <= 0 || firstIndex > _mib.Sensors.Sensors.Count)
            throw new InvalidIIDException("[MIB Manager]$ Invalid sensor index.");

        // Se houver um segundo índice, trata como um range
        if (iid.SecondIndex.HasValue)
        {
            if (iid.SecondIndex < iid.FirstIndex || iid.SecondIndex > _mib.Actuators.Actuators.Count)
                throw new InvalidIIDException("[MIB Manager] Invalid second index.");

            // Processa todas as instâncias no range
            var listValues = new List<string>();

            for (int i = firstIndex; i <= iid.SecondIndex.Value; i++)
            {
                var actuator = _mib.Actuators.Actuators[i - 1];
                listValues.Add(ProcessActuatorObject(actuator, iid.Object, value));
            }

            Console.WriteLine("✅ [MIB Manager] IIDs range processed successfully.\n");

            return string.Join(",", listValues);
        }
        else
        {
            // Processa uma única instância
            var actuator = _mib.Actuators.Actuators[firstIndex - 1];
            return ProcessActuatorObject(actuator, iid.Object, value);
        }
    }

    private string ProcessActuatorObject(Actuator actuator, int objectId, string value)
    {
        switch (objectId)
        {
            case 1: // actuators.id
                if (value != null) throw new ReadOnlyException("[MIB Manager] actuators.id is read-only.");
                return actuator.Id;

            case 2: // actuators.type
                if (value != null) throw new ReadOnlyException("[MIB Manager] actuators.type is read-only.");
                return actuator.Type;

            case 3: // actuators.status
                if (value != null)
                {
                    actuator.Status = int.Parse(value);
                    UpdateLastTimeUpdated();
                } 
                return actuator.Status.ToString();

            case 4: // actuators.minValue
                if (value != null) throw new ReadOnlyException("[MIB Manager] actuators.minValue is read-only.");
                return actuator.MinValue.ToString();

            case 5: // actuators.maxValue
                if (value != null) throw new ReadOnlyException("[MIB Manager] actuators.maxValue is read-only.");
                return actuator.MaxValue.ToString();

            case 6: // actuators.lastControlTime
                if (value != null) throw new ReadOnlyException("[MIB Manager] actuators.lastControlTime is read-only.");
                return actuator.LastControlTime.ToString();

            default:
                throw new InvalidIIDException("[MIB Manager] Invalid IID object for actuators table.");
        }
    } 

    public TimeSpan GetUpTime()
    {
        return DateTime.UtcNow - lastBootTime; // Retorna a diferença entre o tempo atual e o tempo do último boot
    }

    public void UpdateLastTimeUpdated()
    {
        _mib.Device.LastTimeUpdated = DateTime.UtcNow;
    }

    public void HandleReset()
    {
        if (_mib.Device.Reset == 1)
        {
            // Log the reset action
            Console.WriteLine("[MIB Manager] Resetting the device...\n");

            // Reset the device time and uptime
            lastBootTime = DateTime.UtcNow;
            _mib.Device.UpTime = TimeSpan.Zero;
            _mib.Device.Reset = 0;  // Reset state to 0 after action

            // Turn off all actuators
            foreach (var actuator in _mib.Actuators.Actuators)
            {
                actuator.Status = 0;  // Turn off actuator (0 = off)
                actuator.LastControlTime = DateTime.UtcNow;  // Update last control time
            }

            // Optionally, disable sensors or reset their states
            foreach (var sensor in _mib.Sensors.Sensors)
            {
                sensor.Status = 0;  // Reset sensor (0 = inactive or standby)
                // Optionally, reset data as well:
                sensor.LastSamplingTime = DateTime.UtcNow;  // Example of resetting the last sampling time
            }
            Console.WriteLine("[MIB Manager] Device reseted successfuly\n");
        }
    }
}