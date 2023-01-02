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
    /// InitializeOnLoad appelera automatiquement le constructeur
    /// quand Unity s'ouvre
    /// </summary>
    [InitializeOnLoad]
    public class ManualCompilation
    {
        #region Variables d'instance

        private static Texture _recompileIcon;
        private static Texture _recompileAndPlayIcon;

        #endregion

        #region Fonctions privées

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
            UnityEditor.EditorSettings.enterPlayModeOptionsEnabled = true;
            UnityEditor.EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneBackupUnlessDirty;

            //Charge les icônes
            _recompileIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Manual Compilation/Resources/icon_recompile.psd");
            _recompileAndPlayIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Manual Compilation/Resources/icon_recompile and play.psd");
        }

        /// <summary>
        /// Permet d'activer ou non la recompilation totale du projet depuis l'éditeur
        /// </summary>
        [MenuItem("Manual Compilation/Clear Build Cache (fail safe)")]
        private static void ToggleClearBuildCache()
        {
            bool clearBuildCache = Menu.GetChecked("Manual Compilation/Clear Build Cache (fail safe)");
            Menu.SetChecked("Manual Compilation/Clear Build Cache (fail safe)", !clearBuildCache);
        }

        /// <summary>
        /// S'abonne au ToolbarExtender pour créer des boutons
        /// à côté des boutons du mode Play
        /// </summary>
        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            // Relance la compilation manuellement depuis un bouton dans la Toolbar d'Unity
            if (GUILayout.Button(new GUIContent(_recompileIcon, "Recompile"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            //if (GUILayout.Button(new GUIContent("R", "Recompile"), EditorStyles.toolbarButton, GUILayout.MinWidth(30)))
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
                    CompilationPipeline.compilationFinished += OnCompilationFinished;

                    Recompile();
                }
            }
        }

        /// <summary>
        /// Appelée quand les scripts sont recompilés
        /// </summary>
        private static void OnCompilationFinished(object obj)
        {
            EditorApplication.EnterPlaymode();
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
        }

        /// <summary>
        /// Recompile les scripts manuellement
        /// </summary>
        private static void Recompile()
        {
#if UNITY_2019_3_OR_NEWER
            bool clearBuildCache = Menu.GetChecked("Manual Compilation/Clear Build Cache (fail safe)");
            CompilationPipeline.RequestScriptCompilation(clearBuildCache ? RequestScriptCompilationOptions.CleanBuildCache : RequestScriptCompilationOptions.None);
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