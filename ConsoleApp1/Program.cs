using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = @"C:\Users\Razzouk\source\repos\ApiAutoTest\ApiAutoTest\ApiAutoTest\deepseek"; // استبدل هذا بالمسار الخاص بك

        if (Directory.Exists(directoryPath))
        {
            string[] files = Directory.GetFiles(directoryPath);

            using (StreamWriter writer = new StreamWriter("MergedOutput.cs"))
            {
                foreach (string file in files)
                {
                    writer.WriteLine($"// File: {Path.GetFileName(file)}");
                    string content = File.ReadAllText(file);
                    writer.WriteLine(content);
                    writer.WriteLine(); // إضافة سطر فارغ بين الملفات
                }
            }

            Console.WriteLine("sucess ApiAutoTest.cs");
        }
        else
        {
            Console.WriteLine("not found");
        }
    }
}