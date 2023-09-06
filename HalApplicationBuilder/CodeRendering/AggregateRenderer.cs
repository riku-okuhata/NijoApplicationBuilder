using HalApplicationBuilder.CodeRendering.Presentation;
using HalApplicationBuilder.CodeRendering.Util;
using HalApplicationBuilder.Core;
using HalApplicationBuilder.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HalApplicationBuilder.CodeRendering.TemplateTextHelper;

namespace HalApplicationBuilder.CodeRendering {
    internal partial class AggregateRenderer : TemplateBase {

        internal AggregateRenderer(GraphNode<Aggregate> aggregate, CodeRenderingContext ctx) {
            if (!aggregate.IsRoot())
                throw new ArgumentException($"{nameof(AggregateRenderer)} requires root aggregate.", nameof(aggregate));

            _aggregate = aggregate;
            _dbEntity = aggregate.GetDbEntity().AsEntry().As<EFCoreEntity>();
            _aggregateInstance = aggregate.GetInstanceClass().AsEntry().As<AggregateInstance>();

            _controller = new WebClient.Controller(_aggregate.Item, ctx);
            _create = new CreateMethod(this, ctx);
            _update = new UpdateMethod(this, ctx);
            _delete = new DeleteMethod(this, ctx);

            _ctx = ctx;
        }


        private readonly GraphNode<Aggregate> _aggregate;
        private readonly GraphNode<EFCoreEntity> _dbEntity;
        private readonly GraphNode<AggregateInstance> _aggregateInstance;

        private readonly CodeRenderingContext _ctx;

        public override string FileName => $"{_aggregate.Item.DisplayName.ToFileNameSafe()}.cs";
        private const string E = "e";

        public const string GETINSTANCEKEY_METHOD_NAME = "GetInstanceKey";
        public const string GETINSTANCENAME_METHOD_NAME = "GetInstanceName";
        public const string TOKEYNAMEPAIR_METHOD_NAME = "ToKeyNamePair";

        private IEnumerable<NavigationProperty.Item> EnumerateNavigationProperties(GraphNode<EFCoreEntity> entity) {
            foreach (var nav in entity.GetNavigationProperties(_ctx.Config)) {
                if (nav.Principal.Owner == entity) yield return nav.Principal;
                if (nav.Relevant.Owner == entity) yield return nav.Relevant;
            }
        }

        #region CREATE
        private readonly CreateMethod _create;
        internal class CreateMethod {
            internal CreateMethod(AggregateRenderer aggFile, CodeRenderingContext ctx) {
                _aggregate = aggFile._aggregate;
                _dbEntity = aggFile._dbEntity;
                _instance = aggFile._aggregateInstance;
                _ctx = ctx;
            }

            private readonly GraphNode<Aggregate> _aggregate;
            private readonly GraphNode<EFCoreEntity> _dbEntity;
            private readonly GraphNode<AggregateInstance> _instance;
            private readonly CodeRenderingContext _ctx;

            internal string ArgType => _instance.Item.ClassName;
            internal string MethodName => $"Create{_aggregate.Item.DisplayName.ToCSharpSafe()}";
        }
        #endregion CREATE


        #region UPDATE
        private readonly UpdateMethod _update;
        internal class UpdateMethod {
            internal UpdateMethod(AggregateRenderer aggFile, CodeRenderingContext ctx) {
                _aggregate = aggFile._aggregate;
                _dbEntity = aggFile._dbEntity;
                _instance = aggFile._aggregateInstance;
                _ctx = ctx;
            }

            private readonly GraphNode<Aggregate> _aggregate;
            private readonly GraphNode<EFCoreEntity> _dbEntity;
            private readonly GraphNode<AggregateInstance> _instance;
            private readonly CodeRenderingContext _ctx;

            internal string MethodName => $"Update{_aggregate.Item.DisplayName.ToCSharpSafe()}";

