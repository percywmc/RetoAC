```markdown
# Prompt 7: Colección Postman

Genera una colección de Postman (formato `Collection v2.1`, archivo `AtlanticCity.postman_collection.json`) que cubra el 100% de los endpoints expuestos por el API Gateway, organizada en carpetas por microservicio.

## Estructura de la colección

### Carpeta: Auth
- `POST {{gateway_url}}/api/v1/auth/login` — body con email/password, guarda `token` y `refreshToken` en variables de colección mediante un script de test (`pm.collectionVariables.set`).
- `POST {{gateway_url}}/api/v1/auth/refresh` — usa `{{refreshToken}}` guardado.

### Carpeta: Cargas (Microservicio de Control)
- `POST {{gateway_url}}/api/v1/cargas` — sube un archivo `.xlsx` (form-data), header `Authorization: Bearer {{token}}`.
- `GET {{gateway_url}}/api/v1/cargas` — historial paginado (query params `page`, `pageSize`).
- `GET {{gateway_url}}/api/v1/cargas/{{idCarga}}` — detalle de una carga.
- `GET {{gateway_url}}/api/v1/cargas/{{idCarga}}/contenido` — contenido del Excel subido.

## Variables de entorno (archivo `AtlanticCity.postman_environment.json`)
- `gateway_url` (ej. `http://localhost:5000`)
- `token`, `refreshToken`, `idCarga` (se completan automáticamente vía scripts de test de las requests de Login y Subida).

## Scripts de test
- En `Login`: extraer `token`/`refreshToken` de la respuesta y guardarlos como variables de colección.
- En `POST /api/v1/cargas`: extraer el `idCarga` devuelto y guardarlo como variable de colección para encadenar las siguientes requests automáticamente.
- Asserts básicos de status code (200/201) en cada request.

Entrega ambos archivos JSON (colección + entorno) listos para importar en Postman, documentados con descripciones en cada request explicando su propósito y precondiciones (ej. "requiere haber ejecutado Login antes").
```
