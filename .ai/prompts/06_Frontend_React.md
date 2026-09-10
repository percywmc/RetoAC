```markdown
# Prompt 6: Cliente Web (React)

Genera un cliente web en **React 16+** (usando componentes funcionales y hooks) que consuma el API Gateway. Aplica una estructura de carpetas clara (`components/`, `pages/`, `services/`, `hooks/`, `context/`).

## Requerimientos generales
- Cliente HTTP centralizado (ej. `axios` con interceptor) que adjunte el JWT en el header `Authorization: Bearer` de cada request al Gateway.
- Manejo de sesión con Context API o similar (guardar/limpiar token en `localStorage`, redirigir a login si el token expira o el Gateway responde `401`).
- Uso de refresh token: si una petición retorna `401`, intenta refrescar el token vía `/api/v1/auth/refresh` antes de forzar logout.

## Pantallas requeridas

### 1. Login
- Formulario de email/contraseña.
- Llama a `POST /api/v1/auth/login`, guarda el JWT y el refresh token, redirige al historial de cargas.
- Manejo de errores (credenciales inválidas).

### 2. Subida de Excel
- Formulario para seleccionar un archivo `.xlsx` y enviarlo a `POST /api/v1/cargas` (multipart/form-data).
- Validación en cliente: extensión `.xlsx` y tamaño máximo antes de enviar.
- Muestra feedback de éxito/error y redirige al historial tras la subida.

### 3. Historial de cargas (tabla)
- Consume `GET /api/v1/cargas` (paginado).
- Tabla con columnas: NombreArchivo, Usuario, FechaRegistro, Estado, FechaFin.
- Colorea visualmente el estado (badge) según: Pendiente, En proceso, Cargado, Finalizado, Notificado.
- **Polling:** refresca la lista cada X segundos (ej. `setInterval` con `useEffect`, 5-10s) mientras existan cargas en estados no terminales (Pendiente/En proceso/Cargado).

### 4. Detalle del estado de una carga
- Consume `GET /api/v1/cargas/{id}` para mostrar el detalle completo y su estado actual con polling similar al historial.
- Consume `GET /api/v1/cargas/{id}/contenido` para previsualizar el contenido del Excel subido (tabla con las filas/columnas extraídas).

## Buenas prácticas
- Componentización (ej. `EstadoBadge`, `TablaHistorial`, `FormularioSubida`).
- Manejo de estados de carga (loading/error) en cada pantalla.
- Variables de entorno (`.env`) para la URL base del Gateway.
```
