namespace COG.Patch;

public static class ServerPatch
{
    [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.LoadServers))]
    [HarmonyPostfix]
    private static void AddServer(ServerManager __instance)
    {
        IRegionInfo[] regionInfos =
        [
            CreateRegionInfo("au-sh.pafyx.top", "梦服上海(新)", 22000),
            CreateRegionInfo("au-as.duikbo.at", "Modded Asia (MAS)", 443, true),
            CreateRegionInfo("www.aumods.xyz", "Modded NA (MNA)", 443, true),
            CreateRegionInfo("au-eu.duikbo.at", "Modded EU (MEU)", 443, true)
        ];

        regionInfos.Do(__instance.AddOrUpdateRegion);
    }

    private static IRegionInfo CreateRegionInfo(string ip, string name, ushort port, bool isHttps = false)
    {
        var serverIp = isHttps ? "https://" : "http://" + ip;
        var serverInfo = new ServerInfo(name, serverIp, port, false);
        return new StaticHttpRegionInfo(name, StringNames.NoTranslation, ip, new[] { serverInfo }).Cast<IRegionInfo>();
    }
}