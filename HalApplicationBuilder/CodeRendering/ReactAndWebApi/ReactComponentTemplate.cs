﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HalApplicationBuilder.CodeRendering.ReactAndWebApi {
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System;
    
    
    public partial class ReactComponentTemplate : ReactComponentTemplateBase {
        
        public virtual string TransformText() {
            this.GenerationEnvironment = null;
            this.Write(@"import React, { useState, useCallback } from 'react';
import { useCtrlS } from './hooks/useCtrlS';
import { useAppContext } from './hooks/AppContext';
import { AgGridReact } from 'ag-grid-react';
import { Link, useNavigate } from 'react-router-dom';
import { useQuery } from 'react-query';
import { BookmarkIcon, ChevronDownIcon, ChevronUpIcon, MagnifyingGlassIcon, PlusIcon, BookmarkSquareIcon } from '@heroicons/react/24/outline';
import { IconButton } from './components/IconButton';
import { ");
            this.Write(this.ToStringHelper.ToStringWithCulture(_searchCondition.ClassName));
            this.Write(", ");
            this.Write(this.ToStringHelper.ToStringWithCulture(_searchResult.ClassName));
            this.Write(", ");
            this.Write(this.ToStringHelper.ToStringWithCulture(_uiInstance.ClassName));
            this.Write(" } from \'");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetImportFromTypes()));
            this.Write("\';\r\n\r\nexport const ");
            this.Write(this.ToStringHelper.ToStringWithCulture(MultiViewComponentName));
            this.Write(@" = () => {

    const [{ apiDomain }, dispatch] = useAppContext()
    useCtrlS(() => {
        dispatch({ type: 'pushMsg', msg: '保存しました。' })
    })

    const [expanded, setExpanded] = useState(true)

    const [editedParam, setEditedParam] = useState<");
            this.Write(this.ToStringHelper.ToStringWithCulture(_searchCondition.ClassName));
            this.Write(">({} as ");
            this.Write(this.ToStringHelper.ToStringWithCulture(_searchCondition.ClassName));
            this.Write(") // TODO\r\n    const [commitedParam, setCommitedParam] = useState<");
            this.Write(this.ToStringHelper.ToStringWithCulture(_searchCondition.ClassName));
            this.Write(">({} as ");
            this.Write(this.ToStringHelper.ToStringWithCulture(_searchCondition.ClassName));
            this.Write(") // TODO\r\n    const { data, isLoading, error } = useQuery({\r\n        queryKey: [" +
                    "\'");
            this.Write(this.ToStringHelper.ToStringWithCulture(_rootAggregate.GetGuid()));
            this.Write("\', JSON.stringify(commitedParam)],\r\n        queryFn: async () => {\r\n            c" +
                    "onst queryParam = new URLSearchParams(commitedParam).toString()\r\n            con" +
                    "st response = await fetch(`${apiDomain}/");
            this.Write(this.ToStringHelper.ToStringWithCulture(_rootAggregate.GetCSharpSafeName()));
            this.Write("/list?${queryParam}`) // TODO バックエンドのURLと合わせる\r\n            if (!response.ok) thro" +
                    "w new Error(\'Network response was not OK.\')\r\n            return (await response." +
                    "json()) as ");
            this.Write(this.ToStringHelper.ToStringWithCulture(_searchResult.ClassName));
            this.Write("[]\r\n        },\r\n    })\r\n    const navigate = useNavigate()\r\n    const toCreateVie" +
                    "w = useCallback(() => {\r\n        navigate(\'");
            this.Write(this.ToStringHelper.ToStringWithCulture(CreateViewUrl));
            this.Write(@"')
    }, [])
    
    if (error) return <p>Error: {JSON.stringify(error)}</p>

    return (
        <div className=""ag-theme-alpine compact h-full w-full"">

            <div className=""flex flex-row justify-start items-center space-x-1"">
                <div className='flex flex-row items-center space-x-1 cursor-pointer' onClick={() => setExpanded(!expanded)}>
                    <h1 className=""text-base font-semibold select-none py-1"">
                        ");
            this.Write(this.ToStringHelper.ToStringWithCulture(_rootAggregate.GetDisplayName()));
            this.Write(@"
                    </h1>
                    {expanded
                        ? <ChevronDownIcon className=""w-4"" />
                        : <ChevronUpIcon className=""w-4"" />}
                </div>
                <div className='flex-1'></div>
                <IconButton icon={PlusIcon} onClick={toCreateView}>新規作成</IconButton>
            </div>

            {expanded &&
                <div className='flex flex-col space-y-1 py-1'>
");
 PushIndent("                    "); 
 _rootAggregate.RenderReactSearchCondition(new RenderingContext(this, new ObjectPath("searchCondition"))); 
 PopIndent(); 
            this.Write(@"                    <div className='flex flex-row justify-start space-x-1'>
                        <IconButton icon={MagnifyingGlassIcon}>検索</IconButton>
                        <IconButton outline>クリア</IconButton>
                        <div className=""flex-1""></div>
                        <IconButton outline icon={BookmarkIcon}>この検索条件を保存</IconButton>
                    </div>
                </div>
            }

            <AgGridReact
                rowData={isLoading ? [] : data}
                columnDefs={columnDefs}
                multiSortKey='ctrl'
                undoRedoCellEditing
                undoRedoCellEditingLimit={20}>
            </AgGridReact>
        </div>
    )
}

const columnDefs = [
    {
        resizable: true,
        cellRenderer: ({ data }: any) => {
            // console.log(data)
            return <Link to=""/"" className=""text-blue-400"">詳細</Link>
        },
    },
");
 foreach (var prop in _searchResult.Properties) { 
            this.Write("    { field: \'");
            this.Write(this.ToStringHelper.ToStringWithCulture(prop.PropertyName));
            this.Write("\', resizable: true, sortable: true, editable: true },\r\n");
 } 
            this.Write("]\r\n\r\nexport const ");
            this.Write(this.ToStringHelper.ToStringWithCulture(CreateViewComponentName));
            this.Write(" = () => {\r\n\r\n    return (\r\n        <div className=\"flex flex-col justify-start s" +
                    "pace-y-1\">\r\n            <h1 className=\"text-base font-semibold select-none py-1\"" +
                    ">\r\n                <Link to=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(MultiViewUrl));
            this.Write("\">");
            this.Write(this.ToStringHelper.ToStringWithCulture(_rootAggregate.GetDisplayName()));
            this.Write("</Link> &#047; 新規作成\r\n            </h1>\r\n            <div className=\"flex-1 flex f" +
                    "lex-col space-y-1\">\r\n");
 PushIndent("                "); 
 _rootAggregate.RenderReactSearchCondition(new RenderingContext(this, new ObjectPath("instance"))); 
 PopIndent(); 
            this.Write("            </div>\r\n            <IconButton icon={BookmarkSquareIcon} className=\"" +
                    "self-start\">保存</IconButton>\r\n        </div>\r\n    )\r\n}\r\n\r\nexport const ");
            this.Write(this.ToStringHelper.ToStringWithCulture(SingleViewComponentName));
            this.Write(" = () => {\r\n\r\n    return (\r\n        <div className=\"flex flex-col justify-start s" +
                    "pace-y-1\">\r\n");
 PushIndent("            "); 
 _rootAggregate.RenderReactSearchCondition(new RenderingContext(this, new ObjectPath("instance"))); 
 PopIndent(); 
            this.Write("        </div>\r\n    )\r\n}\r\n");
            return this.GenerationEnvironment.ToString();
        }
        
        public virtual void Initialize() {
        }
    }
    
    public class ReactComponentTemplateBase {
        
        private global::System.Text.StringBuilder builder;
        
        private global::System.Collections.Generic.IDictionary<string, object> session;
        
        private global::System.CodeDom.Compiler.CompilerErrorCollection errors;
        
        private string currentIndent = string.Empty;
        
        private global::System.Collections.Generic.Stack<int> indents;
        
        private ToStringInstanceHelper _toStringHelper = new ToStringInstanceHelper();
        
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session {
            get {
                return this.session;
            }
            set {
                this.session = value;
            }
        }
        
        public global::System.Text.StringBuilder GenerationEnvironment {
            get {
                if ((this.builder == null)) {
                    this.builder = new global::System.Text.StringBuilder();
                }
                return this.builder;
            }
            set {
                this.builder = value;
            }
        }
        
        protected global::System.CodeDom.Compiler.CompilerErrorCollection Errors {
            get {
                if ((this.errors == null)) {
                    this.errors = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errors;
            }
        }
        
        public string CurrentIndent {
            get {
                return this.currentIndent;
            }
        }
        
        private global::System.Collections.Generic.Stack<int> Indents {
            get {
                if ((this.indents == null)) {
                    this.indents = new global::System.Collections.Generic.Stack<int>();
                }
                return this.indents;
            }
        }
        
        public ToStringInstanceHelper ToStringHelper {
            get {
                return this._toStringHelper;
            }
        }
        
        public void Error(string message) {
            this.Errors.Add(new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message));
        }
        
        public void Warning(string message) {
            global::System.CodeDom.Compiler.CompilerError val = new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message);
            val.IsWarning = true;
            this.Errors.Add(val);
        }
        
        public string PopIndent() {
            if ((this.Indents.Count == 0)) {
                return string.Empty;
            }
            int lastPos = (this.currentIndent.Length - this.Indents.Pop());
            string last = this.currentIndent.Substring(lastPos);
            this.currentIndent = this.currentIndent.Substring(0, lastPos);
            return last;
        }
        
        public void PushIndent(string indent) {
            this.Indents.Push(indent.Length);
            this.currentIndent = (this.currentIndent + indent);
        }
        
        public void ClearIndent() {
            this.currentIndent = string.Empty;
            this.Indents.Clear();
        }
        
        public void Write(string textToAppend) {
            this.GenerationEnvironment.Append(textToAppend);
        }
        
        public void Write(string format, params object[] args) {
            this.GenerationEnvironment.AppendFormat(format, args);
        }
        
        public void WriteLine(string textToAppend) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendLine(textToAppend);
        }
        
        public void WriteLine(string format, params object[] args) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendFormat(format, args);
            this.GenerationEnvironment.AppendLine();
        }
        
        public class ToStringInstanceHelper {
            
            private global::System.IFormatProvider formatProvider = global::System.Globalization.CultureInfo.InvariantCulture;
            
            public global::System.IFormatProvider FormatProvider {
                get {
                    return this.formatProvider;
                }
                set {
                    if ((value != null)) {
                        this.formatProvider = value;
                    }
                }
            }
            
            public string ToStringWithCulture(object objectToConvert) {
                if ((objectToConvert == null)) {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                global::System.Type type = objectToConvert.GetType();
                global::System.Type iConvertibleType = typeof(global::System.IConvertible);
                if (iConvertibleType.IsAssignableFrom(type)) {
                    return ((global::System.IConvertible)(objectToConvert)).ToString(this.formatProvider);
                }
                global::System.Reflection.MethodInfo methInfo = type.GetMethod("ToString", new global::System.Type[] {
                            iConvertibleType});
                if ((methInfo != null)) {
                    return ((string)(methInfo.Invoke(objectToConvert, new object[] {
                                this.formatProvider})));
                }
                return objectToConvert.ToString();
            }
        }
    }
}
