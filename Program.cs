using System.Drawing;
using System.Globalization;
using System.Numerics;
using ImGuiNET;
using ClickableTransparentOverlay;
using Memory;
using System.Runtime.InteropServices;


namespace AltToolbox
{
    public class Program : Overlay
    {
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

        private static bool _savePosButton;
        private static bool _writePosButton;
        private static bool _giveLivesButton;
        private static bool _lockHp;
        private static bool _closeButton;
        private static bool _getLevelButton;

        public static int Paused => M.ReadMemory<int>(PausePointer);
        public static int Hearts => M.ReadMemory<int>(HeartsPointer);
        public static int Hp => M.ReadMemory<int>(HpPointer);
        private static int prevHp;
        private static int prevHearts;
        private static int prevPause;

        public static bool buffering;

        public static string currentLevel;
        
        private static float _jumpHeight;
        private static bool _grounded;

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
            ImGui.Begin("Vince Toolbox");
            //ImGui.DockSpaceOverViewport();
            //ImGui.ShowDemoWindow();
            if (prevHearts < Hearts) currentLevel = Utitily.GetCurrentLevel();
            ImGui.Text(currentLevel);
            ImGui.Text(TimeSpan.FromSeconds(M.ReadFloat(TimerPointer)).ToString(@"h\:mm\:ss\:ff"));
            if (ImGui.CollapsingHeader("Coordiantes"))
            {
                if (ImGui.TreeNode("Vince Position"))
                {
                    ImGui.Text("X: "+M.ReadFloat(VinceXPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.Text("Y: "+M.ReadFloat(VinceYPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.Text("Z: "+M.ReadFloat(VinceZPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Cam Position"))
                {
                    ImGui.Text("X: "+M.ReadFloat(CamXPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.Text("Y: "+M.ReadFloat(CamYPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.Text("Z: "+M.ReadFloat(CamZPointer, "", false).ToString(CultureInfo.InvariantCulture));
                    ImGui.TreePop();
                    
                    if (ImGui.TreeNode("Move New"))
                    {
                        ImGui.Button("manual\ncamera\ndrag", new Vector2(100, 100));
                        if (ImGui.IsItemActive())
                        {
                            var valueRaw = ImGui.GetMouseDragDelta(0, 0.0f);
                            
                            M.WriteMemory(CamXPointer, "float", (M.ReadFloat(CamXPointer) +
                                                                 (-valueRaw.X * 0.001f)).ToString(CultureInfo.InvariantCulture));
                            
                            M.WriteMemory(CamZPointer, "float", (M.ReadFloat(CamZPointer) + 
                                                                 (valueRaw.Y * 0.001f)).ToString(CultureInfo.InvariantCulture));
                        }
                        float test2 = 0;
                        ImGui.PushItemWidth(100);
                        if (ImGui.DragFloat("", ref test2, 0.005f, -.1f,.1f, "height drag"))
                        {
                            M.WriteMemory(CamYPointer, "float", (M.ReadFloat(CamYPointer) + test2).ToString(CultureInfo.InvariantCulture));
                        }
                        ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.65f);
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
                ImGui.Unindent();
            }

            if (ImGui.CollapsingHeader("Speedrun tools"))
            {
                ImGui.PlotLines("",ref Utitily.GetSpeedList()[0], Utitily.GetSpeedList().Length, 
                    2, "velocity", 0, 1.35f, new Vector2(ImGui.GetWindowWidth(), 50));
                
                if (Utitily.GetHighestJumpValue() > .4f) _jumpHeight = Utitily.GetHighestJumpValue();
                ImGui.ProgressBar(_jumpHeight / 1.28f, new Vector2(0,15), "jump");
                ImGui.SameLine();
                ImGui.Checkbox("grounded", ref _grounded);
                _grounded = M.ReadMemory<int>(GroundedPointer) == 1;
                
                ImGui.Checkbox("Alt-Tab mode", ref _enableAlttab);
                
                if (ImGui.Button("save position", new Vector2(110, 50)))
                {
                    Utitily.GetPosition();
                }
                ImGui.SameLine();
                if (ImGui.Button("teleport to\nsaved position", new Vector2(110, 50)))
                {
                    Utitily.SetPosition();
                }
            }
            
            if (ImGui.CollapsingHeader("More info"))
            {
                ImGui.Text("Hearts: "+M.ReadMemory<int>(HeartsPointer));
                
                var hp = M.ReadMemory<int>(HpPointer);
                var maxHp = M.ReadMemory<int>(MaxHpPointer);
                ImGui.Text("HP");
                ImGui.ProgressBar((float)hp / maxHp, new Vector2(0, 15), hp+" / "+maxHp);
                
                ImGui.Spacing();
                
                var beads = M.ReadMemory<int>(BeadsPointer);
                var maxBeads = M.ReadMemory<int>(MaxBeadsPointer);
                ImGui.Text("Beads");
                ImGui.ProgressBar((float)beads / maxBeads, new Vector2(0, 15), beads+" / "+maxBeads);
                
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
                
                ImGui.Text("Game resolution: " + M.ReadMemory<int>(ScreenXPointer)+ " x "+M.ReadMemory<int>(ScreenYPointer));
                ImGui.TreePop();
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
            
            if (_enableAlttab && Paused != 0 && currentLevel != "Voodoo Shop")
            {
                Console.WriteLine(Utitily.GetCurrentLevel());
                    
                M.WriteMemory(PausePointer, "int", "0");
                Console.WriteLine("pause disabled");
            }

            if (_lockHp && Hp < prevHp)
            {
                M.WriteMemory(HpPointer, "int", prevHp.ToString());
                M.WriteMemory(HpUiPointer, "int", prevHp.ToString());
                Console.WriteLine("hp reset to " + prevHp);
            }
            
            if (buffering && prevPause != Paused)
            {
                buffering = false;
                Utitily.SetPosition();
            }
            
            prevHp = Hp;
            prevHearts = Hearts;
            prevPause = Paused;
            
            ImGui.End();
        }

        private static void ClosedGame()
        {
            ImGui.Begin("Voodoo Vince Toolbox");
            ImGui.TextColored(new Vector4(1,0,0,1), "Game not found!");
            if (ImGui.Button("Refresh"))
            {
                FindProcess();
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