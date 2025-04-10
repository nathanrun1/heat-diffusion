

namespace Blizzard.Grid
{
    public class BasicWorldGrid<T> : BasicGrid<T>, IWorldGrid<T> where T : struct
    {
        public float CellHeight { get; private set; }

        public float CellWidth { get; private set; }

        public BasicWorldGrid(float cellHeight, float cellWidth, int width, int height, T[] data = null)
            : base(width, height, data)
        {
            CellHeight = cellHeight;
            CellWidth = cellWidth;
        }
    }
}