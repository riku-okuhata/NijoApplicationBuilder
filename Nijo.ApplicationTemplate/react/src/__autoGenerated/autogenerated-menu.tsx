/**
 * このファイルはソース自動生成によって上書きされます。
 */

import React from 'react'
import { NavLink, RouteObject, useLocation } from 'react-router-dom'
import * as Icon from '@heroicons/react/24/solid'
import DashBoard from './pages/DashBoard'
import * as Util from './util'

interface IAutoGeneratedRoutes {
  TopPage: RouteObject
  UserSettings: RouteObject
  LocalRepository: RouteObject
  NotFound: RouteObject
}

/** 自動生成された React router のルーティング設定 */
export const getAutoGeneratedRouts = (): IAutoGeneratedRoutes => ({
  TopPage: { path: '/', element: <DashBoard /> },
  UserSettings: { path: '/settings', element: <Util.ServerSettingScreen /> },
  LocalRepository: { path: '/changes', element: <Util.LocalReposChangeListPage /> },
  NotFound: { path: '*', element: <p> Not found.</p> },
})

/** 自動生成されたサイドメニュー（上部） */
export const AutoGeneratedSideMenuTop = () => <></>
/** 自動生成されたサイドメニュー（下部） */
export const AutoGeneratedSideMenuBottom = () => {
  // 変更（ローカルリポジトリ）
  const { changesCount } = Util.useLocalRepositoryChangeList()

  return <>
    <SideMenuLink key="/changes" url="/changes" icon={Icon.ArrowsUpDownIcon}>
      {changesCount === 0
        ? <span>一時保存</span>
        : <span className="font-bold">一時保存&nbsp;({changesCount})</span>}
    </SideMenuLink>
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
