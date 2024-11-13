using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebClient {
    /// <summary>
    /// 画面の特定の部分のみのカスタマイズなど、自動生成後のReactプロジェクトでUIを柔軟にカスタマイズできるようにするためのReactコンテキスト。
    /// </summary>
    internal class UiContext : ISummarizedFile {

        internal const string CONTEXT_NAME = "UiContext";

        private const string CONTEXT_PROVIDER = "UiContextProvider";
        private const string TYPE_CUSTOMIZER = "UiCustomizer";
        private const string TYPE_AUTO_GENERATED_UI = "AutoGeneratedUi";
        private const string TYPE_CONTEXT_VALUE = "UiContextValue";

        private readonly HashSet<string> _import = [];
        private readonly List<string> _contextValueType = [];
        private readonly List<string> _defaultValue = [];

        internal void Add(string import, string contextValueType, string defaultValue) {
            _import.Add(import);
            _contextValueType.Add(contextValueType);
            _defaultValue.Add(defaultValue);
        }

        /// <summary>
        /// React context 直下のメンバー名とその型を定義します。
        /// </summary>
        [Obsolete("'Add' に変更になった")]
        internal void AddMember(string sourceCode) {
            _sourceCode.Add(sourceCode);
        }
        [Obsolete("'Add' に変更になった")]
        private readonly List<string> _sourceCode = new();

        void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {

            // データテーブルの列定義生成ヘルパーは集約定義にかかわらず必要なのでここでAddする
            context.ReactProject.Types.AddImport($$"""
                import { {{DataTable.CellType.USE_HELPER}} } from './collection/DataTable.CellType'
                """);
            _contextValueType.Add($$"""
                /** 列定義生成ヘルパー関数。グリッドの列定義ソースを簡略化してくれます。 */
                {{DataTable.CellType.USE_HELPER}}: typeof {{DataTable.CellType.USE_HELPER}},
                """);
            _defaultValue.Add($$"""
                {{DataTable.CellType.USE_HELPER}}
                """);

            context.ReactProject.Types.Add($$"""
                /**
                 * 自動生成されたUIコンポーネントの型の一覧。
                 * カスタマイズが加えられる前の自動生成されたままのもの。
                 */
                export type {{TYPE_AUTO_GENERATED_UI}} = {
                  /** アプリケーション名 */
                  applicationName: string
                  /** ログイン画面を使用する場合はここに指定してください。ログイン成功の場合は引数のコンポーネントをそのまま返してください。 */
                  LoginPage: (props: {
                    /** ログイン済みの場合に表示されるコンポーネント */
                    LoggedInContents: JSX.Element
                  }) => React.ReactNode
                  /** サイドメニュー（上部） */
                  SideMenuTop: () => React.ReactNode
                  /** サイドメニュー（下部） */
                  SideMenuBottom: () => React.ReactNode
                  /** 設定画面 */
                  UserSetting?: () => React.ReactNode
                {{_contextValueType.SelectTextTemplate(source => $$"""
                  {{WithIndent(source, "  ")}}
                """)}}
                }
                /**
                 * 自動生成されたUIコンポーネントにカスタマイズを加えた後の型の一覧。
                 * 各画面ではここに登録されたコンポーネントの中から必要なものをピックアップして画面を組み上げていく。
                 */
                export type {{TYPE_CONTEXT_VALUE}} = {{TYPE_AUTO_GENERATED_UI}} & {}
                /**
                 * UIコンポーネントのカスタマイズを定義する関数の型。
                 */
                export type {{TYPE_CUSTOMIZER}} = (defaultUi: {{TYPE_AUTO_GENERATED_UI}}) => {{TYPE_CONTEXT_VALUE}}                
                """);

            context.ReactProject.AutoGeneratedDir(dir => {
                dir.Generate(new SourceFile {
                    FileName = "default-ui-component.tsx",
                    RenderContent = ctx => {

                        return $$"""
                            import React from 'react'
                            import * as AggregateType from './autogenerated-types'
                            import { AutoGeneratedSideMenuTop, AutoGeneratedSideMenuBottom } from './autogenerated-menu'
                            import { {{DataTable.CellType.USE_HELPER}} } from './collection/DataTable.CellType'
                            {{WithIndent(_import, "")}}

                            /**
                             * 自動生成されたUIコンポーネントにカスタマイズを加えた後の型の一覧。
                             * 各画面ではここに登録されたコンポーネントの中から必要なものをピックアップして画面を組み上げていく。
                             */
                            export const {{CONTEXT_NAME}} = React.createContext({} as AggregateType.{{TYPE_CONTEXT_VALUE}})

                            export const {{CONTEXT_PROVIDER}} = ({ customizer, children }: {
                              customizer: AggregateType.{{TYPE_CUSTOMIZER}}
                              children?: React.ReactNode
                            }) => {
                              const uiContextValue = React.useMemo((): AggregateType.{{TYPE_CONTEXT_VALUE}} => {
                                return customizer({
                                  // 以下は自動生成されたコンポーネントです。
                                  // App.tsxでこれらを上書きする指定がされた場合はそちらが優先されます。
                                  // 特に上書き指定が無い場合は自動生成されたコンポーネントがそのまま使用されます。
                                  applicationName: '{{context.Config.RootNamespace.Replace("'", "")}}',
                                  LoginPage: ({ LoggedInContents }) => LoggedInContents,
                                  SideMenuTop: AutoGeneratedSideMenuTop,
                                  SideMenuBottom: AutoGeneratedSideMenuBottom,
                            {{_defaultValue.SelectTextTemplate(sourceCode => $$"""
                                  {{WithIndent(sourceCode, "      ")}},
                            """)}}
                                })
                              }, [customizer])
                              return (
                                <{{CONTEXT_NAME}}.Provider value={uiContextValue}>
                                  {children}
                                </{{CONTEXT_NAME}}.Provider>
                              )
                            }
                            """;
                    },
                });
            });
        }
    }
}
