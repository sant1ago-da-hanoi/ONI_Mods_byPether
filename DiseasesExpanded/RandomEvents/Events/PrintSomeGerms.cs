﻿using System;
using System.Collections.Generic;
using DiseasesExpanded.RandomEvents;
using System.Text;
using System.Threading.Tasks;

namespace DiseasesExpanded.RandomEvents.Events
{
    class PrintSomeGerms : RandomDiseaseEvent
    {

        public PrintSomeGerms(byte germIdx, int weight = 1)
        {
            GeneralName = "Print Some Germs";
            NameDetails = Db.Get().Diseases[germIdx].Id;
            ID = GenerateId(nameof(PrintSomeGerms), NameDetails);
            Group = nameof(PrintSomeGerms);
            AppearanceWeight = weight;
            DangerLevel = Helpers.EstimateGermDanger(germIdx);

            Condition = new Func<object, bool>(data => true);

            Event = new Action<object>(
                data => 
                {
                    foreach (Telepad pod in Components.Telepads)
                        SimMessages.ModifyDiseaseOnCell(Grid.PosToCell(pod.gameObject), germIdx, 10000000);

                    ONITwitchLib.ToastManager.InstantiateToast(GeneralName, "Additional germs were printed from all Printing Pods.");
                }
            );
        }
    }
}
