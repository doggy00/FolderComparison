namespace FolderComparison
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Folder Comparison";
            Console.CursorVisible = false;

            while (true)
            {
                Console.Clear();
                ShowHeader();

                try
                {
                    var (folder1, folder2) = GetFoldersPaths();
                    var comparisonResult = CompareFolders(folder1, folder2);
                    DisplayResults(comparisonResult, folder1, folder2);
                    ShowFooter();

                    if (!AskForRepeat()) break;
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                    Thread.Sleep(2000);
                }
            }
        }

        static void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║         FOLDER COMPARISON TOOL         ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static (string, string) GetFoldersPaths()
        {
            Console.WriteLine("\n▼ Input paths for comparison\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("First folder: ");
            string path1 = ReadPath();

            Console.Write("Second folder: ");
            string path2 = ReadPath();

            return (path1, path2);
        }

        static string ReadPath()
        {
            Console.ForegroundColor = ConsoleColor.White;
            string path;
            do
            {
                path = Console.ReadLine()?.Trim() ?? "";
                if (Directory.Exists(path)) break;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Path not found: ");
            } while (true);

            return path;
        }

        static (string[] files1, string[] files2) CompareFolders(string path1, string path2)
        {
            Console.Write("\nSearching files...");

            var spinner = new Spinner(10, 10);
            spinner.Start();

            var files1 = Directory.GetFiles(path1, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(path1, f))
                .ToArray();

            var files2 = Directory.GetFiles(path2, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(path2, f))
                .ToArray();

            spinner.Stop();
            Console.WriteLine(" Done!");

            return (files1, files2);
        }

        static void DisplayResults((string[] f1, string[] f2) files, string root1, string root2)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;

            var unique1 = files.f1.Except(files.f2, comparer).ToArray();
            var unique2 = files.f2.Except(files.f1, comparer).ToArray();

            Console.WriteLine("\nComparison results:");
            Console.WriteLine("┌──────────────────────┬───────────┐");
            Console.WriteLine($"│ {"Parameter",-20} │ {"Count",-9} │");
            Console.WriteLine("├──────────────────────┼───────────┤");
            PrintStat("Unique in first", unique1.Length);
            PrintStat("Unique in second", unique2.Length);
            Console.WriteLine("└──────────────────────┴───────────┘");

            void PrintStat(string name, int count)
            {
                Console.WriteLine($"│ {name,-20} │ {count,9} │");
            }

            DisplayFileList("Unique files in first folder:", unique1, root1);
            DisplayFileList("Unique files in second folder:", unique2, root2);
        }

        static void DisplayFileList(string title, string[] relPaths, string root)
        {
            if (relPaths.Length == 0) return;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n{title}");

            foreach (var relPath in relPaths)
            {
                var fullPath = Path.Combine(root, relPath);
                var dir = Path.GetDirectoryName(relPath);

                Console.Write($" {root}\\");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(dir);
                Console.ResetColor();
                Console.Write("\\");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Path.GetFileName(fullPath));
                Console.ResetColor();
            }
        }

        static void ShowFooter()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\n" + new string('═', Console.WindowWidth));
            Console.WriteLine(" Press any key to continue...");
            Console.ReadKey();
        }

        static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nERROR: {message}");
            Console.ResetColor();
        }

        static bool AskForRepeat()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n Repeat comparison? (Y/N): ");
            return Console.ReadKey().Key == ConsoleKey.Y;
        }
    }

    class Spinner : IDisposable
    {
        private readonly int _delay;
        private readonly Thread _thread;
        private bool _active;

        public Spinner(int left, int top)
        {
            _delay = 100;
            _thread = new Thread(() =>
            {
                var counter = 0;
                while (_active)
                {
                    Console.SetCursorPosition(left, top);
                    Console.Write("|/-\\"[counter++ % 4]);
                    Thread.Sleep(_delay);
                }
            });
        }

        public void Start()
        {
            _active = true;
            _thread.Start();
        }

        public void Stop()
        {
            _active = false;
            _thread.Join();
            Console.SetCursorPosition(10, 10);
            Console.Write(" ");
        }

        public void Dispose() => Stop();
    }
}