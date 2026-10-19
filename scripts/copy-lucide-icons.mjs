import { copyFile, mkdir } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const sourceDirectory = resolve(repositoryRoot, "node_modules/lucide-static/icons");
const destinationDirectory = resolve(repositoryRoot, "MeetCampus/wwwroot/icons");
const iconNames = ["globe", "menu", "moon", "user-round"];

await mkdir(destinationDirectory, { recursive: true });

for (const iconName of iconNames) {
    await copyFile(
        resolve(sourceDirectory, `${iconName}.svg`),
        resolve(destinationDirectory, `${iconName}.svg`),
    );
}
