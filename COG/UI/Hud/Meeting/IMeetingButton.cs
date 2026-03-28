using COG.Rpc;
using UnityEngine;

namespace COG.UI.Hud.Meeting;

public interface IMeetingButton
{
    Sprite MeetingButtonSprite { get; }

    bool ShouldShowMeetingButton() => false;

    bool ShouldShowMeetingButtonFor(PlayerVoteArea target) => true;

    void OnMeetingButtonClick(PlayerVoteArea target);

    void OnMeetingButtonUpdate(MeetingHud meetingHud) { }
}
