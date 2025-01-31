﻿using System;
using System.Collections.Generic;
using Klei.AI;
using System.Text;
using System.Threading.Tasks;

namespace DiseasesExpanded.RandomEvents.Events
{
    class PlagueOfHunger : RandomDiseaseEvent
    {
        public PlagueOfHunger(int weight = 1)
        {
            ID = nameof(PlagueOfHunger);
            Group = Helpers.DIRECT_INFECT_GROUP;
            GeneralName = "Plague of Hunger";
            AppearanceWeight = weight;
            DangerLevel = ONITwitchLib.Danger.Extreme;

            Condition = new Func<object, bool>(data => GameClock.Instance.GetCycle() > 250);

            Event = new Action<object>(
                data =>
                {
                    foreach(MinionIdentity mi in Components.MinionIdentities)
                    {
                        if (mi == null)
                            continue;

                        Sicknesses sicknesses = mi.GetSicknesses();
                        if (sicknesses == null)
                            continue;

                        sicknesses.Infect(new SicknessExposureInfo(HungerSickness.ID, GeneralName));
                    }

                    ONITwitchLib.ToastManager.InstantiateToast(GeneralName, "A kilogram of Wheat for a Diamond, and three kilograms of Meal Lice for a Diamond, but do not harm Burgers and Pie!");
                });
        }
    }
}
