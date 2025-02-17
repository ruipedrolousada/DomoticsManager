public class IID
{
    public int Structure { get; set; }
    public int Object { get; set; }
    public int? FirstIndex { get; set; }
    public int? SecondIndex { get; set; }

    public IID(string iidString)
    {
        var parts = iidString.Split('.');
        Structure = int.Parse(parts[0]);
        Object = int.Parse(parts[1]);
        FirstIndex = parts.Length > 2 ? int.Parse(parts[2]) : (int?)null;
        SecondIndex = parts.Length > 3 ? int.Parse(parts[3]) : (int?)null;
    }
}