
# Documentation

For the documentation, please visit:

https://carloslab-ai.github.io/UtilityIntelligence/

# Running Demos in URP and HDRP

Since this plugin doesn't have any graphical features, it is compatible with all render pipelines. However the materials of the demo scenes are created using the Built-In Render Pipeline. Therefore, if you want to run the demos in URP or HDRP, you need to convert all materials to the target pipeline first:

## URP

1. Open **Render Pipeline Converter** (Window -> Rendering -> Render Pipeline Converter)
2. Tick **Material Upgrade**
3. Click **Initialize and Converter** button.

## HDRP

1. Open **HDRP Wizard** (Window -> Rendering -> HDRP Wizard)
2. Click **Convert All Built-In Materials to HDRP**

# Upgrade Guide

To upgrade **Utility Intelligence** you need to do the following:
- Backup your project.
- Remove the following folders:
	- CarlosLab/Common
	- CarlosLab/UtilityIntelligence
- Re-import UtilityIntelligence package

-------------------------------------------------------------
Thank you for choosing **Utility Intelligence**! 🥰