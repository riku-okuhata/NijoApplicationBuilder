using Nijo.Core;
using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Architecture.WebServer {
    public class ApplicationService {
        public string ClassName => $"AutoGeneratedApplicationService";
        internal string FileName => $"{ClassName}.cs";

        public string ConcreteClass => $"OverridedApplicationService";
        internal string ConcreteClassFileName => $"{ConcreteClass}.cs";

        public string ServiceProvider = "ServiceProvider";
        public string DbContext = "DbContext";
        public string CurrentTime = "CurrentTime";

        internal SourceFile Render(ICodeRenderingContext ctx) => new SourceFile {
            FileName = FileName,
            RenderContent = () => $$"""
                namespace {{ctx.Config.RootNamespace}} {
                    using {{ctx.Config.DbContextNamespace}};

                    public partial class {{ClassName}} {
                        public {{ClassName}}(IServiceProvider serviceProvider) {
                            {{ServiceProvider}} = serviceProvider;
                        }

                        public IServiceProvider {{ServiceProvider}} { get; }

                        private {{ctx.Config.DbContextName}}? _dbContext;
                        public virtual {{ctx.Config.DbContextName}} {{DbContext}} => _dbContext ??= {{ServiceProvider}}.GetRequiredService<{{ctx.Config.DbContextName}}>();

                        private DateTime? _currentTime;
                        public virtual DateTime {{CurrentTime}} => _currentTime ??= DateTime.Now;
                    }
                }
                """,
        };
    }
}
