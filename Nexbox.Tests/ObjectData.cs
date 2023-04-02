namespace Nexbox.Tests;

public class ObjectData
{
    public static int z => new Random().Next(0, 500);
    
    public int a
    {
        get => b;
        set => b = value;
    }
    private int b;

    public int getB() => b;
    
    public ObjectData(){}
    public ObjectData(int b) => this.b = b;
}