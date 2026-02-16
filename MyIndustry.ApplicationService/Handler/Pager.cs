namespace MyIndustry.ApplicationService.Handler;

public class Pager
{
    private int _index;

    public int Index
    {
        get => _index;
        set => _index = value < 1 ? 1 : value;
    }
    public int Size { get; set; }

    private Pager()
    {
        
    }
    public Pager(int index, int size)
    {
        Index = index;
        Size = size;
    }
}