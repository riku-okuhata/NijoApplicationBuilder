using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.IntegrationTest.Tests {
    partial class 観点 {

        [UseDataPatterns]
        public async Task Webから追加更新削除(DataPattern pattern) {

            if (pattern.Name != DataPattern.FILENAME_001) {
                Assert.Warn($"期待結果が定義されていません: {pattern.Name}");
                return;
            }

            using var ct = new CancellationTokenSource();
            Task? debugTask = null;
            try {
                debugTask = await TestProject.LaunchWebApiAndClient(pattern, ct.Token);

                using var driver = TestProject.CreateWebDriver();

                // トップページ
                driver.FindElement(Util.ByInnerText("参照先")).Click();

                // 参照先: MultiView
                driver.FindElement(Util.ByInnerText("新規作成")).Click();

                // 参照先: CreateView
                driver.FindElement(By.Name("参照先集約ID")).SendKeys("あ");
                driver.FindElement(By.Name("参照先集約名")).SendKeys("い");
                driver.FindElement(Util.ByInnerText("保存")).Click();

                // 参照先: SingleView

                // 参照元: MultiView
                // 参照元: CreateView
                // 参照元: SingleView

            } finally {
                ct.Cancel();
                debugTask?.Wait();
            }
        }
    }
}
