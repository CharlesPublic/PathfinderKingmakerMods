using Kingmaker;
using Kingmaker.UI.Log;
using Kingmaker.UI.SettingsUI;
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
    public class SettingsHelper2
    {

        /// <summary>
        /// settings wont be saved when i start a new game, so i use this helper function
        /// </summary>
        public static void SetMyFavSettings()
        {
            // Game.Instance.UI.SettingsUIController
            // ApplySettings()

            SettingsRoot.Instance.TutorialMessages.CurrentValue = TutorialState.Kingdom;
            SettingsRoot.Instance.AutofillActionbarSlots.CurrentValue = false;
            SettingsRoot.Instance.TooltipDelay.CurrentValue = 0;

            
            SettingsRoot.Instance.ShortenedTooltips.CurrentValue = false;

            // SettingsRoot.Instance.KingdomDifficulty ??
            SettingsRoot.Instance.KingdomManagementMode.CurrentValue = KingdomDifficulty.Easy;

            
            // Party settings
         //   SettingsRoot.Instance.ShowNamesForParty.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.ShowPartyHP.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.ShowPartyActions.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.ShowPartyAttackIntentions.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.ShowPartyCastIntentions.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.ShowNumericCooldownParty.CurrentState = SettingsEntityDropdownState.DropdownState.Always;

            // Enemy settings
           // SettingsRoot.Instance.ShowNamesForEnemies.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.ShowEnemyHP.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.EnemiesHPIsShort.CurrentValue = true;
           
            SettingsRoot.Instance.ShowEnemyActions.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.ShowEnemyIntentions.CurrentState = SettingsEntityDropdownState.DropdownState.Always;
            SettingsRoot.Instance.ShowNumericCooldownEnemies.CurrentState = SettingsEntityDropdownState.DropdownState.Always;

        }

    }

    [NewType]
    public class BattleLogHelper
    {

       // BattleLogHelper.LogDebug( $"{ex.ToString()}\n" );


        private static readonly object lockObj = new object();

        public static void LogDebug(string msg)
        {
            var fileNameDump = $@"V:\KingmakerDebug_Debug.csv";

            lock (lockObj)
            {
                File.AppendAllText(fileNameDump, msg + "\n");
            }
            
        }

        /// <summary>
        /// this function prints text to the combat log In Game
        /// </summary>
        /// <param name="msg"></param>
        public static void AddEntry(string msg)
        {
            try
            {
                var colors = Game.Instance.BlueprintRoot.UIRoot.LogColors;
                Game.Instance.UI.BattleLogManager.LogView.AddLogEntry(msg, colors.WarningLogColor, null, PrefixIcon.None);
            }
            catch (Exception)
            {

            }
         }
    }
}
