namespace MyIndustry.Domain.ValueObjects;

public class Amount
{
    private readonly decimal _value;

    public Amount(decimal value)
    {
        _value = decimal.Round(value, 2); // (18,2) hassasiyet
    }

    public int ToInt()
    {
        return Convert.ToInt32(_value);
    }

    public decimal ToDecimal()
    {
        return _value;
    }

    public override string ToString()
    {
        return _value.ToString("F2");
    }
}