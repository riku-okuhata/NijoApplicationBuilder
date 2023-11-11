using HalApplicationBuilder.Core;
using HalApplicationBuilder.DotnetEx;
using static HalApplicationBuilder.CodeRendering.TemplateTextHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalApplicationBuilder.CodeRendering.InstanceHandling {
    internal class UpdateFeature {
        internal UpdateFeature(GraphNode<Aggregate> aggregate) {
            _aggregate = aggregate;
        }

        private readonly GraphNode<Aggregate> _aggregate;

        internal string ArgType => _aggregate.Item.ClassName;
        internal string MethodName => $"Update{_aggregate.Item.DisplayName.ToCSharpSafe()}";

        internal string RenderController(CodeRenderingContext ctx) {
            var controller = new WebClient.Controller(_aggregate.Item);

            return $$"""
                namespace {{ctx.Config.RootNamespace}} {
                    using Microsoft.AspNetCore.Mvc;
                    using {{ctx.Config.EntityNamespace}};

                    partial class {{controller.ClassName}} {
                        [HttpPost("{{WebClient.Controller.UPDATE_ACTION_NAME}}")]
                        public virtual IActionResult Update({{_aggregate.Item.ClassName}} param) {
                            if (_dbContext.{{MethodName}}(param, out var updated, out var errors)) {
                                return this.JsonContent(updated);
                            } else {
                                return BadRequest(this.JsonContent(errors));
                            }
                        }
                    }
                }
                """;
        }

        internal string RenderEFCoreMethod(CodeRenderingContext ctx) {
            var controller = new WebClient.Controller(_aggregate.Item);
            var find = new FindFeature(_aggregate);

            var detail = new AggregateDetail(_aggregate);
            var searchKeys = detail
                .GetKeyMembers()
                .Where(m => m is not AggregateMember.KeyOfRefTarget)
                .Select(m => "after." + m.GetFullPath().Join("."))
                .ToArray();

            return $$"""
                namespace {{ctx.Config.EntityNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Infrastructure;

                    partial class {{ctx.Config.DbContextName}} {
                        public bool {{MethodName}}({{detail.ClassName}} after, out {{detail.ClassName}} updated, out ICollection<string> errors) {
                            errors = new List<string>();

                            {{WithIndent(find.RenderDbEntityLoading("this", "beforeDbEntity", searchKeys, tracks: false, includeRefs: false), "            ")}}

                            if (beforeDbEntity == null) {
                                updated = new {{_aggregate.Item.ClassName}}();
                                errors.Add("更新対象のデータが見つかりません。");
                                return false;
                            }

                            var afterDbEntity = after.{{AggregateDetail.TO_DBENTITY}}();

                            // Attach
                            this.Entry(afterDbEntity).State = EntityState.Modified;

                            {{WithIndent(RenderDescendantsAttaching("this", "beforeDbEntity", "afterDbEntity"), "            ")}}

                            try {
                                this.SaveChanges();
                            } catch (DbUpdateException ex) {
                                updated = new {{_aggregate.Item.ClassName}}();
                                foreach (var msg in ex.GetMessagesRecursively()) errors.Add(msg);
                                return false;
                            }

                            var afterUpdate = this.{{find.RenderCaller(m => $"afterDbEntity.{m.GetFullPath().Join(".")}")}};
                            if (afterUpdate == null) {
                                updated = new {{_aggregate.Item.ClassName}}();
                                errors.Add("更新後のデータの再読み込みに失敗しました。");
                                return false;
                            }
                            updated = afterUpdate;
                            return true;
                        }
                    }
                }
                """;
        }

        private string RenderDescendantsAttaching(string dbContext, string before, string after) {
            var builder = new StringBuilder();

            var descendantDbEntities = _aggregate.EnumerateDescendants().ToArray();
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
                        builder.AppendLine($"var arr{i}_{(renderBefore ? "before" : "after")} = new {descendantDbEntities[i].Item.EFCoreEntityClassName}[] {{");
                        builder.AppendLine($"    {(renderBefore ? before : after)}.{paths.Select(p => p.RelationName).Join(".")},");
                        builder.AppendLine($"}};");
                    }
                }
                RenderEntityArray(true);
                RenderEntityArray(false);

                // ChangeState変更
                builder.AppendLine($"foreach (var a in arr{i}_after) {{");
                builder.AppendLine($"    var b = arr{i}_before.SingleOrDefault(b => b.{IEFCoreEntity.KEYEQUALS}(a));");
                builder.AppendLine($"    if (b == null) {{");
                builder.AppendLine($"        {dbContext}.Entry(a).State = EntityState.Added;");
                builder.AppendLine($"    }} else {{");
                builder.AppendLine($"        {dbContext}.Entry(a).State = EntityState.Modified;");
                builder.AppendLine($"    }}");
                builder.AppendLine($"}}");

                builder.AppendLine($"foreach (var b in arr{i}_before) {{");
                builder.AppendLine($"    var a = arr{i}_after.SingleOrDefault(a => a.{IEFCoreEntity.KEYEQUALS}(b));");
                builder.AppendLine($"    if (a == null) {{");
                builder.AppendLine($"        {dbContext}.Entry(b).State = EntityState.Deleted;");
                builder.AppendLine($"    }}");
                builder.AppendLine($"}}");
            }

            return builder.ToString();
        }
    }
}
