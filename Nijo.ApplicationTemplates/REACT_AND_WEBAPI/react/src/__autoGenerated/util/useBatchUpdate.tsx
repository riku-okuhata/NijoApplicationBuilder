import React, { useCallback, useContext, useEffect, useMemo, useReducer, useState } from 'react'
import { UUID } from 'uuidjs'
import * as ReactUtil from './ReactUtil'
import * as Validation from './Validation'
import * as Notification from './Notification'
import { useFieldArray } from 'react-hook-form'
import { useIndexedDbTable } from './Storage'


// 一覧/特定集約 共用

export type LocalRepositoryState
  = '' // No Change (Exists only remote repository)
  | '+' // Add
  | '*' // Modify
  | '-' // Delete

type LocalRepositoryStoredItem = {
  dataTypeKey: string
  itemKey: string
  itemName: string
  serializedItem: string
  state: LocalRepositoryState
}

const useIndexedDbLocalRepositoryTable = () => {
  return useIndexedDbTable<LocalRepositoryStoredItem>({
    dbName: '::nijo::',
    dbVersion: 1,
    tableName: 'LocalRepository',
    keyPath: ['dataTypeKey', 'itemKey'],
  })
}

// -------------------------------------------------
// ローカルリポジトリ変更一覧

export type LocalRepositoryItemListItem = {
  dataTypeKey: string
  itemKey: string
  itemName: string
  state: LocalRepositoryState
}

type LocalRepositoryContextValue = {
  changes: LocalRepositoryItemListItem[]
  setToLocalRepository: (value: LocalRepositoryStoredItem) => Promise<void>
  deleteFromLocalRepository: (key: IDBValidKey) => Promise<void>
  reload: () => Promise<void>
  ready: boolean
}
const LocalRepositoryContext = React.createContext<LocalRepositoryContextValue>({
  changes: [],
  setToLocalRepository: () => Promise.resolve(),
  deleteFromLocalRepository: () => Promise.resolve(),
  reload: () => Promise.resolve(),
  ready: false,
})

export const LocalRepositoryContextProvider = ({ children }: {
  children?: React.ReactNode
}) => {
  const { ready, openCursor, request, dump } = useIndexedDbLocalRepositoryTable()
  const [changes, setChanges] = useState<LocalRepositoryItemListItem[]>([])

  const reload = useCallback(async () => {
    const changes: LocalRepositoryItemListItem[] = []
    await openCursor('readonly', cursor => {
      changes.push({
        dataTypeKey: cursor.value.dataTypeKey,
        state: cursor.value.state,
        itemKey: cursor.value.itemKey,
        itemName: cursor.value.itemName,
      })
    })
    setChanges(changes)
  }, [openCursor, setChanges])

  const setToLocalRepository = useCallback(async (data: LocalRepositoryStoredItem) => {
    await request(table => table.put(data))
    await reload()
  }, [request, reload])

  const deleteFromLocalRepository = useCallback(async (key: IDBValidKey) => {
    await request(table => table.delete(key))
    await reload()
  }, [request, reload])

  const contextValue: LocalRepositoryContextValue = useMemo(() => ({
    changes,
    setToLocalRepository,
    deleteFromLocalRepository,
    reload,
    ready,
  }), [changes, setToLocalRepository, deleteFromLocalRepository, ready, reload])

  useEffect(() => {
    if (ready) reload()
  }, [reload, ready])

  return (
    <LocalRepositoryContext.Provider value={contextValue}>
      {children}
    </LocalRepositoryContext.Provider>
  )
}

export const useLocalRepositoryChangeList = () => {
  const { ready, changes } = useContext(LocalRepositoryContext)
  return { ready, changes }
}

// -------------------------------------------------
// 特定の集約の変更

export type LocalRepositoryArgs<T> = {
  dataTypeKey: string
  getItemKey: (t: T) => string
  getItemName?: (t: T) => string
  serialize: (t: T) => string
  deserialize: (str: string) => T
  findInRemote?: RemoteRepositoryFetchFunction<T>
}
export type RemoteRepositoryFetchFunction<T> = (itemKey: string) => (T | undefined)
export type LocalRepositoryStateAndKeyAndItem<T> = {
  itemKey: string
  state: LocalRepositoryState
  item: T
}

