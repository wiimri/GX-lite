const path = require("path");
const fs = require("fs");
const { chromium } = require("playwright");

const root = path.resolve(__dirname, "..");
const preview = "file:///" + path.join(root, "docs", "ui-preview.html").replace(/\\/g, "/");
const output = path.join(root, "screenshots");
fs.mkdirSync(output, { recursive: true });

function readYouTubeShieldsScript() {
  const source = fs.readFileSync(path.join(root, "src", "BrowserForm.cs"), "utf8");
  const start = source.indexOf("private static string YouTubeShieldsScript()");
  if (start < 0) {
    throw new Error("Could not find YouTubeShieldsScript in BrowserForm.cs");
  }
  const body = source.slice(start);
  const match = body.match(/return @"([\s\S]*?)";\r?\n\s*}/);
  if (!match) {
    throw new Error("Could not extract YouTubeShieldsScript from BrowserForm.cs");
  }
  return match[1].replace(/\"\"/g, "\"");
}

async function main() {
  const browser = await chromium.launch({ channel: "msedge", headless: true });
  const cases = [
    { name: "desktop", width: 1280, height: 720 },
    { name: "compact", width: 760, height: 540 }
  ];

  const results = [];
  for (const testCase of cases) {
    const page = await browser.newPage({
      viewport: { width: testCase.width, height: testCase.height },
      deviceScaleFactor: 1
    });
    await page.goto(preview);

    const report = await page.evaluate(() => {
      const failures = [];
      const doc = document.documentElement;
      if (doc.scrollWidth > window.innerWidth) {
        failures.push(`horizontal overflow: ${doc.scrollWidth}px > ${window.innerWidth}px`);
      }

      const selectors = [".shell", ".top", ".nav", ".address", ".content", ".status"];
      for (const selector of selectors) {
        const node = document.querySelector(selector);
        if (!node) {
          failures.push(`missing ${selector}`);
          continue;
        }
        const rect = node.getBoundingClientRect();
        if (rect.left < -1 || rect.right > window.innerWidth + 1) {
          failures.push(`${selector} out of horizontal viewport`);
        }
        if (rect.top < -1 || rect.bottom > window.innerHeight + 1) {
          failures.push(`${selector} out of vertical viewport`);
        }
      }

      return {
        failures,
        scrollWidth: doc.scrollWidth,
        width: window.innerWidth,
        height: window.innerHeight
      };
    });

    await page.screenshot({
      path: path.join(output, `ui-preview-${testCase.name}.png`),
      fullPage: true
    });

    if (testCase.name === "desktop") {
      await page.locator(".island-bar").click();
      const visibleIslandMembers = await page.locator(".island-member:visible").count();
      if (visibleIslandMembers !== 2) {
        report.failures.push(`collapsed island did not expand: ${visibleIslandMembers}`);
      }

      await page.locator('.shortcut[data-title="YouTube"]').click({ button: "middle" });
      const afterMiddle = await page.locator(".tab[data-tab]").count();
      if (afterMiddle !== 5) {
        report.failures.push(`middle-click shortcut did not create a tab: ${afterMiddle}`);
      }

      await page.locator('.tab[data-tab="Google"] .close').click();
      const hasGoogle = await page.locator('.tab[data-tab="Google"]').count();
      if (hasGoogle !== 0) {
        report.failures.push("tab close glyph did not close Google tab");
      }

      await page.locator('.tab[data-tab="YouTube"]').first().click({ button: "middle" });
      const youtubeCount = await page.locator('.tab[data-tab="YouTube"]').count();
      if (youtubeCount !== 1) {
        report.failures.push(`middle-click tab close removed wrong count: ${youtubeCount}`);
      }

      await page.locator('.favorite[data-title="GitHub"]').click({ button: "middle" });
      const githubCount = await page.locator('.tab[data-tab="GitHub"]').count();
      if (githubCount !== 1) {
        report.failures.push(`middle-click favorite did not create a tab: ${githubCount}`);
      }

      await page.locator(".menu-button").click();
      const menuVisible = await page.locator(".menu-preview").isVisible();
      const menuText = await page.locator(".menu-preview").innerText();
      if (!menuVisible || menuText.indexOf("History") < 0 || menuText.indexOf("Downloads") < 0 || menuText.indexOf("Bookmarks") < 0 || menuText.indexOf("Extensions") < 0) {
        report.failures.push("main menu did not expose required sections");
      }
    }

    await page.close();
    results.push({ ...testCase, ...report });
  }

  const youtubePage = await browser.newPage();
  await youtubePage.route("https://www.youtube.com/mock-ad-page", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "text/html",
      body: `<!doctype html><html><body>
        <div class="html5-video-player ad-showing"><video></video></div>
        <div id="player-ads">ad</div>
        <ytd-promoted-video-renderer>promoted</ytd-promoted-video-renderer>
        <div class="ytp-ad-overlay-container">overlay</div>
        <button class="ytp-ad-skip-button" onclick="document.body.dataset.skip='yes'">Skip</button>
      </body></html>`
    });
  });
  await youtubePage.addInitScript(readYouTubeShieldsScript());
  await youtubePage.goto("https://www.youtube.com/mock-ad-page");
  await youtubePage.waitForTimeout(800);
  const youtubeReport = await youtubePage.evaluate(() => ({
    name: "youtube-shields",
    width: window.innerWidth,
    height: window.innerHeight,
    failures: [
      document.body.dataset.skip === "yes" ? null : "skip button was not clicked",
      document.querySelector("#player-ads") ? "player ad container was not removed" : null,
      document.querySelector("ytd-promoted-video-renderer") ? "promoted video renderer was not removed" : null,
      document.querySelector(".ytp-ad-overlay-container") ? "ad overlay was not removed" : null
    ].filter(Boolean)
  }));
  await youtubePage.close();
  results.push(youtubeReport);

  const nonYouTubePage = await browser.newPage();
  const nonYouTubeErrors = [];
  nonYouTubePage.on("pageerror", (error) => nonYouTubeErrors.push(error.message));
  await nonYouTubePage.route("https://www.crunchyroll.com/mock-page", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "text/html",
      body: "<!doctype html><html><body><main>Crunchyroll compatibility mock</main></body></html>"
    });
  });
  await nonYouTubePage.addInitScript(readYouTubeShieldsScript());
  await nonYouTubePage.goto("https://www.crunchyroll.com/mock-page");
  const nonYouTubeReport = await nonYouTubePage.evaluate((errors) => ({
    name: "non-youtube-isolation",
    width: window.innerWidth,
    height: window.innerHeight,
    failures: [
      window.__gxLightYouTubeShieldsInstalled ? "YouTube Shields was installed outside YouTube" : null,
      errors.length ? `page errors: ${errors.join("; ")}` : null
    ].filter(Boolean)
  }), nonYouTubeErrors);
  await nonYouTubePage.close();
  results.push(nonYouTubeReport);

  await browser.close();

  const failed = results.filter((result) => result.failures.length > 0);
  console.log(JSON.stringify(results, null, 2));
  if (failed.length > 0) {
    process.exitCode = 1;
  }
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
