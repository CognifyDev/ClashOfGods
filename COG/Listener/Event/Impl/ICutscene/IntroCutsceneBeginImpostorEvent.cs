using Il2CppSystem.Collections.Generic;

namespace COG.Listener.Event.Impl.ICutscene;

public class IntroCutsceneBeginImpostorEvent : IntroCutsceneEvent
{
    private List<PlayerControl> _yourTeam;
    
    public IntroCutsceneBeginImpostorEvent(IntroCutscene introCutscene, List<PlayerControl> yourTeam) : base(introCutscene)
    {
        _yourTeam = yourTeam;
    }

    public void SetYourTeam(List<PlayerControl> yourTeam) => _yourTeam = yourTeam;

    public List<PlayerControl> GetYourTeam() => _yourTeam;
}