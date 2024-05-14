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
    private static bool heightCaptured;
    
    public static void GetPosition()
    {
        _localPosX = Program.M.ReadFloat(Program.VinceXPointer);
        _localPosY = Program.M.ReadFloat(Program.VinceYPointer) + 0.01f;
        _localPosZ = Program.M.ReadFloat(Program.VinceZPointer);
        
        _localCamX = Program.M.ReadFloat(Program.CamXPointer);
        _localCamY = Program.M.ReadFloat(Program.CamYPointer);
        _localCamZ = Program.M.ReadFloat(Program.CamZPointer);
    }
    public static void SetPosition()
    {
        if (Program.Paused > 0)
        {
            Program.Buffering = true;
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
        var vinceY = Program.M.ReadFloat(Program.VinceYPointer,"",false);
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

    public static void LockFloat(float value)
    {
        
    }

    
    
}