            internal string RenderDescendantsAttaching(string dbContext, string before, string after) {
                var builder = new StringBuilder();

                var descendantDbEntities = _dbEntity.EnumerateDescendants().ToArray();
                for (int i = 0; i < descendantDbEntities.Length; i++) {
                    var paths = descendantDbEntities[i].PathFromEntry().ToArray();

                    // before, after それぞれの子孫インスタンスを一次配列に格納する
                    void RenderEntityArray(bool renderBefore) {
                        if (paths.Any(path => path.Terminal.IsChildrenMember())) {
                            // 子集約までの経路の途中に配列が含まれる場合
                            builder.AppendLine($"var arr{i}_{(renderBefore ? "before" : "after")} = {(renderBefore ? before : after)}");

                            var select = false;
                            foreach (var path in paths) {
                                if (select && path.Terminal.IsChildrenMember()) {
                                    builder.AppendLine($"    .SelectMany(x => x.{path.RelationName})");
                                } else if (select) {
                                    builder.AppendLine($"    .Select(x => x.{path.RelationName})");
                                } else {
                                    builder.AppendLine($"    .{path.RelationName}");
                                    if (path.Terminal.IsChildrenMember()) select = true;
                                }
                            }
                            builder.AppendLine($"    .ToArray();");

                        } else {
                            // 子集約までの経路の途中に配列が含まれない場合
                            builder.AppendLine($"var arr{i}_{(renderBefore ? "before" : "after")} = new {descendantDbEntities[i].Item.ClassName}[] {{");
                            builder.AppendLine($"    {(renderBefore ? before : after)}.{paths.Select(p => p.RelationName).Join(".")},");
                            builder.AppendLine($"}};");
                        }
                    }
                    RenderEntityArray(true);
                    RenderEntityArray(false);

                    // ChangeState変更
                    builder.AppendLine($"foreach (var a in arr{i}_after) {{");
                    builder.AppendLine($"    var b = arr{i}_before.SingleOrDefault(b => b.{EFCoreEntity.KEYEQUALS}(a));");
                    builder.AppendLine($"    if (b == null) {{");
                    builder.AppendLine($"        {dbContext}.Entry(a).State = EntityState.Added;");
                    builder.AppendLine($"    }} else {{");
                    builder.AppendLine($"        {dbContext}.Entry(a).State = EntityState.Modified;");
                    builder.AppendLine($"    }}");
                    builder.AppendLine($"}}");

                    builder.AppendLine($"foreach (var b in arr{i}_before) {{");
                    builder.AppendLine($"    var a = arr{i}_after.SingleOrDefault(a => a.{EFCoreEntity.KEYEQUALS}(b));");
                    builder.AppendLine($"    if (a == null) {{");
                    builder.AppendLine($"        {dbContext}.Entry(b).State = EntityState.Deleted;");
                    builder.AppendLine($"    }}");
                    builder.AppendLine($"}}");
                }

                return builder.ToString();
            }
        }
        #endregion UPDATE


        #region DELETE
        private readonly DeleteMethod _delete;
        internal class DeleteMethod {
            internal DeleteMethod(AggregateRenderer aggFile, CodeRenderingContext ctx) {
                _aggregate = aggFile._aggregate;
                _dbEntity = aggFile._dbEntity;
                _instance = aggFile._aggregateInstance;
                _ctx = ctx;
            }

            private readonly GraphNode<Aggregate> _aggregate;
            private readonly GraphNode<EFCoreEntity> _dbEntity;
            private readonly GraphNode<AggregateInstance> _instance;
            private readonly CodeRenderingContext _ctx;

            internal string MethodName => $"Delete{_aggregate.Item.DisplayName.ToCSharpSafe()}";
        }
        #endregion DELETE


        #region LIST BY KEYWORD
        private const int LIST_BY_KEYWORD_MAX = 100;
        private string ListByKeywordMethodName => $"SearchByKeyword{_aggregate.Item.DisplayName.ToCSharpSafe()}";
        private IEnumerable<ListByKeywordTargetColumn> EnumerateListByKeywordTargetColumns() {
            return _dbEntity
                .GetColumns()
                .Where(col => col.IsPrimary || col.IsInstanceName)
                .Select(col => new ListByKeywordTargetColumn {
                    Name = col.PropertyName,
                    NameAsString = col.MemberType.GetCSharpTypeName().Contains("string")
                        ? col.PropertyName
                        : $"{col.PropertyName}.ToString()",
                    Path = col.Owner
                        .PathFromEntry()
                        .Select(edge => edge.RelationName)
                        .Concat(new[] { col.PropertyName })
                        .Join("."),
                    IsInstanceKey = col.IsPrimary,
                    IsInstanceName = col.IsInstanceName,
                });
        }
        private class ListByKeywordTargetColumn {
            internal required string Path { get; init; }
            internal required string Name { get; init; }
            internal required string NameAsString { get; init; }
            internal required bool IsInstanceKey { get; init; }
            internal required bool IsInstanceName { get; init; }
        }
        #endregion LIST BY KEYWORD


