using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using COG.Role;
using COG.Rpc;
using COG.UI.CustomOption.ValueRules;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using COG.Utils.WinAPI;
using Reactor.Utilities;
using UnityEngine;
using Mode = COG.Utils.WinAPI.OpenFileDialogue.OpenFileMode;

// ReSharper disable InconsistentNaming

namespace COG.UI.CustomOption;

// Code base from
// https://github.com/TheOtherRolesAU/TheOtherRoles/blob/main/TheOtherRoles/Modules/CustomOptions.cs
public sealed class CustomOption
{
    public enum TabType
    {
        General = 0,
        Impostor = 1,
        Neutral = 2,
        Crewmate = 3,
        Addons = 4
    }

    private static int _typeId;

    private int _selection;

    private CustomOption(TabType type, Func<string> nameGetter, IValueRule rule, CustomOption? parent)
    {
        Id = _typeId;
        _typeId++;
        Name = nameGetter;
        ValueRule = rule;
        _selection = rule.DefaultSelection;
        Parent = parent;
        Page = type;
    }

    public static List<CustomOption> Options { get; } = new();

    public int Id { get; }
    public Func<string> Name { get; set; }
    public TabType Page { get; }
    public CustomOption? Parent { get; }
    public object[] Selections => ValueRule.Selections;
    public IValueRule ValueRule { get; }
    public OptionBehaviour? OptionBehaviour { get; set; }
    public BaseGameSetting? VanillaData { get; private set; }

    public int Selection
    {
        get => _selection;
        set
        {
            _selection = value;

            Coroutines.Start(CoRefreshCustomOptions());

            static IEnumerator CoRefreshCustomOptions()
            {
                yield return null;

                if (!GameSettingMenu.Instance)
                    yield break;

                if (GameSettingMenu.Instance.RoleSettingsTab)
                {
                    GameSettingMenu.Instance.RoleSettingsTab.RefreshChildren();

                    foreach (var role in CustomRoleManager.GetManager().GetModRoles().Where(r => r.ShowInOptions))
                    {
                        var roleRoleNumberOption = role.RoleNumberOption;
                        if (roleRoleNumberOption == null) continue;
                        var roleBehaviour = roleRoleNumberOption.OptionBehaviour;
                        if (!roleBehaviour) continue;

                        roleBehaviour!.Cast<RoleOptionSetting>().UpdateValuesAndText(null);

                        Main.Logger.LogDebug("TEXT UPDATE FOR " + role.Name);

                        role.RoleOptions.Where(o => o.OptionBehaviour).ForEach(o =>
                        {
                            var behaviour = o.OptionBehaviour!;
                            if (behaviour.TryGetComponent<NumberOption>(out var numberOption))
                                numberOption.Value = o.GetFloat();
                            else if (behaviour.TryGetComponent<StringOption>(out var stringOption))
                                stringOption.Value = o.Selection;
                            else if (behaviour.TryGetComponent<ToggleOption>(out var toggleOption))
                                toggleOption.CheckMark.enabled = o.GetBool();
                        });
                    }
                }

                Main.Logger.LogDebug("Trying to update setting options");

                if (GameSettingMenu.Instance.GameSettingsTab)
                {
                    GameSettingMenu.Instance.GameSettingsTab.RefreshChildren();
                    GameSettingMenu.Instance.GameSettingsTab.Children?.ForEach(new Action<OptionBehaviour>(o =>
                    {
                        if (TryGetOption(o, out _)) return;

                        if (o.TryGetComponent<NumberOption>(out var numberOption))
                            numberOption.oldValue = int.MinValue;
                        else if (o.TryGetComponent<StringOption>(out var stringOption))
                            stringOption.oldValue = -1;
                        else if (o.TryGetComponent<ToggleOption>(out var toggleOption))
                            toggleOption.oldValue = !toggleOption.CheckMark.enabled;
                        else
                            Main.Logger.LogDebug("Didnt update!");
                    }));
                }
            }

            NotifySettingChange();
        }
    }

    public static CustomOption Of(TabType type, Func<string> nameGetter, IValueRule rule,
        CustomOption? parent = null)
    {
        return new CustomOption(type, nameGetter, rule, parent);
    }

    public CustomOption Register()
    {
        Options.Add(this);
        return this;
    }

    public static bool TryGetOption(OptionBehaviour optionBehaviour, out CustomOption? customOption)
    {
        customOption = Options.FirstOrDefault(o => o.OptionBehaviour == optionBehaviour)!;
        return customOption != null!;
    }

