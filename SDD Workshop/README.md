# SK SDD Workshop

All the resources that will be used during the workshop.

## Setup & Exercise Resources

Setup document: [Resources/Workshop - SDD with Windsurf+SpecKit.docx](/Resources/Workshop%20-%20SDD%20with%20Windsurf+SpecKit.docx)

All the resources used in the exercises are located in [Resources](/Resources/) folder, grouped by the section's number and the name of the exercise. 

### Playwright Installation Issue

If ```npx playwright install chromium``` does not work even after the proposed solution in the document with uninstall/install, then just download the 4 packages explained below and extract it into ```%USERPROFILE%\AppData\Local\```.

***Note:** the Playwright installation (```npx playwright install chromium```) is just an extraction of 4 packages (or if you want you could download all these 4 files one by one and extract them into ```%USERPROFILE%\AppData\Local\ms-playwright```):*
1. **chromium-1223** downloaded from: https://cdn.playwright.dev/builds/cft/148.0.7778.96/win64/chrome-win64.zip
2. **ffmpeg-1011** downloaded from: https://cdn.playwright.dev/dbazure/download/playwright/builds/ffmpeg/1011/ffmpeg-win64.zip
3. **chromium_headless_shell-1223** downloaded from: https://cdn.playwright.dev/builds/cft/148.0.7778.96/win64/chrome-headless-shell-win64.zip
4. **winldd-1007** downloaded from: https://cdn.playwright.dev/dbazure/download/playwright/builds/winldd/1007/winldd-win64.zip

### Workshop Exercise Repos

1. Windows workshop exercise project (SpecKit installed with ps scripts): [SDD Workshop Windows](/SDD%20Workshop%20Windows/)
2. Unix-like (MacOS, Linux, WSL) workshop exercise project (SpecKit installed with sh scripts): [SDD Workshop Unix](/SDD%20Workshop%20Unix/)
3. Workshop exercise project without installed SpecKit: [SDD Workshop Empty](/SDD%20Workshop%20Empty/)

## Trainers Resources

All the trainers resources are located in [Resources/Trainers](/Resources/Trainers/) folder. There you could find:
1. The detailed guide: [Workshop Trainers Guide - SDD with Windsurf+SpecKit.docx](/Resources/Trainers/Workshop%20Trainers%20Guide%20-%20SDD%20with%20Windsurf+SpecKit.docx)
2. The slides/presentation: [Workshop Presentation - SDD with Windsurf+SpecKit.pptx](/Resources/Trainers/Workshop%20Presentation%20-%20SDD%20with%20Windsurf+SpecKit.pptx)