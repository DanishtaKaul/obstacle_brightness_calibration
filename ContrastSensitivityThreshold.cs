// Sets the obstacle brightness so it is equally visible for every participant under each lighting condition.
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ContrastSensitivityThreshold : MonoBehaviour
{
    // Enum representing different states of the experiment
    enum ExperimentState
    {
        DimToBright = 1,
        BrightToDim,
        AverageShade,
        ExperimentEnd
    }

    public Image blueObstacle;  // UI element representing the obstacle
    public int totalColors = 32;  // Total number of shades

    private Color[] blueShades;  // Array to hold generated blue shades
    private int currentShadeIndex = 0;  // Index of the current shade being displayed
    private ExperimentState currentTrial = ExperimentState.DimToBright;  // Current trial state

    private List<Color> selectedShades = new List<Color>();  // List of shades selected by the participant
    private Color selectedAverage;  // Average color selected by the participant
    private string filePath;  // Path to the file where results will be stored
    public int participantID = 1;  // Identifier for the participant, to be incremented for each participant

    // Initializes the experiment by generating blue shades and setting the initial shade.
    void Start()
    {
        // Set the file path for logging results, including the participant ID
        filePath = $"contrast_sensitivity_results_participant_{participantID}.csv";

        // Generate an array of blue shades from dimmest to brightest
        blueShades = GenerateBlueShades();

        // Update the obstacle's color based on the current state
        UpdateShade();

        // Write headers to the CSV file
        writeTotStream("ParticipantID,Trial,Selected Shade,Hex Value, Line Number");
    }

    // Handles input for selecting shades and navigating through them
    void Update() //changed
    {
        if (currentTrial == ExperimentState.ExperimentEnd)
        {
            return; // Stop processing any further input if the experiment has ended
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Record the currently selected shade
            RecordSelectedShade();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Move to the next shade
            ChangeShade(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Move to the previous shade
            ChangeShade(-1);
        }
    }

    // Generates an array of blue shades from dimmest to brightest
    Color[] GenerateBlueShades()
    {
        Color[] shades = new Color[totalColors];
        for (int i = 0; i < totalColors; i++)
        {
            float shadeValue = (float)i / (totalColors - 1);
            shades[i] = new Color(0, 0, shadeValue, 1);
        }
        return shades;
    }

    // Changes the current shade by a specified direction and updates the shade
    void ChangeShade(int direction)
    {
        currentShadeIndex += direction;

        // Wrap the shade index around if it goes out of bounds
        if (currentShadeIndex >= totalColors)
        {
            currentShadeIndex = 0;
            ChangeTrial(1);  // Move to the next trial
        }
        else if (currentShadeIndex < 0)
        {
            currentShadeIndex = totalColors - 1;
            ChangeTrial(-1);  // Move to the previous trial
        }

        // Log the current shade and trial
        Debug.Log($"Shade: {currentShadeIndex}, Trial: {currentTrial}");

        // Update the obstacle's color based on the current state
        UpdateShade();
    }

    // Updates the obstacle color based on the current trial state
    void UpdateShade()
    {
        switch (currentTrial)
        {
            case ExperimentState.DimToBright:
                // Set obstacle color from dimmest to brightest
                blueObstacle.color = blueShades[currentShadeIndex];
                break;

            case ExperimentState.BrightToDim:
                // Set obstacle color from brightest to dimmest
                blueObstacle.color = blueShades[totalColors - 1 - currentShadeIndex];
                break;

            case ExperimentState.AverageShade:
                // Set obstacle color based on the current shade
                blueObstacle.color = blueShades[currentShadeIndex];

                break;

            case ExperimentState.ExperimentEnd:
                // Handle the end of the experiment
                break;

            default:
                // Log an error if the trial state is unknown
                Debug.LogError("Unknown trial state: " + currentTrial);
                break;
        }
    }

    // Records the currently selected shade and updates the trial state
    void RecordSelectedShade()
    {
        // Log the selected shade
        Debug.Log($"Selected shade: {currentShadeIndex} Colour: #{ConvertColorToString(blueObstacle.color)}");

        // Add the selected shade to the list
        selectedShades.Add(blueObstacle.color);

        // Write the selected shade data to the CSV file
        writeTotStream($"{participantID},{currentTrial},{currentShadeIndex},'#{ConvertColorToString(blueObstacle.color)},#148");

        // If in the AverageShade trial, move to the next trial
        if (currentTrial == ExperimentState.AverageShade)
        {
            ChangeTrial(1);
        }
    }

    // Changes the trial state and resets the shade index
    void ChangeTrial(int direction) //changed
    {
        currentTrial += direction;

        if (currentTrial > ExperimentState.ExperimentEnd)
        {
            currentTrial = ExperimentState.ExperimentEnd;
        }
        else if (currentTrial < ExperimentState.DimToBright)
        {
            currentTrial = ExperimentState.DimToBright;
        }

        switch (currentTrial)
        {
            case ExperimentState.DimToBright:
                // Transitioning to DimToBright state
                break;

            case ExperimentState.BrightToDim:
                // Transitioning to BrightToDim state
                break;

            case ExperimentState.AverageShade:
                // Calculate the average color from selected shades
                selectedAverage = CalculateAverageColor(selectedShades);

                // Log the average shade to the CSV file
                writeTotStream($"{participantID},Calculated Average Shade,-,'#{ConvertColorToString(selectedAverage)},#186");

                // Log the average color to the console immediately
                Debug.Log($"Calculated Average Colour: #{ConvertColorToString(selectedAverage)}");

                // Generate a range of shades from the average color to black
                blueShades = GenerateAverageToBlackColours(selectedAverage.b, totalColors);

                // Set obstacle color based on the current shade
                blueObstacle.color = blueShades[currentShadeIndex];
                break;

            case ExperimentState.ExperimentEnd:
                // Experiment completion logic
                Debug.Log("Experiment completed!");

                // Calculate the final color based on the average and selected colors
                Color finalColor = CalculateFinalColor(selectedAverage, blueShades[currentShadeIndex]);

                Debug.Log($"Average Colour: #{ConvertColorToString(selectedAverage)}");
                Debug.Log($"Final Colour: #{ConvertColorToString(finalColor)}");

                // Set obstacle to the final color
                blueObstacle.color = finalColor;

                // Log the final results to the CSV file
                writeTotStream($"{participantID},Result: Average Shade,-,'#{ConvertColorToString(selectedAverage)},#214");
                writeTotStream($"{participantID},Result: Slope Shade,-,'#{ConvertColorToString(blueShades[currentShadeIndex])},#215");
                writeTotStream($"{participantID},Result: Final Shade,-,'#{ConvertColorToString(finalColor)},#216");

                break;
        }

        Debug.Log($"Trial changed to: {currentTrial}");
        currentShadeIndex = 0;  // Reset the shade index for the new trial
    }

    // Calculates the average color from a list of colors
    Color CalculateAverageColor(List<Color> colorList)
    {
        float totalRed = 0, totalGreen = 0, totalBlue = 0;

        foreach (var color in colorList)
        {
            totalRed += color.r;
            totalGreen += color.g;
            totalBlue += color.b;
        }

        int colorCount = colorList.Count;
        return new Color(totalRed / colorCount, totalGreen / colorCount, totalBlue / colorCount, 1);
    }

    // Generates a range of colors from the average blue value to black
    Color[] GenerateAverageToBlackColours(float averageBlue, int totalColors)
    {
        Color[] shades = new Color[totalColors];

        for (int i = 0; i < totalColors; i++)
        {
            float proportion = (float)i / (totalColors - 1);
            float shadeValue = averageBlue * (1 - proportion);
            shadeValue = Mathf.Clamp(shadeValue, 0, 1);
            shades[i] = new Color(0, 0, shadeValue, 1);
        }

        return shades;
    }

    // Calculates the final color by adding the difference between average and selected colors
    Color CalculateFinalColor(Color averageColor, Color selectedColor)
    {
        float difference = Mathf.Abs(averageColor.b - selectedColor.b);
        float finalBlue = Mathf.Clamp(averageColor.b + difference, 0, 1);
        return new Color(0, 0, finalBlue, 1);
    }

    // Converts a Color object to its hexadecimal string representation
    string ConvertColorToString(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    // Writes text to the CSV file
    void writeTotStream(string text)
    {
        using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8))
        {
            sw.WriteLine(text);
        }
    }
}
