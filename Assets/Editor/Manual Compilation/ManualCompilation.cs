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
    /// Penser à désactiver Auto Refresh dans Edit/Preferences/Asset Pipeline
    /// </summary>
    [InitializeOnLoad]
    public class ManualCompilation
    {
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
            EditorPrefs.SetBool("kAutoRefresh", false);
            UnityEditor.EditorSettings.enterPlayModeOptionsEnabled = true;
            UnityEditor.EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneBackupUnlessDirty;

            //Charge les icônes
            _recompileIcon = EditorGUIUtility.Load("Assets/Editor/Manual Compilation/Resources/icon_recompile.psd") as Texture;
            _recompileAndPlayIcon = EditorGUIUtility.Load("Assets/Editor/Manual Compilation/Resources/icon_recompile and play.psd") as Texture;
        }

        #endregion

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
                Recompile();
            }
        }

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
            if (GUILayout.Button(new GUIContent(_recompileIcon, "Recompile"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.UnlockReloadAssemblies();
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

                    EditorApplication.UnlockReloadAssemblies();
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
        /// Recompile les scripts manuellement
        /// </summary>
        private static void Recompile()
        {
#if UNITY_2019_3_OR_NEWER
            bool cleanBuildCache = Menu.GetChecked("Manual Compilation/Clean Build Cache (fail safe)");
            CompilationPipeline.RequestScriptCompilation(cleanBuildCache ? RequestScriptCompilationOptions.CleanBuildCache : RequestScriptCompilationOptions.None);
#elif UNITY_2017_1_OR_NEWER
            var editorAssembly = Assembly.GetAssembly(typeof(Editor));
            var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
            var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
            dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
#endif
        }

        #endregion
    }
}