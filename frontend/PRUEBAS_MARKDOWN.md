# Pruebas de Markdown en el Chat

Este documento contiene ejemplos de prompts que puedes usar para probar el renderizado de Markdown en el chat.

## 🧪 Prompts de Prueba

### 1. Código Python

```
Escribe una función en Python que calcule el factorial de un número
```

Respuesta esperada con código formateado:

```python
def factorial(n):
    if n == 0:
        return 1
    return n * factorial(n - 1)
```

### 2. Lista de Pasos

```
Dame los pasos para crear un proyecto Angular
```

Respuesta esperada con lista ordenada:

1. Instalar Angular CLI
2. Crear nuevo proyecto
3. Configurar dependencias
4. Ejecutar servidor de desarrollo

### 3. Tabla Comparativa

```
Compara las diferencias entre let, const y var en JavaScript
```

Respuesta esperada con tabla markdown

### 4. Código Multi-lenguaje

```
Muéstrame cómo hacer una petición HTTP en Python, JavaScript y C#
```

Respuesta esperada con múltiples bloques de código

### 5. Documentación con Formato Rico

```
Explícame qué es FastAPI y cómo usarlo
```

Respuesta esperada con:

- Encabezados
- Negrita/cursiva
- Listas
- Bloques de código
- Citas

### 6. Tutorial Paso a Paso

```
Cómo crear una API REST con FastAPI paso a paso
```

Respuesta esperada con:

- Encabezados numerados
- Bloques de código
- Notas destacadas
- Listas de verificación

## ✅ Verificación Visual

Al probar estos prompts, verifica que:

1. **Código en línea**: Las palabras con \`backticks\` tienen fondo gris
2. **Bloques de código**: Tienen fondo destacado y font monospace
3. **Encabezados**: Son más grandes y tienen peso bold
4. **Listas**: Tienen viñetas o números correctos
5. **Tablas**: Tienen bordes y encabezados destacados
6. **Enlaces**: Tienen color morado y son clickeables
7. **Citas**: Tienen borde izquierdo morado
8. **Negrita/Cursiva**: Se aplica correctamente

## 🎨 Diferencias por Rol

### Mensajes del Usuario (fondo morado)

- Código en línea: fondo blanco semitransparente
- Bloques de código: fondo oscuro semitransparente
- Enlaces: blancos con subrayado
- Tablas: encabezados con fondo oscuro semitransparente

### Mensajes del Asistente (fondo blanco)

- Código en línea: fondo gris
- Bloques de código: fondo gris claro
- Enlaces: color morado (#667eea)
- Tablas: encabezados con fondo gris

## 🚀 Prompts de Ejemplo Completos

### Ejemplo 1: Tutorial Completo

```
Dame un tutorial completo de cómo crear una API REST con FastAPI que incluya:
- Instalación
- Estructura del proyecto
- Creación de endpoints
- Modelos con Pydantic
- Documentación automática
- Ejemplo de código completo
```

### Ejemplo 2: Comparación Técnica

```
Compara Python, JavaScript y TypeScript en una tabla que incluya:
- Tipado
- Rendimiento
- Ecosistema
- Casos de uso principales
- Ventajas y desventajas de cada uno
```

### Ejemplo 3: Guía de Mejores Prácticas

```
Dame las mejores prácticas para desarrollar aplicaciones Angular, incluyendo:
- Estructura de carpetas
- Nombres de archivos
- Separación de responsabilidades
- Manejo de estado
- Optimización de rendimiento
Con ejemplos de código cuando sea relevante
```

### Ejemplo 4: Debugging y Solución de Problemas

```
Tengo un error "CORS policy" al llamar a mi API desde Angular.
Explícame qué es CORS, por qué ocurre este error y cómo solucionarlo tanto en:
- Backend FastAPI (Python)
- Backend ASP.NET Core (C#)
- Frontend Angular
Con ejemplos de código para cada caso
```

## 📊 Funcionalidades Markdown Soportadas

### Texto Básico

- **Negrita**: `**texto**` o `__texto__`
- _Cursiva_: `*texto*` o `_texto_`
- ~~Tachado~~: `~~texto~~`
- `Código`: \`código\`

### Estructura

- Encabezados: `# H1`, `## H2`, `### H3`, etc.
- Listas ordenadas: `1. Item`
- Listas no ordenadas: `- Item`
- Citas: `> Cita`
- Líneas: `---`

### Código

- Código en línea: \`código\`
- Bloques de código: \`\`\`python ... \`\`\`

### Enlaces y Referencias

- Enlaces: `[texto](url)`
- Imágenes: `![alt](url)`

### Tablas

```markdown
| Col1 | Col2 |
| ---- | ---- |
| A    | B    |
```

## 🎯 Tests Recomendados

1. **Test básico**: Enviar "Hola, muestra código Python"
2. **Test de lista**: Enviar "Dame una lista de frameworks JavaScript"
3. **Test de tabla**: Enviar "Compara Angular, React y Vue"
4. **Test de código completo**: Enviar "Crea una API completa de tareas con FastAPI"
5. **Test de streaming**: Activar streaming y enviar un prompt largo

## 💡 Tips

- El markdown se renderiza tanto en modo streaming como no-streaming
- Los estilos se adaptan automáticamente al tema (usuario vs asistente)
- El código tiene scroll horizontal si es muy largo
- Las tablas son responsivas
