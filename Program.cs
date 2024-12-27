using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using ImGuiNET;
using ClickableTransparentOverlay;
using Memory;
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;

//dotnet publish -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "C:\Users\leong\Desktop\build"


namespace AltToolbox
{
    public class Program : Overlay
    {
        private static InputSimulator key = new();
        
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        
        private static bool _enableAlttab;
        private static bool _isGameRunning;
        
        public static Mem M;
        private const string VinceAddress = "base+0076320C";
        private const string CamAddress = "base+0075A744";
        private const string PauseAddress = "base+007653F4";
        private const string HeartsUiAddress = "base+00763278";
        private const string HpUiAddress = "base+007655E4";
        private const string LevelIndex1Address = "base+0093DF0";
        private const string LevelIndex2Address = "base+00913C0";
        private const string LevelIndex3Address = "base+00913B0";
        private const string CollectablesAddress = "base+00763494";
        private const string ScreenAddress = "base+00062B28";
        
        public const string HeartsPointer = VinceAddress + ",48C";
        public const string HeartsUiPointer = HeartsUiAddress + ",35C,4,8,0,4,28,DC";
        public const string HpPointer = VinceAddress + ",260";
        public const string MaxHpPointer = VinceAddress + ",264";
        public const string HpUiPointer = HpUiAddress + ",EC,20,24,0,8,DC";
        public const string BeadsPointer = VinceAddress + ",3D4";
        public const string MaxBeadsPointer = VinceAddress + ",3D8";
        public const string DustBagsGlobalPointer = VinceAddress + ",3D0";
        public const string DustBagsLevelPointer = CollectablesAddress + ",4,34";
        public const string DustBagsRemainPointer = CollectablesAddress + ",4,30";
        public const string TimerPointer = VinceAddress + ",1C0";
        public const string VinceXPointer = VinceAddress + ",D8";
        public const string VinceYPointer = VinceAddress + ",DC";
        public const string VinceZPointer = VinceAddress + ",E0";
        public const string SkullPagesPointer = CollectablesAddress + ",4,40";
        public const string SkullPagesRemainPointer = CollectablesAddress + ",4,44";
        public const string MovementSpeedPointer = VinceAddress + ",1FC";
        public const string ScreenXPointer = ScreenAddress + ",4";
        public const string ScreenYPointer = ScreenAddress + ",8";
        public const string GroundedPointer = VinceAddress + ",244";

        public const string CamXPointer = CamAddress + ",14,54";
        public const string CamYPointer = CamAddress + ",14,58";
        public const string CamZPointer = CamAddress + ",14,5C";

        public const string PausePointer = PauseAddress + ",8,3C";

        public const string LevelIndex1Pointer = LevelIndex1Address + ",4";
        public const string LevelIndex2Pointer = LevelIndex2Address + ",4";
        public const string LevelIndex3Pointer = LevelIndex3Address + ",4";
        
        private static bool _lockHp;

        public static int Paused => M.ReadMemory<int>(PausePointer);
        public static int Hearts => M.ReadMemory<int>(HeartsPointer);
        private static int Hp => M.ReadMemory<int>(HpPointer);
        private static int _prevHp;
        private static int _prevPause;

        public static bool Buffering;

        public static string CurrentLevel = "";
        public static int LevelNumber = 0;
        
        private static float _jumpHeight;
        private static bool _grounded;
        private static bool _isHeightLocked;
        private static float _prevX = 0;
        private static float _prevY = 0;
        private static float _prevZ = 0;
        private static float _prevTime = 0;
        
        private static bool _lockX;
        private static float _savedXPos = 0;
        private static bool _isXCaptured;
        
        private static bool _lockY;
        private static float _savedYPos = 0;
        private static bool _isYCaptured;
        
        private static bool _lockZ;
        private static float _savedZPos = 0;
        private static bool _isZCaptured;
        public static string _saveText = "save position";
        public static string _teleportText = "teleport to\nsaved position";
        private static bool _isTpInputAllowed = true;
        private static bool _isSaveInputAllowed = true;

        protected override void Render()
        {
            ImGui.SetNextWindowSize(new Vector2(400, 900), ImGuiCond.Once);
            if (_isGameRunning)
            {
                DrawMenu();
            }
            else
            {
                ClosedGame();
            }
        }

