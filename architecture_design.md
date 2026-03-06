# Arquitectura del Juego de Fútbol 3vs3 (Godot 4 C#)

## 1. Patrones de Diseño Recomendados

Para un juego escalable y con mecánicas de *tag-team* (cambio de personajes), capacidad de cambiar en tiempo real entre IA y Player, y la interacción fluida con la pelota, la mejor forma de organizar el proyecto es combinando los siguientes patrones:

### A. Input Abstraction (Separación de "Cuerpo" y "Mente")
El error más común en principantes es escribir la lógica del teclado directamente dentro del Player (`if (Input.IsActionPressed("ui_right"))`). 
**Solución:** Todo personaje en la cancha tiene un "Cuerpo" (físicas, animaciones, la entidad en sí) pero obedece a un "Controlador" (Mente).
- Se crea una interfaz `IController` que tiene métodos como `Vector2 GetMoveDirection()`, `bool IsPassing()`, `bool IsShooting()`.
- Se crea un `PlayerInputController` (lee el joystick/teclado para tu equipo) y un `AIController` (la IA calcula y decide qué acciones tomar).
- Cuando haces *tag-team* (cambias de jugador), simplemente le desconectas/quitas el `AIController` a ese jugador y le activas el `PlayerInputController`. ¡El código del "Cuerpo" y sus físicas jamás cambia!

### B. Máquina de Estados Finita (State Machine / FSM)
Absolutamente **todo** en el campo debe tener su propia máquina de estados. Usar FSM previene los espaguetis de `if/else` interminables.

**Para los Jugadores (Actores):**
- `IdleState`
- `MoveState`
- `DribbleState` (Movimiento, pero cuando tiene la pelota pegada)
- `TackleState` (Barrida / Dash)
- `PassState` (Frame recovery de la animación de pase)
- `ShootState` (Tiempo de carga de tiro y finalización)
- `StunnedState` (Cuando le hacen una barrida o se cae)

**Para la Pelota:**
- `FreeState` (Rodando libremente, aplicando físicas)
- `CarriedState` (Pegada al pie de un jugador, desactiva sus colisiones físicas y sigue su Marker2D)
- `PassedState` (Viajando entre jugador y jugador con asistencia matemática / Interpolación)
- `ShotState` (Volando con físicas puras hacia el arco, considerando altura)

**Para el Partido (Match Manager):**
- `PreMatchState`, `KickoffState`, `PlayState`, `GoalScoredState`, `HalftimeState`.

### C. Patrón de Componentes (Composition)
Godot fomenta la composición natural. En lugar de un script monolítico gigantesco, divide la lógica en pequeños Nodos Hijos que actúan como componentes aislados:
- `MovementComponent.cs`: Aplica físicas, fricción y mueve al `CharacterBody2D`.
- `StaminaComponent.cs`: Maneja la energía para correr o hacer barridas.
- `BallHandlerComponent.cs`: Área (Area2D) que detecta cuando el jugador toca una pelota libre y maneja la robada de pelota.

### D. Event Bus (Global Signal Bus)
Un Singleton (Autoload en Godot) llamado `EventBus.cs` que actúe como un canal de radio global.
- En lugar de que la pelota busque la referencia a la UI para sumare un gol, la pelota o la red del arco simplemente gritan al aire: `EventBus.EmitGoalScored(Team.Red)`.
- El MatchManager, el AudioPlayer y el UIManager están "suscritos" a esa señal y reaccionan de forma autónoma (suman puntos, ponen ruido de estadio, actualizan los números en pantalla). Todo totalmente desacoplado.

## 2. Abordando el Estilo 2DHD
El 2DHD (estilo Octopath Traveler o un falso 3D) en un entorno top-down (visto desde arriba) requiere planificación desde el día cero.
- **Opción A (Godot 2D Puro):** Tratar la "Altura" (Eje Z) de forma artificial. La pelota necesitará una variable `z_height` virtual para simular pases bombeados. La sombra de la pelota tiene la posición real `(x, y)` en el suelo, pero el *Sprite* visual de la pelota se eleva restando el `z_height` a su posición `y`. Para las colisiones de cabeza/pecho deberás calcular la validación matemática usando ese `z_height`.
- **Opción B (Godot 3D con Billboards):** Muy popular en 2DHD. Usas un entorno 3D (para que el motor físico de Godot se encargue de la gravedad Z real de la pelota, los rebotes y las alturas de los hitboxes de las cabezas), pero todos los jugadores son planos 2D (`Sprite3D` con la opción de *Billboard* activa para que siempre miren a la cámara ortográfica). ¡Les ahorrará semanas de cálculos matemáticos para la pelota!

