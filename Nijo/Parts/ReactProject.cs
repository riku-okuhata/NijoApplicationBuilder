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
using System.Web;

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
            if (Directory.Exists(ProjectRoot)) {
                // プロジェクトcloneの直後にこのディレクトリがなくてエラーになることがあるので明示的に作成
                if (!Directory.Exists(AutoGeneratedDir)) {
                    Directory.CreateDirectory(AutoGeneratedDir);
                }
                return;
            }

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
                import { DefaultNijoApp, AutoGeneratedCustomizer } from './__autoGenerated'

                function App() {
                  return (
                    <DefaultNijoApp customizer={customizer} />
                  )
                }

                // 自動生成されたソースコードに任意の処理やUIを追加編集する場合はここで設定してください。
                const customizer: AutoGeneratedCustomizer = {
                  applicationName: '{{config.RootNamespace.Replace("'", "")}}',
                  {{DefaultUi.CUSTOM_UI_COMPONENT}}: {},
                }

                export default App
                """, new UTF8Encoding(false));

            // index.html 内のtitleを書き換える
            var indexHtml = Path.Combine(ProjectRoot, "index.html");
            var beforeReplaceIndexHtml = File.ReadAllText(indexHtml, new UTF8Encoding(false, false));
            var afterReplaceIndexHtml = beforeReplaceIndexHtml.Replace("このタイトルはプロジェクト新規作成時に上書きされます", HttpUtility.HtmlEncode(config.RootNamespace));
            File.WriteAllText(indexHtml, afterReplaceIndexHtml, new UTF8Encoding(false, false));
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
            internal List<string> DashBoardImports { get; } = new List<string>();
            internal List<string> DashBoardContents { get; } = new List<string>();
            internal AutoGeneratedHooks AutoGeneratedHook => _context.UseSummarizedFile<AutoGeneratedHooks>();
            internal AutoGeneratedComponents AutoGeneratedComponents => _context.UseSummarizedFile<AutoGeneratedComponents>();

            /// <summary>
            /// React画面を追加します。
            /// </summary>
            /// <param name="url">React router に登録するURL</param>
            /// <param name="dirNameInPageDir">自動生成後のソースファイルが作成される場所</param>
            /// <param name="componentPhysicalName">Reactコンポーネント物理名</param>
            /// <param name="showMenu">サイドメニューに表示するか否か</param>
            /// <param name="labelInMenu">サイドメニュー表示名</param>
            /// <param name="sourceFile">ソースファイル</param>
            internal void AddReactPage(
                string url,
                string dirNameInPageDir,
                string componentPhysicalName,
                bool showMenu,
                string? labelInMenu,
                SourceFile sourceFile) {

                // 画面ソースコードの生成
                PagesDir(dir => {
                    dir.Directory(dirNameInPageDir, aggregatePageDir => {
                        aggregatePageDir.Generate(sourceFile);
                    });
                });

                // React router のルーティングと、画面左側のサイドメニューへの登録
                _context.UseSummarizedFile<MenuTsx>().AddMenuItem(new() {
                    ImportAs = componentPhysicalName,
                    ImportFrom = $"./{PAGES}/{dirNameInPageDir}/{Path.GetFileNameWithoutExtension(sourceFile.FileName)}",
                    Url = url,
                    ShowSideMenu = showMenu,
                    Label = labelInMenu,
                });
            }
        }
    }
}
