public class InvalidIIDException : Exception
{
    public InvalidIIDException(string message) : base(message) { }
}

public class InvalidMessageType : Exception
{
    public InvalidMessageType(string message) : base(message) { }
}

public class DecodingException : Exception
{
    public DecodingException(string message) : base(message) { }
}

public class EncodingException : Exception
{
    public EncodingException(string message) : base(message) { }
}

public class UnsupportedValueException : Exception
{
    public UnsupportedValueException(string message) : base(message) { }
}

public class InvalidTagException : Exception
{
    public InvalidTagException(string message) : base(message) { }
}

public class EmptyMibException : Exception
{
    public EmptyMibException(string message) : base(message) { }
}

public class EmptySensorsTableException : Exception
{
    public EmptySensorsTableException(string message) : base(message) { }
}

public class EmptyActuatorsTableException : Exception
{
    public EmptyActuatorsTableException(string message) : base(message) { }
}

public class ReadOnlyException : Exception
{
    public ReadOnlyException(string message) : base(message) { }
}

public class IIDsAndValuesMismatchException : Exception
{
    public IIDsAndValuesMismatchException(string message) : base(message) { }
}