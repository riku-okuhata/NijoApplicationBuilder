import React from 'react'
import { useWatch } from 'react-hook-form'
import { QueryClient, QueryClientProvider } from 'react-query'
import * as Input from '../__autoGenerated/input'
import * as Layout from '../__autoGenerated/collection'
import * as Util from '../__autoGenerated/util'
import { ComboProps } from '../__autoGenerated/input'
import useEvent from 'react-use-event-hook'
const VForm2 = Layout.VForm2

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
      refetchOnWindowFocus: false,
    },
  },
})

export default function () {
  return (
    <Layout.DialogContextProvider>
      <QueryClientProvider client={queryClient}>
        <Page />
      </QueryClientProvider>
    </Layout.DialogContextProvider>
  )
}

const Page = () => {

  const { registerEx, getValues, control } = Util.useFormEx<TestData>({ defaultValues: getDefaultTestData() })
  const rootValue = useWatch({ control })

  const [, dispatchDialog] = Layout.useDialogContext()

  const multiSelectFilter = useEvent((keyword: string | undefined): Promise<DDLItem[]> => {
    const selected = new Set(getValues('マルチセレクト')?.map(x => x.key))
    const unSelected = ddlSource.filter(x => !selected.has(x.key))
    return Promise.resolve(keyword
      ? unSelected.filter(x => x.key.includes(keyword) || x.text.includes(keyword))
      : unSelected)
  })

  return (
    <div className="flex p-4 gap-2">
      <div className="flex justify-stretch items-start">
        <div className="grid grid-cols-[8rem,16rem] grid-rows-[auto,auto,auto,auto,fr1] gap-2 overflow-x-auto">
          <label>普通のテキスト</label>
          <Input.Word {...registerEx('普通のテキスト')} />

          <label>フォーマッタつき</label>
          <Input.Date {...registerEx('フォーマッタつき')} />

          <label>同期DDL</label>
          <Input.ComboBox {...registerEx('同期DDL')} {...comboProps} />

          <label>非同期DDL</label>
          <Input.AsyncComboBox {...registerEx('非同期DDL')} {...comboProps} />

          <label>複数選択</label>
          <Input.MultiSelect {...registerEx('マルチセレクト')} {...comboProps} onFilter={multiSelectFilter} />

          <div className="col-span-2">
            <button type="button" onClick={() => {
              dispatchDialog(state => state.pushDialog({ title: 'aaa' }, ctx => (
                <div className="w-64 h-64">
                  aassa
                </div>
              )))
            }}>test</button>

          </div>
        </div>
      </div>

      <Layout.UnknownObjectViewer label="状態表示" value={rootValue} className="flex-1" />
    </div>
  )
}

type TestData = {
  普通のテキスト?: string
  フォーマッタつき?: string
  同期DDL?: DDLItem
  非同期DDL?: DDLItem
  マルチセレクト?: DDLItem[]
}
const getDefaultTestData = (): TestData => ({
  普通のテキスト: undefined,
  フォーマッタつき: undefined,
  同期DDL: undefined,
  非同期DDL: undefined,
  マルチセレクト: [],
})

const ddlSource = Array.from({ length: 20 }).flatMap((_, i) => [
  { key: `aaa${i}` as const, text: `あ(${i})` },
  { key: `iii${i}` as const, text: `い(${i})` },
  { key: `uuu${i}` as const, text: `う(${i})` },
])
type DDLItem = typeof ddlSource[0]

const comboProps: ComboProps<DDLItem, DDLItem> = {
  getOptionText: item => item.text,
  getValueText: item => item.text,
  getValueFromOption: item => item,
  onFilter: async keyword => keyword
    ? ddlSource.filter(x => x.key.includes(keyword) || x.text.includes(keyword))
    : [...ddlSource],
}
