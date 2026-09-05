param([string]$BaseUrl = 'http://localhost:8000')
$ErrorActionPreference = 'Stop'
$script:checks = 0
function Check($condition, $name) {
    if (-not $condition) { throw "FAILED: $name" }
    $script:checks++
    Write-Output "PASS $name"
}
function Post($path, $body) {
    Invoke-RestMethod "$BaseUrl$path" -Method Post -ContentType 'application/json' -Body ($body | ConvertTo-Json)
}
function Status($path, $method, $body, $expected) {
    $actual = 200
    try {
        $args = @{Uri="$BaseUrl$path"; Method=$method; UseBasicParsing=$true}
        if ($null -ne $body) { $args.ContentType='application/json'; $args.Body=($body | ConvertTo-Json) }
        $actual = (Invoke-WebRequest @args).StatusCode
    } catch {
        if ($null -eq $_.Exception.Response) { throw }
        $actual = [int]$_.Exception.Response.StatusCode
    }
    Check ($actual -eq $expected) "$method $path => $expected"
}
$student = 'SMOKE-' + [guid]::NewGuid().ToString('N')
$missions = Invoke-RestMethod "$BaseUrl/api/missions"
Check ($missions.Count -ge 2) 'Mission cards seeded'
$solar = Post '/api/missions/generate' @{eventId='DEMO-FLR-001';type='SOLAR_STORM';difficulty='EASY'}
$planetary = Post '/api/missions/generate' @{eventId='DEMO-NEO-001';type='PLANETARY_DEFENSE';difficulty='MEDIUM'}
Check ($solar.type -eq 'SOLAR_STORM' -and $planetary.type -eq 'PLANETARY_DEFENSE') 'Both generators'
$again = Post '/api/missions/generate' @{eventId='DEMO-FLR-001';type='SOLAR_STORM';difficulty='EASY'}
Check ($solar.id -eq $again.id) 'Generation is idempotent'
$filtered = Invoke-RestMethod "$BaseUrl/api/missions?type=SOLAR_STORM"
Check (@($filtered | Where-Object type -ne 'SOLAR_STORM').Count -eq 0) 'Type filter'
$detail = Invoke-RestMethod "$BaseUrl/api/missions/$($solar.id)"
$json = $detail | ConvertTo-Json -Depth 10
Check ($json -notmatch '(?i)correctOption|isCorrect|skillWeights') 'HTTP detail has no answer metadata'
Check (-not $detail.source.realData) 'Fixture explicitly simulated'
$correct = ($detail.question.options | Where-Object text -eq $detail.scientificData.classType).id
$wrong = ($detail.question.options | Where-Object id -ne $correct | Select-Object -First 1).id
$path = "/api/students/$student/missions/$($solar.id)/answer"
$answer = Post $path @{optionId=$wrong}
Check (-not $answer.correct -and $answer.pointsEarned -eq 0) 'Wrong earns 0'
$answer = Post $path @{optionId=$correct}
Check ($answer.correct -and $answer.pointsEarned -eq 100) 'First correct earns 100'
$answer = Post $path @{optionId=$correct}
Check ($answer.correct -and $answer.pointsEarned -eq 0) 'Repeated correct earns 0'
Check (($answer | ConvertTo-Json) -notmatch '(?i)correctOption|isCorrect') 'Answer does not disclose key'
$planetaryCorrect = ($planetary.question.options | Where-Object text -eq $planetary.scientificData.objectName).id
$answer = Post "/api/students/$student/missions/$($planetary.id)/answer" @{optionId=$planetaryCorrect}
Check ($answer.pointsEarned -eq 100) 'Planetary answer earns 100'
$progress = Invoke-RestMethod "$BaseUrl/api/students/$student/progress"
Check ($progress.completedMissions -eq 2 -and $progress.totalPoints -eq 200 -and $progress.level -eq 'SPACE_EXPLORER') 'Persistent progress across requests'
$profile = Invoke-RestMethod "$BaseUrl/api/students/$student/profile"
Check ($profile.skills.astronomy -eq 4 -and $profile.skills.physics -eq 3 -and $profile.skills.dataAnalysis -eq 3 -and $profile.skills.programming -eq 0) 'Only first completions contribute skills'
Status '/api/missions?type=INVALID' Get $null 400
Status '/api/missions?type=0' Get $null 400
Status '/api/missions/missing' Get $null 404
Status '/api/missions/generate' Post @{eventId='';type='SOLAR_STORM';difficulty='EASY'} 400
Status '/api/missions/generate' Post @{eventId='DEMO-FLR-001';type='INVALID';difficulty='EASY'} 400
Status '/api/missions/generate' Post @{eventId='DEMO-FLR-001';type='SOLAR_STORM';difficulty='INVALID'} 400
Status '/api/missions/generate' Post @{eventId='DEMO-FLR-001';type=0;difficulty='EASY'} 400
Status '/api/missions/generate' Post @{eventId='DEMO-FLR-001'} 400
Status '/api/missions/generate' Post @{eventId='missing';type='SOLAR_STORM';difficulty='EASY'} 404
Status $path Post @{optionId=''} 400
Status $path Post @{optionId='Z'} 400
Status "/api/students/$student/missions/missing/answer" Post @{optionId='A'} 404
Status '/api/students/missing/progress' Get $null 404
Status '/api/students/missing/profile' Get $null 404
$swagger = Invoke-RestMethod "$BaseUrl/swagger/v1/swagger.json"
Check (@($swagger.paths.PSObject.Properties).Count -eq 6) 'All six route templates in Swagger'
Check (($swagger | ConvertTo-Json -Depth 50) -notmatch '(?i)correctOptionId') 'Swagger contains no secret answer schema'
$cors = Invoke-WebRequest "$BaseUrl/api/missions" -UseBasicParsing -Method Options -Headers @{Origin='http://localhost:5173';'Access-Control-Request-Method'='GET'}
Check ($cors.Headers['Access-Control-Allow-Origin'] -eq '*') 'Existing CORS policy applied'
Write-Output "$script:checks HTTP checks passed. Student: $student"
