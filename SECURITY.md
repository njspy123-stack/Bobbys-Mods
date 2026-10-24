# Security Policy

## Supported versions
Security reports are accepted for the latest published release (currently 0.7.1) and the main branch. Older releases may need an upgrade.

## Reporting a vulnerability
Use GitHub Security → Advisories → Report a vulnerability to report privately. Include the affected version, impact and minimal reproduction steps. Remove account details, tokens and saved lobby profiles. Do not post sensitive exploit details in public issues. Ordinary gameplay bugs, including the known Noclip issues, can use Issues; report any associated security impact privately. No response deadline or bounty is promised.

## Scope and security expectations
Covers this plugin's source, configuration parsing, file writes, build workflows and release packaging. Reports of unintended code execution, credential exposure or unauthorized file access are relevant. Treat config, room names and network data as untrusted. Changes must remain scoped to the local player and restore modified state when disabled. Game updates and third-party loaders can affect compatibility; automated checks do not prove the plugin secure. No finding classes are categorically excluded.