        #region AGGREGATE INSTANCE & CREATE COMMAND
        private string CreateCommandClassName => $"{_aggregate.Item.DisplayName.ToCSharpSafe()}CreateCommand";
        private string CreateCommandToDbEntityMethodName => AggregateInstance.TO_DB_ENTITY_METHOD_NAME;
        private string CreateCommandGetInstanceKeyMethodName => GETINSTANCENAME_METHOD_NAME;

        private string ToDbEntity() {
            var builder = new StringBuilder();
            var indent = "";

            void WriteBody(GraphNode<AggregateInstance> instance, string parentPath, string instancePath, int depth) {
                // 親のPK
                var parent = instance.GetParent()?.Initial;
                if (parent != null) {
                    var parentPkColumns = instance
                        .GetDbEntity()
                        .GetColumns()
                        .Where(col => col is EFCoreEntity.ParentTablePrimaryKey)
                        .Cast<EFCoreEntity.ParentTablePrimaryKey>();
                    foreach (var col in parentPkColumns) {
                        builder.AppendLine($"{indent}{col.PropertyName} = {parentPath}.{col.CorrespondingParentColumn.PropertyName},");
                    }
                }
                // 自身のメンバー
                foreach (var prop in instance.GetSchalarProperties()) {
                    builder.AppendLine($"{indent}{prop.CorrespondingDbColumn.PropertyName} = {instancePath}.{prop.PropertyName},");
                }
                foreach (var prop in instance.GetVariationSwitchProperties(_ctx.Config)) {
                    builder.AppendLine($"{indent}{prop.CorrespondingDbColumn.PropertyName} = {instancePath}.{prop.PropertyName},");
                }
                // Ref
                foreach (var prop in instance.GetRefProperties(_ctx.Config)) {
                    for (int i = 0; i < prop.CorrespondingDbColumns.Length; i++) {
                        var col = prop.CorrespondingDbColumns[i];
                        builder.AppendLine($"{indent}{col.PropertyName} = ({col.MemberType.GetCSharpTypeName()}){InstanceKey.CLASS_NAME}.{InstanceKey.PARSE}({instancePath}.{prop.PropertyName}.{AggregateInstanceKeyNamePair.KEY}).{InstanceKey.OBJECT_ARRAY}[{i}],");
                    }
                }
                // 子要素
                foreach (var child in instance.GetChildrenProperties(_ctx.Config)) {
                    var childProp = child.CorrespondingNavigationProperty.Principal.PropertyName;
                    var childDbEntity = $"{_ctx.Config.EntityNamespace}.{child.CorrespondingNavigationProperty.Relevant.Owner.Item.ClassName}";

                    builder.AppendLine($"{indent}{child.PropertyName} = this.{childProp}.Select(x{depth} => new {childDbEntity} {{");
                    indent += "    ";
                    WriteBody(child.ChildAggregateInstance.AsEntry(), instancePath, $"x{depth}", depth + 1);
                    indent = indent.Substring(indent.Length - 4, 4);
                    builder.AppendLine($"{indent}}}).ToList(),");
                }
                foreach (var child in instance.GetChildProperties(_ctx.Config)) {
                    var childProp = child.CorrespondingNavigationProperty.Principal.PropertyName;
                    var childDbEntity = $"{_ctx.Config.EntityNamespace}.{child.CorrespondingNavigationProperty.Relevant.Owner.Item.ClassName}";

                    builder.AppendLine($"{indent}{child.PropertyName} = new {childDbEntity} {{");
                    indent += "    ";
                    WriteBody(child.ChildAggregateInstance, instancePath, $"{instancePath}.{child.ChildAggregateInstance.Source!.RelationName}", depth + 1);
                    indent = indent.Substring(indent.Length - 4, 4);
                    builder.AppendLine($"{indent}}},");
                }
                foreach (var child in instance.GetVariationProperties(_ctx.Config)) {
                    var childProp = child.CorrespondingNavigationProperty.Principal.PropertyName;
                    var childDbEntity = $"{_ctx.Config.EntityNamespace}.{child.CorrespondingNavigationProperty.Relevant.Owner.Item.ClassName}";

                    builder.AppendLine($"{indent}{child.PropertyName} = new {childDbEntity} {{");
                    indent += "    ";
                    WriteBody(child.ChildAggregateInstance, instancePath, $"{instancePath}.{child.ChildAggregateInstance.Source!.RelationName}", depth + 1);
                    indent = indent.Substring(indent.Length - 4, 4);
                    builder.AppendLine($"{indent}}},");
                }
            }

            builder.AppendLine($"{indent}return new {_ctx.Config.EntityNamespace}.{_dbEntity.Item.ClassName} {{");
            indent += "    ";
            WriteBody(_aggregateInstance, "", "this", 0);
            indent = indent.Substring(indent.Length - 4, 4);
            builder.AppendLine($"{indent}}};");

            return builder.ToString();
        }

