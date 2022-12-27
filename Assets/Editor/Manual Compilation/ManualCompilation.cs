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
        }

        /// <summary>
        /// S'abonne au ToolbarExtender pour créer des boutons
        /// à côté des boutons du mode Play
        /// </summary>
        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            // Relance la compilation manuellement depuis un bouton dans la Toolbar d'Unity
            if (GUILayout.Button(new GUIContent("R", "Recompile"), EditorStyles.toolbarButton, GUILayout.MinWidth(30)))
            {
                EditorApplication.UnlockReloadAssemblies();
                Recompile();
            }

        }

        /// <summary>
        /// Recompile les scripts manuellement
        /// </summary>
        private static void Recompile()
        {
#if UNITY_2019_3_OR_NEWER
            CompilationPipeline.RequestScriptCompilation();
#elif UNITY_2017_1_OR_NEWER
                 var editorAssembly = Assembly.GetAssembly(typeof(Editor));
                 var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
                 var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
                 dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
#endif
        }
    }
}