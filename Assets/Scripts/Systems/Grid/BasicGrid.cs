using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blizzard.Grid
{
    public class BasicGrid<T> : IGrid<T> where T : struct
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Raw grid data
        /// </summary>
        private T[] _data;

        /// <summary>
        /// Creates uninitialized grid with given dimensions, and optionally given flattened raw data.
        /// Assumes raw data respects given dimensions.
        /// </summary>
        public BasicGrid(int width, int height, T[] data = null)
        {
            if (data == null) _data = new T[width * height];
            else _data = data;
            Width = width;
            Height = height;
        }

        public void Initialize()
        {
            _data.Initialize();
        }

        public void Initialize(T value)
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    _data[GetFlattenedIndex(x, y)] = value;
                }
            }
        }

        public T GetAt(int x, int y)
        {
            ValidateGridPosition(x, y);
            return _data[GetFlattenedIndex(x, y)];
        }

        public T GetAt(Vector2Int gridPosition)
        {
            return GetAt(gridPosition.x, gridPosition.y);
        }

        public void SetAt(int x, int y, T value)
        {
            ValidateGridPosition(x, y);
            _data[GetFlattenedIndex(x, y)] = value;
        }

        public void SetAt(Vector2Int gridPosition, T value)
        {
            SetAt(gridPosition.x, gridPosition.y, value);
        }

        public T[] GetData()
        {
            return _data;
        }

        private void ValidateGridPosition(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException($"Grid position ({x}, {y}) out of range!");
            }
        }

        private int GetFlattenedIndex(int x, int y)
        {
            return y * Width + x;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator() as IEnumerator<T>;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

