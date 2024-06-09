using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    internal class VariationSwitch : IAggregateMemberType {
        internal VariationSwitch(VariationGroup<Aggregate> variationGroup) {
            _variationGroup = variationGroup;
        }
        private readonly VariationGroup<Aggregate> _variationGroup;

        public SearchBehavior SearchBehavior => SearchBehavior.Strict;

        private string CsEnumTypeName => _variationGroup.CsEnumType;
        public string GetCSharpTypeName() => CsEnumTypeName;

        public string GetTypeScriptTypeName() {
            return _variationGroup
                .VariationAggregates
                .Select(kv => $"'{kv.Value.RelationName}'")
                .Join(" | ");
        }

        public ReactInputComponent GetReactComponent() {
            var props = new Dictionary<string, string> {
                { "options", $"[{_variationGroup.VariationAggregates.Select(kv => $"'{kv.Value.RelationName}' as const").Join(", ")}]" },
                { "textSelector", "item => item" },
            };

            return new ReactInputComponent {
                Name = "Input.Selection",
                Props = props,
            };
        }

        public IGridColumnSetting GetGridColumnEditSetting() {
            return new ComboboxColumnSetting {
                OptionItemTypeName = _variationGroup.VariationAggregates.Select(kv => $"'{kv.Value.RelationName}'").Join(" | "),
                Options = $"[{_variationGroup.VariationAggregates.Select(kv => $"'{kv.Value.RelationName}' as const").Join(", ")}]",
                EmitValueSelector = $"opt => opt",
                MatchingKeySelectorFromEmitValue = $"value => value",
                MatchingKeySelectorFromOption = $"opt => opt",
                TextSelector = $"opt => opt",
            };
        }
    }
}