export const useLocalRepository = <T,>({
  dataTypeKey,
  getItemKey,
  getItemName,
  serialize,
  deserialize,
  findInRemote,
}: LocalRepositoryArgs<T>) => {

  const {
    ready,
    reload: reloadContext,
    setToLocalRepository: setToDb,
    deleteFromLocalRepository: delFromDb,
  } = useContext(LocalRepositoryContext)
  const {
    openCursor,
    request,
  } = useIndexedDbLocalRepositoryTable()

  const loadAll = useCallback(async (): Promise<LocalRepositoryStateAndKeyAndItem<T>[]> => {
    const arr: LocalRepositoryStateAndKeyAndItem<T>[] = []
    await openCursor('readonly', cursor => {
      if (cursor.value.dataTypeKey === dataTypeKey) arr.push({
        item: deserialize(cursor.value.serializedItem),
        itemKey: cursor.value.itemKey,
        state: cursor.value.state,
      })
    })
    return arr
  }, [dataTypeKey, deserialize, openCursor, getItemKey])

  const getLocalRepositoryState = useCallback(async (itemKey: string): Promise<LocalRepositoryStateAndKeyAndItem<T> | undefined> => {
    // ローカルリポジトリにある場合はその内容を優先
    const key: IDBValidKey = [dataTypeKey, itemKey]
    const foundInLocal = await request(table => table.get(key) as IDBRequest<LocalRepositoryStoredItem>)
    if (foundInLocal) return {
      item: deserialize(foundInLocal.serializedItem),
      itemKey: foundInLocal.itemKey,
      state: foundInLocal.state,
    }
    // ローカルリポジトリに無い場合はリモートから探す
    const foundInRemote = findInRemote?.(itemKey)
    if (foundInRemote) return {
      item: foundInRemote,
      itemKey: getItemKey(foundInRemote),
      state: '',
    }
    return undefined
  }, [dataTypeKey, deserialize, getItemKey, request, findInRemote])

  const decorate = useCallback(async (remoteItems: T[]): Promise<LocalRepositoryStateAndKeyAndItem<T>[]> => {
    const decorated: LocalRepositoryStateAndKeyAndItem<T>[] = []
    const unhandled = new Map((await loadAll()).map(x => [x.itemKey, x]))
    for (const remote of remoteItems) {
      const itemKey = getItemKey(remote)
      const dbKey: IDBValidKey = [dataTypeKey, itemKey]
      const foundInLocal = await request(table => table.get(dbKey) as IDBRequest<LocalRepositoryStoredItem>)
      const item = foundInLocal ? deserialize(foundInLocal.serializedItem) : remote
      const state = foundInLocal?.state ?? ''
      decorated.push({ item, itemKey, state })
      if (foundInLocal) unhandled.delete(itemKey)
    }
    decorated.unshift(...unhandled.values())
    return decorated
  }, [getItemKey, loadAll, request, dataTypeKey, deserialize])

  const addToLocalRepository = useCallback(async (item: T): Promise<LocalRepositoryStateAndKeyAndItem<T>> => {
    const itemKey = UUID.generate()
    const itemName = getItemName?.(item) ?? ''
    const serializedItem = serialize(item)
    const state: LocalRepositoryState = '+'
    await setToDb({ state, dataTypeKey, itemKey, itemName, serializedItem })
    return { itemKey, state, item }
  }, [dataTypeKey, setToDb, getItemName, serialize])

  const updateLocalRepositoryItem = useCallback(async (itemKey: string, item: T): Promise<LocalRepositoryStateAndKeyAndItem<T>> => {
    const serializedItem = serialize(item)
    const itemName = getItemName?.(item) ?? ''
    const stateBeforeUpdate = (await getLocalRepositoryState(itemKey))?.state
    const state: LocalRepositoryState = stateBeforeUpdate === '+' || stateBeforeUpdate === '-'
      ? stateBeforeUpdate
      : '*'
    await setToDb({ dataTypeKey, itemKey, itemName, serializedItem, state })
    return { itemKey, state, item }
  }, [dataTypeKey, setToDb, serialize, getItemName, getLocalRepositoryState])

  const deleteLocalRepositoryItem = useCallback(async (itemKey: string, item: T): Promise<{ remains: boolean }> => {
    const stateBeforeUpdate = (await getLocalRepositoryState(itemKey))?.state
    if (stateBeforeUpdate === '+') {
      await delFromDb([dataTypeKey, itemKey])
      return { remains: false }

    } else if (stateBeforeUpdate === '' || stateBeforeUpdate === '*') {
      const serializedItem = serialize(item)
      const itemName = getItemName?.(item) ?? ''
      const state: LocalRepositoryState = '-'
      await setToDb({ dataTypeKey, itemKey, itemName, serializedItem, state })
      return { remains: true }

    } else if (stateBeforeUpdate === '-') {
      return { remains: true }

    } else {
      // リモートにも存在しない場合
      return { remains: false }
    }
  }, [dataTypeKey, delFromDb, setToDb, serialize, getItemKey, getItemName, getLocalRepositoryState])

  const commit = useCallback(async (itemKey: string): Promise<void> => {
    await delFromDb([dataTypeKey, itemKey])
  }, [delFromDb, dataTypeKey])

  const reset = useCallback(async (): Promise<void> => {
    await openCursor('readwrite', cursor => {
      if (cursor.value.dataTypeKey === dataTypeKey) cursor.delete()
    })
    await reloadContext()
  }, [delFromDb, dataTypeKey, reloadContext])

  return {
    ready,
    loadAll,
    getLocalRepositoryState,
    decorate,
    addToLocalRepository,
    updateLocalRepositoryItem,
    deleteLocalRepositoryItem,
    commit,
    reset,
  }
}
