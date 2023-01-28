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
    public class ManualCompilation : AssetPostprocessor
    {
        #region Constantes

        public const string RECOMPILE_ICON_PATH = "Assets/Plugins/Editor/Manual Compilation/Resources/icon_recompile.psd";
        public const string RECOMPILE_AND_PLAY_ICON_PATH = "Assets/Plugins/Editor/Manual Compilation/Resources/icon_recompile and play.psd";

        #endregion

        #region Variables d'instance

        private static Texture _recompileIcon;
        private static Texture _recompileAndPlayIcon;

        #endregion

        #region Constructeurs

        /// <summary>
        /// Appelé auto. par InitializeOnLoad
        /// ou quand on recompile manuellement
        /// </summary>
        static ManualCompilation()
        {
            // Extension pour ajouter des boutons dans la Toolbar d'Unity
            // avant ou après les boutons pour lancer le mode Play
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            // Empêche la recompilation automatique
            EditorApplication.LockReloadAssemblies();

            // Par défaut, le bouton Play ne recompile plus les scripts
            // EditorPrefs.SetBool("kAutoRefresh", false);
            AssetDatabase.DisallowAutoRefresh();

            UnityEditor.EditorSettings.enterPlayModeOptionsEnabled = true;
            UnityEditor.EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneBackupUnlessDirty;
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
            if (_recompileIcon == null)
            {
                //Charge les icônes
                _recompileIcon = EditorGUIUtility.Load(RECOMPILE_ICON_PATH) as Texture;
                _recompileAndPlayIcon = EditorGUIUtility.Load(RECOMPILE_AND_PLAY_ICON_PATH) as Texture;
            }

            GUILayout.FlexibleSpace();

            if (EditorApplication.isCompiling)
            {
                GUI.enabled = false;
            }

            // Relance la compilation manuellement depuis un bouton dans la Toolbar d'Unity
            if (GUILayout.Button(new GUIContent("R", "Refresh Assets"), EditorStyles.toolbarButton, GUILayout.Width(30)))
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
                    UnityEditor.EditorSettings.enterPlayModeOptionsEnabled = false;
                    UnityEditor.EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;

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

        #endregion
    }
}