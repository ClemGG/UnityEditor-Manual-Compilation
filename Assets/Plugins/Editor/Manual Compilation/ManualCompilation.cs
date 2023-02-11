using UnityEditor;
#if UNITY_2019_3_OR_NEWER
using UnityEditor.Compilation;
#elif UNITY_2017_1_OR_NEWER
 using System.Reflection;
#endif
using UnityEngine;

// https://github.com/marijnz/unity-toolbar-extender
using UnityToolbarExtender;

namespace Project.Editor
{
    /// <summary>
    /// InitializeOnLoad appelera automatiquement le constructeur quand Unity s'ouvre.
    /// </summary>
    [InitializeOnLoad]
    public sealed class ManualCompilation : AssetPostprocessor
    {
        #region Constantes

        public const string RECOMPILE_ICON_PATH = "Assets/Plugins/Editor/Manual Compilation/Resources/icon_recompile.psd";
        public const string RECOMPILE_AND_PLAY_ICON_PATH = "Assets/Plugins/Editor/Manual Compilation/Resources/icon_recompile and play.psd";
        public const string REFRESH_ICON_PATH = "Assets/Plugins/Editor/Manual Compilation/Resources/icon_refresh.psd";

        #endregion

        #region Variables d'instance

        private static Texture _recompileIcon;
        private static Texture _recompileAndPlayIcon;
        private static Texture _refreshIcon;

        #endregion

        #region Constructeurs

        /// <summary>
        /// Lancé à chaque fois que le script est recompilé
        /// </summary>
        static ManualCompilation()
        {
            // Empêche la recompilation automatique
            EditorApplication.LockReloadAssemblies();

            // Par défaut, le bouton Play ne recompile plus les scripts
            // EditorPrefs.SetBool("kAutoRefresh", false);
            AssetDatabase.DisallowAutoRefresh();

            EditorSettings.enterPlayModeOptionsEnabled = true;
#if UNITY_2022_1_OR_NEWER
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneBackupUnlessDirty;
#elif UNITY_2017_1_OR_NEWER
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
#endif
        }

        /// <summary>
        /// Lancé quand la classe est collectée par le GC
        /// </summary>
        ~ManualCompilation()
        {
            // Autorise à nouveau la compilation auto. et recharge auto. des assets 

            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.AllowAutoRefresh();

            //Le bouton Play doit à nouveau recharger les domaines

            EditorSettings.enterPlayModeOptionsEnabled = false;
        }

        #endregion

        #region Fonctions publiques

        /// <summary>
        /// Recompile les scripts manuellement
        /// </summary>
        public static void Recompile()
        {
            EditorApplication.UnlockReloadAssemblies();

#if UNITY_2019_3_OR_NEWER
            bool cleanBuildCache = Menu.GetChecked(ManualCompilationMenus.CLEAN_BUILD_CACHE_PATH);
            CompilationPipeline.RequestScriptCompilation(cleanBuildCache ? RequestScriptCompilationOptions.CleanBuildCache : RequestScriptCompilationOptions.None);
#elif UNITY_2017_1_OR_NEWER
            var editorAssembly = Assembly.GetAssembly(typeof(Editor));
            var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
            var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
            dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
#endif
        }

        /// <summary>
        /// Permet de rafraîchir les assets de l'onglet Project
        /// </summary>
        public static void RefreshAssets()
        {
            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.Refresh();
        }

        #endregion

        #region Fonctions privées

        /// <summary>
        /// S'abonne au ToolbarExtender pour créer des boutons
        /// à côté des boutons du mode Play
        /// </summary>
        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            if (EditorApplication.isCompiling)
            {
                GUI.enabled = false;
            }

            // Relance la compilation manuellement depuis un bouton dans la Toolbar d'Unity
            if (GUILayout.Button(new GUIContent(_refreshIcon, "Refresh Assets"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                if (!EditorApplication.isPlaying)
                {
                    RefreshAssets();
                }
            }

            // Relance la compilation manuellement depuis un bouton dans la Toolbar d'Unity
            if (GUILayout.Button(new GUIContent(_recompileIcon, "Recompile"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                if (!EditorApplication.isPlaying)
                {
                    Recompile();
                }
            }

            // Recompile et lance le jeu (le bouton Play par défaut ne recompilera pas les scripts)
            if (GUILayout.Button(new GUIContent(_recompileAndPlayIcon, "Recompile And Play"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.ExitPlaymode();
                }
                else
                {
                    EditorSettings.enterPlayModeOptionsEnabled = false;
                    EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;

                    CompilationPipeline.compilationFinished += OnCompileAndPlayFinished;

                    Recompile();
                }
            }
        }

        /// <summary>
        /// Appelée quand les scripts sont recompilés
        /// </summary>
        private static void OnCompileAndPlayFinished(object obj)
        {
            EditorApplication.EnterPlaymode();
            CompilationPipeline.compilationFinished -= OnCompileAndPlayFinished;
        }

        /// <summary>
        /// Appelée quand les scripts sont recompilés
        /// ou quand les assets sont réimportées
        /// </summary>
        /// <param name="importedAssets">Les assets importées</param>
        /// <param name="deletedAssets">Les assets supprimées</param>
        /// <param name="movedAssets">Les assets déplacées</param>
        /// <param name="movedFromAssetPaths">Les emplacements d'origine des assets importées</param>
        /// <param name="didDomainReload">Indique si les scripts ont été recompilés</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // Charge les icônes

            if (_recompileIcon == null)
            {
                _recompileIcon = EditorGUIUtility.Load(RECOMPILE_ICON_PATH) as Texture;
                _recompileAndPlayIcon = EditorGUIUtility.Load(RECOMPILE_AND_PLAY_ICON_PATH) as Texture;
                _refreshIcon = EditorGUIUtility.Load(REFRESH_ICON_PATH) as Texture;
            }

            if (didDomainReload)
            {
                // Extension pour ajouter des boutons dans la Toolbar d'Unity
                // avant ou après les boutons pour lancer le mode Play

                ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            }
            else
            {
                // Quand on importe des assets, on débloque la compilation
                // si des scripts sont importés.
                // Ca évite de bloquer la compilation manuelle.

                EditorApplication.UnlockReloadAssemblies();

                // Garde le focus sur l'éditeur

                GUI.FocusWindow(0);
            }
        }

        #endregion
    }
}