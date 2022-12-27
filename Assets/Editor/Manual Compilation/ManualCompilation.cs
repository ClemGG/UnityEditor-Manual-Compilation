using UnityEditor;
#if UNITY_2019_3_OR_NEWER
using UnityEditor.Compilation;
#elif UNITY_2017_1_OR_NEWER
 using System.Reflection;
#endif
using UnityEngine;
using UnityToolbarExtender;

namespace Project.Editor
{
    [InitializeOnLoad]
    public class ManualCompilation
    {
        static ManualCompilation()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            EditorApplication.LockReloadAssemblies();
        }

        ~ManualCompilation()
        {
            ToolbarExtender.LeftToolbarGUI.Remove(OnToolbarGUI);
            EditorApplication.UnlockReloadAssemblies();
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("R", "Recompile"), EditorStyles.toolbarButton, GUILayout.MinWidth(30)))
            {
                EditorApplication.UnlockReloadAssemblies();
                Recompile();
            }
            //if (GUILayout.Button(new GUIContent("u", "unlock"), EditorStyles.toolbarButton, GUILayout.MinWidth(30)))
            //{
            //    EditorApplication.UnlockReloadAssemblies();
            //}

        }


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

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnCompilationFinished()
        {
            //EditorApplication.LockReloadAssemblies();
        }

    }
}