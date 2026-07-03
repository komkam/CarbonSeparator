using STRINGS;
using TUNING;
using UnityEngine;

public class CO2ScrubberConfig : IBuildingConfig
{
    public const string ID = "CarbonSeparator";

    public override BuildingDef CreateBuildingDef()
    {
        string id = ID;

        BuildingDef def = BuildingTemplates.CreateBuildingDef(
            id,
            2,
            2,
            "co2scrubber_kanim",
            30,
            30f,
            TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
            MATERIALS.RAW_METALS,
            800f,
            BuildLocationRule.OnFloor,
            TUNING.BUILDINGS.DECOR.PENALTY.TIER1,
            NOISE_POLLUTION.NOISY.TIER3
        );

        def.RequiresPowerInput = true;
        def.EnergyConsumptionWhenActive = 240f;
        def.SelfHeatKilowattsWhenActive = 1f;

        // GAS INPUT/OUTPUT (correct ONI usage)
        def.InputConduitType = ConduitType.Gas;
        def.OutputConduitType = ConduitType.Gas;

        def.ViewMode = OverlayModes.GasConduits.ID;
        def.AudioCategory = "Metal";

        def.UtilityInputOffset = new CellOffset(0, 0);
        def.UtilityOutputOffset = new CellOffset(1, 0);

        def.PermittedRotations = PermittedRotations.FlipH;

        def.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(
            new CellOffset(1, 0)
        );

        def.AddSearchTerms(SEARCH_TERMS.CO2);
        def.AddSearchTerms(SEARCH_TERMS.FILTER);

        return def;
    }

    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
    {
        go.AddOrGet<LoopingSounds>();
        go.AddOrGet<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
        go.AddOrGet<Prioritizable>();

        // 📦 STORAGE (ONLY ONE - IMPORTANT)
        Storage storage = BuildingTemplates.CreateDefaultStorage(go);
        storage.showInUI = true;
        storage.capacityKg = 1000f;
        storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);

        // ⚙️ CORE CONVERSION (ONLY SOURCE OF CO2)
        var converter = go.AddOrGet<ElementConverter>();

        converter.consumedElements = new ElementConverter.ConsumedElement[]
        {
            new ElementConverter.ConsumedElement(GameTagExtensions.Create(SimHashes.CarbonDioxide),1f,true),
            new ElementConverter.ConsumedElement(new Tag("Filter"), 0.1f, true)
        };

        converter.outputElements = new ElementConverter.OutputElement[]
        {
            new ElementConverter.OutputElement(0.5f,SimHashes.Oxygen,0f,false,false,0f,0f,9999f,byte.MaxValue,0,true),
            new ElementConverter.OutputElement(0.3f,SimHashes.Carbon,0f,false,false,0f,0f,9999f,byte.MaxValue,0,true),
            new ElementConverter.OutputElement(0.2f,SimHashes.ToxicSand,0f,false,false,0f,0f,9999f,byte.MaxValue,0,true)
        };

        // 🧾 FILTER INPUT
        var manual = go.AddOrGet<ManualDeliveryKG>();
        manual.SetStorage(storage);
        manual.RequestedItemTag = new Tag("Filter");
        manual.capacity = 50f;
        manual.refillMass = 10f;
        manual.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;

        go.AddOrGet<KBatchedAnimController>().randomiseLoopedOffset = true;
    }

    public override void DoPostConfigureComplete(GameObject go)
    {
        go.AddOrGet<LogicOperationalController>();
        go.AddOrGetDef<PoweredActiveController.Def>();

    }
}