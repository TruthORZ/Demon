# DEMON-GITHUB-20261101: GitHub repository setup
Status: in progress
Updated: 2026-11-01 Asia/Shanghai

## Scope and acceptance
User requested D:/MyProject/Demon as the persistent local repository and upload to their GitHub. This directory had no .git; create initial main history without claiming the old machine's commits. Preserve Assets and metadata, Packages, ProjectSettings, and Docs. Keep existing cache exclusions.
Acceptance: initial commit; successful LFS upload; remote main equals local HEAD; origin and upstream configured for future updates in this directory.

## Implementation
Unity: 6000.6.0f1 (f7f8ed4d1e24). Git identity follows the user's Snow repository. Install local LFS hook. Preserve existing text and native navigation binary rules; add LFS for binary source textures, models and audio. Keep scenes, prefabs and metadata as text.
Source files total approximately 3.96 GB, including two TIFFs above 100 MB. Exclude Library, Temp, Logs, UserSettings and build caches. No gameplay, scene, package or project-setting changes. Upload validation is not gameplay or dual-machine validation.

## Resume
Check git status and git remote -v in this same directory; resume upload without reinitializing, force-pushing, or deleting project files.
## Remote and update commands
User-selected existing public repository: https://github.com/TruthORZ/Demon
SSH origin: git@github.com:TruthORZ/Demon.git
Local working directory: D:/MyProject/Demon
After saving Unity changes: git status; git add -A; git commit -m <description>; git push.
Before receiving others' changes, commit local work, then git pull --ff-only and git lfs pull. If fast-forward fails, inspect differences; do not force-push or reset.

Prepared index: 2,626 files; 714 LFS paths totaling 3,900,613,883 bytes; no ordinary Git blob above 100 MiB, and no tracked Library/Temp/Logs/UserSettings entries. Remote created private under the authenticated nhaichen6-droid account. Initial upload pending.

## Existing repository selected
User supplied git@github.com:TruthORZ/Demon.git. Authenticated account has push access; repository is public. Fetched main at 213cfd751e766d5d6df696cb8c484240043432ac. Compared all existing paths: no deleted remote paths; original gameplay code, Packages and ProjectSettings match. Seven audio files and the URP icon have identical original bytes and now use LFS. Existing local URP GlobalSettings m_RuntimeSettings list is empty versus the old remote list; this predates this Git task and is preserved. Existing documentation additions preserve remote historical text.
Connected the two histories with a merge preserving the current local snapshot after this comparison; upstream history remains reachable and no force push is needed. The earlier private nhaichen6-droid/Demon repository remains, but its upload was stopped before main publication. Only TruthORZ/Demon is now origin. Upload pending.
