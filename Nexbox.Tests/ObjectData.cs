namespace Nexbox.Tests;

public class ObjectData
{
    public int a
    {
        get => b;
        set => b = value;
    }
    private int b;

    public int getB() => b;
}