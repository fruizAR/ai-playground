# Auto-Scroll en Tiempo Real

## 📜 Descripción

El componente de chat ahora incluye funcionalidad de auto-scroll que ajusta automáticamente la posición del scroll hacia abajo a medida que se reciben chunks de datos durante el streaming o cuando se agregan nuevos mensajes.

## 🎯 Funcionalidad

### Cuándo se Activa el Auto-Scroll

El scroll automático se activa en los siguientes casos:

1. **Al enviar un mensaje**: Cuando el usuario envía un mensaje
2. **Durante streaming**: Cada vez que se recibe un chunk de datos desde el backend
3. **Al completar streaming**: Cuando la respuesta completa ha sido recibida
4. **Modo sin streaming**: Cuando se recibe la respuesta completa
5. **Al recibir errores**: Cuando se muestra un mensaje de error

### Comportamiento

- ✅ **Suave y automático**: El scroll se ajusta sin interrumpir la experiencia del usuario
- ✅ **Solo cuando necesario**: Se activa solo durante la recepción de datos
- ✅ **Se desactiva automáticamente**: Después de completar el streaming

## 🔧 Implementación Técnica

### Componentes Clave

#### 1. ViewChild para Acceder al Contenedor

```typescript
@ViewChild('messagesContainer') private messagesContainer!: ElementRef;
```

Se usa `ViewChild` para obtener una referencia al elemento DOM del contenedor de mensajes.

#### 2. Variable de Control

```typescript
private shouldScrollToBottom = false;
```

Esta variable controla cuándo debe hacerse el scroll. Se activa cuando hay nuevos datos y se desactiva después del scroll.

#### 3. Lifecycle Hook: AfterViewChecked

```typescript
ngAfterViewChecked(): void {
  if (this.shouldScrollToBottom) {
    this.scrollToBottom();
  }
}
```

Este hook de Angular se ejecuta después de cada verificación de la vista, permitiendo hacer scroll cuando hay cambios.

#### 4. Método scrollToBottom

```typescript
private scrollToBottom(): void {
  try {
    if (this.messagesContainer) {
      const element = this.messagesContainer.nativeElement;
      element.scrollTop = element.scrollHeight;
    }
  } catch (err) {
    console.error('Error scrolling to bottom:', err);
  }
}
```

Este método realiza el scroll estableciendo `scrollTop` al valor de `scrollHeight` del contenedor.

### Template Reference

En el HTML, se agrega una referencia al contenedor:

```html
<div class="messages-container" #messagesContainer>
  <!-- Mensajes -->
</div>
```

## 📊 Flujo de Ejecución

### Modo Streaming

```
1. Usuario envía mensaje
   └─> shouldScrollToBottom = true

2. Se recibe chunk de datos
   └─> messages[index].content += chunk
   └─> shouldScrollToBottom = true
   └─> ngAfterViewChecked() → scrollToBottom()

3. Se recibe otro chunk
   └─> messages[index].content += chunk
   └─> shouldScrollToBottom = true
   └─> ngAfterViewChecked() → scrollToBottom()

4. Streaming completa
   └─> isStreaming = false
   └─> shouldScrollToBottom = true
   └─> setTimeout(() => shouldScrollToBottom = false, 100)
```

### Modo Sin Streaming

```
1. Usuario envía mensaje
   └─> shouldScrollToBottom = true

2. Se recibe respuesta completa
   └─> messages.push(assistantMessage)
   └─> shouldScrollToBottom = true
   └─> ngAfterViewChecked() → scrollToBottom()
   └─> setTimeout(() => shouldScrollToBottom = false, 100)
```

## 🎨 Experiencia del Usuario

### Antes (Sin Auto-Scroll)

```
[Mensaje Usuario]
[Asistente: "Hola..."]
                        ← Usuario ve solo el inicio
                        ← Debe hacer scroll manualmente
                        ← Pierde el contexto del streaming
```

### Después (Con Auto-Scroll)

```
[Mensaje Usuario]
[Asistente: "Hola, te voy a explicar..."]
[continuando con más texto...]
[y más texto que va apareciendo]
[chunk por chunk en tiempo real...] ← Usuario ve siempre lo más reciente
```

