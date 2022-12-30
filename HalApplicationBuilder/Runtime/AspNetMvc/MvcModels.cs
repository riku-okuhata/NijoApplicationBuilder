﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HalApplicationBuilder.Runtime.AspNetMvc {
    public class MvcModels {
        internal Core.Config Config { get; init; }
        internal Core.Builder AggregateBuilder { get; init; }

        internal string TransformText() {
            var aggregates = AggregateBuilder.EnumerateAllAggregates();
            var template = new MvcModelsTemplate {
                Namespace = Config.MvcModelNamespace,
                SearchConditionClasses = aggregates.Select(a => a.ToSearchConditionModel(new Core.ViewRenderingContext())),
                SearchResultClasses = aggregates.Select(a => a.ToSearchResultModel(new Core.ViewRenderingContext())),
                InstanceClasses = aggregates.Select(a => a.ToInstanceModel(new Core.ViewRenderingContext())),
            };
            return template.TransformText();
        }
    }

    partial class MvcModelsTemplate {
        internal string Namespace { get; set; }
        internal IEnumerable<Core.AutoGenerateMvcModelClass> SearchConditionClasses { get; set; }
        internal IEnumerable<Core.AutoGenerateMvcModelClass> SearchResultClasses { get; set; }
        internal IEnumerable<Core.AutoGenerateMvcModelClass> InstanceClasses { get; set; }
    }
}
