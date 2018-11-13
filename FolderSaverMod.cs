using Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.Utility;
using Kingmaker.Visual.CharactersRigidbody;
using MyMod.Mods.Helper;
using Patchwork.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// When a savegame is loaded the Areas folder is cleared (AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Areas).
/// Also all files from the save zip file, including 200 areas are extracted and copied to the area folder. (up to 200MB?)
/// This obviously scales horribly as you progress in the game or keep loading a Game in the same area.
/// 
/// This mod changes this behavior to use a plain FolderSaver instead (no zipping, unzipping)
/// Also we only copy changed files to the areas folder, no need to copy 200 areas when we're only in one area.
/// This reduces loading times probably by about 2-5 seconds, depending on your CPU and HardDrive.
/// 
/// Old Save Zip Files should not be affected by this mod and should be loaded normally.
/// 
/// Disadvantages: Steam Upload probably wont work anymore, or uploads of saves to Owlcat.
/// Your Savefiles will be larger, because no zip.
/// If you remove this mod: Loading of old Folder Saves wont work anymore because of a bug in the original code of FolderSaver.CopyFromStash.
/// To fix you would probably need to convert your FolderSave back to a ZipSave.
/// (Just zip the folder content and copy the zip to AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Saved Games\)
/// </summary>

/// If anyone likes to copy this code to their Unity Mod Manager / Kingmaker Mod Loader Mod feel free to do so.
/// This mod uses patchwork  https://github.com/GregRos/Patchwork, i dont have time to convert it to a Unity Mod Manager Mod currently.

/// If anyone from Owlcat finds this mod, feel free to copy my code into the game or change how you like, maybe add an option to use a FolderSaver or ZipSaver

namespace MyMod.Mods
{

    // feel free to replace my BattleLogHelper.LogDebug with your own Exception Logger

    [ModifiesType("Kingmaker.EntitySystem.Persistence.SaveManager")]
    public class SaveManager2 : SaveManager
    {

        [NewMember]
        [DuplicatesBody("PrepareSave")]
        public void PrepareSaveBase(SaveInfo save)
        {
            // reference to SaveManager.PrepareSave
        }

        /// <summary>
        /// After calling SaveManager.PrepareSave we change the SaveInfo.Saver to a FolderSaver
        /// We also remove the file Extension, because this will be a Folder and not a Zip File
        /// </summary>
        /// <param name="save"></param>
        [ModifiesMember("PrepareSave", ModificationScope.Body)]
        public void PrepareSave2(SaveInfo save)
        {
            PrepareSaveBase(save);

            // !string.IsNullOrEmpty(FolderName);
            // !save.IsActuallySaved

            try
            {


                if (save.FolderName.EndsWith(".zks") || save.Saver == null ||
                    save.Saver.GetType() != typeof(FolderSaver2))
                {

                    // set folderName, (remove file extension)
                    save.FolderName = save.FolderName.Replace(".zks", "");

                    //save.Saver = new FolderSaver(save.FolderName);
                    save.Saver =  ((ISaver)new FolderSaver2(save.FolderName));

                    if (save.Type == SaveInfo.SaveType.Quick)
                    {
                        // set name of quicksave to folder name
                        // save.Name = Path.GetFileName(save.FolderName);
                    }
                }

            }
            catch (Exception ex)
            {
                BattleLogHelper.LogDebug($"err {ex.ToString()}");
            }

        }

    }





    [ModifiesType("Kingmaker.EntitySystem.Persistence.ZipSaver")]
    public class ZipSaver2
    {
        // reference to ZipSaver
    }




    [ModifiesType("Kingmaker.EntitySystem.Persistence.AreaDataStash")]
    public class AreaDataStash2
    {

        /// <summary>
        /// This function usually clears the areas folder, we dont want that.
        /// This function is called before a new game is started and 
        /// before a SaveFile is loaded.
        /// </summary>
        [ModifiesMember("ClearAll", ModificationScope.Body)]
        public static void ClearAll() // AreaDataStash.ClearAll
        {
            // We cleared the body
        }


        /// <summary>
        /// This clears the Areas Folder
        /// this is basically a copy of AreaDataStash.ClearAll()
        /// </summary>
        [NewMember]
        public static void ClearAll2()
        {

            var areasFolder = Path.Combine(Application.persistentDataPath, "Areas");
            BattleLogHelper.LogDebug($"try to delete : {areasFolder}");

            if (Directory.Exists(areasFolder))
            {
                // AppData/LocalLow/Owlcat Games/Pathfinder Kingmaker\Areas
                // remove this "security check" if you dont need it
                if (areasFolder.Contains(@"AppData/LocalLow/Owlcat Games/Pathfinder Kingmaker\Areas"))
                {
                    Directory.Delete(areasFolder, true);
                    BattleLogHelper.LogDebug($"deleted : {areasFolder}");
                }
            }

            Directory.CreateDirectory(areasFolder);
        }


    }

