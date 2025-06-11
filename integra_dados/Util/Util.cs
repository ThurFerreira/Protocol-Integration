namespace integra_dados.Util;

public class Util
{
    public static int GenerateRandomNumber()
    {
        return new Random().Next(1, 2^32);
    }
}