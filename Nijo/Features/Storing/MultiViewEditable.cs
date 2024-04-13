using Nijo.Core;
using Nijo.Parts.Utility;
using Nijo.Parts.WebClient;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Features.Storing {
    /// <summary>
    /// 編集可能なマルチビュー。LocalRepositoryの仕組みに大きく依存している。
    /// 
    /// TODO: ソート
    /// </summary>
    internal class MultiViewEditable : IReactPage {
        internal MultiViewEditable(GraphNode<Aggregate> aggregate) {
            if (!aggregate.IsRoot()) throw new ArgumentException("Editable multi view requires root aggregate.");
            _aggregate = aggregate;
        }

        private readonly GraphNode<Aggregate> _aggregate;

        public string Url => $"/{_aggregate.Item.DisplayName.ToHashedString()}";
        string IReactPage.DirNameInPageDir => _aggregate.Item.DisplayName.ToFileNameSafe();
        string IReactPage.ComponentPhysicalName => $"{_aggregate.Item.DisplayName.ToCSharpSafe()}MultiView";
        bool IReactPage.ShowMenu => true;
        string? IReactPage.LabelInMenu => _aggregate.Item.DisplayName;

        SourceFile IReactPage.GetSourceFile() {
            var dataClass = new DisplayDataClass(_aggregate);
            var editView = new SingleView(_aggregate, SingleView.E_Type.Edit);
            var createView = new SingleView(_aggregate, SingleView.E_Type.Create);
            var findMany = new FindManyFeature(_aggregate);
            var createEmptyObject = new TSInitializerFunction(_aggregate).FunctionName;
            var rootLocalRepository = new LocalRepository(_aggregate);
            var refLocalRepos = dataClass
                .GetRefFromPropsRecursively()
                .Select(x => new LocalRepository(x.Item1.MainAggregate))
                .ToArray();
            var keys = _aggregate.GetKeys().OfType<AggregateMember.ValueMember>().ToArray();

            var groupedSearchConditions = findMany
                .EnumerateSearchConditionMembers()
                .Where(vm => !vm.Options.InvisibleInGui)
                .GroupBy(vm => vm.DeclaringAggregate)
                // 自身のメンバーを検索条件の先頭に表示する
                .OrderBy(group => group.Key == _aggregate ? 1 : 2);

            var rowHeader = new DataTableColumn {
                Id = "col-header",
                Header = string.Empty,
                Size = 64,
                EnableResizing = false,
                Cell = $$"""
                    cellProps => {
                      const row = cellProps.row.original.item
                      const singleViewUrl = row.{{DisplayDataClass.LOCAL_REPOS_STATE}} === '+'
                        ? `{{createView.GetUrlStringForReact(new[] { $"row.{DisplayDataClass.LOCAL_REPOS_ITEMKEY}" })}}`
                        : `{{editView.GetUrlStringForReact(keys.Select(k => $"row.{k.Declared.GetFullPathAsSingleViewDataClass().Join("?.")}"))}}`
                      return (
                        <div className="flex items-center gap-1 pl-1">
                          <Link to={singleViewUrl} className="text-link">詳細</Link>
                          <span className="inline-block w-4 text-center">{row.{{DisplayDataClass.LOCAL_REPOS_STATE}}}</span>
                        </div>
                      )
                    }
                    """,
            };
            var gridColumns = new[] { rowHeader }.Concat(DataTableColumn.FromMembers("item", _aggregate, false));

            return new SourceFile {
                FileName = "list.tsx",
                RenderContent = context => $$"""
                    import React, { useCallback, useEffect, useMemo, useRef, useState, useReducer } from 'react'
                    import { Link } from 'react-router-dom'
                    import { useFieldArray, FormProvider } from 'react-hook-form'
                    import dayjs from 'dayjs'
                    import * as Util from '../../util'
                    import * as Input from '../../input'
                    import * as Layout from '../../collection'
                    import * as AggregateType from '../../autogenerated-types'

                    const VForm = Layout.VerticalForm

                    export default function () {
                      const [, dispatchMsg] = Util.useMsgContext()

                      // 検索条件
                      const [filter, setFilter] = useState<AggregateType.{{findMany.TypeScriptConditionClass}}>(() => ({}))
                      const [currentPage, dispatchPaging] = useReducer(pagingReducer, { pageIndex: 0 })

                      const rhfSearchMethods = Util.useFormEx<AggregateType.{{findMany.TypeScriptConditionClass}}>({})
                      const getConditionValues = rhfSearchMethods.getValues
                      const registerExCondition = rhfSearchMethods.registerEx

                      // 編集対象（リモートリポジトリ + ローカルリポジトリ）
                      const editRange = useMemo(() => ({
                        filter,
                        skip: currentPage.pageIndex * 20,
                        take: 20,
                      }), [filter, currentPage])
                      const {
                        ready,
                        items: currentPageItems,
                        add: addToLocalRepository,
                        update: updateLocalRepositoryItem,
                        remove: deleteLocalRepositoryItem,
                      } = Util.{{rootLocalRepository.HookName}}(editRange)
                    {{refLocalRepos.SelectTextTemplate(repos => $$"""
                      const {
                        ready: {{repos.Aggregate.Item.ClassName}}IsReady,
                        items: {{repos.Aggregate.Item.ClassName}}Items,
                        add: addTo{{repos.Aggregate.Item.ClassName}}LocalRepository,
                        update: update{{repos.Aggregate.Item.ClassName}}LocalRepositoryItem,
                        remove: delete{{repos.Aggregate.Item.ClassName}}LocalRepositoryItem,
                      } = Util.{{rootLocalRepository.HookName}}(editRange)
                    """)}}

                      const reactHookFormMethods = Util.useFormEx<{ currentPageItems: GridRow[] }>({})
                      const { control, registerEx, handleSubmit, reset } = reactHookFormMethods
                      const { fields, append, update, remove } = useFieldArray({ name: 'currentPageItems', control })

                      useEffect(() => {
                        if (ready) reset({ currentPageItems })
                      }, [ready, currentPageItems])

                      const handleReload = useCallback(() => {
                        setFilter(getConditionValues())
                      }, [getConditionValues])

                      // データ編集
                      const handleAdd: React.MouseEventHandler<HTMLButtonElement> = useCallback(async () => {
                        const newItem = AggregateType.{{createEmptyObject}}()
                        append(await addToLocalRepository(newItem))
                      }, [append, addToLocalRepository])

                      const handleUpdateRow = useCallback(async (index: number, row: GridRow) => {
                        const [
                          item{{_aggregate.Item.ClassName}}{{string.Concat(dataClass.GetRefFromPropsRecursively().Select(x => $", item{x.Item1.MainAggregate.Item.ClassName}"))}}
                        ] = AggregateType.{{dataClass.ConvertFnNameToLocalRepositoryType}}(row)

                        await updateLocalRepositoryItem(item{{_aggregate.Item.ClassName}}.itemKey, item{{_aggregate.Item.ClassName}}.item)

                    {{dataClass.GetRefFromPropsRecursively().SelectTextTemplate(x => x.IsArray ? $$"""
                        for (let { itemKey, item } of item{{x.Item1.MainAggregate.Item.ClassName}}) {
                          await update{{x.Item1.MainAggregate.Item.ClassName}}LocalRepositoryItem(itemKey, item)
                        }
                    """ : $$"""
                        await update{{x.Item1.MainAggregate.Item.ClassName}}LocalRepositoryItem(item{{x.Item1.MainAggregate.Item.ClassName}}.itemKey, item{{x.Item1.MainAggregate.Item.ClassName}}.item)
                    """)}}

                        update(index, row)
                      }, [update, updateLocalRepositoryItem{{string.Concat(refLocalRepos.Select(r => $", update{r.Aggregate.Item.ClassName}LocalRepositoryItem"))}}])

                      const dtRef = useRef<Layout.DataTableRef<GridRow>>(null)
                      const handleRemove: React.MouseEventHandler<HTMLButtonElement> = useCallback(async () => {
                        if (!dtRef.current) return
                        const deletedRowIndex: number[] = []
                        for (const { row, rowIndex } of dtRef.current.getSelectedRows()) {
                          const deleted = await deleteLocalRepositoryItem(row.itemKey, row.item)
                          if (deleted) update(rowIndex, deleted)
                          else deletedRowIndex.push(rowIndex)
                        }
                        remove(deletedRowIndex)
                      }, [update, remove, deleteLocalRepositoryItem])

                      return (
                        <div className="page-content-root gap-4 pb-[50vh]">

                          <FormProvider {...rhfSearchMethods}>
                            <form className="flex flex-col">
                              <div className="flex gap-2 justify-start">
                                <h1 className="text-base font-semibold select-none py-1">
                                  {{_aggregate.Item.DisplayName}}
                                </h1>
                                <Input.Button onClick={handleReload}>再読み込み</Input.Button>
                                <div className="basis-4"></div>
                                <Input.Button onClick={handleAdd}>追加</Input.Button>
                                <Input.Button onClick={handleRemove}>削除</Input.Button>
                              </div>
                              <VForm.Container leftColumnMinWidth="10rem">
                    {{groupedSearchConditions.SelectTextTemplate(group => $$"""
                    {{If(group.Key == _aggregate, () => $$"""
                                {{WithIndent(group.SelectTextTemplate(RenderSearchConditionValueMember), "            ")}}
                    """).Else(() => $$"""
                                <VForm.Container label="{{group.Key.Item.DisplayName}}">
                                  {{WithIndent(group.SelectTextTemplate(RenderSearchConditionValueMember), "              ")}}
                                </VForm.Container>
                    """)}}
                    """)}}
                              </VForm.Container>
                            </form>
                          </FormProvider>

                          <FormProvider {...reactHookFormMethods}>
                            <form className="flex-1">
                              <Layout.DataTable
                                ref={dtRef}
                                data={fields}
                                columns={COLUMN_DEFS}
                                onChangeRow={handleUpdateRow}
                                className="h-full"
                              ></Layout.DataTable>
                            </form>
                          </FormProvider>
                        </div>
                      )
                    }

                    type GridRow = AggregateType.{{dataClass.TsTypeName}}

                    const COLUMN_DEFS: Layout.ColumnDefEx<Util.TreeNode<GridRow>>[] = [
                      {{WithIndent(gridColumns.SelectTextTemplate(col => col.Render()), "  ")}}
                    ]

                    // TODO: utilに持っていく
                    type PageState = { pageIndex: number, loaded?: boolean }
                    const pagingReducer = Util.defineReducer((state: PageState) => ({
                      loadComplete: () => ({ pageIndex: state.pageIndex, loaded: true }),
                      nextPage: () => ({ pageIndex: state.pageIndex + 1, loaded: false }),
                      prevPage: () => ({ pageIndex: Math.max(0, state.pageIndex - 1), loaded: false }),
                      moveTo: (pageIndex: number) => ({ pageIndex, loaded: false }),
                    }))
                    """,
            };
        }

        private static string RenderSearchConditionValueMember(AggregateMember.ValueMember vm) {
            var component = vm.Options.MemberType.GetReactComponent(new() {
                Type = GetReactComponentArgs.E_Type.InDetailView,
            });

            return $$"""
                <VForm.Item label="{{vm.MemberName}}">
                {{If(vm is AggregateMember.Variation, () => FindManyFeature.VariationMemberProps((AggregateMember.Variation)vm).SelectTextTemplate(x => $$"""
                  <label className="inline-flex items-center">
                    <Input.CheckBox {...registerExCondition(`{{vm.Declared.GetFullPath().SkipLast(1).Concat(new[] { x.Value }).Join(".")}}`)} />
                    {{x.Key.MemberName}}
                  </label>
                """)).ElseIf(vm.Options.MemberType.SearchBehavior == SearchBehavior.Range, () => $$"""
                  <{{component.Name}} {...registerExCondition(`{{vm.Declared.GetFullPath().Join(".")}}.{{FromTo.FROM}}`)}{{string.Concat(component.GetPropsStatement())}} />
                  <span className="select-none">～</span>
                  <{{component.Name}} {...registerExCondition(`{{vm.Declared.GetFullPath().Join(".")}}.{{FromTo.TO}}`)}{{string.Concat(component.GetPropsStatement())}} />
                """).Else(() => $$"""
                  <{{component.Name}} {...registerExCondition(`{{vm.Declared.GetFullPath().Join(".")}}`)}{{string.Concat(component.GetPropsStatement())}} />
                """)}}
                </VForm.Item>
                """;
        }
    }
}
