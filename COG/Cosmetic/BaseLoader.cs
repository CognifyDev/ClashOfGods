using System.Diagnostics.CodeAnalysis;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace COG.Cosmetics;


public abstract class BaseLoader
{
    public abstract string GetCosmeticId(string name);
    
    public abstract void InstallCosmetics(ReferenceData refData);

    public abstract void LoadCosmetics(string directory, string author = "");

    public abstract bool LocateCosmetic(
        string id,
        string type,
        [NotNullWhen(true)] out Il2CppSystem.Type? il2CppType);

    public abstract bool ProvideCosmetic(ProvideHandle handle, string id, string type);
}
