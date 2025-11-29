using System.Collections.Generic;
using System.Linq;
using COG.Config.Impl;

namespace COG.Patch;

[HarmonyPatch(typeof(CreditsController))]
//From : Final Suspect
public class CreditsControllerPatch
{
    private static List<CreditsController.CreditStruct> GetModCredits()
    {
        var devList = new List<string>
        {
            $"<size=120%><color=#DD1717>Clash</color> <color=#690B0B>Of</color> <color=#12EC3D>Gods</color></color></size>",
            $"<color=#fffcbe>By</color><color=#AEEEEE>CognifyDev</color>",
            "",
            $"<color=#fffcbe></color><color=#FF0000>{LanguageConfig.Instance.Developer}</color>",
            "- commandf1",
            "- JieGeLovesDengDuaLang",
            "- TianMengLucky",
            "- NikoCat233",
            "- ELinmei",
            "",
            $"<color=#fffcbe></color><color=#0000FF>{LanguageConfig.Instance.Creators}</color>",
            ""//日后统计，我不知道
        };

        var credits = new List<CreditsController.CreditStruct>();

        AddPersonToCredits(devList);
        AddSpcaeToCredits();

        return credits;

        void AddSpcaeToCredits()
        {
            AddTitleToCredits(string.Empty);
        }

        void AddTitleToCredits(string title)
        {
            credits.Add(new CreditsController.CreditStruct
            {
                format = "title",
                columns = new[] { title }
            });
        }

        void AddPersonToCredits(List<string> list)
        {
            foreach (var cols in list.Select(line => line.Split(" - ").ToList()))
            {
                if (cols.Count < 2) cols.Add(string.Empty);
                credits.Add(new CreditsController.CreditStruct
                {
                    format = "person",
                    columns = cols.ToArray()
                });
            }
        }
    }

    [HarmonyPatch(nameof(CreditsController.AddCredit))]
    [HarmonyPrefix]
    public static void AddCreditPrefix(CreditsController __instance,
        [HarmonyArgument(0)] CreditsController.CreditStruct originalCredit)
    {
        if (originalCredit.columns[0] != "logoImage") return;

        foreach (var credit in GetModCredits())
        {
            __instance.AddCredit(credit);
            __instance.AddFormat(credit.format);
        }
    }
}