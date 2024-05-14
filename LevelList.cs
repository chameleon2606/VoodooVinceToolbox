namespace AltToolbox;

public class LevelList
{
    private static int reReadAttempt = 0;
    private static readonly string[,] levels = new string[,] {
        { "Below Decks","20","0","18" },
        { "Bumper Car Bump Off","8","0","6" },
        { "Crawdad Jimmy's","20","9","18" },
        { "Dolly Playtime","19","0","18" },
        { "Downtown Crypt City","36","25","34" },
        { "Earth, Water & Wood","33","0","31" },
        { "Finger's Land O' Rides","34","0","32" },
        { "Gatekeeper Jam","13","11","11" },
        { "Glowberry Tangle","88","0","86" },
        { "House of Awfully Scary Stuff","54","0","52" },
        { "House of Mirrors","52","0","50" },
        { "Hurricane Hannah","12","0","10" },
        { "Inside the Kosmobot","21","15","18" },
        { "Jean Lafitte's Ship","34","29","32" },
        { "Jimmy's Fanboat Race","24","0","0" },
        { "empty","0","0","0" },
        { "Main Street","75","2","73" },
        { "Main Tunnel","28","0","27" },
        { "Propeller Room","80","0","79" },
        { "Rat Race Rodeo","34","12","32" },
        { "Sarcophagus Hustle","35","31","33" },
        { "The Back Stoop","23","2","21" },
        { "The Basket Case","42","2","40" },
        { "The Bog Wallow","18","15","16" },
        { "The Bone Goliath","60","1","58" },
        { "The Busy Butler","30","0","29" },
        { "The Central Arena","63","0","61" },
        { "The Docks","19","0","17" },
        { "The Midway","28","0","26" },
        { "The Porch Pooch","14","0","12" },
        { "The Rocket Lab","24","0","22" },
        { "The Sausage Works","13","10","11" },
        { "The Square","42","0","40" },
        { "The Trophy Room","24","0","20" },
        { "The Tumbler Room","25","0","24" },
        { "The Upper City","50","0","48" },
        { "Throttle Up","52","0","51" },
        { "Unwanted Guests","44","4","43" },
        { "Voodoo Shop","24","7","22" },
        { "Zombie Guidance Counselor","31","27","28" }
    };
    public static void GetLevel()
    {
        for (int i = 0; i < levels.GetLength(0); i++)
        {
            if (levels[i, 1] == Program.M.ReadMemory<int>(Program.LevelIndex1Pointer).ToString() &&
                levels[i, 2] == Program.M.ReadMemory<int>(Program.LevelIndex2Pointer).ToString() &&
                levels[i, 3] == Program.M.ReadMemory<int>(Program.LevelIndex3Pointer).ToString())
            {
                Program.CurrentLevel = levels[i, 0];
                Console.WriteLine(levels[i,0]);
                reReadAttempt = 0;
                return;
            }
        }

        reReadAttempt++;
        Console.Clear();
        Console.WriteLine($"level not found {Program.M.ReadMemory<int>(Program.LevelIndex1Pointer)} {Program.M.ReadMemory<int>(Program.LevelIndex2Pointer)} {Program.M.ReadMemory<int>(Program.LevelIndex3Pointer)}");
        Console.WriteLine($"attempt {reReadAttempt}");
        GetLevel();
    }
}