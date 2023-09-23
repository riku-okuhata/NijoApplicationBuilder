using HalApplicationBuilder.Core;
using HalApplicationBuilder.DotnetEx;
using static HalApplicationBuilder.CodeRendering.TemplateTextHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HalApplicationBuilder.CodeRendering {
    internal class AggregateKey {
        internal AggregateKey(GraphNode<Aggregate> aggregate) {
            _aggregate = aggregate;
        }
        private readonly GraphNode<Aggregate> _aggregate;

        internal string CSharpClassName => $"{_aggregate.Item.ClassName}Key";
        internal string TypeScriptTypeName => $"{_aggregate.Item.ClassName}Key";

        internal IEnumerable<AggregateMember.ValueMember> GetMembers() {
            return _aggregate.GetKeyMembers();

            //foreach (var member in _aggregate.GetMembers()) {
            //    if (member is AggregateMember.ValueMember vMember) {
            //        if (!vMember.IsKey) continue;
            //        if (member is AggregateMember.KeyOfRefTarget) continue; // Refは参照先の各メンバーではなく参照先のキークラスをもつので除外

            //        yield return member;

            //    } else if (member is AggregateMember.Ref refMember) {
            //        if (!refMember.Relation.IsPrimary()) continue;

            //        yield return member;
            //    }
            //}
        }

        internal string RenderCSharpDeclaring() {
            return $$"""
                public class {{CSharpClassName}} {
                {{GetMembers().SelectTextTemplate(member => $$"""
                    public {{member.CSharpTypeName}} {{member.MemberName}} { get; set; }
                """)}}
                }
                """;
        }
        internal string RenderTypeScriptDeclaring() {
            return $$"""
                export type {{TypeScriptTypeName}} = {
                {{GetMembers().SelectTextTemplate(member => $$"""
                  {{member.MemberName}}: {{member.CSharpTypeName}}
                """)}}
                }
                """;
        }
    }
}
