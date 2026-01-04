import {
  AfterViewChecked,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
} from "@angular/core";
import { AskRequest, Message, StatusResponse } from "../../models/chat.models";
import { Subject, Subscription } from "rxjs";

import { OpenAIService } from "../../services/openai.service";

@Component({
  selector: "app-chat",
  templateUrl: "./chat.component.html",
  styleUrls: ["./chat.component.css"],
})
export class ChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild("messagesContainer") private messagesContainer!: ElementRef;

  messages: Message[] = [];
  userInput: string = "";
  isLoading: boolean = false;
  useStreaming: boolean = true;
  temperature: number = 0.7;
  maxTokens: number = 1000;

  // Estado del backend
  backendStatus: StatusResponse | null = null;
  statusError: string | null = null;

  private streamSubscription?: Subscription;
  private shouldScrollToBottom = false;

  constructor(private openAIService: OpenAIService) {}

  ngOnInit(): void {
    this.checkBackendStatus();
  }

  ngOnDestroy(): void {
    if (this.streamSubscription) {
      this.streamSubscription.unsubscribe();
    }
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
    }
  }

  /**
   * Hace scroll al final del contenedor de mensajes
   */
  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        const element = this.messagesContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    } catch (err) {
      console.error("Error scrolling to bottom:", err);
    }
  }

  /**
   * Verifica el estado del backend
   */
  checkBackendStatus(): void {
    this.openAIService.getStatus().subscribe({
      next: (status) => {
        this.backendStatus = status;
        this.statusError = null;
      },
      error: (error) => {
        this.statusError = "No se puede conectar con el backend";
        console.error("Error checking backend status:", error);
      },
    });
  }

  /**
   * Envía un mensaje al backend
   */
  sendMessage(): void {
    if (!this.userInput.trim() || this.isLoading) {
      return;
    }

    const userMessage: Message = {
      id: this.generateId(),
      role: "user",
      content: this.userInput,
      timestamp: new Date(),
    };

    this.messages.push(userMessage);
    const prompt = this.userInput;
    this.userInput = "";
    this.isLoading = true;
    this.shouldScrollToBottom = true;

    const request: AskRequest = {
      prompt: prompt,
      temperature: this.temperature,
      maxtokens: this.maxTokens,
      stream: this.useStreaming,
    };

    if (this.useStreaming) {
      this.sendWithStreaming(request);
    } else {
      this.sendWithoutStreaming(request);
    }
  }

  /**
   * Envía mensaje con streaming
   */
  private sendWithStreaming(request: AskRequest): void {
    const assistantMessage: Message = {
      id: this.generateId(),
      role: "assistant",
      content: "",
      timestamp: new Date(),
      isStreaming: true,
    };

    this.messages.push(assistantMessage);
    const messageIndex = this.messages.length - 1;

    this.streamSubscription = this.openAIService
      .askWithStreaming(request)
      .subscribe({
        next: (chunk) => {
          if (chunk.content) {
            this.messages[messageIndex].content += chunk.content;
            this.shouldScrollToBottom = true;
          }
          if (chunk.finishReason) {
            this.messages[messageIndex].finishReason = chunk.finishReason;
            this.messages[messageIndex].isTruncated =
              chunk.finishReason !== "stop";
          }
        },
        error: (error) => {
          this.messages[messageIndex].isStreaming = false;
          this.messages[messageIndex].content +=
            "\n\n❌ Error: " + error.message;
          this.isLoading = false;
          console.error("Streaming error:", error);
        },
        complete: () => {
          this.messages[messageIndex].isStreaming = false;
          this.isLoading = false;
          this.shouldScrollToBottom = true;
          setTimeout(() => (this.shouldScrollToBottom = false), 100);
        },
      });
  }

  /**
   * Envía mensaje sin streaming
   */
  private sendWithoutStreaming(request: AskRequest): void {
    this.openAIService.askWithoutStreaming(request).subscribe({
      next: (response) => {
        const assistantMessage: Message = {
          id: this.generateId(),
          role: "assistant",
          content: response.response,
          timestamp: new Date(),
          finishReason: response.finishReason,
          isTruncated: response.finishReason !== "stop",
        };
        this.messages.push(assistantMessage);
        this.isLoading = false;
        this.shouldScrollToBottom = true;
        setTimeout(() => (this.shouldScrollToBottom = false), 100);
      },
      error: (error) => {
        const errorMessage: Message = {
          id: this.generateId(),
          role: "assistant",
          content: "❌ Error: " + (error.error?.detail || error.message),
          timestamp: new Date(),
        };
        this.messages.push(errorMessage);
        this.isLoading = false;
        this.shouldScrollToBottom = true;
        setTimeout(() => (this.shouldScrollToBottom = false), 100);
        console.error("Request error:", error);
      },
    });
  }

  /**
   * Limpia el historial de mensajes
   */
  clearMessages(): void {
    this.messages = [];
  }

  /**
   * Maneja el evento de tecla Enter
   */
  onKeyPress(event: KeyboardEvent): void {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  /**
   * Genera un ID único para los mensajes
   */
  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }

  /**
   * Obtiene el mensaje de advertencia según el motivo de finalización
   */
  getTruncationWarning(finishReason?: string): string {
    if (!finishReason || finishReason === "stop") {
      return "";
    }

    const warnings: { [key: string]: string } = {
      length:
        '⚠️ La respuesta se cortó porque se alcanzó el límite de tokens. Intenta aumentar el "Max Tokens" en la configuración.',
      content_filter:
        "⚠️ La respuesta se cortó por el filtro de contenido de OpenAI.",
      function_call:
        "⚠️ La respuesta se interrumpió para ejecutar una función.",
      tool_calls: "⚠️ La respuesta se interrumpió para ejecutar herramientas.",
    };

    return (
      warnings[finishReason] ||
      `⚠️ La respuesta se cortó. Motivo: ${finishReason}`
    );
  }

  /**
   * Alterna el modo de streaming
   */
  toggleStreaming(): void {
    this.useStreaming = !this.useStreaming;
  }
}
