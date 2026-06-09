# GX Light Browser

Version actual: `1.9`

GX Light Browser es un prototipo de navegador liviano para Windows, inspirado en el flujo de trabajo de Opera GX y Brave, sin copiar marcas, identidad visual ni elementos protegidos de esos navegadores.

Usa Microsoft Edge WebView2 en vez de Electron. Eso permite que la aplicacion sea mas pequena y aproveche el runtime WebView2 que ya viene instalado o disponible en muchos equipos Windows.

## Caracteristicas actuales

- Navegacion con multiples pestanas.
- Perfil persistente aislado en `%LOCALAPPDATA%\GXLightBrowser`.
- Extensiones WebView2 habilitadas.
- Importacion de extensiones Chrome/Edge desempaquetadas desde una carpeta.
- Bloqueo nativo de requests antes de que lleguen a la pagina.
- Archivo local de filtros tipo ABP en `%LOCALAPPDATA%\GXLightBrowser\filters.txt`.
- Prevencion estricta de seguimiento de WebView2.
- Interfaz compacta con marco oscuro, toolbar adaptable, pestanas, accesos directos laterales y tooltips.
- Accesos a Chrome Web Store y Opera Addons.
- Cierre de pestanas con `x`, click del scroll o `Ctrl+W`.
- Accesos directos con click del scroll para abrir en una nueva pestana interna.
- YouTube Shields para quitar contenedores comunes de anuncios, pulsar botones de saltar anuncio y acelerar estados de anuncio dentro del reproductor.
- Modo de baja memoria para pestanas inactivas.
- Busqueda rapida de reglas de dominio en el bloqueador nativo.
- Privacy Firewall integrado para trackers conocidos, endpoints tipo telemetria y parametros de seguimiento.
- Menu principal inspirado en Opera/Brave con accesos a pestanas, historial, descargas, extensiones, passwords, memoria, shields, settings y herramientas de desarrollador.
- Pagina de historial de sesion.
- Pagina de descargas de sesion.
- Autosave y autofill de passwords de WebView2 habilitados.
- Tab islands para agrupar pestanas.
- Menu contextual de tab islands: seleccionar varias pestanas, crear una isla coloreada, duplicar, recargar, copiar URLs y cerrar seleccionadas.
- Suspension real de pestanas inactivas para liberar WebView2 y memoria.
- Monitor visible de memoria y GX Control configurable.
- Pestana de novedades que aparece solo una vez por version instalada.
- Barra de favoritos con apertura en nueva pestana mediante click del scroll.
- Importacion/exportacion de bookmarks en HTML compatible con navegadores.
- Gestor de bookmarks con carpetas, busqueda, seleccion multiple y eliminacion masiva.
- Importacion/exportacion de passwords CSV mediante una boveda local protegida con Windows DPAPI.
- Favicons por pagina y modo opcional de pestanas compactas cuadradas.
- Playlist local para guardar, volver a abrir y eliminar paginas multimedia.
- Icono propio integrado en la ventana y el ejecutable.

## Versionado

GX Light Browser usa versiones simples pensadas para el proyecto personal:

- linea base actual: `1.0`
- linea de mejora actual: `1.9`
- cada mejora o correccion del navegador sube la version menor: `1.10`, `1.11`, ... `1.19`
- despues de `1.19`, la siguiente linea pasa a `2.0`

Cuando una nueva version se ejecuta por primera vez, el navegador abre `gxlight://updated` con un resumen corto. La app guarda la ultima version vista en `%LOCALAPPDATA%\GXLightBrowser\settings.ini`, por lo que esa pestana aparece solo una vez por version.

Desde `1.2`, el aviso de novedades se lee desde `update.json` en GitHub:

```text
https://raw.githubusercontent.com/wiimri/GX-lite/main/update.json
```

Eso permite cambiar el texto de novedades, links y version publicada desde GitHub. Si el navegador no puede conectar a GitHub, usa las notas locales compiladas como respaldo.

El historico completo de cambios esta en `CHANGELOG.md`:

```text
https://github.com/wiimri/GX-lite/blob/main/CHANGELOG.md
```

Importante: cambiar `update.json` muestra novedades o avisa que hay una version disponible, pero no cambia magicamente el binario instalado. Para que el usuario reciba codigo nuevo sin hacerlo manualmente, el siguiente paso es crear un actualizador binario que descargue un release firmado y reemplace el `.exe`.

Los cambios solo de documentacion pueden ir en GitHub sin subir la version de la aplicacion, porque no cambian el binario ni la experiencia dentro del navegador.

## Compilacion

Ejecutar desde PowerShell:

```powershell
cd C:\Users\arias\Documents\DEV\GXLightBrowser
.\scripts\Build.ps1
.\scripts\Run.ps1
```

El script de build descarga el paquete oficial `Microsoft.Web.WebView2` desde NuGet, compila con el compilador de .NET Framework incluido en Windows y genera `bin\GXLightBrowser.exe`.

## Instalador

Desde la version `1.9`, GX Light cuenta con un instalador para Windows 10/11 x64. El instalador copia el
navegador completo y comprueba los requisitos que sistemas modificados como Atlas OS pueden eliminar:

- Microsoft Edge WebView2 Evergreen Runtime.
- Microsoft .NET Framework 4.8.
- `Microsoft.Web.WebView2.Core.dll`.
- `Microsoft.Web.WebView2.WinForms.dll`.
- `WebView2Loader.dll`.

Para construirlo:

```powershell
.\scripts\Build-Installer.ps1
```

