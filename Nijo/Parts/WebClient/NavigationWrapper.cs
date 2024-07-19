using Nijo.Core;
using Nijo.Features.Storing;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebClient {
    internal class NavigationWrapper {
        /// <summary>
        /// URLをラップする関数やフック
        /// </summary>
        internal NavigationWrapper(GraphNode<Aggregate> aggregate) {
            _aggregate = aggregate;
        }
        private readonly GraphNode<Aggregate> _aggregate;

        internal string GetSingleViewUrlHookName => $"get{_aggregate.Item.PhysicalName}SingleViewUrl";
    }


    /// <summary>
    /// UrlUtil.ts ファイル
    /// </summary>
    internal class UrlUtil : ISummarizedFile {

        private readonly List<GraphNode<Aggregate>> _aggregates = new();

        internal void Add(GraphNode<Aggregate> aggregate) {
            _aggregates.Add(aggregate);
        }

        void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {
            context.ReactProject.UtilDir(reactUtilDir => {
                reactUtilDir.Generate( new SourceFile {
                    FileName = "UrlUtil.ts",
                    RenderContent = ctx => {
                        return $$"""
                            import { ItemKey } from './LocalRepository'

                            {{_aggregates.SelectTextTemplate(agg => $$"""
                            {{RenderHooks(ctx, agg)}}

                            """)}}
                            """;
                    },
                });
            });
        }
        
        private static string RenderHooks(CodeRenderingContext context, GraphNode<Aggregate> aggregate) {
            var nav = new NavigationWrapper(aggregate);
            var create = new SingleView(aggregate.GetRoot(), SingleView.E_Type.Create);
            var view = new SingleView(aggregate.GetRoot(), SingleView.E_Type.View);
            var edit = new SingleView(aggregate.GetRoot(), SingleView.E_Type.Edit);
            var keyArray = KeyArray.Create(aggregate);

            return $$"""
                export const {{nav.GetSingleViewUrlHookName}} = (key: ItemKey | undefined, mode: 'new' | 'view' | 'edit'): string => {
                {{If(context.Config.DisableLocalRepository, () => $$"""
                  if (mode === 'new') {
                    return `{{create.GetUrlStringForReact()}}`
                  }
                  if (!key) {
                    return ''
                  }
                """).Else(() => $$"""
                  if (!key) {
                    return ''
                  }
                  if (mode === 'new') {
                    return `{{create.GetUrlStringForReact(["key"])}}`
                  }
                """)}}
                  const [{{keyArray.Select(k => k.VarName).Join(", ")}}] = JSON.parse(key) as [{{keyArray.Select(k => $"{k.TsType} | undefined").Join(", ")}}]
                  if (mode === 'view') {
                    return `{{view.GetUrlStringForReact(keyArray.Select(k => k.VarName))}}`
                  } else {
                    return `{{edit.GetUrlStringForReact(keyArray.Select(k => k.VarName))}}`
                  }
                }
                """;
        }
    }
}
