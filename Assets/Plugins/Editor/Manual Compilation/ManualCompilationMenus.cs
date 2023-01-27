using System.IO;
using UnityEditor;

namespace Project.Editor
{
    public static class ManualCompilationMenus
    {
        #region Fonctions privées

        /// <summary>
        /// Permet d'activer ou non la recompilation totale du projet depuis l'éditeur
        /// </summary>
        [MenuItem("Manual Compilation/Clean Build Cache (fail safe)")]
        private static void ToggleCleanBuildCacheMenuBtn()
        {
            bool clearBuildCache = Menu.GetChecked("Manual Compilation/Clean Build Cache (fail safe)");
            Menu.SetChecked("Manual Compilation/Clean Build Cache (fail safe)", !clearBuildCache);
        }

        /// <summary>
        /// Permet d'activer ou non la recompilation totale du projet depuis l'éditeur
        /// </summary>
        [MenuItem("Manual Compilation/Recompile (use if buttons are disabled)")]
        private static void RecompileMenuBtn()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.UnlockReloadAssemblies();
                ManualCompilation.Recompile();
            }
        }

        /// <summary>
        /// Permet de rafraîchir les assets de l'onglet Project
        /// </summary>
        [MenuItem("Manual Compilation/Refresh Assets (use if buttons are disabled)")]
        private static void RefreshAssetsBtn()
        {
            if (!EditorApplication.isPlaying)
            {
                ManualCompilation.RefreshAssets();
            }
        }

        /// <summary>
        /// Relance Unity si besoin
        /// </summary>
        [MenuItem("Manual Compilation/Restart Unity")]
        public static void ReopenProject()
        {
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
        }

        #endregion
    }
}