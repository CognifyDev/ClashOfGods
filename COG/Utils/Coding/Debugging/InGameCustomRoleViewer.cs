using COG.Role;
using Reactor.Utilities.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace COG.Utils.Coding.Debugging;

#nullable disable
// This class is for viewing custom roles in UnityExplorer in game so as to debug easily
[RegisterInIl2Cpp]
public class InGameCustomRoleViewer : MonoBehaviour
{
    public List<CustomRole> CustomRoles => CustomRoleManager.GetManager().GetRoles();

    public static InGameCustomRoleViewer Instance { get; private set; }

    void Start()
    {
        Instance = this;
        name = nameof(InGameCustomRoleViewer);

        DontDestroyOnLoad(this);
    }
}
#nullable restore
