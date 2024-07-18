using COG.UI.CustomOption;
using COG.Utils;
using System.Linq;
using System.Text;

namespace COG.Command.Impl;

public class OptionCommand : Command
{
    public OptionCommand() : base("option")
    {
        HostOnly = true;
    }

    public override bool OnExecute(PlayerControl player, string[] args) // TODO: Better float range option info
    {
        if (args.Length == 0) return true;
        var operation = args.FirstOrDefault();
        switch (operation)
        {
            case "show":
                {
                    StringBuilder optionBuilder = new("Current Options:");
                    foreach (var option in CustomOption.Options.Where(o => o != null))
                    {
                        optionBuilder.Append("\nId: ").Append(option!.ID).Append(' ').Append(option.Name)
                            .Append(": ");
                        optionBuilder.Append('(');

                        int i = 0;
                        option.Selections.ForEach(s =>
                        {
                            string selected = s.ToString()!;
                            string selectionFullText = option.Selection == i ? $"<b>{selected}</b>" : selected;
                            optionBuilder.Append(i + 1).Append(": ").Append(selectionFullText);
                            i++;
                            if (option.Selections.Length != i) optionBuilder.Append(", ");
                        });

                        optionBuilder.Append(')');
                    }

                    GameUtils.SendSystemMessage(optionBuilder.ToString());
                }
                break;
            case "set":
                {
                    if (!int.TryParse(args[1], out var id)) return true;

                    var option = CustomOption.Options.Where(o => o != null).FirstOrDefault(o => o!.ID == id);
                    if (option == null) return true;

                    if (!int.TryParse(args[2], out var selection)) return true;
                    option.UpdateSelection(selection);

                    GameUtils.SendSystemMessage($"The selection of {option.Name} has set to {option.Selections[selection]}.");
                }
                break;
            case "share":
                {
                    CustomOption.ShareConfigs();
                    GameUtils.SendSystemMessage("Successfully shared!");
                }
                break;
            default:
            case "help":
                {
                    StringBuilder sb = new("Help: ");
                    sb.Append("/option show - Show all mod options\n")
                        .Append("/option set %id% %selection% - Set the selection of option with %id%.\n")
                        .Append(" %id% : The ID of the option you want to change.\n")
                        .Append(" %selection% : The ID of the new selection.\n")
                        .Append("/option share - Share all option to other clients immediately.");

                    GameUtils.SendSystemMessage(sb.ToString());
                }
                break;
        }
        return false;
    }
}