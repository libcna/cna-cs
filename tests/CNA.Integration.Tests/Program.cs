namespace CNA.Integration.Tests;

/// <summary>
/// The test host never calls this. A test that needs a process of its own -- one without a
/// display -- runs this assembly again with a command and reads what it prints.
/// </summary>
internal static class Program
{
    internal const string ConstructGame = "construct-game";

    private static int Main(string[] args)
    {
        if (args is not [ConstructGame])
        {
            return 2;
        }

        try
        {
            using var game = new Microsoft.Xna.Framework.Game();
            Console.WriteLine("constructed");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.GetType().FullName);
            Console.WriteLine(exception.Message);
        }

        return 0;
    }
}
