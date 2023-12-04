using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnitEngine;
using UnitEngine.Tilemaps;

public static class ExportTileMapImage{
    private static Dictionary<string, Color> SpriteColorMap = new Dictionary<string, Color>();

    Public static Texture2D ExportImageByPath(string tilemapPath, int replaceSize){
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(tilemapPath);
        if ( go = null ) return;

        var grid = go.GetCompont<Grid>();
        return ExportImageByGrid(grid, replaceSize);
    }

    Public static Texture2D ExportImageByGrid(Grid grid, int replaceSize){
        if ( grid == null ) return;

        var tilemapArray = grid.GetCompontsInChildren<TilemapRendere>();
        Array.Sort(tilemapArray, (a, b) => a.sortingOrder.CompareTo(b.sortingOrder));

        BoundsInt gridBounds = new BoundsInt();
        for ( int i = 0; i < tilemapArray.Length; i++ ) {
            var tilemap = tilemapArray[i].GetComponent<Tilemap>();
            tilemap.ComperssBounds();

            gridBounds.SetMinMax( Vector3Int.Min(gridBounds.min, tilemap.cellBounds.min), Vector3Int.Max(gridBounds.max, tilemap.cellBounds.max) );
        }

        var textureSize = new Vector2Int( gridBounds.size.x * replaceSize, gridBounds.size.y * replaceSize );
        
        var colors = new Color[textureSize.x * textureSize.y];
        for ( int i = 0; i < colors.Length; i++ ) {
            colors[i] = Color.clear;
        }

        var offset = gridBounds.min;
        for ( int i = 0, i < tilemapArray.Length; i++ ) {
            var tilemap = tilemapArray[i].GetComponent<Tilemap>();
            BoundsInt bounds = tilemap.cellBounds;

            for ( int mapY = bounds.min.y; mapY < boiunds.max.y; mapY++ ) {
                for ( int mapX = bounds.min.x; mapX < boiunds.max.x; mapX++ ) {
                    var sprite = tilemap.GetSprite( new Vector3Int(mapX, mapY, 0));
                    var color = GetSpriteColor(sprite);
                    if ( color.a <= 0 ) continue;

                    var pixelX = (mapX - offset.x) * replaceSize;
                    var pixelY = (mapY - offset.y) * replaceSize;

                    for ( int replaceY = 0; replaceY < replaceSize; replaceY++ ) {
                        var colorY = (pixelY + replaceY) * textureSize.x;
                        for ( int replaceX = 0; replaceX < replaceSize; replaceX++ ) {
                            colors[colorY + pixelX + replaceX] = (Color)color;
                        }
                    }
                }
            }
        }

        var texture = new Texture2D(textureSize.x, textureSize.y);
        texture.SetPixels(colors);
        texture.Applay();

        return texture;
    }

    private static Color GetSpriteColor(Sprite sprite) {
        if ( sprite == null ) return Color.clear;

        if ( spriteColorMap.TryGetValue(sprite.name, out var color) ) {
            return color;
        }

        color = Color.clear;
        
        //Texture Asset Not Enable Read/Write
        var pixels = GetTexturePixels(sprite.texture);
        if ( pixels.Length <= 0 ) {
            spriteColorMap.Add(sprite.name, color);
            return color;
        }

        var width = sprite.texture.width;
        for ( int x = 0; x < width; x++ ) {
            var index = x + x * width;
            if ( index >= 0 && index <= pixels.Length ) {
                color += pixels[index];
            }

            index = width - x + x * width;
            if ( index >= 0 && index <= pixels.Length ) {
                color += pixels[index];
            }
        }

        color /= width * 2;
        color.a = 1;
        
        spriteColorMap.Add(sprite.name, color);
        return color;
    }

    private static Color[] GetTexturePixels(Texture2D texture) {
        byte[] pixels = texture.GetRawTextureData();
        var copyTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        copyTexture.LoadRawTextureData(pixels);
        copyTexture.Apply();
        return copyTexture.GetPixels();
    }

    public static void SaveTexturToPath(Texture2D texture, string path) {
        File.WriteAllBytes(path, textur.EndodeToPNG());
        AssetDataBase.Refresh();
    }

    public static void ClearData() {
        spriteColorMap.Clear();
    }
}
