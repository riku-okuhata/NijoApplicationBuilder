using Microsoft.OpenApi.Services;
using Nijo.Core;
using Nijo.Models.WriteModel2Features;
using Nijo.Parts;
using Nijo.Parts.WebClient;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.ReadModel2Features
{
    /// <summary>
    /// 一括編集画面
    /// </summary>
    internal class MultiViewEditable
    {
        internal MultiViewEditable(GraphNode<Aggregate> rootAggregate)
        {
            _rootAggregate = rootAggregate;
        }
        private readonly GraphNode<Aggregate> _rootAggregate;

        public string Url => $"/{(_rootAggregate.Item.Options.LatinName ?? _rootAggregate.Item.UniqueId).ToKebabCase()}/multi-edit";
        public string DirNameInPageDir => _rootAggregate.Item.PhysicalName.ToFileNameSafe();
        public string ComponentPhysicalName => $"{_rootAggregate.Item.PhysicalName}MultiViewEditable";
        public string UiContextSectionName => ComponentPhysicalName;
        public bool ShowMenu => false;
        public string? LabelInMenu => null;

        internal const string HANDLE_BLUR = "handleBlur";

        public SourceFile GetSourceFile() => new()
        {
            FileName = "multi-view-editable.tsx",
            RenderContent = ctx =>
            {
                var dataClass = new DataClassForDisplay(_rootAggregate);
                var searchCondition = new SearchCondition(_rootAggregate);
                var loadMethod = new LoadMethod(_rootAggregate);
                var rootAggregateComponent = new MultiViewEditableAggregateComponent(_rootAggregate);

                string OnValueChange(AggregateMember.AggregateMemberBase m)
                {
                    return $$"""
                        (row, value, rowIndex) => {
                        {{If(m.Owner != _rootAggregate, () => $$"""
                          if (row.{{m.GetFullPathAsDataClassForDisplay(E_CsTs.TypeScript, _rootAggregate).SkipLast(1).Join("?.")}} === undefined) return
                        """)}}
                          row.{{m.GetFullPathAsDataClassForDisplay(E_CsTs.TypeScript, _rootAggregate).Join(".")}} = value
                          handleChangeRow(rowIndex, row)
                        }
                        """;
                }
                string ReadOnlyDynamic(AggregateMember.AggregateMemberBase member)
                {
                    return $"(row, rowIndex) => Util.isReadOnlyField(`data.${{rowIndex}}.{member.GetFullPathAsReactHookFormRegisterName(E_PathType.ReadOnly, ["rowIndex"]).Join(".")}`, reactHookFormMethods.getValues)";
                }
                var tableBuilder = Parts.WebClient.DataTable.DataTableBuilder.EditableGrid(_rootAggregate, $"AggregateType.{dataClass.TsTypeName}", OnValueChange, ReadOnlyDynamic)
                    // 行ヘッダ（列の状態）
                    .Add(new Parts.WebClient.DataTable.AdhocColumn
                    {
                        Header = string.Empty,
                        DefaultWidth = 48,
                        EnableResizing = false,
                        CellContents = (ctx, arg, argRowObject) => $$"""
                            {{arg}} => {
                              // 追加・更新・削除 の別を表示する
                              const row = {{argRowObject}}
                              const state = AggregateType.{{DataClassForSaveBase.GET_ADD_MOD_DEL_ENUM_TS}}(row)
                              return (
                                <Layout.AddModDelStateCell state={state} />
                              )
                            }
                            """,
                    })
                    // メンバーの列
                    .AddMembers(dataClass);

                var keys = _rootAggregate
                    .GetKeys()
                    .OfType<AggregateMember.ValueMember>();

                // 詳細欄を表示するかどうか
                var showDetailView = _rootAggregate
                    .EnumerateDescendants()
                    .Any(agg => agg.IsChildrenMember()
                             || agg.IsVariationMember());

                return $$"""
                    import React, { useState, useContext, useRef, useMemo, useCallback, useEffect } from 'react'
                    import useEvent from 'react-use-event-hook'
                    import * as ReactRouter from 'react-router-dom'
                    import { useFieldArray, FormProvider, useWatch, UseFormReturn, FieldPath } from 'react-hook-form'
                    import * as Icon from '@heroicons/react/24/outline'
                    import { Panel, PanelGroup, PanelResizeHandle, ImperativePanelHandle } from 'react-resizable-panels'
                    import * as Layout from '../../collection'
                    import { VForm2 } from '../../collection'
                    import * as Input from '../../input'
                    import * as Util from '../../util'
                    import * as AggregateType from '../../autogenerated-types'
                    import * as AggregateHook from '../../autogenerated-hooks'
                    import * as AggregateComponent from '../../autogenerated-components'
                    import * as RefTo from '../../ref-to'
                    import { {{UiContext.CONTEXT_NAME}} } from '../../default-ui-component'

                    export default function () {
                      // 画面初期表示時データ読み込み
                      const [loaded, setLoaded] = useState(false)
                      const { search: locationSerach } = ReactRouter.useLocation()
                      const { {{LoadMethod.LOAD}} } = AggregateHook.{{loadMethod.ReactHookName}}(true)
                      const reactHookFormMethods = Util.useFormEx<{ data: AggregateType.{{dataClass.TsTypeName}}[] }>({})
                      const { reset, setError, clearErrors } = reactHookFormMethods
                      const reload = useCallback(async () => {
                        if (!locationSerach) return
                        try {
                          setLoaded(false)
                          const condition = AggregateType.{{searchCondition.ParseQueryParameter}}(locationSerach)
                          const data = await {{LoadMethod.LOAD}}(condition)
                          reset({ data })
                        } finally {
                          setLoaded(true)
                        }
                      }, [locationSerach])

                      // 画面離脱（他画面への遷移）アラート設定
                      const blockCondition: ReactRouter.BlockerFunction = useEvent(({ currentLocation, nextLocation }) => {
                        if (currentLocation.pathname !== nextLocation.pathname) {
                          if (confirm('画面を移動すると、変更内容が破棄されます。よろしいでしょうか？')) return false
                        }
                        return currentLocation.pathname !== nextLocation.pathname
                      })
                      // ブロッカー
                      let blocker = ReactRouter.useBlocker(blockCondition)

                      React.useEffect(() => {
                        reload()

                        // 画面離脱（ブラウザ閉じるorタブ閉じる）アラート設定
                        const handleBeforeUnload: OnBeforeUnloadEventHandler = e => {
                          e.preventDefault()
                          return null
                        };
                        
                        window.addEventListener("beforeunload", handleBeforeUnload, false);
                        
                        return () => {
                          window.removeEventListener("beforeunload", handleBeforeUnload, false);
                        
                        };
                      }, [reload])

                      // 保存時
                      const { batchUpdateReadModels, nowSaving } = AggregateHook.{{BatchUpdateReadModel.HOOK_NAME}}()
                      const [, dispatchToast] = Util.useToastContext()
                      const handleSave = useEvent(async (updatedData: AggregateType.{{dataClass.TsTypeName}}[]) => {
                        clearErrors()
                        const batchUpdateArgs = updatedData.map(values => ({ dataType: '{{DataClassForSaveBase.GetEnumValueOf(_rootAggregate)}}' as const, values }))
                        const result = await batchUpdateReadModels(batchUpdateArgs, {
                          setError: (itemIndex, name, error) => {
                            setError(`data.${itemIndex}.${name}` as FieldPath<{ data: AggregateType.{{dataClass.TsTypeName}}[] }>, error)
                          },
                        })
                        if (!result.ok) {
                          setError('root', { types: { 'ERROR-0': 'エラーがあります。' } })
                          return
                        }
                        // 更新成功時は画面リロード
                        dispatchToast(msg => msg.info('更新しました。'))
                        reload()
                      })

                      return loaded ? (
                        <AfterLoaded reactHookFormMethods={reactHookFormMethods} onSave={handleSave} nowSaving={nowSaving} />
                      ) : (
                        <div className="relative w-full h-full">
                          <Input.NowLoading />
                        </div>
                      )
                    }

                    const AfterLoaded = ({ reactHookFormMethods, onSave, nowSaving }: {
                      reactHookFormMethods: UseFormReturn<{ data: AggregateType.{{dataClass.TsTypeName}}[] }>
                      onSave: (value: AggregateType.{{dataClass.TsTypeName}}[]) => void
                      nowSaving: boolean
                    }) => {
                      const tableRef = React.useRef<Layout.DataTableRef<AggregateType.{{dataClass.TsTypeName}}>>(null)
                      const { {{UiContextSectionName}}: UI, {{Parts.WebClient.DataTable.CellType.USE_HELPER}}, ...Components } = React.useContext({{UiContext.CONTEXT_NAME}})

                      // 編集中データ
                      const { insert, remove, update } = useFieldArray({ name: 'data', control: reactHookFormMethods.control })
                      const fields = useWatch({ name: 'data', control: reactHookFormMethods.control })

                      // 詳細部分のレイアウト
                      const [singleViewPosition, setSingleViewPosition] = React.useState<'horizontal' | 'vertical'>('horizontal')
                      const [singleViewCollapsed, setSingleViewCollapsed] = React.useState(false)
                      const handleCollapse = useEvent(() => setSingleViewCollapsed(true))
                      const handleExpand = useEvent(() => setSingleViewCollapsed(false))
                      const resizerCssClass = React.useMemo(() => {
                        return singleViewPosition === 'horizontal' ? 'w-2' : 'h-2'
                      }, [singleViewPosition])
                      const singleViewRef = React.useRef<ImperativePanelHandle>(null)
                      const handleClickHorizontal = useEvent(() => {
                        setSingleViewPosition('horizontal')
                        singleViewRef.current?.expand()
                      })
                      const handleClickVertical = useEvent(() => {
                        setSingleViewPosition('vertical')
                        singleViewRef.current?.expand()
                      })
                      const handleClickCollapse = useEvent(() => {
                        singleViewRef.current?.collapse()
                      })

                      // 列定義
                      const { complexPost } = Util.useHttpRequest()
                      const cellType = {{Parts.WebClient.DataTable.CellType.USE_HELPER}}<AggregateType.{{dataClass.TsTypeName}}>()
                      const handleChangeRow = useEvent((rowIndex: number, row: AggregateType.{{dataClass.TsTypeName}}) => {
                        const defaultValue = defaultValuesDict.get(getItemKeyAsString(row))
                        if (defaultValue) {
                          const changed = !AggregateType.{{dataClass.DeepEqualFunction}}(row, defaultValue)
                          update(rowIndex, { ...row, {{DataClassForDisplay.WILL_BE_CHANGED_TS}}: changed })
                        } else {
                          update(rowIndex, row)
                        }
                      })
                      const columnDefs = React.useMemo((): Layout.DataTableColumn<AggregateType.{{dataClass.TsTypeName}}>[] => [
                        {{WithIndent(tableBuilder.RenderColumnDef(ctx), "    ")}}
                      ], [complexPost, cellType])

                      // 選択されている行
                      const [activeRowIndex, setActiveRowIndex] = useState<number | undefined>(undefined)
                      const { debouncedValue: debouncedActiveRowIndex, debouncing } = Util.useDebounce(activeRowIndex, fields.length < 100 ? 0 : 300)
                      const handleActiveRowChanged = useEvent((e: { rowIndex: number } | undefined) => {
                        setActiveRowIndex(e?.rowIndex)
                      })

                      // 値変更チェック
                      const defaultValuesDict: Map<string, AggregateType.{{dataClass.TsTypeName}}> = React.useMemo(() => {
                        const data = (reactHookFormMethods.formState.defaultValues?.data ?? []) as AggregateType.{{dataClass.TsTypeName}}[]
                        return new Map(data.map(item => [getItemKeyAsString(item), item]))
                      }, [reactHookFormMethods.formState.defaultValues])
                      const setChangeFlag = useEvent((itemIndex: number) => {
                        const currentValue = reactHookFormMethods.getValues(`data.${itemIndex}`)
                        if (!currentValue) return
                        const defaultValue = defaultValuesDict.get(getItemKeyAsString(currentValue))
                        if (!defaultValue) return
                        const changed = !AggregateType.{{dataClass.DeepEqualFunction}}(currentValue, defaultValue)
                        reactHookFormMethods.setValue(`data.${itemIndex}.{{DataClassForDisplay.WILL_BE_CHANGED_TS}}`, changed)
                      })

                      // 行追加
                      const handleInsert = useEvent(() => {
                        insert(activeRowIndex ?? 0, AggregateType.{{dataClass.TsNewObjectFunction}}())
                      })

                      // 行削除
                      const handleRemove = useEvent(() => {
                        if (!tableRef.current) return
                        const removeIndexes: number[] = []
                        for (const x of tableRef.current.getSelectedRows()) {
                          const state = AggregateType.{{DataClassForSaveBase.GET_ADD_MOD_DEL_ENUM_TS}}(x.row)
                          if (state === 'ADD') {
                            removeIndexes.push(x.rowIndex)
                          } else {
                            reactHookFormMethods.setValue(`data.${x.rowIndex}.{{DataClassForDisplay.WILL_BE_DELETED_TS}}`, true)
                          }
                        }
                        remove(removeIndexes)
                      })

                      // リセット
                      const handleReset = useEvent(() => {
                        if (!confirm('選択されている行の変更を元に戻しますか？')) return
                        if (!tableRef.current) return
                        for (const x of tableRef.current.getSelectedRows()) {
                          const state = AggregateType.{{DataClassForSaveBase.GET_ADD_MOD_DEL_ENUM_TS}}(x.row)
                          if (state === 'MOD' || state === 'DEL') {
                            const defaultValue = defaultValuesDict.get(getItemKeyAsString(x.row))
                            if (defaultValue) update(x.rowIndex, { ...defaultValue })
                          }
                        }
                      })

                      // 詳細欄からフォーカスが外れたとき
                      const {{HANDLE_BLUR}} = useEvent(() => {
                        if (activeRowIndex !== undefined) setChangeFlag(activeRowIndex)
                      })

                      // 保存時
                      const handleSave = useEvent(() => {
                        onSave(reactHookFormMethods.getValues('data'))
                      })

                      // ブラウザのタイトル
                      const browserTitle = React.useMemo(() => {
                        return UI.{{SET_BROWSER_TITLE}}?.() ?? '{{_rootAggregate.Item.DisplayName.Replace("'", "\\'")}}'
                      }, [UI.{{SET_BROWSER_TITLE}}])

                      return (
                        <FormProvider {...reactHookFormMethods}>
                          <Layout.PageFrame
                            browserTitle={browserTitle}
                            nowLoading={nowSaving}
                            header={<>
                              <Layout.PageTitle>{{_rootAggregate.Item.DisplayName}}&nbsp;一括編集</Layout.PageTitle>
                              <Input.IconButton onClick={handleInsert} outline mini>追加</Input.IconButton>
                              <Input.IconButton onClick={handleRemove} outline mini>削除</Input.IconButton>
                              <Input.IconButton onClick={handleReset} outline mini>リセット</Input.IconButton>
                              <div className="flex-1"></div>
                    {{If(showDetailView, () => $$"""
                              <div className="flex gap-2 items-center">
                                <span className="select-none text-sm text-color-7">表示</span>
                                <div className="flex gap-1 border border-color-4">
                                  <Input.IconButton icon={Icon.ArrowsRightLeftIcon} hideText className="p-2" onClick={handleClickHorizontal} fill={!singleViewCollapsed && singleViewPosition === 'horizontal'}>左右に並べる</Input.IconButton>
                                  <Input.IconButton icon={Icon.ArrowsUpDownIcon} hideText className="p-2" onClick={handleClickVertical} fill={!singleViewCollapsed && singleViewPosition === 'vertical'}>上下に並べる</Input.IconButton>
                                  <Input.IconButton icon={Icon.ArrowsPointingOutIcon} hideText className="p-2" onClick={handleClickCollapse} fill={singleViewCollapsed}>一覧のみ表示</Input.IconButton>
                                </div>
                              </div>
                              <div className="basis-2"></div>
                    """)}}
                              <Input.IconButton onClick={handleSave} fill>保存</Input.IconButton>
                            </>}
                          >
                            <PanelGroup direction={singleViewPosition}>

                              {/* 一覧欄 */}
                              <Panel collapsible className="flex flex-col">
                                <Input.FormItemMessage name="root" />
                                <Layout.DataTable
                                  ref={tableRef}
                                  data={fields}
                                  columns={columnDefs}
                                  onActiveRowChanged={handleActiveRowChanged}
                                  onChangeRow={handleChangeRow}
                                  className="flex-1 border border-color-4"
                                />
                              </Panel>
                    {{If(showDetailView, () => $$"""

                              <PanelResizeHandle className={resizerCssClass} />

                              {/* 詳細欄 */}
                              <Panel
                                ref={singleViewRef}
                                defaultSize={0} // 初期表示時は折りたたみ状態
                                collapsible
                                onCollapse={handleCollapse}
                                onExpand={handleExpand}
                              >
                                <div className="relative h-full border border-color-4 overflow-auto">
                                  {!singleViewCollapsed && debouncedActiveRowIndex !== undefined && (
                                    {{WithIndent(rootAggregateComponent.RenderCaller(["debouncedActiveRowIndex"]), "                ")}}
                                  )}
                                  {!singleViewCollapsed && debouncedActiveRowIndex == undefined && (
                                    <span className="select-none text-sm text-color-7">ここには一覧で選択している行の詳細が表示されます。</span>
                                  )}
                                  {debouncing && (
                                    <Input.NowLoading />
                                  )}
                                </div>
                              </Panel>
                    """)}}
                            </PanelGroup>
                          </Layout.PageFrame>
                        </FormProvider>
                      )
                    }

                    {{rootAggregateComponent.EnumerateThisAndDescendantsRecursively().SelectTextTemplate(component => $$"""

                    {{component.RenderDeclaring(ctx)}}
                    """)}}

                    /** 値変更チェック用の辞書のキー取得関数 */
                    const getItemKeyAsString = (item: AggregateType.{{dataClass.TsTypeName}}): string => {
                      return JSON.stringify([
                    {{keys.SelectTextTemplate(vm => $$"""
                        item.{{vm.Declared.GetFullPathAsDataClassForDisplay(E_CsTs.TypeScript).Join("?.")}},
                    """)}}
                      ])
                    }
                    """;
            },
        };

        internal string NavigationHookName => $"useNavigateTo{_rootAggregate.Item.PhysicalName}MultiViewEditable";

        internal string RenderNavigationHook(CodeRenderingContext context)
        {
            var searchCondition = new SearchCondition(_rootAggregate);
            return $$"""
                /** {{_rootAggregate.Item.DisplayName}}の一括編集画面へ遷移します。初期表示時検索条件を指定することができます。 */
                export const {{NavigationHookName}} = () => {
                  const navigate = ReactRouter.useNavigate()

                  /** {{_rootAggregate.Item.DisplayName}}の一括編集画面へ遷移します。初期表示時検索条件を指定することができます。 */
                  return React.useCallback((init: Types.{{searchCondition.TsTypeName}}) => {
                    // 初期表示時検索条件の設定
                    const searchParams = new URLSearchParams()
                    searchParams.append('{{SearchCondition.URL_FILTER}}', JSON.stringify(init.{{SearchCondition.FILTER_TS}}))
                    if (init.{{SearchCondition.KEYWORD_TS}}) searchParams.append('{{SearchCondition.URL_KEYWORD}}', init.{{SearchCondition.KEYWORD_TS}})
                    if (init.{{SearchCondition.SORT_TS}} && init.{{SearchCondition.SORT_TS}}.length > 0) searchParams.append('{{SearchCondition.URL_SORT}}', JSON.stringify(init.{{SearchCondition.SORT_TS}}))

                    navigate({
                      pathname: '{{Url}}',
                      search: searchParams.toString(),
                    })
                  }, [navigate])
                }
                """;
        }

        #region カスタマイズ部分
        private const string SET_BROWSER_TITLE = "setBrowserTitle";

        internal void RegisterUiContext(UiContext uiContext)
        {
            var components = new MultiViewEditableAggregateComponent(_rootAggregate)
                .EnumerateThisAndDescendantsRecursively();

            uiContext.Add($$"""
                import * as {{UiContextSectionName}} from './{{ReactProject.PAGES}}/{{DirNameInPageDir}}/multi-view-editable'
                """, $$"""
                /** {{_rootAggregate.Item.DisplayName.Replace("*/", "")}} の一括編集画面 */
                {{UiContextSectionName}}: {
                  /** 一括編集画面全体 */
                  default: () => React.ReactNode
                  /** この画面を表示したときのブラウザのタイトルを編集します。 */
                  {{SET_BROWSER_TITLE}}?: () => string
                {{components.SelectTextTemplate(component => $$"""
                  {{WithIndent(component.RenderUiContextType(), "  ")}}
                """)}}
                }
                """, $$"""
                {{UiContextSectionName}}: {
                  default: {{UiContextSectionName}}.default,
                  {{SET_BROWSER_TITLE}}: undefined,
                {{components.SelectTextTemplate(component => $$"""
                  {{component.ComponentName}}: {{UiContextSectionName}}.{{component.ComponentName}},
                """)}}
                }
                """);
        }
        #endregion カスタマイズ部分
    }
}
