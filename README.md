# Asset Reference Analyzer

A Unity Editor tool for finding and cleaning up unreferenced assets in your project.

![Unity Version](https://img.shields.io/badge/Unity-2019.4%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

- **Smart Detection**: Enhanced reference checking across scenes, prefabs, code, and more
- **Folder-Specific Analysis**: Analyze any folder or subfolder
- **Context Menu Integration**: Right-click on folders to analyze instantly
- **Configurable Scanning**: Toggle different detection methods
- **Safe Deletion**: Individual or batch deletion with confirmations
- **Real-Time Progress**: Live progress with cancellation support

## Installation

1. Download `AssetReferenceAnalyzer.cs`
2. Place it in your project's `Assets/Editor/` folder
3. Unity will automatically compile the script

## Usage

### Quick Start
1. Right-click any folder in Project window
2. Select **"Analyze References in Folder"**
3. Configure detection options (optional)
4. Click **"Analyze References"**

### Manual Access
- **Window → Asset Reference Analyzer**

## Detection Options

- **Scenes**: Check all scenes for asset references
- **Prefabs**: Scan prefabs and prefab variants
- **ScriptableObjects**: Check custom data containers
- **Animations**: Scan animation clips and controllers
- **Code References**: Search C# files for asset names/paths
- **Resources Folders**: Protect runtime-loaded assets
- **Addressables**: Check addressable asset system

## Safety Features

- ✅ Confirmation dialogs for all deletions
- ✅ Progress tracking with cancellation
- ✅ Asset preview and selection
- ✅ Resources folder protection
- ✅ Addressable asset protection

## Requirements

- Unity 2019.4 or later
- .NET Framework 4.7.1+
- Any render pipeline

## Best Practices

⚠️ **Always backup your project before batch deletion**

- Test thoroughly after cleanup
- Review results carefully before deleting
- Consider runtime asset loading patterns
- Use version control for safety

## Common Use Cases

- Pre-build optimization
- Cleaning imported asset packs
- Regular project maintenance
- Removing prototype assets

## License

MIT License - Free for commercial and personal use