    public static void ShareConfigs(PlayerControl? target = null)
    {
        if (PlayerUtils.GetAllPlayers().Count <= 0 || !AmongUsClient.Instance.AmHost) return;

        var writer = PlayerControl.LocalPlayer.StartRpcImmediately(KnownRpc.ShareOptions,
            target == null ? null : target.ToSingleElementArray());

        writer.WritePacked((uint)Options.Count);
        foreach (var option in Options)
        {
            writer.WritePacked((uint)option.Id);
            writer.WritePacked((uint)option.Selection);
        }

        writer.Finish();
    }

    private static void LoadOptionFromPreset(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            var decoded = StringUtils.DecodeAsBase64(File.ReadAllText(path));
            var lines = decoded.Split("\r\n");

            foreach (var line in lines)
            {
                var optionInfo = line.Split(" ");
                var optionID = optionInfo[0];
                var optionSelection = optionInfo[1];

                var option = Options.FirstOrDefault(o => o.Id.ToString() == optionID);
                if (option == null) continue;
                option.UpdateSelection(int.Parse(optionSelection));
            }
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error loading options: " + e);
        }
    }

    private static void SaveCurrentOption(string path)
    {
        try
        {
            var realPath = path.EndsWith(".cfg") ? path : path + ".cfg";
            var builder = new StringBuilder();

            foreach (var option in Options.OrderBy(o => o.Id))
                builder.Append(option.Id + " " + option.Selection + "\r\n");

            var encoded = StringUtils.EncodeToBase64(builder.ToString());
            File.WriteAllText(realPath, encoded);
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error saving options: " + e);
        }
    }

    public static void SavePresetWithDialogue()
    {
        var file = OpenFileDialogue.Display(Mode.Save, "Preset File(*.cfg)\0*.cfg\0\0");
        if (file.FilePath is null or "") return;
        SaveCurrentOption(file.FilePath);
    }

    public static void LoadPresetWithDialogue()
    {
        var file = OpenFileDialogue.Display(Mode.Open, "Preset File(*.cfg)\0*.cfg\0\0");
        if (file.FilePath is null or "") return;
        LoadOptionFromPreset(file.FilePath);
    }

    public dynamic GetDynamicValue()
    {
        return ValueRule.Selections[Selection];
    }

    public bool GetBool()
    {
        if (ValueRule is BoolOptionValueRule rule)
            return rule.Selections[Selection];
        throw new NotSupportedException();
    }

    public float GetFloat()
    {
        if (ValueRule is FloatOptionValueRule rule)
            return rule.Selections[Selection];
        if (ValueRule is IntOptionValueRule intRule)
        {
            Main.Logger.LogWarning($"Trying to get float value from int option: {Name()}({Id})");
            return intRule.Selections[Selection];
        }

        throw new NotSupportedException();
    }

    public int GetInt()
    {
        if (ValueRule is IntOptionValueRule rule)
        {
            return rule.Selections[Selection];
        }

        if (ValueRule is FloatOptionValueRule floatRule)
        {
            Main.Logger.LogWarning($"Trying to get int value from float option: {Name()}({Id})");
            return (int)floatRule.Selections[Selection];
        }

        throw new NotSupportedException();
    }

    public string GetString()
    {
        if (ValueRule is StringOptionValueRule rule)
            return rule.Selections[Selection];
        throw new NotSupportedException();
    }

    // Option changes
    public void UpdateSelection(int newSelection)
    {
        Selection = newSelection;
        ShareOptionChange(newSelection);
    }

    public void UpdateSelection(object newValue)
    {
        var index = ValueRule.Selections.ToList().IndexOf(newValue);
        if (index != -1) UpdateSelection(index);
    }

    private void ShareOptionChange(int newSelection)
    {
        PlayerControl.LocalPlayer.StartRpcImmediately(KnownRpc.UpdateOption)
            .WritePacked(Id)
            .WritePacked(newSelection)
            .Finish();
    }

    private void NotifySettingChange()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!OptionBehaviour) return;

        var role = CustomRoleManager.GetManager().GetRoles().FirstOrDefault(r => r.AllOptions.Contains(this));

        if (role == null)
        {
            var valueText = "";

            if (OptionBehaviour!.GetComponent<StringOption>())
                valueText = GetString();
            else if (OptionBehaviour!.GetComponent<ToggleOption>())
                valueText = TranslationController.Instance.GetString(GetBool()
                    ? StringNames.SettingsOn
                    : StringNames.SettingsOff);
            else if (OptionBehaviour!.GetComponent<NumberOption>())
                valueText = OptionBehaviour!.Data.GetValueString(OptionBehaviour.GetFloat());

            var item = TranslationController.Instance.GetString(StringNames.LobbyChangeSettingNotification,
                $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{Name()}</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\"> " + valueText + " </font>"
            );
            GameUtils.RpcNotifySettingChange(int.MinValue + Id, item);
        }
        else if (OptionBehaviour is RoleOptionSetting setting)
        {
            var roleName = setting.Role.TeamType switch
            {
                RoleTeamTypes.Crewmate => role.GetColorName(),
                RoleTeamTypes.Impostor => role.Name.Color(Palette.ImpostorRed),
                _ => role.Name.Color(Color.grey)
            };
            var item = TranslationController.Instance.GetString(StringNames.LobbyChangeSettingNotificationRole,
                $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{roleName}</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + setting.roleMaxCount + "</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + setting.roleChance + "%"
            );

            GameUtils.RpcNotifySettingChange(int.MaxValue - role.Id, item);
        }
        else
        {
            var roleName = role.CampType switch
            {
                CampType.Crewmate => role.GetColorName(),
                CampType.Impostor => role.Name.Color(Palette.ImpostorRed),
                _ => role.Name.Color(Color.grey)
            };

            var valueText = "";

            if (OptionBehaviour!
                .GetComponent<
                    StringOption>()) // It's strange that using (OptionBehaviour is TargetType) expression is useless
                valueText = GetString();
            else if (OptionBehaviour!.GetComponent<ToggleOption>())
                valueText = TranslationController.Instance.GetString(GetBool()
                    ? StringNames.SettingsOn
                    : StringNames.SettingsOff);
            else if (OptionBehaviour!.GetComponent<NumberOption>())
                valueText = OptionBehaviour!.Data.GetValueString(OptionBehaviour.GetFloat());

            var item = TranslationController.Instance.GetString(StringNames.LobbyChangeSettingNotification,
                $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{roleName}: {Name()} </font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\"> " + valueText + " </font>"
            );

            GameUtils.RpcNotifySettingChange(int.MaxValue - Id, item);
        }
    }

    public BaseGameSetting? ToVanillaOptionData()
    {
        var rule = ValueRule;
        if (rule is BoolOptionValueRule)
        {
            var checkboxGameSetting = ScriptableObject.CreateInstance<CheckboxGameSetting>();
            checkboxGameSetting.Type = OptionTypes.Checkbox;
            return VanillaData = checkboxGameSetting;
        }

        if (rule is IntOptionValueRule intOptionValueRule)
        {
            var intGameSetting = ScriptableObject.CreateInstance<IntGameSetting>();
            intGameSetting.Type = OptionTypes.Int;
            intGameSetting.Value = GetInt();
            intGameSetting.Increment = intOptionValueRule.Step;
            intGameSetting.ValidRange = new IntRange(intOptionValueRule.Min, intOptionValueRule.Max);
            intGameSetting.ZeroIsInfinity = false;
            intGameSetting.SuffixType = intOptionValueRule.SuffixType;
            intGameSetting.FormatString = "";
            return VanillaData = intGameSetting;
        }

        if (rule is FloatOptionValueRule floatOptionValueRule)
        {
            var floatGameSetting = ScriptableObject.CreateInstance<FloatGameSetting>();
            floatGameSetting.Type = OptionTypes.Float;
            floatGameSetting.Value = GetFloat();
            floatGameSetting.Increment = floatOptionValueRule.Step;
            floatGameSetting.ValidRange = new FloatRange(floatOptionValueRule.Min, floatOptionValueRule.Max);
            floatGameSetting.ZeroIsInfinity = false;
            floatGameSetting.SuffixType = floatOptionValueRule.SuffixType;
            floatGameSetting.FormatString = "";
            return VanillaData = floatGameSetting;
        }

        if (rule is StringOptionValueRule stringOptionValueRule)
        {
            var stringGameSetting = ScriptableObject.CreateInstance<StringGameSetting>();
            stringGameSetting.Type = OptionTypes.String;
            stringGameSetting.Index = Selection;
            stringGameSetting.Values = new StringNames[stringOptionValueRule.Selections.Length];
            return VanillaData = stringGameSetting;
        }

        return null;
    }
}