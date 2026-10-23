# Build-based security scanning

C# is analyzed locally against a legally installed Gorilla Tag/BepInEx copy. Hosted Actions scanning remains automated for workflow files on relevant pushes/pull requests, weekly and on demand. C# scans must be rerun locally when C# code changes; they do not run on GitHub-hosted machines without the game dependencies.

## Run on this PC

Use PowerShell from the Bobbys Mods Gorila Tag project folder:

    .\Scan-Security.ps1 -Upload

The script uses the verified portable tools in .security-tools and SDK in .dotnet. Elsewhere, provide -CodeQLPath, -DotnetPath, -GhPath and -GamePath. Install the official CodeQL bundle (including queries) and GitHub CLI from GitHub's official releases. CodeQL 2.27.3 was used for initial validation. Sign GitHub CLI into njspy123-stack with security_events permission before uploading. Never put tokens in scripts, config files or issues.

The script clones main into a separate local directory, records the exact commit, rebuilds both plugin and tests with compiler tracing, runs the C# security-extended suite and optionally uploads the SARIF result. No game assemblies, CodeQL database or source archive are uploaded. Local scanner artifacts and tools are excluded by .gitignore. A scan does not rebuild or install the active game plugin.

## Initial verified result

Commit b70f512eb4a417c24e486b4d8d4831c766bcd328: 15/15 C# files scanned; zero security findings. Both builds completed with zero warnings/errors. GitHub accepted the manual-build C# analysis with no analysis warning. The old low-quality no-build default setup was replaced; alerts and secret/dependency protection remain enabled. Later commits at setup time changed only scanning configuration/documentation, not the analyzed C# files.

Read results under GitHub Security and quality → Code scanning. Passing scans cannot prove the absence of vulnerabilities. Unity/game-specific runtime behavior and third-party binary internals are not fully modeled by CodeQL.
