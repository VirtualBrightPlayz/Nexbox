namespace Nexbox.Tests;

public class TestClass
{
    public int AddNumbers(int a, int b) => a + b;

    public ObjectData GetObjectData(int o) => new ObjectData
    {
        a = o
    };
}