# Audit-only afterFileEdit hook. NEVER blocks (always exits 0 and emits {}).
# Logs file edits that land OUTSIDE agents-wip/ to agents-wip/audit.log,
# tagging whether a subagent was active at the time of the edit.
$ErrorActionPreference = 'SilentlyContinue'

try { $raw = [Console]::In.ReadToEnd() } catch { $raw = '' }
$data = $null
if ($raw) { try { $data = $raw | ConvertFrom-Json } catch { $data = $null } }

# Resolve the edited file path from the documented field, with fallbacks.
$path = $null
if ($data) {
  if ($data.file_path) { $path = $data.file_path }
  elseif ($data.tool_input -and $data.tool_input.file_path) { $path = $data.tool_input.file_path }
  elseif ($data.tool_input -and $data.tool_input.path) { $path = $data.tool_input.path }
}

if (-not $path) { Write-Output '{}'; exit 0 }

$root = (Get-Location).Path
$wip = Join-Path $root 'agents-wip'
$wipPrefix = $wip + [System.IO.Path]::DirectorySeparatorChar

try { $full = [System.IO.Path]::GetFullPath($path) } catch { $full = $path }

# Inside the scratchpad -> nothing to audit.
if ($full.StartsWith($wipPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
  Write-Output '{}'; exit 0
}

New-Item -ItemType Directory -Force -Path $wip | Out-Null

$marker = Join-Path $wip '.active-subagents'
$active = ''
if (Test-Path $marker) { $active = ((Get-Content $marker -Raw) -replace "\s+", ' ').Trim() }

$ts = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
if ($active) { $tag = "subagent-active:$active" } else { $tag = 'main' }
$line = "$ts [$tag] $full"

Add-Content -Path (Join-Path $wip 'audit.log') -Value $line

Write-Output '{}'
exit 0
