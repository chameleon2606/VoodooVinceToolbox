using System.Globalization;
using System.Numerics;
using Microsoft.VisualBasic.FileIO;
using ImGuiNET;

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
    
    private enum Levels
    {
        EarthWaterWood = 5,
        GatekeeperJam = 7,
        LandORides = 6,
        BumperCar = 1,
        InsideKosmobot = 12,
    }

    public static void GetTeleports(int level)
    {
        switch (level)
                {
                    case (int)Levels.InsideKosmobot:
                        if (ImGui.Button("teleport to outro cutscene"))
                        {
                            Program.M.WriteMemory(Program.VinceXPointer, "float", "-315");
                            Program.M.WriteMemory(Program.VinceYPointer, "float", "460");
                            Program.M.WriteMemory(Program.VinceZPointer, "float", "3");
                    
                            Program.M.WriteMemory(Program.CamXPointer, "float", "-315");
                            Program.M.WriteMemory(Program.CamYPointer, "float", "460");
                            Program.M.WriteMemory(Program.CamZPointer, "float", "3");
                            ImGui.TreePop();
                        }
                        break;
                    case (int)Levels.GatekeeperJam:
                        if (ImGui.Button("teleport to skip practice"))
                        {
                            Program.M.WriteMemory(Program.VinceXPointer, "float", "12.5");
                            Program.M.WriteMemory(Program.VinceYPointer, "float", "1.2");
                            Program.M.WriteMemory(Program.VinceZPointer, "float", "-10.4");
                    
                            Program.M.WriteMemory(Program.CamXPointer, "float", "11.7");
                            Program.M.WriteMemory(Program.CamYPointer, "float", "1.9");
                            Program.M.WriteMemory(Program.CamZPointer, "float", "-10.8");
                            ImGui.TreePop();
                        }
                        break;
                    case (int)Levels.EarthWaterWood:
                        if (ImGui.Button("teleport to lever platform"))
                        {
                            Program.M.WriteMemory(Program.VinceXPointer, "float", "7.4");
                            Program.M.WriteMemory(Program.VinceYPointer, "float", "3.8");
                            Program.M.WriteMemory(Program.VinceZPointer, "float", "25.5");
                    
                            Program.M.WriteMemory(Program.CamXPointer, "float", "7.1");
                            Program.M.WriteMemory(Program.CamYPointer, "float", "4.5");
                            Program.M.WriteMemory(Program.CamZPointer, "float", "24.7");
                            ImGui.TreePop();
                        }
                        break;
                    case (int)Levels.LandORides:
                        if (ImGui.Button("teleport to tent"))
                        {
                            Program.M.WriteMemory(Program.VinceXPointer, "float", "18.1");
                            Program.M.WriteMemory(Program.VinceYPointer, "float", "-0.7");
                            Program.M.WriteMemory(Program.VinceZPointer, "float", "0");
                    
                            Program.M.WriteMemory(Program.CamXPointer, "float", "18.7");
                            Program.M.WriteMemory(Program.CamYPointer, "float", "0");
                            Program.M.WriteMemory(Program.CamZPointer, "float", "-0.6");
                            ImGui.TreePop();
                        }
                        break;
                    case (int)Levels.BumperCar:
                        if (ImGui.Button("teleport to plane"))
                        {
                            Program.M.WriteMemory(Program.VinceXPointer, "float", "-12");
                            Program.M.WriteMemory(Program.VinceYPointer, "float", "9.5");
                            Program.M.WriteMemory(Program.VinceZPointer, "float", "-56");
                    
                            Program.M.WriteMemory(Program.CamXPointer, "float", "-12.2");
                            Program.M.WriteMemory(Program.CamYPointer, "float", "10.2");
                            Program.M.WriteMemory(Program.CamZPointer, "float", "-56.9");
                            ImGui.TreePop();
                        }
                        break;
                }
    }
}