using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        core();
        view();
    }
    static void core()
    {
        // 1. Define your paths
        string sourceDirectory = @"C:\Users\Razzouk\source\repos\ApiAutoTest\ApiAutoTest\ApiAutoTest\deepseek";
        string outputFilePath = @"C:\Users\Razzouk\source\repos\ApiAutoTest\ApiAutoTest\ConsoleApp1\core_combined_files.txt";

        try
        {
            // 2. Ensure the source exists
            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine("Source directory does not exist.");
                return;
            }

            // 3. Initialize the stream writer
            // Using 'using' ensures the file is closed and saved correctly
            using (StreamWriter writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                // 4. Recursively find all files
                // SearchOption.AllDirectories handles the subfolders automatically
                var allFiles = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);

                foreach (string filePath in allFiles)
                {
                    // Skip the output file if it's located within the source directory
                    if (filePath == outputFilePath) continue;

                    try
                    {
                        // Write the Header (Path and Filename)
                        writer.WriteLine("==============================================================================");
                        writer.WriteLine($"FILE: {filePath}");
                        writer.WriteLine("==============================================================================");
                        writer.WriteLine();

                        // Read and write the content
                        string content = File.ReadAllText(filePath);
                        writer.WriteLine(content);

                        // Add some spacing between files
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                    catch (IOException ex)
                    {
                        writer.WriteLine($"[ERROR: Could not read file {filePath}: {ex.Message}]");
                    }
                }
            }

            Console.WriteLine($"Successfully combined all files into: {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

    }
    static void view()
    {
        // 1. Define your paths
        string sourceDirectory = @"C:\Users\Razzouk\source\repos\ApiAutoTest\ApiAutoTest\TestAutomation";
        string outputFilePath = @"C:\Users\Razzouk\source\repos\ApiAutoTest\ApiAutoTest\ConsoleApp1\view_combined_files.txt";

        // Define the folder names you want to exclude
        HashSet<string> excludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        ".git",
        ".vs"
    };

        try
        {
            // 2. Ensure the source exists
            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine("Source directory does not exist.");
                return;
            }

            // 3. Initialize the stream writer
            using (StreamWriter writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                // 4. Recursively find all files
                var allFiles = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);

                // 5. Filter out files that belong to any excluded folder
                var filteredFiles = allFiles.Where(filePath =>
                {
                    // Split the path into individual directory segments
                    string[] pathSegments = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    // If any segment of the path matches our exclusion list, skip the file
                    return !pathSegments.Any(segment => excludedFolders.Contains(segment));
                });

                foreach (string filePath in filteredFiles)
                {
                    // Skip the output file if it's located within the source directory
                    if (filePath == outputFilePath) continue;

                    try
                    {
                        // Write the Header (Path and Filename)
                        writer.WriteLine("==============================================================================");
                        writer.WriteLine($"FILE: {filePath}");
                        writer.WriteLine("==============================================================================");
                        writer.WriteLine();

                        // Read and write the content
                        string content = File.ReadAllText(filePath);
                        writer.WriteLine(content);

                        // Add some spacing between files
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                    catch (IOException ex)
                    {
                        writer.WriteLine($"[ERROR: Could not read file {filePath}: {ex.Message}]");
                    }
                }
            }

            Console.WriteLine($"Successfully combined all files into: {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}