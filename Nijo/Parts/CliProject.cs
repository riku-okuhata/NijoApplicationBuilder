using Nijo.Core;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts {
    /// <summary>
    /// 自動生成されるコンソールアプリケーションに対する操作を提供します。
    /// </summary>
    public class CliProject {

        public CliProject(GeneratedProject generatedProject) {
            _generatedProject = generatedProject;
        }

        private readonly GeneratedProject _generatedProject;

        public string ProjectRoot => Path.Combine(_generatedProject.SolutionRoot, "cli");
        public string AutoGeneratedDir => Path.Combine(ProjectRoot, "__AutoGenerated");

        /// <summary>
        /// プロジェクトディレクトリを新規作成します。
        /// </summary>
        public void CreateProjectIfNotExists(Core.Config config) {
            if (Directory.Exists(ProjectRoot)) return;

            // 埋め込みリソースからテンプレートを出力
            var resources = new EmbeddedResource.Collection(Assembly.GetExecutingAssembly());
            foreach (var resource in resources.Enumerate("cli")) {
                var destination = Path.Combine(
                    ProjectRoot,
                    Path.GetRelativePath("cli", resource.RelativePath));

                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

                using var reader = resource.GetStreamReader();
                using var writer = SourceFile.GetStreamWriter(destination);
                while (!reader.EndOfStream) {
                    writer.WriteLine(reader.ReadLine());
                }
            }

            // ファイル名変更
            var beforeCsproj = Path.Combine(ProjectRoot, "NIJO_APPLICATION_TEMPLATE_Cli.csproj");
            var afterCsproj = Path.Combine(ProjectRoot, $"{config.ApplicationName}_Cli.csproj");
            File.Move(beforeCsproj, afterCsproj);

            // ソースコード中にあるテンプレートプロジェクトの文字列を置換
            var beforeReplace = File.ReadAllText(afterCsproj);
            var afterReplace = beforeReplace.Replace("NIJO_APPLICATION_TEMPLATE", config.RootNamespace);
            File.WriteAllText(afterCsproj, afterReplace);

            var programCs = Path.Combine(ProjectRoot, "Program.cs");
            var beforeReplace2 = File.ReadAllText(programCs);
            var afterReplace2 = beforeReplace2.Replace("NIJO_APPLICATION_TEMPLATE_Cli", config.RootNamespace);
            File.WriteAllText(programCs, afterReplace2);
        }

        /// <summary>
        /// コード自動生成処理のソースをわかりやすくするためのクラス
        /// </summary>
        public class DirectoryEditor {
            internal DirectoryEditor(CodeRenderingContext context, CliProject project) {
                _context = context;
                _project = project;
                _autoGeneratedDir = DirectorySetupper.StartSetup(_context, _project.AutoGeneratedDir);
            }
            private readonly CodeRenderingContext _context;
            private readonly CliProject _project;
            private readonly DirectorySetupper _autoGeneratedDir;

            /// <summary>
            /// 自動生成ディレクトリ直下へのソース生成を行います。
            /// </summary>
            public void AutoGeneratedDir(Action<DirectorySetupper> setup) {
                setup(_autoGeneratedDir);
            }

            public List<Func<string, string>> ConfigureServices { get; } = new List<Func<string, string>>();
        }
    }
}
