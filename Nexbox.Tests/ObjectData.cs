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

    public Dictionary<string, float> values = new()
    {
        ["a"] = 1.00f,
        ["b"] = 0.50f,
        ["c"] = 0.00f
    };

    public int getB() => b;
    
    public ObjectData(){}
    public ObjectData(int b) => this.b = b;
}