        private static void DrawMenu()
        {
            if (key.InputDeviceState.IsKeyUp(VirtualKeyCode.ADD))
            {
                _isSaveInputAllowed = true;
            }
            else if(key.InputDeviceState.IsKeyDown(VirtualKeyCode.ADD) && _isSaveInputAllowed)
            {
                if (CurrentLevel is not ("Voodoo Shop" or "The Basket Case"))
                {
                    _isSaveInputAllowed = false;
                    Utitily.SavePosition();
                }
            }
            if(key.InputDeviceState.IsKeyUp(VirtualKeyCode.SUBTRACT))
            {
                _isTpInputAllowed = true;
            }
            else if(key.InputDeviceState.IsKeyDown(VirtualKeyCode.SUBTRACT) && _isTpInputAllowed)
            {
                // if you're not in the voodoo shop
                if (CurrentLevel is not ("Voodoo Shop" or "The Basket Case"))
                {
                    _isTpInputAllowed = false;
                    Utitily.TeleportToSavedPosition();
                }
            }
            
            ImGui.Begin("Vince Toolbox");
            // Load the name of the current level, if the ingame time is reset
            if (M.ReadFloat(TimerPointer) != 0 && _prevTime == 0) LevelList.GetLevel();
            _prevTime = M.ReadFloat(TimerPointer);
            
            // Displays the current level with the ingame time
            ImGui.Text(CurrentLevel);
            ImGui.Text(TimeSpan.FromSeconds(M.ReadFloat(TimerPointer)).ToString(@"h\:mm\:ss\:ff"));
            
            if (ImGui.CollapsingHeader("Coordinates"))
            {
                if (ImGui.TreeNode("Vince Position"))
                {
                    ImGui.Checkbox("X:", ref _lockX);
                    if (_lockX)
                    {
                        if (!_isXCaptured)
                        {
                            _savedXPos = M.ReadFloat(VinceXPointer);
                            _isXCaptured = true;
                        }
                        else
                        {
                            M.WriteMemory(VinceXPointer, "float", _savedXPos.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    else
                    {
                        _isXCaptured = false;
                    }
                    ImGui.SameLine();
                    ImGui.Text(M.ReadFloat(VinceXPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    
                    ImGui.Checkbox("Y:", ref _lockY);
                    if (_lockY)
                    {
                        if (!_isYCaptured)
                        {
                            _savedYPos = M.ReadFloat(VinceYPointer);
                            _isYCaptured = true;
                        }
                        else
                        {
                            M.WriteMemory(VinceYPointer, "float", _savedYPos.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    else
                    {
                        _isYCaptured = false;
                    }
                    ImGui.SameLine();
                    ImGui.Text(M.ReadFloat(VinceYPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    
                    ImGui.Checkbox("Z:", ref _lockZ);
                    if (_lockZ)
                    {
                        if (!_isZCaptured)
                        {
                            _savedZPos = M.ReadFloat(VinceZPointer);
                            _isZCaptured = true;
                        }
                        else
                        {
                            M.WriteMemory(VinceZPointer, "float", _savedZPos.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    else
                    {
                        _isZCaptured = false;
                    }
                    ImGui.SameLine();
                    ImGui.Text(M.ReadFloat(VinceZPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Camera Position"))
                {
                    ImGui.Text("X: "+M.ReadFloat(CamXPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.Text("Y: "+M.ReadFloat(CamYPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.Text("Z: "+M.ReadFloat(CamZPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.TreePop();
                    
                    if (ImGui.TreeNode("Move Camera"))
                    {
                        ImGui.Button("manual\ncamera\ndrag", new Vector2(100, 100));
                        if (Hearts > 0)
                        {
                            if (ImGui.IsItemActive() && Hearts > 0)
                            {
                                var valueRaw = ImGui.GetMouseDragDelta(0, 0.0f);
                                
                                M.WriteMemory(CamXPointer, "float", (M.ReadFloat(CamXPointer) +
                                                                     (-valueRaw.X * 0.001f)).ToString(CultureInfo.InvariantCulture));
                                
                                M.WriteMemory(CamZPointer, "float", (M.ReadFloat(CamZPointer) + 
                                                                     (valueRaw.Y * 0.001f)).ToString(CultureInfo.InvariantCulture));
                            }
                            float test2 = 0;
                            ImGui.PushItemWidth(100);
                            ImGui.SameLine();
                            if (ImGui.VSliderFloat("##CamHeight", new Vector2(20, 100), ref test2, -.1f, .1f, "Y"))
                            {
                                M.WriteMemory(CamYPointer, "float", (M.ReadFloat(CamYPointer) + test2).ToString(CultureInfo.InvariantCulture));
                            }
                            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.65f);
                        }
                        ImGui.TreePop();
                    }
                }
            }

            if (ImGui.CollapsingHeader("Cheats"))
            {
                ImGui.Indent();
                if (ImGui.Button("99 lives"))
                {
                    Utitily.GiveHp(99);
                }
                ImGui.Checkbox("Lock HP", ref _lockHp);
                float newHeight = 0;
                if (ImGui.VSliderFloat("Move up Vince", new Vector2(30, 100), ref newHeight, -.1f, .1f, ""))
                {
                    if (!_isHeightLocked)
                    {
                        _prevY = M.ReadFloat(VinceYPointer);
                    }

                    _prevY += newHeight;
                    _isHeightLocked = true;
                    _prevX = M.ReadFloat(VinceXPointer);
                    _prevZ = M.ReadFloat(VinceZPointer);
                }
                
                if (_isHeightLocked)
                {
                    if (Paused == 0)
                    {
                        M.WriteMemory(VinceYPointer, "float", _prevY.ToString(CultureInfo.InvariantCulture));
                        if (Vector2.Distance(new Vector2(_prevX, _prevZ),
                                new Vector2(M.ReadFloat(VinceXPointer), M.ReadFloat(VinceZPointer))) > 1)
                        {
                            _isHeightLocked = false;
                        }
                    }
                }
                
                ImGui.Unindent();
            }

            if (ImGui.CollapsingHeader("Speedrun tools"))
            {
                ImGui.PlotLines("velocity",ref Utitily.GetSpeedList()[0], Utitily.GetSpeedList().Length, 
                    2, "velocity", 0, 1.35f, new Vector2(ImGui.GetWindowWidth() * .99f, 50));
                
                if (Utitily.GetHighestJumpValue() > .4f) _jumpHeight = Utitily.GetHighestJumpValue();
                ImGui.ProgressBar(_jumpHeight / 1.28f, new Vector2(0,15), "jump");
                ImGui.SameLine();
                ImGui.Checkbox("grounded", ref _grounded);
                _grounded = M.ReadMemory<int>(GroundedPointer) == 1;
                
                ImGui.Checkbox("Alt-Tab mode", ref _enableAlttab);
                
                if (_enableAlttab && Paused != 0 && LevelNumber != 38)
                {
                    M.WriteMemory(PausePointer, "int", "0");
                }
                
                if (ImGui.Button(_saveText, new Vector2(110, 50)))
                {
                    Utitily.SavePosition();
                }
                ImGui.SameLine();
                if (ImGui.Button(_teleportText, new Vector2(110, 50)))
                {
                    Utitily.TeleportToSavedPosition();
                }
            }
            
            if (ImGui.CollapsingHeader("More info"))
            {
                ImGui.Text("Hearts: "+M.ReadMemory<int>(HeartsPointer));
                
                ImGui.Spacing();
                var maxHp = M.ReadMemory<int>(MaxHpPointer);
                ImGui.Text("Health");
                ImGui.ProgressBar((float)Hp / maxHp, new Vector2(0, 15), Hp+" / "+maxHp);
                
                ImGui.Spacing();
                
                var beads = M.ReadMemory<int>(BeadsPointer);
                var maxBeads = M.ReadMemory<int>(MaxBeadsPointer);
                ImGui.Text("Beads");
                var frac = beads % 42;
                ImGui.ProgressBar((float)frac / 42, new Vector2(0,15), frac + " / " + 42);
                for (int i = 0; i < maxBeads/42; i++)
                {
                    var test = i < beads / 42;
                    ImGui.RadioButton("", test);
                    ImGui.SameLine();
                    if (i == maxBeads / 42 - 1)
                    {
                        ImGui.Text("Power skulls");
                    }
                }
                
                ImGui.Spacing();
                
                var dustbags = M.ReadMemory<int>(DustBagsLevelPointer);
                var dustbagsRemain = M.ReadMemory<int>(DustBagsRemainPointer);
                ImGui.Text("Dust bags");
                ImGui.ProgressBar(dustbags / (float)(dustbags + dustbagsRemain), new Vector2(0,15), 
                    dustbags + " / " + (dustbags + dustbagsRemain));
                ImGui.Text("Dust bags: " + M.ReadMemory<int>(DustBagsGlobalPointer) + "/100");
                
                ImGui.Spacing();
                
                var skullPages = M.ReadMemory<int>(SkullPagesPointer);
                var skullRemain = M.ReadMemory<int>(SkullPagesRemainPointer);
                ImGui.Text("Skull Pages");
                ImGui.ProgressBar(skullPages / (float)(skullPages + skullRemain), new Vector2(0,15), skullPages + " / " +
                    (skullPages + skullRemain));

                ImGui.Spacing();
                
                ImGui.Text("Game resolution: " + M.ReadMemory<int>(ScreenXPointer)+ " x "+M.ReadMemory<int>(ScreenYPointer));
            }

            
            if (ImGui.CollapsingHeader("Teleports"))
            {
                Utitily.GetTeleports(LevelNumber);
            }
            
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 100, 20));
            if (ImGui.Button("refresh"))
            {
                FindProcess();
            }
            ImGui.SameLine();
            if (ImGui.Button("X"))
            {
                Environment.Exit(0);
            }
            
            if (Buffering && _prevPause != Paused)
            {
                Buffering = false;
                Utitily.TeleportToSavedPosition();
            }
            if (_lockHp && Hp < _prevHp)
            {
                M.WriteMemory(HpPointer, "int", _prevHp.ToString());
                M.WriteMemory(HpUiPointer, "int", _prevHp.ToString());
            }
            
            _prevHp = Hp;
            _prevPause = Paused;
            
            ImGui.End();
        }

        private static void ClosedGame()
        {
            ImGui.Begin("Voodoo Vince Toolbox");
            ImGui.TextColored(new Vector4(1,0,0,1), "Game not found!");
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 100, 20));
            if (ImGui.Button("refresh"))
            {
                FindProcess();
            }
            ImGui.SameLine();
            if (ImGui.Button("X"))
            {
                Environment.Exit(0);
            }
            ImGui.End();
        }

        public static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            
            M = new Mem();
            FindProcess();
            
            Program program = new Program();
            program.Start();
        }

        private static void FindProcess()
        {
            _isGameRunning = M.OpenProcess("Vince");
        }
    }
}