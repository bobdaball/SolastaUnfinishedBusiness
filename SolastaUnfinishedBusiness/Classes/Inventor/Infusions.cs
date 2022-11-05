﻿using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Classes.Inventor.Subclasses;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Utils;
using UnityEngine.AddressableAssets;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;

namespace SolastaUnfinishedBusiness.Classes.Inventor;

internal static class Infusions
{
    private const string ReplicaItemTitleFormat = "Item/&ReplicaItemFormatTitle";
    private const string ReplicaItemTitleDescription = "Item/&ReplicaItemFormatDescription";

    public static readonly FeatureDefinitionFeatureSet ImprovedInfusions = FeatureDefinitionFeatureSetBuilder
        .Create("FeatureSetInfusionUpgrade")
        .SetGuiPresentationNoContent(true)
        .AddToDB();

    public static void Build()
    {
        #region 02 Enhance Focus

        var name = "InfusionEnhanceArcaneFocus";
        var sprite = CustomSprites.GetSprite("EnhanceFocus", Resources.EnhanceFocus, 128);
        var power = BuildInfuseItemPowerInvocation(2, name, sprite, IsFocusOrStaff,
            FeatureDefinitionMagicAffinityBuilder
                //TODO: RAW needs to require attunement
                .Create($"MagicAffinity{name}")
                .SetGuiPresentation(name, Category.Feature, FeatureDefinitionAttackModifiers.AttackModifierMagicWeapon3)
                .SetCastingModifiers(1, dcModifier: 1)
                .AddToDB());

        BuildUpgradedInfuseItemPower(name, power, sprite, IsFocusOrStaff, FeatureDefinitionMagicAffinityBuilder
            //TODO: RAW needs to require attunement
            .Create($"MagicAffinity{name}Upgraded")
            .SetGuiPresentation(name, Category.Feature, FeatureDefinitionAttackModifiers.AttackModifierMagicWeapon3)
            .SetCastingModifiers(2, dcModifier: 2)
            .AddToDB());

        #endregion

        #region 02 Enhance Armor

        name = "InfusionEnhanceDefense";
        sprite = CustomSprites.GetSprite("EnhanceArmor", Resources.EnhanceArmor, 128);
        power = BuildInfuseItemPowerInvocation(2, name, sprite, IsNonEnhancedArmor,
            FeatureDefinitionAttributeModifierBuilder.Create($"AttributeModifier{name}")
                .SetGuiPresentation(name, Category.Feature, ConditionDefinitions.ConditionShielded)
                .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.ArmorClass, 1)
                .AddToDB());

