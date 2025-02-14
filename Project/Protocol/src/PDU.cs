using System.Text;

//PDU:S-1-kdk847ufh84jg87g-S-1-S-T-7-12-02-2025-14-30-45-123-S-1-a1b2c3d4e5f6g7h8-I-1-3-D-2-1-2-D-2-2-3-D-3-2-3-1-I-1-2-S-1-100-S-1-200-I-1-0- 

public enum PDUType
{
    GetRequest = 'G',   // Manager requests data
    SetRequest = 'S',   // Manager modifies data
    Notification = 'N', // Agent sends status updates (optional)
    Response = 'R'      // Agent responds to Manager
}
public class PDU
{
    private const string TAG = "kdk847ufh84jg87g"; // Fixed protocol identifier
    public PDUType Type { get; set; }  // Encapsulated as String ('G', 'S', 'N', 'R')
    public string TimeStamp { get; set; }  // Encapsulated as Timestamp
    public string MessageIdentifier { get; set; }  // Encapsulated as String (16-char ID)
    public List<string> IIDList { get; set; } = new();  // List of IIDs
    public List<string> ValueList { get; set; } = new();  // List of Values
    public List<string> ErrorList { get; set; } = new();  // List of Errors

    /// Encodes the PDU into L-SNMPvS format, ensuring correct encapsulation.
    public string Encode()
    {
        try
        {
            StringBuilder sb = new();

            // Tag (Encapsulated as String)
            sb.Append(new LsnmpData(LsnmpDataType.String, new List<string> { TAG }).Encode());

            // Type (Encapsulated as String)
            sb.Append(new LsnmpData(LsnmpDataType.String, new List<string> { ((char)Type).ToString() }).Encode());

            // Time-Stamp (Encapsulated as Timestamp)
            sb.Append(new LsnmpData(LsnmpDataType.Timestamp, TimeStamp.Split(':').ToList()).Encode());

            // Message-Identifier (Encapsulated as String)
            sb.Append(new LsnmpData(LsnmpDataType.String, new List<string> { MessageIdentifier }).Encode());

            // IID-List (Encapsulated properly)
            sb.Append(EncodeList(IIDList, LsnmpDataType.IID));

            // Value-List (Encapsulated properly)
            sb.Append(EncodeList(ValueList, LsnmpDataType.String)); // Assuming all values are strings unless specified

            // Error-List (Encapsulated properly)
            sb.Append(EncodeList(ErrorList, LsnmpDataType.Integer));

            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new EncodingException("[PDU] Error encoding PDU");
        }
    }

    /// Encapsulates each element in LsnmpData before encoding.
    //N-Elements | 1st-Element | 2nd-Element | ... | Nth-Element
    public string EncodeList(List<string> list, LsnmpDataType type)
    {
        StringBuilder sb = new();

        // Primeiro, adiciona o número de elementos com a codificação correta
        sb.Append(new LsnmpData(LsnmpDataType.Integer, new List<string> { list.Count.ToString() }).Encode());

        // Codifica cada item da lista
        foreach (var item in list)
        {
            if (type == LsnmpDataType.IID)
            {
                // Caso o tipo seja IID, vamos dividir o item (por exemplo, "1.2") em componentes
                List<string> iidComponents = item.Split('.').ToList();  // Divide em componentes individuais
                sb.Append(new LsnmpData(LsnmpDataType.IID, iidComponents).Encode()); // Codifica o IID
            }
            else
            {
                // Caso contrário, codificamos como uma string ou outro tipo de dado
                sb.Append(new LsnmpData(type, new List<string> { item }).Encode());
            }
        }

        return sb.ToString();
    }

    // Decodes an L-SNMPvS formatted string into a PDU object.
    public static PDU Decode(string encodedPdu)
    {
        try
        {
            var parts = encodedPdu.Split('\0', StringSplitOptions.RemoveEmptyEntries);
            int index = 0;

            // Decode fixed fields (Tag, Type, Time-Stamp, Message-Identifier)
            var tag = DecodeLsnmpData(parts, ref index).Values[0];
            if (tag != TAG)
                throw new InvalidTagException("[PDU] Invalid protocol tag");

            var type = (PDUType)DecodeLsnmpData(parts, ref index).Values[0][0];
            if (!Enum.IsDefined(typeof(PDUType), type))
                throw new InvalidMessageType("[PDU] Invalid PDU type");

            var timestamp = string.Join(":", DecodeLsnmpData(parts, ref index).Values);
            var messageId = DecodeLsnmpData(parts, ref index).Values[0];

            // Decode IID-List
            var iidCount = int.Parse(DecodeLsnmpData(parts, ref index).Values[0]); // Read header (I\01\0<N>\0)
            var iidList = new List<string>();
            for (int i = 0; i < iidCount; i++)
            {
                var iidData = DecodeLsnmpData(parts, ref index); // Each IID is an LsnmpData object
                iidList.Add(string.Join(".", iidData.Values)); // Join components with '.'
            }

            // Decode Value-List
            var valueCount = int.Parse(DecodeLsnmpData(parts, ref index).Values[0]); // Header (S\01\0<N>\0)
            var valueList = new List<string>();
            for (int i = 0; i < valueCount; i++)
            {
                var valueData = DecodeLsnmpData(parts, ref index); // Each value is a String (S)
                valueList.Add(valueData.Values[0]);
            }

            // Decode Error-List
            var errorCount = int.Parse(DecodeLsnmpData(parts, ref index).Values[0]); // Header (D\01\0<N>\0)
            var errorList = new List<string>();
            for (int i = 0; i < errorCount; i++)
            {
                var errorData = DecodeLsnmpData(parts, ref index); // Each error is an Integer (D)
                errorList.Add(errorData.Values[0]);
            }

            return new PDU
            {
                Type = type,
                TimeStamp = timestamp,
                MessageIdentifier = messageId,
                IIDList = iidList,
                ValueList = valueList,
                ErrorList = errorList
            };
        }
        catch (Exception ex)
        {
            throw new DecodingException("[PDU] Error decoding PDU");
        }
    }

    private static LsnmpData DecodeLsnmpData(string[] parts, ref int index)
    {
        if (index >= parts.Length)
            throw new DecodingException("[PDU] Unexpected end of encoded PDU");

        char type = parts[index++][0];
        int length = int.Parse(parts[index++]);
        List<string> values = new List<string>();

        for (int i = 0; i < length; i++)
        {
            if (index >= parts.Length)
                throw new DecodingException($"[PDU] Expected {length} values for {type}, got {i}");
            values.Add(parts[index++]);
        }

        return new LsnmpData((LsnmpDataType)type, values);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Type: {Type}");
        sb.AppendLine($"TimeStamp: {TimeStamp}");
        sb.AppendLine($"MessageIdentifier: {MessageIdentifier}");
        sb.AppendLine("IIDList: " + string.Join(", ", IIDList));
        sb.AppendLine("ValueList: " + string.Join(", ", ValueList));
        sb.AppendLine("ErrorList: " + string.Join(", ", ErrorList));
        return sb.ToString();
    }
}