
class Game
{
    public static bool Initialize()
    {
#if DEBUG
        System.Diagnostics.Stopwatch stopwatch = new();

        stopwatch.Restart();
        stopwatch.Start();
        if (!game.resource.PackageIni.Initialize())
        {
            stopwatch.Stop();
            UnityEngine.Debug.LogError("game.resource.PackageIni.Initialize failed.");
            return false;
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("game.resource.PackageIni.Initialize --> performance: " + stopwatch.ElapsedMilliseconds + " milliseconds");

        stopwatch.Restart();
        stopwatch.Start();
        game.resource.settings.ObjData.Initialize();

        game.resource.settings.NpcRes.Initialize();
        game.resource.settings.Item.Initialize();
        game.resource.settings.Npcs.Initialize();
        game.resource.settings.Skill.Initialize();
        game.resource.settings.MagicDesc.Initialize();
        game.resource.settings.Music.Initialize();
        stopwatch.Stop();
        UnityEngine.Debug.Log("game.resource.settings.<...>.Initialize --> performance: " + stopwatch.ElapsedMilliseconds + " milliseconds");
#else
        if (!game.resource.PackageIni.Initialize())
        {
            UnityEngine.Debug.LogError("game.resource.PackageIni.Initialize failed.");
            return false;
        }
        game.resource.settings.NpcRes.Initialize();
        game.resource.settings.Item.Initialize();
        game.resource.settings.Npcs.Initialize();
        game.resource.settings.Skill.Initialize();
        game.resource.settings.MagicDesc.Initialize();
        game.resource.settings.Music.Initialize();
#endif
        return true;
    }

    public static void InitializeService()
    {
        System.Diagnostics.Stopwatch stopwatch = new();

        stopwatch.Restart();
        stopwatch.Start();
        game.resource.PackageIni.InitializeService();
        stopwatch.Stop();
        UnityEngine.Debug.Log("game.resource.PackageIni.Initialize --> performance: " + stopwatch.ElapsedMilliseconds + " milliseconds");
    }

    public static game.Resource Resource(string _path) => new game.Resource(_path);

    public class Font
    {
        public static UnityEngine.Font Zero()
        {
            if(game.resource.Cache.Font.font0 == null)
            {
                game.resource.Cache.Font.font0 = UnityEngine.Resources.Load<UnityEngine.Font>("font/0");
            }

            return game.resource.Cache.Font.font0;
        }

        public static UnityEngine.Font One()
        {
            if(game.resource.Cache.Font.font1 == null)
            {
                game.resource.Cache.Font.font1 = UnityEngine.Resources.Load<UnityEngine.Font>("font/1");
            }

            return game.resource.Cache.Font.font1;
        }

        public static UnityEngine.Font Two()
        {
            if(game.resource.Cache.Font.font2 == null)
            {
                game.resource.Cache.Font.font2 = UnityEngine.Resources.Load<UnityEngine.Font>("font/2");
            }

            return game.resource.Cache.Font.font2;
        }
    }
}
