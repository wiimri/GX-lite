# Historial de versiones

Este archivo es el historial estable de GX Light Browser. Sirve como seccion de versiones dentro de GitHub aunque la pagina de GitHub Releases todavia no tenga binarios publicados.

## Version actual

- Version publicada: `1.3`
- Fecha: `2026-06-08`
- Codigo fuente: <https://github.com/wiimri/GX-lite>
- Tags: <https://github.com/wiimri/GX-lite/tags>

## Como funciona el aviso de novedades

Desde la version `1.2`, el navegador lee el manifiesto remoto:

```text
https://raw.githubusercontent.com/wiimri/GX-lite/main/update.json
```

Ese archivo indica cual es la version publicada, el nombre de la release, los links y las novedades que debe mostrar `gxlight://updated`.

Si GitHub no responde, GX Light usa las notas locales compiladas como respaldo.

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
