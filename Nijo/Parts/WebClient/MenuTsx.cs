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
                import { RouteObject, NavLink, useLocation } from 'react-router-dom'
                import * as Icon from '@heroicons/react/24/solid'
                import DashBoard from './pages/DashBoard'
                import * as Util from './util'
                {{_menuItems.SelectTextTemplate(menu => $$"""
                import {{menu.ImportAs}} from '{{menu.ImportFrom}}'
                """)}}

                export const THIS_APPLICATION_NAME = '{{context.Schema.ApplicationName}}' as const

                interface IAutoGeneratedRoutes {
                {{_menuItems.SelectTextTemplate(menu => $$"""
                  {{menu.ImportAs}}: RouteObject
                """)}}
                  TopPage: RouteObject
                  UserSettings: RouteObject
                {{If(!context.Config.DisableLocalRepository, () => $$"""
                  LocalRepository: RouteObject
                """)}}
                  NotFound: RouteObject
                }

                /** 自動生成された React router のルーティング設定 */
                export const getAutoGeneratedRouts = (): IAutoGeneratedRoutes => ({
                {{_menuItems.SelectTextTemplate(menu => $$"""
                  {{menu.ImportAs}}: { path: '{{menu.Url}}', element: <{{menu.ImportAs}} /> },
                """)}}
                  TopPage: { path: '/', element: <DashBoard /> },
                  UserSettings: { path: '/settings', element: <Util.ServerSettingScreen /> },
                {{If(!context.Config.DisableLocalRepository, () => $$"""
                  LocalRepository: { path: '/changes', element: <Util.LocalReposChangeListPage /> },
                """)}}
                  NotFound: { path: '*', element: <p> Not found.</p> },
                })

                /** 自動生成されたサイドメニュー（上部） */
                export const AutoGeneratedSideMenuTop = () => {
                  return (<>
                {{_menuItems.Where(m => m.ShowSideMenu).SelectTextTemplate(menu => $$"""
                    <SideMenuLink key="{{menu.Url}}" url="{{menu.Url}}">{{menu.Label?.Replace("'", "\\'")}}</SideMenuLink>
                """)}}
                  </>)
                }
                /** 自動生成されたサイドメニュー（下部） */
                export const AutoGeneratedSideMenuBottom = () => {
                {{If(!context.Config.DisableLocalRepository, () => $$"""
                  // 変更（ローカルリポジトリ）
                  const { changesCount } = Util.useLocalRepositoryChangeList()

                """)}}
                  return <>
                {{If(!context.Config.DisableLocalRepository, () => $$"""
                    <SideMenuLink key="/changes" url="/changes" icon={Icon.ArrowsUpDownIcon}>
                      {changesCount === 0
                        ? <span>一時保存</span>
                        : <span className="font-bold">一時保存&nbsp;({changesCount})</span>}
                    </SideMenuLink>
                """)}}
                    <SideMenuLink key="/settings" url="/settings" icon={Icon.Cog8ToothIcon}>
                      設定
                    </SideMenuLink>
                  </>
                }

                export const SideMenuLink = ({ url, icon, depth, children }: {
                  url: string
                  icon?: React.ElementType
                  depth?: number
                  children?: React.ReactNode
                }) => {

                  // このメニューのページが開かれているかどうかでレイアウトを分ける
                  const location = useLocation()
                  const className = location.pathname.startsWith(url)
                    ? 'flex-none outline-none inline-block w-full p-1 ellipsis-ex border-y border-color-4 bg-color-base font-bold'
                    : 'flex-none outline-none inline-block w-full p-1 ellipsis-ex border-r border-color-4 my-px'

                  // インデント
                  const style: React.CSSProperties = {
                    paddingLeft: depth === undefined ? undefined : `${depth * 1.2}rem`,
                  }

                  return (
                    <NavLink to={url} className={className} style={style}>
                      {React.createElement(icon ?? Icon.CircleStackIcon, { className: 'inline w-4 mr-1 opacity-70 align-middle' })}
                      <span className="text-sm align-middle select-none">{children}</span>
                    </NavLink>
                  )
                }
                """,
        };
    }
}
