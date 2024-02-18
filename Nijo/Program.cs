using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nijo.Core;
using Nijo.Util.DotnetEx;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Build.Evaluation;
using System.Diagnostics;

[assembly: InternalsVisibleTo("Nijo.IntegrationTest")]

namespace Nijo {
    public class Program {

        static async Task<int> Main(string[] args) {
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) => {
                cancellationTokenSource.Cancel();

                // キャンセル時のリソース解放を適切に行うために既定の動作（アプリケーション終了）を殺す
                e.Cancel = true;
            };

            var rootCommand = DefineCommand(cancellationTokenSource);

            var parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()

                // 例外処理
                .UseExceptionHandler((ex, _) => {
                    if (ex is OperationCanceledException) {
                        Console.Error.WriteLine("キャンセルされました。");
                    } else {
                        cancellationTokenSource.Cancel();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine(ex.ToString());
                        Console.ResetColor();
                    }
                })
                .Build();

            return await parser.InvokeAsync(args);
        }

        private static RootCommand DefineCommand(CancellationTokenSource cancellationTokenSource) {
            var rootCommand = new RootCommand("nijo");

            var services = new ServiceCollection();
            GeneratedProject.ConfigureDefaultServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // 引数定義
            var verbose = new Option<bool>("--verbose", description: "詳細なログを出力します。");
            var path = new Argument<string?>(() => string.Empty);
            var applicationName = new Argument<string?>();
            var keepTempIferror = new Option<bool>("--keep-temp-if-error", description: "エラー発生時、原因調査ができるようにするため一時フォルダを削除せず残します。");

            // コマンド定義
            var create = new Command(name: "create", description: "新しいプロジェクトを作成します。") { verbose, applicationName, keepTempIferror };
            create.SetHandler((verbose, applicationName, keepTempIferror) => {
                if (string.IsNullOrEmpty(applicationName)) throw new ArgumentException($"Application name is required.");
                var logger = ILoggerExtension.CreateConsoleLogger(verbose);
                var projectRootDir = Path.Combine(Directory.GetCurrentDirectory(), applicationName);
                GeneratedProject.Create(
                    projectRootDir,
                    applicationName,
                    keepTempIferror,
                    serviceProvider,
                    cancellationTokenSource.Token,
                    logger);
            }, verbose, applicationName, keepTempIferror);
            rootCommand.AddCommand(create);

            var debug = new Command(name: "debug", description: "プロジェクトのデバッグを開始します。") { verbose, path };
            debug.SetHandler((verbose, path) => {
                var logger = ILoggerExtension.CreateConsoleLogger(verbose);
                var project = GeneratedProject.Open(path, serviceProvider, logger);
                var firstLaunch = true;
                while (true) {
                    logger.LogInformation("-----------------------------------------------");
                    logger.LogInformation("デバッグを開始します。キーボードのQで終了します。それ以外のキーでリビルドします。");

                    using var launcher = project.CreateLauncher();
                    if (firstLaunch) {
                        // 初回ビルド時はブラウザ立ち上げ
                        launcher.OnReady += (s, e) => {
                            try {
                                var npmUrl = project.GetDebuggingClientUrl();
                                var launchBrowser = new Process();
                                launchBrowser.StartInfo.FileName = "cmd";
                                launchBrowser.StartInfo.Arguments = $"/c \"start {npmUrl}\"";
                                launchBrowser.Start();
                                launchBrowser.WaitForExit();
                            } catch (Exception ex) {
                                logger.LogError("Fail to launch browser: {msg}", ex.Message);
                            }
                            firstLaunch = false;
                        };
                    }
                    launcher.Launch();

                    // キー入力待機
                    var input = Console.ReadKey(true);
                    if (input.Key == ConsoleKey.Q) break;
                }
            }, verbose, path);
            rootCommand.AddCommand(debug);

            var fix = new Command(name: "fix", description: "コード自動生成処理をかけなおします。") { verbose, path };
            fix.SetHandler((verbose, path) => {
                var logger = ILoggerExtension.CreateConsoleLogger(verbose);
                GeneratedProject
                    .Open(path, serviceProvider, logger)
                    .CodeGenerator
                    .UpdateAutoGeneratedCode();
            }, verbose, path);
            rootCommand.AddCommand(fix);

            var dump = new Command(name: "dump", description: "スキーマ定義から構築したスキーマ詳細をTSV形式で出力します。") { verbose, path };
            dump.SetHandler((verbose, path) => {
                var logger = ILoggerExtension.CreateConsoleLogger(verbose);
                var tsv = GeneratedProject
                    .Open(path, serviceProvider, logger)
                    .BuildSchema()
                    .DumpTsv();
                Console.WriteLine(tsv);
            }, verbose, path);
            rootCommand.AddCommand(dump);

            return rootCommand;
        }
    }
}
