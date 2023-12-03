using HalApplicationBuilder.Core;
using HalApplicationBuilder.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalApplicationBuilder.IntegrationTest.Perspectives {
    partial class Perspective {
        [UseDataPatterns]
        public async Task コンパイルエラーチェック(DataPattern pattern) {
            try {
                //// デバッグ用スキーマダンプ ここから
                //var schema = SharedResource.Project.BuildSchema();
                //var allAggregates = schema
                //    .RootAggregates()
                //    .SelectMany(a => a.EnumerateThisAndDescendants())
                //    .ToArray();
                //var maxIndent = allAggregates.Max(a => a.EnumerateAncestors().Count());
                //foreach (var aggregate in allAggregates) {
                //    var depth = aggregate.EnumerateAncestors().Count();
                //    var indent1L = string.Concat(Enumerable.Repeat("\t", depth));
                //    var indent2L = "\t" + indent1L;
                //    var indent2R = string.Concat(Enumerable.Repeat("\t", maxIndent - depth));
                //    TestContext.Out.WriteLine($"{indent1L}{aggregate}");
                //    foreach (var member in aggregate.GetMembers()) {
                //        if (member is AggregateMember.ValueMember vm) {
                //            var originals = new List<AggregateMember.ValueMember>();
                //            var original = vm.Original;
                //            while (original != null) {
                //                originals.Add(original);
                //                original = original.Original;
                //            }
                //            TestContext.Out.WriteLine($"{indent2L}{vm.MemberName}{indent2R}\t{vm.GetType().Name}\t{originals.Select(o => $"{o.MemberName}\t{o.GetType().Name}").Join("\t")}");
                //        }
                //    }
                //}
                //Assert.Fail();
                //// デバッグ用スキーマダンプ ここまで

                File.WriteAllText(SharedResource.Project.SchemaXml.GetPath(), pattern.LoadXmlString());
                SharedResource.Project.CodeGenerator.UpdateAutoGeneratedCode();
                using var ct = new CancellationTokenSource();
                await SharedResource.Project.Debugger.BuildAsync(ct.Token, HalappProjectDebugger.E_NpmBuild.OnlyCompilerCheck);
            } catch (Exception ex) {
                TestContext.Out.WriteLine("--- SCHEMA ---");
                TestContext.Out.WriteLine(SharedResource.Project.BuildSchema().Graph.ToMermaidText());
                Assert.Fail(ex.Message);
            }
        }
    }
}
