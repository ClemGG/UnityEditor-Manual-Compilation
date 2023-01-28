using System.IO;
using UnityEditor;

namespace Project.Editor
{
    /// <summary>
    /// Affiche les menus de la compilation manuelle 
    /// dans la barre d'outils d'Unity
    /// </summary>
    public static class ManualCompilationMenus
    {
        #region Constantes

        public const string CLEAN_BUILD_CACHE_PATH = "Manual Compilation/Settings/Clean Build Cache (fail safe)";
        public const string RECOMPILE_PATH = "Manual Compilation/Recompile (use if buttons are disabled)";
        public const string REFRESH_ASSETS_PATH = "Manual Compilation/Refresh Assets (use if buttons are disabled)";
        public const string RESTART_UNITY_PATH = "Manual Compilation/Restart Unity";

        #endregion

        #region Fonctions privées

        /// <summary>
        /// Permet d'activer ou non la recompilation totale du projet depuis l'éditeur
        /// </summary>
        [MenuItem(CLEAN_BUILD_CACHE_PATH)]
        private static void ToggleCleanBuildCacheMenuBtn()
        {
            bool clearBuildCache = Menu.GetChecked(CLEAN_BUILD_CACHE_PATH);
            Menu.SetChecked(CLEAN_BUILD_CACHE_PATH, !clearBuildCache);
        }

        /// <summary>
        /// Permet d'activer ou non la recompilation totale du projet depuis l'éditeur
        /// </summary>
        [MenuItem(RECOMPILE_PATH)]
        private static void RecompileMenuBtn()
        {
            if (!EditorApplication.isPlaying)
            {
                ManualCompilation.Recompile();
            }
        }

        /// <summary>
        /// Permet de rafraîchir les assets de l'onglet Project
        /// </summary>
        [MenuItem(REFRESH_ASSETS_PATH)]
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
        [MenuItem(RESTART_UNITY_PATH)]
        public static void ReopenProject()
        {
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
        }

        #endregion
    }
}