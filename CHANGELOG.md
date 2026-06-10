# Historial de versiones

Este archivo es el historial estable de GX Light Browser. Sirve como seccion de versiones dentro de GitHub aunque la pagina de GitHub Releases todavia no tenga binarios publicados.

## Version actual

- Version publicada: `1.13`
- Fecha: `2026-06-10`
- Codigo fuente: <https://github.com/wiimri/GX-lite>
- Tags: <https://github.com/wiimri/GX-lite/tags>

## Como funciona el aviso de novedades

Desde la version `1.2`, el navegador lee el manifiesto remoto:

```text
https://raw.githubusercontent.com/wiimri/GX-lite/main/update.json
```

Ese archivo indica cual es la version publicada, el nombre de la release, los links y las novedades que debe mostrar `gxlight://updated`.

Si GitHub no responde, GX Light usa las notas locales compiladas como respaldo.

## v1.13 - Instalacion en Program Files y actualizador verificado

Fecha: `2026-06-10`

Cambios:

- El instalador usa `{autopf}\GXLightBrowser`, que corresponde a la carpeta nativa Program Files.
- La instalacion solicita permisos de administrador y deja de reutilizar la antigua ruta en LocalAppData.
- La migracion elimina solamente los binarios y accesos directos antiguos; el perfil del usuario permanece intacto.
- GX Light detecta automaticamente una version remota mayor al iniciar.
- `Menu > Buscar actualizaciones` descarga el instalador en vez de abrir solamente un enlace.
- La descarga se valida contra el SHA-256 publicado antes de abrir el instalador.
- El proceso guarda la sesion, abre el instalador y cierra GX Light para permitir reemplazar los binarios.
- El build genera hashes permanentes y versionados para GitHub Releases.

Pruebas:

- Compilacion del ejecutable completada.
- Pruebas Playwright y Privacy Firewall ejecutadas.
- Instalador construido y comprobado sobre la ruta de programas de Windows.

## v1.12 - Islas colapsables, favicons y actualizaciones visibles

Fecha: `2026-06-10`

Cambios:

- El ajuste de passwords ahora se llama `Preguntar antes de guardar passwords` y explica que solo se guarda despues de aceptar el popup nativo.
- Las credenciales nativas quedan bajo la proteccion del perfil de Windows y la boveda importada conserva DPAPI.
- Las pestanas suspendidas usan el indicador corto `[S]`.
- Los favicons se vuelven a consultar al completar cada navegacion.
- Se agregaron tamanos de pestana automatico, pequeno, mediano y grande.
- `Ctrl+clic` selecciona pestanas individuales y `Shift+clic` selecciona rangos.
- Las islas nuevas se colapsan en una barra vertical y pueden desplegarse, colapsarse o disolverse.
- El estado colapsado de las islas se conserva entre sesiones.
- `Menu > Buscar actualizaciones` consulta GitHub y ofrece descargar el instalador permanente.
- Se agrego `docs/SEGURIDAD.md` con reglas actuales y prioridades de seguridad.

Pruebas:

- Compilacion de .NET Framework completada.
- Pruebas Playwright de UI y aislamiento de YouTube ejecutadas.
- Prueba del Privacy Firewall ejecutada.

## v1.11 - Passwords persistentes y restauracion ligera de sesion

Fecha: `2026-06-09`

Cambios:

- El guardado de passwords se activa tanto en `CoreWebView2.Settings` como en `CoreWebView2.Profile`.
- Nuevo interruptor `Guardar passwords automaticamente` dentro del menu de passwords.
- El cierre de GX Light guarda configuración y sesión, detiene tareas periódicas y dispone limpiamente los WebViews.
- `session.dat` usa formato v2 con URL y título codificados, evitando fallos por caracteres como `|`.
- Nuevo interruptor `Guardar pestanas al cerrar`.
- Al restaurar una sesión, solamente la pestaña seleccionada crea un WebView; todas las demás quedan suspendidas hasta abrirlas.

Pruebas:

- El log confirmó `Password autosave=True` sobre el perfil persistente de GX Light.
- Una prueba controlada reinició tres pestañas y conservó URLs/títulos que contenían separadores.
- La sesión y configuración originales fueron respaldadas y restauradas después de la prueba.

## v1.10 - Modo de compatibilidad Crunchyroll

Fecha: `2026-06-09`

Cambios:

- Crunchyroll activa automaticamente un modo de compatibilidad que pausa el bloqueo de recursos para ese sitio.
- El modo de compatibilidad sigue bloqueando ventanas emergentes automáticas.
- GX Light usa el host de la navegación iniciada para evaluar los primeros recursos, incluso cuando WebView2 todavía reporta `about:blank`.
- Los recursos bloqueados y popups bloqueados se cuentan por separado.
- La última URL realmente bloqueada queda visible en la barra de estado y registrada en el log.
- El instalador también se publica como `GXLightBrowser-Setup-x64.exe`, habilitando un enlace permanente a la última versión.

