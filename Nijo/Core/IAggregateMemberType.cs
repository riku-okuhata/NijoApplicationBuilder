using Nijo.Models.ReadModel2Features;
using Nijo.Models.RefTo;
using Nijo.Models.WriteModel2Features;
using Nijo.Parts.Utility;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core {
    internal interface IAggregateMemberType {
        /// <summary>
        /// TODO: 廃止予定
        /// </summary>
        SearchBehavior SearchBehavior { get; }

        string GetCSharpTypeName();
        string GetTypeScriptTypeName();

        /// <summary>
        /// 詳細画面用のReactの入力コンポーネントを設定するコードの詳細を行います。
        /// </summary>
        ReactInputComponent GetReactComponent();
        /// <summary>
        /// グリッド用のReactの入力コンポーネントを設定するコードの詳細を行います。
        /// </summary>
        IGridColumnSetting GetGridColumnEditSetting();

        /// <summary>
        /// コード自動生成時に呼ばれる。C#の列挙体の定義を作成するなどの用途を想定している。
        /// </summary>
        void GenerateCode(CodeRenderingContext context) { }

        string GetSearchConditionCSharpType();
        string GetSearchConditionTypeScriptType();

        /// <summary>
        /// 検索条件の絞り込み処理をレンダリングします。
        /// </summary>
        /// <param name="member">検索対象のメンバーの情報</param>
        /// <param name="query"> <see cref="IQueryable{T}"/> の変数の名前</param>
        /// <param name="searchCondition">検索処理のパラメータの値の変数の名前</param>
        /// <param name="searchConditionObject">検索条件のオブジェクトの型</param>
        /// <param name="searchQueryObject">検索結果のクエリのオブジェクトの型</param>
        /// <returns> <see cref="IQueryable{T}"/> の変数に絞り込み処理をつけたものを再代入するソースコード</returns>
        string RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject);

        /// <summary>
        /// 検索条件欄のUI（"VerticalForm.Item" の子要素）をレンダリングします。
        /// </summary>
        /// <param name="vm">検索対象のメンバーの情報</param>
        /// <param name="ctx">コンテキスト引数</param>
        string RenderVFormBody(AggregateMember.ValueMember vm, ReactPageRenderingContext ctx);
    }

    /// <summary>検索条件のオブジェクトの型</summary>
    internal enum E_SearchConditionObject {
        /// <summary>検索条件の型は <see cref="Models.ReadModel2Features.SearchCondition"/> </summary>
        SearchCondition,
        /// <summary>検索条件の型は <see cref="Models.RefTo.RefSearchCondition"/> </summary>
        RefSearchCondition,
    }
    /// <summary>検索結果のクエリのオブジェクトの型</summary>
    internal enum E_SearchQueryObject {
        /// <summary>クエリのオブジェクトの型は <see cref="Models.WriteModel2Features.EFCoreEntity"/> </summary>
        EFCoreEntity,
        /// <summary>クエリのオブジェクトの型は <see cref="Models.ReadModel2Features.SearchResult"/> </summary>
        SearchResult,
    }

    /// <summary>
    /// 文字列系メンバー型
    /// </summary>
    public abstract class StringMemberType : IAggregateMemberType {

        /// <summary>
        /// 検索時の挙動。
        /// 既定値は <see cref="E_SearchBehavior.PartialMatch"/>
        /// </summary>
        protected virtual E_SearchBehavior SearchBehavior { get; } = E_SearchBehavior.PartialMatch;
        SearchBehavior IAggregateMemberType.SearchBehavior => SearchBehavior switch {
            E_SearchBehavior.PartialMatch => Core.SearchBehavior.PartialMatch,
            E_SearchBehavior.ForwardMatch => Core.SearchBehavior.ForwardMatch,
            E_SearchBehavior.BackwardMatch => Core.SearchBehavior.BackwardMatch,
            _ => Core.SearchBehavior.Strict,
        };

        public virtual string GetCSharpTypeName() => "string";
        public virtual string GetTypeScriptTypeName() => "string";

        public virtual string GetSearchConditionCSharpType() => "string";
        public virtual string GetSearchConditionTypeScriptType() => "string";

        public virtual ReactInputComponent GetReactComponent() {
            return new ReactInputComponent { Name = "Input.Word" };
        }

        public virtual IGridColumnSetting GetGridColumnEditSetting() {
            return new TextColumnSetting {
            };
        }
        string IAggregateMemberType.RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            var pathFromSearchCondition = searchConditionObject == E_SearchConditionObject.SearchCondition
                ? member.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.CSharp)
                : member.Declared.GetFullPathAsRefSearchConditionFilter(E_CsTs.CSharp);
            var fullpathNullable = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}";
            var fullpathNotNull = $"{searchCondition}.{pathFromSearchCondition.Join(".")}";
            var method = SearchBehavior switch {
                E_SearchBehavior.PartialMatch => "Contains",
                E_SearchBehavior.ForwardMatch => "StartsWith",
                E_SearchBehavior.BackwardMatch => "EndsWith",
                _ => "Equals",
            };
            var whereStatement = searchQueryObject == E_SearchQueryObject.SearchResult
                ? member.GetHandlingStatementAsSearchResult("Any", path => $"{path}.{method}(trimmed)")
                : member.GetHandlingStatementAsDbEntity("Any", path => $"{path}.{method}(trimmed)");

            return $$"""
                if (!string.IsNullOrWhiteSpace({{fullpathNullable}})) {
                    var trimmed = {{fullpathNotNull}}.Trim();
                    {{query}} = {{query}}.Where(x => x.{{whereStatement.Join(".")}});
                }
                """;
        }

        string IAggregateMemberType.RenderVFormBody(AggregateMember.ValueMember vm, ReactPageRenderingContext ctx) {
            var fullpath = ctx.RenderingObjectType switch {
                E_ReactPageRenderingObjectType.SearchCondition => vm.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.TypeScript).Join("."),
                E_ReactPageRenderingObjectType.RefTarget => vm.Declared.GetFullPathAsDataClassForRefTarget().Join("."),
                E_ReactPageRenderingObjectType.DataClassForDisplay => vm.Declared.GetFullPathAsDataClassForDisplay(E_CsTs.TypeScript).Join("."),
                _ => throw new NotImplementedException(),
            };

            return $$"""
                <Input.Word {...{{ctx.Register}}(`{{fullpath}}`)} />
                """;
        }

        /// <summary>
        /// 文字列検索の挙動
        /// </summary>
        public enum E_SearchBehavior {
            /// <summary>
            /// 完全一致。
            /// 発行されるSQL文: WHERE DBの値 = 検索条件
            /// </summary>
            Strict,
            /// <summary>
            /// 部分一致。
            /// 発行されるSQL文: WHERE DBの値 LIKE '%検索条件%'
            /// </summary>
            PartialMatch,
            /// <summary>
            /// 前方一致。
            /// 発行されるSQL文: WHERE DBの値 LIKE '検索条件%'
            /// </summary>
            ForwardMatch,
            /// <summary>
            /// 後方一致。
            /// 発行されるSQL文: WHERE DBの値 LIKE '%検索条件'
            /// </summary>
            BackwardMatch,
        }
    }

    /// <summary>
    /// 数値や日付など連続した量をもつ値
    /// </summary>
    public abstract class SchalarMemberType : IAggregateMemberType {
        public SearchBehavior SearchBehavior => SearchBehavior.Range;

        public abstract string GetCSharpTypeName();
        public abstract string GetTypeScriptTypeName();

        public string GetSearchConditionCSharpType() {
            var type = GetCSharpTypeName();
            return $"{FromTo.CLASSNAME}<{type}?>";
        }
        public string GetSearchConditionTypeScriptType() {
            var type = GetTypeScriptTypeName();
            return $"{{ {FromTo.FROM_TS}?: {type}, {FromTo.TO_TS}?: {type} }}";
        }

        public abstract IGridColumnSetting GetGridColumnEditSetting();
        public abstract ReactInputComponent GetReactComponent();

        string IAggregateMemberType.RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            var pathFromSearchCondition = searchConditionObject == E_SearchConditionObject.SearchCondition
                ? member.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.CSharp)
                : member.Declared.GetFullPathAsRefSearchConditionFilter(E_CsTs.CSharp);
            var nullableFullPathFrom = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}.{FromTo.FROM}";
            var nullableFullPathTo = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}.{FromTo.TO}";
            var fullPathFrom = $"{searchCondition}.{pathFromSearchCondition.Join(".")}.{FromTo.FROM}";
            var fullPathTo = $"{searchCondition}.{pathFromSearchCondition.Join(".")}.{FromTo.TO}";
            var whereStatementFromTo = searchQueryObject == E_SearchQueryObject.SearchResult
                ? member.GetHandlingStatementAsSearchResult("Any", path => $"{path} >= min && {path} <= max")
                : member.GetHandlingStatementAsDbEntity("Any", path => $"{path} >= min && {path} <= max");
            var whereStatementFromOnly = searchQueryObject == E_SearchQueryObject.SearchResult
                ? member.GetHandlingStatementAsSearchResult("Any", path => $"{path} >= from")
                : member.GetHandlingStatementAsDbEntity("Any", path => $"{path} >= from");
            var whereStatementToOnly = searchQueryObject == E_SearchQueryObject.SearchResult
                ? member.GetHandlingStatementAsSearchResult("Any", path => $"{path} <= to")
                : member.GetHandlingStatementAsDbEntity("Any", path => $"{path} <= to");

            return $$"""
                if ({{nullableFullPathFrom}} != null && {{nullableFullPathTo}} != null) {
                    // from, to のうち to の方が小さい場合は from-to を逆に読み替える
                    var min = {{fullPathFrom}} < {{fullPathTo}}
                        ? {{fullPathFrom}}
                        : {{fullPathTo}};
                    var max = {{fullPathFrom}} < {{fullPathTo}}
                        ? {{fullPathTo}}
                        : {{fullPathFrom}};
                    {{query}} = {{query}}.Where(x => x.{{whereStatementFromTo.Join(".")}});

                } else if ({{nullableFullPathFrom}} != null) {
                    var from = {{fullPathFrom}};
                    {{query}} = {{query}}.Where(x => x.{{whereStatementFromOnly.Join(".")}});

                } else if ({{nullableFullPathTo}} != null) {
                    var to = {{fullPathTo}};
                    {{query}} = {{query}}.Where(x => x.{{whereStatementToOnly.Join(".")}});
                }
                """;
        }

        string IAggregateMemberType.RenderVFormBody(AggregateMember.ValueMember vm, ReactPageRenderingContext ctx) {
            var component = GetReactComponent();
            var fullpath = ctx.RenderingObjectType switch {
                E_ReactPageRenderingObjectType.SearchCondition => vm.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.TypeScript).Join("."),
                E_ReactPageRenderingObjectType.RefTarget => vm.Declared.GetFullPathAsDataClassForRefTarget().Join("."),
                E_ReactPageRenderingObjectType.DataClassForDisplay => vm.Declared.GetFullPathAsDataClassForDisplay(E_CsTs.TypeScript).Join("."),
                _ => throw new NotImplementedException(),
            };

            return $$"""
                <{{component.Name}} {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.FROM_TS}}`)}{{component.GetPropsStatement().Join("")}} />
                <span className="select-none">～</span>
                <{{component.Name}} {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.TO_TS}}`)}{{component.GetPropsStatement().Join("")}} />
                """;
        }
    }

    /// <summary>
    /// 検索処理の挙動
    /// </summary>
    public enum SearchBehavior {
        /// <summary>
        /// 完全一致。
        /// 発行されるSQL文: WHERE DBの値 = 検索条件
        /// </summary>
        Strict,
        /// <summary>
        /// 部分一致。
        /// 発行されるSQL文: WHERE DBの値 LIKE '%検索条件%'
        /// </summary>
        PartialMatch,
        /// <summary>
        /// 前方一致。
        /// 発行されるSQL文: WHERE DBの値 LIKE '検索条件%'
        /// </summary>
        ForwardMatch,
        /// <summary>
        /// 後方一致。
        /// 発行されるSQL文: WHERE DBの値 LIKE '%検索条件'
        /// </summary>
        BackwardMatch,
        /// <summary>
        /// 範囲検索。
        /// 発行されるSQL文: WHERE DBの値 >= 検索条件.FROM
        ///                AND   DBの値 <= 検索条件.TO
        /// </summary>
        Range,
        /// <summary>
        /// 列挙体など。
        /// 発行されるSQL文: WHERE DBの値 IN (画面で選択された値1, 画面で選択された値2, ...)
        /// </summary>
        Contains,
    }
    /// <summary>
    /// 詳細画面用のReactの入力コンポーネント
    /// </summary>
    public sealed class ReactInputComponent {
        public required string Name { get; init; }
        public Dictionary<string, string> Props { get; init; } = [];

        /// <summary>
        /// <see cref="Props"/> をReactのコンポーネントのレンダリングの呼び出し時用の記述にして返す
        /// </summary>
        internal IEnumerable<string> GetPropsStatement() {
            foreach (var p in Props) {
                if (p.Value == string.Empty)
                    yield return $" {p.Key}";
                else if (p.Value.StartsWith("\"") && p.Value.EndsWith("\""))
                    yield return $" {p.Key}={p.Value}";
                else
                    yield return $" {p.Key}={{{p.Value}}}";
            }
        }
    }
    /// <summary>
    /// グリッド用のReactの入力コンポーネントの設定
    /// </summary>
    public interface IGridColumnSetting {
        /// <summary>
        /// セルの値を表示するとき、テキストボックスの初期値を設定するときに使われる文字列フォーマット処理。
        /// 第1引数はフォーマット前の値が入っている変数の名前、第2引数はフォーマット後の値が入る変数の名前。
        /// </summary>
        public Func<string, string, string>? GetValueFromRow { get; set; }
        /// <summary>
        /// テキストボックスで入力された文字列をデータクラスのプロパティに設定するときのフォーマット処理。
        /// 第1引数はフォーマット前の値が入っている変数の名前、第2引数はフォーマット後の値が入る変数の名前。
        /// </summary>
        public Func<string, string, string>? SetValueToRow { get; set; }
    }
    /// <summary>
    /// テキストボックスで編集するカラムの設定
    /// </summary>
    public sealed class TextColumnSetting : IGridColumnSetting {
        public Func<string, string, string>? GetValueFromRow { get; set; }
        public Func<string, string, string>? SetValueToRow { get; set; }
    }
    /// <summary>
    /// コンボボックスで編集するカラムの設定
    /// </summary>
    public sealed class ComboboxColumnSetting : IGridColumnSetting {
        public required string OptionItemTypeName { get; set; }
        public required string Options { get; set; }
        public required string EmitValueSelector { get; set; }
        public required string MatchingKeySelectorFromEmitValue { get; set; }
        public required string MatchingKeySelectorFromOption { get; set; }
        public required string TextSelector { get; set; }
        public required Func<string, string, string> OnClipboardCopy { get; set; }
        public required Func<string, string, string> OnClipboardPaste { get; set; }
        public Func<string, string, string>? GetDisplayText { get; set; }
        public Func<string, string, string>? GetValueFromRow { get; set; }
        public Func<string, string, string>? SetValueToRow { get; set; }
    }
    /// <summary>
    /// 非同期コンボボックスで編集するカラムの設定
    /// </summary>
    public sealed class AsyncComboboxColumnSetting : IGridColumnSetting {
        public required string OptionItemTypeName { get; set; }
        public required string QueryKey { get; set; }
        public required string Query { get; set; }
        public required string EmitValueSelector { get; set; }
        public required string MatchingKeySelectorFromEmitValue { get; set; }
        public required string MatchingKeySelectorFromOption { get; set; }
        public required string TextSelector { get; set; }
        public required Func<string, string, string> OnClipboardCopy { get; set; }
        public required Func<string, string, string> OnClipboardPaste { get; set; }
        public Func<string, string, string>? GetValueFromRow { get; set; }
        public Func<string, string, string>? SetValueToRow { get; set; }
    }
}
