using integra_dados.Models.DataProtocol;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace integra_dados.Models;

public class Registry
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
    
    public int CodeId { get; set; }
    
    [JsonIgnore]
    public string? Uri { get; set; }
    
    public StatusVariable? Status { get; set; }
    
    public string Ip { get; set; }
    public int Porta { get; set; }
    [JsonIgnore]
    public string? Nome { get; set; }
    public string? Fonte { get; set; }
    public string? TipoDado { get; set; }
    public string? DescricaoPontoGeografico { get; set; }
    public string? Observacao { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
    public string? TopicoBroker { get; set; }
    public bool OtimizarPublicacaoBroker { get; set; }
    public int FreqLeituraSeg { get; set; }
    public Protocol? Protocol { get; set; }
    public WriteProtocol? WriteProtocol { get; set; }

    [System.Text.Json.Serialization.JsonIgnore] // Ignora ao serializar para JSON, equivalente ao @Transient
    public long LastRead { get; set; } = 0;

    [System.Text.Json.Serialization.JsonIgnore]
    public object? LastRegisteredValue { get; set; }

    public void SetIdSistema()
    {
        CodeId = Util.Util.GenerateRandomNumber();
    }

    public bool IsTimeToSendMessage(int freqLeitura)
    {
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (LastRead == 0 || now >= LastRead + (freqLeitura * 1000))
        {
            LastRead = now;
            return true;
        }

        return false;
    }

    public void UpdateRegistry(int? onOffSwitchValue)
    {
        const int VALUE_NOT_VALID = -1;
        UpdateDate();

        if (onOffSwitchValue == VALUE_NOT_VALID || onOffSwitchValue == null)
        {
            UpgradeStatusToUnavailable();
        }
        else
        {
            UpgradeStatusToAvailable();
        }
    }
    
    public void UpdateRegistry(double? onOffSwitchValue)
    {
        const double VALUE_NOT_VALID = -1.0;
        UpdateDate();

        if (onOffSwitchValue == VALUE_NOT_VALID || onOffSwitchValue == null)
        {
            UpgradeStatusToUnavailable();
        }
        else
        {
            UpgradeStatusToAvailable();
        }
    }
    
    public void UpdateRegistry(bool? onOffSwitchValue)
    {
        UpdateDate();

        if (onOffSwitchValue == null)
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

    public void UpgradeStatusToAvailable()
    {
        Status = StatusVariable.AVAILABLE;
    }

    public void UpgradeStatusToUnavailable()
    {
        Status = StatusVariable.UNAVAILABLE;
    }

    public bool ShouldSendToBroker(Object? actualRegisteredValue)
    {
        if (OtimizarPublicacaoBroker && actualRegisteredValue != null)
        {
            if (actualRegisteredValue.Equals(LastRegisteredValue))
            {
                return false;
            }

            LastRegisteredValue = actualRegisteredValue;
        }
        else
        {
            return false;
        }

        return true;
    }
}