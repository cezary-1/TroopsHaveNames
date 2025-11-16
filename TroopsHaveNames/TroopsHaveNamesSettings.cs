using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using TaleWorlds.Localization;

namespace TroopsHaveNames
{
    public sealed class TroopsHaveNamesSettings : AttributeGlobalSettings<TroopsHaveNamesSettings>
    {
        public override string Id => "TroopsHaveNames";
        public override string DisplayName => new TextObject("{=THN_TITLE}Troops Have Names").ToString();
        public override string FolderName => "TroopsHaveNamesSettings";
        public override string FormatType => "json";

        [SettingPropertyBool(
            "{=THN_Affect}Affect Others Also?",
            Order = 0, RequireRestart = false,
            HintText = "{=THN_Affect_H}If true, all non-heroes (not only troops) will have names (Default: trie)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool Affect { get; set; } = true;

        [SettingPropertyBool(
            "{=THN_IdName}Keep Native After Name",
            Order = 0, RequireRestart = false,
            HintText = "{=THN_IdName_H}If true, troops affected will have after their names their native names. (e.g Pethros [Imperial Recruit]) (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool IdName { get; set; } = false;

        [SettingPropertyBool(
            "{=THN_OnlyAlly}Only Ally",
            Order = 1, RequireRestart = false,
            HintText = "{=THN_OnlyAlly_H}If true, only ally troops will have names (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool OnlyAlly { get; set; } = false;

        [SettingPropertyBool(
            "{=THN_OnlyBattles}Only In Battles",
            Order = 2, RequireRestart = false,
            HintText = "{=THN_OnlyBattles_H}If true, only troops in battles will have names. (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool OnlyBattle { get; set; } = false;

        [SettingPropertyBool(
            "{=THN_OnlyNonBattles}Only In Non-Battles",
            Order = 3, RequireRestart = false,
            HintText = "{=THN_OnlyNonBattles_H}If true, only troops in non-battles will have names. (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool OnlyNonBattle { get; set; } = false;

        [SettingPropertyBool(
            "{=THN_OnlyNoble}Only Noble Troops",
            Order = 4, RequireRestart = false,
            HintText = "{=THN_OnlyNoble_H}If true, only noble troops will have names. (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool OnlyNoble { get; set; } = false;

        [SettingPropertyInteger(
            "{=THN_Tier}Min Tier", 0, 10,
            Order = 5, RequireRestart = false,
            HintText = "{=THN_Tier_H}At this tier troops will have names (Default: 0)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public int Tier { get; set; } = 0;

        [SettingPropertyBool(
            "{=THN_WithoutNames}Names For Nameless Cultures",
            Order = 6, RequireRestart = false,
            HintText = "{=THN_WithoutNames_H}If true, troops without names in their cultures will have randomized name across all. (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool WithoutNames { get; set; } = true;

        [SettingPropertyBool(
            "{=THN_HideEnemyNames}Hide Enemy Troops Names?",
            Order = 7, RequireRestart = false,
            HintText = "{=THN_HideEnemyNames_H}If true, enemy troops will have their names hidden. (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool HideEnemyNames { get; set; } = false;


        [SettingPropertyBool(
            "{=THN_Debug}Debug Info (Default: false)",
            Order = 7, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool Debug { get; set; } = false;

        [SettingPropertyBool(
            "{=THN_Surnames}Surnames?",
            Order = 0, RequireRestart = false,
            HintText = "{=THN_Surnames_H}If true, troops will also have surnames. (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_SURNAMES}Surnames")]
        public bool Surnames { get; set; } = true;

        [SettingPropertyBool(
            "{=THN_SurnamesNoble}Only Noble?",
            Order = 1, RequireRestart = false,
            HintText = "{=THN_SurnamesNoble_H}If true, noble troops will only have surnames. (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_SURNAMES}Surnames")]
        public bool SurnamesNoble { get; set; } = false;

        [SettingPropertyInteger(
            "{=THN_SurnamesTier}Min Tier", 0, 10,
            Order = 2, RequireRestart = false,
            HintText = "{=THN_SurnamesTier_H}At this tier troops will have surnames (Default: 0)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_SURNAMES}Surnames")]
        public int SurnamesTier { get; set; } = 0;

    }
}
