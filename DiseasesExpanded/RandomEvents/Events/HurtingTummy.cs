﻿using System;
using System.Collections.Generic;
using DiseasesExpanded.RandomEvents;
using Klei.AI;
using System.Threading.Tasks;

namespace DiseasesExpanded.RandomEvents.Events
{
    class HurtingTummy : RandomDiseaseEvent
    {
        public HurtingTummy(bool gas = false, int weight = 1)
        {
            byte germIdx = gas ? GermIdx.GassyGermsIdx : GermIdx.FoodPoisoningIdx;
            GeneralName = "Hurting Tummy";
            NameDetails = Db.Get().Diseases[germIdx].Id;
            ID = GenerateId(nameof(HurtingTummy), NameDetails);
            Group = Helpers.DIRECT_INFECT_GROUP;
            AppearanceWeight = weight;
            DangerLevel = Helpers.EstimateGermDanger(germIdx);

            Condition = new Func<object, bool>(data => GameClock.Instance.GetCycle() > 25);

            Event = new Action<object>(
                data =>
                {
                    int dupeIdx = UnityEngine.Random.Range(0, Components.MinionIdentities.Count);
                    MinionIdentity mi = Components.MinionIdentities[dupeIdx];
                    if (mi == null)
                        return;

                    Sicknesses sicknesses = mi.GetSicknesses();
                    if (sicknesses == null)
                        return;

                    sicknesses.Infect(new SicknessExposureInfo(gas ? GasSickness.ID : FoodSickness.ID, GeneralName));

                    ONITwitchLib.ToastManager.InstantiateToastWithGoTarget(GeneralName, "One of your duplicants got hurting tummy. It's nothing good for sure...", mi.gameObject);
                });
        }
    }
}
