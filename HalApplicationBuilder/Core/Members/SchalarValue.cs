﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using HalApplicationBuilder.Runtime;

namespace HalApplicationBuilder.Core.Members {
    internal class SchalarValue : AggregateMemberBase {
        public override bool IsCollection => false;

        public override IEnumerable<Aggregate> GetChildAggregates() {
            yield break;
        }

        private bool IsNullable() {
            return UnderlyingPropertyInfo.PropertyType.IsGenericType
                && UnderlyingPropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        private Type GetPropertyTypeExceptNullable() {
            return IsNullable()
                ? UnderlyingPropertyInfo.PropertyType.GetGenericArguments()[0]
                : UnderlyingPropertyInfo.PropertyType;
        }
        private string GetCSharpTypeName() {
            var type = GetPropertyTypeExceptNullable();
            string valueTypeName = null;
            if (type.IsEnum) valueTypeName = type.FullName;
            else if (type == typeof(string)) valueTypeName = "string";
            else if (type == typeof(bool)) valueTypeName = "bool";
            else if (type == typeof(int)) valueTypeName = "int";
            else if (type == typeof(float)) valueTypeName = "float";
            else if (type == typeof(decimal)) valueTypeName = "decimal";
            else if (type == typeof(DateTime)) valueTypeName = "DateTime";

            var question = IsNullable() ? "?" : null;

            return valueTypeName + question;
        }

        internal override IEnumerable<AutoGenerateDbEntityProperty> ToDbColumnModel() {
            yield return new AutoGenerateDbEntityProperty {
                Virtual = false,
                CSharpTypeName = GetCSharpTypeName(),
                PropertyName = UnderlyingPropertyInfo.GetCustomAttribute<ColumnAttribute>()?.Name
                    ?? UnderlyingPropertyInfo.Name,
                Initializer = null,
            };
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToSearchConditionModel(ViewRenderingContext context) {
            var propName = UnderlyingPropertyInfo.Name;
            var nestedContext = context.Nest(propName, isCollection: false);
            var type = GetPropertyTypeExceptNullable();
            if (new[] { typeof(int), typeof(float), typeof(decimal), typeof(DateTime) }.Contains(type)) {
                // 範囲検索
                var template = new SchalarValueSearchCondition {
                    Type = SchalarValueSearchCondition.E_Type.Range,
                    AspFor = new[] {
                        $"{nestedContext.AspForPath}.{nameof(FromTo<object>.From)}",
                        $"{nestedContext.AspForPath}.{nameof(FromTo<object>.To)}",
                    },
                };
                yield return new AutoGenerateMvcModelProperty {
                    Virtual = false,
                    CSharpTypeName = $"{typeof(FromTo<>).Namespace}.{nameof(FromTo<object>)}<{type.FullName}>",
                    PropertyName = propName,
                    View = template.TransformText(),
                };

            } else if (type.IsEnum) {
                // enumドロップダウン
                var template = new SchalarValueSearchCondition {
                    Type = SchalarValueSearchCondition.E_Type.Select,
                    AspFor = new[] { nestedContext.AspForPath },
                    EnumTypeName = type.FullName,
                    Options = IsNullable()
                        ? new[] { KeyValuePair.Create("", "") }
                        : Array.Empty<KeyValuePair<string, string>>(),
                };
                yield return new AutoGenerateMvcModelProperty {
                    Virtual = false,
                    CSharpTypeName = type.FullName,
                    PropertyName = propName,
                    View = template.TransformText(),
                };

            } else {
                // ただのinput
                var template = new SchalarValueSearchCondition {
                    Type = SchalarValueSearchCondition.E_Type.Input,
                    AspFor = new[] { nestedContext.AspForPath },
                };
                yield return new AutoGenerateMvcModelProperty {
                    Virtual = false,
                    CSharpTypeName = GetCSharpTypeName(),
                    PropertyName = propName,
                    View = template.TransformText(),
                };
            }
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToSearchResultModel(ViewRenderingContext context) {
            var propertyName = UnderlyingPropertyInfo.Name;
            var nested = context.Nest(propertyName, isCollection: false);
            yield return new AutoGenerateMvcModelProperty {
                Virtual = false,
                CSharpTypeName = GetCSharpTypeName(),
                PropertyName = propertyName,
                View = $"<span>@{nested.Path}</span>",
            };
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToInstanceModel(ViewRenderingContext context) {
            var propertyName = UnderlyingPropertyInfo.Name;
            var nested = context.Nest(propertyName, isCollection: false);
            yield return new AutoGenerateMvcModelProperty {
                Virtual = false,
                CSharpTypeName = GetCSharpTypeName(),
                PropertyName = propertyName,
                View = $"<input asp-for=\"{nested.AspForPath}\"/>",
            };
        }


        public static bool IsPrimitive(Type type) {
            if (type == typeof(string)) return true;
            if (type == typeof(bool)) return true;
            if (type == typeof(bool?)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(int?)) return true;
            if (type == typeof(float)) return true;
            if (type == typeof(float?)) return true;
            if (type == typeof(decimal)) return true;
            if (type == typeof(decimal?)) return true;
            if (type == typeof(DateTime)) return true;
            if (type == typeof(DateTime?)) return true;
            if (type.IsEnum) return true;

            return false;
        }
    }

    partial class SchalarValueSearchCondition {
        public enum E_Type {
            Input,
            Range,
            Select,
        }
        public string[] AspFor { get; set; }
        public E_Type Type { get; set; }
        public string EnumTypeName { get; set; }
        public KeyValuePair<string, string>[] Options { get; set; }
    }
}
