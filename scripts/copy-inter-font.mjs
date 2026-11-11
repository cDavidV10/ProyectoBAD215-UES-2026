import { cp, mkdir, readdir } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const projectRoot = join(dirname(fileURLToPath(import.meta.url)), "..");
const sourceDir = join(projectRoot, "node_modules", "@fontsource-variable", "inter", "files");
const targetDir = join(projectRoot, "BEMO-SistemaPlanilla", "BEMOWeb", "wwwroot", "css", "files");

await mkdir(targetDir, { recursive: true });

const fontFiles = (await readdir(sourceDir)).filter((file) => file.endsWith("-wght-normal.woff2"));

await Promise.all(
  fontFiles.map((file) => cp(join(sourceDir, file), join(targetDir, file), { force: true })),
);

console.log(`Copied ${fontFiles.length} Inter font files to ${targetDir}`);
