using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.RuleBook;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class StatIconManager
{
    private static readonly Dictionary<SpecialStatIcon, SpecialTriggeredAbility> BASE_GAME_ABILITIES = new()
    {
        { SpecialStatIcon.Ants, SpecialTriggeredAbility.Ant },
        { SpecialStatIcon.Bell, SpecialTriggeredAbility.BellProximity },
        { SpecialStatIcon.Bones, SpecialTriggeredAbility.Lammergeier },
        { SpecialStatIcon.CardsInHand, SpecialTriggeredAbility.CardsInHand },
        { SpecialStatIcon.GreenGems, SpecialTriggeredAbility.GreenMage },
        { SpecialStatIcon.Mirror, SpecialTriggeredAbility.Mirror },
        { SpecialStatIcon.SacrificesThisTurn, SpecialTriggeredAbility.SacrificesThisTurn }
    };

    public class FullStatIcon
    {
        public readonly SpecialStatIcon Id;
        public readonly SpecialTriggeredAbility AbilityId;
        public readonly StatIconInfo Info;
        public readonly Type VariableStatBehavior;

        /// <summary>
        /// Tracks all rulebook redirects that this ability's description will have. Explanation of the variables is as follows:
        /// Key (string): the text that will be recoloured to indicate that it's clickable.
        /// Tuple.Item1 (PageRangeType): the type of page the redirect will go to. Use PageRangeType.Unique if you want to redirect to a custom rulebook page using its pageId.
        /// Tuple.Item2 (Color): the colour the Key text will be recoloured to.
        /// Tuple.Item3 (string): the id that the API will match against to find the redirect page. Eg, for ability redirects this will be the Ability id as a string.
        /// </summary>
        public Dictionary<string, RuleBookManager.RedirectInfo> RulebookDescriptionRedirects = new();

        public FullStatIcon(SpecialStatIcon id, SpecialTriggeredAbility abilityId, StatIconInfo info, Type variableStatBehavior)
        {
            Id = id;
            AbilityId = abilityId;
            Info = info;
            VariableStatBehavior = variableStatBehavior;

            TypeManager.Add(id.ToString(), variableStatBehavior);
        }
    }

    public readonly static ReadOnlyCollection<FullStatIcon> BaseGameStatIcons = new(GenBaseGameStatIconList());
    private readonly static ObservableCollection<FullStatIcon> NewStatIcons = new();

    public static List<FullStatIcon> AllStatIcons { get; private set; } = BaseGameStatIcons.ToList();
    public static List<StatIconInfo> AllStatIconInfos { get; private set; } = BaseGameStatIcons.Select(x => x.Info).ToList();

    static StatIconManager()
    {
        NewStatIcons.CollectionChanged += static (_, _) =>
        {
            AllStatIcons = BaseGameStatIcons.Concat(NewStatIcons).ToList();
            AllStatIconInfos = AllStatIcons.Select(x => x.Info).ToList();
        };

        // Let's help people fix the most common mistake with special stat icons
        CardManager.ModifyCardList += delegate (List<CardInfo> cards)
        {
            foreach (CardInfo card in cards)
            {
                if (card.specialStatIcon != SpecialStatIcon.None)
                {
                    FullStatIcon icon = AllStatIcons.FirstOrDefault(i => i.Id == card.specialStatIcon);
                    if (icon != null && icon.AbilityId != SpecialTriggeredAbility.None)
                    {
                        card.specialAbilities = new(card.specialAbilities);
                        card.AddSpecialAbilities(icon.AbilityId);
                    }
                }
            }
            return cards;
        };
    }

    private static List<FullStatIcon> GenBaseGameStatIconList()
    {
        List<FullStatIcon> baseGame = new();
        Assembly gameAsm = typeof(AbilityInfo).Assembly;
        foreach (var staticon in Resources.LoadAll<StatIconInfo>("Data/staticons"))
        {
            string name = staticon.iconType.ToString();
            SpecialTriggeredAbility ab = BASE_GAME_ABILITIES.ContainsKey(staticon.iconType) ? BASE_GAME_ABILITIES[staticon.iconType] : (SpecialTriggeredAbility)Enum.Parse(typeof(SpecialTriggeredAbility), staticon.iconType.ToString());
            baseGame.Add(new FullStatIcon(staticon.iconType, ab, staticon, gameAsm.GetType($"DiskCardGame.{name}")));
        }
        return baseGame;
    }

    public static FullStatIcon Add(string guid, StatIconInfo info, Type behavior)
    {
        // Register the special ability at the same time
        SpecialTriggeredAbility abilityId = SpecialTriggeredAbilityManager.Add(guid, info.rulebookName, behavior).Id;

        FullStatIcon full = new(GuidManager.GetEnumValue<SpecialStatIcon>(guid, info.rulebookName), abilityId, info, behavior);
        full.Info.iconType = full.Id;
        info.name = $"{guid}_{info.rulebookName}";
        NewStatIcons.Add(full);
        return full;
    }

    public static StatIconInfo New(string guid, string rulebookName, string rulebookDescription, Type behavior)
    {
        StatIconInfo info = ScriptableObject.CreateInstance<StatIconInfo>();
        info.rulebookDescription = rulebookDescription;
        info.rulebookName = rulebookName;

        return Add(guid, info, behavior).Info;
    }

    public static void Remove(SpecialStatIcon id) => NewStatIcons.Remove(NewStatIcons.FirstOrDefault(x => x.Id == id));
    public static void Remove(FullStatIcon ability) => NewStatIcons.Remove(ability);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StatIconInfo), nameof(StatIconInfo.LoadAbilityData))]
    private static void AbilityLoadPrefix()
    {
        StatIconInfo.allIconInfo = AllStatIconInfos;
    }

    [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData", new Type[] { typeof(AbilityMetaCategory) })]
    [HarmonyPostfix]
    private static void AddNewStatIconsToRuleBook(AbilityMetaCategory metaCategory, RuleBookInfo __instance, ref List<RuleBookPageInfo> __result)
    {
        if (NewStatIcons.Count > 0)
        {
            foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
            {
                if (pageRangeInfo.type == PageRangeType.StatIcons)
                {
                    int insertPosition = __result.FindLastIndex(rbi => rbi.pagePrefab == pageRangeInfo.rangePrefab) + 1;
                    int curPageNum = (int)SpecialStatIcon.NUM_ICONS;
                    foreach (FullStatIcon fab in NewStatIcons.Where(x => __instance.StatIconShouldBeAdded((int)x.Id, metaCategory)))
                    {
                        RuleBookPageInfo info = new();
                        info.pagePrefab = pageRangeInfo.rangePrefab;
                        info.headerText = string.Format(Localization.Translate("APPENDIX XII, SUBSECTION VII - VARIABLE STATS {0}"), curPageNum);
                        __instance.FillStatIconPage(info, pageRangeInfo, (int)fab.Id);
                        __result.Insert(insertPosition, info);
                        curPageNum += 1;
                        insertPosition += 1;
                    }
                }
            }
        }
    }
}
