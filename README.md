# Asset Reference Analyzer

A powerful Unity Editor tool that helps you identify and clean up unreferenced assets in your project, keeping your builds lean and organized.

![Unity Version](https://img.shields.io/badge/Unity-2019.4%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![Platform](https://img.shields.io/badge/Platform-Editor%20Only-orange)

## 🚀 Features

- **📁 Folder-Specific Analysis**: Analyze any folder or subfolder in your project
- **🖱️ Context Menu Integration**: Right-click on any folder to instantly analyze it
- **⚡ Real-Time Progress**: Live progress tracking with cancellation support
- **🎯 Smart Detection**: Accurately identifies truly unreferenced assets
- **🗑️ Safe Deletion**: Individual or batch deletion with confirmation dialogs
- **🔍 Asset Preview**: Shows asset icons and allows quick selection/highlighting
- **📊 Detailed Results**: Clear reporting of found unreferenced assets
- **🎨 Clean UI**: Responsive interface that works smoothly during analysis

## 📋 Requirements

- Unity 2019.4 or later
- Any render pipeline (Built-in, URP, HDRP)
- .NET Framework 4.7.1 or later

## 🛠️ Installation

### Method 1: Unity Package (.unitypackage)
1. Download the latest `AssetReferenceAnalyzer.unitypackage` from releases
2. In Unity, go to **Assets → Import Package → Custom Package...**
3. Select the downloaded file and click "Import"

### Method 2: Manual Installation
1. Download the source code
2. Place `AssetReferenceAnalyzer.cs` in your project's `Assets/Editor/` folder
3. Unity will automatically compile the script

## 🎯 Usage

### Quick Start - Context Menu
1. **Right-click** on any folder in the Project window
2. Select **"Analyze References in Folder"**
3. The analyzer window opens with your folder pre-selected
4. Click **"Analyze References"** to start

### Manual Selection
1. Open **Window → Asset Reference Analyzer**
2. Enter or browse to select your target folder
3. Click **"Analyze References"** to begin analysis

### Interface Guide

#### Folder Selection
- **Folder Field**: Enter path manually or use Browse/Current buttons
- **Browse**: Opens file dialog to select folder
- **Current**: Uses currently selected folder in Project window

#### Analysis Controls
- **Analyze References**: Start the analysis process
- **Cancel**: Stop analysis in progress (appears during analysis)
- **Progress Bar**: Shows real-time progress with percentage

#### Results Panel
- **Asset List**: Shows all unreferenced assets with icons
- **Select Button**: Highlights asset in Project window
- **Delete Button**: Removes individual asset (with confirmation)
- **Delete All**: Removes all unreferenced assets (with confirmation)

## 🔍 How It Works

The analyzer performs a comprehensive scan of your project:

1. **Asset Discovery**: Finds all assets in the selected folder
2. **Reference Checking**: Scans entire project for dependencies
3. **Smart Analysis**: Identifies assets with no incoming references
4. **Safe Reporting**: Lists only truly unreferenced assets

### What Gets Analyzed
- ✅ Textures and sprites
- ✅ Audio clips
- ✅ 3D models and meshes
- ✅ Materials and shaders
- ✅ Prefabs and ScriptableObjects
- ✅ Animation clips and controllers
- ✅ Fonts and UI assets
- ✅ Any other asset type

### What's Excluded
- ❌ Folders (only contents are analyzed)
- ❌ Assets referenced in scenes
- ❌ Assets referenced by other assets
- ❌ Assets referenced in code

## ⚠️ Safety Features

- **Confirmation Dialogs**: All deletions require explicit confirmation
- **Batch Progress**: Shows progress when deleting multiple assets
- **Cancellation**: Can cancel batch operations midway
- **Undo Support**: Deleted assets can be restored via Edit → Undo
- **Asset Database Integration**: Properly removes assets from Unity's database

## 🎨 Screenshots

### Main Interface
The clean, intuitive interface makes asset cleanup effortless:
- Clear folder selection with validation
- Real-time progress tracking
- Organized results with asset previews

### Context Menu Integration
Right-click any folder to instantly analyze it:
- Seamless workflow integration
- No need to manually navigate to folders
- Instant access from Project window

## 🔧 Technical Details

### Performance
- **Efficient Scanning**: Uses Unity's AssetDatabase for fast dependency checks
- **Responsive UI**: Processes assets in batches to maintain UI responsiveness
- **Memory Friendly**: Streams results without loading all assets into memory

### Compatibility
- **Unity Versions**: 2019.4 LTS through latest versions
- **Render Pipelines**: Built-in, URP, HDRP
- **Platforms**: Windows, macOS, Linux (Editor only)

## 📝 Common Use Cases

### Before Build Optimization
