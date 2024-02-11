import React, { useMemo, useState } from 'react'
import { expect, test } from 'vitest'
import { setup as setupIndexedDB } from 'vitest-indexeddb'
import { renderHook, act, waitFor } from '@testing-library/react'
import { useIndexedDbTable } from '../src/__autoGenerated/util'
import {
  useLocalRepository,
  LocalRepositoryStateAndKeyAndItem,
  LocalRepositoryState,
  LocalRepositoryContextProvider,
  LocalRepositoryArgs,
} from '../src/__autoGenerated/util/LocalRepository'


// TODO: ローカルリポジトリでリモートにあるデータと同じキーのデータが入力された場合のテスト

test('useLocalRepository 状態遷移テスト（排他に引っかかるパターンなし網羅）', async () => {
  setupIndexedDB()
  const { Remote, scope, Debug } = setupLocalRepositoryHook()

  // ----------------------------------------
  expect(await current(Remote)).toEqual<TestData[]>([
  ])

  // 画面表示 1回目
  await scope(async Local => {
    expect(await current(Local)).toEqual<LocalState[]>([
    ])

    let a = await add(Local, 'a')
    let b = await add(Local, 'b')
    let x = await add(Local, 'x')
    let y = await add(Local, 'y')
    expect(await current(Local)).toEqual<LocalState[]>([
      ['+', { key: 'a', name: '', version: 0 }],
      ['+', { key: 'b', name: '', version: 0 }],
      ['+', { key: 'x', name: '', version: 0 }],
      ['+', { key: 'y', name: '', version: 0 }],
    ])

    a = await edit(Local, a, 'あ')
    a = await edit(Local, a, 'い')
    expect(await current(Local)).toEqual<LocalState[]>([
      ['+', { key: 'a', name: 'い', version: 0 }],
      ['+', { key: 'b', name: '', version: 0 }],
      ['+', { key: 'x', name: '', version: 0 }],
      ['+', { key: 'y', name: '', version: 0 }],
    ])

    await remove(Local, x)
    await remove(Local, x)
    expect(await current(Local)).toEqual<LocalState[]>([
      ['+', { key: 'a', name: 'い', version: 0 }],
      ['+', { key: 'b', name: '', version: 0 }],
      ['+', { key: 'y', name: '', version: 0 }],
    ])
  })

  // 他者がリモートリポジトリのデータを更新
  expect(await current(Remote)).toEqual<TestData[]>([
  ])
  await add(Remote, 'z')
  expect(await current(Remote)).toEqual<TestData[]>([
    { key: 'z', name: '', version: 0 },
  ])

  // 画面表示 2回目
  await scope(async Local => {
    expect(await current(Local)).toEqual<LocalState[]>([
      ['+', { key: 'a', name: 'い', version: 0 }],
      ['+', { key: 'b', name: '', version: 0 }],
      ['+', { key: 'y', name: '', version: 0 }],
      ['', { key: 'z', name: '', version: 0 }],
    ])

    let a = (await get(Local, 'a'))!
    let y = (await get(Local, 'y'))!
    await edit(Local, a, 'う')
    await edit(Local, a, 'え')
    await remove(Local, y)
    await remove(Local, y)
    await add(Local, 'c')
    await add(Local, 'd')
    expect(await current(Local)).toEqual<LocalState[]>([
      ['+', { key: 'a', name: 'え', version: 0 }],
      ['+', { key: 'b', name: '', version: 0 }],
      ['+', { key: 'c', name: '', version: 0 }],
      ['+', { key: 'd', name: '', version: 0 }],
      ['', { key: 'z', name: '', version: 0 }],
    ])

    await save(Remote, Local)
    await save(Remote, Local)
    expect(await current(Local)).toEqual<LocalState[]>([
      ['', { key: 'a', name: 'え', version: 0 }],
      ['', { key: 'b', name: '', version: 0 }],
      ['', { key: 'c', name: '', version: 0 }],
      ['', { key: 'd', name: '', version: 0 }],
      ['', { key: 'z', name: '', version: 0 }],
    ])
  })

  expect(await current(Remote)).toEqual<TestData[]>([
    { key: 'a', name: 'え', version: 0 },
    { key: 'b', name: '', version: 0 },
    { key: 'c', name: '', version: 0 },
    { key: 'd', name: '', version: 0 },
    { key: 'z', name: '', version: 0 },
  ])

  // 画面表示 3回目
  await scope(async Local => {
    expect(await current(Local)).toEqual<LocalState[]>([
      ['', { key: 'a', name: 'え', version: 0 }],
      ['', { key: 'b', name: '', version: 0 }],
      ['', { key: 'c', name: '', version: 0 }],
      ['', { key: 'd', name: '', version: 0 }],
      ['', { key: 'z', name: '', version: 0 }],
    ])

    let a = (await get(Local, 'a'))!
    let b = (await get(Local, 'b'))!
    let c = (await get(Local, 'c'))!
    let d = (await get(Local, 'd'))!
    a = await edit(Local, a, 'お')
    a = await edit(Local, a, 'か')
    b = await edit(Local, b, 'ぱ')
    b = await edit(Local, b, 'ぴ')
    await remove(Local, c)
    await remove(Local, c)
    await remove(Local, d)
    await remove(Local, d)
    expect(await current(Local)).toEqual<LocalState[]>([
      ['*', { key: 'a', name: 'か', version: 0 }],
      ['*', { key: 'b', name: 'ぴ', version: 0 }],
      ['-', { key: 'c', name: '', version: 0 }],
      ['-', { key: 'd', name: '', version: 0 }],
      ['', { key: 'z', name: '', version: 0 }],
    ])
  })

  // 画面表示 4回目
  await scope(async Local => {
    await save(Remote, Local)
    await save(Remote, Local)

    expect(await current(Local)).toEqual<LocalState[]>([
      ['', { key: 'a', name: 'か', version: 1 }],
      ['', { key: 'b', name: 'ぴ', version: 1 }],
      ['', { key: 'z', name: '', version: 0 }],
    ])
  })

  expect(await current(Remote)).toEqual<TestData[]>([
    { key: 'a', name: 'か', version: 1 },
    { key: 'b', name: 'ぴ', version: 1 },
    { key: 'z', name: '', version: 0 },
  ])
})


