import React, { memo } from 'react'
import useEvent from 'react-use-event-hook'
import { useFieldArray, useWatch } from 'react-hook-form'
import * as Icon from '@heroicons/react/24/solid'
import { Panel, PanelGroup, PanelResizeHandle } from 'react-resizable-panels'
import * as Util from './__autoGenerated/util'
import * as Input from './__autoGenerated/input'
import * as Layout from './__autoGenerated/collection'
import { AggregateOrMember, GridRow, createNewGridRow, PageState, ValidationError } from './types'
import { useBackend } from './useBackend'
import { useColumnDef } from './useColumnDef'
import { useTypeCombo } from './useTypeCombo'
import { useFlattenArrayTree } from './useFlattenArrayTree'
import { useAppSetting } from './useAppSetting'
import { useValidationErrorContext, ValidationErrorContextProvider } from './useValidationError'

function App() {

  // データ
  const { ready, load, validate, save, backendDomain, onChangBackendDomain } = useBackend()
  const { getValues, reset, control } = Util.useFormEx<PageState>({})
  const { fields, update, insert, remove } = useFieldArray({ name: 'aggregates', control })

  // ツリー構造関連の操作
  const fieldsRef = React.useRef<AggregateOrMember[]>()
  fieldsRef.current = fields
  const {
    collapsedRowIds,
    expandableRows,
    expandedRows,
    collapseAll,
    expandAll,
    toggleCollapsing,
    ...treeMethods
  } = useFlattenArrayTree(fields, fieldsRef)

  // ----------------------
  // データ型の種類
  const aggregateOrMemberTypes = useWatch({ name: 'aggregateOrMemberTypes', control })
  const typeCombo = useTypeCombo(aggregateOrMemberTypes, fields, treeMethods.getDescendants)

  // ----------------------
  // アプリ設定
  const editingXmlFilePath = useWatch({ name: 'editingXmlFilePath', control })
  const { openAppSettingDialog } = useAppSetting(backendDomain, onChangBackendDomain, editingXmlFilePath)

  // ----------------------
  // 列定義
  const optionalAttributes = useWatch({ name: 'optionalAttributes', control })
  const columns = useColumnDef(optionalAttributes, typeCombo, expandableRows, collapsedRowIds, toggleCollapsing)

  const gridRef = React.useRef<Layout.DataTableRef<GridRow>>(null)

  // ----------------------
  // 選択中の行の情報
  const [activeRowUniqueId, setActiveRowUniqueId] = React.useState<GridRow['uniqueId']>()
  const [activeRowTreeInfo, setActiveRowTreeInfo] = React.useState<{ member: string, type: string }[]>([])
  const handleActiveRowChanged = useEvent((activeRow?: { getRow: () => GridRow, rowIndex: number }) => {
    if (activeRow) {
      const row = activeRow.getRow()
      const ancestorsAndRow = [...treeMethods.getAncestors(row), row]
      setActiveRowTreeInfo(ancestorsAndRow.map(row => {
        const type = typeCombo.typeComboSource.find(t => t.key === row.type)?.displayName ?? row.type
        return { member: row.displayName ?? '', type: type ? `（${type}）` : '' }
      }))
      setActiveRowUniqueId(row.uniqueId)
    } else {
      setActiveRowTreeInfo([])
      setActiveRowUniqueId(undefined)
    }
  })

  // ----------------------
  // イベント

  // 挿入
  const insertRows = useEvent(() => {
    expandAll()
    const selectedRows = gridRef.current?.getSelectedRows()
    if (selectedRows === undefined) return
    const index = Math.min(...selectedRows.map(r => r.rowIndex))
    const depth = fields[index]?.depth ?? 0
    const count = gridRef.current?.getSelectedRows().length ?? 0
    insert(index, Array.from({ length: count }).map(() => createNewGridRow(depth)))
    executeValidate()
  })

  // 下挿入
  const insertRowsBelow = useEvent(() => {
    expandAll()
    const selectedRows = gridRef.current?.getSelectedRows()
    if (selectedRows === undefined) return
    const index = Math.max(...selectedRows.map(r => r.rowIndex)) + 1
    const depth = fields[index]?.depth ?? 0
    const count = selectedRows.length
    insert(index, Array.from({ length: count }).map(() => createNewGridRow(depth)))
    executeValidate()
  })

  // 削除
  const removeRows = useEvent(() => {
    expandAll()
    const selectedRows = gridRef.current?.getSelectedRows().map(r => r.rowIndex)
    if (selectedRows !== undefined) remove(selectedRows)
    executeValidate()
  })

  // インデント上げ下げ
  const handleIncreaseIndent = useEvent(() => {
    expandAll()
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    for (const { rowIndex, row } of selectedRows) {
      update(rowIndex, { ...row, depth: Math.max(0, row.depth - 1) })
    }
    executeValidate()
  })
  const handleDecreaseIndent = useEvent(() => {
    expandAll()
    const selectedRows = gridRef.current?.getSelectedRows()
    if (!selectedRows) return
    for (const { rowIndex, row } of selectedRows) {
      update(rowIndex, { ...row, depth: row.depth + 1 })
    }
    executeValidate()
  })

  // 行編集
  const handleUpdate = useEvent((rowIndex: number, row: GridRow) => {
    // 引数のrowIndexは折り畳みされた後のインデックスなので実際の更新に使うものは別
    const actualRowIndex = getValues('aggregates')?.findIndex(r => r.uniqueId === row.uniqueId)
    if (actualRowIndex !== undefined) {
      update(actualRowIndex, row)
      executeValidate()
    }
  })

  // CHECK。短時間で連続してクエリが発行されるのを防ぐため、一定時間経過後にリクエストを投げる。
  const timeoutHandle = React.useRef<NodeJS.Timeout | undefined>(undefined)
  const [validationErrors, setValidationErrors] = React.useState<ValidationError>({})
  const executeValidate = useEvent(async () => {
    // 直前に処理の予約がある場合はそれをキャンセルする
    if (timeoutHandle.current !== undefined) clearTimeout(timeoutHandle.current)
    // 処理の実行予約を行う
    return new Promise<void>(() => {
      timeoutHandle.current = setTimeout(async () => {
        // 一定時間内に処理がキャンセルされなかった場合のみここの処理が実行される
        const aggregates = getValues('aggregates')
        if (aggregates) setValidationErrors(await validate(aggregates))
      }, 300)
    })
  })

  // 再読み込み
  const reload = useEvent(async () => {
    reset(await load())
    executeValidate()
  })

  // 保存
  const handleSave = useEvent(async () => {
    if (await save(fields)) {
      await reload()
    } else {
      executeValidate()
    }
  })

  // 画面初期表示時
  React.useEffect(() => {
    if (ready) reload()
  }, [ready])

  // キーボード操作
  const handleKeyDown: React.KeyboardEventHandler = useEvent(e => {
    console.log('!')
    if (e.key === 'Space') {
    }
  })

  return (
    <ValidationErrorContextProvider value={validationErrors}>
      <PanelGroup direction="vertical" className="w-full h-full p-1">
        <Panel className="flex flex-col gap-1">
          <div className="flex gap-1 items-center">
            <Input.IconButton onClick={collapseAll} outline mini>折り畳み</Input.IconButton>
            <Input.IconButton onClick={expandAll} outline mini>展開</Input.IconButton>
            <div className="basis-2"></div>
            <Input.IconButton onClick={insertRows} outline mini>挿入</Input.IconButton>
            <Input.IconButton onClick={insertRowsBelow} outline mini>下挿入</Input.IconButton>
            <Input.IconButton onClick={removeRows} outline mini>行削除</Input.IconButton>
            <div className="basis-2"></div>
            <Input.IconButton onClick={handleIncreaseIndent} outline mini>&lt;&lt;インデント上げ</Input.IconButton>
            <Input.IconButton onClick={handleDecreaseIndent} outline mini>インデント下げ&gt;&gt;</Input.IconButton>
            <div className="flex-1"></div>
            <Input.IconButton onClick={reload} icon={Icon.ArrowPathIcon} outline mini>再読み込み</Input.IconButton>
            <Input.IconButton onClick={executeValidate} icon={Icon.CheckIcon} outline mini>CHECK</Input.IconButton>
            <Input.IconButton onClick={openAppSettingDialog} icon={Icon.Cog6ToothIcon} outline mini>設定</Input.IconButton>
            <Input.IconButton onClick={handleSave} icon={Icon.BookmarkSquareIcon} fill mini>保存</Input.IconButton>
          </div>

          <Util.InlineMessageList />

          {/* 選択中の行の情報 */}
          <div className="flex flex-wrap gap-1 items-center select-none text-sm">
            {activeRowTreeInfo.map((info, ix) => (
              <React.Fragment key={ix}>
                {ix > 0 && <span className="text-color-5">&gt;</span>}
                <div className="bg-color-3 px-1 rounded-md">
                  <span className="text-color-7">{info.member}</span>
                  <span className="text-color-5">{info.type}</span>
                </div>
              </React.Fragment>
            ))}
            &nbsp;
          </div>

          {/* グリッド */}
          <Layout.DataTable
            ref={gridRef}
            data={expandedRows}
            columns={columns}
            onKeyDown={handleKeyDown}
            onChangeRow={handleUpdate}
            onActiveRowChanged={handleActiveRowChanged}
            showActiveCellBorderAlways
            className="flex-1"
          />
        </Panel>

        <PanelResizeHandle className="h-2" />

        <Panel defaultSize={10} collapsible>
          <ActiveRowErrors activeRowUniqueId={activeRowUniqueId} />
        </Panel>
      </PanelGroup>
    </ValidationErrorContextProvider >
  )
}

const ActiveRowErrors = ({ activeRowUniqueId }: { activeRowUniqueId: GridRow['uniqueId'] | undefined }) => {
  const errors = useValidationErrorContext(activeRowUniqueId)

  return (
    <ul className="h-full w-full flex flex-col overflow-y-auto bg-color-gutter">
      {errors?.map((err, ix) => (
        <li key={ix} className="text-sm text-orange-600">{err}</li>
      ))}
    </ul>
  )
}

const style: React.CSSProperties = {
  fontFamily: '"Cascadia Mono", "BIZ UDGothic"',
}

const AppAndContextProviders = () => {
  return (
    <div className="w-full h-full" style={style}>
      <Util.ToastContextProvider>
        <Util.MsgContextProvider>
          <Layout.DialogContextProvider>
            <App />
            <Util.Toast />
          </Layout.DialogContextProvider>
        </Util.MsgContextProvider>
      </Util.ToastContextProvider>
    </div>
  )
}

export default AppAndContextProviders
