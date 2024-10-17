using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebClient {
    /// <summary>
    /// autogenerated-menu.tsx ファイル
    /// </summary>
    internal class MenuTsx : ISummarizedFile {

        public class MenuItem {
            public required string Url { get; set; }
            public required string ImportAs { get; set; }
            public required string ImportFrom { get; set; }
            public required bool ShowSideMenu { get; set; }
            public required string? Label { get; set; }
        }
        private readonly List<MenuItem> _menuItems = new();
        public void AddMenuItem(MenuItem menuItem) {
            _menuItems.Add(menuItem);
        }

        public void OnEndGenerating(CodeRenderingContext context) {
            context.ReactProject.AutoGeneratedDir(dir => {
                dir.Generate(Render());
            });
        }

        private SourceFile Render() => new SourceFile {
            FileName = "autogenerated-menu.tsx",
            RenderContent = context => $$"""
                import React from 'react'
                import { RouteObject } from 'react-router-dom'
                {{_menuItems.SelectTextTemplate(menu => $$"""
                import {{menu.ImportAs}} from '{{menu.ImportFrom}}'
                """)}}

                export const THIS_APPLICATION_NAME = '{{context.Schema.ApplicationName}}' as const

                export const routes: RouteObject[] = [
                {{_menuItems.SelectTextTemplate(menu => $$"""
                  { path: '{{menu.Url}}', element: <{{menu.ImportAs}} /> },
                """)}}
                ]

                export type SideMenuItem = {
                  url: string
                  text: React.ReactNode
                  icon?: React.ElementType
                  children?: SideMenuItem[]
                }
                export const menuItems: SideMenuItem[] = [
                {{_menuItems.Where(m => m.ShowSideMenu).SelectTextTemplate(menu => $$"""
                  { url: '{{menu.Url}}', text: '{{menu.Label?.Replace("'", "\\'")}}' },
                """)}}
                ]

                export const SHOW_LOCAL_REPOSITORY_MENU = {{(context.Config.DisableLocalRepository ? "false" : "true")}}
                """,
        };
    }
}
