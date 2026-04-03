using System.IO;
using UnityEditor;
using UnityEngine;

namespace FigmaImporter.Editor
{
    internal sealed class FigmaImporterHelpWindow : EditorWindow
    {
        private const string WindowTitle = "Figma Importer Help";
        private const string GitDownloadUrl = "https://git-scm.com/downloads";
        private static readonly string[] PackageNames =
        {
            "com.redhong01.figma_to_unity_importer",
            "com.manakhovn.figma_to_unity_importer"
        };
        private Vector2 _scrollPosition;

        [MenuItem(FigmaImporterMenuPaths.Help.QuickStartTutorial)]
        internal static void OpenWindow()
        {
            var window = GetWindow<FigmaImporterHelpWindow>(WindowTitle);
            window.minSize = new Vector2(620f, 520f);
            window.Show();
        }

        [MenuItem(FigmaImporterMenuPaths.Help.OpenReadme)]
        private static void OpenReadmeMenu()
        {
            OpenReadme();
        }

        [MenuItem(FigmaImporterMenuPaths.Help.OpenDiagnosticsHub)]
        private static void OpenDiagnosticsHubMenu()
        {
            FigmaDiagnosticsHubWindow.OpenAuthAndApiPage();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Figma Importer Onboarding", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use this quick start after first install. It covers setup, first import, and common troubleshooting paths.",
                MessageType.Info);

            DrawStepOnePrerequisites();
            DrawStepTwoAuth();
            DrawStepThreeImportFlow();
            DrawStepFourTroubleshooting();
            DrawStepFiveUsefulLinks();

            EditorGUILayout.EndScrollView();
        }

        private static void DrawSectionTitle(string title)
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private static void DrawStepOnePrerequisites()
        {
            DrawSectionTitle("1) Prerequisites");
            EditorGUILayout.HelpBox(
                "Install Git on every collaborator machine. Unity needs a system Git executable for 'Install package from git URL'.",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Download Git", GUILayout.Height(24f)))
            {
                Application.OpenURL(GitDownloadUrl);
            }

            if (GUILayout.Button("Initialize Dependencies Now", GUILayout.Height(24f)))
            {
                EditorApplication.ExecuteMenuItem(FigmaImporterMenuPaths.Dependencies.InitializeNow);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawStepTwoAuth()
        {
            DrawSectionTitle("2) OAuth Token Setup");
            EditorGUILayout.HelpBox(
                "Open Importer, click OpenOauthUrl, then GetToken on this machine. OAuth tokens can be device/session-sensitive in team workflows.",
                MessageType.None);

            if (GUILayout.Button("Open Importer", GUILayout.Height(24f)))
            {
                EditorApplication.ExecuteMenuItem(FigmaImporterMenuPaths.Importer.OpenWindow);
            }
        }

        private static void DrawStepThreeImportFlow()
        {
            DrawSectionTitle("3) First Import");
            EditorGUILayout.HelpBox(
                "In Importer: set Figma URL, pick Root Object, click Fetch Figma Node Data, then Apply Selected Import Modes.",
                MessageType.None);

            EditorGUILayout.HelpBox(
                "Tip: Keep 'Filter Canvas Related Objects' enabled while picking root object to avoid placing nodes outside UI canvas hierarchy.",
                MessageType.Info);
        }

        private static void DrawStepFourTroubleshooting()
        {
            DrawSectionTitle("4) Troubleshooting");
            EditorGUILayout.HelpBox(
                "401/403 from Figma API: re-run OpenOauthUrl + GetToken on this device and verify file permissions.\n" +
                "Missing fonts/SVG fallback: use Diagnostics Hub and Fallback Resolver.",
                MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Diagnostics Hub", GUILayout.Height(24f)))
            {
                FigmaDiagnosticsHubWindow.OpenAuthAndApiPage();
            }

            if (GUILayout.Button("Open Fallback Resolver", GUILayout.Height(24f)))
            {
                FigmaDiagnosticsHubWindow.OpenFallbackPage();
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawStepFiveUsefulLinks()
        {
            DrawSectionTitle("5) Useful Links");
            EditorGUILayout.HelpBox(
                "README includes installation notes, package URL usage, and feature overview.",
                MessageType.None);

            if (GUILayout.Button("Open Package README", GUILayout.Height(24f)))
            {
                OpenReadme();
            }
        }

        private static void OpenReadme()
        {
            var readmePath = ResolveReadmePath();
            if (string.IsNullOrEmpty(readmePath) || !File.Exists(readmePath))
            {
                Debug.LogWarning("[FigmaImporter] README not found for Help window.");
                return;
            }

            EditorUtility.OpenWithDefaultApp(readmePath);
        }

        private static string ResolveReadmePath()
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(FigmaImporterHelpWindow).Assembly);
            if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            {
                return Path.Combine(packageInfo.resolvedPath, "README.md");
            }

            for (var i = 0; i < PackageNames.Length; i++)
            {
                var packageName = PackageNames[i];
                var infoByName = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/" + packageName + "/package.json");
                if (infoByName != null && !string.IsNullOrEmpty(infoByName.resolvedPath))
                {
                    return Path.Combine(infoByName.resolvedPath, "README.md");
                }

                var projectPackagePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Packages", packageName, "README.md"));
                if (File.Exists(projectPackagePath))
                {
                    return projectPackagePath;
                }
            }

            return string.Empty;
        }
    }
}
