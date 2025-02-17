public class LsnmpErrors
{
    public enum ErrorCode
    {
        NoError = 0,
        DecodingError = 1,
        InvalidTag = 2,
        UnknownMessageType = 3,
        DuplicateMessage = 4,
        InvalidIID = 5,
        UnknownValueType = 6,
        UnsupportedValue = 7,
        ValueListMismatch = 8,
        EncodingError = 9,
        ReadOnlyError = 10,
        IIDsAndValuesMismatch = 11,
        EmptySensorsTableError = 12,
        EmptyActuatorsTableError = 13,
        UnknownError = 12
    }

    public Dictionary<Type, ErrorCode> errorMap = new Dictionary<Type, ErrorCode>()
    {
        { typeof(InvalidIIDException), ErrorCode.InvalidIID },
        { typeof(DecodingException), ErrorCode.DecodingError },
        { typeof(EncodingException), ErrorCode.EncodingError },
        { typeof(UnsupportedValueException), ErrorCode.UnsupportedValue },
        { typeof(ReadOnlyException), ErrorCode.ReadOnlyError },
        { typeof(IIDsAndValuesMismatchException), ErrorCode.IIDsAndValuesMismatch },
        { typeof(InvalidMessageType), ErrorCode.UnknownMessageType },
        { typeof(InvalidTagException), ErrorCode.InvalidTag },
        { typeof(EmptySensorsTableException), ErrorCode.EmptySensorsTableError },
        { typeof(EmptyActuatorsTableException), ErrorCode.EmptyActuatorsTableError },
        { typeof(Exception), ErrorCode.UnknownError }
    };
}