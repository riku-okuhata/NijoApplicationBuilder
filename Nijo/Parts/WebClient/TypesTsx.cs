using Nijo.Core;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebClient {
    internal class TypesTsx : ISummarizedFile {

        private readonly Dictionary<GraphNode<Aggregate>, List<string>> _sourceCodes = new();

        /// <summary>
        /// TypeScriptのデータ構造定義のソースコードを追加します。
        /// </summary>
        internal void Add(GraphNode<Aggregate> aggregate, string sourceCode) {
            if (_sourceCodes.TryGetValue(aggregate, out var list)) {
                list.Add(sourceCode);
            } else {
                _sourceCodes.Add(aggregate, [sourceCode]);
            }
        }

        void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {
            context.ReactProject.AutoGeneratedDir(dir => {
                dir.Generate(new SourceFile {
                    FileName = "autogenerated-types.ts",
                    RenderContent = context => $$"""
                        import { UUID } from 'uuidjs'
                        import * as Util from './util'

                        {{_sourceCodes.SelectTextTemplate(item => $$"""
                        // ------------------ {{item.Key.Item.DisplayName}} ------------------
                        {{item.Value.SelectTextTemplate(source => $$"""
                        {{source}}

                        """)}}

                        """)}}
                        """,
                });
            });
        }
    }
}
