using UnityEditor;
using UnityEngine;

public class SpriteSingleModeImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Solo afecta a imágenes dentro de la carpeta IMG_Tuto
        if (!assetPath.Contains("/IMG_Tuto/"))
            return;

        TextureImporter textureImporter = (TextureImporter)assetImporter;

        if (textureImporter.importSettingsMissing || textureImporter.textureType != TextureImporterType.Sprite)
        {
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.alphaIsTransparency = true;
        }
    }
}