        BuildUpgradedInfuseItemPower(name, power, sprite, IsNonEnhancedArmor, FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{name}Upgraded")
            .SetGuiPresentation(name, Category.Feature, ConditionDefinitions.ConditionShielded)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.ArmorClass, 2)
            .AddToDB());

        #endregion

        #region 02 Enhance Weapon

        name = "InfusionEnhanceWeapon";
        sprite = CustomSprites.GetSprite("EnhanceWeapon", Resources.EnhanceWeapon, 128);
        power = BuildInfuseItemPowerInvocation(2, name, sprite, IsWeapon, FeatureDefinitionAttackModifierBuilder
            .Create($"AttackModifier{name}")
            .SetGuiPresentation(name, Category.Feature, FeatureDefinitionAttackModifiers.AttackModifierMagicWeapon3)
            .SetAttackRollModifier(1)
            .SetDamageRollModifier(1)
            .SetMagicalWeapon()
            .AddToDB());

        BuildUpgradedInfuseItemPower(name, power, sprite, IsWeapon,
            FeatureDefinitionAttackModifierBuilder
                .Create($"AttackModifier{name}Upgraded")
                .SetGuiPresentation(name, Category.Feature, FeatureDefinitionAttackModifiers.AttackModifierMagicWeapon3)
                .SetAttackRollModifier(2)
                .SetDamageRollModifier(2)
                .SetMagicalWeapon()
                .AddToDB());

        #endregion

        #region 02 Mind Sharpener

        name = "InfusionMindSharpener";
        sprite = CustomSprites.GetSprite("MindSharpener", Resources.MindSharpener, 128);
        BuildInfuseItemPowerInvocation(2, name, sprite, IsBodyArmor, FeatureDefinitionMagicAffinityBuilder
            .Create($"MagicAffinity{name}")
            .SetGuiPresentation(name, Category.Feature, ConditionDefinitions.ConditionCalmedByCalmEmotionsAlly)
            //RAW it adds reaction to not break concentration
            .SetConcentrationModifiers(ConcentrationAffinity.Advantage, 10)
            .AddToDB());

        #endregion

        #region 02 Returning Weapon

        sprite = CustomSprites.GetSprite("ReturningWeapon", Resources.ReturningWeapon, 128);
        name = "InfusionReturningWeaponWithBonus";
        var infuseWithBonus = BuildInfuseItemPower(name, name, sprite, IsThrownWeapon,
            FeatureDefinitionAttackModifierBuilder
                .Create($"AttackModifier{name}")
                .SetGuiPresentation(name, Category.Feature, ConditionDefinitions.ConditionRevealedByDetectGoodOrEvil)
                .SetCustomSubFeatures(ReturningWeapon.Instance)
                .SetAttackRollModifier(1)
                .SetDamageRollModifier(1)
                .SetMagicalWeapon()
                .AddToDB());

        name = "InfusionReturningWeaponNoBonus";
        var noBonusModifier = FeatureDefinitionAttackModifierBuilder
            .Create($"AttackModifier{name}")
            .SetGuiPresentation(name, Category.Feature, ConditionDefinitions.ConditionRevealedByDetectGoodOrEvil)
            .SetCustomSubFeatures(ReturningWeapon.Instance)
            .SetAttackRollModifier(0)
            .SetDamageRollModifier(0)
            .AddToDB();

        var infuseNoBonus = BuildInfuseItemPower(name, name, sprite, new CustomItemFilter(IsThrownWeapon),
            noBonusModifier);

        //remove Infused marker by setting Returning marker
        noBonusModifier.SetCustomSubFeatures(ReturningWeapon.Instance);

        name = "InfusionReturningWeapon";
        var masterPower = BuildInfuseItemPowerInvocation(2, name, sprite, FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{name}")
            .SetGuiPresentation(name, Category.Feature, sprite)
            .SetSharedPool(ActivationTime.Action, InventorClass.InfusionPool)
            .SetCustomSubFeatures(ValidatorsPowerUse.NotInCombat)
            .SetUniqueInstance()
            .AddToDB());

        PowerBundle.RegisterPowerBundle(masterPower, true, infuseWithBonus, infuseNoBonus);

        #endregion

        #region 06 Resistant Armor

        sprite = CustomSprites.GetSprite("ResistantArmor", Resources.ResistantArmor, 128);
        name = "InfusionResistantArmor";
        //TODO: RAW needs to require attunement

        var elements = new[]
        {
            DamageTypeAcid, DamageTypeCold, DamageTypeFire, DamageTypeForce, DamageTypeLightning,
            DamageTypeNecrotic, DamageTypePoison, DamageTypePsychic, DamageTypeRadiant, DamageTypeThunder
        };
        var powers = new List<FeatureDefinitionPower>();

        foreach (var element in elements)
        {
            power = BuildInfuseItemPower(name + element, element, sprite, IsBodyArmor,
                FeatureDefinitionDamageAffinityBuilder
                    .Create($"DamageAffinity{name}{element}")
                    .SetGuiPresentation($"Feature/&{name}Title",
                        Gui.Format("Feature/&DamageResistanceFormat", Gui.Localize($"Rules/&{element}Title")),
                        ConditionDefinitions.ConditionProtectedFromEnergyLightning)
                    .SetCustomSubFeatures(ReturningWeapon.Instance)
                    .SetDamageAffinityType(DamageAffinityType.Resistance)
                    .SetDamageType(element)
                    .AddToDB());

            power.GuiPresentation.Title = $"Rules/&{element}Title";
            power.GuiPresentation.Description = $"Rules/&{element}Description";

            powers.Add(power);
        }

        masterPower = BuildInfuseItemPowerInvocation(6, name, sprite, FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{name}")
            .SetGuiPresentation(name, Category.Feature, sprite)
            .SetSharedPool(ActivationTime.Action, InventorClass.InfusionPool)
            .SetCustomSubFeatures(ValidatorsPowerUse.NotInCombat)
            .SetUniqueInstance()
            .AddToDB());

        PowerBundle.RegisterPowerBundle(masterPower, true, powers);

        #endregion

        #region Replicate Magic Item

        int level;

        #region Level 02

        level = 2;
        BuildCreateItemPowerInvocation(ItemDefinitions.Backpack_Bag_Of_Holding, level);
        //RAW this should be spectacles that don't require attunement
        BuildCreateItemPowerInvocation(ItemDefinitions.RingDarkvision, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.WandOfMagicDetection, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.WandOfIdentify, level);
        //RAW they are level 6, but at that level you get Cloak Of Elvenkind which is better
        BuildCreateItemPowerInvocation(ItemDefinitions.BootsOfElvenKind, level);

        #endregion

        #region Level 06

        level = 6;
        BuildCreateItemPowerInvocation(ItemDefinitions.CloakOfElvenkind, level);
        BuildCreateItemPowerInvocation(CustomItemsContext.GlovesOfThievery, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.PipesOfHaunting, level);
        //RAW they are level 14, but at 10 you get much better Winged Boots
        BuildCreateItemPowerInvocation(ItemDefinitions.BootsLevitation, level);
        //RAW they are level 10, but at 10 you get Winged Boots and Slippers Of Spider Climbing
        BuildCreateItemPowerInvocation(ItemDefinitions.BootsOfStridingAndSpringing, level);

        #endregion

        #region Level 10

        level = 10;
        BuildCreateItemPowerInvocation(ItemDefinitions.Bracers_Of_Archery, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.BroochOfShielding, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.CloakOfProtection, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.GauntletsOfOgrePower, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.GlovesOfMissileSnaring, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.HeadbandOfIntellect, level);
        BuildCreateItemPowerInvocation(CustomItemsContext.HelmOfAwareness, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.SlippersOfSpiderClimbing, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.BootsWinged, level);

        #endregion

        #region Level 14

        level = 14;
        BuildCreateItemPowerInvocation(ItemDefinitions.AmuletOfHealth, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.BeltOfGiantHillStrength, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.Bracers_Of_Defense, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.RingProtectionPlus1, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.GemOfSeeing, level);
        BuildCreateItemPowerInvocation(ItemDefinitions.HornOfBlasting, level);

        //Sadly this one is just a copy of Cloak of Protection as of v1.4.13
        // BuildCreateItemPowerInvocation(ItemDefinitions.CloakOfBat, 14);

        #endregion

        #endregion
    }

    private static FeatureDefinitionPower BuildInfuseItemPowerInvocation(int level,
        string name, AssetReferenceSprite icon,
        IsValidItemHandler filter, params FeatureDefinition[] features)
    {
        var power = BuildInfuseItemPower(name, name, icon, filter, features);
        BuildInfuseItemPowerInvocation(level, name, icon, power);
        return power;
    }

    private static FeatureDefinitionPower BuildInfuseItemPowerInvocation(int level, string name,
        AssetReferenceSprite icon,
        FeatureDefinitionPower power)
    {
        CustomInvocationDefinitionBuilder
            .Create($"Invocation{name}")
            .SetGuiPresentation(name, Category.Feature, icon)
            .SetCustomSubFeatures(Hidden.Marker)
            .SetPoolType(InvocationPoolTypeCustom.Pools.Infusion)
            .SetRequirements(level)
            .SetGrantedFeature(power)
            .AddToDB();
        return power;
    }

    private static void BuildCreateItemPowerInvocation(ItemDefinition item, int level = 2)
    {
        var replica = BuildItemReplica(item);
        var description = BuildReplicaDescription(item);
        var invocation = CustomInvocationDefinitionBuilder
            .Create($"InvocationCreate{replica.name}")
            .SetGuiPresentation(Category.Feature, replica)
            .SetCustomSubFeatures(Hidden.Marker)
            .SetPoolType(InvocationPoolTypeCustom.Pools.Infusion)
            .SetRequirements(level)
            .SetGrantedFeature(BuildCreateItemPower(replica, description))
            .AddToDB();

        invocation.Item = replica;
        invocation.GuiPresentation.title = replica.FormatTitle();
        invocation.GuiPresentation.description = description;
    }

    private static FeatureDefinitionPowerSharedPool BuildInfuseItemPower(string name, string guiName,
        AssetReferenceSprite icon, IsValidItemHandler itemFilter, params FeatureDefinition[] features)
    {
        return BuildInfuseItemPower(name, guiName, icon, new InfusionItemFilter(itemFilter), features);
    }

    private static FeatureDefinitionPowerSharedPool BuildInfuseItemPower(string name, string guiName,
        AssetReferenceSprite icon, ICustomItemFilter itemFilter, params FeatureDefinition[] features)
    {
        var properties = features.Select(f =>
        {
            f.AddCustomSubFeatures(Infused.Marker);
            return new FeatureUnlockByLevel(f, 0);
        });

        return FeatureDefinitionPowerSharedPoolBuilder.Create($"Power{name}")
            .SetGuiPresentation(guiName, Category.Feature, icon)
            .SetSharedPool(ActivationTime.Action, InventorClass.InfusionPool)
            .SetUniqueInstance()
            .SetCustomSubFeatures(ExtraCarefulTrackedItem.Marker, InventorClass.InfusionLimiter,
                SkipEffectRemovalOnLocationChange.Always, ValidatorsPowerUse.NotInCombat, itemFilter)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetAnimationMagicEffect(AnimationDefinitions.AnimationMagicEffect.Animation1)
                .SetTargetingData(Side.Ally, RangeType.Distance, 3, TargetType.Item,
                    itemSelectionType: ActionDefinitions.ItemSelectionType.Carried)
                .SetParticleEffectParameters(FeatureDefinitionPowers.PowerOathOfJugementWeightOfJustice)
                .SetDurationData(DurationType.Permanent)
                .SetEffectForms(EffectFormBuilder
                    .Create()
                    .HasSavingThrow(EffectSavingThrowType.None)
                    .SetItemPropertyForm(ItemPropertyUsage.Unlimited, 1, properties.ToArray())
                    .Build())
                .Build())
            .AddToDB();
    }

    private static void BuildUpgradedInfuseItemPower(string name, FeatureDefinitionPower power,
        AssetReferenceSprite sprite, IsValidItemHandler itemFilter, params FeatureDefinition[] features)
    {
        var upgrade = BuildInfuseItemPower($"{name}Upgraded", name,
            sprite, itemFilter, features);
        upgrade.overriddenPower = power;
        upgrade.AddCustomSubFeatures(new ValidatorsPowerUse(ValidatorsCharacter.HasAnyFeature(power)));
        ImprovedInfusions.FeatureSet.Add(upgrade);
    }

    private static FeatureDefinitionPowerSharedPool BuildCreateItemPower(ItemDefinition item, string description)
    {
        var power = FeatureDefinitionPowerSharedPoolBuilder.Create($"PowerCreate{item.name}")
            .SetGuiPresentation(Category.Feature, item)
            .SetSharedPool(ActivationTime.Action, InventorClass.InfusionPool)
            .SetCustomSubFeatures(
                ExtraCarefulTrackedItem.Marker,
                SkipEffectRemovalOnLocationChange.Always,
                InventorClass.InfusionLimiter,
                ValidatorsPowerUse.NotInCombat)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetAnimationMagicEffect(AnimationDefinitions.AnimationMagicEffect.Animation1)
                .SetTargetingData(Side.All, RangeType.Self, 1, TargetType.Self)
                .SetParticleEffectParameters(SpellDefinitions.Bless)
                .SetDurationData(DurationType.Permanent)
                .SetEffectForms(EffectFormBuilder
                    .Create()
                    .HasSavingThrow(EffectSavingThrowType.None)
                    .SetSummonItemForm(item, 1, true)
                    .Build())
                .Build())
            .AddToDB();

        power.GuiPresentation.title = item.FormatTitle();
        power.GuiPresentation.description = description;

        return power;
    }

    private static ItemDefinition BuildItemReplica(ItemDefinition baseItem)
    {
        var replica = ItemDefinitionBuilder
            .Create(baseItem, $"InfusedReplica{baseItem.name}")
            .AddItemTags(TagsDefinitions.ItemTagQuest) //TODO: implement custom tag, instead of quest
            .SetGold(0)
            .HideFromDungeonEditor()
            .SetRequiresIdentification(false)
            .AddToDB();

        replica.GuiPresentation.title = GuiReplicaTitle(baseItem);

        return replica;
    }

    private static string GuiReplicaTitle(ItemDefinition item)
    {
        return Gui.Format(ReplicaItemTitleFormat, item.FormatTitle());
    }

    private static string BuildReplicaDescription(ItemDefinition item)
    {
        return Gui.Format(ReplicaItemTitleDescription, item.FormatTitle(), item.FormatDescription());
    }

    #region Item Filters

    private class Infused
    {
        private Infused() { }
        public static Infused Marker { get; } = new();
    }

    private class InfusionItemFilter : CustomItemFilter
    {
        internal InfusionItemFilter(IsValidItemHandler handler) : base(handler)
        {
        }

        public override bool IsValid(RulesetCharacter character, RulesetItem rulesetItem)
        {
            var armorer = IsArmorsmithItem(character, rulesetItem);

            if (!armorer && rulesetItem.ItemDefinition.magical)
            {
                return false;
            }

            if (!armorer && rulesetItem.HasSubFeatureOfType<Infused>())
            {
                return false;
            }

            return base.IsValid(character, rulesetItem);
        }
    }

    private static bool IsArmorsmithItem(RulesetCharacter character, RulesetItem item)
    {
        //Weapon, or armor if character is level 9 armorsmith
        return character.HasSubFeatureOfType<InnovationArmor.ArmorerInfusions>()
               && IsBodyArmor(character, item);
    }

    private static bool IsFocusOrStaff(RulesetCharacter _, RulesetItem item)
    {
        var definition = item.ItemDefinition;
        var staffType = WeaponTypeDefinitions.QuarterstaffType.Name;
        return definition.IsFocusItem
               || (definition.IsWeapon && definition.WeaponDescription.WeaponType == staffType);
    }

    private static bool IsWeapon(RulesetCharacter character, RulesetItem item)
    {
        //Weapon, or armor if character is level 9 armorsmith
        return item.ItemDefinition.IsWeapon || IsArmorsmithItem(character, item);
    }

    private static bool IsThrownWeapon(RulesetCharacter _, RulesetItem item)
    {
        var definition = item.ItemDefinition;
        return definition.IsWeapon
               && definition.WeaponDescription.WeaponTags.Contains(TagsDefinitions.WeaponTagThrown);
    }

    //Any armor that doesn't have +AC bonus - so that Enhanced Armor infusion won't stack for Armorer subclass, but will work on other types of magic armor
    private static bool IsNonEnhancedArmor(RulesetCharacter _, RulesetItem item)
    {
        if (!item.ItemDefinition.IsArmor) { return false; }

        if (!item.ItemDefinition.SlotsWhereActive.Contains(SlotTypeDefinitions.TorsoSlot.Name)) { return true; }

        var features = new List<FeatureDefinition>();

        item.EnumerateFeaturesToBrowse<FeatureDefinitionAttributeModifier>(features);
        foreach (var feature in features)
        {
            var modifier = feature as FeatureDefinitionAttributeModifier;

            if (modifier == null) { continue; }

            if (modifier.ModifiedAttribute == AttributeDefinitions.ArmorClass)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsBodyArmor(RulesetCharacter _, RulesetItem item)
    {
        var definition = item.ItemDefinition;
        return definition.IsArmor
               && definition.SlotsWhereActive.Contains(SlotTypeDefinitions.TorsoSlot.Name);
    }

    #endregion
}
