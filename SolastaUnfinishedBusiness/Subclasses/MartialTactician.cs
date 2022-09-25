﻿using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class MartialTactician : AbstractSubclass
{
    internal const string CounterStrikeTag = "CounterStrike";

    // ReSharper disable once InconsistentNaming
    private CharacterSubclassDefinition Subclass;

    internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
    {
        return FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;
    }

    internal override CharacterSubclassDefinition GetSubclass()
    {
        return Subclass ??= TacticianFighterSubclassBuilder.BuildAndAddSubclass();
    }
}

internal static class TacticianFighterSubclassBuilder
{
    private static readonly FeatureDefinitionPower PowerPoolTacticianGambit = FeatureDefinitionPowerPoolBuilder
        .Create("PowerPoolTacticianGambit")
        .Configure(
            4,
            UsesDetermination.Fixed,
            AttributeDefinitions.Dexterity,
            RechargeRate.ShortRest)
        .SetGuiPresentation(Category.Feature)
        .AddToDB();

    private static FeatureDefinitionPowerSharedPool BuildPowerSharedPoolTacticianInspire()
    {
        //Create the temp hp form
        var healingEffect = new EffectForm
        {
            formType = EffectForm.EffectFormType.TemporaryHitPoints,
            temporaryHitPointsForm = new TemporaryHitPointsForm
            {
                DiceNumber = 1, DieType = DieType.D6, BonusHitPoints = 2
            }
        };

        //Add to our new effect
        var newEffectDescription = new EffectDescription();

        newEffectDescription.Copy(FeatureDefinitionPowers.PowerDomainLifePreserveLife.EffectDescription);
        newEffectDescription.EffectForms.Clear();
        newEffectDescription.EffectForms.Add(healingEffect);
        newEffectDescription.HasSavingThrow = false;
        newEffectDescription.DurationType = DurationType.Day;
        newEffectDescription.SetTargetSide(Side.Ally);
        newEffectDescription.SetTargetType(TargetType.Individuals);
        newEffectDescription.SetTargetProximityDistance(12);
        newEffectDescription.SetCanBePlacedOnCharacter(true);
        newEffectDescription.SetRangeType(RangeType.Distance);

        var powerSharedPoolTacticianInspire = FeatureDefinitionPowerSharedPoolBuilder
            .Create("PowerSharedPoolTacticianInspire")
            .SetGuiPresentation(Category.Feature,
                FeatureDefinitionPowers.PowerDomainLifePreserveLife.GuiPresentation.SpriteReference)
            .Configure(
                PowerPoolTacticianGambit,
                RechargeRate.ShortRest,
                ActivationTime.BonusAction,
                1,
                true,
                true,
                AttributeDefinitions.Strength,
                newEffectDescription,
                false)
            .AddToDB();

        return powerSharedPoolTacticianInspire;
    }

    private static FeatureDefinitionPowerSharedPool BuildPowerSharedPoolTacticianKnockDown()
    {
        //Create the damage form
        var damageEffect = new EffectForm
        {
            DamageForm = new DamageForm
            {
                DiceNumber = 1, DieType = DieType.D6, BonusDamage = 2, DamageType = DamageTypeBludgeoning
            },
            SavingThrowAffinity = EffectSavingThrowType.None
        };

        //Create the prone effect - Weirdly enough the motion form seems to also automatically apply the prone condition
        var proneMotionEffect = new EffectForm
        {
            formType = EffectForm.EffectFormType.Motion,
            motionForm = new MotionForm { type = MotionForm.MotionType.FallProne, distance = 1 },
            savingThrowAffinity = EffectSavingThrowType.Negates
        };

        //Add to our new effect
        var newEffectDescription = FeatureDefinitionPowers.PowerFighterActionSurge.EffectDescription.Copy();

        newEffectDescription.SetEffectForms(damageEffect, proneMotionEffect);
        newEffectDescription.SetSavingThrowDifficultyAbility(AttributeDefinitions.Strength);
        newEffectDescription.SetDifficultyClassComputation(EffectDifficultyClassComputation.AbilityScoreAndProficiency);
        newEffectDescription.SavingThrowAbility = AttributeDefinitions.Strength;
        newEffectDescription.HasSavingThrow = true;
        newEffectDescription.DurationType = DurationType.Round;

        var powerSharedPoolTacticianKnockDown = FeatureDefinitionPowerSharedPoolBuilder
            .Create("PowerSharedPoolTacticianKnockDown")
            .SetGuiPresentation(Category.Feature,
                FeatureDefinitionPowers.PowerFighterActionSurge.GuiPresentation.SpriteReference)
            .Configure(
                PowerPoolTacticianGambit,
                RechargeRate.ShortRest,
                ActivationTime.OnAttackHit,
                1,
                true,
                true,
                AttributeDefinitions.Strength,
                newEffectDescription,
                false)
            .AddToDB();

        return powerSharedPoolTacticianKnockDown;
    }

