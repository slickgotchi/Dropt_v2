using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace Tilemaps.Editor
{
    [CustomEditor(typeof(TileMapTreeGenerator))]
    public sealed class TileMapTreeGeneratorEditor : UnityEditor.Editor
    {
        public static void SwitchToStatic(Tilemap source, Tilemap target)
        {
            Dictionary<Vector3Int, TileBase> tiles = new Dictionary<Vector3Int, TileBase>();
            Dictionary<string, TileBase> cache = new Dictionary<string, TileBase>();

            foreach (var pos in source.cellBounds.allPositionsWithin)
            {
                if (!source.HasTile(pos))
                    continue;

                var tile = source.GetTile(pos);

                if ((tile is RuleTile))
                {
                    var sprite = source.GetSprite(pos);

                    if (null == sprite)
                        continue;

                    if (!cache.ContainsKey(sprite.name))
                    {
                        var assets = AssetDatabase.FindAssets(sprite.name).Where(
                            temp => AssetDatabase.GUIDToAssetPath(temp).EndsWith(sprite.name + ".asset"));

                        var asset = assets.FirstOrDefault();
                        if (!string.IsNullOrEmpty(asset))
                        {
                            tile = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(asset));
                        }

                        cache[sprite.name] = tile;
                    }

                    tile = cache[sprite.name];

                    if (tile != null)
                    {
                        tiles[pos] = tile;
                    }
                }
                else
                {
                    tiles[pos] = tile;
                }
            }

            if (source == target)
            {
                target.ClearAllTiles();
            }

            foreach (var item in tiles)
            {
                target.SetTile(item.Key, item.Value);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var treeGenerator = target as TileMapTreeGenerator;
            var tilemap = treeGenerator.GetComponent<Tilemap>();

            if (GUILayout.Button("Clear"))
            {
                Undo.RecordObject(tilemap, "Tile Map Tree Generator");
                Clear(tilemap);

                foreach (var layer in treeGenerator.Layers)
                {
                    if (layer.TargetLayer == tilemap)
                        continue;

                    ClearAll(layer.TargetLayer);
                }
            }

            if (GUILayout.Button("Generate"))
            {
                Undo.RecordObject(tilemap, "Tile Map Tree Generator");

                treeGenerator.Seed = DateTime.Now.Millisecond;

                Clear(tilemap);

                foreach (var layer in treeGenerator.Layers)
                {
                    if (layer.TargetLayer == tilemap)
                        continue;

                    ClearAll(layer.TargetLayer);
                }

                SwitchToStatic(tilemap, tilemap);

                Generate(treeGenerator, tilemap);

                SwitchLayersToStatic(treeGenerator);
            }

            if (GUILayout.Button("Generate By Seed"))
            {
                Undo.RecordObject(tilemap, "Tile Map Tree Generator");

                Clear(tilemap);

                foreach (var layer in treeGenerator.Layers)
                {
                    if (layer.TargetLayer == tilemap)
                        continue;

                    ClearAll(layer.TargetLayer);
                }

                SwitchToStatic(tilemap, tilemap);

                Generate(treeGenerator, tilemap);

                SwitchLayersToStatic(treeGenerator);
            }
        }

        private void SwitchLayersToStatic(TileMapTreeGenerator treeGenerator)
        {
            HashSet<Tilemap> processedMaps = new HashSet<Tilemap>();
            foreach (var layer in treeGenerator.Layers)
            {
                if (processedMaps.Add(layer.TargetLayer))
                {
                    SwitchToStatic(layer.TargetLayer, layer.TargetLayer);
                }
            }
        }

        private void Generate(TileMapTreeGenerator treeGenerator, Tilemap tilemap)
        {
            System.Random random = new System.Random(treeGenerator.Seed);

            Dictionary<int, Vector3Int> usedFields = new Dictionary<int, Vector3Int>();

            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (pos.z != 0 || !tilemap.HasTile(pos) || null == tilemap.GetSprite(pos))
                    continue;

                usedFields[GetKey(pos.x, pos.y)] = pos;
            }

            foreach (var layer in treeGenerator.Layers)
            {
                if (!layer.Enabled)
                {
                    continue;
                }

                if (layer.Height <= 0)
                {
                    GenerateMainLayer(layer, usedFields, tilemap, random);
                }
                else if (layer.Patterns.Length > 0)
                {
                    GeneratePatternLayer(layer, usedFields, tilemap, random);
                }
                else
                {
                    GenerateDepthLayer(layer, usedFields, random);
                }
            }
        }

        private void GeneratePatternLayer(GeneratorLayer layer, Dictionary<int, Vector3Int> usedFields,
            Tilemap tilemap, Random random)
        {
            foreach (var pos in usedFields.Values)
            {
                if (!usedFields.ContainsKey(GetKey(pos.x, pos.y - 1)))
                {
                    var pattern = GetRandom(random, layer.Patterns);
                    TryToDrawPattern(layer, tilemap, pattern, new Vector3Int(pos.x, pos.y - 1));
                }
            }
        }

        private void TryToDrawPattern(GeneratorLayer layer, Tilemap tilemap, Tilemap pattern, Vector3Int offset)
        {
            if (null == pattern)
                return;

            int minX = int.MaxValue;
            int maxY = int.MinValue;

            foreach (var pos in pattern.cellBounds.allPositionsWithin)
            {
                if (!pattern.HasTile(pos))
                    continue;

                minX = Math.Min(minX, pos.x);
                maxY = Math.Max(maxY, pos.y);
            }

            foreach (var pos in pattern.cellBounds.allPositionsWithin)
            {
                if (!pattern.HasTile(pos))
                    continue;

                var newPos = pos + offset - new Vector3Int(minX, maxY);
                newPos.z = offset.z;

                if (pos.y == maxY)
                {
                    bool isContainAboveTile = tilemap.HasTile(new Vector3Int(newPos.x, newPos.y + 1));

                    if (!isContainAboveTile)
                        return;
                }

                if (tilemap.HasTile(new Vector3Int(newPos.x, newPos.y)) || layer.TargetLayer.HasTile(new Vector3Int(newPos.x, newPos.y)))
                    return;

                if (tilemap.HasTile(newPos) || layer.TargetLayer.HasTile(newPos))
                    return;
            }

            foreach (var pos in pattern.cellBounds.allPositionsWithin)
            {
                if (!pattern.HasTile(pos))
                    continue;

                var newPos = pos + offset - new Vector3Int(minX, maxY);
                newPos.z = offset.z;

                layer.TargetLayer.SetTile(newPos, pattern.GetTile(pos));
            }
        }

        private void GenerateDepthLayer(GeneratorLayer layer, Dictionary<int, Vector3Int> usedFields, System.Random random)
        {
            foreach (var pos in usedFields.Values)
            {
                var tile = GetRandom(random, layer.Tiles);

                for (int y = 1; y <= layer.Height; y++)
                {
                    layer.TargetLayer.SetTile(new Vector3Int(pos.x, pos.y - y, -1), tile);
                }
            }
        }

        private void GenerateMainLayer(GeneratorLayer layer, Dictionary<int, Vector3Int> usedFields,
            Tilemap tilemap, System.Random random)
        {
            if (layer.Tiles.Length == 0)
                return;

            foreach (var pos in usedFields.Values)
            {
                if (!tilemap.HasTile(pos))
                    continue;

                int sides = 0;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        if (usedFields.ContainsKey(GetKey(pos.x + x, pos.y + y)))
                            sides++;
                    }
                }

                if (sides >= 8)
                {
                    var newPos = pos;
                    layer.TargetLayer.SetTile(newPos, GetRandom(random, layer.Tiles));
                }
            }
        }

        private void Clear(Tilemap tilemap)
        {
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (pos.z == 0 || !tilemap.HasTile(pos))
                    continue;

                tilemap.SetTile(pos, null);
            }
        }

        private void ClearAll(Tilemap tilemap)
        {
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                tilemap.SetTile(pos, null);
            }
        }

        private T GetRandom<T>(Random random, T[] array)
        {
            if (array.Length == 0)
                return default;

            var index = random.Next(0, array.Length);
            return array[index];
        }

        private int GetKey(int x, int y)
        {
            return y * 10000 + x;
        }
    }
}