Pruebas:

- Dos pruebas controladas de Crunchyroll permanecieron abiertas durante 25 segundos sin registrar el `HTTP 403` anterior.
- La sesión original fue respaldada y restaurada automáticamente después de cada prueba.

## v1.9 - Instalador con requisitos para Windows y Atlas OS

Fecha: `2026-06-09`

Cambios:

- Nuevo instalador x64 construido con Inno Setup.
- El instalador copia el ejecutable y las tres bibliotecas requeridas por WebView2.
- Se detecta e instala Microsoft Edge WebView2 Evergreen Runtime cuando falta.
- Se detecta e instala Microsoft .NET Framework 4.8 cuando falta.
- GX Light comprueba WebView2 antes de crear pestañas y muestra instrucciones de reparación si no esta disponible.
- Nueva guia `docs/INSTALACION.md` para Windows y Atlas OS.

Notas:

- El instalador usa los bootstrapper oficiales de Microsoft.
- Atlas OS puede impedir la instalación si fueron deshabilitados servicios esenciales de Microsoft.
- La distribución actual requiere Windows 10/11 x64.

## v1.8 - Aislamiento de YouTube Shields y diagnostico de Crunchyroll

Fecha: `2026-06-09`

Cambios:

- YouTube Shields termina inmediatamente fuera de `youtube.com` y `youtu.be`.
- El `MutationObserver` espera a que exista `document.documentElement`, corrigiendo el error mostrado en DevTools.
- Se agrego una prueba Playwright que abre una pagina simulada de Crunchyroll y comprueba que YouTube Shields no se instale ni genere errores.
- La barra de estado identifica cuando Crunchyroll responde `HTTP 403` y aclara que no fue bloqueado por Shields.
- El diagnostico del rechazo queda registrado en el log local.

Notas:

- La prueba con la sesion real confirmo que Crunchyroll esta respondiendo `HTTP 403` desde el servidor.
- GX Light no intentara evadir restricciones de acceso o DRM del servicio.

## v1.7 - Atajos, favicons, Playlist y compatibilidad multimedia

Fecha: `2026-06-09`

Cambios:

- `Ctrl+W` se procesa a nivel del formulario aunque una pagina WebView2 tenga el foco.
- El ejecutable y la ventana incorporan un icono propio de GX Light.
- WebView2 entrega los favicons de cada pagina a la barra de pestanas.
- `Menu > Tab appearance` permite mostrar u ocultar favicons y activar pestanas compactas cuadradas.
- YouTube Shields ya no elimina el contenedor principal de anuncios ni fuerza saltos de tiempo agresivos; intenta mantener y recuperar la reproduccion.
- Las solicitudes multimedia necesarias de YouTube y Crunchyroll tienen excepciones de compatibilidad limitadas al sitio activo.
- Se agrego una Playlist local para guardar, abrir y eliminar paginas multimedia.

Notas:

- La Playlist guarda enlaces; no descarga ni evita contenido protegido por DRM.
- Crunchyroll puede seguir rechazando sesiones por reglas propias del servicio o limitaciones DRM de WebView2.

## v1.6 - Correccion de eliminacion de bookmarks y carpetas importadas

Fecha: `2026-06-08`

Cambios:

- Se corrigio `Eliminar todos` en el gestor de bookmarks.
- Los comandos internos de bookmarks ya no dependen solo de que WebView2 reporte el origen como `data:text/html`.
- Si la pestana interna aparece como `gxlight://home`, los comandos siguen siendo aceptados mientras la pestana activa sea interna.
- La importacion de bookmarks HTML conserva carpetas anidadas como rutas `Padre / Hija`.
- La exportacion HTML agrupa favoritos por carpeta.
- README, CHANGELOG y `update.json` quedan sincronizados con la version actual.

Notas:

- Esta version corrige el caso donde aparecia el dialogo de confirmacion, pero al aceptar no se eliminaban los favoritos.

## v1.5 - Gestion avanzada de favoritos

Fecha: `2026-06-08`

Cambios:

