# NASA Data Platform — MVP educativo

Backend sobre la solución existente `nasa-data-platform.sln`, proyecto único `NasaDataPlatform.API`, .NET 9, Controllers y EF Core/MySQL. Solo hay dos bounded contexts: Missions y Students, ambos con Application, Domain, Infrastructure e Interfaces. No se añadieron paquetes ni otra base de datos.

## Estado inicial y alcance

Missions y Students estaban vacíos. Se reutilizaron `IBaseRepository`, `IUnitOfWork`, `BaseRepository`, `UnitOfWork`, `AppDbContext`, la convención de rutas, Swagger y la política CORS. La infraestructura real está en `Shared/Infrastructure`, y se conserva allí. Había cambios locales anteriores a esta implementación que se preservaron.

**No existe extracción NASA/DONKI/JPL en este checkout.** No fue posible conectar código del compañero que todavía no está aquí. No se inventaron URLs ni se reimplementó esa extracción. La demostración utiliza dos fixtures de desarrollo en `appsettings.Development.json`, siempre con `source.realData=false` y `system=EDUCATIONAL_FIXTURE`.

El proyecto de referencia se consultó únicamente para distribución de carpetas, composición y convenciones; no se modificó ni se copió su dominio.

## Ejecutar

Desde la raíz, con .NET SDK 9 y MySQL disponible:

```powershell
dotnet restore nasa-data-platform.sln
dotnet build nasa-data-platform.sln --no-restore
dotnet run --project NasaDataPlatform.API/NasaDataPlatform.API.csproj --no-build --launch-profile http
```

El perfil `http` existente escucha en **http://localhost:8000**. Swagger: http://localhost:8000/swagger.

La conexión local existente se conservó en `NasaDataPlatform.API/appsettings.Local.json`, ignorado por Git y excluido de publicación. Se retiró del archivo versionado y se desactivó el logging de valores sensibles de EF. En otra máquina configura `ConnectionStrings__DefaultConnection` mediante variables de entorno, o un `appsettings.Local.json` privado en Development. No subas credenciales. Las variables de entorno y argumentos prevalecen sobre el archivo local. En producción ese archivo no se carga.

La base MySQL utiliza el mismo `AppDbContext` y `EnsureCreatedAsync` del enfoque original. Se crean tablas `missions`, `students` y `mission_attempts` en una base vacía. **No hay migraciones**: `EnsureCreated` no actualiza esquemas existentes; una base con tablas previas incompatibles requiere una migración revisada. No se borra ni reinicia una base existente.

## Endpoints

| Método | Ruta | Resultado |
|---|---|---|
| GET | `/api/missions` | Tarjetas; filtro opcional `?type=SOLAR_STORM` o `PLANETARY_DEFENSE` |
| GET | `/api/missions/{id}` | Detalle educativo, opciones y procedencia |
| POST | `/api/missions/generate` | Generación determinística, persistencia, DTO público; 201 con Location |
| POST | `/api/students/{studentId}/missions/{missionId}/answer` | Evaluación y registro del intento |
| GET | `/api/students/{studentId}/progress` | Completadas, puntos y nivel |
| GET | `/api/students/{studentId}/profile` | Competencias y recomendaciones |

No hay endpoints CRUD adicionales ni endpoint `/source`. El detalle y Swagger nunca incluyen `CorrectOptionId`, `correctOption`, `isCorrect` ni pesos internos.

Tipos: `SOLAR_STORM`, `PLANETARY_DEFENSE`. Dificultades: `EASY`, `MEDIUM`, `HARD`; el MVP usa una plantilla por tipo, sin adaptación adicional por dificultad. Se rechazan valores desconocidos y enums numéricos en JSON. Los IDs públicos son strings; las claves enteras internas permiten conservar el repositorio base sin duplicarlo.

## Flujo e integración

`MissionsController → MissionService → IScientificEventPort → adapter → Mission.Generate → IMissionRepository → IUnitOfWork`.

`IScientificEventPort` es el punto para conectar la extracción del compañero: recibe `eventId` y tipo y devuelve datos normalizados `ScientificEvent`, o null si no existe el evento. Al incorporar la integración, implementar ese puerto en Missions/Infrastructure y sustituir su registro en `AddMissions`. `ScientificData` actualmente necesita fecha y clasificación solar o nombre del objeto. `MissionSource` conserva proveedor, sistema, identificador y si son datos reales. El Domain no depende de HTTP, EF ni JSON externo.

El adapter actual busca los fixtures configurados únicamente en Development cuando `Missions:EnableFixtures=true`. No sustituye silenciosamente un evento desconocido por uno simulado: devuelve 404. Con fixtures deshabilitados o fuera de Development, generar devuelve 503 hasta disponer de una integración. Las misiones ya persistidas siguen disponibles.