    [ModifiesType("Kingmaker.Game")]
    public class Game2 : Game
    {
        [NewMember]
        [DuplicatesBody("LoadNewGame", typeof(Game))]
        public void LoadNewGameB(BlueprintAreaPreset preset)
        {
            // reference to Kingmaker.Game.LoadNewGame
        }

        /// <summary>
        ///  When we start a new game, we still wanna clear the Areas folder
        /// </summary>
        /// <param name="preset"></param>
        [ModifiesMember("LoadNewGame", ModificationScope.Body)]
        public new void LoadNewGame(BlueprintAreaPreset preset)
        {
            AreaDataStash2.ClearAll2(); // will call AreaDataStash.ClearAll2();
            LoadNewGameB(preset);
        }


    }


    [ModifiesType("Kingmaker.EntitySystem.Persistence.ThreadedGameLoader")]
    public class ThreadedGameLoader2
    {

        [ModifiesMember("m_SaveInfo", ModificationScope.Accessibility)]
        private readonly SaveInfo m_SaveInfo; // reference to ThreadedGameLoader.m_SaveInfo

        /// <summary>
        /// we remove all files from areas Folder that are not in the SaveGame
        /// </summary>
        [NewMember]
        private void RemoveAllFilesNotInSaveGame()
        {
            if (m_SaveInfo?.Saver == null) return;

            var allFiles = m_SaveInfo.Saver.GetAllFiles();
            var saveContentsDic = new HashSet<string>(allFiles);

            var areasFolder = Path.Combine(Application.persistentDataPath, "Areas");
            Directory.CreateDirectory(areasFolder); // create if not exists

            // remove all files not in save
            foreach (var item in new DirectoryInfo(areasFolder).GetFiles())
            {
                var fileNameInArea = item.Name;

                if (!saveContentsDic.Contains(fileNameInArea))
                {
                    File.Delete(item.FullName);
                    BattleLogHelper.LogDebug($"not in save, delete {item.FullName}");
                }
            }
        }


        /// <summary>
        /// Instead of calling AreaDataStash.ClearAll (clear area folder), 
        /// we only wanna remove files from areas folder that are not in the SaveGame.
        ///  We call our RemoveAllFilesNotInSaveGame in the ctor of ThreadedGameLoader2
        ///  TODO: There is probably a better place where to do this, instead of in the ctor...
        /// </summary>
        /// <param name="m_SaveInfo"></param>
        [ModifiesMember(".ctor", ModificationScope.Body)]
        public ThreadedGameLoader2(SaveInfo m_SaveInfo)
        {
            this.m_SaveInfo = m_SaveInfo;

            try
            {
                // Usually AreaDataStash.ClearAll (clear area folder) is called before this ctor is invoked

                if (m_SaveInfo.Saver.GetType() == typeof(ZipSaver2))
                {
                    AreaDataStash2.ClearAll2(); // keep the old functionality
                }
                else
                {
                    RemoveAllFilesNotInSaveGame();
                }

            }
            catch (Exception ex)
            {
                BattleLogHelper.LogDebug($"ThreadedGameLoader .ctor {ex.ToString()}");
            }

        }

      

    }

    [ModifiesType("Kingmaker.EntitySystem.Persistence.FolderSaver")]
    public class FolderSaver2
    {


        [ModifiesMember(".ctor", ModificationScope.Accessibility)]
        public FolderSaver2(string folderName)
        {

        }


        [ModifiesMember("m_FolderName", ModificationScope.Accessibility)]
        private readonly string m_FolderName; // reference to FolderSaver.m_FolderName


        [NewMember]
        [DuplicatesBody("CopyToStash")]
        public void CopyToStashBase(string fileName)
        {
            // this is a reference to FolderSaver.CopyToStash
        }

