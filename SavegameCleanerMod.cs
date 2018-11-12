using Kingmaker;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Patchwork.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMod.Mods.Helper
{

    [NewType]
    public class SaveGameCleaner2
    {

        /// <summary>
        /// Remove dead units without loot from the area file
        /// </summary>
        public static void CleanUpSavegame_AreaUnits()
        {
            foreach (SceneEntitiesState allSceneState in Game.Instance.CurrentScene.GetAllSceneStates())
            {
                if (!allSceneState.IsSceneLoaded) continue;

                var toRemove = new List<EntityDataBase>();

                foreach (var allEntityData in allSceneState.AllEntityData)
                {
                    if (allEntityData is UnitEntityData unit)
                    {
                        if (!unit.IsPlayerFaction)
                        {
                            var state = unit.Descriptor.State;

                            if (unit.IsRevealed &&  state.IsFinallyDead && ! unit.IsDeadAndHasLoot)
                            {
                                toRemove.Add(unit);
                                BattleLogHelper.AddEntry($"Removing unit: {unit.CharacterName}");
                            }

                        }
                    }
                }

                foreach (var item in toRemove)
                {
                    allSceneState.RemoveEntityData(item);
                    
                    item?.View?.Destroy();
                    item?.Destroy();
                }


            }
        }

        /// <summary>
        /// Clear the statistic.json file
        /// Remove dead summons from the party.json
        /// Remove pets without master from the party.json
        /// </summary>
        public static void CleanUpSavegame_Party()
        {
            // clear the statistic.json file
            Game.Instance.Statistic?.Dispose();
            Game.Instance.Statistic = new GameStatistic();

            // Party:
            var partyUnits = Game.Instance.Player.CrossSceneState.AllEntityData;
            var toDelete = new List<EntityDataBase>();

            BattleLogHelper.AddEntry($"All party units count: {partyUnits.Count}");

            var types = new List<Type>();

            foreach (var item in partyUnits)
            {
                if (item == null) continue;

                types.Add(item.GetType());

                if (item is UnitEntityData u)
                {
                    var summonPart = u.Descriptor.Get<UnitPartSummonedMonster>();

                  //  var classes = u.GetClassesStr(); // AnimalClass (6)

                    var bp = u.Blueprint.name; // AnimalCompanionUnitSmilodon
                    var master = u.Descriptor.Master.Value;

                    // if you recruit a custom companion with an animal companion and delete the custom companion,
                    // the pet will stay in the party.json file. this code removes the pet
                    if (bp.StartsWith("AnimalCompanion") && master == null)
                    {
                        BattleLogHelper.AddEntry($"removing AnimalCompanion without master");
                        toDelete.Add(item);
                    }

                    if (summonPart != null)
                    {
                        if (!item.IsInGame)
                            toDelete.Add(item);
                    }
                }


            }


            foreach (var item in toDelete)
            {
                if (item is UnitEntityData u)
                {
                    BattleLogHelper.AddEntry($"removing {u.CharacterName} from party.json");
                }

                Game.Instance.Player.CrossSceneState.RemoveEntityData(item);
                item?.View?.Destroy();
                item?.Destroy();
            }
        }

    }
}
