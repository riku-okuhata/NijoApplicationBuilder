using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nijo.IntegrationTest.Perspectives {
    [NonParallelizable]
    public partial class 観点 {

        #region 期待結果が定義されていない場合にテストの事前準備をスキップするための仕組み
        private static DelayedExecuter If(DataPattern pattern) {
            return new DelayedExecuter(pattern);
        }
        private class DelayedExecuter {
            public DelayedExecuter(DataPattern pattern) {
                _pattern = pattern;
            }
            private readonly DataPattern _pattern;
            private readonly Dictionary<E_DataPattern, Func<Task>> _describes = new();
            public DelayedExecuter When(E_DataPattern pattern, Func<Task> then) {
                _describes[pattern] = then;
                return this;
            }
            public async Task LaunchWebApi() {
                if (!_describes.TryGetValue(_pattern.AsEnum(), out var describe)) {
                    Assert.Warn("期待結果が定義されていません。");
                    return;
                }

                using var ct = new CancellationTokenSource();
                Task? dotnetRun = null;
                try {
                    // nijo.xmlの更新
                    File.WriteAllText(SharedResource.Project.SchemaXml.GetPath(), _pattern.LoadXmlString());
                    SharedResource.Project.CodeGenerator.UpdateAutoGeneratedCode();

                    // コードとDBを再作成
                    await Task.WhenAll(
                        Task.Run(SharedResource.Project.Migrator.DeleteAndRecreateDatabase),
                        SharedResource.Project.Debugger.BuildAsync(ct.Token, GeneratedProjectDebugger.E_NpmBuild.None));

                    dotnetRun = await SharedResource.Project.Debugger.CreateServerRunningProcess(ct.Token);

                    await describe();
                } finally {
                    ct.Cancel();
                    dotnetRun?.Wait();
                }
            }
            public async Task LaunchWebApiAndClient() {
                if (!_describes.TryGetValue(_pattern.AsEnum(), out var describe)) {
                    Assert.Warn("期待結果が定義されていません。");
                    return;
                }

                using var ct = new CancellationTokenSource();
                Task? dotnetRun = null;
                Task? npmStart = null;
                try {
                    // nijo.xmlの更新
                    File.WriteAllText(SharedResource.Project.SchemaXml.GetPath(), _pattern.LoadXmlString());
                    SharedResource.Project.CodeGenerator.UpdateAutoGeneratedCode();

                    // コードとDBを再作成
                    await Task.WhenAll(
                        Task.Run(SharedResource.Project.Migrator.DeleteAndRecreateDatabase),
                        SharedResource.Project.Debugger.BuildAsync(ct.Token, GeneratedProjectDebugger.E_NpmBuild.None));

                    // 開始
                    dotnetRun = await SharedResource.Project.Debugger.CreateServerRunningProcess(ct.Token);
                    npmStart = await SharedResource.Project.Debugger.CreateClientRunningProcess(ct.Token);

                    await describe();

                } finally {
                    ct.Cancel();
                    dotnetRun?.Wait();
                    npmStart?.Wait();
                }
            }
        }
        #endregion 期待結果が定義されていない場合にテストの事前準備をスキップするための仕組み
    }
}
