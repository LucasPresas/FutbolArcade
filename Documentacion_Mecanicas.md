# Documentación del Proyecto FUT (Mecánicas, IA y Cámara)

Este documento detalla el funcionamiento de los sistemas principales implementados en el juego, enfocados en el control del jugador, el comportamiento de la cámara y la inteligencia artificial (IA).

---

## 1. Sistema de Cámara Dinámica (`MatchCamera.cs`)

La cámara principal del juego (ubicada en `Pitch.tscn`) utiliza un script personalizado para seguir el flujo del partido con suavidad y proporcionar *feedback* visual (Zoom) durante acciones clave.

### Características Principales:
- **Seguimiento con interpolación (Lerp):** La cámara no está "pegada" rígidamente a la pelota, sino que utiliza una velocidad de enfoque (`FollowSpeed = 3.5`) para moverse suavemente hacia ella.
- **Zona Muerta (Deadzone):** Parámetro `DeadzoneSize = (3, 2)`. Si la pelota se mueve dentro de este recuadro invisible en el centro de la pantalla, la cámara no se desplaza. Esto evita micromovimientos que causan mareos en el jugador (Motion Sickness).
- **Límites de Cancha (Clamp):** Configurados en `MinX`/`MaxX` (-20 a 20) y `MinZ`/`MaxZ` (-12 a 12). Detienen el paneo de la cámara en los bordes para no renderizar el vacío fuera de la textura del piso (`Floor`).
- **Control de Perspectiva (FOV):** La cámara opera en Proyección de Perspectiva (`fov = 50.0`). Cuando un jugador controlado recibe la señal de "Zoom In", _MatchCamera_ interpola el FOV local hacia `ZoomedFov = 30.0` acercando la visión al área de la jugada.

### Hooks desde el Player:
Cuando un jugador real presiona Z o X para cargar disparo/pase, `Player.cs` llama a:
```csharp
_matchCamera.RequestZoomOn(this);
```
Al soltar el botón, se llama a `_matchCamera.CancelZoom();` retornando el FOV a 50.0.

---

## 2. Mecánicas de Jugador (`Player.cs` & `BallHandlerComponent.cs`)

### Control de Pelota (Posesión)
La mecánica principal dicta que la pelota (`Ball.cs`) deja de ser un objeto físico rebotando cuando es capturada por el `GrabArea` del jugador:
- La pelota se adjunta (Snap) dinámicamente al punto `DribblePoint` frente al jugador.
- Pierde las colisiones y su masa para que el jugador pueda correr, moverse y girar con ella integrada a sus pies, dándole un peso fluido al estilo "Arcade".

### Sistema de Disparo (Carga de Potencia)
- **Mantener Z:** Incrementa la variable `_chargeAmount` durante `0.83` segundos de 0 a 100%. Muestra y rellena una barra de potencia local en 3D (`PowerBarRoot`) encima del jugador de Verde a Rojo, junto a un texto en pantalla que indica el porcentaje.
- **Soltar Z:** Traspasa la posesión al aire y dispara un impulso rígido (ApplyCentralImpulse) basándose en `StatsResource.ShootForce` (fuerza min: 30%, max: 100%).

### Sistema de Pases Inteligentes y Tiempo Bala (Slow-Motion)
- **Tap rápido de X:** El jugador busca en un radio esférico de 8 metros (`PassScanArea`) a compañeros de equipo usando raycasts/overlaps. Si hay uno, transfiere la pelota directamente calculando dirección mediante el vector `target.GlobalPosition - this.GlobalPosition`.
- **Mantener X (Slow-Mo):** Si X se mantiene oprimido por más de `0.2` segundos (`PassHoldThreshold`), el sistema activa un "Tiempo Bala" (Tiempo del Engine escalado a `0.2`). La barra de pase se llena en tiempo real para disparar un pase a máxima potencia.

