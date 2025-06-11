namespace integra_dados.Models;

public class SupervisoryRegistry : Registry 
{
    public void SetIdSistema()
    {
        IdSistema = Util.Util.GenerateRandomNumber();
    }
}