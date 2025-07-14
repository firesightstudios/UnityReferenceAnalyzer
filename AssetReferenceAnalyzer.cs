using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AssetReferenceAnalyzer : EditorWindow
    {
        private Vector2 _scrollPosition;
        private readonly List<string> _unreferencedAssets = new();
        private string _selectedFolderPath;
        private bool _isAnalyzing;
        private bool _shouldCancelAnalysis;
        private float _analysisProgress;
        private string _currentAnalysisStep = "";
        private int _totalAssetsToAnalyze;
        private int _processedAssets;
        private bool _hasAnalyzed;
        private IEnumerator<bool> _analysisCoroutine;

        [MenuItem("Window/Asset Reference Analyzer")]
        private static void OpenWindow()
        {
            GetWindow<AssetReferenceAnalyzer>("Asset Reference Analyzer");
        }

        [MenuItem("Assets/Analyze References in Folder", false, 21)]
        private static void AnalyzeFolder()
        {
            var window = GetWindow<AssetReferenceAnalyzer>("Asset Reference Analyzer");

            // Get the selected folder path
            var selectedPath = GetSelectedFolderPath();

            window._selectedFolderPath = selectedPath;
            window.Show();
        }

        [MenuItem("Assets/Analyze References in Folder", true)]
        private static bool ValidateAnalyzeFolder()
        {
            // Only show the menu item if a folder is selected or if we're in the Assets folder
            return GetSelectedFolderPath() != null;
        }

        private static string GetSelectedFolderPath()
        {
            // Check if we have a selected object
            if (Selection.activeObject != null)
            {
                var selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

                // If it's already a folder, use it directly
                if (AssetDatabase.IsValidFolder(selectedPath))
                {
                    return selectedPath;
                }
                // If it's a file, get its parent folder
                else if (!string.IsNullOrEmpty(selectedPath))
                {
                    var parentFolder = Path.GetDirectoryName(selectedPath)?.Replace('\\', '/');
                    if (AssetDatabase.IsValidFolder(parentFolder))
                    {
                        return parentFolder;
                    }
                }
            }

            // Check if we right-clicked in the Project window on a folder
            // This handles the case where you right-click on empty space in a folder
            var selectedGUIDs = Selection.assetGUIDs;
            if (selectedGUIDs.Length > 0)
            {
                foreach (var guid in selectedGUIDs)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        return path;
                    }
                }
            }

            // Try to get the current folder from the Project window
            var projectWindowUtilType = typeof(ProjectWindowUtil);
            var getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (getActiveFolderPath != null)
            {
                var activeFolderPath = (string)getActiveFolderPath.Invoke(null, null);
                if (!string.IsNullOrEmpty(activeFolderPath) && AssetDatabase.IsValidFolder(activeFolderPath))
                {
                    return activeFolderPath;
                }
            }

            // Default to Assets folder as a last resort
            return "Assets";
        }

        private void OnGUI()
        {
            // Only update analysis during Layout events to avoid GUI layout issues
            if (Event.current.type == EventType.Layout && _isAnalyzing)
            {
                UpdateAnalysis();
            }

            DrawGUI();
        }

        private void DrawGUI()
        {
            EditorGUILayout.Space();

            // Folder selection section
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Folder Selection", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Folder:", GUILayout.Width(50));
            GUI.enabled = !_isAnalyzing;
            _selectedFolderPath = EditorGUILayout.TextField(_selectedFolderPath ?? "");
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var absolutePath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(absolutePath))
                {
                    _selectedFolderPath = ConvertToRelativePath(absolutePath);
                }
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // Show current folder info
            if (!string.IsNullOrEmpty(_selectedFolderPath))
            {
                if (AssetDatabase.IsValidFolder(_selectedFolderPath))
                {
                    EditorGUILayout.HelpBox($"Will analyze: {_selectedFolderPath}", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"Invalid folder: {_selectedFolderPath}", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a folder to analyze", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Analysis section
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Analysis", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !_isAnalyzing && !string.IsNullOrEmpty(_selectedFolderPath) &&
                          AssetDatabase.IsValidFolder(_selectedFolderPath);
            if (GUILayout.Button("Analyze References", GUILayout.Height(25)))
            {
                StartAnalysis();
            }

            GUI.enabled = true;

            // Cancel button
            GUI.enabled = _isAnalyzing;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Cancel", GUILayout.Width(80), GUILayout.Height(25)))
            {
                CancelAnalysis();
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // Progress section - always consistent
            EditorGUILayout.Space();
            var statusText = _isAnalyzing ? _currentAnalysisStep : "Ready";
            var progressText = _isAnalyzing ? $"{_processedAssets}/{_totalAssetsToAnalyze} assets" : "";

            EditorGUILayout.LabelField("Status:", statusText);
            EditorGUILayout.LabelField("Progress:", progressText);

            var progressRect = EditorGUILayout.GetControlRect(false, 20);
            if (_isAnalyzing)
            {
                EditorGUI.ProgressBar(progressRect, _analysisProgress, $"{(_analysisProgress * 100):F1}%");
            }
            else
            {
                EditorGUI.ProgressBar(progressRect, 0f, "");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Results section - always consistent structure
            EditorGUILayout.BeginVertical("box");

            // Header section - always present
            EditorGUILayout.BeginHorizontal();
            if (_unreferencedAssets.Count > 0)
            {
                GUILayout.Label($"Unreferenced Assets ({_unreferencedAssets.Count})", EditorStyles.boldLabel);
            }
            else if (_hasAnalyzed && !_isAnalyzing)
            {
                GUILayout.Label("✓ No unreferenced assets found!", EditorStyles.boldLabel);
            }
            else
            {
                GUILayout.Label("Results", EditorStyles.boldLabel);
            }

            GUILayout.FlexibleSpace();

            // Delete All button - always present but conditionally enabled
            GUI.enabled = !_isAnalyzing && _unreferencedAssets.Count > 0;
            GUI.backgroundColor = _unreferencedAssets.Count > 0 ? Color.red : Color.white;
            if (GUILayout.Button("Delete All", GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Delete All Unreferenced Assets",
                        $"Are you sure you want to delete all {_unreferencedAssets.Count} unreferenced assets?\n\nThis action cannot be undone!",
                        "Delete All", "Cancel"))
                {
                    DeleteAllUnreferencedAssets();
                }
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Scroll view - always present with a consistent structure
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, "box", GUILayout.ExpandHeight(true));

            if (_unreferencedAssets.Count > 0)
            {
                DrawAssetList();
            }
            else if (_hasAnalyzed && !_isAnalyzing)
            {
                EditorGUILayout.LabelField("All assets in the selected folder are referenced!",
                    EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Results will appear here after analysis",
                    EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawAssetList()
        {
            var assetsToDisplay = new List<string>(_unreferencedAssets);

            for (var i = 0; i < assetsToDisplay.Count; i++)
            {
                var asset = assetsToDisplay[i];

                // Alternate row colors
                GUI.backgroundColor = i % 2 == 0 ? new Color(0.8f, 0.8f, 0.8f, 0.3f) : Color.white;

                EditorGUILayout.BeginHorizontal("box");
                GUI.backgroundColor = Color.white;

                // Asset icon and name
                var obj = AssetDatabase.LoadAssetAtPath<Object>(asset);
                Texture2D icon = null;
                if (obj != null)
                {
                    icon = AssetPreview.GetMiniThumbnail(obj);
                }

                if (icon != null)
                {
                    GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
                }
                else
                {
                    GUILayout.Space(16); // Reserve space for icon
                }

                EditorGUILayout.LabelField(asset, GUILayout.ExpandWidth(true));

                // Buttons - always present but conditionally enabled
                GUI.enabled = !_isAnalyzing;
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    if (obj != null)
                    {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Asset",
                            $"Are you sure you want to delete:\n{asset}\n\nThis action cannot be undone!",
                            "Delete", "Cancel"))
                    {
                        DeleteAsset(asset);
                    }
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }
        }

        private void StartAnalysis()
        {
            _isAnalyzing = true;
            _shouldCancelAnalysis = false;
            _unreferencedAssets.Clear();
            _analysisProgress = 0f;
            _processedAssets = 0;
            _totalAssetsToAnalyze = 0;
            _hasAnalyzed = false;

            _currentAnalysisStep = "Initializing...";

            // Start the analysis coroutine
            _analysisCoroutine = AnalyzeFolderReferences();
        }

        private void CancelAnalysis()
        {
            _shouldCancelAnalysis = true;
            _isAnalyzing = false;
            _analysisCoroutine = null;
            _currentAnalysisStep = "Analysis cancelled";
        }

        private void UpdateAnalysis()
        {
            if (_analysisCoroutine != null && !_shouldCancelAnalysis)
            {
                // Process a few items per frame to keep the UI responsive
                const int itemsPerFrame = 5;
                for (var i = 0; i < itemsPerFrame; i++)
                {
                    if (!_analysisCoroutine.MoveNext())
                    {
                        // Analysis completed
                        _isAnalyzing = false;
                        _analysisCoroutine = null;
                        _hasAnalyzed = true;
                        _currentAnalysisStep = $"Complete! Found {_unreferencedAssets.Count} unreferenced assets";
                        break;
                    }

                    if (_shouldCancelAnalysis)
                    {
                        CancelAnalysis();
                        break;
                    }
                }
            }

            // Only trigger repaint if we're still analyzing
            if (_isAnalyzing)
            {
                Repaint();
            }
        }

        private void DeleteAsset(string assetPath)
        {
            try
            {
                if (AssetDatabase.DeleteAsset(assetPath))
                {
                    _unreferencedAssets.Remove(assetPath);
                }
                else
                {
                    Debug.LogError($"Failed to delete: {assetPath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error deleting {assetPath}: {e.Message}");
            }
        }

        private void DeleteAllUnreferencedAssets()
        {
            var deletedCount = 0;
            var totalCount = _unreferencedAssets.Count;

            var assetsToDelete = new List<string>(_unreferencedAssets);

            foreach (var asset in assetsToDelete)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Deleting Assets",
                        $"Deleting {deletedCount + 1}/{totalCount}: {Path.GetFileName(asset)}",
                        (float)deletedCount / totalCount))
                {
                    break; // User cancelled
                }

                try
                {
                    if (AssetDatabase.DeleteAsset(asset))
                    {
                        _unreferencedAssets.Remove(asset);
                        deletedCount++;
                    }
                    else
                    {
                        Debug.LogError($"Failed to delete: {asset}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error deleting {asset}: {e.Message}");
                }
            }

            EditorUtility.ClearProgressBar();
            Debug.Log($"Batch deletion complete. Deleted {deletedCount} out of {totalCount} assets.");
            AssetDatabase.Refresh();
        }

        private static string ConvertToRelativePath(string absolutePath)
        {
            var assetsPath = Application.dataPath;

            if (absolutePath.StartsWith(assetsPath))
            {
                var relativePath = "Assets" + absolutePath.Substring(assetsPath.Length);
                relativePath = relativePath.Replace('\\', '/');
                return relativePath;
            }

            Debug.LogError("Selected folder must be within the Assets directory");
            return "";
        }

        private IEnumerator<bool> AnalyzeFolderReferences()
        {
            if (string.IsNullOrEmpty(_selectedFolderPath) || !AssetDatabase.IsValidFolder(_selectedFolderPath))
            {
                yield break;
            }

            _currentAnalysisStep = "Finding assets...";
            yield return true;

            var guids = AssetDatabase.FindAssets("", new[] { _selectedFolderPath });

            // Count non-folder assets
            foreach (var guid in guids)
            {
                if (_shouldCancelAnalysis) yield break;

                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.IsValidFolder(assetPath))
                {
                    _totalAssetsToAnalyze++;
                }
            }

            if (_totalAssetsToAnalyze == 0)
            {
                _currentAnalysisStep = "No assets found";
                yield break;
            }

            _currentAnalysisStep = "Analyzing references...";
            yield return true;

            // Analyze each asset
            foreach (var guid in guids)
            {
                if (_shouldCancelAnalysis) yield break;

                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.IsValidFolder(assetPath))
                    continue;

                _processedAssets++;
                _analysisProgress = (float)_processedAssets / _totalAssetsToAnalyze;

                _currentAnalysisStep = $"Checking: {Path.GetFileName(assetPath)}";

                if (!HasReferences(assetPath))
                {
                    _unreferencedAssets.Add(assetPath);
                }

                // Yield control back to UI periodically
                yield return true;
            }
        }

        private bool HasReferences(string assetPath)
        {
            if (_shouldCancelAnalysis) return false;

            var allAssets = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/") && !AssetDatabase.IsValidFolder(path))
                .ToArray();

            foreach (var path in allAssets)
            {
                if (_shouldCancelAnalysis) return false;
                if (path == assetPath) continue;

                var dependencies = AssetDatabase.GetDependencies(path, false);
                if (dependencies.Contains(assetPath))
                {
                    return true;
                }
            }

            return false;
        }
    }
}