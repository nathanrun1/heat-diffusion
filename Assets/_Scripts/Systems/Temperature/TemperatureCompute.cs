using Blizzard.Temperature;
using Blizzard.Grid;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Collections;
using UnityEngine.InputSystem;
using TMPro;

namespace Blizzard.Temperature
{
    public class TemperatureCompute : MonoBehaviour
    {
        [SerializeField] private ComputeShader tempCompute;
        public RenderTexture renderTexture;

        [SerializeField] private int totalGridWidth;
        [SerializeField] private int totalGridHeight;
        [SerializeField] private int localGridWidth;
        [SerializeField] private int localGridHeight;

        [SerializeField] private bool doCompute = true;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField, Range(0.5f, 20f)] private float timeScale = 1.0f;
        [SerializeField] private TextMeshProUGUI _curTempText;

        [Header("Real time modifications")]
        [SerializeField] private float setTemperature = 10.0f;
        [SerializeField] private float setInsulation = 0.0f;
        [SerializeField] private float setSourceHeat = 0.0f;


        private TemperatureCell[] temperatureData;
        private TemperatureGrid testCPUGrid = null;

        private ComputeBuffer prevBuffer;
        private ComputeBuffer nextBuffer;
        TemperatureCell[] window;

        private Vector2Int selectedCell = new(0, 0);

        /// <summary>
        /// Populates temperature data with random temperature values, with padding of 1 to avoid compute shader out of bounds.
        /// </summary>
        private void PopulateTemperatureData()
        {
            int resolution = totalGridWidth * totalGridHeight;
            int localResolution = localGridWidth * localGridHeight;
            temperatureData = new TemperatureCell[resolution];
            for (int y = 0; y < totalGridHeight; ++y)
            {
                for (int x = 0; x < totalGridWidth; ++x)
                {
                    int i = y * totalGridWidth + x; 
                    float randTemp = Random.Range(0, 0f);
                    temperatureData[i].temperature = randTemp;
                    temperatureData[i].heatSource = 0f;
                    temperatureData[i].insulation = 0f;
                    if (testCPUGrid != null) testCPUGrid.grid.SetAt(x, y, new TemperatureCell { temperature = randTemp });

                    // temp
                    //if (x == 5) temperatureData[i].insulation = 1f;
                    //if (x == 16 && y == 16) temperatureData[i].heatSource = -50f;
                }
            }

            // temp
            //temperatureData[0].heatSource = 50f;
            //temperatureData[16].heatSource = -50f;
        }

        /// <summary>
        /// Writes values from grid within specified subset (using offset & extent dimensions) into given subset grid. Assumes given
        /// subset grid is of correct dimensions.
        /// </summary>
        void GetGridSubset(TemperatureCell[] grid, ref TemperatureCell[] subset, int offsetX, int offsetY, int width, int height)
        {
            for (int y = offsetY; y < offsetY + height; ++y)
            {
                for (int x = offsetX; x < offsetX + width; ++x)
                {
                    int index = y * totalGridWidth + x;
                    int subsetIndex = (y - offsetY) * width + (x - offsetX);
                    subset[subsetIndex] = grid[index];
                }
            }
        }

        /// <summary>
        /// Reads values from given subset grid within specified subset (using offset & extent dimensions) into grid. Assumes given
        /// subset grid is of correct dimensions.
        /// </summary>
        void SetGridSubset(TemperatureCell[] grid, ref TemperatureCell[] subset, int offsetX, int offsetY, int width, int height)
        {
            for (int y = offsetY; y < offsetY + height; ++y)
            {
                for (int x = offsetX; x < offsetX + width; ++x)
                {
                    int index = y * totalGridWidth + x;
                    int subsetIndex = (y - offsetY) * width + (x - offsetX);
                    grid[index] = subset[subsetIndex];
                }
            }
        }

        /// <summary>
        /// Logs given data as grid to debug console
        /// </summary>
        private void LogGrid(float[] data)
        {
            string log = $"-- GRID, WIDTH: {localGridWidth}, HEIGHT: {localGridHeight}--\n";
            for (int i = 0; i < data.Length; ++i)
            {
                if (i % localGridWidth == 0) log += $"[ {data[i]},";
                else if (i % localGridWidth == localGridWidth - 1) log += $" {data[i]} ]\n";
                else log += $" {data[i]},";
            }
            Debug.Log(log);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _playerInput.inputActions.Player.Move.performed += (ctx) =>
            {
                Debug.Log("move performed");
                if (ctx.action.WasPressedThisFrame()) 
                {
                    Vector2 input = ctx.ReadValue<Vector2>();
                    selectedCell += new Vector2Int((int)input.x, (int)input.y);
                    Debug.Log("triggered: " + new Vector2Int((int)input.x, (int)input.y) + "\n new selected: " + selectedCell);
                }
            };

            _playerInput.inputActions.Player.Space.performed += (ctx) =>
            {
                Debug.Log("Fire!");
                temperatureData[selectedCell.x + selectedCell.y * totalGridWidth] = new TemperatureCell
                {
                    temperature = setTemperature,
                    insulation = setInsulation,
                    heatSource = setSourceHeat
                };
            };

            //testCPUGrid = new TemperatureGrid(new Grid<TemperatureCell>(sparseGridWidth, sparseGridHeight, 1));

            renderTexture = new RenderTexture(localGridWidth * 8, localGridHeight * 8, 1);
            renderTexture.enableRandomWrite = true;
            renderTexture.format = RenderTextureFormat.ARGBFloat;
            renderTexture.Create();

            PopulateTemperatureData();
            window = new TemperatureCell[localGridWidth * localGridHeight];
            window[0].heatSource = 20f;
            GetGridSubset(temperatureData, ref window, 0, 0, localGridWidth, localGridHeight);

            prevBuffer = new ComputeBuffer(localGridWidth * localGridHeight, TemperatureCell.GetSize());
            nextBuffer = new ComputeBuffer(localGridWidth * localGridHeight, TemperatureCell.GetSize());


            tempCompute.SetBuffer(0, "prev", prevBuffer);
            tempCompute.SetBuffer(1, "prev", prevBuffer);
            tempCompute.SetBuffer(0, "next", nextBuffer);
            tempCompute.SetTexture(1, "temperature", renderTexture);
            tempCompute.SetInt("width", localGridWidth);
            tempCompute.SetInt("height", localGridHeight);
            
            if (testCPUGrid != null) testCPUGrid.grid.Log();


            Debug.Log("first " + window[0]);
        }

        // Update is called once per frame
        void Update()
        {
            if (doCompute)
            {
                GetGridSubset(temperatureData, ref window, 0, 0, localGridWidth, localGridHeight);
                prevBuffer.SetData(window);
                tempCompute.SetFloat("deltaTime", Time.deltaTime * timeScale);
                tempCompute.SetInt("selectedX", selectedCell.x);
                tempCompute.SetInt("selectedY", selectedCell.y);
                tempCompute.Dispatch(0, (localGridWidth * localGridHeight) / 64, 1, 1);
                tempCompute.Dispatch(1, localGridWidth, localGridHeight, 1);
                nextBuffer.GetData(window);
                SetGridSubset(temperatureData, ref window, 0, 0, localGridWidth, localGridHeight);
                _curTempText.text = $"{temperatureData[selectedCell.x + selectedCell.y * totalGridWidth].temperature}°";
            }
        }

        private void OnApplicationQuit()
        {
            prevBuffer.Dispose();
            nextBuffer.Dispose();
        }
    }
}
