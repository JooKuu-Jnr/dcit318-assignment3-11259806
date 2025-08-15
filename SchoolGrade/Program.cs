using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SchoolGradingApp
{
    public class Student
    {
        public int Id;
        public string FullName;
        public int Score;

        public Student(int id, string fullName, int score)
        {
            Id = id;
            FullName = fullName;
            Score = score;
        }

        public string GetGrade()
        {
            if (Score >= 80 && Score <= 100) return "A";
            if (Score >= 70 && Score <= 79) return "B";
            if (Score >= 60 && Score <= 69) return "C";
            if (Score >= 50 && Score <= 59) return "D";
            return "F";
        }
    }

    public class InvalidScoreFormatException : Exception
    {
        public InvalidScoreFormatException(string message) : base(message) { }
    }

    public class MissingFieldException : Exception
    {
        public MissingFieldException(string message) : base(message) { }
    }

    public class StudentResultProcessor
    {
        public List<Student> ReadStudentsFromFile(string inputFilePath)
        {
            var students = new List<Student>();
            using var reader = new StreamReader(inputFilePath, detectEncodingFromByteOrderMarks: true);
            string? line;
            int lineNumber = 0;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                char delim = DetectDelimiter(line);
                var parts = SplitCsvLine(line, delim);
                if (parts.Count < 3)
                    throw new MissingFieldException($"Line {lineNumber}: expected 3 fields");

                string first = parts[0].Trim().Trim('"');
                string last = parts[^1].Trim().Trim('"');

                bool looksLikeHeader = first.Equals("id", StringComparison.OrdinalIgnoreCase)
                                       && last.Equals("score", StringComparison.OrdinalIgnoreCase);
                if (looksLikeHeader) continue;

                if (!int.TryParse(first, out int id))
                    throw new Exception($"Line {lineNumber}: Id is not an integer");

                if (!int.TryParse(last, out int score))
                    throw new InvalidScoreFormatException($"Line {lineNumber}: score is not an integer");

                string fullName = JoinMiddle(parts).Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                    throw new MissingFieldException($"Line {lineNumber}: FullName is missing");

                students.Add(new Student(id, fullName, score));
            }

            return students;
        }

        public void WriteReportToFile(List<Student> students, string outputFilePath)
        {
            using var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8);
            foreach (var s in students)
                writer.WriteLine($"{s.FullName} (ID: {s.Id}): Score = {s.Score}, Grade = {s.GetGrade()}");
        }

        private static char DetectDelimiter(string line)
        {
            if (line.Contains(",")) return ',';
            if (line.Contains(";")) return ';';
            if (line.Contains("\t")) return '\t';
            return ',';
        }

        private static List<string> SplitCsvLine(string line, char delimiter)
        {
            var parts = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                    continue;
                }

                if (c == delimiter && !inQuotes)
                {
                    parts.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            parts.Add(sb.ToString());
            for (int i = 0; i < parts.Count; i++) parts[i] = parts[i].Trim();
            return parts;
        }

        private static string JoinMiddle(List<string> parts)
        {
            if (parts.Count == 3) return parts[1].Trim().Trim('"');
            var sb = new StringBuilder();
            for (int i = 1; i < parts.Count - 1; i++)
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(parts[i].Trim().Trim('"'));
            }
            return sb.ToString();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string inputPath = args.Length > 0 ? args[0] : "students.txt";
                string inputFullPath = Path.GetFullPath(inputPath);
                string inputDir = Path.GetDirectoryName(inputFullPath) ?? Directory.GetCurrentDirectory();
                string outputPath = Path.Combine(inputDir, "report.txt");

                var processor = new StudentResultProcessor();
                var students = processor.ReadStudentsFromFile(inputFullPath);
                Console.WriteLine($"Loaded {students.Count} students from: {inputFullPath}");
                processor.WriteReportToFile(students, outputPath);
                Console.WriteLine($"Report written to: {outputPath}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File error: {ex.Message}");
            }
            catch (InvalidScoreFormatException ex)
            {
                Console.WriteLine($"Invalid score: {ex.Message}");
            }
            catch (MissingFieldException ex)
            {
                Console.WriteLine($"Missing field: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}
