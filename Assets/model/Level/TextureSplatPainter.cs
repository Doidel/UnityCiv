using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using TerrainGenerator;

public class TextureSplatPainter {

    public static ReturnedMaps Paint(Vector3 terrainWorldPos, Dictionary<Point, Tile> affectedTiles, TerrainChunkSettings settings)
    {
        var gm = GridManager.instance;
        Vector3 initPos = gm.calcInitPos();

        var map = new float[settings.AlphamapResolution, settings.AlphamapResolution, 5]; // last nr for the x different textures
        var heights = new float[settings.HeightmapResolution, settings.HeightmapResolution];
        for (var y = 0; y < settings.AlphamapResolution; y++)
        {
            for (var x = 0; x < settings.AlphamapResolution; x++)
            {

                var normX = x * 1.0 / (settings.AlphamapResolution - 1);
                var normY = y * 1.0 / (settings.AlphamapResolution - 1);

                var wpX = (float)normY * settings.Length + terrainWorldPos.x;
                var wpZ = (float)normX * settings.Length + terrainWorldPos.z;
                //var wpX = (float)normX * settings.Length * settings.Scale + terrainWorldPos.x;
                //var wpZ = (float)normY * settings.Length * settings.Scale + terrainWorldPos.z;
                float py = -(float)(wpZ - initPos.z) / (gm.hexHeight * 0.75f);
                float px = (wpX - initPos.x) / gm.hexWidth - py / 2;

                var cube_pos = gm.hex_to_cube(new Vector2(px, py));
                var rr = cube_roundvals(cube_pos);

                // test for uniform cube coordinates
                /*if ((rr.positions[0].x + rr.positions[0].y + rr.positions[0].z != 0) ||
                    (rr.positions[1].x + rr.positions[1].y + rr.positions[1].z != 0) ||
                    (rr.positions[2].x + rr.positions[2].y + rr.positions[2].z != 0))
                    throw new ArgumentException("not uniform");*/


                for (int i = 0; i < 3; i++)
                {
                    var ht = gm.cube_to_hex(rr.positions[i]);
                    var p = new Point((int)ht.x, (int)ht.y);
                    //if (affectedTiles.ContainsKey(p))
                    Tile tile;
                    if (affectedTiles.TryGetValue(p, out tile))
                    {
                        //var tile = affectedTiles[p];
                        /*if (x >= map.GetLength(0) || y >= map.GetLength(1))
                            throw new InvalidProgramException("x or y too high!");
                        if ((int)tile.Type >= 5)
                            throw new InvalidProgramException("tileindex out of range");
                        if (i >= rr.fracts.Count())
                            throw new InvalidProgramException("rr out of range!");*/
                        // mix tex colors for splatting
                        map[x, y, (int)tile.Type] += Math.Abs(rr.fracts[i]);
                        // calc terrain height
                        var tileHeight = tile.Type == Tile.TerrainType.RIVER ? 0.01f : 0.1f;
                        heights[x, y] += Math.Abs(rr.fracts[i]) * tileHeight;
                        // height is power of two + one (http://answers.unity3d.com/questions/581760/why-are-heightmap-resolutions-power-of-2-plus-one.html)
                        // so we'll extend the heightmap here, i.e. cheat...
                        if (x == settings.AlphamapResolution - 1 && y == settings.AlphamapResolution - 1)
                        {
                            heights[x + 1, y + 1] = heights[x, y];
                        }
                        if (x == settings.AlphamapResolution - 1)
                        {
                            heights[x + 1, y] = heights[x, y];
                        }
                        if (y == settings.AlphamapResolution - 1)
                        {
                            heights[x, y + 1] = heights[x, y];
                        }
                    }
                    else
                    {
                        //Debug.Log("Couldn't access tile. Must not happen!");
                    }
                }
            }
        }

        for (int hx = 0; hx < heights.GetLength(0); hx++)
        {
            for (int hy = 0; hy < heights.GetLength(1); hy++)
            {
                //if (heights[hx, hy] < 0.009f)
                    //Debug.Log("still some left");
            }
        }

        // TODO: make another height pass where height map is blurred, fractal drawing, using sinuses, whatever...
        return new ReturnedMaps() { Alphamap = map, Heightmap = heights };
    }

    public class ReturnedMaps
    {
        public float[,,] Alphamap;
        public float[,] Heightmap;
    }

    // See http://www.redblobgames.com/grids/hexagons/#rounding
    private static RoundResult cube_roundvals(Vector3 cubeInput)
    {
        float rx = (float)Math.Round(cubeInput.x);
        float ry = (float)Math.Round(cubeInput.y);
        float rz = (float)Math.Round(cubeInput.z);

        var x_diff = Math.Abs(rx - cubeInput.x);
        var y_diff = Math.Abs(ry - cubeInput.y);
        var z_diff = Math.Abs(rz - cubeInput.z);

        // when one is resetted, get the other two's fractional by extrapolating the resetted one and reset once the first, then the other
        

        RoundResult rr = new RoundResult();

        //reset largest rounding change
        if (x_diff > y_diff && x_diff > z_diff)
        {
            rx = -ry - rz;
            var extrapolatedRx = rx + (cubeInput.x - rx > 0 ? 1 : -1);
            var diff = extrapolatedRx + ry + rz;

            rr.positions[0] = new Vector3(rx, ry, rz);
            rr.positions[1] = new Vector3(extrapolatedRx, ry - diff, rz);
            rr.positions[2] = new Vector3(extrapolatedRx, ry, rz - diff);
            rr.fracts[0] = (1 - x_diff);
            rr.fracts[1] = (x_diff / (y_diff + z_diff)) * y_diff;
            rr.fracts[2] = (x_diff / (y_diff + z_diff)) * z_diff;
        }
        else if (y_diff > z_diff)
        {
            ry = -rx - rz;
            var extrapolatedRy = ry + (cubeInput.y - ry > 0 ? 1 : -1);
            var diff = extrapolatedRy + rx + rz;

            rr.positions[0] = new Vector3(rx - diff, extrapolatedRy, rz);
            rr.positions[1] = new Vector3(rx, ry, rz);
            rr.positions[2] = new Vector3(rx, extrapolatedRy, rz - diff);
            rr.fracts[0] = (y_diff / (x_diff + z_diff)) * x_diff;
            rr.fracts[1] = (1 - y_diff);
            rr.fracts[2] = (y_diff / (x_diff + z_diff)) * z_diff;
        }
        else {
            rz = -rx - ry;
            var extrapolatedRz = rz + (cubeInput.z - rz > 0 ? 1 : -1);
            var diff = extrapolatedRz + rx + ry;

            rr.positions[0] = new Vector3(rx - diff, ry, extrapolatedRz);
            rr.positions[1] = new Vector3(rx, ry - diff, extrapolatedRz);
            rr.positions[2] = new Vector3(rx, ry, rz);
            rr.fracts[0] = (z_diff / (x_diff + y_diff)) * x_diff;
            rr.fracts[1] = (z_diff / (x_diff + y_diff)) * y_diff;
            rr.fracts[2] = (1 - z_diff);
        }

        return rr;
    }

    private class RoundResult
    {
        public float[] fracts = new float[3];
        public Vector3[] positions = new Vector3[3];
    }
}
