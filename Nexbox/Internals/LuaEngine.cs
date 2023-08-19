using Nexbox.Interpreters;

namespace Nexbox.Internals;

internal class LuaEngine
{
    internal LuaInterpreter e;

    internal LuaEngine(LuaInterpreter e) => this.e = e;
}