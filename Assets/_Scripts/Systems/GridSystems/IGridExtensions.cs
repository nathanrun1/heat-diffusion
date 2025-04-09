using System;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Blizzard.Grid
{
    public static class IGridExtensions
    {
        /// <summary>
        /// Converts world position to grid position
        /// </summary>
        public static Vector2Int WorldToCell<T>(this IGrid<T> grid, Vector2 worldPosition) where T : struct
        {
            Vector2Int gridPosition = new Vector2Int();
            gridPosition.x = Mathf.FloorToInt(worldPosition.x / grid.CellSize);
            gridPosition.y = Mathf.FloorToInt(worldPosition.y / grid.CellSize);

            return gridPosition;
        }

        /// <summary>
        /// Converts grid position to world position of bottom left of grid square
        /// </summary>
        public static Vector2 CellToWorld<T>(this IGrid<T> grid, Vector2Int gridPosition) where T : struct
        {
            Vector2 worldPosition;
            worldPosition.x = gridPosition.x * grid.CellSize;
            worldPosition.y = gridPosition.y * grid.CellSize;

            return worldPosition;
        }

        /// <summary>
        /// Gets row of cells from given y position
        /// </summary>
        public static T[] GetRow<T>(this IGrid<T> grid, int y) where T : struct
        {
            T[] row = new T[grid.Width];
            for (int i = 0; i < grid.Width; ++i)
            {
                row[i] = grid.GetAt(i, y);
            }
            return row;
        }

        /// <summary>
        /// Gets column of cells from given x position
        /// </summary>
        public static T[] GetCol<T>(this IGrid<T> grid, int x) where T : struct
        {
            T[] col = new T[grid.Height];
            for (int i = 0; i < grid.Height; ++i)
            {
                col[i] = grid.GetAt(x, i);
            }
            return col;
        }

        /// <summary>
        /// Gets array of adjacent cells to cell at given grid position, assuming given grid position is valid. Neighbors to the left or below (x - 1 / y - 1) can be optionally given.
        /// </summary>
        public static void GetNeighborsAt<T>(this IGrid<T> grid, Vector2Int gridPosition, out T[] neighbors, T? left = null, T? down = null) where T : struct
        {
            neighbors = new T[4];
            int neighborCount = 0;
            if (gridPosition.x != 0 && left == null)
            {
                neighbors[neighborCount] = (grid.GetAt(gridPosition.x - 1, gridPosition.y));
                neighborCount++;
                //Debug.Log($"nbr left at {gridPosition.x - 1}, {gridPosition.y}with value {grid.GetAt(gridPosition.x - 1, gridPosition.y).ToString()}");
            } 
            else if (left != null)
            {
                neighbors[neighborCount] = (T)left;
                neighborCount++;
                //Debug.Log($"nbr left given with value {left.ToString()}");
            }

            if (gridPosition.x != grid.Width - 1)
            {
                neighbors[neighborCount] = grid.GetAt(gridPosition.x + 1, gridPosition.y);
                neighborCount++;
                //Debug.Log($"nbr right at {gridPosition.x + 1}, {gridPosition.y} with value {grid.GetAt(gridPosition.x + 1, gridPosition.y).ToString()}");
            }

            if (gridPosition.y != 0 && down == null)
            {
                neighbors[neighborCount] = grid.GetAt(gridPosition.x, gridPosition.y - 1);
                neighborCount++;
                //Debug.Log($"nbr down at {gridPosition.x}, {gridPosition.y - 1}");
            }
            else if (down != null)
            {
                neighbors[neighborCount] = (T)down;
                neighborCount++;
                //Debug.Log($"nbr down given with value {down.ToString()}");
            }

            if (gridPosition.y != grid.Height - 1)
            {
                neighbors[neighborCount] = grid.GetAt(gridPosition.x, gridPosition.y + 1);
                neighborCount++;
                //Debug.Log($"nbr up at {gridPosition.x}, {gridPosition.y + 1}");
            }
            Array.Resize(ref neighbors, neighborCount);
        }

        /// <summary>
        /// Logs contents of grid to Unity debug console
        /// </summary>
        public static void Log<T>(this IGrid<T> grid) where T : struct
        {
            string log = $"-- CPU Grid, Width = {grid.Width}, Height = {grid.Height} --\n";
            for (int y = 0; y < grid.Height; ++y)
            {
                for (int x = 0; x < grid.Width; ++x)
                {
                    if (x == 0)
                    {
                        log += $"[ {grid.GetAt(x, y).ToString()},"; // Row prefix
                    }
                    else if (x == grid.Width - 1)
                    {
                        log += $" {grid.GetAt(x, y).ToString()} ]\n"; // Row suffix
                    }
                    else
                    {
                        log += $" {grid.GetAt(x, y).ToString()},";
                    }
                }
            }
            Debug.Log(log);
        }
    }
}
