# SPECS.md - PomodoroFocus

## Overview

App Pomodoro en Blazor WebAssembly que ayuda al usuario a gestionar su tiempo usando la técnica Pomodoro.

### Actor Principal
- **Usuario**: Cualquier persona que quiera gestionar su tiempo con la técnica Pomodoro.

### Stack
- .NET 10.0 + Blazor WebAssembly
- Clean Architecture (Domain → Application → Infrastructure → Web)
- Tests: NUnit

---

## Casos de Uso Funcionales

### Grupo 1: Gestión del Temporizador Pomodoro

| ID | Nombre | Actor | Descripción |
|----|--------|-------|-------------|
| UC-01 | Iniciar un Pomodoro | Usuario | Inicia un ciclo de trabajo con cuenta regresiva desde la duración configurada (defecto 25 min). Muestra "Pausar" y "Cancelar", oculta "Iniciar". |
| UC-02 | Pausar y Reanudar | Usuario | Detiene temporalmente la cuenta regresiva y la continúa del mismo punto. El botón alterna entre "Pausar"/"Reanudar". |
| UC-03 | Cancelar (No Completado) | Usuario | Interrumpe un Pomodoro en curso sin que cuente. Muestra confirmación. Vuelve al estado inicial. El contador no se incrementa. |
| UC-04 | Finalizar (Completado) | Usuario | Termina un Pomodoro antes de tiempo pero lo cuenta como completado. Incrementa contador y ofrece el descanso correspondiente. |

#### Flujo UC-01
1. Usuario ve botón "Iniciar Pomodoro"
2. Usuario hace clic
3. App muestra temporizador en cuenta regresiva
4. Botones "Pausar" y "Cancelar" visibles, "Iniciar" oculto
5. **Post**: Temporizador activo descontando tiempo

#### Flujo UC-02
1. Pomodoro en curso → clic en "Pausar"
2. Temporizador se detiene, botón cambia a "Reanudar"
3. Clic en "Reanudar" → temporizador continúa
4. **Post**: Estado alterna entre "en curso" y "pausado"

#### Flujo UC-03
1. Pomodoro en curso → clic en "Cancelar"
2. App muestra confirmación "No Completado"
3. Usuario confirma
4. Temporizador se reinicia, app vuelve a estado inicial, contador no incrementa
5. **Post**: App lista para nuevo ciclo

#### Flujo UC-04
1. Pomodoro en curso → clic en "Cancelar"
2. App muestra confirmación "Completado"
3. Usuario confirma
4. Contador de Pomodoros se incrementa (+1)
5. App ofrece iniciar descanso correspondiente (corto o largo)
6. **Post**: Ciclo avanza, se ofrece descanso

---

### Grupo 2: Gestión de Descansos

| ID | Nombre | Actor | Descripción |
|----|--------|-------|-------------|
| UC-05 | Iniciar Descanso Automáticamente | Sistema | Al llegar a 00:00, incrementa contador y ofrece descanso corto (< 4 Pomodoros) o largo (= 4 Pomodoros, reinicia contador). |
| UC-06 | Pausar, Reanudar o Cancelar Descanso | Usuario | Permite pausar/reanudar un descanso (igual que UC-02) o cancelarlo para volver a estado principal con botón "Iniciar Pomodoro". |

#### Flujo UC-05
1. Temporizador Pomodoro llega a 00:00
2. Sistema emite alerta (sonido/visual)
3. Contador de Pomodoros completados se incrementa
4. Si contador < 4: muestra "Iniciar Descanso Corto"
5. Si contador = 4: muestra "Iniciar Descanso Largo", contador reinicia a 0
6. **Post**: Usuario puede iniciar el descanso correspondiente

#### Flujo UC-06
1. Descanso activo → "Pausar" (mismo comportamiento UC-02)
2. O "Cancelar" → detiene descanso
3. App vuelve a estado principal con "Iniciar Pomodoro"
4. **Post**: Descanso interrumpido, app lista para nuevo Pomodoro

---

### Grupo 3: Configuración y Comportamiento Inicial

| ID | Nombre | Actor | Descripción |
|----|--------|-------|-------------|
| UC-07 | Carga Inicial | Usuario | Al abrir la app muestra temporizador en 25 min y botones: "Iniciar Pomodoro", "Iniciar Descanso Corto", "Iniciar Descanso Largo". |
| UC-08 | Configurar Tiempos | Usuario | Personaliza duraciones: Pomodoro (15/20/25/30 min), Descanso Corto (3/4/5/10 min), Descanso Largo (10/15/20 min). Solo cuando ningún temporizador está en curso. |

#### Flujo UC-07
1. Usuario navega a la URL de la app
2. Página carga y muestra temporizador con duración por defecto (25 min)
3. Botones visibles: "Iniciar Pomodoro", "Iniciar Descanso Corto", "Iniciar Descanso Largo"
4. **Post**: App lista para primera acción del usuario

#### Flujo UC-08
1. Usuario accede a configuración (ícono de engranaje)
2. UI muestra selectores para las 3 duraciones
3. Usuario selecciona nuevos valores
4. App guarda configuración para la sesión actual
5. **Pre-condición**: Ningún temporizador en curso
6. **Post**: Nuevos ciclos usan los tiempos configurados

---

## Casos de Uso Técnicos

| ID | Nombre | Actor | Descripción |
|----|--------|-------|-------------|
| UT-01 | Probar la Lógica del Temporizador | Desarrollador | Pruebas unitarias con NUnit para métodos clave: Start(), Pause(), Resume(), Cancel(), OnTick(), OnFinish(). Validar estado, tiempo restante, incremento de contador, transición a descansos. |
| UT-02 | Probar el Flujo Completo de la UI | Desarrollador | Pruebas E2E (Playwright/Selenium) simulando el "camino feliz": cargar página, iniciar, pausar/reanudar, finalizar, descansar, cancelar descanso. |
| CD-01 | Automatizar Despliegue a Producción | Desarrollador | CI/CD con GitHub Actions: trigger en push a main → checkout → setup-dotnet → publish Blazor WASM en Release → deploy a gh-pages. |

---

## Resumen de Implementación

### Comportamiento esperado
- **Temporizador**: `System.Timers.Timer` con intervalos de 1 segundo
- **Auto-transiciones**: Pomodoro → Short Break / Long Break → Pomodoro
- **Long Break**: Cada 4 Pomodoros completados
- **Configuración**: Solo cuando el temporizador está idle (Ready/Completed)

### Valores por defecto
| Parámetro | Default |
|-----------|---------|
| Pomodoro | 25 min |
| Short Break | 5 min |
| Long Break | 15 min |
| Pomodoros antes de Long Break | 4 |

### Opciones de configuración
| Parámetro | Opciones |
|-----------|----------|
| Pomodoro | 15, 20, 25, 30 min |
| Short Break | 3, 4, 5, 10 min |
| Long Break | 10, 15, 20 min |