        private IEnumerable<string> GetInstanceNameProps() {
            var useKeyInsteadOfName = _aggregateInstance
                .GetSchalarProperties()
                .Any(p => p.CorrespondingDbColumn.IsInstanceName) == false;
            var props = useKeyInsteadOfName
                ? _aggregateInstance
                    .GetSchalarProperties()
                    .Where(p => p.CorrespondingDbColumn.IsPrimary)
                    .ToArray()
                : _aggregateInstance
                    .GetSchalarProperties()
                    .Where(p => p.CorrespondingDbColumn.IsInstanceName)
                    .ToArray();
            if (props.Length == 0) {
                yield return $"return string.Empty;";
            } else {
                for (int i = 0; i < props.Length; i++) {
                    var head = i == 0 ? "return " : "    + ";
                    yield return $"{head}this.{props[i].PropertyName}?.ToString()";
                }
                yield return $"    ?? string.Empty;";
            }
        }
        #endregion AGGREGATE INSTANCE & CREATE COMMAND


        #region CONTROLLER
        private readonly WebClient.Controller _controller;
        #endregion CONTROLLER
        protected override string Template() {
            var search = new Searching.SearchFeature(_dbEntity, _ctx);
            var find = new Finding.FindFeature(_aggregate, _ctx);
            var fromDbEntity = new InstanceConverting.FromDbEntityRenderer(_aggregate, _ctx);

            var keyColumns = EnumerateListByKeywordTargetColumns().Where(col => col.IsInstanceKey).ToArray();
            var nameColumns = EnumerateListByKeywordTargetColumns().Where(col => col.IsInstanceName).ToArray();

            return $$"""
                #pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
                #pragma warning disable CS8618 // null 非許容の変数には、コンストラクターの終了時に null 以外の値が入っていなければなりません
                #pragma warning disable IDE1006 // 命名スタイル

                #region データ新規作成
                namespace {{_ctx.Config.RootNamespace}} {
                    using Microsoft.AspNetCore.Mvc;
                    using {{_ctx.Config.EntityNamespace}};

                    partial class {{_controller.ClassName}} : ControllerBase {
                        [HttpPost("{{WebClient.Controller.CREATE_ACTION_NAME}}")]
                        public virtual IActionResult Create([FromBody] {{CreateCommandClassName}} param) {
                            if (_dbContext.{{_create.MethodName}}(param, out var created, out var errors)) {
                                return this.JsonContent(created);
                            } else {
                                return BadRequest(this.JsonContent(errors));
                            }
                        }
                    }
                }
                namespace {{_ctx.Config.EntityNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Infrastructure;

                    partial class {{_ctx.Config.DbContextName}} {
                        public bool {{_create.MethodName}}({{CreateCommandClassName}} command, out {{_aggregateInstance.Item.ClassName}} created, out ICollection<string> errors) {
                            var dbEntity = command.{{CreateCommandToDbEntityMethodName}}();
                            this.Add(dbEntity);

                            try {
                                this.SaveChanges();
                            } catch (DbUpdateException ex) {
                                created = new {{_aggregateInstance.Item.ClassName}}();
                                errors = ex.GetMessagesRecursively("  ").ToList();
                                return false;
                            }

                            var instanceKey = command.{{CreateCommandGetInstanceKeyMethodName}}().ToString();
                            var afterUpdate = this.{{find.FindMethodName}}(instanceKey);
                            if (afterUpdate == null) {
                                created = new {{_aggregateInstance.Item.ClassName}}();
                                errors = new[] { "更新後のデータの再読み込みに失敗しました。" };
                                return false;
                            }

                            created = afterUpdate;
                            errors = new List<string>();
                            return true;
                        }
                    }
                }
                #endregion データ新規作成


                #region 一覧検索
                {{search.RenderControllerAction()}}
                {{search.RenderDbContextMethod()}}
                #endregion 一覧検索


                #region キーワード検索
                namespace {{_ctx.Config.RootNamespace}} {
                    using Microsoft.AspNetCore.Mvc;
                    using {{_ctx.Config.EntityNamespace}};

                    partial class {{_controller.ClassName}} {
                        [HttpGet("{{WebClient.Controller.KEYWORDSEARCH_ACTION_NAME}}")]
                        public virtual IActionResult SearchByKeyword([FromQuery] string? keyword) {
                            var items = _dbContext.{{ListByKeywordMethodName}}(keyword);
                            return this.JsonContent(items);
                        }
                    }
                }
                namespace {{_ctx.Config.EntityNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Infrastructure;

                    partial class {{_ctx.Config.DbContextName}} {
                        /// <summary>
                        /// {{_aggregate.Item.DisplayName}}をキーワードで検索します。
                        /// </summary>
                        public IEnumerable<{{AggregateInstanceKeyNamePair.CLASSNAME}}> {{ListByKeywordMethodName}}(string? keyword) {
                            var query = this.{{_dbEntity.Item.DbSetName}}.Select(e => new {
                {{EnumerateListByKeywordTargetColumns().SelectTextTemplate(col => $$"""
                                e.{{col.Path}},
                """)}}
                            });

                            if (!string.IsNullOrWhiteSpace(keyword)) {
                                var like = $"%{keyword.Trim().Replace("%", "\\%")}%";
                                query = query.Where(item => {{EnumerateListByKeywordTargetColumns().Select(col => $"EF.Functions.Like(item.{col.NameAsString}, like)").Join($"{Environment.NewLine}                            || ")}});
                            }

                            query = query
                                .OrderBy(item => item.{{EnumerateListByKeywordTargetColumns().Where(col => col.IsInstanceKey).First().Name}})
                                .Take({{LIST_BY_KEYWORD_MAX + 1}});

                            return query
                                .AsEnumerable()
                                .Select(item => new {{AggregateInstanceKeyNamePair.CLASSNAME}} {
                                    {{AggregateInstanceKeyNamePair.KEY}} = {{InstanceKey.CLASS_NAME}}.{{InstanceKey.CREATE}}(new object?[] {
                {{keyColumns.SelectTextTemplate(col => $$"""
                                        item.{{col.Name}},
                """)}}
                                    }).ToString(),
                                    {{AggregateInstanceKeyNamePair.NAME}} = {{(nameColumns.Any()
                                        ? nameColumns.Select(col => $"item.{col.Name}?.ToString()").Join(" + ")
                                        : keyColumns.Select(col => $"item.{col.Name}?.ToString()").Join(" + "))}},
                                });
                        }
                    }
                }
                #endregion キーワード検索


                #region 詳細検索
                {{find.RenderController()}}
                {{find.RenderEFCoreFindMethod()}}
                #endregion 詳細検索


                #region 更新
                namespace {{_ctx.Config.RootNamespace}} {
                    using Microsoft.AspNetCore.Mvc;
                    using {{_ctx.Config.EntityNamespace}};

                    partial class {{_controller.ClassName}} {
                        [HttpPost("{{WebClient.Controller.UPDATE_ACTION_NAME}}")]
                        public virtual IActionResult Update({{_aggregateInstance.Item.ClassName}} param) {
                            if (_dbContext.{{_update.MethodName}}(param, out var updated, out var errors)) {
                                return this.JsonContent(updated);
                            } else {
                                return BadRequest(this.JsonContent(errors));
                            }
                        }
                    }
                }
                namespace {{_ctx.Config.EntityNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Infrastructure;

                    partial class {{_ctx.Config.DbContextName}} {
                        public bool {{_update.MethodName}}({{_aggregateInstance.Item.ClassName}} after, out {{_aggregateInstance.Item.ClassName}} updated, out ICollection<string> errors) {
                            errors = new List<string>();
                            var key = after.{{GETINSTANCEKEY_METHOD_NAME}}().ToString();

                            {{WithIndent(find.RenderDbEntityLoading("this", "beforeDbEntity", "key", tracks: false, includeRefs: false), "            ")}}

                            if (beforeDbEntity == null) {
                                updated = new {{_aggregateInstance.Item.ClassName}}();
                                errors.Add("更新対象のデータが見つかりません。");
                                return false;
                            }

                            var afterDbEntity = after.{{AggregateInstance.TO_DB_ENTITY_METHOD_NAME}}();

                            // Attach
                            this.Entry(afterDbEntity).State = EntityState.Modified;

                            {{WithIndent(_update.RenderDescendantsAttaching("this", "beforeDbEntity", "afterDbEntity"), "            ")}}

                            try {
                                this.SaveChanges();
                            } catch (DbUpdateException ex) {
                                updated = new {{_aggregateInstance.Item.ClassName}}();
                                foreach (var msg in ex.GetMessagesRecursively()) errors.Add(msg);
                                return false;
                            }

                            var afterUpdate = this.{{find.FindMethodName}}(key);
                            if (afterUpdate == null) {
                                updated = new {{_aggregateInstance.Item.ClassName}}();
                                errors.Add("更新後のデータの再読み込みに失敗しました。");
                                return false;
                            }
                            updated = afterUpdate;
                            return true;
                        }
                    }
                }
                #endregion 更新


                #region 削除
                namespace {{_ctx.Config.RootNamespace}} {
                    using Microsoft.AspNetCore.Mvc;
                    using {{_ctx.Config.EntityNamespace}};

                    partial class {{_controller.ClassName}} {
                        [HttpDelete("{{WebClient.Controller.DELETE_ACTION_NAME}}/{key}")]
                        public virtual IActionResult Delete(string key) {
                            if (_dbContext.{{_delete.MethodName}}(key, out var errors)) {
                                return Ok();
                            } else {
                                return BadRequest(this.JsonContent(errors));
                            }
                        }
                    }
                }
                namespace {{_ctx.Config.EntityNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Infrastructure;

                    partial class {{_ctx.Config.DbContextName}} {
                        public bool {{_delete.MethodName}}(string key, out ICollection<string> errors) {

                            {{WithIndent(find.RenderDbEntityLoading("this", "entity", "key", tracks: true, includeRefs: false), "            ")}}

                            if (entity == null) {
                                errors = new[] { "削除対象のデータが見つかりません。" };
                                return false;
                            }

                            this.Remove(entity);
                            try {
                                this.SaveChanges();
                            } catch (DbUpdateException ex) {
                                errors = ex.GetMessagesRecursively().ToArray();
                                return false;
                            }

                            errors = Array.Empty<string>();
                            return true;
                        }
                    }
                }
                #endregion 削除


                #region データ構造
                namespace {{_ctx.Config.RootNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;

                    /// <summary>
                    /// {{_aggregateInstance.GetCorrespondingAggregate().Item.DisplayName}}のデータ作成コマンドです。
                    /// </summary>
                    public partial class {{CreateCommandClassName}} {
                {{_aggregateInstance.GetProperties(_ctx.Config).SelectTextTemplate(prop => $$"""
                        public {{prop.CSharpTypeName}} {{prop.PropertyName}} { get; set; }
                """)}}

                        /// <summary>
                        /// {{_aggregate.Item.DisplayName}}のデータ作成コマンドをデータベースに保存する形に変換します。
                        /// </summary>
                        public {{_ctx.Config.EntityNamespace}}.{{_dbEntity.Item.ClassName}} {{CreateCommandToDbEntityMethodName}}() {
                            {{WithIndent(ToDbEntity(), "            ")}}
                        }
                        /// <summary>
                        /// 主キーを返します。
                        /// </summary>
                        public {{InstanceKey.CLASS_NAME}} {{CreateCommandGetInstanceKeyMethodName}}() {
                            return {{InstanceKey.CLASS_NAME}}.{{InstanceKey.CREATE}}(new object[] {
                {{_aggregateInstance.GetSchalarProperties().Where(p => p.CorrespondingDbColumn.IsPrimary).SelectTextTemplate(p => $$"""
                                this.{{p.PropertyName}},
                """)}}
                            });
                        }
                    }

                    /// <summary>
                    /// {{_aggregateInstance.GetCorrespondingAggregate().Item.DisplayName}}のデータ1件の詳細を表すクラスです。
                    /// </summary>
                    public partial class {{_aggregateInstance.Item.ClassName}} : {{AggregateInstance.BASE_CLASS_NAME}} {
                {{_aggregateInstance.GetProperties(_ctx.Config).SelectTextTemplate(prop => $$"""
                        public {{prop.CSharpTypeName}} {{prop.PropertyName}} { get; set; }
                """)}}

                        {{WithIndent(fromDbEntity.Render(), "        ")}}

                        /// <summary>
                        /// {{_aggregate.Item.DisplayName}}のデータ1件の内容をデータベースに保存する形に変換します。
                        /// </summary>
                        public {{_ctx.Config.EntityNamespace}}.{{_dbEntity.Item.ClassName}} {{AggregateInstance.TO_DB_ENTITY_METHOD_NAME}}() {
                            {{WithIndent(ToDbEntity(), "            ")}}
                        }
                        /// <summary>
                        /// 主キーを返します。
                        /// </summary>
                        public {{InstanceKey.CLASS_NAME}} {{GETINSTANCEKEY_METHOD_NAME}}() {
                            return {{InstanceKey.CLASS_NAME}}.{{InstanceKey.CREATE}}(new object[] {
                {{_aggregateInstance.GetSchalarProperties().Where(p => p.CorrespondingDbColumn.IsPrimary).SelectTextTemplate(p => $$"""
                                this.{{p.PropertyName}},
                """)}}
                            });
                        }
                        public string {{GETINSTANCENAME_METHOD_NAME}}() {
                {{GetInstanceNameProps().SelectTextTemplate(line => $$"""
                            {{line}}
                """)}}
                        }
                        public {{AggregateInstanceKeyNamePair.CLASSNAME}} {{TOKEYNAMEPAIR_METHOD_NAME}}() {
                            return new {{AggregateInstanceKeyNamePair.CLASSNAME}} {
                                {{AggregateInstanceKeyNamePair.KEY}} = this.{{GETINSTANCEKEY_METHOD_NAME}}().ToString(),
                                {{AggregateInstanceKeyNamePair.NAME}} = this.{{GETINSTANCENAME_METHOD_NAME}}(),
                            };
                        }
                    }

                {{_aggregateInstance.EnumerateDescendants().SelectTextTemplate(ins => $$"""
                    /// <summary>
                    /// {{ins.GetCorrespondingAggregate().Item.DisplayName}}のデータ1件の詳細を表すクラスです。
                    /// </summary>
                    public partial class {{ins.Item.ClassName}} {
                {{ins.GetProperties(_ctx.Config).SelectTextTemplate(prop => $$"""
                        public {{prop.CSharpTypeName}} {{prop.PropertyName}} { get; set; }
                """)}}
                    }
                """)}}
                }

                {{search.RenderCSharpClassDef()}}

                namespace {{_ctx.Config.EntityNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Infrastructure;

                {{_dbEntity.EnumerateThisAndDescendants().SelectTextTemplate(ett => $$"""
                    /// <summary>
                    /// {{ett.GetCorrespondingAggregate()?.Item.DisplayName}}のデータベースに保存されるデータの形を表すクラスです。
                    /// </summary>
                    public partial class {{ett.Item.ClassName}} {
                {{ett.GetColumns().SelectTextTemplate(col => $$"""
                        public {{col.MemberType.GetCSharpTypeName()}} {{col.PropertyName}} { get; set; }
                """)}}

                {{EnumerateNavigationProperties(ett).SelectTextTemplate(nav => $$"""
                        public virtual {{nav.CSharpTypeName}} {{nav.PropertyName}} { get; set; }
                """)}}

                        /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
                        public bool {{EFCoreEntity.KEYEQUALS}}({{ett.Item.ClassName}} entity) {
                {{ett.GetColumns().Where(c => c.IsPrimary).SelectTextTemplate(col => $$"""
                            if (entity.{{col.PropertyName}} != this.{{col.PropertyName}}) return false;
                """)}}
                            return true;
                        }
                    }
                """)}}

                    partial class {{_ctx.Config.DbContextName}} {
                {{_dbEntity.EnumerateThisAndDescendants().SelectTextTemplate(ett => $$"""
                        public DbSet<{{_ctx.Config.EntityNamespace}}.{{ett.Item.ClassName}}> {{ett.Item.DbSetName}} { get; set; }
                """)}}
                    }
                }
                #endregion データ構造
                """;
        }
    }
}
