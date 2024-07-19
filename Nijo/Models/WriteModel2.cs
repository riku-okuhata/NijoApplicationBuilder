using Nijo.Core;
using Nijo.Models.WriteModel2Features;
using Nijo.Models.WriteModel2Features.ForRef;
using Nijo.Parts;
using Nijo.Parts.WebClient;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models {
    /// <summary>
    /// 登録・更新・削除される単位のデータ
    /// </summary>
    internal class WriteModel2 : IModel {
        void IModel.GenerateCode(CodeRenderingContext context, GraphNode<Aggregate> rootAggregate) {
            var allAggregates = rootAggregate.EnumerateThisAndDescendants();
            var aggregateFile = context.CoreLibrary.UseAggregateFile(rootAggregate);

            foreach (var agg in allAggregates) {
                // データ型: EFCore Entity
                var efCoreEntity = new EFCoreEntity(agg);
                aggregateFile.DataClassDeclaring.Add(efCoreEntity.Render(context));
                context.CoreLibrary.DbContextOnModelCreating.Add(efCoreEntity.RenderCallingOnModelCreating(context));

                // データ型: DataClassForSave
                var dataClassForSave = new DataClassForSave(agg, DataClassForSave.E_Type.UpdateOrDelete);
                aggregateFile.DataClassDeclaring.Add(dataClassForSave.RenderCSharp(context));
                aggregateFile.DataClassDeclaring.Add(dataClassForSave.RenderCSharpErrorStructure(context));
                aggregateFile.DataClassDeclaring.Add(dataClassForSave.RenderCSharpReadOnlyStructure(context));
                context.ReactProject.Types.Add(rootAggregate, dataClassForSave.RenderTypeScript(context));
                context.ReactProject.Types.Add(rootAggregate, dataClassForSave.RenderTypeScriptErrorStructure(context));
                context.ReactProject.Types.Add(rootAggregate, dataClassForSave.RenderTypeScriptReadOnlyStructure(context));

                // データ型: DataClassForNewItem
                var dataClassForNewItem = new DataClassForSave(agg, DataClassForSave.E_Type.Create);
                aggregateFile.DataClassDeclaring.Add(dataClassForNewItem.RenderCSharp(context));
                aggregateFile.DataClassDeclaring.Add(dataClassForNewItem.RenderCSharpErrorStructure(context));
                aggregateFile.DataClassDeclaring.Add(dataClassForNewItem.RenderCSharpReadOnlyStructure(context));
                context.ReactProject.Types.Add(rootAggregate, dataClassForNewItem.RenderTypeScript(context));
                context.ReactProject.Types.Add(rootAggregate, dataClassForNewItem.RenderTypeScriptErrorStructure(context));
                context.ReactProject.Types.Add(rootAggregate, dataClassForNewItem.RenderTypeScriptReadOnlyStructure(context));
            }

            // 処理: 新規作成処理 AppSrv
            // 処理: 更新処理 AppSrv
            // 処理: 削除処理 AppSrv
            var create = new CreateMethod(rootAggregate);
            var update = new UpdateMethod(rootAggregate);
            var delete = new DeleteMethod(rootAggregate);
            aggregateFile.AppServiceMethods.Add(create.Render(context));
            aggregateFile.AppServiceMethods.Add(update.Render(context));
            aggregateFile.AppServiceMethods.Add(delete.Render(context));

            // 処理: SetReadOnly AppSrv
            var setReadOnly = new SetReadOnly(rootAggregate);
            aggregateFile.AppServiceMethods.Add(setReadOnly.Render(context));

            // ---------------------------------------------
            // 他の集約から参照されるときのための部品

            foreach (var agg in allAggregates) {
                // データ型
                var forSave = new DataClassForSaveRefTarget(agg);
                var forDisplay = new DataClassForDisplayRefTarget(agg);
                aggregateFile.DataClassDeclaring.Add(forSave.RenderCSharp(context));
                aggregateFile.DataClassDeclaring.Add(forDisplay.RenderCSharp(context));
                context.ReactProject.Types.Add(rootAggregate, forSave.RenderTypeScript(context));
                context.ReactProject.Types.Add(rootAggregate, forDisplay.RenderTypeScript(context));

                // UI: コンボボックス
                // UI: 検索ダイアログ
                var comboBox = new SearchComboBox(agg);
                var searchDialog = new SearchDialog(agg);
                context.ReactProject.AutoGeneratedInput.Add(comboBox.Render(context));
                context.ReactProject.AutoGeneratedInput.Add(searchDialog.Render(context));

                // 処理: 参照先検索
                var searchRef = new SearchRefMethod(agg);
                context.ReactProject.AutoGeneratedHook.Add(searchRef.HookName, searchRef.RenderHook(context));
                aggregateFile.ControllerActions.Add(searchRef.RenderController(context));
                aggregateFile.AppServiceMethods.Add(searchRef.RenderAppSrvMethod(context));
            }
        }

        void IModel.GenerateCode(CodeRenderingContext context) {
            // 列挙体
            context.CoreLibrary.Enums.Add(DataClassForSave.RenderAddModDelEnum());

            // データ型: 一括コミット コンテキスト引数
            var batchUpdateContext = new BatchUpdateContext();
            context.CoreLibrary.UtilDir(utilDir => {
                utilDir.Generate(batchUpdateContext.Render());
            });

            // 処理: 一括コミット
            // 処理: 一括コミット AppSrv
            var batchUpdate = new BatchUpdate();
            context.ReactProject.AutoGeneratedHook.Add(batchUpdate.HookName, batchUpdate.RenderHook(context));
            context.WebApiProject.ControllerDir(dir => {
                dir.Generate(batchUpdate.RenderController());
            });
            context.CoreLibrary.AppSrvMethods.Add(batchUpdate.RenderAppSrvMethod(context));

            // 処理: デバッグ用ダミーデータ作成関数
            var dummyDataGenerator = new DummyDataGenerator();
            context.ReactProject.UtilDir(utilDir => {
                utilDir.Generate(dummyDataGenerator.Render());
            });
        }
    }
}
