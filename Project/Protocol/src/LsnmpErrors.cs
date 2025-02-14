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
        UnknownError = 11 // Erro genérico
    }

    public Dictionary<Type, LsnmpErrors.ErrorCode> errorMap = new Dictionary<Type, LsnmpErrors.ErrorCode>()
    {
        { typeof(InvalidIIDException), LsnmpErrors.ErrorCode.InvalidIID },
        { typeof(DecodingException), LsnmpErrors.ErrorCode.DecodingError },
        { typeof(EncodingException), LsnmpErrors.ErrorCode.EncodingError },
        { typeof(UnsupportedValueException), LsnmpErrors.ErrorCode.UnsupportedValue },
        { typeof(ReadOnlyException), LsnmpErrors.ErrorCode.ReadOnlyError },
        { typeof(Exception), LsnmpErrors.ErrorCode.UnknownError } // Erro genérico
    };
}