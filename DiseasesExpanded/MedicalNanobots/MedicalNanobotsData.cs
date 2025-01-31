﻿using HarmonyLib;
using UnityEngine;
using KSerialization;
using System;
using System.Collections.Generic;
using Klei.AI;

namespace DiseasesExpanded
{
    [SerializationConfig(MemberSerialization.OptIn)]
    class MedicalNanobotsData : KMonoBehaviour, ISaveLoadable
    {
        public static readonly ComplexRecipe.RecipeElement MainIngridient = new ComplexRecipe.RecipeElement(SimHashes.Steel.CreateTag(), RECIPE_MASS_LARGE);

        private static MedicalNanobotsData _instance = null;
        private static bool _isReady = false;
        private const int maxDevelopmentLevel = 15;

        public const string FABRICATOR_ID = SupermaterialRefineryConfig.ID;
        public const float RECIPE_TIME = 600;
        public const float RECIPE_MASS_LARGE = 10000;
        public const float RECIPE_MASS_NORMAL = 1000;

        public static MedicalNanobotsData Instance
        {
            get
            {
                if (_instance == null)
                    _instance = SaveGame.Instance.GetComponent<MedicalNanobotsData>();
                return _instance;
            }
        }

        [Serialize]
        private Dictionary<MutationVectors.Vectors, int> DevelopmentLevels = new Dictionary<MutationVectors.Vectors, int>();

        // called in Game_DestroyInstances_Patch
        public static void Clear()
        {
            _instance = null;
            Debug.Log($"{ModInfo.Namespace}: MedicalNanobotsData._instance cleared.");
        }

        public static bool IsReadyToUse()
        {
            return _isReady;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            _isReady = true;

            Debug.Log($"{ModInfo.Namespace}: MedicalNanobotsData Spawned. Current development: {GetDevelopmentCode()}");
            Instance.UpdateAll();
        }


        public int GetDevelopmentLevel(MutationVectors.Vectors development)
        {
            if (!DevelopmentLevels.ContainsKey(development))
                DevelopmentLevels.Add(development, 0);
            return DevelopmentLevels[development];
        }

        public void IncreaseDevelopment(MutationVectors.Vectors development, int amount = 1)
        {
            if (!DevelopmentLevels.ContainsKey(development))
                DevelopmentLevels.Add(development, 0);
            DevelopmentLevels[development] += amount;

            if (DevelopmentLevels[development] > maxDevelopmentLevel)
                DevelopmentLevels[development] = maxDevelopmentLevel;

            UpdateAll();
            Notify();
        }

        public void UpdateGerms()
        {
            if (!Db.Get().Diseases.Exists(MedicalNanobots.ID))
                return;

            Disease dis = Db.Get().Diseases.Get((HashedString)MedicalNanobots.ID);
            if (dis == null)
                return;

            ((MedicalNanobots)dis).UpdateGermData();
        }

        public void UpdateExposureTable()
        {
            for (int i = 0; i < TUNING.GERM_EXPOSURE.TYPES.Length; i++)
                if (TUNING.GERM_EXPOSURE.TYPES[i].germ_id == MedicalNanobots.ID)
                {
                    TUNING.GERM_EXPOSURE.TYPES[i] = MedicalNanobots.GetExposureType(
                        exposureThresholdLevel: GetDevelopmentLevel(MutationVectors.Vectors.Res_ExposureThreshold)
                        );
                }
        }

        public void UpdateEffect()
        {
            ResourceSet<Effect> effects = Db.Get().effects;
            if (effects == null)
                return;
            Effect effect = effects.Get(MedicalNanobots.EFFECT_ID);
            if (effect == null)
                return;
            effects.Remove(effect);
            effects.Add(MedicalNanobots.GetEffect());
        }

        public void UpdateAll()
        {
            if (!_isReady)
                return;

            UpdateGerms();
            UpdateEffect();
            UpdateExposureTable();
        }

        private void Notify()
        {
            NotificationType notiType = NotificationType.Event;
            Notifier notifier = this.gameObject.AddOrGet<Notifier>();
            if (notifier == null)
                return;

            notifier.Add(new Notification(string.Format(STRINGS.NOTIFICATIONS.NANOBOTUPGRADE.PATTERN, GetDevelopmentCode()), notiType));
        }

        public string GetLegendString()
        {
            string baseStr = STRINGS.GERMS.MEDICALNANOBOTS.LEGEND_HOVERTEXT.Replace("\n", "");
            string hover = $"{baseStr} ({GetDevelopmentCode()}) \n\n{STRINGS.GERMS.MEDICALNANOBOTS.LEGEND_HEADER}\n{GetMutationsCodeLegend()}";
            return hover;
        }

        public string GetDevelopmentCode()
        {
            string pattern = STRINGS.GERMS.MEDICALNANOBOTS.VERSION_PATTERN;

            string attackCode = string.Format("{0:x}{1:x}{2:x}-{3:x}{4:x}{5:x}",
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Attributes),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Breathing),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Calories),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Damage),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Stamina),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Stress)
                );
            string resCode = string.Format("{0:x}{1:x}{2:x}-{3:x}{4:x}{5:x}",
                GetDevelopmentLevel(MutationVectors.Vectors.Res_BaseInfectionResistance),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_EffectDuration),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_Replication),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_ExposureThreshold),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_RadiationResistance),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_TemperatureResistance)
                );

            return pattern.Replace("{ATTACK}", attackCode)
                            .Replace("{RESILIANCE}", resCode);
        }

        public string GetMutationsCodeLegend()
        {
            string attackLegend = string.Format(STRINGS.GERMS.MEDICALNANOBOTS.VERION_EFFICIENCY_PATTERN,
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Attributes),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Breathing),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Calories),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Damage),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Stamina),
                GetDevelopmentLevel(MutationVectors.Vectors.Att_Stress)
                );
            string resistLegend = string.Format(STRINGS.GERMS.MEDICALNANOBOTS.VERSION_RESILIANCE_PATTERN,
                GetDevelopmentLevel(MutationVectors.Vectors.Res_BaseInfectionResistance),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_EffectDuration),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_Replication),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_ExposureThreshold),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_RadiationResistance),
                GetDevelopmentLevel(MutationVectors.Vectors.Res_TemperatureResistance)
                );

            string help = string.Format(STRINGS.GERMS.MEDICALNANOBOTS.UPGRADE_HELP_PATTERN, GetFabricatorName());
            string legend = $"{attackLegend}\n{resistLegend}\n\n{help}";

            return legend;
        }

        public string GetFabricatorName()
        {
            StringEntry name;
            StringKey key = new StringKey($"STRINGS.BUILDINGS.PREFABS.{FABRICATOR_ID.ToUpperInvariant()}.NAME");
            if (Strings.TryGet(key, out name))
                return name.String;

            return FABRICATOR_ID;
        }
    }
}