- Se agrego seleccion multiple de bookmarks con checkboxes en el gestor de favoritos.
- Nuevo boton "Seleccionar todos / Deseleccionar todos" en la barra de herramientas.
- Nuevo boton "Eliminar seleccionados" que elimina multiples favoritos con una sola confirmacion.
- Nuevo boton "Eliminar todos" para borrar todos los favoritos de una vez con confirmacion.
- La tecla Suprimir (Delete) elimina los favoritos seleccionados directamente.
- Ctrl+A selecciona o deselecciona todos los favoritos visibles.
- Eliminacion individual ya no requiere confirmacion extra (un solo clic).
- Se agrego checkbox maestro en la cabecera de la tabla.
- Las filas seleccionadas se resaltan con un fondo distinto.
- Se agrego indicador de cuantos favoritos estan seleccionados.
- Se agrego un tip visible sobre las teclas Suprimir y Ctrl+A.

Notas:

- La version 1.4 ya habia incluido importacion con jerarquia de carpetas y remocion de Opera Addons.
- Esta version completa la experiencia de gestion masiva de favoritos.

## v1.4 - Bookmarks con carpetas y mejoras de UI

Fecha: `2026-06-08`

Cambios:

- Se mejoro la importacion de bookmarks HTML conservando la jerarquia original de carpetas.
- La barra de favoritos ahora muestra carpetas como botones desplegables con dropdown.
- Se agrego menu contextual en carpetas de la barra de favoritos.
- Se removio el boton de Opera Addons ya que no era funcional.
- El boton de menu principal ahora usa el icono hamburguesa estandar.

Notas:

- La importacion ahora usa un parser basado en stack para respetar la estructura DL/DT/DD del HTML de bookmarks.

## v1.3 - Correccion de links desde novedades

Fecha: `2026-06-08`

Cambios:

- Se corrigio la apertura de `Ver release` y `Abrir GitHub` desde la pestana de novedades.
- Las paginas internas ahora pueden pedir al host que navegue explicitamente a una URL externa.
- El canal `gxlight:navigate` esta limitado a documentos internos generados por GX Light.
- El navegador sigue leyendo `update.json` desde GitHub al iniciar.
- Este cambio prepara mejor la experiencia para futuros releases descargables.

Notas:

- Esta version corrige el caso donde la barra de direccion cambiaba a GitHub, pero el contenido seguia mostrando la pagina interna `data:text/html`.
- El boton `Ver release` apunta a este historial para evitar caer en una pagina de Releases vacia.

## v1.2 - Manifiesto remoto de actualizaciones

Fecha: `2026-06-08`

Cambios:

- Se agrego `update.json` en GitHub como fuente remota de novedades.
- La pestana `gxlight://updated` puede mostrar novedades editadas desde GitHub.
- Si GitHub no responde, el navegador usa notas locales de respaldo.
- El aviso sigue apareciendo solo una vez por version publicada.
- Se preparo el camino para un actualizador binario futuro.

Limitacion:

- Esta version no reemplaza automaticamente el `.exe`; solo agrega el canal remoto de informacion.

## v1.1 - Favoritos, tab islands y passwords import/export

Fecha: `2026-06-08`

Cambios:

- Se agrego barra de favoritos.
- `Ctrl+D` guarda la pagina actual como favorito.
- Click del scroll en favoritos abre una nueva pestana interna.
- Se agrego importacion/exportacion de bookmarks en HTML compatible con navegadores.
- Se agrego menu contextual de pestanas para seleccionar multiples pestanas.
- Se agrego creacion de tab islands coloreadas desde pestanas seleccionadas.
- Se agrego importacion/exportacion de passwords CSV mediante boveda local protegida con Windows DPAPI.
- Se actualizaron pruebas Playwright para cubrir la barra de favoritos.

Limitacion:

- WebView2 no expone API publica para inyectar passwords importadas directamente al gestor nativo; la boveda local funciona como companera de import/export.

## v1.0 - Base versionada del navegador

Fecha: `2026-06-08`

Cambios:

- Base Windows ligera con WebView2.
- Pestanas, cierre con `x`, click del scroll y atajos basicos.
- Menu principal con historial, descargas, extensiones, passwords, memoria, shields y settings.
- GX Control configurable con RAM limiter, hard limit, hot tabs killer, CPU policy y network policy.
- Suspension real de pestanas para liberar WebView2 y memoria.
- Privacy Firewall local y bloqueo nativo de trackers/anuncios.
- Pestana de novedades que aparece solo una vez por version instalada.
- Pruebas Playwright para UI responsive, tabs y YouTube Shields.

## Proximo paso

Para que tu viejo pueda recibir mejoras sin reemplazar manualmente el ejecutable, falta crear un actualizador binario:

- publicar un `.zip` o instalador por version en GitHub Releases
- descargar el release mas reciente desde el navegador o un helper externo
- cerrar GX Light de forma segura
- reemplazar el `.exe`
- volver a abrir el navegador

Ese paso debe hacerse con cuidado para no romper perfiles ni extensiones.
