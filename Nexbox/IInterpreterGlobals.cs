namespace Nexbox;

public interface IInterpreterGlobals
{
    public string[] GetGlobals();
    public object GetProperty(string name);
    public void SetProperty(string name, object value);
}