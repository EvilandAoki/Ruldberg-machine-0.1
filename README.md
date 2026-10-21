# Ruldberg Machine

Máquina de Rube Goldberg de 12 etapas construida en Unity con primitivas (cubos y esferas) y física de PhysX. Una bola roja se suelta con la barra espaciadora y desencadena una cadena de dominós, una rampa, un embudo, un péndulo, un carrito, un zigzag de rampas y una trampilla, hasta que un cubo de azúcar cae sobre una taza de café.

La escena no se arma a mano: un script de editor la genera completa a partir de una tabla de coordenadas.

## Contenido

- [Requisitos](#requisitos)
- [Inicio rápido](#inicio-rápido)
- [Las 12 etapas](#las-12-etapas)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Cómo está hecha la escena](#cómo-está-hecha-la-escena)
- [Materiales físicos](#materiales-físicos)
- [Ajustes de física en tiempo de ejecución](#ajustes-de-física-en-tiempo-de-ejecución)
- [Diferencias con el diseño original](#diferencias-con-el-diseño-original)
- [Diagnóstico](#diagnóstico)
- [Problemas conocidos](#problemas-conocidos)

## Requisitos

| Componente | Versión |
|---|---|
| Unity | 6000.3.26f1 |
| Render pipeline | Universal Render Pipeline 17.3.0 |
| Input System | 1.20.0 |

El builder usa `PhysicsMaterial` en Unity 6 y `PhysicMaterial` en versiones anteriores. Las pruebas se hicieron únicamente en Unity 6000.3.26f1.

## Inicio rápido

1. Clona el repositorio y ábrelo desde Unity Hub con la versión indicada.
2. Espera a que Unity importe y compile los scripts. No debe haber errores en rojo en la consola.
3. Abre una escena (la incluida es `Assets/Scenes/SampleScene.unity`).
4. Ejecuta el menú **Tools > Rube Goldberg > Construir escena**. Si ya existe una escena construida, acepta reemplazarla.
5. Guarda la escena con `Ctrl+S`.
6. Pulsa **Play**, haz clic dentro de la ventana **Game** para darle el foco y pulsa la **barra espaciadora**.
7. Abre la pestaña **Console**: deben aparecer las etapas, una por línea, con su tiempo.

Para eliminar la escena generada, usa **Tools > Rube Goldberg > Borrar escena**.

### Qué hace el constructor

- Fija el Fixed Timestep en 0,01 s.
- Crea los materiales físicos y los materiales de color en `Assets/RubeGoldberg/Materials`.
- Genera todos los objetos bajo un objeto raíz llamado `RubeGoldberg`, con la posición, rotación y escala de la tabla de datos.
- Añade Rigidbody a los objetos dinámicos y los Hinge Joint del péndulo y la trampilla.
- Configura la cámara ortográfica y la luz direccional.

## Las 12 etapas

| Etapa | Evento | Objeto que lo registra |
|---|---|---|
| 1 | Se suelta la bola roja | `BolaRoja` |
| 2 | La bola roja toca la plataforma | `PlataformaA` |
| 3 | La bola roja toca el primer dominó | `Domino1` |
| 4 | `Domino6` golpea la bolita verde | `BolitaVerde` |
| 5 | La bolita verde cruza el embudo | `ZonaEmbudo` (trigger) |
| 6 | La bolita verde llega a la rampa de salida | `RampaSalida` |
| 7 | La bolita verde golpea el péndulo | `Pendulo` |
| 8 | El péndulo golpea el carrito | `Carrito` |
| 9 | El carrito golpea el bloque | `Bloque` |
| 10 | El bloque golpea la bola gris | `BolaGris` |
| 10a | La bola gris toca la primera rampa del zigzag | `RampaZ1` |
| 10b | La bola gris toca la segunda rampa del zigzag | `RampaZ2` |
| 10c | La bola gris toca la tercera rampa del zigzag | `RampaZ3` |
| 11 | La bola gris golpea la pata de la trampilla | `Pata` |
| 12 | El cubo de azúcar cae sobre el café | `TazaCafe` |

Cada etapa se registra una sola vez, solo después de soltar la bola roja, y solo cuando el golpe lo da el objeto esperado.

La etapa 5 usa un trigger porque la bolita verde atraviesa el embudo sin tocar sus paredes.

## Estructura del proyecto

```
Assets/
  Editor/
    RubeGoldbergBuilder.cs     Construye la escena desde la tabla de datos
  Scripts/
    RGSetup.cs                 Añade los scripts de etapa al pulsar Play
    RGTrigger.cs               Suelta la bola roja con la barra espaciadora
    RGStepMarker.cs            Registra el primer golpe esperado de cada etapa
    RGZoneMarker.cs            Registra la etapa 5 (trigger del embudo)
    RGRetractPata.cs           Retrae la pata cuando la trampilla empieza a caer
    RGLog.cs                   Estado de los cuerpos clave cada 0,5 s
    RGContactLog.cs            Contactos de la pata y la trampilla (diagnóstico)
  RubeGoldberg/
    Materials/                 Materiales físicos y de color generados
  Scenes/
    SampleScene.unity          Escena con la máquina construida
```

Unity exige un `MonoBehaviour` por archivo, con el mismo nombre que la clase. Por eso cada script de ejecución está en su propio archivo.

## Cómo está hecha la escena

### Ejes y unidades

- 1 unidad de Unity equivale a 1 metro.
- X es horizontal, Y es la altura y Z vale 0 para todo lo que se mueve: la máquina vive en un único plano, vista de frente.
- La cámara es ortográfica, con tamaño 1,7 y posición (2,75; 0,80; -8,00).

### Escala de las primitivas

- Cubo: la escala es el tamaño en metros.
- Esfera: la escala es el diámetro.
- Cilindro: mide 2 m de alto por defecto, así que la escala en Y es la mitad de la altura real. Solo se usa como decoración (el hilo del péndulo), sin collider.

### Objetos dinámicos

Todos los Rigidbody tienen `Freeze Position Z`, `Freeze Rotation X` y `Freeze Rotation Y`, y `Collision Detection = Continuous Dynamic`. Así la máquina se comporta como un sistema 2D.

| Objeto | Forma | Masa (kg) |
|---|---|---|
| `BolaRoja` | Esfera, cinemática hasta soltarla | 0,3 |
| `Domino1` a `Domino6` | Cubos de 0,04 m de grosor y 0,20 m de fondo | 0,02 a 0,08 |
| `BolitaVerde` | Esfera | 0,03 |
| `Pendulo` | Objeto vacío con `Hilo` y `Bob` como hijos, Hinge Joint sin límites | 0,03 |
| `Carrito` | Cubo | 0,1 |
| `Bloque` | Cubo | 0,08 |
| `BolaGris` | Esfera | 0,25 |
| `Trampilla` | Cubo, Hinge Joint con límites -25 y 0 | 0,05 |
| `Pata` | Cubo | 0,05 |
| `CuboAzucar` | Cubo | 0,02 |

### Hinge Joints

| Objeto | Anchor | Axis | Límites |
|---|---|---|---|
| `Pendulo` | (0, 0, 0) | (0, 0, 1) | Sin límites |
| `Trampilla` | (-0,5; 0; 0) | (0, 0, 1) | Min -25, Max 0 |

El Anchor de la trampilla está en coordenadas locales: -0,5 es su extremo izquierdo.

## Materiales físicos

Los materiales físicos tienen `Bounciness = 0`. Todos usan `Friction Combine = Average`, salvo `PM_Pata`, que usa `Minimum`.

| Material | Fricción | Se usa en |
|---|---|---|
| `PM_Hielo` | 0 | `Riel`, `Carrito`, `Bloque`, `Trampilla` |
| `PM_Pata` | 0,02 (Minimum) | `Pata` |
| `PM_Liso` | 0,1 | `CuboAzucar` |
| `PM_Medio` | 0,4 | Rampas, bolas, mesa, `PlataformaB`, bolita verde |
| `PM_Agarre` | 0,7 | Dominós y `PlataformaA` |

`PM_Pata` usa `Minimum` porque PhysX aplica el modo de combinación de mayor prioridad entre los dos materiales en contacto, y `Minimum` tiene más prioridad que `Average`. Así la fricción efectiva de la pata es 0,02 contra cualquier superficie.

## Ajustes de física en tiempo de ejecución

`RGSetup` aplica estos ajustes al pulsar Play, porque la configuración por defecto de PhysX está pensada para objetos de tamaño métrico y esta escena tiene piezas de 2 a 5 cm:

| Ajuste | Valor | Motivo |
|---|---|---|
| `maxAngularVelocity` | 100 rad/s | El valor por defecto (7 rad/s) frenaba artificialmente las esferas pequeñas. |
| `solverIterations` | 30 | Más precisión en contactos entre piezas pequeñas. |
| `solverVelocityIterations` | 10 | Igual que el anterior. |
| `sleepThreshold` | 0 | Evita que las fichas ligeras se duerman. |
| `contactOffset` | 0,002 m | El valor por defecto (0,01 m) es muy grande para fichas de 4 cm. |

Los scripts de etapa se añaden en tiempo de ejecución y no se guardan en la escena. Las referencias a scripts guardadas en la escena se rompían al recargarla al pulsar Play.

## Diferencias con el diseño original

El diseño partía de una tabla de coordenadas calculada a mano. Al simular y probar en Unity, la cadena se detenía en varios puntos, y se corrigió lo siguiente:

| Cambio | Motivo |
|---|---|
| Se elimina la palanca de las etapas 4 y 5 | `Domino6` golpeaba su extremo casi en línea con el pivote y no podía moverla. |
| Dominós en x = 0,72; 0,81; 0,91; 1,03; 1,18 y 1,37 | Con huecos de 0,02 m las fichas se apoyaban unas en otras y se atascaban. |
| `PostePelota` en (1,55; 1,15) y `BolitaVerde` en lo alto del poste | La punta de `Domino6`, la parte más rápida, empuja la bolita. |
| `RampaPuente` en (1,845; 1,121), rotación -32 grados | Lleva la bolita al embudo por gravedad. |
| Embudo bajado a y = 0,86 | La boca original estaba por encima de la plataforma. |
| `Pendulo` con masa 0,03 y `Carrito` en x = 3,06 | La bolita se quedaba bajo el péndulo y este no llegaba a mover el carrito. |
| `PM_Hielo` en riel, carrito y bloque | Con `PM_Liso` el carrito se detenía antes de llegar al bloque. |
| `BolaRoja` con masa 0,3 y `PlataformaA` con `PM_Agarre` | Con masa 0,1 las fichas se deslizaban en lugar de caer. |
| `BolaGris` con masa 0,25 y `PM_Pata` en la pata | La pata se frenaba a los pocos centímetros con la carga de la trampilla. |
| Trampilla con `PM_Hielo`, azúcar con `PM_Liso` y taza desplazada 0,03 m en X | El azúcar avanzaba a 4,5 mm/s sobre la trampilla inclinada. |
| La pata se retrae al caer la trampilla | De pie, la pata bloqueaba la trayectoria del azúcar. |

La retracción de la pata (`RGRetractPata`) es un recurso, no una solución puramente física: cuando la trampilla baja de -2 grados, la pata desactiva sus colisiones y cae a través de la mesa.

## Diagnóstico

Al pulsar Play, `RGSetup` añade dos registros de diagnóstico:

- **`RGLog`**: cada 0,5 s escribe una línea `RGLOG t=...` con la posición, rotación y velocidad de los 15 cuerpos clave (bolas, dominós 1 a 6, péndulo, carrito, bloque, pata, trampilla y azúcar).
- **`RGContactLog`**: en la pata y la trampilla, escribe líneas `RGCONTACT` con quién toca a quién, la normal, el impulso y la velocidad relativa. Solo actúa cuando la bola gris pasa de x = 4,8.

Para localizar el punto donde se detiene una corrida, busca la primera etapa que no aparece en la consola: ese es el eslabón roto. En Linux, el log del editor está en `~/.config/unity3d/Editor.log`.

## Problemas conocidos

- **Mensaje `NoSubscription` en la consola.** Viene del paquete Unity AI Assistant, que intenta contactar `generators.ai.unity.com` y no tiene suscripción. No afecta a la escena.
- **La trampilla gira al revés.** Si no cae, cambia en su Hinge Joint los límites a Min 0 y Max 25. Con Unity 6000.3.26f1 el signo del documento funcionó.
- **Tiempos distintos entre corridas.** La física no es idéntica entre motores ni entre versiones de Unity. La cadena completa tarda unos 10 s hasta la etapa 11.
- **La escena guarda masas y materiales.** Si cambias valores en el builder, vuelve a ejecutar **Construir escena** y acepta reemplazar. Si no, la escena conserva los valores anteriores.
- **Scripts de diagnóstico siempre activos.** `RGLog` y `RGContactLog` escriben en la consola en cada ejecución. Quita su línea en `RGSetup.cs` si no los necesitas.
