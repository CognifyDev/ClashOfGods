namespace COG.Patch;

public static class ServerPath
{
    [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.LoadServers))]
    [HarmonyPostfix]
    static void AddServer(ServerManager __instance)
    {
        IRegionInfo[] regionInfos = new IRegionInfo[]
        {
            CreateRegionInfo("au-sh.pafyx.top", "梦服上海(新)", 22000),
            CreateRegionInfo("au-as.duikbo.at", "Modded Asia (MAS)", 443, true),
            CreateRegionInfo("www.aumods.xyz", "Modded NA (MNA)", 443, true),
            CreateRegionInfo("au-eu.duikbo.at", "Modded EU (MEU)", 443, true)
        };
        
        regionInfos.Do(__instance.AddOrUpdateRegion);
    }

    static IRegionInfo CreateRegionInfo(string name, string ip, ushort port, bool isHttps = false)
    {
        var serverIp = isHttps ? "https://" : "http://" + ip;
        var serverInfo = new ServerInfo(name, serverIp, port, false);
        ServerInfo[] ServerInfo = { serverInfo };
        return new StaticHttpRegionInfo(name, StringNames.NoTranslation, ip, ServerInfo).Cast<IRegionInfo>();
    }
}