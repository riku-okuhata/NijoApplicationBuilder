// このファイルはソース自動生成処理によって上書きされます。

import React from 'react'

/**
 * 自動生成されたUIコンポーネントの型の一覧。
 * カスタマイズが加えられる前の自動生成されたままのもの。
 */
export type AutoGeneratedUi = {
  /** アプリケーション名 */
  applicationName: string
  /** ログイン画面を使用する場合はここに指定してください。ログイン成功の場合は引数のコンポーネントをそのまま返してください。 */
  LoginPage: (props: {
    /** ログイン済みの場合に表示されるコンポーネント */
    LoggedInContents: JSX.Element
  }) => React.ReactNode
  /** サイドメニュー（上部） */
  SideMenuTop: () => React.ReactNode
  /** サイドメニュー（下部） */
  SideMenuBottom: () => React.ReactNode
  /** 設定画面 */
  UserSetting?: () => React.ReactNode
}

/**
 * 自動生成されたUIコンポーネントにカスタマイズを加えた後の型の一覧。
 * 各画面ではここに登録されたコンポーネントの中から必要なものをピックアップして画面を組み上げていく。
 */
export type UiContextValue = AutoGeneratedUi & {}
/**
 * UIコンポーネントのカスタマイズを定義する関数の型。
 */
export type UiCustomizer = (defaultUi: AutoGeneratedUi) => UiContextValue
