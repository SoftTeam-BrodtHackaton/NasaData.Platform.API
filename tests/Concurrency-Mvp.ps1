param([string]$BaseUrl = 'http://localhost:8000')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$mission = Invoke-RestMethod "$BaseUrl/api/missions/generate" -Method Post -ContentType application/json -Body '{"eventId":"DEMO-FLR-001","type":"SOLAR_STORM","difficulty":"EASY"}'
$correct = ($mission.question.options | Where-Object text -eq $mission.scientificData.classType).id
$wrong = ($mission.question.options | Where-Object id -ne $correct | Select-Object -First 1).id
$client = New-Object System.Net.Http.HttpClient
try {
    foreach ($existing in @($false, $true)) {
        $student = 'CONCURRENT-' + [guid]::NewGuid().ToString('N')
        $url = "$BaseUrl/api/students/$student/missions/$($mission.id)/answer"
        if ($existing) {
            Invoke-RestMethod $url -Method Post -ContentType application/json -Body (@{optionId=$wrong} | ConvertTo-Json) | Out-Null
        }
        $pending = @()
        for ($i=0; $i -lt 12; $i++) {
            $content = New-Object System.Net.Http.StringContent ((@{optionId=$correct} | ConvertTo-Json), [System.Text.Encoding]::UTF8, 'application/json')
            $pending += $client.PostAsync($url, $content)
        }
        $points = 0
        $conflicts = 0
        foreach ($task in $pending) {
            $response = $task.GetAwaiter().GetResult()
            try {
                if ([int]$response.StatusCode -eq 409) { $conflicts++; continue }
                if ([int]$response.StatusCode -ne 200) { throw "Unexpected status: $($response.StatusCode)" }
                $result = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
                $points += $result.pointsEarned
            } finally { $response.Dispose() }
        }
        $progress = Invoke-RestMethod "$BaseUrl/api/students/$student/progress"
        $profile = Invoke-RestMethod "$BaseUrl/api/students/$student/profile"
        if ($points -ne 100 -or $progress.totalPoints -ne 100 -or $progress.completedMissions -ne 1 -or $profile.skills.astronomy -ne 2) {
            throw 'Concurrent submissions duplicated or lost rewards.'
        }
        Write-Output "PASS 12 concurrent answers; existing student=$existing; earned=100; completed=1; conflicts=$conflicts"
    }
} finally { $client.Dispose() }
