public static class Program
{
    public static void Main(string[] args)
    {
        using (MeteorsGame game = new MeteorsGame())
        {
            game.Run();
        }
    }
}