// ----------------------------------------
// テストコードを読みやすくするためのヘルパー関数
type TestData = {
  key?: string
  name?: string
  numValue?: number
  version: number
}
type LocalState = [LocalRepositoryState, TestData]
type TestLocalRepos = { current: ReturnType<typeof useLocalRepository<TestData>> }
type TestRemoteRepos = { current: { state: Map<string, TestData>, dispatch: (v: Map<string, TestData>) => void } }
const REPOS_SETTING: LocalRepositoryArgs<TestData> = {
  dataTypeKey: 'TEST-DATA-20240204',
  getItemKey: data => data.key ?? '',
  getItemName: data => data.name ?? '',
}

const setupLocalRepositoryHook = () => {
  /** リモートリポジトリ */
  const { result: Remote } = renderHook(() => {
    const [state, dispatch] = useState(() => new Map<string, TestData>())
    return { state, dispatch: dispatch as (v: Map<string, TestData>) => void }
  })

  /** 画面表示と対応するスコープ */
  const scope = async (fn: (Local: TestLocalRepos) => Promise<void>): Promise<void> => {
    const wrapper = ({ children }: { children?: React.ReactNode }) => {
      return (
        <LocalRepositoryContextProvider>
          {children}
        </LocalRepositoryContextProvider>
      )
    }

    const { result, unmount } = renderHook(() => {
      const remoteItems = useMemo(() => {
        return Array.from(Remote.current.state.values())
      }, [Remote.current.state])
      return useLocalRepository({ ...REPOS_SETTING, remoteItems })
    }, { wrapper })

    // 内部でuseEffectを使っているので初期化完了まで待つ
    await waitFor(() => expect(result.current.ready).toBe(true))

    await fn(result)
    unmount()
  }

  // 不具合調査用
  const { result: Debug } = renderHook(() => useIndexedDbTable<{
    dataTypeKey: string
    itemKey: string
    itemName: string
    serializedItem: string
    state: LocalRepositoryState
  }>({
    dbName: '::nijo::',
    dbVersion: 1,
    tableName: 'LocalRepository',
    keyPath: ['dataTypeKey', 'itemKey'],
  }))

  return { Remote, scope, Debug }
}