        /// <summary>
        /// for Loading Games
        /// Copy Files from savegame folder to Areas Folder
        /// Copy only changed files
        /// </summary>
        /// <param name="fileName"></param>
        [ModifiesMember("CopyToStash", ModificationScope.Body)]
        public void CopyToStash(string fileName)
        {
            // m_FolderName: ...AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Saved Games
            // areas folder: ...AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Areas

            var areasFolder = Path.Combine(Application.persistentDataPath, "Areas");

            var saveGameFile = Path.Combine(m_FolderName, fileName);
            var destination = Path.Combine(areasFolder, fileName);

            var fi1 = new FileInfo(saveGameFile);
            var fi2 = new FileInfo(destination);

            if (FileHelper2.FileHasChanges(fi1, fi2))
            {
                File.Delete(destination);

                BattleLogHelper.LogDebug($"has changes, copy to {destination}");
                CopyToStashBase(fileName);
            }
            else
            {
                // BattleLogHelper.LogDebug($"no changes");
            }
        }

        [NewMember]
        [DuplicatesBody("CopyFromStash")]
        public void CopyFromStashBase(string fileName)
        {
            // reference to FolderSaver.CopyFromStash
        }


        /// <summary>
        /// Bugfix: Check for File.Exists before copy
        /// The ZipSaver tests for File.Exists, but the FolderSaver doesnt, this results in an Exception
        /// If you remove this mod then this function call will prevent you from loading folder saves,
        /// because the original FolderSaver throws an Exception here
        /// </summary>
        /// <param name="fileName"></param>
        [ModifiesMember("CopyFromStash", ModificationScope.Body)]
        public void CopyFromStash2(string fileName)
        {
            // saving...
            // copy from areas folder to SaveGame

            try
            {
                var areaDataStashFolder = Path.Combine(Application.persistentDataPath, "Areas");
                string fileToCopy = Path.Combine(areaDataStashFolder, fileName);

                // this check is missing in the FolderSaver.CopyFromStash
                if (File.Exists(fileToCopy))
                {
                    CopyFromStashBase(fileName);
                }
                else
                {
                    //BattleLogHelper.LogDebug($"doesnt exist: {fileToCopy}");
                }

            }
            catch (Exception ex)
            {
                BattleLogHelper.LogDebug($"err {ex.ToString()}");
            }

        }
    }



    [NewType]
    public class FileHelper2
    {

        /// <summary>
        /// Compare File MetaData
        /// </summary>
        public static bool FileHasChanges(FileInfo first, FileInfo second)
        {

            // we probably dont need to check if the first file exists here...
            if (!first.Exists || !second.Exists)
                return true;

            if (first.Length != second.Length) // check file size
                return true;

            // dont compare creation time...
            // first.CreationTime != second.CreationTime

            if ((first.LastWriteTime != second.LastWriteTime)) // check last modified DateTime
                return true;

            return false;
        }

    }



    /// <summary>
    /// Disable uploads to steam when we press the save button
    /// </summary>
    [ModifiesType("Kingmaker.EntitySystem.Persistence.SteamSavesReplicator")]
    public class SteamSavesReplicator2
    {

        [ModifiesMember("RegisterSave", ModificationScope.Body)]
        public void RegisterSave(SaveInfo saveInfo) { } // we cleared the body

        [ModifiesMember("PullUpdates", ModificationScope.Body)]
        public void PullUpdates() { } // we cleared the body
    }


    /// <summary>
    /// disable uploads to Owlcat when we press the save button
    /// </summary>
    [ModifiesType("Kingmaker.EntitySystem.Persistence.SavesStorage.SavesStorageAccess")]
    public class SavesStorageAccess2
    {

        [ModifiesMember("Upload", ModificationScope.Body)]
        public static Task<string> Upload(SaveInfo saveInfo, SaveCreateDTO dto) { return Task.FromResult(""); } // we cleared the body

    }


    /// <summary>
    /// Disables saving of ragdoll state for units (UnitEntityData m_SavedRagdoll), this reduces our save files by around 6000 lines of json per unit.
    /// Con: Visual Bug: Units wont be laying on the ground when they die...
    /// </summary>
    [ModifiesType("Kingmaker.Visual.CharacterSystem.SavedRagdollState")]
    public class SavedRagdollState2
    {

        [ModifiesMember("SaveRagdollState", ModificationScope.Body)]
        public void SaveRagdollState(RigidbodyCreatureController controller)
        {
            // we cleared the body

            /// Alternative: Remove units completely after they contain no more loot:

            /// foreach (var allSceneState in Game.Instance.CurrentScene.GetAllSceneStates())
            /// foreach (var allEntityData in allSceneState.AllEntityData)
            /// if (allEntityData is UnitEntityData unit)
            /// if (unit.IsRevealed &&  unit.Descriptor.State.IsFinallyDead && ! unit.IsDeadAndHasLoot)
            /// allSceneState.RemoveEntityData(allEntityData); allEntityData?.destroy();
        }
    }


}
