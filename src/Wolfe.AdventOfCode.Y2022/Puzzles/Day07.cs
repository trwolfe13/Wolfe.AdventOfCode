﻿namespace Wolfe.AdventOfCode.Y2022.Puzzles;

internal class Day07 : IPuzzleDay
{
    public int Day => 7;

    public Task<string> Part1(string input, CancellationToken cancellationToken = default)
    {
        var root = Parse(input);
        return FlattenDirectories(root)
            .Where(d => d.Size <= 100000)
            .Sum(d => d.Size)
            .ToString()
            .ToTask();
    }

    public Task<string> Part2(string input, CancellationToken cancellationToken = default) =>
        "".ToTask();

    private static DirectoryObject Parse(string input)
    {
        var lines = input.ToLines().GetSneakyEnumerator();
        var root = new DirectoryObject("/", null);

        var currentDirectory = root;

        while (lines.MoveNext() && lines.Current != null)
        {
            var (_, command, arg) = lines.Current.Split(' ');
            switch (command)
            {
                case "cd":
                    currentDirectory = arg switch
                    {
                        "/" => root,
                        ".." => currentDirectory.Parent ?? throw new InvalidOperationException("Attempted move to parent beyond root"),
                        _ => currentDirectory.Entries.OfType<DirectoryObject>().First(d => d.Name == arg)
                    };
                    break;
                case "ls":
                    while (!lines.Peek()?.StartsWith("$") ?? false)
                    {
                        lines.MoveNext();
                        var (attribute, name) = lines.Current.Split(" ");
                        if (attribute == "dir")
                        {
                            var directory = new DirectoryObject(name!, currentDirectory);
                            currentDirectory.Entries.Add(directory);
                        }
                        else
                        {
                            var file = new FileObject(name!, int.Parse(attribute!), currentDirectory);
                            currentDirectory.Entries.Add(file);
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected command {lines.Current}");
            }
        }

        return root;
    }

    private IEnumerable<DirectoryObject> FlattenDirectories(DirectoryObject root)
    {

        var dirs = root.Entries.OfType<DirectoryObject>();
        foreach (var dir in dirs)
        {
            yield return dir;
            foreach (var subDir in FlattenDirectories(dir))
            {
                yield return subDir;
            }
        }
    }

    interface IFileSystemObject
    {
        DirectoryObject? Parent { get; }
        string Name { get; }
        long Size { get; }
    }

    abstract class FileSystemObject : IFileSystemObject
    {
        private readonly string _type;

        protected FileSystemObject(string name, string type, DirectoryObject? parent)
        {
            _type = type;
            Parent = parent;
            Name = name;
        }

        public DirectoryObject? Parent { get; }
        public string Name { get; }
        public abstract long Size { get; }
        public override string ToString() => string.Format($"{Name} ({_type}, size={Size})");
    }

    class FileObject : FileSystemObject
    {
        public FileObject(string name, int size, DirectoryObject parent): base(name, "file", parent)
        {
            Size = size;
        }
        public override long Size { get; }
    }

    class DirectoryObject : FileSystemObject
    {
        public DirectoryObject(string name, DirectoryObject? parent): base(name, "dir", parent) { }
        public override long Size => Entries.Sum(f => f.Size);
        public List<IFileSystemObject> Entries = new();
    }
}
