// ------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン: 17.0.0.0
//  
//     このファイルへの変更は、正しくない動作の原因になる可能性があり、
//     コードが再生成されると失われます。
// </auto-generated>
// ------------------------------------------------------------------------------
namespace HalApplicationBuilder.CodeRendering.ReactAndWebApi
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using HalApplicationBuilder;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class WebApiControllerTemplate : WebApiControllerTemplateBase
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
 var dbContextTypeName = $"{_config.DbContextNamespace}.{_config.DbContextName}"; 
            this.Write("using Microsoft.AspNetCore.Mvc;\r\nusing System.Text.Json;\r\n\r\nnamespace ");
            this.Write(this.ToStringHelper.ToStringWithCulture(_config.MvcControllerNamespace));
            this.Write(";\r\n\r\n");
 foreach (var rootAggregate in _rootAggregates) { 
 var controllerName = GetControllerName(rootAggregate); 
 var uiInstance = rootAggregate.ToUiInstanceClass().CSharpTypeName; 
 var search = rootAggregate.BuildSearchMethod("param", "query", "e"); 
            this.Write("\r\n[ApiController]\r\n[Route(\"[controller]\")]\r\npublic class ");
            this.Write(this.ToStringHelper.ToStringWithCulture(controllerName));
            this.Write(" : ControllerBase {\r\n    public ");
            this.Write(this.ToStringHelper.ToStringWithCulture(controllerName));
            this.Write("(\r\n        ILogger<");
            this.Write(this.ToStringHelper.ToStringWithCulture(controllerName));
            this.Write("> logger,\r\n        ");
            this.Write(this.ToStringHelper.ToStringWithCulture(dbContextTypeName));
            this.Write(" dbContext,\r\n        ");
            this.Write(this.ToStringHelper.ToStringWithCulture(typeof(RuntimeService).FullName));
            this.Write(" runtimeService) {\r\n        _logger = logger;\r\n        _dbContext = dbContext;\r\n " +
                    "       _runtimeService = runtimeService;\r\n    }\r\n    private readonly ILogger<");
            this.Write(this.ToStringHelper.ToStringWithCulture(controllerName));
            this.Write("> _logger;\r\n    private readonly ");
            this.Write(this.ToStringHelper.ToStringWithCulture(dbContextTypeName));
            this.Write(" _dbContext;\r\n    private readonly ");
            this.Write(this.ToStringHelper.ToStringWithCulture(typeof(RuntimeService).FullName));
            this.Write(" _runtimeService;\r\n\r\n    [HttpGet(\"list\")]\r\n    public IActionResult Search([From" +
                    "Query] string param) {\r\n        var json = System.Web.HttpUtility.UrlDecode(para" +
                    "m);\r\n        var condition = string.IsNullOrWhiteSpace(json)\r\n            ? new " +
                    "");
            this.Write(this.ToStringHelper.ToStringWithCulture(search.SearchConditionClassName));
            this.Write("()\r\n            : System.Text.Json.JsonSerializer.Deserialize<");
            this.Write(this.ToStringHelper.ToStringWithCulture(search.SearchConditionClassName));
            this.Write(">(json)!;\r\n        var searchResult = _dbContext\r\n            .");
            this.Write(this.ToStringHelper.ToStringWithCulture(search.MethodName));
            this.Write("(condition)\r\n            .AsEnumerable();\r\n        return JsonContent(searchResul" +
                    "t);\r\n    }\r\n    [HttpPost(\"create\")]\r\n    public IActionResult Create(");
            this.Write(this.ToStringHelper.ToStringWithCulture(uiInstance));
            this.Write(" param) {\r\n        var success = _runtimeService.");
            this.Write(this.ToStringHelper.ToStringWithCulture(nameof(RuntimeService.TrySaveNewInstance)));
            this.Write(@"(param, out var instanceKey, out var errors);
        if (success) {
            return Ok(new { instanceKey });
        } else {
            return BadRequest(errors);
        }
    }
    [HttpGet(""detail/{instanceKey}"")]
    public IActionResult Find(string instanceKey) {
        var instance = _runtimeService.");
            this.Write(this.ToStringHelper.ToStringWithCulture(nameof(RuntimeService.FindInstance)));
            this.Write("<");
            this.Write(this.ToStringHelper.ToStringWithCulture(uiInstance));
            this.Write(">(instanceKey, out var _);\r\n        if (instance == null) {\r\n            return N" +
                    "otFound();\r\n        } else {\r\n            return JsonContent(instance);\r\n       " +
                    " }\r\n    }\r\n    [HttpPost(\"update\")]\r\n    public IActionResult Update(");
            this.Write(this.ToStringHelper.ToStringWithCulture(uiInstance));
            this.Write(" param) {\r\n        var success = _runtimeService.");
            this.Write(this.ToStringHelper.ToStringWithCulture(nameof(RuntimeService.TryUpdate)));
            this.Write(@"(param, out var instanceKey, out var errors);
        if (success) {
            return Ok(new { instanceKey });
        } else {
            return BadRequest(errors);
        }
    }

    private ContentResult JsonContent<T>(T obj) {
        var options = new JsonSerializerOptions {
            // レスポンスに大文字が含まれるとき、大文字のまま返す。
            // react hook form や ag-grid では大文字小文字を区別しているため
            PropertyNameCaseInsensitive = true,
        };
        var json = JsonSerializer.Serialize(obj, options);

        return Content(json, ""application/json"");
    }
}

");
 } 
            return this.GenerationEnvironment.ToString();
        }
    }
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public class WebApiControllerTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
