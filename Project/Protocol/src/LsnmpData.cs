public enum LsnmpDataType
{
    Integer = 'I',
    Timestamp = 'T',
    String = 'S',
    IID = 'D'
}

public class LsnmpData
{
    public LsnmpDataType Type { get; set; }

    //timestamp :
    // 5 (interval, days:hours:mins:secs:ms) or 7 (full date, day:month:year:hours:mins:secs:ms)
    //iid :
    // 2, 3, or 4
    public List<string> Values { get; set; }  // A list to store multiple components (for Timestamp & IID)

    public LsnmpData(LsnmpDataType type, List<string> values)
    {
        Type = type;
        Values = values;
    }

    /// Returns the correct length for encoding based on the data type.
    public string GetEncodedLength()
    {
        return Type switch
        {
            LsnmpDataType.Integer => "1\0",
            LsnmpDataType.String => "1\0",
            LsnmpDataType.Timestamp => $"{Values.Count}\0",  // 5 (interval) or 7 (full date)
            LsnmpDataType.IID => $"{Values.Count}\0",  // 2, 3, or 4
            _ => throw new Exception("Unknown L-SNMPvS Data Type!")
        };
    }

    /// Encodes a single L-SNMPvS data element into the correct string format.
    public string Encode()
    {
        string typeChar = ((char)Type).ToString();
        string length = GetEncodedLength();  // Get the correct length based on Type
        string valueString = string.Join("\0", Values) + "\0";  // Each value ends with '\0'

        return typeChar + "\0" + length + valueString;
    }

    /// Decodes an L-SNMPvS formatted string into an LsnmpData object.
    public static LsnmpData Decode(string data)
    {
        var parts = data.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) throw new FormatException("Invalid L-SNMPvS data format");

        LsnmpDataType type = (LsnmpDataType)parts[0][0]; // First character is the type
        int length = int.Parse(parts[1]); // Second part is the length
        List<string> values = parts.Skip(2).Take(length).ToList(); // Remaining parts are values

        return new LsnmpData(type, values);
    }

    public override string ToString()
    {
        return $"Type: {Type}, Lenght: {GetEncodedLength()}, Values: [{string.Join(", ", Values)}]";
    }
}