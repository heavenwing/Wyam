﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;

namespace Wyam.Modules.CodeAnalysis
{
    /// <summary>
    /// Reads an MSBuild solution or project file and returns all referenced source files as documents.
    /// This module will be executed once and input documents will be ignored if a search path is 
    /// specified. Otherwise, if a delegate is specified the module will be executed once per input 
    /// document and the resulting output documents will be aggregated.
    /// Note that this requires the MSBuild tools to be installed (included with Visual Studio).
    /// See https://github.com/dotnet/roslyn/issues/212 and https://roslyn.codeplex.com/workitem/218.
    /// </summary>
    public abstract class ReadWorkspace : IModule, IAsNewDocuments
    {
        private readonly FilePath _path;
        private readonly DocumentConfig _pathDelegate;
        private Func<string, bool> _whereProject;
        private Func<IFile, bool> _whereFile;
        private string[] _extensions;

        protected ReadWorkspace(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            _path = path;
        }

        protected ReadWorkspace(DocumentConfig path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            _pathDelegate = path;
        }
        
        /// <summary>
        /// Filters the project based on name.
        /// </summary>
        /// <param name="predicate">A predicate that should return <c>true</c> if the project should be included.</param>
        public ReadWorkspace WhereProject(Func<string, bool> predicate)
        {
            Func<string, bool> currentPredicate = _whereProject;
            _whereProject = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        /// <summary>
        /// Filters the source code file based on path.
        /// </summary>
        /// <param name="predicate">A predicate that should return <c>true</c> if the source code file should be included.</param>
        public ReadWorkspace WhereFile(Func<IFile, bool> predicate)
        {
            Func<IFile, bool> currentPredicate = _whereFile;
            _whereFile = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }
        
        /// <summary>
        /// Filters the source code files based on extension.
        /// </summary>
        /// <param name="extensions">The extensions to include (if defined, any extensions not listed will be excluded).</param>
        public ReadWorkspace WithExtensions(params string[] extensions)
        {
            _extensions = _extensions?.Concat(extensions.Select(x => x.StartsWith(".") ? x : "." + x)).ToArray()
                ?? extensions.Select(x => x.StartsWith(".") ? x : "." + x).ToArray();
            return this;
        }

        protected abstract IEnumerable<Project> GetProjects(IFile file);

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return _path != null
                ? Execute(null, _path, context)
                : inputs.AsParallel().SelectMany(input =>
                    Execute(input, _pathDelegate.Invoke<FilePath>(input, context), context));
        }

        private IEnumerable<IDocument> Execute(IDocument input, FilePath projectPath, IExecutionContext context)
        {
            if (projectPath != null)
            {
                IFile projectFile = context.FileSystem.GetInputFile(projectPath);
                return GetProjects(projectFile)
                    .AsParallel()
                    .Where(project => project != null && (_whereProject == null || _whereProject(project.Name)))
                    .SelectMany(project =>
                    {
                        Trace.Verbose("Read project {0}", project.Name);
                        return project.Documents
                            .AsParallel()
                            .Where(x => !string.IsNullOrWhiteSpace(x.FilePath))
                            .Select(x => context.FileSystem.GetInputFile(x.FilePath))
                            .Where(x => x.Exists && (_whereFile == null || _whereFile(x)) && (_extensions == null || _extensions.Contains(x.Path.Extension)))
                            .Select(file => {
                                Trace.Verbose($"Read file {file.Path.FullPath}");
                                DirectoryPath inputPath = context.FileSystem.GetContainingInputPath(file.Path);
                                FilePath relativePath = inputPath?.GetRelativePath(file.Path) ?? projectPath.Directory.GetRelativePath(file.Path);
                                return context.GetDocument(file.Path, file.OpenRead(), new MetadataItems
                                {
                                    { Keys.SourceFileRoot, inputPath ?? file.Path.Directory },
                                    { Keys.SourceFileBase, file.Path.FileNameWithoutExtension },
                                    { Keys.SourceFileExt, file.Path.Extension },
                                    { Keys.SourceFileName, file.Path.FileName },
                                    { Keys.SourceFileDir, file.Path.Directory },
                                    { Keys.SourceFilePath, file.Path },
                                    { Keys.SourceFilePathBase, file.Path.Directory.CombineFile(file.Path.FileNameWithoutExtension) },
                                    { Keys.RelativeFilePath, relativePath },
                                    { Keys.RelativeFilePathBase, relativePath.Directory.CombineFile(file.Path.FileNameWithoutExtension) },
                                    { Keys.RelativeFileDir, relativePath.Directory }
                                });
                            });
                    });
            }
            return Array.Empty<IDocument>();
        }
    }
}
