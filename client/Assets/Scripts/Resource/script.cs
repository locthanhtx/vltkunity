
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
        private static resource.Buffer onCreateBuffer;
        private static bool onCreateBufferChecked;

        ////////////////////////////////////////////////////////////////////////////////

        public Script(string scriptPath)
        {
            this.Initialize();
            this.Load(scriptPath);
        }

        ////////////////////////////////////////////////////////////////////////////////

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
            this.luaenv?.Dispose();
            this.luaenv = null;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public Typename CallFunction<Typename>(string functionName, params object[] args)
        {
            LuaFunction luaFunction = this.luaenv?.Global.Get<LuaFunction>(functionName);
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
            this.luaenv?.Global.Get<LuaFunction>(functionName)?.Call(args);
        }
    }
}
