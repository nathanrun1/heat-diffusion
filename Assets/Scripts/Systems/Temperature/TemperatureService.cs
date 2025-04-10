using UnityEngine;
using Blizzard.Grid;
using System.Collections;
using UnityEditor.PackageManager.UI;


namespace Blizzard.Temperature
{
    public class TemperatureService
    {
        /// <summary>
        /// Grid containing temperature data for a large set region of the scene
        /// </summary>
        public IWorldGrid<TemperatureCell> Grid { get; private set; }
        /// <summary>
        /// Texture displaying a heatmap of the current computed subregion of the scene.
        /// </summary>
        public RenderTexture HeatmapTexture { get; private set; }

        /// <summary>
        /// Subregion of grid that gets updated at each step
        /// </summary>
        private IGrid<TemperatureCell> _window;

        /// <summary>
        /// Whether or not heat diffusion simulation is currently enabled
        /// </summary>
        private bool _doHeatDiffusion = false;
        /// <summary>
        /// Compute shader used to compute heat diffusion
        /// </summary>
        private ComputeShader _heatDiffusionShader;
        /// <summary>
        /// Stores temperature data copied from window to compute shader
        /// </summary>
        private ComputeBuffer _inputBuffer;
        /// <summary>
        /// Stores temperature data copied from compute shader to window
        /// </summary>
        private ComputeBuffer _outputBuffer;

        public TemperatureService(IWorldGrid<TemperatureCell> grid, IGrid<TemperatureCell> window, ComputeShader heatDiffusionShader)
        {
            Grid = grid;
            _window = window;
            _heatDiffusionShader = heatDiffusionShader;
            SetupComputeShader();
            SetupHeatmap();
        }

        ~TemperatureService()
        {
            DisposeComputeBuffers();
        }

        /// <summary>
        /// Compute a single heat diffusion step using given delta time. Updates temperature grid and heatmap texture.
        /// </summary>
        public void DoHeatDiffusionStep(float deltaTime)
        {
            UpdateActiveSubgrid(new(0, 0)); // TODO: get offset from somewhere
            _heatDiffusionShader.SetFloat("deltaTime", deltaTime);

            int threadGroupSize =
                TemperatureConstants.ComputeThreadGroupDimensions.x *
                TemperatureConstants.ComputeThreadGroupDimensions.y;
            _heatDiffusionShader.Dispatch(0, (_window.Width * _window.Height) / threadGroupSize, 1, 1);
            _outputBuffer.GetData(_window.GetData()); // TODO: async this (currently waits for GPU to finish)
            Grid.ReadFromSubgrid(_window, new(0, 0)); // TODO: get offset from somewhere

            Debug.Log(_window.Width);
            _heatDiffusionShader.Dispatch(1, _window.Width, _window.Height, 1); // Render heatmap
        }





        /// <summary>
        /// Populates temperature grid with random temperatures between given range
        /// </summary>
        private void PopulateRandomTemperatureData(float minInclusive, float maxInclusive)
        {
            for (int x = 0; x < Grid.Width; ++x)
            {
                for (int y = 0; y < Grid.Height; ++y)
                {
                    TemperatureCell cur = Grid.GetAt(x, y);
                    Grid.SetAt(x, y, new TemperatureCell
                    {
                        temperature = Random.Range(minInclusive, maxInclusive),
                        insulation = cur.insulation,
                        heat = cur.heat
                    });
                }
            }
        }

        /// <summary>
        /// Fetches data from main grid into window grid, and copies it to input compute buffer.
        /// </summary>
        /// <param name="offset"></param>
        private void UpdateActiveSubgrid(Vector2Int offset)
        {
            Grid.WriteToSubgrid(_window, offset);
            _inputBuffer.SetData(_window.GetData());
        }
        
        /// <summary>
        /// Sets paramaters for the compute shader.
        /// </summary>
        private void SetupComputeShader()
        {
            // Initialize compute buffers
            _inputBuffer = new ComputeBuffer(_window.Width * _window.Height, TemperatureCell.GetSize());
            _outputBuffer = new ComputeBuffer(_window.Width * _window.Height, TemperatureCell.GetSize());

            _heatDiffusionShader.SetBuffer(0, "prev", _inputBuffer);
            _heatDiffusionShader.SetBuffer(0, "next", _outputBuffer);
            _heatDiffusionShader.SetInt("width", _window.Width);
            _heatDiffusionShader.SetInt("height", _window.Height);
            _heatDiffusionShader.SetInt("selectedX", 0);
            _heatDiffusionShader.SetInt("selectedY", 0);
            _heatDiffusionShader.SetFloat("diffusionFactor", TemperatureConstants.DiffusionFactor);
        }

        /// <summary>
        /// Initializes heatmap texture and sets associated parameters
        /// </summary>
        private void SetupHeatmap()
        {
            // Setup heatmap texture
            HeatmapTexture = new RenderTexture(_window.Width * 8, _window.Height * 8, 1);
            HeatmapTexture.enableRandomWrite = true;
            HeatmapTexture.format = RenderTextureFormat.ARGBFloat;
            HeatmapTexture.Create();

            _heatDiffusionShader.SetBuffer(1, "prev", _inputBuffer);
            _heatDiffusionShader.SetTexture(1, "heatmap", HeatmapTexture);
        }

        private void DisposeComputeBuffers()
        {
            _inputBuffer.Dispose();
            _outputBuffer.Dispose();
        }
    }
}