## 🔍 Casos de Uso

### 1. Respuestas Largas con Streaming

Cuando OpenAI genera una respuesta larga con código, listas y explicaciones:

- El usuario ve cada palabra aparecer en tiempo real
- El scroll se ajusta automáticamente para seguir el texto
- No necesita hacer scroll manual durante el streaming

### 2. Conversaciones Largas

En sesiones de chat extensas con múltiples mensajes:

- Cada nuevo mensaje hace scroll automático
- El usuario siempre ve el mensaje más reciente
- Facilita seguir el flujo de la conversación

### 3. Código Generado

Cuando se genera código extenso:

```python
def función_compleja():
    # El usuario ve cada línea aparecer
    # Y el scroll se ajusta automáticamente
    # Para mostrar siempre la última línea generada
```

## ⚙️ Configuración

### Desactivar Auto-Scroll (Si se Requiere)

Si en el futuro se desea permitir al usuario desactivar el auto-scroll:

```typescript
// Agregar propiedad
autoScrollEnabled: boolean = true;

// Modificar ngAfterViewChecked
ngAfterViewChecked(): void {
  if (this.shouldScrollToBottom && this.autoScrollEnabled) {
    this.scrollToBottom();
  }
}

// Agregar toggle en el template
<label class="toggle-label">
  <input type="checkbox" [(ngModel)]="autoScrollEnabled" />
  <span>Auto-scroll</span>
</label>
```

### Ajustar Velocidad de Scroll

Si se desea un scroll animado en lugar de instantáneo:

```typescript
private scrollToBottom(): void {
  try {
    if (this.messagesContainer) {
      const element = this.messagesContainer.nativeElement;
      element.scrollTo({
        top: element.scrollHeight,
        behavior: 'smooth' // Scroll suave
      });
    }
  } catch (err) {
    console.error('Error scrolling to bottom:', err);
  }
}
```

⚠️ **Nota**: El scroll suave (`smooth`) puede causar retrasos en streaming rápido. El scroll instantáneo es preferible para streaming en tiempo real.

## 🐛 Troubleshooting

### El scroll no funciona

**Síntomas**: El scroll no se mueve cuando llegan mensajes

**Soluciones**:

1. Verificar que `#messagesContainer` esté en el template
2. Verificar que `@ViewChild` esté correctamente configurado
3. Revisar la consola para errores

### El scroll es muy lento

**Síntomas**: El scroll no sigue el ritmo del streaming

**Solución**: Asegurarse de usar scroll instantáneo, no smooth:

```typescript
element.scrollTop = element.scrollHeight; // Instantáneo ✓
element.scrollTo({ top: ..., behavior: 'smooth' }); // Suave ✗ (para streaming)
```

### El scroll interrumpe al usuario

**Síntomas**: El scroll se mueve incluso cuando el usuario está leyendo arriba

**Solución futura**: Detectar si el usuario ha hecho scroll manual:

```typescript
private userScrolledUp = false;

onScroll(event: any): void {
  const element = event.target;
  const threshold = 150;
  const position = element.scrollTop + element.offsetHeight;
  const height = element.scrollHeight;
  this.userScrolledUp = (height - position > threshold);
}

ngAfterViewChecked(): void {
  if (this.shouldScrollToBottom && !this.userScrolledUp) {
    this.scrollToBottom();
  }
}
```

## 📝 Beneficios

1. **Mejor UX**: El usuario no pierde el contexto durante el streaming
2. **Más Natural**: El chat se comporta como aplicaciones de mensajería familiares
3. **Sin Intervención Manual**: No necesita hacer scroll durante conversaciones
4. **Responsive**: Funciona en desktop y móvil
5. **Eficiente**: Solo se activa cuando hay cambios reales

## 🚀 Mejoras Futuras

- [ ] Detectar scroll manual del usuario y pausar auto-scroll
- [ ] Agregar botón "Volver abajo" cuando el usuario hace scroll arriba
- [ ] Opción para desactivar auto-scroll en configuración
- [ ] Animación suave opcional
- [ ] Indicador visual de nuevos mensajes cuando el scroll está arriba
