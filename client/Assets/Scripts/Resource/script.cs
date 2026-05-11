
using System.Collections.Generic;
using XLua;

namespace game.resource
{
    public class Script
    {
        private const string LuaCompatibilityBootstrap = @"
floor = math.floor
ceil = math.ceil
abs = math.abs
sqrt = math.sqrt
random = math.random
min = math.min
max = math.max
mod = math.fmod
format = string.format
getn = function(t)
    if t == nil then
        return 0
    end
    return #t
end
";

        [LuaCallCSharp]
        private static byte[] CustomLoaderFunction(ref string moduleName)
        {
            return Game.Resource(moduleName).Get<resource.Buffer>();
        }

        ////////////////////////////////////////////////////////////////////////////////

        private LuaEnv luaenv;
        private readonly bool shared;
        private readonly Dictionary<string, LuaFunction> functionCache = new();
        private static readonly Dictionary<string, Script> sharedScripts = new();
        private static resource.Buffer onCreateBuffer;
        private static bool onCreateBufferChecked;

        ////////////////////////////////////////////////////////////////////////////////

        public Script(string scriptPath)
            : this(scriptPath, false)
        {
        }

        private Script(string scriptPath, bool shared)
        {
            this.shared = shared;
            this.Initialize();
            this.Load(scriptPath);
        }

        ////////////////////////////////////////////////////////////////////////////////

        public static Script GetShared(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath))
            {
                return null;
            }

            if (sharedScripts.TryGetValue(scriptPath, out Script cachedScript))
            {
                return cachedScript;
            }

            Script script = new Script(scriptPath, true);
            sharedScripts[scriptPath] = script;
            return script;
        }

        private void Initialize()
        {
            this.luaenv = new LuaEnv();
            this.luaenv.AddLoader(Script.CustomLoaderFunction);

            resource.Buffer onCreateBuffer = Script.GetOnCreateBuffer();
            if (onCreateBuffer != null && onCreateBuffer.size > 0)
            {
                this.luaenv.DoString(onCreateBuffer, resource.mapping.Script.Library.onCreate);
            }

            this.luaenv.DoString(LuaCompatibilityBootstrap, "jx1.lua.compat");
        }

        private static resource.Buffer GetOnCreateBuffer()
        {
            if (onCreateBufferChecked)
            {
                return onCreateBuffer;
            }

            onCreateBuffer = Game.Resource(resource.mapping.Script.Library.onCreate).Get<resource.Buffer>();
            onCreateBufferChecked = true;
            return onCreateBuffer;
        }

        private void Load(string scriptPath)
        {
            resource.Buffer scriptBuffer = Game.Resource(scriptPath).Get<resource.Buffer>();
            if (scriptBuffer == null || scriptBuffer.size <= 0)
            {
                UnityEngine.Debug.LogWarning("Lua script missing or empty: " + scriptPath);
                return;
            }

            this.luaenv.DoString(scriptBuffer, scriptPath);
        }

        public void Release()
        {
            if (this.shared)
            {
                return;
            }

            this.functionCache.Clear();
            this.luaenv?.Dispose();
            this.luaenv = null;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public Typename CallFunction<Typename>(string functionName, params object[] args)
        {
            LuaFunction luaFunction = this.GetFunction(functionName);
            if (luaFunction == null)
            {
                return default;
            }

            object[] result = luaFunction.Call(args);

            if(result != null && result.Length > 0)
            {
                object value = result[0];
                if (value == null)
                {
                    return default;
                }

                if (value is Typename typedValue)
                {
                    return typedValue;
                }

                return (Typename)System.Convert.ChangeType(value, typeof(Typename));
            }

            return default;
        }

        public void CallFunction(string functionName, params object[] args)
        {
            this.GetFunction(functionName)?.Call(args);
        }

        private LuaFunction GetFunction(string functionName)
        {
            if (this.luaenv == null || string.IsNullOrEmpty(functionName))
            {
                return null;
            }

            if (this.functionCache.TryGetValue(functionName, out LuaFunction cachedFunction))
            {
                return cachedFunction;
            }

            LuaFunction luaFunction = this.luaenv.Global.Get<LuaFunction>(functionName);
            if (luaFunction != null)
            {
                this.functionCache[functionName] = luaFunction;
            }

            return luaFunction;
        }
    }
}
