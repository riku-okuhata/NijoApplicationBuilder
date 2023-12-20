using Nijo.Core;
using Nijo.DotnetEx;
using Nijo.Features.WebClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Features {
    public sealed class Infrastucture : ISourceFileUsedByMultiFeature {
        public List<Func<string, string>> ConfigureServices { get; } = new List<Func<string, string>>();
        public List<Func<string, string>> ConfigureServicesWhenWebServer { get; } = new List<Func<string, string>>();
        public List<Func<string, string>> ConfigureServicesWhenBatchProcess { get; } = new List<Func<string, string>>();
        public List<Func<string, string>> ConfigureApp { get; } = new List<Func<string, string>>();
        public List<Func<string, string>> OnModelCreating { get; } = new List<Func<string, string>>();

        void ISourceFileUsedByMultiFeature.GenerateSourceFile(ICodeRenderingContext context) {
            context.EditWebApiDirectory(genDir => {
                genDir.Generate(Configure.Render(this));
                genDir.Generate(EnumDefs.Render());
                genDir.Generate(new ApplicationService(context.Config).Render());

                genDir.Directory("Util", utilDir => {
                    utilDir.Generate(Util.RuntimeSettings.Render());
                    utilDir.Generate(Util.DotnetExtensions.Render());
                    utilDir.Generate(Util.FromTo.Render());
                    utilDir.Generate(Util.Utility.RenderJsonConversionMethods());
                    utilDir.Generate(Logging.HttpResponseExceptionFilter.Render());
                    utilDir.Generate(Logging.DefaultLogger.Render());
                });
                genDir.Directory("Web", controllerDir => {
                    controllerDir.Generate(Searching.MultiView.RenderCSharpSearchConditionBaseClass());
                    controllerDir.Generate(WebClient.DebuggerController.Render());
                });
                genDir.Directory("EntityFramework", efDir => {
                    efDir.Generate(new DbContextClass(context.Config).RenderDeclaring(this));
                });
            });

            context.EditReactDirectory(reactDir => {
                var reactProjectTemplate = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "ApplicationTemplates", "REACT_AND_WEBAPI", "react");

                const string REACT_PAGE_DIR = "pages";
                string GetAggDirName(GraphNode<Aggregate> a) => a.Item.DisplayName.ToFileNameSafe();

                reactDir.CopyFrom(Path.Combine(reactProjectTemplate, "src", "__autoGenerated", "nijo.css"));
                reactDir.Generate(index.Render(a => $"{REACT_PAGE_DIR}/{GetAggDirName(a)}"));
                reactDir.Generate(types.Render());

                reactDir.Directory("application", reactApplicationDir => {
                    var source = Path.Combine(reactProjectTemplate, "src", "__autoGenerated", "application");
                    foreach (var file in Directory.GetFiles(source)) reactApplicationDir.CopyFrom(file);
                });
                reactDir.Directory("decoration", decorationDir => {
                    var source = Path.Combine(reactProjectTemplate, "src", "__autoGenerated", "decoration");
                    foreach (var file in Directory.GetFiles(source)) decorationDir.CopyFrom(file);
                });
                reactDir.Directory("layout", layoutDir => {
                    var source = Path.Combine(reactProjectTemplate, "src", "__autoGenerated", "layout");
                    foreach (var file in Directory.GetFiles(source)) layoutDir.CopyFrom(file);
                });
                reactDir.Directory("user-input", userInputDir => {
                    var source = Path.Combine(reactProjectTemplate, "src", "__autoGenerated", "user-input");
                    foreach (var file in Directory.GetFiles(source)) userInputDir.CopyFrom(file);

                    // TODO: どの集約がコンボボックスを作るのかをNijoFeatureBaseに主導権握らせたい
                    userInputDir.Generate(KeywordSearching.ComboBox.RenderDeclaringFile());
                });
                reactDir.Directory("util", reactUtilDir => {
                    var source = Path.Combine(reactProjectTemplate, "src", "__autoGenerated", "util");
                    foreach (var file in Directory.GetFiles(source)) reactUtilDir.CopyFrom(file);
                    reactUtilDir.Generate(Util.DummyDataGenerator.Render());
                });
            });
        }
    }
}
