using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blizzard.Grid
{
    public class Grid<T> : IGrid<T> where T : struct
    {
        public float CellSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private T[,] _cells;

        public Grid(int width, int height, float cellSize)
        {
            _cells = new T[width, height];

            Width = width;
            Height = height;
            CellSize = cellSize;
        }

        public void Initialize()
        {
            _cells.Initialize();
        }

        public void Initialize(T value)
        {
            for (int i = 0; i < Height; ++i)
            {
                for (int j = 0; i < Width; ++j)
                {
                    _cells[i, j] = value;
                }
            }
        }

        public T GetAt(int x, int y)
        {
            ValidateGridPosition(x, y);

            return _cells[x, y];
        }

        public T GetAt(Vector2Int gridPosition)
        {
            return GetAt(gridPosition.x, gridPosition.y);
        }

        public void SetAt(int x, int y, T value)
        {
            ValidateGridPosition(x, y);
            _cells[x, y] = value;
        }

        public void SetAt(Vector2Int gridPosition, T value)
        {
            SetAt(gridPosition.x, gridPosition.y, value);
        }

        private void ValidateGridPosition(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException($"Grid position ({x}, {y}) out of range!");
            }
        }
    }
}

