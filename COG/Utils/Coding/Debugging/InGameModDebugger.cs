using COG.Game.Events;
using COG.Role;
using Reactor.Utilities.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace COG.Utils.Coding.Debugging;

#nullable disable
// This class is for viewing custom roles in UnityExplorer in game so as to debug easily
[RegisterInIl2Cpp]
public class InGameModDebugger : MonoBehaviour
{
    public List<CustomRole> CustomRoles => CustomRoleManager.GetManager().GetRoles();
    public List<DeadPlayer> DeadPlayers => DeadPlayer.DeadPlayers;
    public EventRecorder EventRecorder => EventRecorder.Instance;

    public static InGameModDebugger Instance { get; private set; }

    void Start()
    {
        Instance = this;
        name = nameof(InGameModDebugger);

        DontDestroyOnLoad(this);
    }

    public void ForceSetRole(byte playerId, string name)
    {
        var player = PlayerUtils.GetPlayerById(playerId);
        var role = CustomRoleManager.GetManager().GetRoleByClassName(name, true);
        player.RpcSetCustomRole(role);
    }

    public void ForceSetRole(string name) => ForceSetRole(PlayerControl.LocalPlayer.PlayerId, name);
}
#nullable restore