El resultado queda en `dist\`. Consultar [docs/INSTALACION.md](docs/INSTALACION.md) para detalles de
compatibilidad y reparacion en Atlas OS.

El instalador publicado puede descargarse desde:

```text
https://github.com/wiimri/GX-lite/releases/download/v1.9/GXLightBrowser-Setup-1.9-x64.exe
```

## Extensiones

Abrir `Menu > Extensions` y usar una de estas opciones:

- `Importar extension desempaquetada...`
- `Ver extensiones instaladas`
- `Abrir Chrome Web Store`
- `Abrir Opera Addons`

Al importar, seleccionar:

- una carpeta de extension desempaquetada que contenga `manifest.json`, o
- una carpeta de version de una extension instalada en Chrome/Edge.

El importador copia la extension al perfil de GX Light y elimina carpetas tipo `_metadata`, porque WebView2 rechaza rutas de extension con carpetas que comienzan con `_`.

Importante: WebView2 permite cargar extensiones locales, pero no equivale a instalar directo desde Chrome Web Store con un click. Algunas extensiones que dependen de la UI propia de Chrome, de instalacion desde tienda o de APIs no soportadas pueden requerir adaptacion.

Los botones de tiendas son accesos de navegacion. La instalacion directa desde Chrome Web Store depende de la integracion tienda/navegador; por ahora el camino confiable es descargar o ubicar la carpeta de la extension e importarla.

## Favoritos

La barra de favoritos queda visible bajo la barra de navegacion.

- `Ctrl+D` guarda la pagina actual en favoritos.
- Click izquierdo abre el favorito en la pestana actual.
- Click del scroll abre el favorito en una nueva pestana interna.
- Click derecho permite abrir, copiar la direccion o eliminar el favorito.
- `Menu > Bookmarks` permite importar/exportar bookmarks HTML compatibles con navegadores.

## Passwords

El autosave/autofill nativo de WebView2 esta habilitado para el perfil de la app en `%LOCALAPPDATA%\GXLightBrowser\Profile`.

GX Light tambien incluye una boveda local para importar/exportar passwords:

- formato CSV: `name,url,username,password,note`
- las entradas importadas se guardan cifradas para el usuario actual de Windows con DPAPI
- al exportar se genera un CSV normal en texto visible, asi que hay que tratarlo con cuidado

Importante: WebView2 no expone una API publica para inyectar passwords importadas directamente en su gestor nativo. La boveda de GX Light sirve como companera de import/export; WebView2 sigue manejando el guardado y autofill normal durante la navegacion.

## Bloqueo de anuncios

La version actual incluye un bloqueador nativo que intercepta requests de WebView2 mediante `WebResourceRequested`. Soporta el subconjunto mas importante de reglas ABP/EasyList:

- `||domain.com^`
- reglas simples por substring de URL
- reglas basicas de permiso con `@@`
- comentarios y reglas cosmeticas se ignoran de forma segura

Para refrescar listas grandes de filtros:

```powershell
.\scripts\Update-Filters.ps1
```

## Camino hacia un bloqueador nivel Brave

El bloqueador de produccion de Brave se basa en el motor open source `brave/adblock-rust`. Este prototipo esta preparado para que la clase `AdBlocker` pueda reemplazarse mas adelante por una integracion nativa con `adblock-rust`.

Opciones posibles:

- toolchain de Rust mas un wrapper C ABI/UniFFI para C#, o
- binding Node `adblock-rs` como sidecar local.

El bloqueador actual ya intercepta requests de forma nativa y no depende de APIs Manifest V3 para su bloqueo incorporado.

## Privacy Firewall

GX Light Browser incluye un Privacy Firewall local a nivel navegador. No instala drivers, no modifica Windows Firewall y no actua como VPN; funciona dentro del pipeline de requests del navegador.

Actualmente:

- bloquea dominios conocidos de trackers, analytics, pixels, RUM, ad-tech y attribution
- bloquea requests de terceros que parecen telemetria, beacons, pixels o endpoints de tracking
- limpia parametros de seguimiento como `utm_*`, `fbclid`, `gclid`, `msclkid`, `mc_cid` y similares durante la navegacion
- mantiene excepciones de compatibilidad para recursos necesarios de reproduccion de YouTube y Crunchyroll

Esto no oculta tu IP ni reemplaza un bloqueador nativo completo como `adblock-rust`. Su objetivo es reducir superficies de seguimiento dentro del navegador.

## Verificacion UI

El chrome responsive del navegador esta reflejado en `docs\ui-preview.html` para revisarlo con Playwright:

```powershell
npm.cmd run test:ui
npm.cmd run test:firewall
```

Esto guarda screenshots en `screenshots\` y revisa que no haya overflow horizontal en escritorio ni modo compacto. Tambien verifica cierre de pestanas, click del scroll en accesos/favoritos y el script de YouTube Shields contra una pagina mock con anuncios.

La prueba de firewall revisa bloqueo de trackers y limpieza de parametros de seguimiento.

## Controles para bajos recursos

- La barra superior mantiene navegacion, direccion, uso de memoria, limitador de memoria y menu.
- El resto de funciones vive dentro de `Menu`.
- `GX 0.8G` abre GX Control con RAM limiter, hard limit, hot tabs killer, CPU policy y network policy.
- Las pestanas inactivas pasan a bajo consumo.
- `Menu > Suspend inactive tabs now` descarta WebViews inactivos y mantiene la pestana visible.

## Auditoria

Ver `AUDIT.md` para la revision actual de arquitectura, roadmap de bajos recursos, notas de memoria y prioridades siguientes.

## Fuentes revisadas

- APIs de extensiones de Microsoft WebView2: `CoreWebView2EnvironmentOptions.AreBrowserExtensionsEnabled`, `CoreWebView2Profile.AddBrowserExtensionAsync` y `GetBrowserExtensionsAsync`.
- Motor de bloqueo de Brave: `https://github.com/brave/adblock-rust`.