### Sistema de Robos (Tackle)
- **Presionar C:** Otorga al jugador una ventana de tiempo de `0.25` segundos. Durante esta fracción de segundo, se escanea repetidamente el frente del jugador. Si entra un enemigo en la zona colindante (distancia de cápsulas, `1.6` metros), el propio código del jugador "interviene" el `BallHandler` del contrincante, forza un *Release()* y expulsa la pelota lejos del enemigo, adueñándose indirectamente de la nueva posesión suelta.

---

## 3. Inteligencia Artificial (`AIController.cs`)

Los compañeros (Equipo 0) y Enemigos (Equipo 1) derivan de la misma clase abstracta de Jugador, con la única diferencia de que sus _Inputs_ están puenteados hacia `AIController.cs`.

La IA utiliza un sistema de **Machine States contextual**. Posee una función central (`UpdateAIState()`) que escanea en tiempo real el escenario cada _frame_ (¿dónde estoy, dónde está la pelota, quién la tiene, qué tan lejos estoy de ella en relación al resto de mis aliados?).

### Roles y Posicionamiento Inicial (`MatchManager.cs` y Setup)
En el Kick-off, se posicionan las IAs como laterales y defensores. Esto arma el `_startPosition`. A partir de esto se configuran 4 estados vitales:

- **Positioning (Vigilancia zonal):** La IA reconoce que NO es la IA más cercana a la pelota. En lugar de correr hacia ella (amontonamiento de bots), la IA retorna a su `_startPosition` base y simplemente desliza su eje `Z` (lateral) para seguirla con la mirada para interceptar pases cruzados sin abandonar la formación táctica ("Líneas de 3", "Líneas de 4", etc).

- **Defending (Cierre de arcos):** Se activa si el equipo contrario roba u obtiene la posesión. Las IAs calculan la línea recta que existe entre **"Posición Acutal de la Pelota"** y **"Su propio arco"**, realizando un `Lerp` e interponiéndose a medio camino para forzar la intercepción de embestidas, manteniendo su límite lateral intacto.

- **Chasing (Presión constante):** Únicamente habilitado para la IA que resulte ser la "Absoluta IA más cercana libre a la pelota" del equipo (utilizando identificadores únicos `GetInstanceId` de desempate). Este jugador abandona su rol táctico y va volando verticalmente hacia las coordenadas X,Z de la pelota para tratar de robarla empleando un umbral de proximidad física límite (`<= 1.6m`) gatillando su Tackle (`IsTackling() => true`).

- **Attacking (Romper líneas):** Cuando una IA capta el balón en sus pies, abandona las 3 reglas pasadas y su vector de movimiento ahora se dirige perpendicularmente al centro geométrico del arco contrario preestablecido (`_targetGoalPosition` o `goal_1`). Al cruzar el umbral de disparo (`ShootDistance < 12 metros`), fuerza `IsShooting() => true`, apuntando matemáticamente.

---

## 4. Match Manager y Entorno (La Cancha y los Goles)

- **Kick-off Inteligente (`ResetPositions`):** El `MatchManager.cs` rastrea permanentemente a la lista global `all_players`. Tras un gol, no solamente transporta a `.GlobalPosition = _startPosition`, sino que también halla por distancias absolutas quién está plantado justo en el centro del mapa `Vector3.Zero` otorgándole el balón en auto-posesión. Permitiendo arrancar con la pelota segura y fluida.
- **Event Bus:** Eventos y señales globales donde el script `GoalArea.cs` de las mallas (arcos) dictamina si una esfera ha entrado con exactitud y emite la señal `GoalScored` universalmente asíncrona que interrumpe la simulación, resetea todo a inicio, y añade +1 Punto a la lógica central de Canvas/UI de texto.
- **Limites Físicos (Wall y Floor):** Las coordenadas de colisión evitan el descarrilamiento vertical con la redimension de un `BoxShape3D` para el área general jugable y topes superiores-inferiores, simulando paredes traslúcidas que impiden el "Corner" hasta su futura adición.
