const root = "C:/Users/20135/.zcode/cli/plugins/cache/zcode-plugins-official/computer-use/0.6.4";
const { join } = await import("node:path");
const { pathToFileURL } = await import("node:url");
const { setupComputerUseRuntime } = await import(pathToFileURL(join(root, "scripts", "computer-use-client.mjs")).href);
await setupComputerUseRuntime({ globals: globalThis });
const app = await agent.computerUse.getApp({name: "Among Us"});
await app.getScreenshot();
