namespace integra_dados.Models;

public class Registry
{
    public string Uri { get; set; }
    public int Status { get; set; }
    public int IdSistema { get; set; }
    public string Nome { get; set; }
    public string Fonte { get; set; }
    public string TipoDado { get; set; }
    public string DescricaoPontoGeografico { get; set; }
    public string Observacao { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
    public string TopicoBroker { get; set; }
    public bool OtimizarPublicacaoBroker { get; set; }

    [System.Text.Json.Serialization.JsonIgnore] // Ignora ao serializar para JSON, equivalente ao @Transient
    public int Counter { get; set; } = 0;

    [System.Text.Json.Serialization.JsonIgnore]
    public object LastRegisteredValue { get; set; }
    
    public void SetIdSistema()
    {
        IdSistema = Util.Util.GenerateRandomNumber();
    }
    
    public bool IsTimeToSendMessage(int freqLeitura)
    {
        if (Counter == freqLeitura)
        {
            ResetCounter();
            return true;
        }
        return false;
    }
    
    public void ResetCounter()
    {
        Counter = 0;
    }

    public void IncrementCounter()
    {
        Counter++;
    }
    
    public void UpdateRegistry(int onOffSwitchValue)
    {
        const int VALUE_NOT_VALID = -1;
        UpdateDate();
    
        if (onOffSwitchValue == VALUE_NOT_VALID)
        {
            UpgradeStatusToUnavailable();
        }
        else
        {
            UpgradeStatusToAvailable();
        }
    }

    private void UpdateDate()
    {
        UltimaAtualizacao = DateTime.Now;
    }
    
    public void UpgradeStatusToAvailable () {
        Status = (int) StatusVariable.AVAILABLE;
    }
    public void UpgradeStatusToUnavailable () {
        Status = (int) StatusVariable.UNAVAILABLE;
    }
    
    public bool ShouldSendToBroker(Object actualRegisteredValue) {
        if (OtimizarPublicacaoBroker) {
            if (actualRegisteredValue.Equals(LastRegisteredValue)) {
                return false;
            } LastRegisteredValue = actualRegisteredValue;
        } return true;
    }
}