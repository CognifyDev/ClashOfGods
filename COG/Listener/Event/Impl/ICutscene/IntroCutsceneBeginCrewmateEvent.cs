using Il2CppSystem.Collections.Generic;

namespace COG.Listener.Event.Impl.ICutscene;

public class IntroCutsceneBeginCrewmateEvent : IntroCutsceneEvent
{
    private List<PlayerControl> _teamToDisplay;

    public IntroCutsceneBeginCrewmateEvent(IntroCutscene introCutscene, List<PlayerControl> teamToDisplay) :
        base(introCutscene)
    {
        _teamToDisplay = teamToDisplay;
    }

    public void SetTeamToDisplay(List<PlayerControl> teamToDisplay)
    {
        _teamToDisplay = teamToDisplay;
    }

    public List<PlayerControl> GetTeamToDisplay()
    {
        return _teamToDisplay;
    }
}