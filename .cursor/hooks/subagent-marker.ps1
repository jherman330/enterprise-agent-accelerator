# Maintains a ref-counted marker of active subagents so the audit hook can tag
# edits that occur during a subagent window. NEVER blocks (exits 0, emits {}).
param([string]$Mode)
$ErrorActionPreference = 'SilentlyContinue'

try { $raw = [Console]::In.ReadToEnd() } catch { $raw = '' }
$data = $null
if ($raw) { try { $data = $raw | ConvertFrom-Json } catch { $data = $null } }

$type = ''
if ($data) {
  if ($data.subagent_type) { $type = $data.subagent_type }
  elseif ($data.subagentType) { $type = $data.subagentType }
}

$root = (Get-Location).Path
$wip = Join-Path $root 'agents-wip'
New-Item -ItemType Directory -Force -Path $wip | Out-Null
$marker = Join-Path $wip '.active-subagents'
$ts = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')

if ($Mode -eq 'start') {
  $entry = $ts
  if ($type) { $entry = "$ts/$type" }
  Add-Content -Path $marker -Value $entry
}
elseif ($Mode -eq 'stop') {
  if (Test-Path $marker) {
    $lines = @(Get-Content $marker)
    if ($lines.Count -le 1) { Remove-Item $marker -Force }
    else { $lines[0..($lines.Count - 2)] | Set-Content $marker }
  }
}

Write-Output '{}'
exit 0