    private static FeatureDefinitionPowerSharedPool BuildPowerSharedPoolTacticianCounterStrike()
    {
        // TODO: make it do the same damage as the wielded weapon (seems impossible with current tools, would need to use the AdditionalDamage feature but I'm not sure how to combine that with this to make it a reaction ability)
        var damageEffect = new EffectForm
        {
            damageForm = new DamageForm
            {
                DiceNumber = 1, DieType = DieType.D6, BonusDamage = 2, DamageType = DamageTypeBludgeoning
            },
            savingThrowAffinity = EffectSavingThrowType.None
        };

        //Add to our new effect
        var newEffectDescription = new EffectDescription();

        newEffectDescription.Copy(FeatureDefinitionPowers.PowerDomainLawHolyRetribution.EffectDescription);
        newEffectDescription.EffectForms.SetRange(damageEffect);

        var powerSharedPoolTacticianCounterStrike = FeatureDefinitionPowerSharedPoolBuilder
            .Create("PowerSharedPoolTacticianCounterStrike")
            .SetGuiPresentation(Category.Feature, FeatureDefinitionPowers.PowerDomainLawHolyRetribution.GuiPresentation
                .SpriteReference)
            .Configure(
                PowerPoolTacticianGambit,
                RechargeRate.ShortRest,
                ActivationTime.Reaction,
                1,
                true,
                true,
                AttributeDefinitions.Strength,
                newEffectDescription,
                false)
            .AddToDB();

        return powerSharedPoolTacticianCounterStrike;
    }

    private static FeatureDefinitionPowerPoolModifier BuildPowerPoolTacticianGambitAdd()
    {
        return FeatureDefinitionPowerPoolModifierBuilder
            .Create("PowerPoolTacticianGambitAdd")
            .SetGuiPresentation(Category.Feature)
            .Configure(
                1,
                UsesDetermination.Fixed,
                AttributeDefinitions.Dexterity,
                PowerPoolTacticianGambit)
            .AddToDB();
    }

    internal static CharacterSubclassDefinition BuildAndAddSubclass()
    {
        var powerPoolTacticianGambitAdd = BuildPowerPoolTacticianGambitAdd();

        return CharacterSubclassDefinitionBuilder
            .Create("MartialTactician")
            .SetGuiPresentation(Category.Subclass, RoguishShadowCaster.GuiPresentation.SpriteReference)
            .AddFeaturesAtLevel(3, PowerPoolTacticianGambit)
            .AddFeaturesAtLevel(3, BuildPowerSharedPoolTacticianKnockDown())
            .AddFeaturesAtLevel(3, BuildPowerSharedPoolTacticianInspire())
            .AddFeaturesAtLevel(3, BuildPowerSharedPoolTacticianCounterStrike())
            .AddFeaturesAtLevel(7, FeatureDefinitionFeatureSets.FeatureSetChampionRemarkableAthlete)
            .AddFeaturesAtLevel(10, powerPoolTacticianGambitAdd)
            .AddFeaturesAtLevel(15, powerPoolTacticianGambitAdd)
            .AddFeaturesAtLevel(18, powerPoolTacticianGambitAdd)
            .AddToDB();
    }
}
