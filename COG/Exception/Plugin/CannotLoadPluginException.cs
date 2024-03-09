namespace COG.Exception.Plugin;

public class CannotLoadPluginException : System.Exception
{
    public CannotLoadPluginException(string message) : base(message)
    {
    }
}