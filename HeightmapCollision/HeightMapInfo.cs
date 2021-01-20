#region File Description
//-----------------------------------------------------------------------------
// HeightMapInfo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
#endregion

namespace HeightmapCollision
{
    /// <summary>
    /// HeightMapInfo is a collection of data about the heightmap. It includes
    /// information about how high the terrain is, and how far apart each vertex is.
    /// It also has several functions to get information about the heightmap, including
    /// its height at different points, and whether a point is on the heightmap.
    /// It is the runtime equivalent of HeightMapInfoContent.
    /// </summary>
    public class HeightMapInfo
    {
        #region Private fields


        // TerrainScale is the distance between each entry in the Height property.
        // For example, if TerrainScale is 30, Height[0,0] and Height[1,0] are 30
        // units apart.        
        [ContentSerializer]
        public float TerrainScale{ get; private set; }

        // This 2D array of floats tells us the height that each position in the 
        // heightmap is.
        [ContentSerializer]
        public float[,] Height { get; private set; }

        // the position of the heightmap's -x, -z corner, in worldspace.
        [ContentSerializer]
        public Vector3 HeightmapPosition { get; private set; }

        // the total width of the heightmap, including terrainscale.
        [ContentSerializer]
        public float HeightmapWidth { get; private set; }

        // the total height of the height map, including terrainscale.
        [ContentSerializer]
        public float HeightmapHeight { get; private set; }


        #endregion

        private HeightMapInfo()
        {

        }

        // the constructor will initialize all of the member variables.
        public HeightMapInfo(float[,] heights, float terrainScale)
        {
            if (heights == null)
            {
                throw new ArgumentNullException("heights");
            }

            this.TerrainScale = terrainScale;
            this.Height = heights;

            HeightmapWidth = (heights.GetLength(0) - 1) * terrainScale;
            HeightmapHeight = (heights.GetLength(1) - 1) * terrainScale;

            HeightmapPosition = new Vector3(-(heights.GetLength(0) - 1) / 2 * terrainScale, 0, -(heights.GetLength(1) - 1) / 2 * terrainScale);
        }


        // This function takes in a position, and tells whether or not the position is 
        // on the heightmap.
        public bool IsOnHeightmap(Vector3 position)
        {
            // first we'll figure out where on the heightmap "position" is...
            Vector3 positionOnHeightmap = position - HeightmapPosition;

            // ... and then check to see if that value goes outside the bounds of the
            // heightmap.
            return (positionOnHeightmap.X > 0 &&
                positionOnHeightmap.X < HeightmapWidth &&
                positionOnHeightmap.Z > 0 &&
                positionOnHeightmap.Z < HeightmapHeight);
        }

        // This function takes in a position, and returns the heightmap's height at that
        // point. Be careful - this function will throw an IndexOutOfRangeException if
        // position isn't on the heightmap!
        // This function is explained in more detail in the accompanying doc.
        public float GetHeight(Vector3 position)
        {
            // the first thing we need to do is figure out where on the heightmap
            // "position" is. This'll make the math much simpler later.
            Vector3 positionOnHeightmap = position - HeightmapPosition;

            // we'll use integer division to figure out where in the "heights" array
            // positionOnHeightmap is. Remember that integer division always rounds
            // down, so that the result of these divisions is the indices of the "upper
            // left" of the 4 corners of that cell.
            int left, top;
            left = (int)positionOnHeightmap.X / (int)TerrainScale;
            top = (int)positionOnHeightmap.Z / (int)TerrainScale;

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = (positionOnHeightmap.X % TerrainScale) / TerrainScale;
            float zNormalized = (positionOnHeightmap.Z % TerrainScale) / TerrainScale;

            // Now that we've calculated the indices of the corners of our cell, and
            // where we are in that cell, we'll use bilinear interpolation to calculuate
            // our height. This process is best explained with a diagram, so please see
            // the accompanying doc for more information.
            // First, calculate the heights on the bottom and top edge of our cell by
            // interpolating from the left and right sides.
            float topHeight = MathHelper.Lerp(
                Height[left, top],
                Height[left + 1, top],
                xNormalized);

            float bottomHeight = MathHelper.Lerp(
                Height[left, top + 1],
                Height[left + 1, top + 1],
                xNormalized);

            // next, interpolate between those two values to calculate the height at our
            // position.
            return MathHelper.Lerp(topHeight, bottomHeight, zNormalized);
        }
    }



    /// <summary>
    /// This class will load the HeightMapInfo when the game starts. This class needs 
    /// to match the HeightMapInfoWriter.
    /// </summary>
    //public class HeightMapInfoReader : ContentTypeReader<HeightMapInfo>
    //{
    //    protected override HeightMapInfo Read(ContentReader input,
    //        HeightMapInfo existingInstance)
    //    {
    //        float terrainScale = input.ReadSingle();
    //        int width = input.ReadInt32();
    //        int height = input.ReadInt32();
    //        float[,] heights = new float[width, height];

    //        for (int x = 0; x < width; x++)
    //        {
    //            for (int z = 0; z < height; z++)
    //            {
    //                heights[x, z] = input.ReadSingle();
    //            }
    //        }
    //        return new HeightMapInfo(heights, terrainScale);
    //    }
    //}
}
