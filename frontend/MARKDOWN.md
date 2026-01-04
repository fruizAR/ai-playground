# Soporte de Markdown en el Chat

El componente de chat ahora soporta renderizado completo de Markdown, lo que permite mostrar respuestas formateadas de manera rica y profesional.

## Características Soportadas

### 1. Texto Básico

- **Negrita**: `**texto en negrita**` o `__texto en negrita__`
- _Cursiva_: `*texto en cursiva*` o `_texto en cursiva_`
- ~~Tachado~~: `~~texto tachado~~`

### 2. Encabezados

```markdown
# Encabezado H1

## Encabezado H2

### Encabezado H3

#### Encabezado H4

##### Encabezado H5

###### Encabezado H6
```

### 3. Listas

**Lista no ordenada:**

```markdown
- Elemento 1
- Elemento 2
  - Sub-elemento 2.1
  - Sub-elemento 2.2
- Elemento 3
```

**Lista ordenada:**

```markdown
1. Primer elemento
2. Segundo elemento
3. Tercer elemento
```

### 4. Código

**Código en línea:**

```markdown
Usa `código en línea` para comandos o variables.
```

**Bloques de código:**

````markdown
```python
def saludar(nombre):
    return f"Hola, {nombre}!"
```
````

````markdown
```javascript
function saludar(nombre) {
  return `Hola, ${nombre}!`;
}
```
````

### 5. Enlaces

```markdown
[Texto del enlace](https://ejemplo.com)
[GitHub](https://github.com)
```

### 6. Citas

```markdown
> Esta es una cita.
> Puede tener varias líneas.
>
> Y múltiples párrafos.
```

### 7. Tablas

```markdown
| Columna 1 | Columna 2 | Columna 3 |
| --------- | --------- | --------- |
| Dato 1    | Dato 2    | Dato 3    |
| Dato 4    | Dato 5    | Dato 6    |
```

### 8. Líneas horizontales

```markdown
---

---

---
```

### 9. Imágenes

```markdown
![Texto alternativo](url-de-la-imagen.jpg)
```

## Implementación Técnica

### Librería Utilizada

- **ngx-markdown**: Librería Angular para renderizado de Markdown
- **marked**: Parser de Markdown subyacente

### Instalación

```bash
npm install ngx-markdown marked
```

### Configuración en AppModule

```typescript
import { MarkdownModule } from "ngx-markdown";

@NgModule({
  imports: [MarkdownModule.forRoot()],
})
export class AppModule {}
```

### Uso en el Componente

```html
<markdown [data]="message.content"></markdown>
```

## Estilos Personalizados

Los estilos de Markdown están personalizados para adaptarse al tema del chat:

- **Bloques de código**: Fondo gris claro con bordes redondeados
- **Código en línea**: Fondo semitransparente con padding
- **Enlaces**: Color morado (#667eea) que combina con el tema
- **Tablas**: Bordes sutiles con encabezados resaltados
- **Citas**: Borde izquierdo morado con padding

### Diferenciación por Rol

Los mensajes de usuario (con fondo degradado morado) tienen estilos ajustados:

- Código en línea con fondo blanco semitransparente
- Bloques de código con fondo oscuro semitransparente
- Enlaces con color blanco y subrayado

## Ejemplos de Uso

### Ejemplo 1: Documentación de Código

Pregunta: "¿Cómo creo un componente en Angular?"

Respuesta formateada:

````markdown
# Crear un Componente en Angular

Para crear un componente nuevo, usa el Angular CLI:

```bash
ng generate component nombre-componente
```
````

Esto creará:

- `nombre-componente.component.ts` - Lógica del componente
- `nombre-componente.component.html` - Plantilla
- `nombre-componente.component.css` - Estilos
- `nombre-componente.component.spec.ts` - Tests

## Estructura Básica

```typescript
import { Component } from "@angular/core";

@Component({
  selector: "app-nombre-componente",
  templateUrl: "./nombre-componente.component.html",
  styleUrls: ["./nombre-componente.component.css"],
})
export class NombreComponenteComponent {
  // Tu código aquí
}
```

````

### Ejemplo 2: Comparación con Tabla
Pregunta: "Compara Python y JavaScript"

Respuesta formateada:
```markdown
## Comparación Python vs JavaScript

| Característica | Python | JavaScript |
|----------------|--------|------------|
| Tipado | Dinámico | Dinámico |
| Uso Principal | Backend, Data Science | Frontend, Backend |
| Sintaxis | Limpia, legible | Basada en C |
| Async | async/await | async/await, Promises |

### Ventajas de Python
- Sintaxis clara y legible
- Excelente para ciencia de datos
- Gran ecosistema de librerías

### Ventajas de JavaScript
- Corre en el navegador
- Node.js para backend
- Ecosistema npm muy amplio
````

### Ejemplo 3: Tutorial con Pasos

Pregunta: "¿Cómo configuro un proyecto FastAPI?"

Respuesta formateada:

````markdown
# Configuración de FastAPI

## Pasos de Instalación

1. **Crear entorno virtual**
   ```bash
   python -m venv venv
   source venv/bin/activate  # Linux/Mac
   venv\Scripts\activate     # Windows
   ```
````

2. **Instalar FastAPI y Uvicorn**

   ```bash
   pip install fastapi uvicorn
   ```

3. **Crear archivo principal** (`main.py`)

   ```python
   from fastapi import FastAPI

   app = FastAPI()

   @app.get("/")
   def read_root():
       return {"Hello": "World"}
   ```

4. **Ejecutar el servidor**
   ```bash
   uvicorn main:app --reload
   ```

> **Nota**: El flag `--reload` reinicia automáticamente al detectar cambios.

¡Tu API estará disponible en http://localhost:8000!

````

## Beneficios del Soporte Markdown

1. **Mejor Legibilidad**: El contenido formateado es más fácil de leer y entender
2. **Código Destacado**: Los bloques de código tienen syntax highlighting y formato
3. **Estructura Clara**: Los encabezados y listas organizan la información
4. **Profesionalismo**: Las respuestas se ven más pulidas y profesionales
5. **Compatibilidad**: Markdown es un estándar ampliamente usado

## Notas Importantes

- El renderizado se hace en el lado del cliente (navegador)
- Los estilos están optimizados tanto para mensajes del usuario como del asistente
- El markdown se procesa de manera segura (sin permitir HTML arbitrario por defecto)
- El componente soporta markdown en tiempo real durante el streaming

## Troubleshooting

### El markdown no se renderiza
- Verifica que `MarkdownModule.forRoot()` esté en los imports de AppModule
- Asegúrate de que las dependencias estén instaladas: `npm install`

### Los estilos no se aplican
- Los estilos usan `::ng-deep` para penetrar el encapsulamiento de Angular
- Verifica que `chat.component.css` contenga los estilos de markdown

### Problemas con código
- Los bloques de código deben usar triple backtick (```)
- El código en línea usa single backtick (`)
````