const isLocal = (localOrRemote: TestLocalRepos | TestRemoteRepos): localOrRemote is TestLocalRepos => {
  return typeof (localOrRemote as TestLocalRepos).current?.addToLocalRepository === 'function'
}
async function current(local: TestLocalRepos): Promise<LocalState[]>
async function current(remote: TestRemoteRepos): Promise<TestData[]>
async function current(localOrRemote: TestLocalRepos | TestRemoteRepos): Promise<(LocalState[] | TestData[])> {
  if (isLocal(localOrRemote)) {
    return await act(async () => {
      const localItems = await localOrRemote.current.loadLocalItems()
      const localState: LocalState[] = localItems
        .sort((a, b) => {
          if ((a.item.key ?? '') < (b.item.key ?? '')) return -1
          if ((a.item.key ?? '') > (b.item.key ?? '')) return 1
          return 0
        })
        .map(x => [x.state, x.item] as const)
      return localState
    })
  } else {
    return Array
      .from(localOrRemote.current.state.values())
      .sort((a, b) => {
        if ((a.key ?? '') < (b.key ?? '')) return -1
        if ((a.key ?? '') > (b.key ?? '')) return 1
        return 0
      })
  }
}
async function add(local: TestLocalRepos, key: string): Promise<LocalRepositoryStateAndKeyAndItem<TestData>>
async function add(remote: TestRemoteRepos, key: string): Promise<void>
async function add(localOrRemote: TestLocalRepos | TestRemoteRepos, key: string): Promise<LocalRepositoryStateAndKeyAndItem<TestData> | void> {
  return await act(async () => {
    const data: TestData = { key, name: '', version: 0 }
    if (isLocal(localOrRemote)) {
      return await localOrRemote.current.addToLocalRepository(data)
    } else {
      if (localOrRemote.current.state.has(key)) throw new Error(`キー重複: ${key}`)
      const map = new Map(localOrRemote.current.state)
      map.set(key, data)
      localOrRemote.current.dispatch(map)
    }
  })
}
async function edit(local: TestLocalRepos, item: LocalRepositoryStateAndKeyAndItem<TestData>, name: string): Promise<LocalRepositoryStateAndKeyAndItem<TestData>> {
  return await act(async () => {
    if (!item.itemKey) throw new Error('Key is nothing.')
    return await local.current.updateLocalRepositoryItem(item.itemKey, { ...item.item, name })
  })
}
async function remove(local: TestLocalRepos, item: LocalRepositoryStateAndKeyAndItem<TestData>): Promise<void> {
  await act(async () => {
    await local.current.deleteLocalRepositoryItem(item.itemKey, item.item)
  })
}
async function get(local: TestLocalRepos, key: string): Promise<LocalRepositoryStateAndKeyAndItem<TestData> | undefined> {
  const localItems = await local.current.loadLocalItems()
  return localItems.find(x => x.item.key === key)
}
async function save(remote: TestRemoteRepos, local: TestLocalRepos): Promise<void> {
  await act(async () => {
    const localItems = await local.current.loadLocalItems()
    const newRemote = new Map(remote.current.state)
    const commited: string[] = []
    for (const localItem of localItems) {
      if (localItem.state === '+') {
        if (!localItem.item.key) { console.error(`キーなし: ${localItem.item.name}`); continue }
        if (remote.current.state.has(localItem.item.key)) { console.error(`キー重複: ${localItem.item.key}`); continue }
        commited.push(localItem.itemKey)
        newRemote.set(localItem.item.key, localItem.item)

      } else if (localItem.state === '*') {
        if (!localItem.item.key) { console.error(`キーなし: ${localItem.item.name}`); continue }
        if (!remote.current.state.has(localItem.item.key)) { console.error(`更新対象なし: ${localItem.item.key}`); continue }
        commited.push(localItem.itemKey)
        newRemote.set(localItem.item.key, { ...localItem.item, version: localItem.item.version + 1 })
        remote.current.dispatch(newRemote)

      } else if (localItem.state === '-') {
        if (!localItem.item.key) { console.error(`キーなし: ${localItem.item.name}`); continue }
        if (!remote.current.state.has(localItem.item.key)) { console.error(`更新対象なし: ${localItem.item.key}`); continue }
        commited.push(localItem.itemKey)
        newRemote.delete(localItem.item.key)
        remote.current.dispatch(newRemote)
      }
    }
    remote.current.dispatch(newRemote)
    await local.current.commit(...commited)
  })
}
