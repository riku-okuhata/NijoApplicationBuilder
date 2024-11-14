import React, { useMemo } from 'react'
import { createBrowserRouter, Link, NavLink, Outlet, RouteObject, RouterProvider, useLocation } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from 'react-query'
import * as Icon from '@heroicons/react/24/outline'
import { ImperativePanelHandle, Panel, PanelGroup, PanelResizeHandle } from 'react-resizable-panels'
import { DialogContextProvider } from './collection'
import * as Util from './util'
import * as AutoGenerated from './autogenerated-menu'
import { UiContext, UiContextProvider } from './default-ui-component'

export * from './collection'
export * from './input'
export * from './util'

import './nijo-default-style.css'
import { UiCustomizer } from './autogenerated-types'

/** useQueryを使うために必須 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
      refetchOnWindowFocus: false,
    },
  },
})

/** DefaultNijoAppのプロパティ */
export type DefaultNijoAppProps = {
  /** UIカスタマイザ。自動生成されたUIをカスタマイズする場合はこの関数の中で定義してください。 */
  uiCustomizer: UiCustomizer
  /** アプリケーション名 */
  applicationName: string
  /** Raect router のルーティング処理（クライアント側のURLとページの紐づき設定）を編集するReactフック */
  useRouteCustomizer?: () => ((defaultRoutes: RouteObject[]) => RouteObject[])
}

/** 自動生成されるソースとその外側との境界 */
export function DefaultNijoApp(props: DefaultNijoAppProps) {
  return (
    <UiContextProvider customizer={props.uiCustomizer}>
      <ReactRouterSetting {...props} />
    </UiContextProvider>
  )
}

/** URL設定 */
const ReactRouterSetting = (props: DefaultNijoAppProps) => {
  const { useRouteCustomizer } = props
  const modifyRoutes = useRouteCustomizer?.()
  const UI = React.useContext(UiContext)
  const router = useMemo(() => {
    const defaultRoutes: RouteObject[] = [
      ...Object.values(AutoGenerated.getAutoGeneratedRouts(UI)),
    ]
    return createBrowserRouter([{
      path: '/',
      children: modifyRoutes?.(defaultRoutes) ?? defaultRoutes,
      element: (
        <QueryClientProvider client={queryClient}>
          <Util.MsgContextProvider>
            <Util.ToastContextProvider>
              <Util.LocalRepositoryContextProvider>
                <Util.UserSettingContextProvider>
                  <DialogContextProvider>
                    <ApplicationRoot {...props} />
                    <Util.EnvNameRibbon />
                    <Util.Toast />
                  </DialogContextProvider>
                </Util.UserSettingContextProvider>
              </Util.LocalRepositoryContextProvider>
            </Util.ToastContextProvider>
          </Util.MsgContextProvider>
        </QueryClientProvider>
      ),
    },
    ])
  }, [UI, modifyRoutes, ...Object.values(props)])

  return (
    <RouterProvider router={router} />
  )
}

/** アプリケーション本体 */
const ApplicationRoot = ({ applicationName }: DefaultNijoAppProps) => {

  const { data: { darkMode, fontFamily } } = Util.useUserSetting()
  const { LoginPage, SideMenuTop, SideMenuBottom } = React.useContext(UiContext)

  // サイドメニュー開閉
  const sideMenuRef = React.useRef<ImperativePanelHandle>(null)
  const sideMenuContextValue = React.useMemo((): Util.SideMenuContextType => ({
    toggle: () => sideMenuRef.current?.getCollapsed()
      ? sideMenuRef.current.expand()
      : sideMenuRef.current?.collapse(),
    setCollapsed: collapsed => collapsed
      ? sideMenuRef.current?.collapse()
      : sideMenuRef.current?.expand(),
  }), [sideMenuRef])

  return (
    <LoginPage LoggedInContents={(
      <Util.SideMenuContext.Provider value={sideMenuContextValue}>
        <PanelGroup
          direction='horizontal'
          autoSaveId="LOCAL_STORAGE_KEY.SIDEBAR_SIZE_X"
          className={darkMode ? 'dark' : undefined}
          style={{ fontFamily: fontFamily ?? Util.DEFAULT_FONT_FAMILY }}>

          {/* サイドメニュー */}
          <Panel ref={sideMenuRef} defaultSize={20} collapsible>
            <PanelGroup direction="vertical"
              className="bg-color-2 text-color-12"
              autoSaveId="LOCAL_STORAGE_KEY.SIDEBAR_SIZE_Y">
              <Panel className="flex flex-col">
                <Link to='/' className="p-1 ellipsis-ex font-semibold select-none border-r border-color-4">
                  {applicationName}
                </Link>
                <nav className="flex-1 overflow-y-auto leading-none flex flex-col">
                  <SideMenuTop />
                  <div className="flex-1 min-h-0 border-r border-color-4"></div>
                </nav>
                <nav className="flex flex-col">
                  <SideMenuBottom />
                </nav>
                <span className="p-1 text-sm whitespace-nowrap overflow-hidden border-r border-color-4">
                  ver. 0.9.0.0
                </span>
              </Panel>
            </PanelGroup>
          </Panel>

          <PanelResizeHandle className="w-1 bg-color-base" />

          {/* コンテンツ */}
          <Panel className={`flex flex-col bg-color-base text-color-12`}>
            <Util.MsgContextProvider>

              {/* createBrowserRouterのchildrenのうち現在のURLと対応するものがOutletの位置に表示される */}
              <Outlet />

            </Util.MsgContextProvider>

            {/* コンテンツの外で発生したエラーが表示される欄 */}
            <Util.InlineMessageList />

          </Panel>
        </PanelGroup>
      </Util.SideMenuContext.Provider>
    )} />
  )
}
