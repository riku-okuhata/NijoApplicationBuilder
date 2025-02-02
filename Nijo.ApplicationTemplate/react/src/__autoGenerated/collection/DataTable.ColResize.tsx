import { useMemo, useCallback } from 'react'
import useEvent from 'react-use-event-hook'
import * as RT from '@tanstack/react-table'

export const getColumnResizeOption = <T,>(): Partial<RT.TableOptions<T>> => ({
  defaultColumn: {
    minSize: 8,
    maxSize: 800,
  },
  columnResizeMode: 'onChange',
})

export const useColumnResizing = <T,>(
  api: RT.Table<T>,
  /** 列定義が動的に変わるとき、変わったタイミングで列幅再計算させるためのもの */
  columns: unknown
) => {

  const { colSizes, colSizesByIndex } = useMemo(() => {
    const headers = api.getFlatHeaders()
    const colSizes: { [key: string]: number } = {}
    const colSizesByIndex: { [key: number]: number } = {}
    for (let i = 0; i < headers.length; i++) {
      const header = headers[i]!
      colSizes[`--header-${header.id}-size`] = header.getSize()
      colSizes[`--col-${header.column.id}-size`] = header.column.getSize()
      colSizesByIndex[i] = header.column.getSize()
    }
    return { colSizes, colSizesByIndex }

    // @tanstack/react-table の仕様上、
    // columnSizingInfoオブジェクトの参照が変わったタイミングでの列幅再計算が妥当
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [api.getState().columnSizingInfo, columns])

  const getColWidth = useCallback((column: RT.Column<T, unknown>) => {
    return `calc(var(--header-${column.id}-size) * 1px)`
  }, [])

  const getColWidthByIndex = useEvent((colIndex: number): number => {
    return colSizesByIndex[colIndex] ?? 0
  })

  const ResizeHandler = useCallback(({ header }: {
    header: RT.Header<T, unknown>
  }) => {
    return (
      <div {...{
        onDoubleClick: () => header.column.resetSize(),
        onMouseDown: header.getResizeHandler(),
        onTouchStart: header.getResizeHandler(),
        className: `absolute top-0 bottom-0 right-0 w-3 cursor-ew-resize border-r border-color-3`,
      }}>
      </div>
    )
  }, [])

  return {
    columnSizeVars: colSizes,
    getColWidth,
    getColWidthByIndex,
    ResizeHandler,
  }
}
