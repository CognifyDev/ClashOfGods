using Il2CppSystem.Collections;

namespace COG.Listener.Event.Impl.ICutscene;

public class IntroCutsceneShowRoleEvent : IntroCutsceneEvent
{
    private IEnumerator _result;

    public IntroCutsceneShowRoleEvent(IntroCutscene introCutscene, IEnumerator result) : base(introCutscene)
    {
        _result = result;
    }

    public void SetResult(IEnumerator result)
    {
        _result = result;
    }

    public IEnumerator GetResult()
    {
        return _result;
    }
}