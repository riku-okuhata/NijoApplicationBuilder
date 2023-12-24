using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nijo.Features.InstanceHandling;
using Nijo.Core;
using Nijo.DotnetEx;

namespace Nijo.Features.WebClient {
#pragma warning disable IDE1006 // 命名スタイル
    internal class index {
#pragma warning restore IDE1006 // 命名スタイル

        internal static SourceFile Render(IEnumerable<Infrastucture.IReactPage> reactPages) => new SourceFile {
            FileName = "index.tsx",
            RenderContent = ctx => $$"""
                import './nijo.css';
                import 'ag-grid-community/styles/ag-grid.css';
                import 'ag-grid-community/styles/ag-theme-alpine.css';

                {{reactPages.SelectTextTemplate(page => $$"""
                import {{page.ComponentPhysicalName}} from './{{Infrastucture.REACT_PAGE_DIR}}/{{page.DirNameInPageDir}}/{{Path.GetFileNameWithoutExtension(page.GetSourceFile().FileName)}}'
                """)}}

                export const THIS_APPLICATION_NAME = '{{ctx.Schema.ApplicationName}}' as const

                export const routes: { url: string, el: JSX.Element }[] = [
                {{reactPages.SelectTextTemplate(page => $$"""
                  { url: '{{page.Url}}', el: <{{page.ComponentPhysicalName}} /> },
                """)}}
                ]
                export const menuItems: { url: string, text: string }[] = [
                {{reactPages.Where(p => p.ShowMenu).SelectTextTemplate(page => $$"""
                  { url: '{{page.Url}}', text: '{{page.LabelInMenu}}' },
                """)}}
                ]
                """,
        };


        [Obsolete]
        internal static SourceFile Render(Func<GraphNode<Aggregate>, string> dirNameResolver) {

            return new SourceFile {
                FileName = "index.tsx",
                RenderContent = ctx => {
                    var components = GetComponents(ctx.Schema.RootAggregates(), dirNameResolver).ToArray();
                    return $$"""
                        import './nijo.css';
                        import 'ag-grid-community/styles/ag-grid.css';
                        import 'ag-grid-community/styles/ag-theme-alpine.css';

                        {{components.SelectTextTemplate(component => $$"""
                        import {{component.PhysicalName}} from '{{component.From}}'
                        """)}}

                        export const THIS_APPLICATION_NAME = '{{ctx.Schema.ApplicationName}}' as const

                        export const routes: { url: string, el: JSX.Element }[] = [
                        {{components.SelectTextTemplate(component => $$"""
                          { url: '{{component.Url}}', el: <{{component.PhysicalName}} /> },
                        """)}}
                        ]
                        export const menuItems: { url: string, text: string }[] = [
                        {{components.Where(c => c.ShowMenu).SelectTextTemplate(component => $$"""
                          { url: '{{component.Url}}', text: '{{component.DisplayName}}' },
                        """)}}
                        ]
                        """;
                },
            };
        }

        private static IEnumerable<ImportedComponent> GetComponents(
            IEnumerable<GraphNode<Aggregate>> rootAggregates,
            Func<GraphNode<Aggregate>, string> dirNameResolver) {

            foreach (var aggregate in rootAggregates) {
                var aggregateName = aggregate.Item.DisplayName.ToCSharpSafe();

                if (aggregate.IsSearchable()) {
                    yield return new ImportedComponent {
                        ShowMenu = true,
                        Url = new Searching.AggregateSearchFeature(aggregate).GetMultiView().Url,
                        PhysicalName = $"{aggregateName}MultiView",
                        DisplayName = aggregate.Item.DisplayName,
                        From = $"./{dirNameResolver(aggregate)}/{Path.GetFileNameWithoutExtension(Searching.MultiView.REACT_FILENAME)}",
                    };
                }

                if (aggregate.IsCreatable()) {
                    var createView = new SingleView(aggregate, SingleView.E_Type.Create);
                    yield return new ImportedComponent {
                        ShowMenu = aggregate.GetRoot().Item.Options.Type == E_AggreateType.Command,
                        Url = createView.Route,
                        PhysicalName = $"{aggregateName}CreateView",
                        DisplayName = aggregate.Item.DisplayName,
                        From = $"./{dirNameResolver(aggregate)}/{Path.GetFileNameWithoutExtension(createView.FileName)}",
                    };
                }

                if (aggregate.IsStored()) {
                    var detailView = new SingleView(aggregate, SingleView.E_Type.View);
                    yield return new ImportedComponent {
                        ShowMenu = false,
                        Url = detailView.Route,
                        PhysicalName = $"{aggregateName}DetailView",
                        DisplayName = aggregate.Item.DisplayName,
                        From = $"./{dirNameResolver(aggregate)}/{Path.GetFileNameWithoutExtension(detailView.FileName)}",
                    };
                }

                if (aggregate.IsEditable()) {
                    var editView = new SingleView(aggregate, SingleView.E_Type.Edit);
                    yield return new ImportedComponent {
                        ShowMenu = false,
                        Url = editView.Route,
                        PhysicalName = $"{aggregateName}EditView",
                        DisplayName = aggregate.Item.DisplayName,
                        From = $"./{dirNameResolver(aggregate)}/{Path.GetFileNameWithoutExtension(editView.FileName)}",
                    };
                }
            }
        }
        public class ImportedComponent {
            public required bool ShowMenu { get; init; }
            public required string Url { get; init; }
            public required string PhysicalName { get; init; }
            public required string DisplayName { get; init; }
            public required string From { get; init; }
        }
    }
}
