﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン: 17.0.0.0
//  
//     このファイルへの変更は、正しくない動作の原因になる可能性があり、
//     コードが再生成されると失われます。
// </auto-generated>
// ------------------------------------------------------------------------------
namespace HalApplicationBuilder.CodeRendering.WebClient
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using HalApplicationBuilder.CodeRendering.Util;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class ComboBox : ComboBoxBase
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write(@"import React, { forwardRef, ForwardedRef, useState, useCallback } from ""react""
import { useQuery } from ""react-query""
import { useFormContext } from 'react-hook-form';
import { Combobox } from ""@headlessui/react""
import { ChevronUpDownIcon } from ""@heroicons/react/24/outline""
import { NowLoading } from ""./NowLoading""
import { useAppContext } from ""../hooks/AppContext""
import { usePageContext } from ""../hooks/PageContext""
import { useHttpRequest } from ""../hooks/useHttpRequest""

export const ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ComponentName));
            this.Write(@" = forwardRef(({ raectHookFormId }: {
  raectHookFormId: string
}, ref: ForwardedRef<HTMLElement>) => {

  const [{ pageIsReadOnly },] = usePageContext()

  const [keyword, setKeyword] = useState('')
  const { get } = useHttpRequest()
  const [, dispatch] = useAppContext()
  const { data, refetch, isFetching } = useQuery({
    queryKey: ['");
            this.Write(this.ToStringHelper.ToStringWithCulture(UseQueryKey));
            this.Write("\'],\r\n    queryFn: async () => {\r\n      const response = await get<");
            this.Write(this.ToStringHelper.ToStringWithCulture(AggregateInstanceKeyNamePair.TS_DEF));
            this.Write("[]>(`");
            this.Write(this.ToStringHelper.ToStringWithCulture(Api));
            this.Write(@"`, { keyword })
      return response.ok ? response.data : []
    },
    onError: error => {
      dispatch({ type: 'pushMsg', msg: `ERROR!: ${JSON.stringify(error)}` })
    },
  })

  const [setTimeoutHandle, setSetTimeoutHandle] = useState<NodeJS.Timeout | undefined>(undefined)
  const onChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setKeyword(e.target.value)
    if (setTimeoutHandle !== undefined) clearTimeout(setTimeoutHandle)
    setSetTimeoutHandle(setTimeout(() => {
      refetch()
      setSetTimeoutHandle(undefined)
    }, 300))
  }, [setKeyword, setTimeoutHandle, setSetTimeoutHandle, refetch])
  const onBlur = useCallback(() => {
    if (setTimeoutHandle !== undefined) clearTimeout(setTimeoutHandle)
    setSetTimeoutHandle(undefined)
    refetch()
  }, [setTimeoutHandle, setSetTimeoutHandle, refetch])

  const { watch, setValue } = useFormContext()
  const onChangeSelectedValue = useCallback((value?: ");
            this.Write(this.ToStringHelper.ToStringWithCulture(AggregateInstanceKeyNamePair.TS_DEF));
            this.Write(") => {\r\n    setValue(raectHookFormId, value)\r\n  }, [setValue, watch])\r\n  const di" +
                    "splayValue = useCallback((item?: ");
            this.Write(this.ToStringHelper.ToStringWithCulture(AggregateInstanceKeyNamePair.TS_DEF));
            this.Write(") => {\r\n    return item?.name || \'\'\r\n  }, [])\r\n\r\n  return (\r\n    <Combobox ref={r" +
                    "ef} value={watch(raectHookFormId) || null} onChange={onChangeSelectedValue} null" +
                    "able disabled={pageIsReadOnly}>\r\n      <div className=\"relative ");
            this.Write(this.ToStringHelper.ToStringWithCulture(FormOfAggregateInstance.INPUT_WIDTH));
            this.Write(@""">
        <Combobox.Input displayValue={displayValue} onChange={onChange} onBlur={onBlur} className=""w-full"" spellCheck=""false"" autoComplete=""off"" />
        {!pageIsReadOnly &&
          <Combobox.Button className=""absolute inset-y-0 right-0 flex items-center pr-2"">
            <ChevronUpDownIcon className=""h-5 w-5 text-gray-400"" aria-hidden=""true"" />
          </Combobox.Button>}
        <Combobox.Options className=""absolute mt-1 w-full overflow-auto bg-white py-1 shadow-lg focus:outline-none"">
          {(setTimeoutHandle !== undefined || isFetching) &&
            <NowLoading />}
          {(setTimeoutHandle === undefined && !isFetching && data?.length === 0) &&
            <span className=""p-1 text-sm select-none opacity-50"">データなし</span>}
          {(setTimeoutHandle === undefined && !isFetching) && data?.map(item => (
            <Combobox.Option key={item.");
            this.Write(this.ToStringHelper.ToStringWithCulture(AggregateInstanceKeyNamePair.JSON_KEY));
            this.Write("} value={item}>\r\n              {({ active }) => (\r\n                <div className" +
                    "={active ? \'bg-neutral-200\' : \'\'}>\r\n                  {item.");
            this.Write(this.ToStringHelper.ToStringWithCulture(AggregateInstanceKeyNamePair.JSON_NAME));
            this.Write("}\r\n                </div>\r\n              )}\r\n            </Combobox.Option>\r\n    " +
                    "      ))}\r\n        </Combobox.Options>\r\n      </div>\r\n    </Combobox>\r\n  )\r\n})\r\n" +
                    "");
            return this.GenerationEnvironment.ToString();
        }
    }
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public class ComboBoxBase
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
        public System.Text.StringBuilder GenerationEnvironment
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
