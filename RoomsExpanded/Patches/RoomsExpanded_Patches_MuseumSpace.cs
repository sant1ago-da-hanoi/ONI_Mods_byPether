﻿using UnityEngine;
using Klei.AI;
using System;
using System.Linq;
using System.Collections.Generic;
using Database;
using HarmonyLib;

namespace RoomsExpanded
{
    class RoomsExpanded_Patches_MuseumSpace
    {
        public static void AddRoom(ref RoomTypes __instance)
        {
            if (!Settings.Instance.MuseumSpace.IncludeRoom)
                return;

            __instance.Add(RoomTypes_AllModded.MuseumSpace);
        }

        public static int CountUniqueArtifacts(Room room)
        {
            List<string> foundArtifacts = new List<string>();
            if (room != null)
                foreach (KPrefabID building in room.buildings)
                {
                    if (building == null) continue;
                    ItemPedestal pedestal = building.GetComponent<ItemPedestal>();
                    if (pedestal == null) continue;
                    SingleEntityReceptacle receptacle = Traverse.Create(pedestal).Field("receptacle").GetValue<SingleEntityReceptacle>();
                    if (receptacle == null || receptacle.Occupant == null) continue;
                    foreach (List<string> list in ArtifactConfig.artifactItems.Values)
                    {
                        bool shouldBreak = false;
                        foreach (string id in list)
                            if (receptacle.Occupant.name == id && !foundArtifacts.Contains(id))
                            {
                                foundArtifacts.Add(id);
                                shouldBreak = true;
                                break;
                            }
                        if (shouldBreak) break;
                    }

                }
            return foundArtifacts.Count;
        }

        public static bool CanGrantExtraBonus()
        {
            ColonyAchievementTracker tracker = SaveGame.Instance.GetComponent<ColonyAchievementTracker>();
            if (tracker == null)
                return false;

            ColonyAchievementStatus status = tracker.achievements.Where(a => a.Key == Db.Get().ColonyAchievements.CollectedArtifacts.Id).First().Value;
            if (status.success && !status.failed)
                return true;

            return !DlcManager.IsExpansion1Active();
        }

        private static int CalculateExtraBonus(int artifactsToCalculate)
        {
            int total = 0;
            int divider = 1;
            int maxPerDivideLvl = 5;

            while(artifactsToCalculate > 0)
            {
                int chunkAmount = Math.Min(artifactsToCalculate, divider * maxPerDivideLvl);
                artifactsToCalculate -= chunkAmount;
                total += chunkAmount / divider;
                divider += 1;
            }

            return total;
        }

        public static Effect CalculateEffectBonus(MinionModifiers modifiers, int uniqueArtifacts = 0)
        {
            AttributeInstance scienceAttrInstance = modifiers.attributes.AttributeTable.Where(p => p.Name == "Piloting").FirstOrDefault();
            if (scienceAttrInstance == null)
                return null;

            float piloting = scienceAttrInstance.GetTotalValue();
            float bonus = Settings.Instance.MuseumSpace.Bonus;
            int moraleBonus = Mathf.Clamp((int)Math.Ceiling(piloting * bonus), 1, 10);
            
            if(CanGrantExtraBonus())
                moraleBonus += CalculateExtraBonus(uniqueArtifacts);

            Effect effect = new Effect(RoomTypeMuseumSpaceData.EffectId, STRINGS.ROOMS.EFFECTS.MUSEUMSPACE.NAME, STRINGS.ROOMS.EFFECTS.MUSEUMSPACE.DESCRIPTION, 240, false, true, false);
            effect.SelfModifiers = new List<AttributeModifier>();
            effect.SelfModifiers.Add(new AttributeModifier("QualityOfLife", moraleBonus, description: STRINGS.ROOMS.EFFECTS.MUSEUMSPACE.NAME));
            return effect;
        }
    }
}
