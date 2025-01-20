using System.CommandLine;
using System.IO;
using System.Runtime.CompilerServices;
#region MyRegion
var outputOption = new Option<FileInfo>(new[] { "--output", "-o"}, "File path and name for the bundled file");
var languageOption = new Option<string>(new[] { "--language", "-l" }, "Choose languages (e.g., '.cs .py')");
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Include file source notes in the output");
var sortOption = new Option<string>(new[] { "--sort", "-s" }, "Sort files ('a-b' or 'languageSort')");
var removeEmptyLinesOption = new Option<bool>(new[] { "--remove_empty_lines", "-r" }, "Remove empty lines from source files");
var authorOption = new Option<string>(new[] { "--author", "-a" }, "Add author information");

sortOption.SetDefaultValue("alphabetic");

var bundleCommand = new Command("bundle", "Bundle code files into a single file")
{
    outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption
};

bundleCommand.SetHandler((FileInfo output, string language, bool note, string sort, bool removeEmptyLines, string author) =>
{
    try
    {
        if (output == null || string.IsNullOrWhiteSpace(output.FullName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Output file path is required.");
            Console.ResetColor();
            return;
        }

        if (!Directory.Exists(output.DirectoryName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Directory '{output.DirectoryName}' does not exist.");
            Console.ResetColor();
            return;
        }

        var files = Directory.GetFiles(output.DirectoryName);
        if (!string.IsNullOrWhiteSpace(language) && language != "all")
        {
            var extensions = language.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            files = files.Where(file => extensions.Contains(Path.GetExtension(file))).ToArray();
        }

        if (sort == "alphabetic")
            files = files.OrderBy(f => f).ToArray();
        else if (sort == "language")
            files = files.OrderBy(f => Path.GetExtension(f)).ToArray();

        using var newFile = new StreamWriter(output.FullName);
        foreach (var file in files)
        {
            if (note)
                newFile.WriteLine($"# Source code: {file}");
            if (!string.IsNullOrWhiteSpace(author))
                newFile.WriteLine($"# Author: {author}");

            var lines = File.ReadAllLines(file);
            if (removeEmptyLines)
                lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            foreach (var line in lines)
                newFile.WriteLine(line);
        }

        Console.WriteLine($"File '{output.FullName}' created successfully.");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

var createRspCommand = new Command("create-rsp", "Create a RSP file");
createRspCommand.SetHandler(async () =>
{
    try
    {
        var rsp = new FileInfo("rsp.rsp");
        Console.WriteLine("enter details for bundle file");
        Console.WriteLine("enter path");
        string output, languages, note, sort, remove_empty_lines, author;
        output = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(output))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: the path is invalid");
            Console.ResetColor();
            output = Console.ReadLine();
        }
        Console.WriteLine("enter choosen languages | all");
        languages = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(languages))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: the languages is invalid");
            Console.ResetColor();
            languages = Console.ReadLine();
        }
        Console.WriteLine("add the source of the file as commet (y/n)");
        note = Console.ReadLine();
        Console.WriteLine("sort the file by (alphabetic/language/none)");
        sort = Console.ReadLine();
        Console.WriteLine("remove empty lines (y/n)");
        remove_empty_lines = Console.ReadLine();
        Console.WriteLine("add the author of the file as commet");
        author = Console.ReadLine();
        using (StreamWriter stream = new StreamWriter(output))
        {
            stream.Write($"fib bundle --output {output} --language {languages}");
            if (note.Trim().ToLower() == "y")
                stream.Write(" --note");
            stream.Write($" --sort {sort}");
            if (remove_empty_lines.Trim().ToLower() == "y")
                stream.Write($" --remove_empty_lines");
            if (author.Trim().ToLower() == "y")
                stream.Write($" --author {author}");
        }
        Console.WriteLine("RSP file 'rsp.rsp' created successfully.");
    }

    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
});

var rootCommand = new RootCommand("Root command for File Bundler CLI")
{
    bundleCommand,
    createRspCommand
};

await rootCommand.InvokeAsync(args);

#endregion
