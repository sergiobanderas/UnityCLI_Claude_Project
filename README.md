# UnityCLI Claude Project

Prototipo de conducción arcade en 3D hecho con **Unity 6**, desarrollado con **Claude Code** y **Unity CLI** (más detalles en [Desarrollo con Unity CLI y Claude Code](#desarrollo-con-unity-cli-y-claude-code)). Conduces un coche low poly por una ciudad: aceleras y vas marcha atrás con el teclado, giras con el ratón y puedes hacer doble salto. La cámara en tercera persona usa Cinemachine.

## Requisitos

| Herramienta | Versión |
|---|---|
| Unity Editor | **6000.6.3f1** |
| Render pipeline | Universal Render Pipeline (URP) 17.6 |
| Input System | 1.20 |
| Cinemachine | 6.6 |

Unity instala los paquetes solo al abrir el proyecto, a partir de `Packages/manifest.json`.

## Cómo abrir el proyecto

1. Clona el repositorio:
   ```bash
   git clone https://github.com/sergiobanderas/UnityCLI_Claude_Project.git
   ```
2. En **Unity Hub**, pulsa *Add → Add project from disk* y elige la carpeta clonada.
3. Ábrelo con Unity **6000.6.3f1**. La primera vez tarda un rato porque genera la carpeta `Library/`.
4. Abre la escena `Assets/Scenes/City.unity` y pulsa **Play**.
5. Haz clic dentro de la ventana *Game* para que capture el ratón y el teclado.

## Controles

| Entrada | Acción |
|---|---|
| **W** | Acelerar |
| **S** | Frenar y, una vez parado, marcha atrás |
| **Ratón izquierda/derecha** | Girar el coche |
| **Espacio** | Saltar (se puede saltar otra vez en el aire) |
| **Esc** | Liberar el cursor (en el Editor) |

Con mando, el botón inferior (A / Cruz) también salta.

## Escenas

| Escena | Contenido |
|---|---|
| `Assets/Scenes/City.unity` | Ciudad low poly con calles, edificios y coches aparcados. Unos muros invisibles impiden que el coche se salga del mapa. |
| `Assets/Scenes/Level1.unity` | Escena de pruebas: un plano de césped con el coche. |

## Estructura

```
Assets/
├── Scenes/            City y Level1
├── Scripts/
│   ├── CarController.cs   Controlador del coche (escena actual)
│   └── CubeMovement.cs    Controlador antiguo del cubo (ya no se usa)
├── Prefabs/
│   └── PlayerCar.prefab   Coche del jugador (collider, Rigidbody y CarController)
├── Physics/
│   └── CarNoFriction      Material físico sin fricción para el coche
├── ithappy/           Asset de terceros: ciudad
└── Hodaart/           Asset de terceros: vehículos
```

## Cómo funciona

### CarController

[`CarController.cs`](Assets/Scripts/CarController.cs) mueve el coche con un `Rigidbody`:

- **Velocidad.** `currentSpeed` va de `-maxReverseSpeed` a `maxSpeed`. Si es negativa, el coche va marcha atrás. Si pulsas la dirección contraria mientras el coche aún se mueve, primero frena con `brakeDeceleration`.
- **Dirección.** La velocidad horizontal es `transform.forward * currentSpeed`. La vertical la controla la gravedad.
- **Giro.** Lee el movimiento horizontal del ratón (`<Mouse>/delta/x`), lo acumula en `Update` y lo aplica en `FixedUpdate` con `Rigidbody.MoveRotation`.
- **Salto.** Calcula la velocidad inicial con `v = √(2·g·h)` para llegar exactamente a `jumpHeight`. El contador de saltos se reinicia al tocar el suelo, que se detecta lanzando un raycast hacia abajo. `maxJumps = 2` permite saltar una vez más en el aire.
- **Entrada.** Usa `InputAction` del nuevo Input System. Los controles se pueden cambiar desde el Inspector.

Todos los valores (velocidad, aceleración, sensibilidad del ratón, altura del salto...) se ajustan en el Inspector, en el prefab `PlayerCar`.

### Cámara

La `Main Camera` tiene un **CinemachineBrain**. La cámara virtual `ThirdPersonCamera` está configurada así:

- **CinemachineFollow** en modo `LockToTargetWithWorldUp`: se queda detrás del coche, a 4,5 m de altura y 9 m de distancia, y le sigue con suavizado de posición y de giro.
- **CinemachineRotationComposer**: mantiene el coche centrado en pantalla, con suavizado.

## Desarrollo con Unity CLI y Claude Code

Este proyecto se ha desarrollado con **[Claude Code](https://claude.com/claude-code)**, el asistente de programación de Anthropic, conectado al Editor de Unity mediante **Unity CLI**. Claude Code no solo escribía los scripts: también instalaba paquetes (Cinemachine), montaba las escenas, entraba en Play Mode, hacía capturas de pantalla y revisaba la consola, todo desde la terminal.

### 1. Instalar Unity CLI

Unity CLI es la herramienta de línea de comandos oficial de Unity. Su comando es `unity`.

```powershell
# Windows (PowerShell)
irm https://unity.com/install.ps1 | iex
```
```bash
# macOS / Linux
curl -fsSL https://unity.com/install.sh | bash
```

Abre una terminal nueva y comprueba que funciona:

```bash
unity --version
unity auth login      # inicia sesión con tu cuenta de Unity
```

### 2. Conectar el Editor con el paquete Pipeline

El paquete `com.unity.pipeline` abre un pequeño servidor dentro del Editor. A través de él, la CLI (y Claude Code) pueden compilar, ejecutar código C#, entrar en Play Mode o hacer capturas.

```bash
cd ruta/al/proyecto
unity pipeline install     # añade com.unity.pipeline al proyecto (este repo ya lo trae)
unity pipeline list        # muestra los Editores abiertos y conectados
unity command editor status
```

Con el Editor abierto, `editor status` debe devolver `"status":"ready"`.

### 3. Instalar Claude Code

```powershell
# Windows (PowerShell)
irm https://claude.ai/install.ps1 | iex
```
```bash
# macOS / Linux
curl -fsSL https://claude.ai/install.sh | bash
```

Después ejecuta `claude` dentro de la carpeta del proyecto e inicia sesión con tu cuenta de Claude.

### 4. Añadir el plugin de Unity a Claude Code

El plugin oficial de Unity añade a Claude Code *skills* (instrucciones especializadas) para Unity CLI, el paquete Pipeline, físicas, UI, URP y más. Dentro de Claude Code, escribe:

```
/plugin marketplace add Unity-Technologies/unity-agent-plugin
/plugin install unity@unity-agent-plugin
```

### 5. Usarlo desde la extensión de VS Code

Al final trabajamos con la **extensión de Claude Code para Visual Studio Code**, en lugar de con la terminal:

1. En VS Code, abre *Extensions* (`Ctrl+Shift+X`), busca **Claude Code** e instálala.
2. Abre la carpeta del proyecto de Unity en VS Code.
3. Abre el panel de Claude Code con su icono en la barra lateral e inicia sesión.
4. Con el Editor de Unity abierto, ya puedes pedirle cosas en lenguaje natural, por ejemplo *"añade un doble salto al coche"*. Claude edita los scripts y usa `unity command ...` para compilar y probar en el Editor.

Ventajas de la extensión: ves los cambios en los archivos como *diffs*, puedes hacer clic en las referencias a archivos y líneas, y Claude sabe qué archivo tienes abierto o seleccionado.

> Los scripts auxiliares que Claude usó para montar las escenas desde el Editor están en `AgentScripts/`, fuera de `Assets/`. No forman parte del juego.

## Assets de terceros

Este proyecto usa assets de la Unity Asset Store:

- **Cartoon City Free**, de *ithappy* (`Assets/ithappy/`)
- **LowPoly Vehicle Collection 2**, de *Hodaart* (`Assets/Hodaart/`)

Son propiedad de sus autores y se rigen por el [Unity Asset Store EULA](https://unity.com/legal/as-terms). Si en tu copia del repositorio no vienen esas carpetas, descarga los paquetes gratis desde la Asset Store e impórtalos en las mismas rutas.

## Créditos

Desarrollado como proyecto de clase con la ayuda de [Claude Code](https://claude.com/claude-code) (extensión de VS Code) y [Unity CLI](https://github.com/Unity-Technologies/unity-agent-plugin).
