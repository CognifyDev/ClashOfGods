using System;
using System.Collections.Generic;
using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Role;
using COG.Role.Impl.SubRole;
using COG.Rpc;
using COG.Utils;
using COG.Utils.Coding;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace COG.UI.Hud.Meeting;

[ShitCode]
public class GuesserButton
{
    private const string GuessButtonName = "GuessButton";
    private readonly PlayerVoteArea _area;
    private readonly GameObject _guessButton;
    private readonly Guesser _guesser;

    private readonly NetworkedPlayerInfo? _target;
    private PassiveButton? _confirmButton;

    private GameObject? _container;
    private int _page = 1;
    private GameObject? _roleButtonContainer;
    private readonly IEnumerable<CustomRole> _roles;
    private SpriteRenderer? _selectedButton;
    private CustomRole? _selectedRole;

    /// <summary>
    ///     赌怪按钮
    /// </summary>
    public GuesserButton(GameObject template, PlayerVoteArea playerVoteArea, Guesser guesser, MeetingHud meetingHud, IEnumerable<CustomRole> roles)
    {
        _guessButton = Object.Instantiate(template, playerVoteArea.transform);
        _guesser = guesser;
        _target = PlayerUtils.GetPlayerById(playerVoteArea.TargetPlayerId)!.Data;
        _area = playerVoteArea;
        _guessButton.name = GuessButtonName;
        _guessButton.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
        var renderer = _guessButton.GetComponent<SpriteRenderer>();
        renderer.sprite = ResourceUtils.LoadSprite(ResourceConstant.GuessButton, 150F);
        var passive = _guessButton.GetComponent<PassiveButton>();
        passive.StopAllCoroutines();
        passive.OnClick.AddListener(new Action(() => OpenGuessUI(meetingHud)));
        _roles = roles;
    }
    
    public void Register() => Buttons.Add(this);

    public static List<GuesserButton> Buttons { get; } = [];

    public void OpenGuessUI(MeetingHud meetingHud)
    {
        if (meetingHud.state != MeetingHud.VoteStates.NotVoted) return; //在等待投票过程中，弃票按钮不显示，导致无法实例化页面取消按钮
        _guesser.CurrentGuessing = _target;

        ResetState();
        MeetingHud.Instance.ButtonParent.gameObject.SetActive(false);

        _container = new GameObject("GuesserButtons");
        _container.transform.SetParent(MeetingHud.Instance.ButtonParent.transform.parent);
        _container.transform.localPosition = Vector3.zero;
        _container.transform.localScale = Vector3.one;

        var titleTemplate = MeetingHud.Instance.TitleText;
        var title = Object.Instantiate(titleTemplate, _container.transform);
        title.transform.localPosition = new Vector3(0, 2.2f, -1);
        title.TryDestroyComponent<TextTranslatorTMP>();
        title.text = LanguageConfig.Instance.GetHandler("role.sub-roles.guesser.in-game").GetString("ui-title");

        SetUpTargetArea();
        SetUpRolePage(1);
        SetUpCancelButton();
    }

    public void SetUpTargetArea()
    {
        var newArea = Object.Instantiate(_area, _container!.transform);
        newArea.transform.localScale = Vector3.one;
        newArea.transform.localPosition = new Vector3(0, 1.5f, 0);
        newArea.transform.FindChild(GuessButtonName).gameObject.TryDestroy();
    }

    public void SetUpRolePage(int page)
    {
        var roleButtonTemplate = Object.Instantiate(_area, _container!.transform).gameObject;
        roleButtonTemplate.SetActive(false);

        SetUpRoleButtons(roleButtonTemplate, page);
    }

    public void SetUpRoleButtons(GameObject roleButtonTemplate, int page)
    {
        const float initialX = -3f;
        const float deltaX = 2f;
        const int maxRow = 4;

        const float initialY = 0.8f;
        const float deltaY = -0.5f;
        const int maxLine = 6;

        const int maxPerPage = maxRow * maxLine;

        _page = page;

        var roles = new Stack<CustomRole>(_roles.Skip((page - 1) * maxPerPage).Take(maxPerPage));
        _roleButtonContainer = new GameObject("RoleButtonContainer");
        _roleButtonContainer.transform.SetParent(_container!.transform);
        _roleButtonContainer.transform.localPosition = Vector3.zero;
        _roleButtonContainer.transform.localScale = Vector3.one;

        for (var i = 0; i < maxRow; i++)
        {
            var x = initialX + i * deltaX;

            for (var j = 0; j < maxLine; j++)
            {
                var y = initialY + j * deltaY;
                Main.Logger.LogInfo($"开启职业数量: {roles.ToArray().Count()}");
                if (!roles.Any()) break;

                var position = new Vector3(x, y, 0);
                var roleButton = Object.Instantiate(roleButtonTemplate, _roleButtonContainer.transform);
                var nameText = roleButton.transform.FindChild("NameText").GetComponent<TextMeshPro>();
                var currentRole = roles.Pop();

                roleButton.transform.localPosition = position;
                roleButton.name = currentRole.GetNormalName();
                nameText.text = currentRole.GetColorName();

                nameText.transform.localPosition = Vector3.zero;
                nameText.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                roleButton.transform.FindChild("votePlayerBase").FindChild("ControllerHighlight")
                    .GetComponentInChildren<SpriteRenderer>().enabled = false;
                roleButton.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                roleButton.transform.GetAllChildren()
                    .DoIf(c => !new[] { "MaskArea", "votePlayerBase", "NameText" }.Contains(c.name),
                        c => c.gameObject.TryDestroy());

                roleButton.SetActive(false);
                roleButton.TryDestroyComponent<PlayerVoteArea>();
                roleButton.TryDestroyComponent<VoteSpreader>();

                var votePlayerBase = roleButton.transform.FindChild("votePlayerBase");
                var highlight = votePlayerBase.FindChild("ControllerHighlight").GetComponent<SpriteRenderer>();
                highlight.enabled = false;
                var passive = votePlayerBase.GetComponent<PassiveButton>();
                passive.OnClick = new Button.ButtonClickedEvent();
                passive.OnClick.AddListener(new Action(() =>
                {
                    if (_selectedButton)
                        _selectedButton!.enabled = false;

                    _selectedButton = highlight;
                    _selectedRole = currentRole;
                    _selectedButton.enabled = true;

                    Main.Logger.LogInfo($"Selected role: {_selectedRole.GetNormalName()}");

                    if (!_confirmButton)
                        SetUpConfirmButton();
                }));
                passive.OnMouseOut = new UnityEvent();
                passive.OnMouseOver = new UnityEvent();
                passive.OnMouseOut.AddListener(new Action(() =>
                    highlight.enabled = _selectedButton == highlight));
                passive.OnMouseOver.AddListener(new Action(() =>
                    highlight.enabled = true));

                roleButton.SetActive(true);
            }
        }

        SetUpPageButton(page > 1, roles.Any());
    }

