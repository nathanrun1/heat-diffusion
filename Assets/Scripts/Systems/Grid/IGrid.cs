using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.Grid
{
    /// <summary>
    /// A 2D grid of cells, each cell containing data of type T
    /// </summary>
    public interface IGrid<T> : IEnumerable<T> where T : struct
    {
        /// <summary>
        /// Amount of rows in the grid
        /// </summary>
        public abstract int Height { get; }
        /// <summary>
        /// Amount of columns in the grid
        /// </summary>
        public abstract int Width { get; }

        /// <summary>
        /// Initializes all cell data in grid to default value of T
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Initializes all cell data in grid to given value
        /// </summary>
        public abstract void Initialize(T value);

        /// <summary>
        /// Gets cell data at given grid position
        /// </summary>
        public T GetAt(int x, int y);

        /// <summary>
        /// Gets cell data at given grid position
        /// </summary>
        public T GetAt(Vector2Int gridPosition);

        /// <summary>
        /// Sets cell data at given grid position to given value
        /// </summary>
        public void SetAt(int x, int y, T value);

        /// <summary>
        /// Sets cell data at given grid position to given value
        /// </summary>
        public void SetAt(Vector2Int gridPosition, T value);

        /// <summary>
        /// Returns raw & flattened data contained within the grid
        /// </summary>
        /// <returns></returns>
        public T[] GetData();
    }
}
