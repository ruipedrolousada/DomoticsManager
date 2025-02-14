using System.Text;

public class HelperMethods
{
    public static string DisplayWithNulls(string encodedData)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in encodedData)
        {
            if (c == '\0')
                sb.Append("-");  // Substitui '\0' por um marcador visível
            else
                sb.Append(c);
        }
        return sb.ToString();
    }
}