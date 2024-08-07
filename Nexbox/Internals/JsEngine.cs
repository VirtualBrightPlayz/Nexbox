using Nexbox.Interpreters;

namespace Nexbox.Internals;

internal class JsEngine : IScriptEngine
{
    internal JavaScriptInterpreter e;

    internal JsEngine(JavaScriptInterpreter e) => this.e = e;
}