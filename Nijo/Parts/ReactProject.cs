using Nijo.Parts.WebClient;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nijo.Parts {
    /// <summary>
    /// 自動生成されるReact.jsのSPAに対する操作を提供します。
    /// </summary>
    public class ReactProject {

        /// <summary>
        /// 画面のソースが格納されるディレクトリの名前
        /// </summary>
        public const string PAGES = "pages";
        /// <summary>
        /// 入力コンポーネントが格納されるディレクトリの名前
        /// </summary>
        public const string INPUT = "input";

        public ReactProject(GeneratedProject generatedProject) {
            _generatedProject = generatedProject;
        }

        private readonly GeneratedProject _generatedProject;

        public string ProjectRoot => Path.Combine(_generatedProject.SolutionRoot, "react");
        public string AutoGeneratedDir => Path.Combine(ProjectRoot, "src", "__autoGenerated");

        /// <summary>
        /// プロジェクトディレクトリを新規作成します。
        /// </summary>
        public void CreateProjectIfNotExists(Core.Config config) {
            if (Directory.Exists(ProjectRoot)) return;

            // 埋め込みリソースからテンプレートを出力
            var resources = new EmbeddedResource.Collection(Assembly.GetExecutingAssembly());
            foreach (var resource in resources.Enumerate("react")) {
                var destination = Path.Combine(
                    ProjectRoot,
                    Path.GetRelativePath("react", resource.RelativePath));

                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

                using var reader = resource.GetStreamReader();
                using var writer = SourceFile.GetStreamWriter(destination);
                while (!reader.EndOfStream) {
                    writer.WriteLine(reader.ReadLine());
                }
            }

            // デバッグページを削除する
            var appTsx = Path.Combine(ProjectRoot, "src", "App.tsx");
            File.WriteAllText(appTsx, $$"""
                import { DefaultNijoApp } from './__autoGenerated'

                function App() {

                  return (
                    <DefaultNijoApp />
                  )
                }

                export default App
                """, new UTF8Encoding(false));
        }

        /// <summary>
        /// 依存先パッケージをインストールします。
        /// </summary>
        public async Task NpmInstall(CancellationToken cancellationToken) {
            var npmCi = new Process();
            try {
                npmCi.StartInfo.WorkingDirectory = ProjectRoot;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    npmCi.StartInfo.FileName = "powershell";
                    npmCi.StartInfo.Arguments = "/c \"npm ci\"";
                } else {
                    npmCi.StartInfo.FileName = "npm";
                    npmCi.StartInfo.Arguments = "ci";
                }
                npmCi.Start();
                await npmCi.WaitForExitAsync(cancellationToken);

            } finally {
                npmCi.EnsureKill();
            }
        }

        /// <summary>
        /// デバッグ時に起動されるサーバーのURLを返します。
        /// </summary>
        public Uri GetDebuggingClientUrl() {
            // vite.config.ts からポートを参照してURLを生成して返す
            var viteConfigTs = Path.Combine(ProjectRoot, "vite.config.ts");
            if (!File.Exists(viteConfigTs))
                throw new FileNotFoundException(viteConfigTs);

            using var stream = new StreamReader(viteConfigTs, Encoding.UTF8);
            var regex = new Regex(@"port:\s*([^,]*)");
            while (!stream.EndOfStream) {
                var line = stream.ReadLine();
                if (line == null) continue;
                var match = regex.Match(line);
                if (!match.Success) continue;
                var port = match.Groups[1].Value;
                return new Uri($"http://localhost:{port}");
            }

            throw new InvalidOperationException("vite.config.ts からポート番号を読み取れません。'port: 9999'のようにポートを設定している行があるか確認してください。");
        }

        /// <summary>
        /// コード自動生成処理のソースをわかりやすくするためのクラス
        /// </summary>
        public class DirectoryEditor {
            internal DirectoryEditor(CodeRenderingContext context, ReactProject project) {
                _context = context;
                _project = project;
                _autoGeneratedDir = DirectorySetupper.StartSetup(_context, _project.AutoGeneratedDir);
            }
            private readonly CodeRenderingContext _context;
            private readonly ReactProject _project;
            private readonly DirectorySetupper _autoGeneratedDir;

            /// <summary>
            /// 自動生成ディレクトリ直下へのソース生成を行います。
            /// </summary>
            public void AutoGeneratedDir(Action<DirectorySetupper> setup) {
                setup(_autoGeneratedDir);
            }
            /// <summary>
            /// ユーティリティに関するクラスを格納するディレクトリへのソース生成を行います。
            /// </summary>
            public void UtilDir(Action<DirectorySetupper> setup) {
                _autoGeneratedDir.Directory("util", setup);
            }
            /// <summary>
            /// 画面のソースが格納されるディレクトリへのソース生成を行います。
            /// </summary>
            public void PagesDir(Action<DirectorySetupper> setup) {
                _autoGeneratedDir.Directory(PAGES, setup);
            }

            /// <summary>
            /// DI設定
            /// </summary>
            public List<Func<string, string>> ConfigureServices { get; } = new List<Func<string, string>>();
            /// <summary>
            /// Webサーバーが起動するタイミングで実行されるアプリケーション設定処理
            /// </summary>
            public List<Func<string, string>> ConfigureWebApp { get; } = new List<Func<string, string>>();


            /// <summary>
            /// autogenerated-types.ts ファイル
            /// </summary>
            internal TypesTsx Types => _context.UseSummarizedFile<TypesTsx>();
            /// <summary>
            /// UtlUtil.ts ファイル
            /// </summary>
            internal UrlUtil UrlUtil => _context.UseSummarizedFile<UrlUtil>();
            internal List<IReactPage> Pages { get; } = new List<IReactPage>();
            internal List<string> DashBoardImports { get; } = new List<string>();
            internal List<string> DashBoardContents { get; } = new List<string>();
            internal Dictionary<string, string> AutoGeneratedHook { get; } = new Dictionary<string, string>();
            internal List<string> AutoGeneratedInput { get; } = new List<string>();
        }
    }
}
