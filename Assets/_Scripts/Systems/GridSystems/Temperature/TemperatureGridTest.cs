using UnityEngine;
using Blizzard.Grid;

namespace Blizzard.Temperature
{
    public class TemperatureGridTest : MonoBehaviour
    {
        TemperatureGrid temperatureGrid;
        [SerializeField] private float _updateDelay = 1.0f;
        [SerializeField] private int _logInterval = 10;
        [SerializeField] private int _width;
        [SerializeField] private int _height;

        private float _timeSinceLastUpdate = 0.0f;
        private int _updateCount = 0;

        private void Start()
        {
            Grid<TemperatureCell> grid = new Grid<TemperatureCell>(_width, _height, 10);

            // Fill grid with random temperatures
            float temperatureSum = 0.0f;
            for (int x = 0; x < grid.Width; ++x)
            {
                for (int y = 0; y < grid.Height; ++y)
                {
                    TemperatureCell randTemp;
                    randTemp.temperature = Random.Range(-10, 11);
                    randTemp.insulation = 0;
                    randTemp.heatSource = 0;
                    grid.SetAt(x, y, randTemp);
                    temperatureSum += randTemp.temperature; 
                }
            }
            Debug.Log($"Predicted equilibrium: {temperatureSum / (float)(_width * _height)}");
            temperatureGrid = new TemperatureGrid(grid);
        }

        void FixedUpdate()
        {
            return;
            _timeSinceLastUpdate += Time.fixedDeltaTime;
            if (_timeSinceLastUpdate > _updateDelay)
            {
                _timeSinceLastUpdate -= _updateDelay;

                _updateCount = (_updateCount + 1) % _logInterval;
                if (_updateCount == 0) temperatureGrid.UpdateTemperatureAll(_updateDelay, 0, true);
                else temperatureGrid.UpdateTemperatureAll(_updateDelay, 0);
            }
        }
    }
}
