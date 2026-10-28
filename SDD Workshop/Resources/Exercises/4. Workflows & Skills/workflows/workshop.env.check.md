---
title: Workshop Env Check
description: A workflow that checks the environment for the workshop
---
# Description
Quickly verify that the local project environment is ready for the SDD with Windsurf and Spec Kit workshop.

# Steps
1. Check Git availability.
   - Run:
     ```bash
     git --version
     ```
   - Report the detected version.
2. Check Node.js and npm availability.
   - Run:
     ```bash
     node --version
     npm --version
     ```
   - Report both versions.
3. Check whether the project can start locally.
   - Run:
     ```bash
     npm run dev
     ```
   - Tell the user to verify the app at:
     ```text
     http://localhost:5173/
     ```