## 3. Arquitectura de Jerarquía (Godot Node Tree)

Un jugador base se vería típicamente así en el panel de escenas:
```text
PlayerBase (CharacterBody2D o 3D) [Script: Actor.cs]
 ├── Visuals (Node2D/Node3D) -> Ayuda a flipear (mirar izq/der) sin romper colisiones.
 │    ├── Sprite
 │    └── Shadow
 ├── CollisionShape
 ├── Controllers (Node)
 │    ├── PlayerInput (Node) [Script: PlayerController.cs, IController]
 │    └── AIInput (Node) [Script: AIController.cs, IController]
 ├── StateMachine (Node) [Script: StateMachine.cs]
 │    ├── Idle (Node) [Script: IdleState.cs]
 │    ├── Move (Node) [Script: MoveState.cs]
 │    └── ... (etc)
 └── Components (Node)
      ├── Movement (Node) [Script: MovementComponent.cs]
      └── BallHandler (Area) [Script: BallHandlerComponent.cs]
```

## 4. Estructura de Carpetas Sugerida

Para mantener C# organizado como es debido:
```text
res://
 ├── Scenes/
 │    ├── Actors/ (Modelos de Jugadores, Arqueros)
 │    ├── Ball/ 
 │    ├── Match/ (Cancha, Arcos, Estructura del estadio)
 │    └── UI/ (HUD, Marcador)
 ├── Scripts/
 │    ├── Core/ (StateMachine, EventBus, Interfaces como IState, IController)
 │    ├── Controllers/ (Player, IA)
 │    ├── Components/ (Movement, Stamina)
 │    ├── States/ (Carpetas internas: PlayerStates, BallStates, MatchStates)
 │    └── Actors/ (Actor.cs, Ball.cs)
 └── Assets/
      ├── Sprites/
      ├── Sounds/
      └── Fonts/

## 5. Diseño del Entorno (Cancha de Barrio Argentina)

El juego se ambienta en canchas improvisadas de barrio en Argentina, típicas de los baldíos. Para reflejar esto a nivel sistema:
- **Props Modulares y Aleatoriedad:** Cada nivel/cancha debe poder instanciar props como árboles o manchas de barro aleatorias en el campo. Se recomienda tener un nodo `EnvironmentGenerator` o usar `TileMap` con *TileData* y *probabilidades* de spawn para generar estas imperfecciones.
- **Límites con Físicas (Rebote):** Los bordes de la cancha (paredes o alambrados) deben implementar físicas reales. Se les asignará un `PhysicsMaterial` (con "Bounce" configurado) para que la pelota (`RigidBody2D` o `RigidBody3D`) rebote naturalmente al impactar, permitiendo usar las paredes para hacer pases.

## 6. Flujo de Juego y Selector de Modos

Para manejar el cambio de pantallas y la configuración del partido, se debe usar un Singleton/Autoload como `GameManager.cs` o `SceneManager.cs`.
El flujo de UI debe ser:
1. **Menú Principal:** Pantalla de inicio.
2. **Selector de Modo:** Menú donde el jugador elige:
   - **1 vs 1**
   - **3 vs 3**
   - **Player vs IA**
   - **Multijugador Local / Co-op:** Tanto 1vs1 como 3vs3 usando varios *gamepads* (Godot soporta mapeo por Device ID `0`, `1`, `2`, `3`).

## 7. Escalabilidad y Futuras Expansiones

La estructura basada en Componentes, Controladores y Máquina de Estados se ha diseñado para que al implementar características futuras el sistema no colapse:
- **Selección de Equipos Personalizada y Gestión de Jugadores:** Se manejará usando el sistema de `Resource` (datos serializados). Cada personaje será un `PlayerStats.tres` o `Resource` que guardará su nombre, estética y estadísticas (velocidad, fuerza, resistencia).
- **Sistema de Clases:** Para personalizar aún más a los jugadores, se cargarán distintas clases u arquetipos. Por ejemplo, al inyectar un `ClassData` en el `ActorBase`, este modifica la velocidad del `MovementComponent` o le da habilidades especiales, manteniendo todo altamente escalable.
- **Habilidades Especiales de Tiro:** Cada jugador podrá tener mecánicas únicas al patear la pelota (ej. remates potentes, con curva, o que apliquen efectos). Esto se integrará fácilmente creando componentes como `SpecialShotComponent` y añadiendo un estado `SpecialShootState` a la máquina de estados del jugador, que se active según energía o *cooldowns*.
```