`StudentsController → StudentService → IMissionEvaluationPort → MissionEvaluationAdapter → MissionService.EvaluateAsync`.

Students recibe exclusivamente evaluación, feedback y contribuciones STEM; nunca la entidad Mission ni la clave correcta. No hay llamadas HTTP de la API hacia sí misma ni acceso científico desde Students. Los controladores convierten a DTOs explícitos.

## Reglas y persistencia

- El estudiante se registra al enviar su primera respuesta válida. No hay CRUD de estudiantes ni autenticación en este MVP; `studentId` es un identificador de demostración. Consultar un estudiante inexistente devuelve 404.
- `PointsPolicy.FirstCompletion` centraliza los 100 puntos. Una respuesta incorrecta vale 0. Cada misión solo premia la primera resolución correcta.
- Cada respuesta válida registra misión, estudiante, opción, acierto, puntos y fecha UTC. Una opción inexistente o vacía devuelve 400 sin crear intento.
- Intento, puntos, completadas y skills se guardan con una única llamada a `SaveChangesAsync`, transaccional en MySQL. `Revision` usa concurrencia optimista; una colisión devuelve 409 y puede reintentarse sin duplicar puntos. La clave única del estudiante protege también su creación simultánea.
- La generación devuelve el mismo ID para proveedor/sistema/evento/tipo/dificultad iguales. Una clave única evita duplicación por carreras; los conflictos de escritura devuelven 409.
- Niveles: 0–99 `SPACE_CADET`, 100–299 `SPACE_EXPLORER`, 300+ `MISSION_SPECIALIST`.
- Solar suma Astronomy 2, Physics 2, DataAnalysis 1; Planetary suma 2, 1, 2. Programming no suma en estas plantillas. Cada unidad representa un punto porcentual y se limita a 100; repetir una completada no cambia skills. Las tres áreas con valores positivos más altos determinan recomendaciones.
- Las misiones guardan sus objetos de valor como JSON interno en columnas MySQL; estudiantes e intentos están relacionados por FK. El JSON de persistencia no es el contrato HTTP.
- Manejo central de errores: 400 validación, 404 recurso/evento inexistente, 409 concurrencia/duplicados, 502 error HTTP externo, 503 integración no configurada y 500 inesperado sin detalles internos.

`Shared/Domain` y los repositorios base no se cambiaron. En Shared/Infrastructure se añadió manejo de excepciones y se registraron las configuraciones EF desde el assembly. Program conserva su composición original y añade `AddMissions`, `AddStudents`, UnitOfWork, serialización, seed de desarrollo y `UseCors` para activar la política ya existente. Swagger ahora identifica correctamente esta API.

## Demostración en PowerShell

```powershell
$base = 'http://localhost:8000'
Invoke-RestMethod "$base/api/missions"

$mission = Invoke-RestMethod "$base/api/missions/generate" -Method Post -ContentType 'application/json' -Body '{"eventId":"DEMO-FLR-001","type":"SOLAR_STORM","difficulty":"EASY"}'
Invoke-RestMethod "$base/api/missions/$($mission.id)"

# B corresponde a M5.2 únicamente en este fixture; no es una regla general.
Invoke-RestMethod "$base/api/students/USER-001/missions/$($mission.id)/answer" -Method Post -ContentType 'application/json' -Body '{"optionId":"B"}'
Invoke-RestMethod "$base/api/students/USER-001/progress"
Invoke-RestMethod "$base/api/students/USER-001/profile"
```

Si USER-001 ya completó esa misión, recibirá 0 puntos adicionales. También hay solicitudes listas en `NasaDataPlatform.API/NasaDataPlatform.API.http` para Rider.

## Verificación

No había proyectos ni framework de tests. `dotnet test nasa-data-platform.sln --no-restore` termina sin tests descubiertos. Para mantener una sola solución y un solo proyecto se añadieron comprobaciones sin dependencias:

```powershell
# Después del build; compila el ejecutable de comprobación con Roslyn del SDK en obj/MvpChecks.
./tests/Run-DomainChecks.ps1
# Con el servidor iniciado y fixtures habilitados:
./tests/Smoke-Mvp.ps1
./tests/Concurrency-Mvp.ps1
```

Resultados comprobados: build sin errores ni advertencias; 14 comprobaciones de dominio, 30 comprobaciones HTTP y dos escenarios de 12 respuestas concurrentes. Se verificó persistencia tras reiniciar el proceso. Las pruebas HTTP crean estudiantes `SMOKE-*` y `CONCURRENT-*` y conservan sus registros; no eliminan datos. Las pruebas de dominio incluyen saturación de skills a 100 tras 150 misiones adicionales.

La limitación funcional pendiente es conectar la extracción científica real cuando esté disponible en el repositorio. El flujo educativo completo funciona con los fixtures identificados.
