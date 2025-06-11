namespace integra_dados.Models.Response.Api;

public class ApiResponse
{
    private DateTime date;
    private float value;
    private float[] arrayValue;
    
    public ApiResponse(DateTime date, float[] arrayValue) {
        this.date = date;
        this.arrayValue = arrayValue;
    }

    public ApiResponse(DateTime date, float value) {
        this.date = date;
        this.value = value;
    }
}