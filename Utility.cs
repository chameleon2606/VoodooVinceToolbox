using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace AltToolbox;

public class Utitily
{
    private static float _localPosX;
    private static float _localPosY;
    private static float _localPosZ;
    
    private static float _localCamX;
    private static float _localCamY;
    private static float _localCamZ;

    
    public static float[] speedArray = new float[550];
    private static int speedIndex = 0;
    private static float lastTimerState = 0;

    private static int prevGround;
    private static float initialHeight;
    private static float highestYValue;
    private static bool recordingHeight;
    private static float finalValue;
    
    public static void GetPosition()
    {
        _localPosX = Program.M.ReadFloat(Program.VinceXPointer, "", false);
        _localPosY = Program.M.ReadFloat(Program.VinceYPointer, "", false);
        _localPosZ = Program.M.ReadFloat(Program.VinceZPointer, "", false);
        
        _localCamX = Program.M.ReadFloat(Program.CamXPointer, "", false);
        _localCamY = Program.M.ReadFloat(Program.CamYPointer, "", false);
        _localCamZ = Program.M.ReadFloat(Program.CamZPointer, "", false);
    }
    public static void SetPosition()
    {
        
        if (Program.Paused > 0)
        {
            Program.buffering = true;
            return;
        }
        Program.M.WriteMemory(Program.VinceXPointer, "float", _localPosX.ToString(CultureInfo.InvariantCulture));
        Program.M.WriteMemory(Program.VinceYPointer, "float", _localPosY.ToString(CultureInfo.InvariantCulture));
        Program.M.WriteMemory(Program.VinceZPointer, "float", _localPosZ.ToString(CultureInfo.InvariantCulture));
        
        Program.M.WriteMemory(Program.CamXPointer, "float", _localCamX.ToString(CultureInfo.InvariantCulture));
        Program.M.WriteMemory(Program.CamYPointer, "float", _localCamY.ToString(CultureInfo.InvariantCulture));
        Program.M.WriteMemory(Program.CamZPointer, "float", _localCamZ.ToString(CultureInfo.InvariantCulture));
    }


    public static void GiveHp(int amount)
    {
        Program.M.WriteMemory(Program.HeartsPointer, "int", amount.ToString());
        Program.M.WriteMemory(Program.HeartsUiPointer, "int", Program.Hearts.ToString());
    }
    public static string GetCurrentLevel()
    {
        using var parser = new TextFieldParser(@"levels.csv");
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields == null || !fields.Any(unused =>
                    fields[1] == Program.M.ReadMemory<int>(Program.LevelIndex1Pointer).ToString() &&
                    fields[2] == Program.M.ReadMemory<int>(Program.LevelIndex2Pointer).ToString() &&
                    fields[3] == Program.M.ReadMemory<int>(Program.LevelIndex3Pointer).ToString())) continue;
            Console.WriteLine("get level");
            return fields[0];
        }

        return "Level not found!";
    }

    public static float[] GetSpeedList()
    {
        //if (MathF.Round(Program.M.ReadFloat(Program.TimerPointer), 1) != MathF.Round(Program.lastTimerState, 1))
        //{
          for (int i = 0; i < speedArray.Length - 1; i++)
          {
              speedArray[i] = speedArray[i + 1];
          }
          speedArray[^1] = Program.M.ReadFloat(Program.MovementSpeedPointer, "", false);
        //}
        
        return speedArray;
    }

    
    public static float GetHighestJumpValue()
    {
        var vinceY = Program.M.ReadFloat(Program.VinceYPointer, "", false);
        var grounded = Program.M.ReadMemory<int>(Program.GroundedPointer);
        
        if (prevGround != grounded && prevGround == 1)
        {
            initialHeight = vinceY;
            recordingHeight = true;
            highestYValue = 0;
        }

        if (recordingHeight)
        {
            if (vinceY-initialHeight > highestYValue)
            {
                highestYValue = vinceY - initialHeight;
            }
            if (grounded == 1)
            {
                recordingHeight = false;
            }
        }

        prevGround = grounded;
        return highestYValue;
    }
    
}