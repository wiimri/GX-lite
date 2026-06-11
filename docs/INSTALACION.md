# Instalacion de Gan Browser

## Opcion recomendada

Usar `GXLightBrowser-Setup-x64.exe` desde la ultima release.

El instalador:

- copia `GXLightBrowser.exe` y las tres bibliotecas requeridas de WebView2;
- comprueba Microsoft .NET Framework 4.8;
- comprueba Microsoft Edge WebView2 Evergreen Runtime;
- instala silenciosamente los requisitos faltantes usando instaladores oficiales de Microsoft;
- crea accesos directos y un desinstalador.
- instala Gan Browser en la carpeta de programas nativa de Windows (`{autopf}\GXLightBrowser`).

La instalacion solicita permisos de administrador porque escribe en `Program Files`.

Al actualizar desde una version antigua instalada en `%LOCALAPPDATA%\Programs\GXLightBrowser`, la version
nueva se instala en `Program Files`. El perfil de usuario no se mueve ni se elimina porque permanece en
`%LOCALAPPDATA%\GXLightBrowser`. El instalador elimina solamente la carpeta binaria antigua y sus accesos
directos obsoletos para evitar abrir accidentalmente una version anterior.

## Atlas OS y Windows modificados

Atlas OS y otros sistemas modificados pueden eliminar WebView2 o deshabilitar Microsoft Edge Update.
El instalador intenta reparar WebView2 automáticamente.

Si WebView2 no puede instalarse:

1. comprobar que Windows Update, Microsoft Edge Update y los servicios de instalacion no esten bloqueados;
2. ejecutar nuevamente el instalador de Gan Browser;
3. instalar manualmente Microsoft Edge WebView2 Evergreen Runtime desde:
   <https://developer.microsoft.com/microsoft-edge/webview2/>

Gan Browser muestra un mensaje claro al iniciar cuando WebView2 sigue ausente.

## Compatibilidad

- Windows 10 u 11 de 64 bits.
- Procesadores x64.
- Conexion a Internet durante la primera instalacion de requisitos.

El instalador online no garantiza funcionamiento en versiones de Windows severamente modificadas que
bloqueen componentes, servicios o instaladores oficiales de Microsoft.

## Construir el instalador

Requiere Inno Setup 6:

```powershell
.\scripts\Build-Installer.ps1
```

El resultado se genera en `dist\`.