    public void SetUpPageButton(bool previousPage, bool nextPage)
    {
        if (previousPage)
            CreateBottomButton(LanguageConfig.Instance.PreviousPage, new Vector3(-2.5f, -2.2f, 0f), () =>
            {
                _roleButtonContainer.TryDestroy();
                SetUpRolePage(_page - 1);
            });
        if (nextPage)
            CreateBottomButton(LanguageConfig.Instance.NextPage, new Vector3(2.5f, -2.2f, 0f), () =>
            {
                _roleButtonContainer.TryDestroy();
                SetUpRolePage(_page + 1);
            });
    }

    public void SetUpCancelButton()
    {
        CreateBottomButton(LanguageConfig.Instance.Cancel, new Vector3(-3.6f, -2.2f, 0f), CloseGuessUI);
    }

    public void SetUpConfirmButton()
    {
        _confirmButton = CreateBottomButton(LanguageConfig.Instance.Confirm, new Vector3(3.6f, -2.2f, 0f), DestroyAll);
    }

    public void ConfirmRole(CustomRole role)
    {
        if (!_target) return;
        if(_target.GetMainRole() == _selectedRole && !_target.IsDead)
        {
            
        }
        MeetingHud.Instance.playerStates.ForEach(x => x.gameObject.SetActive(true));
    }
    public static void GuessPlayer(PlayerControl guesser , PlayerControl target)
    {
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
        if (PlayerControl.LocalPlayer.PlayerId == target.PlayerId)
        {
            PlayerUtils.PlayKillAnimation(guesser.Data, target.Data);
        }
    }
    public static void RpcGuessPlayer(PlayerControl guesser, PlayerControl target)
    {
        var writer = PlayerControl.LocalPlayer.StartRpcImmediately(KnownRpc.GuessPlayer);
        writer.Write(guesser.PlayerId);
        writer.Write(target.PlayerId);
        writer.Finish();
    }

    public PassiveButton CreateBottomButton(string text, Vector3 localPosition, Action onClick)
    {
        var button = Object.Instantiate(MeetingHud.Instance.SkipVoteButton, _container!.transform);
        button.transform.localPosition = localPosition;
        button.name = text;

        var passive = button.GetComponent<PassiveButton>();
        passive.StopAllCoroutines();

        var highlight = passive.transform.FindChild("ControllerHighlight").GetComponent<SpriteRenderer>();
        highlight.enabled = false;

        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener(onClick);
        passive.OnMouseOut = new UnityEvent();
        passive.OnMouseOver = new UnityEvent();
        passive.OnMouseOut.AddListener(new Action(() => highlight.enabled = false));
        passive.OnMouseOver.AddListener(new Action(() => highlight.enabled = true));

        var textMesh = button.GetComponentInChildren<TextMeshPro>();
        textMesh.TryDestroyComponent<TextTranslatorTMP>();
        textMesh.text = text;

        return passive;
    }

    public void CloseGuessUI()
    {
        MeetingHud.Instance.ButtonParent.gameObject.SetActive(true);
        _guesser.CurrentGuessing = null;
        ResetState();
        ResetRoleArea();
    }


    [Todo("Sync in EventRecorder for every player")]
    private void Guess()
    {
        if (!_target) return;

        PlayerControl.LocalPlayer.RpcMurderAdvanced(new AdvancedKillOptions(false,
            new KillAnimationOptions(true, PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => !p.AmOwner), // Dont show animtion to guesser
                _target!, _target!), _target!.Object));
        _guesser.GuessedTime++;
    }

    public void ResetState()
    {
        _container.TryDestroy();
        _selectedButton = null;
        _selectedRole = null;
        _confirmButton.TryDestroy();
        _roleButtonContainer.TryDestroy();
        _roleButtonContainer = null;
        //_area.TryDestroyGameObject(); // <!> 销毁_area会导致MeetingHud报错
    }

    public void ResetRoleArea()
    {
        _area.TryDestroyGameObject();
    }

    public static void DestroyAll()
    {
        Buttons.ForEach(b =>
        {
            b.ResetState();
            b.ResetRoleArea();
            b._guessButton.TryDestroy();
        });
        Buttons.Clear();
    }
}